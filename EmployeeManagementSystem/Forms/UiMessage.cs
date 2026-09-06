using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Forms
{
    /// <summary>
    /// The application's message boxes in one place.
    ///
    /// Previously every data-access method popped its own MessageBox, which tied the
    /// database layer to WinForms. Now the repositories throw and the UI reports.
    /// </summary>
    public static class UiMessage
    {
        public static void Info(string text)
        {
            MessageBox.Show(text, "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warn(string text)
        {
            MessageBox.Show(text, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Cancelled()
        {
            MessageBox.Show("Cancelled.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, "Confirmation Message",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Reports a failure. The full exception goes to the debug trace, not to the user.</summary>
        public static void Error(Exception ex)
        {
            // A rule the user broke is not a crash. These carry a sentence written
            // for the person at the keyboard, so show that and nothing else.
            if (ex is DuplicateKeyException || ex is DataRuleViolationException)
            {
                Warn(ex.Message);
                return;
            }

            System.Diagnostics.Debug.WriteLine(ex);
            MessageBox.Show(ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
