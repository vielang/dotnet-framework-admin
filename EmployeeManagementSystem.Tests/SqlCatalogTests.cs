using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmployeeManagementSystem.Data;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Unit tests for the XML SQL catalog. No database required.
    /// </summary>
    public class SqlCatalogTests : IDisposable
    {
        private readonly string _dir;

        public SqlCatalogTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "sqlcatalog-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); } catch (IOException) { }
        }

        private void WriteFile(string name, string xml)
        {
            File.WriteAllText(Path.Combine(_dir, name), xml);
        }

        // ------------------------------------------------------------ loading

        [Fact]
        public void Loads_statements_keyed_by_namespace_and_id()
        {
            WriteFile("a.xml", @"<statements namespace='Employee'>
                                   <statement id='SelectAll'>SELECT 1 FROM dual</statement>
                                 </statements>");

            SqlCatalog catalog = SqlCatalog.LoadFrom(_dir);

            Assert.Equal("SELECT 1 FROM dual", catalog.Get("Employee.SelectAll"));
        }

        [Fact]
        public void Merges_multiple_files()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='One'>SELECT 1 FROM dual</statement></statements>");
            WriteFile("b.xml", "<statements namespace='User'><statement id='Two'>SELECT 2 FROM dual</statement></statements>");

            SqlCatalog catalog = SqlCatalog.LoadFrom(_dir);

            Assert.Equal(new[] { "Employee.One", "User.Two" }, catalog.Keys.ToArray());
        }

        [Fact]
        public void Key_lookup_is_case_insensitive()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='SelectAll'>SELECT 1 FROM dual</statement></statements>");

            SqlCatalog catalog = SqlCatalog.LoadFrom(_dir);

            Assert.Equal("SELECT 1 FROM dual", catalog.Get("employee.selectall"));
        }

        [Fact]
        public void Strips_the_xml_indentation_from_the_statement()
        {
            WriteFile("a.xml",
@"<statements namespace='Employee'>
  <statement id='SelectAll'>
    SELECT id
    FROM   employees
  </statement>
</statements>");

            string sql = SqlCatalog.LoadFrom(_dir).Get("Employee.SelectAll");

            Assert.Equal("SELECT id" + Environment.NewLine + "FROM   employees", sql);
        }

        // ------------------------------------------------------------- errors

        [Fact]
        public void Rejects_a_duplicate_key()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='Dup'>SELECT 1 FROM dual</statement></statements>");
            WriteFile("b.xml", "<statements namespace='Employee'><statement id='Dup'>SELECT 2 FROM dual</statement></statements>");

            var ex = Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));

            Assert.Contains("Employee.Dup", ex.Message);
        }

        [Fact]
        public void Rejects_a_missing_namespace()
        {
            WriteFile("a.xml", "<statements><statement id='X'>SELECT 1 FROM dual</statement></statements>");

            var ex = Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));

            Assert.Contains("namespace", ex.Message);
        }

        [Fact]
        public void Rejects_a_missing_id()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement>SELECT 1 FROM dual</statement></statements>");

            var ex = Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));

            Assert.Contains("id", ex.Message);
        }

        [Fact]
        public void Rejects_an_empty_statement()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='Blank'>   </statement></statements>");

            var ex = Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));

            Assert.Contains("empty", ex.Message);
        }

        [Fact]
        public void Rejects_a_wrong_root_element()
        {
            WriteFile("a.xml", "<sqlmap namespace='Employee'><statement id='X'>SELECT 1 FROM dual</statement></sqlmap>");

            Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));
        }

        [Fact]
        public void Rejects_malformed_xml()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='X'>SELECT");

            Assert.Throws<InvalidOperationException>(() => SqlCatalog.LoadFrom(_dir));
        }

        [Fact]
        public void Rejects_a_folder_with_no_files()
        {
            Assert.Throws<FileNotFoundException>(() => SqlCatalog.LoadFrom(_dir));
        }

        [Fact]
        public void Rejects_a_missing_folder()
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => SqlCatalog.LoadFrom(Path.Combine(_dir, "nope")));
        }

        [Fact]
        public void Unknown_key_reports_the_keys_it_does_know()
        {
            WriteFile("a.xml", "<statements namespace='Employee'><statement id='SelectAll'>SELECT 1 FROM dual</statement></statements>");

            SqlCatalog catalog = SqlCatalog.LoadFrom(_dir);
            var ex = Assert.Throws<KeyNotFoundException>(() => catalog.Get("Employee.Missing"));

            Assert.Contains("Employee.Missing", ex.Message);
            Assert.Contains("Employee.SelectAll", ex.Message);
        }

        // -------------------------------------------- the shipped SQL catalog

        /// <summary>
        /// Guards against a repository asking for a statement that no longer exists -
        /// the failure would otherwise only appear when a user clicks that button.
        /// </summary>
        [Fact]
        public void Shipped_catalog_defines_every_statement_the_repositories_use()
        {
            string[] required =
            {
                "Employee.SelectAll",
                "Employee.SelectByStatus",
                "Employee.CountAll",
                "Employee.CountByStatus",
                "Employee.ExistsByEmployeeId",
                "Employee.Insert",
                "Employee.Update",
                "Employee.UpdateSalary",
                "Employee.SoftDelete",
                "User.FindByUsername",
                "User.CountByUsername",
                "User.Insert",
            };

            SqlCatalog catalog = SqlCatalog.LoadFromDefaultDirectory();

            foreach (string key in required)
            {
                // Get throws with a helpful message when the key is absent.
                Assert.False(string.IsNullOrWhiteSpace(catalog.Get(key)), key + " is empty");
            }
        }

        [Fact]
        public void Shipped_catalog_uses_oracle_style_bind_variables_only()
        {
            SqlCatalog catalog = SqlCatalog.LoadFromDefaultDirectory();

            foreach (string key in catalog.Keys)
            {
                string sql = catalog.Get(key);

                // "@name" is SQL Server syntax; it would bind as a literal against Oracle.
                Assert.DoesNotContain("@", sql);
            }
        }
    }
}
