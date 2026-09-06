using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Forms
{
    public partial class LoginForm : Form
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
        }

        /// <summary>
        /// Resolved lazily, so opening this form in the Visual Studio designer - where
        /// Program.Main never ran and AppServices is empty - does not throw.
        /// </summary>
        private IUserRepository Users
        {
            get { return _injectedRepository ?? AppServices.Users; }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void login_signupBtn_Click(object sender, EventArgs e)
        {
            new RegisterForm().Show();
            this.Hide();
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

                new MainForm().Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }
    }
}
