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
        public string Image { get; set; }
        public int Salary { get; set; }
        public string Status { get; set; }
    }
}
