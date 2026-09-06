using System;
using System.Collections.Generic;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Integration tests against a real Oracle database. Each test creates its own
    /// rows under a unique prefix and deletes them afterwards, so the tests do not
    /// depend on the seed data and can run in any order.
    /// </summary>
    [Collection(OracleCollection.Name)]
    [Trait("Category", "Integration")]
    public class EmployeeRepositoryTests : IDisposable
    {
        private readonly OracleFixture _oracle;
        private readonly IEmployeeRepository _employees;
        private readonly string _prefix;

        public EmployeeRepositoryTests(OracleFixture oracle)
        {
            _oracle = oracle;
            _employees = oracle.CreateEmployeeRepository();
            _prefix = "T" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        public void Dispose()
        {
            _oracle.DeleteEmployees(_prefix);
        }

        private Employee NewEmployee(string suffix = "1", string status = EmployeeStatus.Active)
        {
            return new Employee
            {
                EmployeeId = _prefix + "-" + suffix,
                FullName = "Test Person " + suffix,
                Gender = "Female",
                ContactNumber = "0900000123",
                Position = "Tester",
                Image = null,
                Salary = 1234,
                Status = status,
            };
        }

        private Employee Reload(string employeeId)
        {
            return _employees.GetAll().SingleOrDefault(e => e.EmployeeId == employeeId);
        }

        // ------------------------------------------------------------ mapping

        [SkippableFact]
        public void Maps_snake_case_columns_onto_the_model()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);

            Employee loaded = Reload(added.EmployeeId);

            Assert.NotNull(loaded);
            Assert.Equal(added.EmployeeId, loaded.EmployeeId);       // employee_id
            Assert.Equal(added.FullName, loaded.FullName);           // full_name
            Assert.Equal(added.ContactNumber, loaded.ContactNumber); // contact_number
            Assert.Equal(added.Position, loaded.Position);
            Assert.Equal(added.Status, loaded.Status);
            Assert.True(loaded.Id > 0, "identity column should be populated");
        }

        [SkippableFact]
        public void Reads_oracle_NUMBER_as_int()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            added.Salary = 4242;
            _employees.Add(added);

            Assert.Equal(4242, Reload(added.EmployeeId).Salary);
        }

        [SkippableFact]
        public void Keeps_a_null_image_null()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);

            Assert.Null(Reload(added.EmployeeId).Image);
        }

        // ------------------------------------------------------------- writes

        [SkippableFact]
        public void Update_changes_the_editable_fields()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);

            added.FullName = "Renamed Person";
            added.Position = "Lead Tester";
            added.Status = EmployeeStatus.Inactive;

            Assert.Equal(1, _employees.Update(added));

            Employee loaded = Reload(added.EmployeeId);
            Assert.Equal("Renamed Person", loaded.FullName);
            Assert.Equal("Lead Tester", loaded.Position);
            Assert.Equal(EmployeeStatus.Inactive, loaded.Status);
        }

        /// <summary>
        /// Regression test for parameter binding. UpdateSalary supplies salary,
        /// updateDate and employeeId in that order while the SQL mentions them in a
        /// different order in the SET and WHERE clauses. ODP.NET binds by position
        /// unless BindByName is set, so without OracleParams this test writes the
        /// salary into the wrong column or matches the wrong row.
        /// </summary>
        [SkippableFact]
        public void UpdateSalary_binds_parameters_by_name_not_position()
        {
            _oracle.SkipIfUnavailable();

            Employee a = NewEmployee("A");
            Employee b = NewEmployee("B");
            a.Salary = 100;
            b.Salary = 200;
            _employees.Add(a);
            _employees.Add(b);

            Assert.Equal(1, _employees.UpdateSalary(a.EmployeeId, 9999));

            Assert.Equal(9999, Reload(a.EmployeeId).Salary);
            Assert.Equal(200, Reload(b.EmployeeId).Salary);   // untouched
        }

        [SkippableFact]
        public void Update_reports_zero_rows_for_an_unknown_employee()
        {
            _oracle.SkipIfUnavailable();

            Employee ghost = NewEmployee("GHOST");

            Assert.Equal(0, _employees.Update(ghost));
        }

        [SkippableFact]
        public void UpdateSalary_reports_zero_rows_for_an_unknown_employee()
        {
            _oracle.SkipIfUnavailable();

            Assert.Equal(0, _employees.UpdateSalary(_prefix + "-GHOST", 1));
        }

        // -------------------------------------------------------- soft delete

        [SkippableFact]
        public void SoftDelete_hides_the_row_from_every_query()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);
            Assert.True(_employees.ExistsByEmployeeId(added.EmployeeId));

            Assert.Equal(1, _employees.SoftDelete(added.EmployeeId));

            Assert.False(_employees.ExistsByEmployeeId(added.EmployeeId));
            Assert.Null(Reload(added.EmployeeId));
        }

        [SkippableFact]
        public void SoftDelete_a_second_time_changes_nothing()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);
            _employees.SoftDelete(added.EmployeeId);

            Assert.Equal(0, _employees.SoftDelete(added.EmployeeId));
        }

        [SkippableFact]
        public void Update_will_not_resurrect_a_soft_deleted_row()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);
            _employees.SoftDelete(added.EmployeeId);

            added.FullName = "Should not apply";

            Assert.Equal(0, _employees.Update(added));
        }

        // ------------------------------------------------------------ queries

        [SkippableFact]
        public void GetByStatus_returns_only_that_status()
        {
            _oracle.SkipIfUnavailable();

            _employees.Add(NewEmployee("ACT", EmployeeStatus.Active));
            _employees.Add(NewEmployee("INA", EmployeeStatus.Inactive));

            List<Employee> mine = _employees.GetByStatus(EmployeeStatus.Active)
                                            .Where(e => e.EmployeeId.StartsWith(_prefix))
                                            .ToList();

            Assert.Single(mine);
            Assert.Equal(_prefix + "-ACT", mine[0].EmployeeId);
        }

        [SkippableFact]
        public void Counts_agree_with_the_rows_returned()
        {
            _oracle.SkipIfUnavailable();

            int before = _employees.CountAll();
            int activeBefore = _employees.CountByStatus(EmployeeStatus.Active);

            _employees.Add(NewEmployee("ACT", EmployeeStatus.Active));
            _employees.Add(NewEmployee("INA", EmployeeStatus.Inactive));

            Assert.Equal(before + 2, _employees.CountAll());
            Assert.Equal(activeBefore + 1, _employees.CountByStatus(EmployeeStatus.Active));
            Assert.Equal(_employees.CountAll(), _employees.GetAll().Count);
        }

        [SkippableFact]
        public void ExistsByEmployeeId_is_false_for_an_unknown_id()
        {
            _oracle.SkipIfUnavailable();

            Assert.False(_employees.ExistsByEmployeeId(_prefix + "-NOBODY"));
        }

        // ------------------------------------------------- integrity (V2, V3)

        /// <summary>
        /// Finding F4, now enforced by the unique index in migration V2. The
        /// application's check-then-insert races; this is the guard that does not.
        /// </summary>
        [SkippableFact]
        public void Duplicate_active_employee_id_is_rejected_by_the_database()
        {
            _oracle.SkipIfUnavailable();

            Employee first = NewEmployee();
            _employees.Add(first);

            Employee duplicate = NewEmployee();
            duplicate.FullName = "Duplicate";

            var ex = Assert.Throws<DuplicateKeyException>(() => _employees.Add(duplicate));

            Assert.Contains(first.EmployeeId, ex.Message);
            Assert.Single(_employees.GetAll(), e => e.EmployeeId == first.EmployeeId);
        }

        /// <summary>
        /// The unique index is function-based on
        /// "CASE WHEN delete_date IS NULL THEN employee_id END", so it must constrain
        /// only active rows. Reusing the id of a soft-deleted employee has to keep
        /// working, or V2 would have broken the delete-then-re-add workflow.
        /// </summary>
        [SkippableFact]
        public void An_employee_id_can_be_reused_after_a_soft_delete()
        {
            _oracle.SkipIfUnavailable();

            Employee original = NewEmployee();
            _employees.Add(original);
            _employees.SoftDelete(original.EmployeeId);

            Employee reused = NewEmployee();
            reused.FullName = "Second Person, Same Id";

            _employees.Add(reused);   // must not throw

            Employee active = Reload(original.EmployeeId);
            Assert.NotNull(active);
            Assert.Equal("Second Person, Same Id", active.FullName);
        }

        [SkippableFact]
        public void A_status_outside_the_allowed_values_is_rejected()
        {
            _oracle.SkipIfUnavailable();

            Employee bad = NewEmployee();
            bad.Status = "not-a-real-status";

            Assert.Throws<DataRuleViolationException>(() => _employees.Add(bad));
        }

        [SkippableFact]
        public void A_negative_salary_is_rejected_on_insert()
        {
            _oracle.SkipIfUnavailable();

            Employee bad = NewEmployee();
            bad.Salary = -5000;

            Assert.Throws<DataRuleViolationException>(() => _employees.Add(bad));
        }

        [SkippableFact]
        public void A_negative_salary_is_rejected_on_update()
        {
            _oracle.SkipIfUnavailable();

            Employee added = NewEmployee();
            _employees.Add(added);

            Assert.Throws<DataRuleViolationException>(
                () => _employees.UpdateSalary(added.EmployeeId, -1));

            Assert.Equal(added.Salary, Reload(added.EmployeeId).Salary);
        }

        [SkippableFact]
        public void An_employee_id_longer_than_the_column_is_rejected_with_a_readable_message()
        {
            _oracle.SkipIfUnavailable();

            Employee tooLong = NewEmployee();
            tooLong.EmployeeId = new string('X', 60);   // employee_id is VARCHAR2(50)

            var ex = Assert.Throws<DataRuleViolationException>(() => _employees.Add(tooLong));

            Assert.Contains("too long", ex.Message);
            Assert.DoesNotContain("ORA-", ex.Message);
        }
    }
}
