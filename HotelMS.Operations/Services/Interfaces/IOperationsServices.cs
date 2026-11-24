using HotelMS.Operations.DTOs;

namespace HotelMS.Operations.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync(bool activeOnly = false);
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IStaffService
{
    Task<IEnumerable<StaffDto>> GetAllAsync();
    Task<StaffDto> GetByIdAsync(int id);
    Task<StaffDto> CreateAsync(CreateStaffDto dto);
    Task<StaffDto> UpdateAsync(int id, UpdateStaffDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IReportService
{
    Task<StaffSalaryReportDto> GetStaffSalaryReportAsync();
    Task<HotelIncomeReportDto> GetHotelIncomeReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
