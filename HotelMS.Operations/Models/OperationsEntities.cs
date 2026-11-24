namespace HotelMS.Operations.Models;

public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
}

public class Staff
{
    public int StaffId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public string EmployeeName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string NIC { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int Age { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
