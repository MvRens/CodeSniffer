using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Authentication;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.LiteDB;
using CodeSniffer.Repository.LiteDB.Checks;
using CodeSniffer.Repository.LiteDB.Reports;
using CodeSniffer.Repository.LiteDB.Source;
using CodeSniffer.Repository.LiteDB.Users;
using CodeSniffer.Repository.Reports;
using CodeSniffer.Repository.Source;
using CodeSniffer.Repository.Users;
using CodeSniffer.Sniffer;
using JsonWebToken;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

[assembly:ApiController]

namespace CodeSniffer
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            IHost? host = null;
            Container? container = null;
            ILogger? logger = null;
            PluginManager? pluginManager = null;

            var serviceCancellationTokenSource = new CancellationTokenSource();
            try
            {
                var appSettings = LoadAppSettings();
                logger = CreateLogger(appSettings);

                pluginManager = new PluginManager(logger, appSettings.PluginsPath);
                await pluginManager.Initialize();


                /*
                foreach (var sourceCodePlugin in pluginManager.ByType<ICsSourceCodeRepositoryPlugin>())
                {
                    var config = new JsonObject
                    {
                        { "Url", "https://github.com/MvRens/Tapeti.git" }
                    };

                    var sourceRepository = sourceCodePlugin.Create(logger, config);
                    await foreach (var revision in sourceRepository.GetRevisions(CancellationToken.None))
                    {
                        Console.WriteLine(revision.Id);

                        if (revision.Id == "c1063ec33a68ca26f1b7b815c90d5c0362cc5953")
                            await sourceRepository.Checkout(revision, "D:\\Temp\\checkout\\");
                    }
                }

                foreach (var snifferPlugin in pluginManager.ByType<ICsSnifferPlugin>())
                {
                    var config = new JsonObject
                    {
                        { "SolutionsOnly", true }
                    };

                    var sniffer = snifferPlugin.Create(logger, config);
                    sniffer.Execute("D:\\Temp\\checkout\\");
                }

                return 0;
                */


                container = CreateContainer(appSettings);
                container.RegisterInstance(appSettings);
                container.RegisterInstance(logger);
                container.RegisterInstance(typeof(IPluginManager), pluginManager);

                host = CreateHost(args, logger, container, appSettings);

                await host.RunAsync(CancellationToken.None);
                return 0;
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Fatal(e, "Unhandled exception: {message}", e.Message);
                else
                {
                    Console.WriteLine($@"Unhandled exception before logger was initialized: {e.Message}");
                    if (e.StackTrace != null)
                        Console.WriteLine(e.StackTrace);
                }

                return 1;
            }
            finally
            {
                serviceCancellationTokenSource.Cancel();
                host?.Dispose();

                if (container != null)
                    await container.DisposeAsync();

                if (pluginManager != null)
                    await pluginManager.DisposeAsync();

                logger?.Information("Stopping logging...");
                (logger as Logger)?.Dispose();
            }
        }


        private static AppSettings LoadAppSettings()
        {
            var appSettingsFilename = Environment.GetEnvironmentVariable("APPSETTINGS") ?? AppSettings.DefaultFilename();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFilename, true)
                .Build();

            return configuration.Get<AppSettings>() ?? new AppSettings();
        }


        private static Logger CreateLogger(AppSettings appSettings)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            var logger = new LoggerConfiguration()
                // ASP.NET is waaaay too verbose, replace by Serilog.AspNetCore's request logging middleware (see WebServerStartup)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

                .MinimumLevel.Is(appSettings.Log.MinimumLevel)
                .Enrich.FromLogContext()

                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")

                .WriteTo.File(Path.Combine(appSettings.Log.Filename),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day)

                .CreateLogger();


            logger.Information("Started logging");
            return logger;
        }


        private static Container CreateContainer(AppSettings appSettings)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var instancePerDependencyLifestyle = new InstancePerDependencyLifestyle();

            var key = SymmetricJwk.FromBase64Url(appSettings.JWT.Secret);
            var authenticationProvider = new JwtAuthenticationProvider(appSettings.JWT.Issuer, appSettings.JWT.Audience, key, SignatureAlgorithm.HS256, () => container.GetInstance<IUserRepository>());
            container.RegisterInstance<IAuthenticationProvider>(authenticationProvider);


            container.RegisterSingleton<ILiteDbConnectionPool>(() =>
            {
                var pool = new LiteDbConnectionPool(TimeSpan.FromMinutes(1));
                pool.OnInitializeDatabase += new LiteDbRepositoryInitialization(container.GetInstance<ILogger>()).Perform;
                return pool;
            });

            container.Register<ILiteDbConnectionString>(() => new StaticLiteDbConnectionString(appSettings.DbConnectionString), instancePerDependencyLifestyle);
            container.Register<ISourceRepository, LiteDbSourceRepository>(instancePerDependencyLifestyle);
            container.Register<IDefinitionRepository, LiteDbDefinitionRepository>(instancePerDependencyLifestyle);
            container.Register<IReportRepository, LiteDbReportRepository>(instancePerDependencyLifestyle);
            container.Register<ISourceCodeStatusRepository, LiteDbSourceCodeStatusRepository>(instancePerDependencyLifestyle);
            container.Register<IUserRepository, LiteDbUserRepository>(instancePerDependencyLifestyle);

            container.Register<IConfigurationFacade, ConfigurationFacade>();
            container.RegisterSingleton<IRepositoryMonitor, RepositoryMonitor>();
            container.RegisterSingleton<IJobRunner, JobRunner>();
            container.RegisterSingleton<IJobMonitor, JobMonitor>();
            container.RegisterSingleton<IReportFacade, ReportFacade>();

            return container;
        }


        public static IHost CreateHost(string[] args, ILogger logger, Container container, AppSettings appSettings)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog(logger)

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .UseUrls($"http://{appSettings.WebAPI.Host}:{appSettings.WebAPI.Port}")
                        .UseStartup(_ => new WebServerStartup(logger, appSettings));

                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSimpleInjector(container, options =>
                        {
                            // Container zelf pas disposen nadat Tapeti connectie gesloten is
                            options.DisposeContainerWithServiceProvider = false;
                            
                            options.AddHostedService<CodeSnifferService>();
                            options.AddAspNetCore().AddControllerActivation();
                        });
                    });
                })
                .Build();
        }
    }
}