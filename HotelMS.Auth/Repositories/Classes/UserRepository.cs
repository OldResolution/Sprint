using HotelMS.Auth.Data;
using HotelMS.Auth.Models;
using HotelMS.Auth.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Auth.Repositories.Classes;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.AsNoTracking().ToListAsync();

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null) =>
        await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower() && (!excludeId.HasValue || u.UserId != excludeId.Value));

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
