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

        public string DbConnectionString { get; }


        public AppSettings()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CodeSniffer");

                PluginPaths = new[] { Path.Combine(appDataPath, @"Plugins") };
                DbConnectionString = Path.Combine(appDataPath, @"CodeSniffer.litedb");
            }
            else
            {
                const string dataPath = @"/var/lib/codesniffer";

                PluginPaths = new[] { Path.Combine(dataPath, @"plugins") };
                DbConnectionString = Path.Combine(dataPath, @"codesniffer.litedb");
            }
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