using System;
using System.Data;
using System.Linq;
using Dapper;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Dapper implementation of <see cref="IUserRepository"/>.
    /// Every statement comes from Sql\UserStatements.xml.
    /// </summary>
    public sealed class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connections;
        private readonly SqlCatalog _sql;

        public UserRepository(IDbConnectionFactory connections, SqlCatalog sql)
        {
            if (connections == null) throw new ArgumentNullException("connections");
            if (sql == null) throw new ArgumentNullException("sql");

            _connections = connections;
            _sql = sql;
        }

        public User FindByCredentials(string username, string password)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.Query<User>(
                    _sql.Get("User.FindByCredentials"),
                    OracleParams.New()
                        .Set("username", username)
                        .Set("password", password)).FirstOrDefault();
            }
        }

        public bool UsernameExists(string username)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.ExecuteScalar<int>(
                    _sql.Get("User.CountByUsername"),
                    OracleParams.New().Set("username", username)) > 0;
            }
        }

        public void Register(string username, string password)
        {
            using (IDbConnection connection = _connections.Create())
            {
                connection.Execute(
                    _sql.Get("User.Insert"),
                    OracleParams.New()
                        .Set("username", username)
                        .Set("password", password)
                        .Set("dateRegister", DateTime.Today));
            }
        }
    }
}
