using System.Collections.Generic;

namespace GameManager
{
    public interface IFolderConfiguration
    {
        string DestinationFolder { get; set; }

        int NumberOfDays { get; set; }

        string SourceFolder { get; set; }

        IEnumerable<string> FoldersToIgnore { get; set; }
    }
}