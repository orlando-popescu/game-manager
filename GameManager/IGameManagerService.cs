namespace GameManager
{
    public interface IGameManagerService
    {
        void ArchiveFolders();

        void RestoreFolder(string directoryPath);
    }
}