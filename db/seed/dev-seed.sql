-- =====================================================================
-- Development seed data. NOT a migration - never applied to production.
-- Applied only by db\setup-db.ps1, and only when -Seed is passed.
--
-- The admin password is 'admin', stored as a PBKDF2-HMAC-SHA256 hash. The
-- values below were produced by Data/PasswordHasher.cs; they are not
-- computed here because a second implementation in PL/SQL would be one
-- more thing to keep in step with the C# one.
--
-- To change the seeded password, hash the new one with PasswordHasher.Create
-- and replace all four values. Do not hand-edit the hash.
--
-- Only EMID-01 gets a photo, because Directory\EMID-01.jpg is the only image
-- that actually ships. The other two pointed at files that do not exist, which
-- showed an empty picture box and - until the form stopped demanding a photo -
-- made those rows impossible to update at all. NULL is the honest value for
-- "this employee has no photo".
-- =====================================================================

SET DEFINE OFF
WHENEVER SQLERROR EXIT SQL.SQLCODE

DELETE FROM employees;
DELETE FROM users;

INSERT INTO users (username, date_register,
                   password_algorithm, password_hash, password_salt, password_iterations)
VALUES ('admin', TRUNC(SYSDATE),
        'PBKDF2-SHA256',
        'mlOYMBegwFiTrJtYwdXfvXCsoj6NO0/e2wEMdenDu8U=',
        'iPAtEAXdxIXTjBCJVp7peA==',
        120000);

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-01', 'Nguyen Van A', 'Male', '0900000001', 'Developer', 'Directory\EMID-01.jpg', 1500, TRUNC(SYSDATE), 'Active');

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-02', 'Tran Thi B', 'Female', '0900000002', 'Manager', NULL, 2500, TRUNC(SYSDATE), 'Active');

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-03', 'Le Van C', 'Male', '0900000003', 'Designer', NULL, 1200, TRUNC(SYSDATE), 'Inactive');

COMMIT;

SELECT 'users: ' || COUNT(*) AS seeded FROM users;
SELECT 'employees: ' || COUNT(*) AS seeded FROM employees;

EXIT
