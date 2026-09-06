using System;
using Oracle.ManagedDataAccess.Client;

namespace EmployeeManagementSystem.Data
{
    /// <summary>A write collided with a uniqueness rule enforced by the database.</summary>
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>A write broke a value rule: a check constraint, or a column length.</summary>
    public class DataRuleViolationException : Exception
    {
        public DataRuleViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Turns the Oracle error codes the application can actually provoke into
    /// exceptions carrying a sentence a user can act on.
    ///
    /// Without this the UI shows raw text like
    /// "ORA-00001: unique constraint (EMS.UQ_EMPLOYEES_ACTIVE_ID) violated",
    /// which names an index nobody outside the team has heard of.
    /// </summary>
    internal static class OracleErrors
    {
        private const int UniqueConstraintViolated = 1;      // ORA-00001
        private const int CheckConstraintViolated = 2290;    // ORA-02290
        private const int ValueTooLargeForColumn = 12899;    // ORA-12899

        /// <summary>
        /// Returns the exception to throw instead of <paramref name="ex"/>, or null
        /// when the error is not one the user can do anything about - the caller
        /// should then rethrow so the original stack survives.
        /// </summary>
        /// <param name="subject">
        /// What the caller was writing, phrased to read at the start of a sentence,
        /// e.g. "Employee ID 'EMID-01'".
        /// </param>
        public static Exception Translate(OracleException ex, string subject)
        {
            if (ex == null)
            {
                return null;
            }

            switch (ex.Number)
            {
                case UniqueConstraintViolated:
                    return new DuplicateKeyException(subject + " is already in use.", ex);

                case CheckConstraintViolated:
                    return new DataRuleViolationException(
                        subject + " has a value the database does not accept. " +
                        "Status must be Active or Inactive, and salary cannot be negative.", ex);

                case ValueTooLargeForColumn:
                    return new DataRuleViolationException(
                        subject + " is too long for the field it is stored in.", ex);

                default:
                    return null;
            }
        }
    }
}
