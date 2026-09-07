using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Dapper implementation of <see cref="IEmployeeRepository"/>.
    /// It holds no SQL of its own - every statement comes from Sql\EmployeeStatements.xml.
    /// </summary>
    public sealed class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnectionFactory _connections;
        private readonly SqlCatalog _sql;

        public EmployeeRepository(IDbConnectionFactory connections, SqlCatalog sql)
        {
            if (connections == null) throw new ArgumentNullException("connections");
            if (sql == null) throw new ArgumentNullException("sql");

            _connections = connections;
            _sql = sql;
        }

        public IReadOnlyList<Employee> GetAll()
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.Query<Employee>(_sql.Get("Employee.SelectAll")).ToList();
            }
        }

        public IReadOnlyList<Employee> GetByStatus(string status)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.Query<Employee>(
                    _sql.Get("Employee.SelectByStatus"),
                    OracleParams.New().Set("status", status)).ToList();
            }
        }

        public int CountAll()
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.ExecuteScalar<int>(_sql.Get("Employee.CountAll"));
            }
        }

        public int CountByStatus(string status)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.ExecuteScalar<int>(
                    _sql.Get("Employee.CountByStatus"),
                    OracleParams.New().Set("status", status));
            }
        }

        public bool ExistsByEmployeeId(string employeeId)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.ExecuteScalar<int>(
                    _sql.Get("Employee.ExistsByEmployeeId"),
                    OracleParams.New().Set("employeeId", employeeId)) > 0;
            }
        }

        public void Add(Employee employee, byte[] photo)
        {
            if (employee == null) throw new ArgumentNullException("employee");

            // The database enforces uniqueness (migration V2), so a duplicate can
            // still arrive here even though the caller checked first - two users
            // adding the same id at the same time both pass that check.
            try
            {
                using (IDbConnection connection = _connections.Create())
                {
                    connection.Execute(
                        _sql.Get("Employee.Insert"),
                        OracleParams.New()
                            .Set("employeeId", employee.EmployeeId)
                            .Set("fullName", employee.FullName)
                            .Set("gender", employee.Gender)
                            .Set("contactNumber", employee.ContactNumber)
                            .Set("position", employee.Position)
                            .SetBlob("photo", photo)
                            .Set("salary", employee.Salary)
                            .Set("insertDate", DateTime.Today)
                            .Set("status", employee.Status));
                }

                Log.Information("Added employee {EmployeeId} ({Status}), photo {Bytes} byte(s).",
                    employee.EmployeeId, employee.Status, photo == null ? 0 : photo.Length);
            }
            catch (OracleException ex)
            {
                throw Translate(ex, "Employee ID '" + employee.EmployeeId + "'");
            }
        }

        public int Update(Employee employee)
        {
            if (employee == null) throw new ArgumentNullException("employee");

            try
            {
                using (IDbConnection connection = _connections.Create())
                {
                    int rows = connection.Execute(
                        _sql.Get("Employee.Update"),
                        OracleParams.New()
                            .Set("fullName", employee.FullName)
                            .Set("gender", employee.Gender)
                            .Set("contactNumber", employee.ContactNumber)
                            .Set("position", employee.Position)
                            .Set("status", employee.Status)
                            .Set("updateDate", DateTime.Today)
                            .Set("employeeId", employee.EmployeeId));

                    Log.Information("Updated employee {EmployeeId}, {Rows} row(s).",
                        employee.EmployeeId, rows);
                    return rows;
                }
            }
            catch (OracleException ex)
            {
                throw Translate(ex, "Employee ID '" + employee.EmployeeId + "'");
            }
        }

        public int UpdateSalary(string employeeId, int salary)
        {
            try
            {
                using (IDbConnection connection = _connections.Create())
                {
                    int rows = connection.Execute(
                        _sql.Get("Employee.UpdateSalary"),
                        OracleParams.New()
                            .Set("salary", salary)
                            .Set("updateDate", DateTime.Today)
                            .Set("employeeId", employeeId));

                    // Salary changes are the ones somebody will ask about later.
                    Log.Information("Set salary of {EmployeeId} to {Salary}, {Rows} row(s).",
                        employeeId, salary, rows);
                    return rows;
                }
            }
            catch (OracleException ex)
            {
                throw Translate(ex, "The salary for employee '" + employeeId + "'");
            }
        }

        public byte[] GetPhoto(string employeeId)
        {
            using (IDbConnection connection = _connections.Create())
            {
                // Fetched only for the employee the user selected, never for a list.
                return connection.ExecuteScalar<byte[]>(
                    _sql.Get("Employee.SelectPhoto"),
                    OracleParams.New().Set("employeeId", employeeId));
            }
        }

        public int SetPhoto(string employeeId, byte[] photo)
        {
            try
            {
                using (IDbConnection connection = _connections.Create())
                {
                    int rows = connection.Execute(
                        _sql.Get("Employee.UpdatePhoto"),
                        OracleParams.New()
                            .SetBlob("photo", photo)
                            .Set("updateDate", DateTime.Today)
                            .Set("employeeId", employeeId));

                    Log.Information("Set photo of {EmployeeId} to {Bytes} byte(s), {Rows} row(s).",
                        employeeId, photo == null ? 0 : photo.Length, rows);
                    return rows;
                }
            }
            catch (OracleException ex)
            {
                throw Translate(ex, "The photo for employee '" + employeeId + "'");
            }
        }

        public int SoftDelete(string employeeId)
        {
            using (IDbConnection connection = _connections.Create())
            {
                int rows = connection.Execute(
                    _sql.Get("Employee.SoftDelete"),
                    OracleParams.New()
                        .Set("deleteDate", DateTime.Today)
                        .Set("employeeId", employeeId));

                Log.Information("Soft-deleted employee {EmployeeId}, {Rows} row(s).", employeeId, rows);
                return rows;
            }
        }

        /// <summary>
        /// Rewrites an Oracle error into something a user can act on, or returns the
        /// original so an unexpected failure keeps its own message.
        /// </summary>
        private static Exception Translate(OracleException ex, string subject)
        {
            return OracleErrors.Translate(ex, subject) ?? (Exception)ex;
        }
    }
}
