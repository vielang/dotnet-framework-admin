using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Login and registration against the USERS table.
    ///
    /// Passwords are hashed with PBKDF2-HMAC-SHA256 before storage and verified in
    /// memory. Nothing here sends a password to the database.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Returns the matching user, or null when the username is unknown, the
        /// password is wrong, or the account has no usable credential. The returned
        /// object carries no password material.
        /// </summary>
        User FindByCredentials(string username, string password);

        bool UsernameExists(string username);

        /// <summary>Creates an account, hashing the password with a fresh random salt.</summary>
        void Register(string username, string password);
    }
}
