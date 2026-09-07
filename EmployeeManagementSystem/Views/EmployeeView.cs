using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Serilog;
using EmployeeManagementSystem.Forms;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Views
{
    /// <summary>Create, update and soft-delete employees.</summary>
    public partial class EmployeeView : DataView
    {
        /// <summary>Where photo paths are stored relative to, and where new photos are copied.</summary>
        private const string PictureFolderName = "Directory";

        /// <summary>The photo path already stored for the selected employee, if any.</summary>
        private string _storedPicturePath;

        /// <summary>A file the user just picked with Import, not yet saved. Null when unchanged.</summary>
        private string _importedPicturePath;

        /// <summary>
        /// Folder that new photos are written into. Defaults to a folder beside the
        /// executable; settable so a test does not have to write next to whatever
        /// process happens to be hosting it.
        ///
        /// Writing beside the executable is the same bet AppLog deliberately refused
        /// to make: if this application is ever installed under Program Files, an
        /// ordinary user cannot create this folder and photos will not save. Storing
        /// photos properly is finding F17.
        /// </summary>
        internal string PictureDirectory { get; set; }

        public EmployeeView()
        {
            InitializeComponent();

            PictureDirectory = Path.Combine(Application.StartupPath, PictureFolderName);
        }

        protected override void LoadData()
        {
            EmployeeGrid.Bind(dataGridView1, Employees.GetAll());
        }

        // ---------------------------------------------------------------- commands

        private void addEmployee_addBtn_Click(object sender, EventArgs e)
        {
            Employee employee = ReadForm();
            if (employee == null)
            {
                return;
            }

            try
            {
                if (Employees.ExistsByEmployeeId(employee.EmployeeId))
                {
                    UiMessage.Warn(employee.EmployeeId + " is already taken");
                    return;
                }

                employee.Image = PlannedPicturePath(employee.EmployeeId, null);
                employee.Salary = 0;

                // Database first. Only once the row exists does the photo get copied,
                // so a rejected insert never leaves a file behind.
                Employees.Add(employee);

                if (!TryCommitPicture(employee.EmployeeId))
                {
                    UiMessage.Warn("The employee was saved, but the photo could not be stored.");
                    ClearStoredImage(employee.EmployeeId);
                }

                LoadData();
                UiMessage.Info("Added successfully!");
                ClearFields();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        private void addEmployee_updateBtn_Click(object sender, EventArgs e)
        {
            Employee employee = ReadForm();
            if (employee == null)
            {
                return;
            }

            if (!UiMessage.Confirm("Are you sure you want to UPDATE Employee ID: " + employee.EmployeeId + "?"))
            {
                UiMessage.Cancelled();
                return;
            }

            try
            {
                employee.Image = PlannedPicturePath(employee.EmployeeId, _storedPicturePath);

                if (Employees.Update(employee) == 0)
                {
                    // Nothing was written, so nothing on disk has been touched either -
                    // the existing photo survives.
                    UiMessage.Warn("No employee found with ID " + employee.EmployeeId + ".");
                    return;
                }

                if (!TryCommitPicture(employee.EmployeeId))
                {
                    UiMessage.Warn("The employee was saved, but the photo could not be stored.");
                    ClearStoredImage(employee.EmployeeId);
                }

                LoadData();
                UiMessage.Info("Updated successfully!");
                ClearFields();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        private void addEmployee_deleteBtn_Click(object sender, EventArgs e)
        {
            string employeeId = addEmployee_id.Text.Trim();

            if (employeeId.Length == 0)
            {
                UiMessage.Warn("Select the employee you want to delete.");
                return;
            }

            if (!UiMessage.Confirm("Are you sure you want to DELETE Employee ID: " + employeeId + "?"))
            {
                UiMessage.Cancelled();
                return;
            }

            try
            {
                if (Employees.SoftDelete(employeeId) == 0)
                {
                    UiMessage.Warn("No employee found with ID " + employeeId + ".");
                    return;
                }

                LoadData();
                UiMessage.Info("Deleted successfully!");
                ClearFields();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        private void addEmployee_clearBtn_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        // ------------------------------------------------------------------- form

        /// <summary>Reads the form into an Employee, or returns null after reporting what is missing.</summary>
        private Employee ReadForm()
        {
            // A photo is optional. Requiring one used to make any employee whose photo
            // file was missing impossible to update at all - the form simply reported
            // "fill all blank fields" and never said which.
            if (addEmployee_id.Text.Trim().Length == 0
                || addEmployee_fullName.Text.Trim().Length == 0
                || addEmployee_gender.Text.Trim().Length == 0
                || addEmployee_phoneNumber.Text.Trim().Length == 0
                || addEmployee_position.Text.Trim().Length == 0
                || addEmployee_status.Text.Trim().Length == 0)
            {
                UiMessage.Warn("Please fill all blank fields.");
                return null;
            }

            return new Employee
            {
                EmployeeId = addEmployee_id.Text.Trim(),
                FullName = addEmployee_fullName.Text.Trim(),
                Gender = addEmployee_gender.Text.Trim(),
                ContactNumber = addEmployee_phoneNumber.Text.Trim(),
                Position = addEmployee_position.Text.Trim(),
                Status = addEmployee_status.Text.Trim(),
            };
        }

        private void ClearFields()
        {
            addEmployee_id.Text = "";
            addEmployee_fullName.Text = "";
            addEmployee_gender.SelectedIndex = -1;
            addEmployee_phoneNumber.Text = "";
            addEmployee_position.SelectedIndex = -1;
            addEmployee_status.SelectedIndex = -1;

            SetPicture(null);
            _storedPicturePath = null;
            _importedPicturePath = null;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Employee employee = EmployeeGrid.RowAt(dataGridView1, e.RowIndex);
            if (employee == null)
            {
                return;
            }

            addEmployee_id.Text = employee.EmployeeId;
            addEmployee_fullName.Text = employee.FullName;
            addEmployee_gender.Text = employee.Gender;
            addEmployee_phoneNumber.Text = employee.ContactNumber;
            addEmployee_position.Text = employee.Position;
            addEmployee_status.Text = employee.Status;

            _storedPicturePath = employee.Image;
            _importedPicturePath = null;      // selecting a row discards an unsaved import
            ShowPicture(employee.Image);
        }

        private void addEmployee_importBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files (*.jpg; *.png)|*.jpg;*.png";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        SetPicture(Image.FromStream(stream));
                    }
                    _importedPicturePath = dialog.FileName;
                }
                catch (Exception ex)
                {
                    UiMessage.Error(ex);
                }
            }
        }

        // ---------------------------------------------------------------- pictures

        /// <summary>
        /// Turns a stored photo path into one that can actually be opened.
        ///
        /// Rows written by this application store a path relative to the executable
        /// ("Directory\EMID-01.jpg"). A bare relative path is resolved against the
        /// *current working directory*, which is not necessarily where the .exe lives -
        /// launching from a shortcut with a different "Start in" was enough to make
        /// every photo disappear. Older rows may still hold an absolute path, so both
        /// are accepted.
        /// </summary>
        private static string ResolvePicturePath(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
            {
                return null;
            }

            return Path.IsPathRooted(stored)
                ? stored
                : Path.Combine(Application.StartupPath, stored);
        }

        /// <summary>Loads a photo without leaving the file locked. Missing files clear the box.</summary>
        private void ShowPicture(string storedPath)
        {
            string full = ResolvePicturePath(storedPath);

            if (full == null || !File.Exists(full))
            {
                SetPicture(null);
                return;
            }

            // Read through a stream so the file itself is not left locked.
            using (var stream = new FileStream(full, FileMode.Open, FileAccess.Read))
            {
                SetPicture(Image.FromStream(stream));
            }
        }

        /// <summary>
        /// Replaces the displayed photo, disposing the previous one.
        ///
        /// PictureBox does not dispose the image it is holding when a new one is
        /// assigned, so without this every row click would abandon a bitmap and leak
        /// the unmanaged GDI+ memory behind it.
        /// </summary>
        private void SetPicture(Image image)
        {
            Image previous = addEmployee_picture.Image;
            addEmployee_picture.Image = image;

            if (previous != null && !ReferenceEquals(previous, image))
            {
                previous.Dispose();
            }
        }

        /// <summary>
        /// Works out what belongs in the employee's image column, copying the file in
        /// when the user imported one.
        /// </summary>
        /// <param name="existingPath">
        /// What is already stored for this employee, kept when nothing was imported.
        /// Adding a new employee passes null: a new person does not inherit the photo
        /// of whichever row happened to be selected in the grid.
        /// </param>
        private string PlannedPicturePath(string employeeId, string existingPath)
        {
            if (string.IsNullOrEmpty(_importedPicturePath))
            {
                return existingPath;            // nothing was imported: leave it alone
            }

            // Stored relative, so the application keeps working if the folder moves.
            return Path.Combine(PictureFolderName, employeeId + ".jpg");
        }

        /// <summary>
        /// Copies the imported photo into place. Call this only once the database write
        /// has succeeded.
        ///
        /// A file cannot take part in a database transaction, so the two are ordered
        /// instead: the database first, the file second. The copy overwrites, and it
        /// used to run *before* the UPDATE - so an update that changed no rows (someone
        /// else had deleted the employee) destroyed the old photo and changed nothing
        /// in the database. Doing the database first means a failure there leaves the
        /// photo exactly as it was.
        /// </summary>
        /// <returns>True when there was nothing to do, or the photo was stored.</returns>
        private bool TryCommitPicture(string employeeId)
        {
            if (string.IsNullOrEmpty(_importedPicturePath))
            {
                return true;                    // nothing was imported
            }

            string target = Path.Combine(PictureDirectory, employeeId + ".jpg");

            try
            {
                Directory.CreateDirectory(PictureDirectory);
                File.Copy(_importedPicturePath, target, true);
                Log.Information("Stored the photo for {EmployeeId}.", employeeId);
                return true;
            }
            catch (Exception ex)
            {
                // Deliberately no MessageBox here. A helper that does I/O and also puts
                // a modal dialog on screen cannot be tested without hanging, and it is
                // the same mistake the data layer used to make. The caller reports.
                Log.Error(ex, "Saved {EmployeeId} but could not store the photo.", employeeId);
                return false;
            }
        }

        /// <summary>Compensating write: the row must not claim a photo that is not there.</summary>
        private void ClearStoredImage(string employeeId)
        {
            try
            {
                Employee stored = Employees.GetAll()
                    .FirstOrDefault(e => e.EmployeeId == employeeId);

                if (stored == null || stored.Image == null)
                {
                    return;
                }

                stored.Image = null;
                Employees.Update(stored);
            }
            catch (Exception ex)
            {
                // Nothing further to try. The application already tolerates a missing
                // photo file, so this is untidy rather than broken.
                Log.Error(ex, "Could not clear the photo path for {EmployeeId}.", employeeId);
            }
        }
    }
}
