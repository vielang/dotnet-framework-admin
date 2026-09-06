using System;
using System.Globalization;
using System.Windows.Forms;
using EmployeeManagementSystem.Forms;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Views
{
    /// <summary>Sets the salary of active employees.</summary>
    public partial class SalaryView : DataView
    {
        public SalaryView()
        {
            InitializeComponent();
            disableFields();
        }

        protected override void LoadData()
        {
            EmployeeGrid.Bind(dataGridView1,
                Employees.GetByStatus(EmployeeStatus.Active),
                "Id", "Image", "Status");
        }

        public void disableFields()
        {
            salary_employeeID.Enabled = false;
            salary_name.Enabled = false;
            salary_position.Enabled = false;
        }

        private void salary_updateBtn_Click(object sender, EventArgs e)
        {
            string employeeId = salary_employeeID.Text.Trim();

            if (employeeId.Length == 0
                || salary_name.Text.Trim().Length == 0
                || salary_position.Text.Trim().Length == 0
                || salary_salary.Text.Trim().Length == 0)
            {
                UiMessage.Warn("Please fill all blank fields.");
                return;
            }

            int salary;
            if (!int.TryParse(salary_salary.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out salary)
                || salary < 0)
            {
                UiMessage.Warn("Salary must be a whole number of 0 or more.");
                return;
            }

            if (!UiMessage.Confirm("Are you sure you want to UPDATE Employee ID:" + employeeId + "?"))
            {
                UiMessage.Cancelled();
                return;
            }

            try
            {
                if (Employees.UpdateSalary(employeeId, salary) == 0)
                {
                    UiMessage.Warn("No active employee found with ID " + employeeId + ".");
                    return;
                }

                LoadData();
                UiMessage.Info("Update successfully!");
                clearFields();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        public void clearFields()
        {
            salary_employeeID.Text = "";
            salary_name.Text = "";
            salary_position.Text = "";
            salary_salary.Text = "";
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Employee employee = EmployeeGrid.RowAt(dataGridView1, e.RowIndex);
            if (employee == null)
            {
                return;
            }

            salary_employeeID.Text = employee.EmployeeId;
            salary_name.Text = employee.FullName;
            salary_position.Text = employee.Position;
            salary_salary.Text = employee.Salary.ToString(CultureInfo.InvariantCulture);
        }

        private void salary_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }
    }
}
