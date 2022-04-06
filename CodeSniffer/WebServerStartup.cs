using System;
using System.IO;
using CodeSniffer.Auth;
using JsonWebToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace CodeSniffer
{
    public class WebServerStartup
    {
        private readonly ILogger logger;
        private readonly AppSettings appSettings;


        public WebServerStartup(ILogger logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }


        private const string JwtScheme = "JWT";


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAuthentication(JwtScheme)
                .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>(JwtScheme, null, options =>
                {
                    options.Issuer = appSettings.JWT.Issuer;
                    options.Audience = appSettings.JWT.Audience;
                    options.Key = SymmetricJwk.FromBase64Url(appSettings.JWT.Secret);
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicy(
                    new[] { new JwtAuthorizationRequirement() },
                    new[] { JwtScheme });
                
                PolicyConfiguration.Apply(options);
            });

            services.AddSingleton<IAuthorizationHandler, JwtAuthorizationHandler>();



            services.AddSwaggerGen(options =>
            {
                //options.SwaggerDoc("v2", new OpenApiInfo { Title = "CodeSniffer API", Version = "v2" });
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "CodeSniffer API", Version = "v1" });

                // Use the full name for schema Id's to prevent conflicts when introducing a V2 API
                options.CustomSchemaIds(t => t.FullName);
            });

            services.Configure<HostOptions>(hostOptions =>
            {
                hostOptions.ShutdownTimeout = TimeSpan.FromMinutes(1);
            });
        }


        public void Configure(IApplicationBuilder app)
        {
            if (appSettings.WebAPI.UseDeveloperExceptionPage)
                app.UseDeveloperExceptionPage();

            app.UseSerilogRequestLogging(o =>
            {
                o.Logger = logger;
                o.GetLevel = (context, elapsedMilliseconds, exception) =>
                {
                    if (exception != null)
                        return LogEventLevel.Error;

                    return context.Response.StatusCode switch
                    {
                        >= 500 => LogEventLevel.Error,
                        < 200 or >= 400 => LogEventLevel.Warning,
                        _ => elapsedMilliseconds > 100 ? LogEventLevel.Warning : LogEventLevel.Debug
                    };
                };
            });

            app.UseSwagger();
            app.UseSwaggerUI(swaggerUIOptions =>
            {
                //swaggerUIOptions.SwaggerEndpoint("/v2/swagger.json", "CodeSniffer API v2");
                swaggerUIOptions.SwaggerEndpoint("v1/swagger.json", "CodeSniffer API v1");
            });

            var hasFrontend = UseFrontend(app, "/", "frontend/dist", out var frontendPath);

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                // Vue-Router uses history mode, map all unknown URLs for the frontend back to index.html
                if (hasFrontend)
                    endpoints.MapFallbackToFile("/{**slug:nonfile}", "index.html", new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(frontendPath)
                    });
            });
        }

        private bool UseFrontend(IApplicationBuilder app, string relativeUrl, string relativePath, out string fullPath)
        {
            #if DEBUG
            // While debuggen the working directory is set to the project folder, not bin.
            fullPath = Path.GetFullPath(Path.Combine(@"..", relativePath));
            #else
            fullPath = Path.GetFullPath(Path.Combine(@".", relativePath));
            #endif

            if (!Directory.Exists(fullPath))
            {
                logger.Warning("Frontend path '{path}' not found, static file hosting for this path will be disabled", fullPath);
                return false;
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString(relativeUrl),
                FileProvider = new PhysicalFileProvider(fullPath),
                ServeUnknownFileTypes = true
            });

            return true;
        }
    }
}