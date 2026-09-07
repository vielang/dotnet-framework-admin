using System;
using Dapper;
using Serilog;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem
{
    /// <summary>
    /// Composition root. Program.Main calls <see cref="Initialize"/> once, and the
    /// forms read their repositories from here.
    ///
    /// A service locator rather than constructor injection, because the WinForms
    /// designer needs every Form and UserControl to keep a parameterless constructor.
    /// The repositories themselves sit behind interfaces, so they can still be
    /// replaced with fakes in a test.
    /// </summary>
    public static class AppServices
    {
        private static IEmployeeRepository _employees;
        private static IUserRepository _users;

        public static IEmployeeRepository Employees
        {
            get { return Require(_employees); }
        }

        public static IUserRepository Users
        {
            get { return Require(_users); }
        }

        public static bool IsInitialized
        {
            get { return _employees != null && _users != null; }
        }

        /// <summary>Wires the real Oracle-backed repositories. Call once at startup.</summary>
        public static void Initialize()
        {
            // Oracle columns are snake_case (employee_id), the model is PascalCase
            // (EmployeeId); this is what lets Dapper line the two up.
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var connections = new OracleConnectionFactory();
            SqlCatalog sql = SqlCatalog.LoadFromDefaultDirectory();

            // The connection string is deliberately absent: it carries a password.
            Log.Information("Loaded {Count} SQL statements from {Directory}.",
                System.Linq.Enumerable.Count(sql.Keys), SqlCatalog.DefaultDirectory);

            Initialize(new EmployeeRepository(connections, sql), new UserRepository(connections, sql));
        }

        /// <summary>Wires explicit implementations - used by tests.</summary>
        public static void Initialize(IEmployeeRepository employees, IUserRepository users)
        {
            if (employees == null) throw new ArgumentNullException("employees");
            if (users == null) throw new ArgumentNullException("users");

            _employees = employees;
            _users = users;
        }

        private static T Require<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new InvalidOperationException(
                    "AppServices.Initialize() has not been called yet.");
            }
            return service;
        }
    }
}
