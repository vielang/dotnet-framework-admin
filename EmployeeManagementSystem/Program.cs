using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Forms;

namespace EmployeeManagementSystem
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AppServices.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The application could not start:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
