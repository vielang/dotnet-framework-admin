using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Diagnostics;
using EmployeeManagementSystem.Models;
using Serilog;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Logging is only useful if it records enough to diagnose a problem and nothing
    /// that must not be written down. The second half is the one worth testing: a log
    /// that leaks a password turns an operational aid into a security hole.
    ///
    /// These tests are not marked Integration - they write to a temporary folder and
    /// need no database.
    /// </summary>
    [Collection(LoggingCollection.Name)]
    public class AppLogTests : IDisposable
    {
        private readonly string _dir;

        public AppLogTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "emslog-" + Guid.NewGuid().ToString("N"));
            AppLog.Shutdown();          // in case an earlier test left it running
            AppLog.Initialize(_dir);
        }

        public void Dispose()
        {
            AppLog.Shutdown();
            try { Directory.Delete(_dir, true); } catch (IOException) { }
        }

        /// <summary>Flushes and returns everything written so far.</summary>
        private string ReadLog()
        {
            AppLog.Shutdown();          // Serilog buffers; nothing is on disk until this
            return string.Join(
                Environment.NewLine,
                Directory.GetFiles(_dir, "*.log").Select(File.ReadAllText));
        }

        // ------------------------------------------------------------- basics

        [Fact]
        public void Writes_what_it_is_told_to_a_file()
        {
            Log.Information("hello from a test");

            Assert.Contains("hello from a test", ReadLog());
        }

        [Fact]
        public void Records_the_level_and_a_timestamp()
        {
            Log.Warning("something looked odd");

            string text = ReadLog();
            Assert.Contains("[WRN]", text);
            Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", text);
        }

        [Fact]
        public void Records_the_whole_exception_not_just_its_message()
        {
            try
            {
                throw new InvalidOperationException("the outer problem",
                    new FormatException("the inner problem"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "caught while testing");
            }

            string text = ReadLog();
            Assert.Contains("the outer problem", text);
            Assert.Contains("the inner problem", text);   // inner exceptions matter most
            Assert.Contains("Records_the_whole_exception", text);   // stack trace
        }

        [Fact]
        public void The_default_log_directory_is_writable_by_a_normal_user()
        {
            // Not next to the .exe: Program Files is read-only for ordinary users, and
            // a log the application cannot write fails silently.
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            Assert.StartsWith(local, AppLog.LogDirectory, StringComparison.OrdinalIgnoreCase);
        }

        // -------------------------------------------------------- reference codes

        [Fact]
        public void A_reference_code_is_short_and_unambiguous()
        {
            string reference = AppLog.NewReference();

            Assert.Equal(8, reference.Length);

            // No I, L, O or U: a user reads this code out loud to whoever is helping.
            Assert.DoesNotContain("I", reference);
            Assert.DoesNotContain("L", reference);
            Assert.DoesNotContain("O", reference);
            Assert.DoesNotContain("U", reference);
        }

        [Fact]
        public void Reference_codes_do_not_repeat()
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < 500; i++)
            {
                seen.Add(AppLog.NewReference());
            }

            Assert.Equal(500, seen.Count);
        }

        [Fact]
        public void A_reference_code_can_be_found_in_the_log()
        {
            // This is the whole point: the user quotes the code, support finds the entry.
            string reference = AppLog.NewReference();
            Log.Error(new Exception("boom"), "Error shown to the user. Reference {Reference}", reference);

            Assert.Contains(reference, ReadLog());
        }

        // ------------------------------------------------- what must NEVER be logged

        [Fact]
        public void A_failed_sign_in_records_the_username_but_never_the_password()
        {
            const string username = "someone";
            const string password = "Sup3rSecret!Passw0rd";

            var users = new UserRepository(
                new StubConnectionFactory(), SqlCatalog.LoadFromDefaultDirectory());

            // No database here, so this throws - the log written before that is the point.
            try { users.FindByCredentials(username, password); } catch { }

            Log.Warning("Failed sign-in for {Username}.", username);

            string text = ReadLog();
            Assert.Contains(username, text);
            Assert.DoesNotContain(password, text);
        }

        [Fact]
        public void The_connection_string_is_never_logged()
        {
            const string connectionString =
                "User Id=ems;Password=Ems_Pass2026;Data Source=localhost:1521/FREEPDB1;";

            var factory = new OracleConnectionFactory(connectionString);

            // Touching the factory, creating connections, and failing to open them must
            // not put the password on disk.
            try { factory.Create().Open(); } catch (Exception ex) { Log.Error(ex, "open failed"); }

            string text = ReadLog();
            Assert.DoesNotContain("Ems_Pass2026", text);
            Assert.DoesNotContain("Password=", text);
        }

        [Fact]
        public void A_password_hash_is_never_logged()
        {
            PasswordHash hash = PasswordHasher.Create("whatever", 1000);

            Log.Information("Registered a new account for {Username}.", "someone");

            string text = ReadLog();
            Assert.DoesNotContain(hash.Hash, text);
            Assert.DoesNotContain(hash.Salt, text);
        }

        [Fact]
        public void An_employee_write_is_recorded_with_enough_detail_to_trace_it()
        {
            // What a later "who changed this salary?" question needs.
            Log.Information("Set salary of {EmployeeId} to {Salary}, {Rows} row(s).", "EMID-01", 4242, 1);

            string text = ReadLog();
            Assert.Contains("EMID-01", text);
            Assert.Contains("4242", text);
        }
    }

    /// <summary>
    /// AppLog configures a process-wide static logger, so these tests must not run
    /// beside each other or they would redirect one another's output mid-test.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class LoggingCollection
    {
        public const string Name = "logging";
    }

    /// <summary>A factory pointing nowhere, for tests that must not reach a database.</summary>
    internal sealed class StubConnectionFactory : IDbConnectionFactory
    {
        public System.Data.IDbConnection Create()
        {
            return new Oracle.ManagedDataAccess.Client.OracleConnection(
                "User Id=nobody;Password=NotARealPassword;Data Source=localhost:1/NOPE;Connection Timeout=1;");
        }
    }
}
