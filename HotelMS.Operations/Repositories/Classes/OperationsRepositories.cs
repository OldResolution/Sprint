using HotelMS.Operations.Data;
using HotelMS.Operations.Models;
using HotelMS.Operations.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Operations.Repositories.Classes;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly OperationsDbContext _context;

    public DepartmentRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync(bool activeOnly = false)
    {
        var query = _context.Departments.Include(d => d.Staff).AsNoTracking();
        if (activeOnly)
            query = query.Where(d => d.IsActive);
        return await query.ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments.Include(d => d.Staff).FirstOrDefaultAsync(d => d.DepartmentId == id);
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == name.ToLower());
    }

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department> UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<bool> HasStaffAsync(int departmentId)
    {
        return await _context.Staff.AnyAsync(s => s.DepartmentId == departmentId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return false;
        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class StaffRepository : IStaffRepository
{
    private readonly OperationsDbContext _context;

    public StaffRepository(OperationsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _context.Staff.Include(s => s.Department).AsNoTracking().ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _context.Staff.Include(s => s.Department).FirstOrDefaultAsync(s => s.StaffId == id);
    }

    public async Task<Staff?> GetByCodeAsync(string code)
    {
        return await _context.Staff.Include(s => s.Department).FirstOrDefaultAsync(s => s.StaffCode == code);
    }

    public async Task<Staff?> GetByNicAsync(string nic, int? excludeId = null)
    {
        return await _context.Staff.FirstOrDefaultAsync(s => s.NIC == nic && (excludeId == null || s.StaffId != excludeId));
    }

    public async Task<IEnumerable<Staff>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _context.Staff.Where(s => s.DepartmentId == departmentId).AsNoTracking().ToListAsync();
    }

    public async Task<Staff> CreateAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    public async Task<Staff> UpdateAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null) return false;
        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateStaffCodeAsync()
    {
        var maxId = await _context.Staff.AnyAsync() ? await _context.Staff.MaxAsync(s => s.StaffId) : 0;
        return $"STF{1001 + maxId}";
    }
}
