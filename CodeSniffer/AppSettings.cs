using System;
using System.IO;
using System.Runtime.InteropServices;
using Serilog.Events;

namespace CodeSniffer
{
    public class AppSettings
    {
        public string[] PluginPaths { get; set; }

        public AppSettingsWebAPI WebAPI { get; } = new();
        public AppSettingsLog Log { get; } = new();
        public AppSettingsJWT JWT { get; } = new();

        public string DbConnectionString { get; set; }
        public string CheckoutPath { get; set; }
        public int ScanInterval { get; set; } = 5;


        public AppSettings()
        {
            string appDataPath;
            Func<string, string> p;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CodeSniffer");
                p = v => v;
            }
            else
            {
                appDataPath = @"/var/lib/codesniffer";
                p = v => v.ToLower();
            }

            PluginPaths = new[] { Path.Combine(appDataPath, p(@"Plugins")) };
            DbConnectionString = Path.Combine(appDataPath, p(@"CodeSniffer.litedb"));
            CheckoutPath = Path.Combine(Path.GetTempPath(), p(@"CodeSniffer"));
        }


        public static string DefaultFilename()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CodeSniffer", @"appsettings.json")
                : @"/etc/codesniffer/appsettings.json";
        }
    }


    public class AppSettingsWebAPI
    {
        public string Host { get; set; } = "*";
        public int Port { get; set; } = 7042;
        public bool UseDeveloperExceptionPage { get; set; } = true;
    }


    public class AppSettingsLog
    {
        public AppSettingsLog()
        {
            Filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CodeSniffer", @"CodeSniffer.log")
                : @"/var/log/codesniffer/codesniffer.log";
        }


        public string Filename { get; set; }
        public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Debug;
    }


    public class AppSettingsJWT
    {
        public string Issuer { get; set; } = "https://example.com/";
        public string? Audience { get; set; }
        public string Secret { get; set; } = "R9MyWaEoyiMYViVWo8Fk4TUGWiSoaW6U1nOqXri8_XU";
    }

}