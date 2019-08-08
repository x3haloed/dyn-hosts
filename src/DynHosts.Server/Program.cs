using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DynHosts.Server
{
    public class Program
    {
        public static bool IsService { get; private set; }
        public static string PathToContentRoot { get; private set; }
        public static string PathToHostsFile { get; private set; } = @"C:\Windows\System32\drivers\etc\hosts";

        public static async Task<int> Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "dhserv",
                Description = "DynHosts Server"
            };

            app.HelpOption("-?|-h|--help");

            var consoleOption = app.Option("-c|--console",
                    "Flag specifying that the application should run within a console window instead of as a service.",
                    CommandOptionType.NoValue);

            var pathOption = app.Option("--path <PATH>",
                    "Path to the hosts file. Useful for testing.",
                    CommandOptionType.SingleValue);

            var urlsOption = app.Option("-u|--url <URLS>",
                    "Semi-colon-delimited list of URL(s) to serve on.",
                    CommandOptionType.SingleValue);

            app.OnExecuteAsync(async (cancellationToken) =>
            {
                if (urlsOption.HasValue())
                {
                    if (pathOption.HasValue())
                    {
                        PathToHostsFile = pathOption.Value();
                    }

                    //not running as a service if the debugger is attached or the --console arg was passed
                    IsService = !(Debugger.IsAttached || consoleOption.HasValue());

                    PathToContentRoot = Directory.GetCurrentDirectory();
                    if (IsService)
                    {
                        string pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                        PathToContentRoot = Path.GetDirectoryName(pathToExe);
                    }

                    string currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

                    //build configuration
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(PathToContentRoot)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{currentEnv}.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();

                    var hostBuilder = Host.CreateDefaultBuilder();

                    if (IsService)
                    {
                        hostBuilder.UseWindowsService();
                    }

                    IHost host = hostBuilder
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseContentRoot(PathToContentRoot);
                        webBuilder.UseUrls(urlsOption.Value());
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseKestrel();
                    })
                    .Build();

                    await host.RunAsync();

                    return 0;
                }
                else
                {
                    app.ShowHint();
                }

                return 0;
            });        

            return await app.ExecuteAsync(args);
        }
    }
}
