using HotelMS.HotelManagement.Data;
using HotelMS.HotelManagement.Models;
using HotelMS.HotelManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.HotelManagement.Repositories.Classes;

public class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _context;
    public RoomRepository(HotelDbContext context) => _context = context;

    public async Task<IEnumerable<Room>> GetAllAsync(bool activeOnly = false) =>
        await _context.Rooms
            .Include(r => r.RoomRates)
            .Where(r => !activeOnly || r.IsActive)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Room?> GetByIdAsync(int id) =>
        await _context.Rooms.Include(r => r.RoomRates).FirstOrDefaultAsync(r => r.RoomId == id);

    public async Task<Room?> GetByNumberAsync(string roomNumber) =>
        await _context.Rooms.Include(r => r.RoomRates).FirstOrDefaultAsync(r => r.RoomNumber.ToLower() == roomNumber.ToLower());

    public async Task<bool> ExistsByNumberAsync(string roomNumber, int? excludeId = null) =>
        await _context.Rooms.AnyAsync(r => r.RoomNumber.ToLower() == roomNumber.ToLower() && (!excludeId.HasValue || r.RoomId != excludeId.Value));

    public async Task<Room> CreateAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return false;
        room.IsActive = false; // Soft delete per spec
        await _context.SaveChangesAsync();
        return true;
    }
}

public class RateRepository : IRateRepository
{
    private readonly HotelDbContext _context;
    public RateRepository(HotelDbContext context) => _context = context;

    public async Task<IEnumerable<RoomRate>> GetAllAsync() =>
        await _context.RoomRates.AsNoTracking().ToListAsync();

    public async Task<RoomRate?> GetByIdAsync(int id) =>
        await _context.RoomRates.FindAsync(id);

    public async Task<IEnumerable<RoomRate>> GetByRoomIdAsync(int roomId) =>
        await _context.RoomRates.Where(r => r.RoomId == roomId).AsNoTracking().ToListAsync();

    public async Task<RoomRate?> GetEffectiveRateAsync(int roomId, DateOnly date) =>
        await _context.RoomRates
            .Where(r => r.RoomId == roomId && r.EffectiveFrom <= date && (r.EffectiveTo == null || r.EffectiveTo >= date))
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync();

    public async Task<RoomRate> CreateAsync(RoomRate rate)
    {
        _context.RoomRates.Add(rate);
        await _context.SaveChangesAsync();
        return rate;
    }

    public async Task<RoomRate> UpdateAsync(RoomRate rate)
    {
        _context.RoomRates.Update(rate);
        await _context.SaveChangesAsync();
        return rate;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rate = await _context.RoomRates.FindAsync(id);
        if (rate == null) return false;
        _context.RoomRates.Remove(rate);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ServiceCatalogRepository : IServiceCatalogRepository
{
    private readonly HotelDbContext _context;
    public ServiceCatalogRepository(HotelDbContext context) => _context = context;

    public async Task<IEnumerable<RoomServiceItem>> GetAllAsync(bool availableOnly = false) =>
        await _context.RoomServices.Where(s => !availableOnly || s.IsAvailable).AsNoTracking().ToListAsync();

    public async Task<RoomServiceItem?> GetByIdAsync(int id) =>
        await _context.RoomServices.FindAsync(id);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null) =>
        await _context.RoomServices.AnyAsync(s => s.ServiceName.ToLower() == name.ToLower() && (!excludeId.HasValue || s.ServiceId != excludeId.Value));

    public async Task<RoomServiceItem> CreateAsync(RoomServiceItem item)
    {
        _context.RoomServices.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<RoomServiceItem> UpdateAsync(RoomServiceItem item)
    {
        _context.RoomServices.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var item = await _context.RoomServices.FindAsync(id);
        if (item == null) return false;
        item.IsAvailable = false; // Soft delete per spec
        await _context.SaveChangesAsync();
        return true;
    }
}

public class InventoryRepository : IInventoryRepository
{
    private readonly HotelDbContext _context;
    public InventoryRepository(HotelDbContext context) => _context = context;

    public async Task<IEnumerable<InventoryItem>> GetAllAsync() =>
        await _context.Inventory.AsNoTracking().ToListAsync();

    public async Task<InventoryItem?> GetByIdAsync(int id) =>
        await _context.Inventory.FindAsync(id);

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        _context.Inventory.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(InventoryItem item)
    {
        _context.Inventory.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.Inventory.FindAsync(id);
        if (item == null) return false;
        _context.Inventory.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
