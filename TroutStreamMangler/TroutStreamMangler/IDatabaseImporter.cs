using System.IO;

namespace TroutStreamMangler
{
    public interface IDatabaseImporter
    {
        string DatabaseName { get; set; }
        string UserName { get; set; }
        string HostName { get; set; }
        DirectoryInfo RootDirectory { get; set; }
    }
}