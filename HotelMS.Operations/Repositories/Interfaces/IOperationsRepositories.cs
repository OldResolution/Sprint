using HotelMS.Operations.Models;

namespace HotelMS.Operations.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync(bool activeOnly = false);
    Task<Department?> GetByIdAsync(int id);
    Task<Department?> GetByNameAsync(string name);
    Task<Department> CreateAsync(Department department);
    Task<Department> UpdateAsync(Department department);
    Task<bool> HasStaffAsync(int departmentId);
    Task<bool> DeleteAsync(int id);
}

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<Staff?> GetByCodeAsync(string code);
    Task<Staff?> GetByNicAsync(string nic, int? excludeId = null);
    Task<IEnumerable<Staff>> GetByDepartmentIdAsync(int departmentId);
    Task<Staff> CreateAsync(Staff staff);
    Task<Staff> UpdateAsync(Staff staff);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateStaffCodeAsync();
}
