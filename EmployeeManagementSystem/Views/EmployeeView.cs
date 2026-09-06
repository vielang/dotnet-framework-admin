using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
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

        /// <summary>Folder next to the executable where employee photos are stored.</summary>
        private static string PictureDirectory
        {
            get { return Path.Combine(Application.StartupPath, PictureFolderName); }
        }

        public EmployeeView()
        {
            InitializeComponent();
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

                employee.Image = SavePicture(employee.EmployeeId, null);
                employee.Salary = 0;

                Employees.Add(employee);

                LoadData();
                UiMessage.Info("Added successfully!");
                clearFields();
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
                employee.Image = SavePicture(employee.EmployeeId, _storedPicturePath);

                if (Employees.Update(employee) == 0)
                {
                    UiMessage.Warn("No employee found with ID " + employee.EmployeeId + ".");
                    return;
                }

                LoadData();
                UiMessage.Info("Updated successfully!");
                clearFields();
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
                clearFields();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        private void addEmployee_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
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

        public void clearFields()
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
        private string SavePicture(string employeeId, string existingPath)
        {
            if (string.IsNullOrEmpty(_importedPicturePath))
            {
                return existingPath;            // nothing was imported: leave it alone
            }

            string relative = Path.Combine(PictureFolderName, employeeId + ".jpg");
            string target = Path.Combine(Application.StartupPath, relative);

            Directory.CreateDirectory(PictureDirectory);
            File.Copy(_importedPicturePath, target, true);

            // Stored relative, so the application keeps working if the folder moves.
            return relative;
        }
    }
}
