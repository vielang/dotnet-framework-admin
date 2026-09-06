using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Loads every SQL statement from the XML files in the "Sql" folder and exposes
    /// them by "Namespace.Id" key - the same idea as an iBATIS/MyBatis SqlMap, but
    /// without the dependency.
    ///
    /// File format:
    ///
    ///   &lt;statements namespace="Employee"&gt;
    ///     &lt;statement id="SelectAll"&gt;SELECT ... &lt;/statement&gt;
    ///   &lt;/statements&gt;
    ///
    /// The files are copied next to the executable, so a statement can be corrected
    /// without rebuilding the application.
    /// </summary>
    public sealed class SqlCatalog
    {
        private readonly Dictionary<string, string> _statements;

        private SqlCatalog(Dictionary<string, string> statements)
        {
            _statements = statements;
        }

        /// <summary>Statement keys that were loaded, sorted - handy in error messages and tests.</summary>
        public IEnumerable<string> Keys
        {
            get { return _statements.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase); }
        }

        /// <summary>The "Sql" folder that ships next to the executable.</summary>
        public static string DefaultDirectory
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql"); }
        }

        public static SqlCatalog LoadFromDefaultDirectory()
        {
            return LoadFrom(DefaultDirectory);
        }

        /// <summary>Reads every *.xml in <paramref name="directory"/> into one catalog.</summary>
        public static SqlCatalog LoadFrom(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException(
                    "SQL statement folder not found: " + directory +
                    ". The Sql\\*.xml files must be copied next to the executable.");
            }

            var statements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] files = Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                throw new FileNotFoundException("No SQL statement files found in " + directory + ".");
            }

            foreach (string file in files)
            {
                LoadFile(file, statements);
            }

            return new SqlCatalog(statements);
        }

        private static void LoadFile(string file, IDictionary<string, string> statements)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(file);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot parse SQL file " + file + ": " + ex.Message, ex);
            }

            XElement root = doc.Root;
            if (root == null || root.Name.LocalName != "statements")
            {
                throw new InvalidOperationException(
                    "SQL file " + file + " must have a <statements> root element.");
            }

            string ns = (string)root.Attribute("namespace");
            if (string.IsNullOrWhiteSpace(ns))
            {
                throw new InvalidOperationException(
                    "SQL file " + file + " is missing the namespace attribute on <statements>.");
            }

            foreach (XElement element in root.Elements("statement"))
            {
                string id = (string)element.Attribute("id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidOperationException(
                        "A <statement> in " + file + " is missing its id attribute.");
                }

                string key = ns.Trim() + "." + id.Trim();
                string sql = Normalize(element.Value);

                if (sql.Length == 0)
                {
                    throw new InvalidOperationException("Statement " + key + " in " + file + " is empty.");
                }

                if (statements.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        "Duplicate SQL statement key " + key + " (second definition in " + file + ").");
                }

                statements.Add(key, sql);
            }
        }

        /// <summary>Trims the XML indentation off the statement so the SQL sent to Oracle stays readable.</summary>
        private static string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string[] lines = raw.Replace("\r\n", "\n").Split('\n');
            var kept = lines.Select(l => l.TrimEnd())
                            .SkipWhile(string.IsNullOrWhiteSpace)
                            .ToList();

            while (kept.Count > 0 && string.IsNullOrWhiteSpace(kept[kept.Count - 1]))
            {
                kept.RemoveAt(kept.Count - 1);
            }

            if (kept.Count == 0)
            {
                return string.Empty;
            }

            int indent = kept.Where(l => l.Trim().Length > 0)
                             .Min(l => l.Length - l.TrimStart().Length);

            return string.Join(Environment.NewLine,
                kept.Select(l => l.Length >= indent ? l.Substring(indent) : l.TrimStart()));
        }

        /// <summary>Returns the statement registered under <paramref name="key"/> ("Employee.SelectAll").</summary>
        public string Get(string key)
        {
            string sql;
            if (!_statements.TryGetValue(key, out sql))
            {
                throw new KeyNotFoundException(
                    "SQL statement \"" + key + "\" is not defined. Known statements: " +
                    string.Join(", ", Keys));
            }
            return sql;
        }
    }
}
