using System.Collections.Generic;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>Everything the UI is allowed to do with the EMPLOYEES table.</summary>
    public interface IEmployeeRepository
    {
        /// <summary>All employees that have not been soft-deleted.</summary>
        IReadOnlyList<Employee> GetAll();

        /// <summary>Non-deleted employees with the given status.</summary>
        IReadOnlyList<Employee> GetByStatus(string status);

        int CountAll();

        int CountByStatus(string status);

        /// <summary>True when a non-deleted employee already uses this business id.</summary>
        bool ExistsByEmployeeId(string employeeId);

        void Add(Employee employee);

        /// <summary>Updates the editable profile fields, matched on <see cref="Employee.EmployeeId"/>.</summary>
        /// <returns>Number of rows changed.</returns>
        int Update(Employee employee);

        /// <returns>Number of rows changed.</returns>
        int UpdateSalary(string employeeId, int salary);

        /// <summary>Stamps DELETE_DATE so the row disappears from every query.</summary>
        /// <returns>Number of rows changed.</returns>
        int SoftDelete(string employeeId);
    }
}
