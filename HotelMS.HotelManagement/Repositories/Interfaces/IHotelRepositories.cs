using HotelMS.HotelManagement.Models;

namespace HotelMS.HotelManagement.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAllAsync(bool activeOnly = false);
    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetByNumberAsync(string roomNumber);
    Task<bool> ExistsByNumberAsync(string roomNumber, int? excludeId = null);
    Task<Room> CreateAsync(Room room);
    Task<Room> UpdateAsync(Room room);
    Task<bool> SoftDeleteAsync(int id);
}

public interface IRateRepository
{
    Task<IEnumerable<RoomRate>> GetAllAsync();
    Task<RoomRate?> GetByIdAsync(int id);
    Task<IEnumerable<RoomRate>> GetByRoomIdAsync(int roomId);
    Task<RoomRate?> GetEffectiveRateAsync(int roomId, DateOnly date);
    Task<RoomRate> CreateAsync(RoomRate rate);
    Task<RoomRate> UpdateAsync(RoomRate rate);
    Task<bool> DeleteAsync(int id);
}

public interface IServiceCatalogRepository
{
    Task<IEnumerable<RoomServiceItem>> GetAllAsync(bool availableOnly = false);
    Task<RoomServiceItem?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<RoomServiceItem> CreateAsync(RoomServiceItem item);
    Task<RoomServiceItem> UpdateAsync(RoomServiceItem item);
    Task<bool> SoftDeleteAsync(int id);
}

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task<InventoryItem> UpdateAsync(InventoryItem item);
    Task<bool> DeleteAsync(int id);
}
