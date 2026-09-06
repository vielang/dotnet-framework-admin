using System;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagementSystem.Data
{
    /// <summary>The stored form of a password. Nothing here can be turned back into it.</summary>
    public sealed class PasswordHash
    {
        public PasswordHash(string algorithm, string hash, string salt, int iterations)
        {
            Algorithm = algorithm;
            Hash = hash;
            Salt = salt;
            Iterations = iterations;
        }

        public string Algorithm { get; private set; }
        public string Hash { get; private set; }
        public string Salt { get; private set; }
        public int Iterations { get; private set; }
    }

    /// <summary>
    /// Hashes passwords with PBKDF2-HMAC-SHA256.
    ///
    /// Three properties make this safe, and all three matter:
    ///   - one-way: the stored value cannot be turned back into the password
    ///   - salted: every user gets fresh random bytes, so identical passwords
    ///     produce different hashes and one precomputed table cannot crack many
    ///     accounts at once
    ///   - slow on purpose: the iteration count makes guessing expensive. It is a
    ///     cost you pay once per login and an attacker pays per guess.
    ///
    /// The algorithm name is stored alongside each hash so it can be changed later
    /// without invalidating existing accounts.
    /// </summary>
    public static class PasswordHasher
    {
        public const string Pbkdf2Sha256 = "PBKDF2-SHA256";

        /// <summary>
        /// Deliberately expensive: this is the cost a legitimate user pays once per
        /// login and an attacker pays per guess.
        ///
        /// 120 000 measures around 400 ms on the machine this was developed on.
        /// Current OWASP guidance for PBKDF2-HMAC-SHA256 is higher still, but login
        /// currently runs synchronously on the UI thread, so raising this freezes the
        /// window for that long. Raise it together with making login async (F11) - not
        /// before.
        ///
        /// Existing accounts keep working when this changes, because every row stores
        /// the iteration count it was created with.
        /// </summary>
        public const int DefaultIterations = 120000;

        private const int SaltBytes = 16;   // 128 bits
        private const int HashBytes = 32;   // 256 bits, matching SHA-256

        /// <summary>Hashes a password with a fresh random salt.</summary>
        public static PasswordHash Create(string password)
        {
            return Create(password, DefaultIterations);
        }

        public static PasswordHash Create(string password, int iterations)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (iterations < 1000) throw new ArgumentOutOfRangeException("iterations", "Too low to be useful.");

            byte[] salt = new byte[SaltBytes];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, iterations);

            return new PasswordHash(
                Pbkdf2Sha256,
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt),
                iterations);
        }

        /// <summary>
        /// Checks a password against a stored hash. Returns false rather than throwing
        /// when the stored values are missing or malformed - an account that cannot be
        /// verified simply cannot log in.
        /// </summary>
        public static bool Verify(string password, string algorithm, string hash, string salt, int iterations)
        {
            if (password == null
                || string.IsNullOrEmpty(hash)
                || string.IsNullOrEmpty(salt)
                || iterations < 1)
            {
                return false;
            }

            if (!string.Equals(algorithm, Pbkdf2Sha256, StringComparison.OrdinalIgnoreCase))
            {
                return false;   // unknown algorithm: refuse rather than guess
            }

            byte[] expected;
            byte[] saltBytes;
            try
            {
                expected = Convert.FromBase64String(hash);
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actual = Derive(password, saltBytes, iterations, expected.Length);

            return FixedTimeEquals(expected, actual);
        }

        /// <summary>
        /// Burns roughly the same time as a real verification, for use when the
        /// username does not exist. Without it, a failed login returns noticeably
        /// faster for unknown users than for known ones, which lets an attacker
        /// discover which usernames are registered.
        /// </summary>
        public static void BurnTime()
        {
            byte[] salt = new byte[SaltBytes];
            Derive("not-a-real-password", salt, DefaultIterations);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            return Derive(password, salt, iterations, HashBytes);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        {
            // .NET Framework's Rfc2898DeriveBytes defaults to SHA-1. The overload
            // taking a HashAlgorithmName (available from 4.7.2) is what selects
            // SHA-256 - without it this would silently be a weaker hash.
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(length);
            }
        }

        /// <summary>
        /// Compares two byte arrays in time that does not depend on where they first
        /// differ. A normal comparison returns as soon as it finds a mismatch, and the
        /// timing of that leaks how much of the hash an attacker has guessed right.
        /// (.NET Framework has no CryptographicOperations.FixedTimeEquals.)
        /// </summary>
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < a.Length; i++)
            {
                difference |= a[i] ^ b[i];
            }

            return difference == 0;
        }
    }
}
