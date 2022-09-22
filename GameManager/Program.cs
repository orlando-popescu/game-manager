using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GameManager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Application started.");

            var folderConfiguration = new FolderConfiguration();

            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            configuration.Bind(folderConfiguration);

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            IGameManagerService gameService = new GameManagerService(folderConfiguration, logger);

            if (args.Length == 1)
            {
                gameService.RestoreFolder(args[0]);
            }
            else
            {
                gameService.ArchiveFolders();
            }
        }
    }
}