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

            RunSessions();
        }

        /// <summary>
        /// Login, then the main window, then back to login when the user logs out.
        ///
        /// Each window is shown modally and disposed before the next one opens, so
        /// exactly one form is alive at a time. The forms used to navigate by doing
        /// "new OtherForm().Show(); this.Hide();", which never disposed anything -
        /// five trips between login and register left six forms alive - and left
        /// Application.Run watching a hidden form, so closing the visible window
        /// produced a process with no windows that never exited.
        /// </summary>
        private static void RunSessions()
        {
            while (true)
            {
                using (var login = new LoginForm())
                {
                    login.ShowDialog();

                    if (!login.LoginSucceeded)
                    {
                        return;     // the user closed or exited the login window
                    }
                }

                using (var main = new MainForm())
                {
                    main.ShowDialog();

                    if (!main.LogoutRequested)
                    {
                        return;     // anything other than "log out" ends the application
                    }
                }
            }
        }
    }
}
