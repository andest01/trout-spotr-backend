namespace TroutDash.DatabaseImporter
{
    public interface IDbConnection
    {
        string DatabaseName { get; }
        string UserName { get; }
        string HostName { get; }
    }
}