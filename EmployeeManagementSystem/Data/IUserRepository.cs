using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data
{
    /// <summary>Login and registration against the USERS table.</summary>
    public interface IUserRepository
    {
        /// <summary>Returns the matching user, or null when the credentials are wrong.</summary>
        User FindByCredentials(string username, string password);

        bool UsernameExists(string username);

        void Register(string username, string password);
    }
}
