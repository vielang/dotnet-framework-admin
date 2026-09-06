using System;

namespace EmployeeManagementSystem.Models
{
    /// <summary>A row of the USERS table.</summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? DateRegister { get; set; }
    }
}
