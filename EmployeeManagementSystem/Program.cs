using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Diagnostics;
using EmployeeManagementSystem.Forms;
using Serilog;

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
            // Logging first, before anything that can fail, so that a failure during
            // start-up is itself recorded rather than lost.
            AppLog.Initialize();
            Log.Information("---- Starting {Version} on {Machine} ----",
                typeof(Program).Assembly.GetName().Version, Environment.MachineName);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                InstallCrashHandlers();

                try
                {
                    AppServices.Initialize();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Start-up failed while wiring up services.");

                    MessageBox.Show(
                        "The application could not start:" + Environment.NewLine + Environment.NewLine + ex.Message
                        + Environment.NewLine + Environment.NewLine + "Details were written to:"
                        + Environment.NewLine + AppLog.LogDirectory,
                        "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                RunSessions();
            }
            finally
            {
                Log.Information("---- Stopping ----");
                AppLog.Shutdown();
            }
        }

        /// <summary>
        /// Catches the two kinds of exception that otherwise vanish: one thrown inside
        /// an event handler, and one thrown on a thread nobody is watching. Without
        /// these, a crash shows Windows' own dialog and leaves nothing behind.
        /// </summary>
        private static void InstallCrashHandlers()
        {
            Application.ThreadException += (sender, e) =>
            {
                string reference = AppLog.NewReference();
                Log.Error(e.Exception, "Unhandled exception on the UI thread. Reference {Reference}", reference);
                ShowCrash(e.Exception, reference);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string reference = AppLog.NewReference();
                Log.Fatal(ex, "Unhandled exception, terminating = {Terminating}. Reference {Reference}",
                    e.IsTerminating, reference);
                Log.CloseAndFlush();   // the process may be about to die
            };

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }

        private static void ShowCrash(Exception ex, string reference)
        {
            MessageBox.Show(
                "Something went wrong." + Environment.NewLine + Environment.NewLine
                + ex.Message + Environment.NewLine + Environment.NewLine
                + "Reference: " + reference + Environment.NewLine
                + "Logs: " + AppLog.LogDirectory,
                "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        Log.Information("Login window closed without signing in; exiting.");
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

                    Log.Information("User logged out; returning to the login window.");
                }
            }
        }
    }
}
