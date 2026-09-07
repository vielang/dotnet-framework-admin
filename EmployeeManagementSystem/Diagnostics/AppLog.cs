using System;
using System.IO;
using System.Security.Cryptography;
using Serilog;
using Serilog.Events;

namespace EmployeeManagementSystem.Diagnostics
{
    /// <summary>
    /// Application logging.
    ///
    /// Before this existed the whole application had one Debug.WriteLine, and that
    /// line disappears entirely in a Release build. When a user reported a problem
    /// there was nothing to look at - which is the difference between software you
    /// can run and software you can operate.
    ///
    /// Logs go to a rolling daily file under %LOCALAPPDATA%, not next to the
    /// executable: Program Files is not writable by a normal user, and a log the
    /// application cannot write is worse than no log because the failure is silent.
    /// </summary>
    public static class AppLog
    {
        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        private static bool _initialised;

        /// <summary>Folder holding the log files. Safe to show to a user.</summary>
        public static string LogDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "EmployeeManagementSystem",
                    "logs");
            }
        }

        /// <summary>
        /// Sets up logging. Call once, first thing in Main - before anything that
        /// could fail, so that the failure itself is recorded.
        /// </summary>
        public static void Initialize()
        {
            Initialize(LogDirectory);
        }

        /// <summary>Writes to a specific folder. Tests use this to inspect what was logged.</summary>
        public static void Initialize(string directory)
        {
            if (_initialised)
            {
                return;
            }

            Directory.CreateDirectory(directory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(directory, "ems-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: OutputTemplate,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            _initialised = true;
        }

        /// <summary>Flushes buffered entries. Without this the last writes are lost on exit.</summary>
        public static void Shutdown()
        {
            Log.CloseAndFlush();
            _initialised = false;
        }

        /// <summary>
        /// A short code shown to the user and written to the log with the same value,
        /// so "it broke" becomes "it broke, reference 7K2M9QW4" and the entry can be
        /// found in seconds.
        ///
        /// Random rather than sequential: a counter would leak how much the
        /// application is being used, and would collide across machines.
        /// </summary>
        public static string NewReference()
        {
            // Crockford-style alphabet: no I, L, O or U, so nothing is misread when a
            // user reads the code out over the phone.
            const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

            byte[] bytes = new byte[8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var code = new char[8];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = alphabet[bytes[i] % alphabet.Length];
            }

            return new string(code);
        }
    }
}
