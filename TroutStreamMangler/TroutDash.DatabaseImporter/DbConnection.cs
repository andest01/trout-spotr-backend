namespace TroutDash.DatabaseImporter
{
    public class DbConnection : IDbConnection
    {
        private readonly string _databaseName;
        private readonly string _userName;
        private readonly string _hostName;

        public DbConnection(string databaseName, string userName, string hostName)
        {
            _databaseName = databaseName;
            _userName = userName;
            _hostName = hostName;
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string HostName
        {
            get { return _hostName; }
        }
    }
}