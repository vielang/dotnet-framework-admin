using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Views
{
    /// <summary>Headline counters: total, active and inactive employees.</summary>
    public partial class DashboardView : DataView
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        protected override void LoadData()
        {
            dashboard_TE.Text = Employees.CountAll().ToString();
            dashboard_AE.Text = Employees.CountByStatus(EmployeeStatus.Active).ToString();
            dashboard_IE.Text = Employees.CountByStatus(EmployeeStatus.Inactive).ToString();
        }
    }
}
