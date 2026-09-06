using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly IUserRepository _injectedRepository;

        public RegisterForm()
            : this(null)
        {
        }

        /// <summary>Injects a repository. Passing null falls back to <see cref="AppServices"/>.</summary>
        public RegisterForm(IUserRepository users)
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

        private void signup_loginBtn_Click(object sender, EventArgs e)
        {
            new LoginForm().Show();
            this.Hide();
        }

        private void signup_showPass_CheckedChanged(object sender, EventArgs e)
        {
            signup_password.PasswordChar = signup_showPass.Checked ? '\0' : '*';
        }

        private void sigunp_btn_Click(object sender, EventArgs e)
        {
            string username = signup_username.Text.Trim();
            string password = signup_password.Text.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                UiMessage.Warn("Please fill all blank fields");
                return;
            }

            try
            {
                if (Users.UsernameExists(username))
                {
                    UiMessage.Warn(username + " is already taken");
                    return;
                }

                Users.Register(username, password);

                UiMessage.Info("Registered successfully!");

                new LoginForm().Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }
    }
}
