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
        /// <summary>Folder next to the executable where employee photos are stored.</summary>
        private static string PictureDirectory
        {
            get { return Path.Combine(Application.StartupPath, "Directory"); }
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

                employee.Image = SavePicture(employee.EmployeeId);
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
            if (addEmployee_id.Text.Trim().Length == 0
                || addEmployee_fullName.Text.Trim().Length == 0
                || addEmployee_gender.Text.Trim().Length == 0
                || addEmployee_phoneNumber.Text.Trim().Length == 0
                || addEmployee_position.Text.Trim().Length == 0
                || addEmployee_status.Text.Trim().Length == 0
                || addEmployee_picture.Image == null)
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
            addEmployee_picture.Image = null;
            addEmployee_picture.ImageLocation = null;
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

            ShowPicture(employee.Image);
        }

        private void addEmployee_importBtn_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files (*.jpg; *.png)|*.jpg;*.png";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    addEmployee_picture.ImageLocation = dialog.FileName;
                }
            }
        }

        // ---------------------------------------------------------------- pictures

        /// <summary>Loads a photo without leaving the file locked. Missing files clear the box.</summary>
        private void ShowPicture(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                addEmployee_picture.Image = null;
                addEmployee_picture.ImageLocation = null;
                return;
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                addEmployee_picture.Image = Image.FromStream(stream);
            }
            addEmployee_picture.ImageLocation = path;
        }

        /// <summary>Copies the imported photo next to the executable and returns its stored path.</summary>
        private string SavePicture(string employeeId)
        {
            string source = addEmployee_picture.ImageLocation;
            string target = Path.Combine(PictureDirectory, employeeId + ".jpg");

            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            Directory.CreateDirectory(PictureDirectory);
            File.Copy(source, target, true);

            return target;
        }
    }
}
