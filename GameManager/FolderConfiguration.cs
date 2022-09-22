using System.Collections.Generic;

namespace GameManager
{
    public class FolderConfiguration : IFolderConfiguration
    {
        public int NumberOfDays { get; set; }

        public string SourceFolder { get; set; }

        public string DestinationFolder { get; set; }

        public IEnumerable<string> FoldersToIgnore { get; set; }
    }
}