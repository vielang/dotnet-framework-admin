using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Forms
{
    public partial class LoginForm : AppForm
    {
        private readonly IUserRepository _injectedRepository;

        public LoginForm()
            : this(null)
        {
        }

        /// <summary>Injects a repository. Passing null falls back to <see cref="AppServices"/>.</summary>
        public LoginForm(IUserRepository users)
        {
            _injectedRepository = users;
            InitializeComponent();

            // Enter submits (AcceptButton, set in the designer); Escape gives up and
            // ends the application, which is what closing a login window means here.
            EscapeClosesForm = true;

            // label5 reads "Sign up" and is wired to the sign-up button, so it must keep
            // its click; exit closes the window. Everything else here is decoration.
            MakeDraggable(panel2, label5);
            MakeDraggable(label2);
            MakeDraggable(label3);
        }

        /// <summary>
        /// Resolved lazily, so opening this form in the Visual Studio designer - where
        /// Program.Main never ran and AppServices is empty - does not throw.
        /// </summary>
        private IUserRepository Users
        {
            get { return _injectedRepository ?? AppServices.Users; }
        }

        /// <summary>True when the user signed in. Program.RunSessions reads this.</summary>
        public bool LoginSucceeded { get; private set; }

        private void exit_Click(object sender, EventArgs e)
        {
            Close();        // LoginSucceeded stays false, so the application ends
        }

        private void login_signupBtn_Click(object sender, EventArgs e)
        {
            using (var register = new RegisterForm())
            {
                register.ShowDialog(this);

                if (register.ExitRequested)
                {
                    Close();
                    return;
                }

                if (!string.IsNullOrEmpty(register.RegisteredUsername))
                {
                    login_username.Text = register.RegisteredUsername;
                    login_password.Focus();
                }
            }
        }

        private void login_showPass_CheckedChanged(object sender, EventArgs e)
        {
            login_password.PasswordChar = login_showPass.Checked ? '\0' : '*';
        }

        private void login_btn_Click(object sender, EventArgs e)
        {
            string username = login_username.Text.Trim();
            string password = login_password.Text.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                UiMessage.Warn("Please fill all blank fields");
                return;
            }

            try
            {
                User user = Users.FindByCredentials(username, password);

                if (user == null)
                {
                    UiMessage.Warn("Incorrect Username or Password!");
                    return;
                }

                UiMessage.Info("Login successfully!");

                LoginSucceeded = true;
                Close();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }
    }
}
