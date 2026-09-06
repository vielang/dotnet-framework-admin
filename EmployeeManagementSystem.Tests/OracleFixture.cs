using System;
using System.Data;
using Dapper;
using EmployeeManagementSystem.Data;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Shared connection to the local development database.
    ///
    /// Probes once. When Oracle is not reachable the tests skip with a message
    /// saying how to start it, rather than failing with a connection error that
    /// looks like a product bug.
    /// </summary>
    public sealed class OracleFixture
    {
        public const string ConnectionStringVariable = "EMS_TEST_CONNECTION";

        private const string DefaultConnectionString =
            "User Id=ems;Password=Ems_Pass2026;Data Source=localhost:1521/FREEPDB1;Connection Timeout=10;";

        public OracleFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                ConnectionString = DefaultConnectionString;
            }

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            Connections = new OracleConnectionFactory(ConnectionString);
            Sql = SqlCatalog.LoadFromDefaultDirectory();

            try
            {
                using (IDbConnection connection = Connections.Create())
                {
                    connection.Open();
                    connection.ExecuteScalar<int>("SELECT 1 FROM dual");
                }
                IsAvailable = true;
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                UnavailableReason =
                    "Oracle is not reachable. Start it with " +
                    "\"powershell -ExecutionPolicy Bypass -File db\\setup-db.ps1\", " +
                    "or point " + ConnectionStringVariable + " at another instance. (" + ex.Message + ")";
            }
        }

        public string ConnectionString { get; private set; }
        public IDbConnectionFactory Connections { get; private set; }
        public SqlCatalog Sql { get; private set; }
        public bool IsAvailable { get; private set; }
        public string UnavailableReason { get; private set; }

        /// <summary>Skips the calling test when there is no database to talk to.</summary>
        public void SkipIfUnavailable()
        {
            Skip.IfNot(IsAvailable, UnavailableReason);
        }

        public IEmployeeRepository CreateEmployeeRepository()
        {
            return new EmployeeRepository(Connections, Sql);
        }

        public IUserRepository CreateUserRepository()
        {
            return new UserRepository(Connections, Sql);
        }

        /// <summary>Removes rows a test created, by employee_id prefix.</summary>
        public void DeleteEmployees(string employeeIdPrefix)
        {
            if (!IsAvailable) return;

            using (IDbConnection connection = Connections.Create())
            {
                connection.Execute(
                    "DELETE FROM employees WHERE employee_id LIKE :prefix",
                    OracleParams.New().Set("prefix", employeeIdPrefix + "%"));
            }
        }

        /// <summary>Removes users a test created, by username prefix.</summary>
        public void DeleteUsers(string usernamePrefix)
        {
            if (!IsAvailable) return;

            using (IDbConnection connection = Connections.Create())
            {
                connection.Execute(
                    "DELETE FROM users WHERE username LIKE :prefix",
                    OracleParams.New().Set("prefix", usernamePrefix + "%"));
            }
        }
    }

    [CollectionDefinition(Name)]
    public class OracleCollection : ICollectionFixture<OracleFixture>
    {
        public const string Name = "oracle";
    }
}
