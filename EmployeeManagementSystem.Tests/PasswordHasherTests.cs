using System;
using EmployeeManagementSystem.Data;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Unit tests for password hashing. No database needed.
    ///
    /// Most tests pass an explicit low iteration count: PBKDF2 is slow on purpose,
    /// and at the production setting each call costs roughly 400 ms. What is being
    /// tested here is the logic, not the cost - one test checks the real default.
    /// </summary>
    public class PasswordHasherTests
    {
        private const int Fast = 1000;   // the minimum the hasher accepts

        private static bool Verify(string password, PasswordHash h)
        {
            return PasswordHasher.Verify(password, h.Algorithm, h.Hash, h.Salt, h.Iterations);
        }

        // -------------------------------------------------------- round trip

        [Fact]
        public void The_right_password_verifies()
        {
            PasswordHash h = PasswordHasher.Create("correct horse battery staple", Fast);

            Assert.True(Verify("correct horse battery staple", h));
        }

        [Fact]
        public void A_wrong_password_does_not_verify()
        {
            PasswordHash h = PasswordHasher.Create("correct horse battery staple", Fast);

            Assert.False(Verify("Correct horse battery staple", h));   // one capital letter
            Assert.False(Verify("wrong", h));
            Assert.False(Verify("", h));
        }

        [Fact]
        public void The_stored_hash_is_not_the_password()
        {
            PasswordHash h = PasswordHasher.Create("hunter2", Fast);

            Assert.DoesNotContain("hunter2", h.Hash);
            Assert.DoesNotContain("hunter2", h.Salt);
        }

        [Fact]
        public void An_empty_password_can_still_be_hashed_and_verified()
        {
            // The repository refuses empty passwords; the hasher itself need not.
            PasswordHash h = PasswordHasher.Create("", Fast);

            Assert.True(Verify("", h));
            Assert.False(Verify("x", h));
        }

        [Fact]
        public void Unicode_passwords_round_trip()
        {
            const string password = "mật khẩu tiếng Việt 🔐";
            PasswordHash h = PasswordHasher.Create(password, Fast);

            Assert.True(Verify(password, h));
        }

        // ------------------------------------------------------------- salt

        [Fact]
        public void The_same_password_hashed_twice_gives_different_results()
        {
            PasswordHash a = PasswordHasher.Create("same password", Fast);
            PasswordHash b = PasswordHasher.Create("same password", Fast);

            Assert.NotEqual(a.Salt, b.Salt);   // fresh random salt each time
            Assert.NotEqual(a.Hash, b.Hash);   // so the hash differs too

            Assert.True(Verify("same password", a));
            Assert.True(Verify("same password", b));
        }

        [Fact]
        public void A_hash_does_not_verify_against_a_different_salt()
        {
            PasswordHash a = PasswordHasher.Create("password", Fast);
            PasswordHash b = PasswordHasher.Create("password", Fast);

            // a's hash with b's salt must not verify
            Assert.False(PasswordHasher.Verify("password", a.Algorithm, a.Hash, b.Salt, a.Iterations));
        }

        // ---------------------------------------------------- broken input

        [Fact]
        public void A_tampered_hash_does_not_verify()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            char[] tampered = h.Hash.ToCharArray();
            tampered[0] = tampered[0] == 'A' ? 'B' : 'A';

            Assert.False(PasswordHasher.Verify(
                "password", h.Algorithm, new string(tampered), h.Salt, h.Iterations));
        }

        [Fact]
        public void A_wrong_iteration_count_does_not_verify()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            Assert.False(PasswordHasher.Verify("password", h.Algorithm, h.Hash, h.Salt, Fast + 1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not base64 at all !!")]
        public void Malformed_stored_values_return_false_rather_than_throwing(string broken)
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            // An account whose stored hash is unusable simply cannot log in.
            Assert.False(PasswordHasher.Verify("password", h.Algorithm, broken, h.Salt, h.Iterations));
            Assert.False(PasswordHasher.Verify("password", h.Algorithm, h.Hash, broken, h.Iterations));
        }

        [Fact]
        public void An_unknown_algorithm_is_refused_rather_than_guessed()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            Assert.False(PasswordHasher.Verify("password", "MD5", h.Hash, h.Salt, h.Iterations));
            Assert.False(PasswordHasher.Verify("password", null, h.Hash, h.Salt, h.Iterations));
        }

        [Fact]
        public void A_null_password_never_verifies()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            Assert.False(Verify(null, h));
        }

        [Fact]
        public void Zero_or_negative_iterations_are_refused()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            Assert.False(PasswordHasher.Verify("password", h.Algorithm, h.Hash, h.Salt, 0));
            Assert.False(PasswordHasher.Verify("password", h.Algorithm, h.Hash, h.Salt, -1));
        }

        [Fact]
        public void Creating_with_a_trivially_low_iteration_count_is_refused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PasswordHasher.Create("password", 1));
        }

        [Fact]
        public void Creating_with_a_null_password_is_refused()
        {
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.Create(null, Fast));
        }

        // ---------------------------------------------------------- defaults

        [Fact]
        public void The_default_cost_is_high_enough_to_be_worth_having()
        {
            Assert.True(PasswordHasher.DefaultIterations >= 100000,
                "Iteration count is what makes guessing expensive; do not lower it.");
        }

        [Fact]
        public void The_algorithm_name_is_recorded_so_it_can_be_changed_later()
        {
            PasswordHash h = PasswordHasher.Create("password", Fast);

            Assert.Equal(PasswordHasher.Pbkdf2Sha256, h.Algorithm);
            Assert.Equal(Fast, h.Iterations);
        }
    }
}
