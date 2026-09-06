-- =====================================================================
-- V3 - constrain the employee value domains.
--
-- Before this, the database accepted status = 'not-a-real-status' and
-- salary = -5000. Only the combo boxes on the form kept the data clean,
-- so every other write path bypassed the rules.
--
-- Note on NULL: a CHECK constraint fails only when it evaluates to
-- FALSE, so these permit NULL. Making status NOT NULL is a separate
-- decision because it needs a rule for whatever existing rows hold
-- NULL, and that is a data question rather than a schema question.
-- The application always writes a status.
-- =====================================================================

DECLARE
  v_bad_status NUMBER;
  v_bad_salary NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_bad_status
    FROM employees
   WHERE status IS NOT NULL
     AND status NOT IN ('Active', 'Inactive');

  SELECT COUNT(*) INTO v_bad_salary
    FROM employees
   WHERE salary < 0;

  IF v_bad_status > 0 OR v_bad_salary > 0 THEN
    RAISE_APPLICATION_ERROR(-20002,
      'Cannot apply V3: ' || v_bad_status || ' row(s) have a status outside '
      || '(Active, Inactive) and ' || v_bad_salary || ' row(s) have a negative salary. '
      || 'Correct the data, then re-run the migration.');
  END IF;
END;
/

ALTER TABLE employees ADD CONSTRAINT ck_employees_status
  CHECK (status IN ('Active', 'Inactive'));

ALTER TABLE employees ADD CONSTRAINT ck_employees_salary
  CHECK (salary >= 0);
