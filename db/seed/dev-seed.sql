-- =====================================================================
-- Development seed data. NOT a migration - never applied to production.
-- Applied only by db\setup-db.ps1, and only when -Seed is passed.
--
-- The 'admin' password is stored in clear text because the application
-- still compares passwords in clear text (finding F1). Once hashing
-- lands, this file has to produce a hash instead.
-- =====================================================================

SET DEFINE OFF
WHENEVER SQLERROR EXIT SQL.SQLCODE

DELETE FROM employees;
DELETE FROM users;

INSERT INTO users (username, password, date_register)
VALUES ('admin', 'admin', TRUNC(SYSDATE));

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-01', 'Nguyen Van A', 'Male', '0900000001', 'Developer', 'Directory\EMID-01.jpg', 1500, TRUNC(SYSDATE), 'Active');

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-02', 'Tran Thi B', 'Female', '0900000002', 'Manager', 'Directory\EMID-02.jpg', 2500, TRUNC(SYSDATE), 'Active');

INSERT INTO employees (employee_id, full_name, gender, contact_number, position, image, salary, insert_date, status)
VALUES ('EMID-03', 'Le Van C', 'Male', '0900000003', 'Designer', 'Directory\EMID-03.jpg', 1200, TRUNC(SYSDATE), 'Inactive');

COMMIT;

SELECT 'users: ' || COUNT(*) AS seeded FROM users;
SELECT 'employees: ' || COUNT(*) AS seeded FROM employees;

EXIT
