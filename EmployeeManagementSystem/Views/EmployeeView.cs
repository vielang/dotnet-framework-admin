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
        /// <summary>
        /// A photo the user just picked with Import, held in memory until it is saved.
        /// Null means the photo was not changed.
        ///
        /// Photos used to live in files beside the executable, with only the path in
        /// the database. That only ever worked for one person on one machine, it broke
        /// entirely if the application was installed somewhere an ordinary user cannot
        /// write, and the row and the file could disagree. Migration V5 moved them into
        /// a BLOB, so the picture now travels with the row.
        /// </summary>
        private byte[] _importedPhoto;

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

                employee.Salary = 0;

                // The photo is part of the same INSERT, so the row and its picture
                // cannot end up disagreeing. Ordering the two writes was the best that
                // could be done while the photo lived in a file; now there is only one
                // write to get right.
                Employees.Add(employee, _importedPhoto);

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
                if (Employees.Update(employee) == 0)
                {
                    UiMessage.Warn("No employee found with ID " + employee.EmployeeId + ".");
                    return;
                }

                // Only touch the photo when the user actually chose a new one, so
                // editing a phone number never disturbs the picture.
                if (_importedPhoto != null)
                {
                    Employees.SetPhoto(employee.EmployeeId, _importedPhoto);
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
            _importedPhoto = null;
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

            _importedPhoto = null;            // selecting a row discards an unsaved import
            ShowPhotoFor(employee);
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
                    var file = new FileInfo(dialog.FileName);
                    if (file.Length > MaxPhotoBytes)
                    {
                        UiMessage.Warn(
                            "That picture is " + (file.Length / 1024 / 1024) + " MB. "
                            + "Please choose one under " + (MaxPhotoBytes / 1024 / 1024) + " MB.");
                        return;
                    }

                    byte[] bytes = File.ReadAllBytes(dialog.FileName);
                    Image image = ToImage(bytes);

                    if (image == null)
                    {
                        UiMessage.Warn("That file is not a picture this application can read.");
                        return;
                    }

                    SetPicture(image);
                    _importedPhoto = bytes;   // saved with the next Add or Update
                }
                catch (Exception ex)
                {
                    UiMessage.Error(ex);
                }
            }
        }

        // ---------------------------------------------------------------- photos

        /// <summary>
        /// The largest photo this application will store. Without a ceiling, a user
        /// picking a 40 MB camera file would put 40 MB into every row read and every
        /// backup of the table.
        /// </summary>
        private const int MaxPhotoBytes = 2 * 1024 * 1024;

        /// <summary>Shows the photo held for this employee, fetching it only now.</summary>
        private void ShowPhotoFor(Employee employee)
        {
            if (!employee.HasPhoto)
            {
                SetPicture(null);
                return;
            }

            try
            {
                byte[] photo = Employees.GetPhoto(employee.EmployeeId);
                SetPicture(ToImage(photo));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not read the photo for {EmployeeId}.", employee.EmployeeId);
                SetPicture(null);
            }
        }

        /// <summary>
        /// Turns stored bytes into an Image, or null when they are not a picture.
        ///
        /// The MemoryStream is deliberately not disposed: GDI+ reads from the stream
        /// lazily, so an Image built from a disposed stream throws the moment it is
        /// drawn. The stream is collected with the Image.
        /// </summary>
        private static Image ToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                return Image.FromStream(new MemoryStream(bytes));
            }
            catch (ArgumentException)
            {
                return null;        // stored bytes are not an image the framework knows
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
    }
}
