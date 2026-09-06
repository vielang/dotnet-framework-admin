using System;
using System.Data;
using Dapper;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    [Collection(OracleCollection.Name)]
    [Trait("Category", "Integration")]
    public class UserRepositoryTests : IDisposable
    {
        private readonly OracleFixture _oracle;
        private readonly IUserRepository _users;
        private readonly string _prefix;

        public UserRepositoryTests(OracleFixture oracle)
        {
            _oracle = oracle;
            _users = oracle.CreateUserRepository();
            _prefix = "tu" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        public void Dispose()
        {
            _oracle.DeleteUsers(_prefix);
        }

        /// <summary>Reads the raw stored credential, bypassing the repository.</summary>
        private dynamic ReadStoredRow(string username)
        {
            using (IDbConnection connection = _oracle.Connections.Create())
            {
                return connection.QuerySingleOrDefault(
                    "SELECT password_algorithm, password_hash, password_salt, password_iterations " +
                    "FROM users WHERE username = :username",
                    OracleParams.New().Set("username", username));
            }
        }

        // ------------------------------------------------------- login flow

        [SkippableFact]
        public void Register_then_find_by_credentials()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-a";
            _users.Register(username, "s3cret");

            User found = _users.FindByCredentials(username, "s3cret");

            Assert.NotNull(found);
            Assert.Equal(username, found.Username);
            Assert.True(found.Id > 0);
            Assert.NotNull(found.DateRegister);
        }

        [SkippableFact]
        public void FindByCredentials_rejects_a_wrong_password()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-b";
            _users.Register(username, "s3cret");

            Assert.Null(_users.FindByCredentials(username, "wrong"));
            Assert.Null(_users.FindByCredentials(username, "S3cret"));   // case matters
            Assert.Null(_users.FindByCredentials(username, ""));
        }

        [SkippableFact]
        public void FindByCredentials_rejects_an_unknown_user()
        {
            _oracle.SkipIfUnavailable();

            Assert.Null(_users.FindByCredentials(_prefix + "-nobody", "whatever"));
        }

        [SkippableFact]
        public void UsernameExists_tracks_registration()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-c";

            Assert.False(_users.UsernameExists(username));
            _users.Register(username, "pw");
            Assert.True(_users.UsernameExists(username));
        }

        [SkippableFact]
        public void Duplicate_username_is_rejected_by_the_database()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-d";
            _users.Register(username, "pw");

            var ex = Assert.Throws<DuplicateKeyException>(() => _users.Register(username, "pw2"));

            Assert.Contains(username, ex.Message);
            Assert.DoesNotContain("ORA-", ex.Message);
        }

        [SkippableFact]
        public void Registering_without_a_password_is_refused()
        {
            _oracle.SkipIfUnavailable();

            Assert.Throws<ArgumentException>(() => _users.Register(_prefix + "-e", ""));
            Assert.Throws<ArgumentException>(() => _users.Register(_prefix + "-e", null));
        }

        // ------------------------------------------------- password storage

        /// <summary>
        /// Replaces the old KNOWN_GAP_password_is_stored_in_clear_text test. Finding F1
        /// is closed by migration V4; this is the guard that keeps it closed.
        /// </summary>
        [SkippableFact]
        public void The_password_is_not_stored_anywhere_in_the_row()
        {
            _oracle.SkipIfUnavailable();

            const string password = "PlainTextPassword";
            string username = _prefix + "-f";
            _users.Register(username, password);

            dynamic row = ReadStoredRow(username);

            Assert.NotNull(row);
            Assert.Equal("PBKDF2-SHA256", (string)row.PASSWORD_ALGORITHM);
            Assert.True(Convert.ToInt32(row.PASSWORD_ITERATIONS) >= 100000);

            string hash = (string)row.PASSWORD_HASH;
            string salt = (string)row.PASSWORD_SALT;

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.False(string.IsNullOrWhiteSpace(salt));
            Assert.NotEqual(password, hash);
            Assert.DoesNotContain(password, hash);
            Assert.DoesNotContain(password, salt);
        }

        [SkippableFact]
        public void Two_users_with_the_same_password_get_different_hashes()
        {
            _oracle.SkipIfUnavailable();

            _users.Register(_prefix + "-g1", "identical");
            _users.Register(_prefix + "-g2", "identical");

            dynamic one = ReadStoredRow(_prefix + "-g1");
            dynamic two = ReadStoredRow(_prefix + "-g2");

            // Per-user salt: one cracked hash must not reveal the other account.
            Assert.NotEqual((string)one.PASSWORD_SALT, (string)two.PASSWORD_SALT);
            Assert.NotEqual((string)one.PASSWORD_HASH, (string)two.PASSWORD_HASH);

            // ...and both still log in.
            Assert.NotNull(_users.FindByCredentials(_prefix + "-g1", "identical"));
            Assert.NotNull(_users.FindByCredentials(_prefix + "-g2", "identical"));
        }

        [SkippableFact]
        public void A_successful_login_does_not_hand_the_credential_to_the_caller()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-h";
            _users.Register(username, "s3cret");

            User found = _users.FindByCredentials(username, "s3cret");

            Assert.Null(found.PasswordHash);
            Assert.Null(found.PasswordSalt);
            Assert.Null(found.PasswordAlgorithm);
            Assert.Equal(0, found.PasswordIterations);
        }

        [SkippableFact]
        public void An_account_with_no_usable_credential_cannot_log_in()
        {
            _oracle.SkipIfUnavailable();

            // This is the state migration V4 leaves pre-existing accounts in.
            string username = _prefix + "-i";
            using (IDbConnection connection = _oracle.Connections.Create())
            {
                connection.Execute(
                    "INSERT INTO users (username, date_register) VALUES (:username, SYSDATE)",
                    OracleParams.New().Set("username", username));
            }

            Assert.True(_users.UsernameExists(username));
            Assert.Null(_users.FindByCredentials(username, ""));
            Assert.Null(_users.FindByCredentials(username, "anything"));
        }

        /// <summary>
        /// The seeded admin account must actually work, or the documented
        /// "admin / admin" login is a lie.
        /// </summary>
        [SkippableFact]
        public void The_seeded_admin_account_can_log_in()
        {
            _oracle.SkipIfUnavailable();
            Skip.IfNot(_users.UsernameExists("admin"),
                "No seeded admin account - run db\\setup-db.ps1 -Seed.");

            Assert.NotNull(_users.FindByCredentials("admin", "admin"));
            Assert.Null(_users.FindByCredentials("admin", "not-the-password"));
        }
    }
}
