-- =====================================================================
-- V4 - stop storing passwords in clear text.
--
-- Before this, users.password held the password itself and login was a
-- string comparison inside SQL. Anyone who could read the table had every
-- account, and the password travelled through a WHERE clause on every
-- login attempt.
--
-- After this, the table holds only a PBKDF2-HMAC-SHA256 derivation, and
-- verification happens in the application (Data/PasswordHasher.cs).
--
-- ---------------------------------------------------------------------
-- READ THIS BEFORE APPLYING TO A DATABASE THAT HAS REAL ACCOUNTS
-- ---------------------------------------------------------------------
-- A hash cannot be computed from inside this migration: it has to match
-- the application's algorithm, iteration count and encoding exactly, and
-- reproducing that in PL/SQL would be a second implementation to keep in
-- step - the kind of duplication that silently drifts.
--
-- So this migration DROPS the clear-text column without converting it.
-- Existing rows keep their id and username but end up with no usable
-- credential, which means:
--
--     *** EVERY EXISTING ACCOUNT CAN NO LONGER LOG IN ***
--
-- Those accounts must be re-created. Because username is UNIQUE, the old
-- row has to go first:
--
--     DELETE FROM users WHERE password_hash IS NULL;
--
-- That statement is deliberately NOT part of this migration - deleting
-- accounts is a decision for whoever owns the data, not for a schema
-- change. On a development database, db\setup-db.ps1 -Seed recreates the
-- admin account with a proper hash.
-- =====================================================================

ALTER TABLE users ADD (
  password_algorithm  VARCHAR2(50),
  password_hash       VARCHAR2(200),
  password_salt       VARCHAR2(100),
  password_iterations NUMBER(10)
);

-- Report what is about to be locked out, so it appears in the migration log
-- instead of being discovered later by a confused user.
DECLARE
  v_orphans NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_orphans FROM users;

  IF v_orphans > 0 THEN
    DBMS_OUTPUT.PUT_LINE(
      'V4: ' || v_orphans || ' existing account(s) now have no usable password '
      || 'and must be re-created. See the comment at the top of this migration.');
  END IF;
END;
/

ALTER TABLE users DROP COLUMN password;

-- A row is only a valid credential when all four parts are present.
ALTER TABLE users ADD CONSTRAINT ck_users_password_complete CHECK (
  (password_algorithm IS NULL AND password_hash IS NULL
   AND password_salt IS NULL AND password_iterations IS NULL)
  OR
  (password_algorithm IS NOT NULL AND password_hash IS NOT NULL
   AND password_salt IS NOT NULL AND password_iterations > 0)
);
