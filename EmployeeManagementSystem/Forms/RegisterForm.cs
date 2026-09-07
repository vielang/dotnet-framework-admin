using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Forms
{
    public partial class RegisterForm : AppForm
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

            // label5 reads "Login" and is wired to the login button, so it keeps its
            // click; exit closes the window.
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

        /// <summary>True when the user asked to quit rather than go back to login.</summary>
        public bool ExitRequested { get; private set; }

        /// <summary>Set after a successful registration, so login can prefill the name.</summary>
        public string RegisteredUsername { get; private set; }

        private void exit_Click(object sender, EventArgs e)
        {
            ExitRequested = true;
            Close();
        }

        private void signup_loginBtn_Click(object sender, EventArgs e)
        {
            Close();        // back to the login window that opened this one
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

                RegisteredUsername = username;
                Close();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }
    }
}
