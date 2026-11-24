using HotelMS.Operations.Models;

namespace HotelMS.Operations.Data;

public static class DbInitializer
{
    public static void Initialize(OperationsDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Departments.Any()) return;

        var hr = new Department
        {
            DepartmentName = "Human Resources",
            Description = "Staff recruitment, onboarding, and payroll",
            IsActive = true
        };

        var housekeeping = new Department
        {
            DepartmentName = "Housekeeping",
            Description = "Room cleaning and maintenance operations",
            IsActive = true
        };

        var frontDesk = new Department
        {
            DepartmentName = "Front Desk",
            Description = "Reception, check-in, check-out, and guest support",
            IsActive = true
        };

        context.Departments.AddRange(hr, housekeeping, frontDesk);
        context.SaveChanges();

        var staff1 = new Staff
        {
            StaffCode = "STF1001",
            DepartmentId = frontDesk.DepartmentId,
            EmployeeName = "Alice Cooper",
            Address = "742 Evergreen Terrace",
            NIC = "NIC-9901-AC",
            Salary = 35000m,
            Age = 28,
            Occupation = "Receptionist",
            Email = "alice.cooper@hotelms.com"
        };

        var staff2 = new Staff
        {
            StaffCode = "STF1002",
            DepartmentId = housekeeping.DepartmentId,
            EmployeeName = "Bob Marley",
            Address = "56 Hope Road",
            NIC = "NIC-9902-BM",
            Salary = 30000m,
            Age = 35,
            Occupation = "Housekeeping Supervisor",
            Email = "bob.marley@hotelms.com"
        };

        context.Staff.AddRange(staff1, staff2);
        context.SaveChanges();
    }
}
