using HotelMS.HotelManagement.DTOs;

namespace HotelMS.HotelManagement.Services.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllAsync(bool activeOnly = false);
    Task<RoomDto> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IRateService
{
    Task<IEnumerable<RoomRateDto>> GetAllAsync();
    Task<RoomRateDto> GetByIdAsync(int id);
    Task<RoomRateDto> CreateAsync(CreateRoomRateDto dto);
    Task<RoomRateDto> UpdateAsync(int id, UpdateRoomRateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<decimal> GetEffectiveRateAsync(int roomId, DateOnly date);
}

public interface IServiceCatalogService
{
    Task<IEnumerable<RoomServiceItemDto>> GetAllAsync(bool availableOnly = false);
    Task<RoomServiceItemDto> GetByIdAsync(int id);
    Task<RoomServiceItemDto> CreateAsync(CreateRoomServiceItemDto dto);
    Task<RoomServiceItemDto> UpdateAsync(int id, UpdateRoomServiceItemDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IInventoryService
{
    Task<IEnumerable<InventoryItemDto>> GetAllAsync();
    Task<InventoryItemDto> GetByIdAsync(int id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto);
    Task<InventoryItemDto> UpdateAsync(int id, UpdateInventoryItemDto dto);
    Task<bool> DeleteAsync(int id);
}
