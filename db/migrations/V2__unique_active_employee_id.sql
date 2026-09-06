-- =====================================================================
-- V2 - enforce that an active employee_id is unique.
--
-- Until now uniqueness was guarded only by the application, which did
-- SELECT COUNT(*) and then INSERT. That is a time-of-check/time-of-use
-- race: two concurrent adds both see zero and both insert.
--
-- The index is function-based on
--     CASE WHEN delete_date IS NULL THEN employee_id END
-- so it only constrains rows that have not been soft-deleted. Oracle
-- does not index rows where the expression is NULL, which means a
-- soft-deleted id can be reused, and the same id can appear many times
-- in the deleted history - both of which the application relies on.
-- =====================================================================

-- Fail with a message a human can act on, rather than ORA-01452.
DECLARE
  v_duplicates NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO v_duplicates
    FROM (SELECT employee_id
            FROM employees
           WHERE delete_date IS NULL
           GROUP BY employee_id
          HAVING COUNT(*) > 1);

  IF v_duplicates > 0 THEN
    RAISE_APPLICATION_ERROR(-20001,
      'Cannot apply V2: ' || v_duplicates || ' employee_id value(s) are duplicated '
      || 'among active rows. Resolve the duplicates, then re-run the migration.');
  END IF;
END;
/

CREATE UNIQUE INDEX uq_employees_active_id
  ON employees (CASE WHEN delete_date IS NULL THEN employee_id END);
