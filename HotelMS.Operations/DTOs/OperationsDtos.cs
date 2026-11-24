namespace HotelMS.Operations.DTOs;

public record DepartmentDto(
    int DepartmentId,
    string DepartmentName,
    string? Description,
    bool IsActive,
    int StaffCount
);

public record CreateDepartmentDto(
    string DepartmentName,
    string? Description,
    bool IsActive = true
);

public record UpdateDepartmentDto(
    string DepartmentName,
    string? Description,
    bool IsActive
);

public record StaffDto(
    int StaffId,
    string StaffCode,
    int DepartmentId,
    string DepartmentName,
    string EmployeeName,
    string Address,
    string NIC,
    decimal Salary,
    int Age,
    string Occupation,
    string Email
);

public record CreateStaffDto(
    int DepartmentId,
    string EmployeeName,
    string Address,
    string NIC,
    decimal Salary,
    int Age,
    string Occupation,
    string Email
);

public record UpdateStaffDto(
    int DepartmentId,
    string EmployeeName,
    string Address,
    string NIC,
    decimal Salary,
    int Age,
    string Occupation,
    string Email
);

public record DepartmentSalarySummaryDto(
    int DepartmentId,
    string DepartmentName,
    int StaffCount,
    decimal TotalSalary
);

public record StaffSalaryReportDto(
    int TotalStaffCount,
    decimal TotalSalaryExpense,
    List<DepartmentSalarySummaryDto> Departments
);

public record HotelIncomeReportDto(
    DateTime GeneratedAt,
    decimal TotalRevenue,
    int SuccessfulPaymentsCount,
    Dictionary<string, decimal> RevenueByPaymentMethod,
    int PendingInvoicesCount,
    int PaidInvoicesCount
);
