using System;
using System.Data;
using System.Linq;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Dapper implementation of <see cref="IUserRepository"/>.
    /// Every statement comes from Sql\UserStatements.xml.
    ///
    /// Passwords never reach the database. The row is fetched by username and the
    /// hash is verified here, in <see cref="PasswordHasher"/>.
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
            User user = FindByUsername(username);

            if (user == null)
            {
                // Spend the same time as a real verification would, so that an
                // unknown username cannot be told apart from a wrong password by
                // how quickly the answer comes back.
                PasswordHasher.BurnTime();

                // Username only. A password must never reach the log - and neither
                // must any hint of which usernames exist, so both failure paths log
                // exactly the same sentence.
                Log.Warning("Failed sign-in for {Username}.", username);
                return null;
            }

            bool ok = PasswordHasher.Verify(
                password,
                user.PasswordAlgorithm,
                user.PasswordHash,
                user.PasswordSalt,
                user.PasswordIterations);

            if (!ok)
            {
                Log.Warning("Failed sign-in for {Username}.", username);
                return null;
            }

            Log.Information("Signed in as {Username}.", username);

            // The caller is the UI. It has no business holding the hash.
            user.ForgetCredentials();
            return user;
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
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("A password is required.", "password");
            }

            PasswordHash hash = PasswordHasher.Create(password);

            // users.username has had a unique constraint since V1, so a duplicate
            // can arrive here despite the caller checking first.
            try
            {
                using (IDbConnection connection = _connections.Create())
                {
                    connection.Execute(
                        _sql.Get("User.Insert"),
                        OracleParams.New()
                            .Set("username", username)
                            .Set("dateRegister", DateTime.Today)
                            .Set("passwordAlgorithm", hash.Algorithm)
                            .Set("passwordHash", hash.Hash)
                            .Set("passwordSalt", hash.Salt)
                            .Set("passwordIterations", hash.Iterations));
                }

                Log.Information("Registered a new account for {Username}.", username);
            }
            catch (OracleException ex)
            {
                throw OracleErrors.Translate(ex, "The username '" + username + "'") ?? (Exception)ex;
            }
        }

        private User FindByUsername(string username)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.Query<User>(
                    _sql.Get("User.FindByUsername"),
                    OracleParams.New().Set("username", username)).FirstOrDefault();
            }
        }
    }
}
