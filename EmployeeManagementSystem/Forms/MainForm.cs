using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Views;

namespace EmployeeManagementSystem.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            if (!UiMessage.Confirm("Are you sure you want to logout?"))
            {
                return;
            }

            new LoginForm().Show();
            this.Hide();
        }

        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            ShowView(dashboardView);
        }

        private void addEmloyee_btn_Click(object sender, EventArgs e)
        {
            ShowView(employeeView);
        }

        private void salary_btn_Click(object sender, EventArgs e)
        {
            ShowView(salaryView);
        }

        /// <summary>Brings one view to the front and reloads it from the database.</summary>
        private void ShowView(DataView view)
        {
            dashboardView.Visible = ReferenceEquals(view, dashboardView);
            employeeView.Visible = ReferenceEquals(view, employeeView);
            salaryView.Visible = ReferenceEquals(view, salaryView);

            view.RefreshData();
        }
    }
}
