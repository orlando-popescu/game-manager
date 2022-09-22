using System;
using System.IO;
using System.Linq;
using Serilog;

namespace GameManager
{
    public class GameManagerService : IGameManagerService
    {
        private const string SteamAppsFolder = "steamapps";

        private readonly IFolderConfiguration _configuration;

        private readonly ILogger _logger;

        public GameManagerService(IFolderConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void ArchiveFolders()
        {
            try
            {
                var baseDirectory = _configuration.SourceFolder;

                _logger.Information($"Getting directory list for: {baseDirectory}...");

                var directories = Directory.GetDirectories(baseDirectory);

                _logger.Information($"Found {directories.Length} directories.");

                foreach (var directory in directories)
                {
                    var directoryInfo = new DirectoryInfo(directory);

                    HandleSteamApps(directoryInfo);

                    if (ShouldMove(directoryInfo))
                    {
                        CopyAndDeleteDirectories(new DirectoryInfo(_configuration.DestinationFolder), _configuration.DestinationFolder);
                    }
                }
                _logger.Information("Finished moving game files.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while moving game files.");
            }
        }

        public void RestoreFolder(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or whitespace", nameof(directoryPath));
            }

            if (Directory.Exists(directoryPath))
            {
                try
                {
                    _logger.Information($"Moving directory: {directoryPath} to {_configuration.SourceFolder}");
                    var direrctoryInfo = new DirectoryInfo(directoryPath);
                    CopyAndDeleteDirectories(direrctoryInfo, _configuration.SourceFolder);
                    _logger.Information("Finished moving directories.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error while moving game files.");
                }
            }
            else
            {
                _logger.Warning($"Directory: {directoryPath} does not exist and will not continue.");
            }
        }

        private bool ShouldMove(DirectoryInfo directoryInfo)
        {
            var timeToCheck = DateTime.UtcNow.AddDays(_configuration.NumberOfDays * -1);

            return directoryInfo.LastWriteTimeUtc < timeToCheck && Directory.Exists(_configuration.SourceFolder) && !_configuration.FoldersToIgnore.Contains(directoryInfo.Name);
        }

        private void HandleSteamApps(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase))
            {
                var steamappsDirectory = Path.Join(directoryInfo.FullName, SteamAppsFolder, "common");
                directoryInfo = new DirectoryInfo(steamappsDirectory);

                foreach (var steamApp in directoryInfo.GetDirectories())
                {
                    if (ShouldMove(steamApp))
                    {
                        var destinationDirectory = Path.Combine(_configuration.DestinationFolder, SteamAppsFolder);
                        CopyAndDeleteDirectories(steamApp, destinationDirectory);
                    }
                }
            }
        }

        private void CopyAndDeleteDirectories(DirectoryInfo directoryInfo, string destinationDirectory)
        {
            _logger.Information($"Copying directory: {directoryInfo.Name} to {destinationDirectory} ...");
            DirectoryCopy(directoryInfo.FullName, Path.Combine(destinationDirectory, directoryInfo.Name), true);

            _logger.Information($"Finished copying files. Deleting directory with path: {directoryInfo.FullName}.");

            Directory.Delete(directoryInfo.FullName, true);

            _logger.Information($"Directory deleted.");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}