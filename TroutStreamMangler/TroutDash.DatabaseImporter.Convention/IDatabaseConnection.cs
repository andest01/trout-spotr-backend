namespace TroutDash.DatabaseImporter.Convention
{
    public interface IDatabaseConnection
    {
        string DatabaseName { get; }
        string UserName { get; }
        string HostName { get; }
    }
}   