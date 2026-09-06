using System;

namespace EmployeeManagementSystem.Models
{
    /// <summary>
    /// A row of the USERS table.
    ///
    /// There is no Password property: the table stores only a one-way derivation.
    /// The four password_* fields are what the repository needs to verify a login,
    /// and it clears them before handing the object to the UI.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime? DateRegister { get; set; }

        public string PasswordAlgorithm { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int PasswordIterations { get; set; }

        /// <summary>Wipes the credential fields once they are no longer needed.</summary>
        public void ForgetCredentials()
        {
            PasswordAlgorithm = null;
            PasswordHash = null;
            PasswordSalt = null;
            PasswordIterations = 0;
        }
    }
}
