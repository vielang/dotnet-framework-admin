using System.Data;

namespace EmployeeManagementSystem.Data
{
    /// <summary>Hands out database connections; lets tests swap the database out.</summary>
    public interface IDbConnectionFactory
    {
        /// <summary>Creates a new, unopened connection. The caller owns and disposes it.</summary>
        IDbConnection Create();
    }
}
