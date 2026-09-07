using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Views
{
    /// <summary>
    /// Binds a list of employees to a DataGridView.
    ///
    /// Columns are addressed by name from here on. The old code read row.Cells[4],
    /// which silently pointed at a different field the moment a property moved.
    /// </summary>
    internal static class EmployeeGrid
    {
        private static readonly Dictionary<string, string> Headers =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Id", "#" },
                { "EmployeeId", "Employee ID" },
                { "FullName", "Full Name" },
                { "Gender", "Gender" },
                { "ContactNumber", "Contact Number" },
                { "Position", "Position" },
                { "HasPhoto", "Photo" },
                { "Salary", "Salary" },
                { "Status", "Status" },
            };

        /// <summary>Shows <paramref name="employees"/>, hiding the named columns.</summary>
        public static void Bind(DataGridView grid, IReadOnlyList<Employee> employees, params string[] hiddenColumns)
        {
            grid.AutoGenerateColumns = true;
            grid.DataSource = new List<Employee>(employees);

            foreach (DataGridViewColumn column in grid.Columns)
            {
                string header;
                if (Headers.TryGetValue(column.Name, out header))
                {
                    column.HeaderText = header;
                }

                column.Visible = Array.IndexOf(hiddenColumns, column.Name) < 0;
            }
        }

        /// <summary>Reads one bound row back as an <see cref="Employee"/>, or null for the header row.</summary>
        public static Employee RowAt(DataGridView grid, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
            {
                return null;
            }

            return grid.Rows[rowIndex].DataBoundItem as Employee;
        }
    }
}
