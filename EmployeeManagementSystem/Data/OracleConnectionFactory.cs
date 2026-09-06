using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Creates connections to the Oracle database described by the "OracleDb"
    /// connection string in App.config. This is the single place in the application
    /// that knows which database it talks to.
    /// </summary>
    public sealed class OracleConnectionFactory : IDbConnectionFactory
    {
        public const string ConnectionStringName = "OracleDb";

        private readonly string _connectionString;

        public OracleConnectionFactory()
            : this(ReadConnectionStringFromConfig())
        {
        }

        public OracleConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public IDbConnection Create()
        {
            return new OracleConnection(_connectionString);
        }

        private static string ReadConnectionStringFromConfig()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Connection string \"" + ConnectionStringName + "\" is missing from App.config.");
            }

            return settings.ConnectionString;
        }
    }
}
