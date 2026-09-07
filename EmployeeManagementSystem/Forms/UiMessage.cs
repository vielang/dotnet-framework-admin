using System;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Diagnostics;
using Serilog;

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

        /// <summary>
        /// Reports a failure. The user gets one sentence and a reference code; the full
        /// exception goes to the log under that same code, so a support call starts with
        /// "reference 7K2M9QW4" instead of "it broke".
        /// </summary>
        public static void Error(Exception ex)
        {
            // A rule the user broke is not a crash. These carry a sentence written
            // for the person at the keyboard, so show that and nothing else.
            if (ex is DuplicateKeyException || ex is DataRuleViolationException)
            {
                Log.Information("Rejected by a data rule: {Message}", ex.Message);
                Warn(ex.Message);
                return;
            }

            string reference = AppLog.NewReference();
            Log.Error(ex, "Error shown to the user. Reference {Reference}", reference);

            MessageBox.Show(
                ex.Message + Environment.NewLine + Environment.NewLine + "Reference: " + reference,
                "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
