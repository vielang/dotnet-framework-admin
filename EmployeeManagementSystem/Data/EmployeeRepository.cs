using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Oracle.ManagedDataAccess.Client;
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

        public void Add(Employee employee)
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
                            .Set("image", employee.Image)
                            .Set("salary", employee.Salary)
                            .Set("insertDate", DateTime.Today)
                            .Set("status", employee.Status));
                }
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
                    return connection.Execute(
                        _sql.Get("Employee.Update"),
                        OracleParams.New()
                            .Set("fullName", employee.FullName)
                            .Set("gender", employee.Gender)
                            .Set("contactNumber", employee.ContactNumber)
                            .Set("position", employee.Position)
                            .Set("status", employee.Status)
                            .Set("updateDate", DateTime.Today)
                            .Set("employeeId", employee.EmployeeId));
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
                    return connection.Execute(
                        _sql.Get("Employee.UpdateSalary"),
                        OracleParams.New()
                            .Set("salary", salary)
                            .Set("updateDate", DateTime.Today)
                            .Set("employeeId", employeeId));
                }
            }
            catch (OracleException ex)
            {
                throw Translate(ex, "The salary for employee '" + employeeId + "'");
            }
        }

        public int SoftDelete(string employeeId)
        {
            using (IDbConnection connection = _connections.Create())
            {
                return connection.Execute(
                    _sql.Get("Employee.SoftDelete"),
                    OracleParams.New()
                        .Set("deleteDate", DateTime.Today)
                        .Set("employeeId", employeeId));
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
