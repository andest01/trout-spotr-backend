namespace TroutDash.DatabaseImporter.Convention
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string _databaseName;
        private readonly string _hostName;
        private readonly string _userName;

        public DatabaseConnection(string databaseName, string hostName, string userName)
        {
            _databaseName = databaseName;
            _hostName = hostName;
            _userName = userName;
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string HostName
        {
            get { return _hostName; }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }
    }
}