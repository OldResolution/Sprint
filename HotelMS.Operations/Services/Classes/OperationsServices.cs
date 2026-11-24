using HotelMS.Operations.Clients;
using HotelMS.Operations.DTOs;
using HotelMS.Operations.Exceptions;
using HotelMS.Operations.Models;
using HotelMS.Operations.Repositories.Interfaces;
using HotelMS.Operations.Services.Interfaces;

namespace HotelMS.Operations.Services.Classes;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(bool activeOnly = false)
    {
        var list = await _repo.GetAllAsync(activeOnly);
        return list.Select(MapToDto);
    }

    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        var d = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Department with ID {id} not found.");
        return MapToDto(d);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var existing = await _repo.GetByNameAsync(dto.DepartmentName);
        if (existing != null)
            throw new ConflictException($"Department '{dto.DepartmentName}' already exists.");

        var dept = new Department
        {
            DepartmentName = dto.DepartmentName,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        var created = await _repo.CreateAsync(dept);
        return MapToDto(created);
    }

    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var dept = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Department with ID {id} not found.");

        var existing = await _repo.GetByNameAsync(dto.DepartmentName);
        if (existing != null && existing.DepartmentId != id)
            throw new ConflictException($"Department '{dto.DepartmentName}' already exists.");

        dept.DepartmentName = dto.DepartmentName;
        dept.Description = dto.Description;
        dept.IsActive = dto.IsActive;

        var updated = await _repo.UpdateAsync(dept);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dept = await _repo.GetByIdAsync(id) ?? throw new NotFoundException($"Department with ID {id} not found.");

        var hasStaff = await _repo.HasStaffAsync(id);
        if (hasStaff)
            throw new ConflictException("Cannot delete department because staff members are assigned to it.");

        return await _repo.DeleteAsync(id);
    }

    private static DepartmentDto MapToDto(Department d) => new(
        d.DepartmentId,
        d.DepartmentName,
        d.Description,
        d.IsActive,
        d.Staff?.Count ?? 0
    );
}

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepo;
    private readonly IDepartmentRepository _deptRepo;

    public StaffService(IStaffRepository staffRepo, IDepartmentRepository deptRepo)
    {
        _staffRepo = staffRepo;
        _deptRepo = deptRepo;
    }

    public async Task<IEnumerable<StaffDto>> GetAllAsync()
    {
        var list = await _staffRepo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<StaffDto> GetByIdAsync(int id)
    {
        var s = await _staffRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Staff member with ID {id} not found.");
        return MapToDto(s);
    }

    public async Task<StaffDto> CreateAsync(CreateStaffDto dto)
    {
        var dept = await _deptRepo.GetByIdAsync(dto.DepartmentId) ?? throw new NotFoundException($"Department with ID {dto.DepartmentId} not found.");
        if (!dept.IsActive)
            throw new BadRequestException("Cannot assign staff to an inactive department.");

        var existingNic = await _staffRepo.GetByNicAsync(dto.NIC);
        if (existingNic != null)
            throw new ConflictException($"Staff member with NIC '{dto.NIC}' already exists.");

        var code = await _staffRepo.GenerateStaffCodeAsync();

        var staff = new Staff
        {
            StaffCode = code,
            DepartmentId = dto.DepartmentId,
            EmployeeName = dto.EmployeeName,
            Address = dto.Address,
            NIC = dto.NIC,
            Salary = dto.Salary,
            Age = dto.Age,
            Occupation = dto.Occupation,
            Email = dto.Email
        };

        var created = await _staffRepo.CreateAsync(staff);
        created.Department = dept;
        return MapToDto(created);
    }

    public async Task<StaffDto> UpdateAsync(int id, UpdateStaffDto dto)
    {
        var staff = await _staffRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Staff member with ID {id} not found.");

        var dept = await _deptRepo.GetByIdAsync(dto.DepartmentId) ?? throw new NotFoundException($"Department with ID {dto.DepartmentId} not found.");
        if (!dept.IsActive)
            throw new BadRequestException("Cannot assign staff to an inactive department.");

        var existingNic = await _staffRepo.GetByNicAsync(dto.NIC, id);
        if (existingNic != null)
            throw new ConflictException($"Staff member with NIC '{dto.NIC}' already exists.");

        staff.DepartmentId = dto.DepartmentId;
        staff.EmployeeName = dto.EmployeeName;
        staff.Address = dto.Address;
        staff.NIC = dto.NIC;
        staff.Salary = dto.Salary;
        staff.Age = dto.Age;
        staff.Occupation = dto.Occupation;
        staff.Email = dto.Email;

        var updated = await _staffRepo.UpdateAsync(staff);
        updated.Department = dept;
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _staffRepo.GetByIdAsync(id);
        if (exists == null)
            throw new NotFoundException($"Staff member with ID {id} not found.");

        return await _staffRepo.DeleteAsync(id);
    }

    private static StaffDto MapToDto(Staff s) => new(
        s.StaffId,
        s.StaffCode,
        s.DepartmentId,
        s.Department?.DepartmentName ?? string.Empty,
        s.EmployeeName,
        s.Address,
        s.NIC,
        s.Salary,
        s.Age,
        s.Occupation,
        s.Email
    );
}

public class ReportService : IReportService
{
    private readonly IStaffRepository _staffRepo;
    private readonly IDepartmentRepository _deptRepo;
    private readonly IReservationClient _reservationClient;

    public ReportService(
        IStaffRepository staffRepo,
        IDepartmentRepository deptRepo,
        IReservationClient reservationClient)
    {
        _staffRepo = staffRepo;
        _deptRepo = deptRepo;
        _reservationClient = reservationClient;
    }

    public async Task<StaffSalaryReportDto> GetStaffSalaryReportAsync()
    {
        var departments = await _deptRepo.GetAllAsync();
        var staffMembers = await _staffRepo.GetAllAsync();

        var deptSummaries = departments.Select(d =>
        {
            var deptsStaff = staffMembers.Where(s => s.DepartmentId == d.DepartmentId).ToList();
            return new DepartmentSalarySummaryDto(
                d.DepartmentId,
                d.DepartmentName,
                deptsStaff.Count,
                deptsStaff.Sum(s => s.Salary)
            );
        }).ToList();

        return new StaffSalaryReportDto(
            staffMembers.Count(),
            staffMembers.Sum(s => s.Salary),
            deptSummaries
        );
    }

    public async Task<HotelIncomeReportDto> GetHotelIncomeReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var payments = await _reservationClient.GetPaymentsAsync();
        var invoices = await _reservationClient.GetInvoicesAsync();

        var successfulPayments = payments
            .Where(p => string.Equals(p.PaymentStatus, "Successful", StringComparison.OrdinalIgnoreCase));

        if (fromDate.HasValue)
            successfulPayments = successfulPayments.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue)
            successfulPayments = successfulPayments.Where(p => p.PaymentDate <= toDate.Value);

        var list = successfulPayments.ToList();
        var totalRevenue = list.Sum(p => p.Amount);

        var paymentMethods = list
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var pendingInvoices = invoices.Count(i => string.Equals(i.InvoiceStatus, "Pending", StringComparison.OrdinalIgnoreCase));
        var paidInvoices = invoices.Count(i => string.Equals(i.InvoiceStatus, "Paid", StringComparison.OrdinalIgnoreCase));

        return new HotelIncomeReportDto(
            DateTime.UtcNow,
            totalRevenue,
            list.Count,
            paymentMethods,
            pendingInvoices,
            paidInvoices
        );
    }
}
