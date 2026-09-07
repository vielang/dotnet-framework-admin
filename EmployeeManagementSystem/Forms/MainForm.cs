using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Views;

namespace EmployeeManagementSystem.Forms
{
    public partial class MainForm : AppForm
    {
        public MainForm()
        {
            InitializeComponent();

            // panel1 is the purple strip across the top - the window has no title bar,
            // so this is what a user reaches for to move it. The three panels cover the
            // client area but for a one-pixel strip nobody can hit, so without this the
            // window cannot be moved at all.
            MakeDraggable(panel1, exit);
            MakeDraggable(panel2, greet_user);
        }

        /// <summary>True when the user logged out, which sends them back to login.</summary>
        public bool LogoutRequested { get; private set; }

        private void exit_Click(object sender, EventArgs e)
        {
            Close();        // LogoutRequested stays false, so the application ends
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            if (!UiMessage.Confirm("Are you sure you want to logout?"))
            {
                return;
            }

            LogoutRequested = true;
            Close();
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
