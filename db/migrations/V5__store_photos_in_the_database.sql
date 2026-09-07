-- =====================================================================
-- V5 - keep employee photos in the database instead of on the disk.
--
-- The image column held a path such as "Directory\EMID-01.jpg", pointing
-- at a file next to the executable. Three things were wrong with that:
--
--   * it only works for one person on one machine. A second user opening
--     the same employee sees an empty picture box, because the file is on
--     somebody else's disk.
--   * it does not work at all if the application is installed under
--     Program Files, where an ordinary user cannot create the folder.
--   * the row and the file could disagree. Ordering the writes narrowed
--     that window but could not close it: a file cannot take part in a
--     database transaction.
--
-- A BLOB closes all three. The photo travels with the row, needs no
-- filesystem permissions, and is written by the same INSERT that creates
-- the employee - so it is genuinely atomic rather than merely ordered.
--
-- ---------------------------------------------------------------------
-- READ THIS BEFORE APPLYING TO A DATABASE WITH REAL PHOTOS
-- ---------------------------------------------------------------------
-- The old column stored a path, not an image, and SQL cannot read a file
-- off the disk to fill the new column. So existing photos are NOT
-- migrated: the paths are dropped and the photos must be imported again
-- through the application.
--
-- The files themselves are untouched - they are still in the Directory
-- folder next to the executable, so nothing is destroyed, only unlinked.
-- =====================================================================

ALTER TABLE employees ADD (photo BLOB);

-- Say out loud how many employees will need their photo importing again.
DECLARE
  v_with_path NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_with_path FROM employees WHERE image IS NOT NULL;

  IF v_with_path > 0 THEN
    DBMS_OUTPUT.PUT_LINE(
      'V5: ' || v_with_path || ' employee(s) had a photo path. The files are still on '
      || 'disk in the Directory folder, but the photos must be re-imported through '
      || 'the application.');
  END IF;
END;
/

ALTER TABLE employees DROP COLUMN image;
