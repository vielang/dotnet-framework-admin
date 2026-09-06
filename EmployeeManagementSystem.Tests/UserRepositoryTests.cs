using System;
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
            Assert.NotNull(found.DateRegister);   // date_register -> DateRegister
        }

        [SkippableFact]
        public void FindByCredentials_rejects_a_wrong_password()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-b";
            _users.Register(username, "s3cret");

            Assert.Null(_users.FindByCredentials(username, "wrong"));
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

            // The user sees the username, not the name of an index.
            Assert.Contains(username, ex.Message);
            Assert.DoesNotContain("ORA-", ex.Message);
        }

        /// <summary>
        /// Documents finding F1: passwords are stored and compared in clear text.
        /// When hashing lands this test must be replaced with one asserting the
        /// stored value is NOT the password.
        /// </summary>
        [SkippableFact]
        public void KNOWN_GAP_password_is_stored_in_clear_text()
        {
            _oracle.SkipIfUnavailable();

            string username = _prefix + "-e";
            _users.Register(username, "PlainTextPassword");

            User found = _users.FindByCredentials(username, "PlainTextPassword");

            Assert.Equal("PlainTextPassword", found.Password);
        }
    }
}
