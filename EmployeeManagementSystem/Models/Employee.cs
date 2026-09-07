namespace EmployeeManagementSystem.Models
{
    /// <summary>A row of the EMPLOYEES table.</summary>
    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string Position { get; set; }

        /// <summary>
        /// Whether this employee has a photo, without carrying the photo itself.
        /// The list queries select this rather than the BLOB: the grid shows a tick,
        /// not the picture, so fetching a couple of megabytes per row would move a
        /// lot of bytes to draw nothing. Use IEmployeeRepository.GetPhoto for the
        /// one employee the user actually selected.
        /// </summary>
        public bool HasPhoto { get; set; }

        public int Salary { get; set; }
        public string Status { get; set; }
    }
}
