using HotelMS.HotelManagement.DTOs;
using HotelMS.HotelManagement.Exceptions;
using HotelMS.HotelManagement.Models;
using HotelMS.HotelManagement.Models.Enums;
using HotelMS.HotelManagement.Repositories.Interfaces;
using HotelMS.HotelManagement.Services.Interfaces;

namespace HotelMS.HotelManagement.Services.Classes;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;

    public RoomService(IRoomRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<RoomDto>> GetAllAsync(bool activeOnly = false)
    {
        var rooms = await _repo.GetAllAsync(activeOnly);
        return rooms.Select(MapRoomToDto);
    }

    public async Task<RoomDto> GetByIdAsync(int id)
    {
        var room = await _repo.GetByIdAsync(id);
        if (room == null) throw new NotFoundException($"Room with ID {id} was not found.");
        return MapRoomToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var exists = await _repo.ExistsByNumberAsync(dto.RoomNumber.Trim());
        if (exists) throw new ConflictException($"Room number '{dto.RoomNumber}' already exists.");

        if (!Enum.TryParse<RoomType>(dto.RoomType, true, out var rType))
            throw new ValidationException("Invalid RoomType. Allowed: Single, Double, Deluxe, Suite, Hall.");

        if (!Enum.TryParse<RoomStatus>(dto.Status, true, out var rStatus))
            throw new ValidationException("Invalid Status. Allowed: Available, Occupied, Maintenance.");

        var room = new Room
        {
            RoomNumber = dto.RoomNumber.Trim(),
            RoomType = rType,
            Capacity = dto.Capacity,
            Status = rStatus,
            IsActive = dto.IsActive
        };

        var created = await _repo.CreateAsync(room);
        return MapRoomToDto(created);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _repo.GetByIdAsync(id);
        if (room == null) throw new NotFoundException($"Room with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
        {
            var exists = await _repo.ExistsByNumberAsync(dto.RoomNumber.Trim(), id);
            if (exists) throw new ConflictException($"Room number '{dto.RoomNumber}' already exists.");
            room.RoomNumber = dto.RoomNumber.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.RoomType) && Enum.TryParse<RoomType>(dto.RoomType, true, out var rType))
            room.RoomType = rType;

        if (dto.Capacity.HasValue && dto.Capacity.Value > 0)
            room.Capacity = dto.Capacity.Value;

        if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<RoomStatus>(dto.Status, true, out var rStatus))
            room.Status = rStatus;

        if (dto.IsActive.HasValue)
            room.IsActive = dto.IsActive.Value;

        var updated = await _repo.UpdateAsync(room);
        return MapRoomToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var room = await _repo.GetByIdAsync(id);
        if (room == null) throw new NotFoundException($"Room with ID {id} was not found.");
        return await _repo.SoftDeleteAsync(id); // Soft delete per spec
    }

    private static RoomDto MapRoomToDto(Room r)
    {
        var rate = r.RoomRates?.FirstOrDefault();
        return new RoomDto
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomType = r.RoomType.ToString(),
            Capacity = r.Capacity,
            Status = r.Status.ToString(),
            IsActive = r.IsActive,
            CurrentPrice = rate != null ? rate.BaseRate + rate.SurgeRate : 3500.00m
        };
    }
}

public class RateService : IRateService
{
    private readonly IRateRepository _repo;
    private readonly IRoomRepository _roomRepo;

    public RateService(IRateRepository repo, IRoomRepository roomRepo)
    {
        _repo = repo;
        _roomRepo = roomRepo;
    }

    public async Task<IEnumerable<RoomRateDto>> GetAllAsync()
    {
        var rates = await _repo.GetAllAsync();
        return rates.Select(MapRateToDto);
    }

    public async Task<RoomRateDto> GetByIdAsync(int id)
    {
        var rate = await _repo.GetByIdAsync(id);
        if (rate == null) throw new NotFoundException($"Rate with ID {id} was not found.");
        return MapRateToDto(rate);
    }

    public async Task<RoomRateDto> CreateAsync(CreateRoomRateDto dto)
    {
        var room = await _roomRepo.GetByIdAsync(dto.RoomId);
        if (room == null) throw new NotFoundException($"Room with ID {dto.RoomId} was not found.");

        var rate = new RoomRate
        {
            RoomId = dto.RoomId,
            BaseRate = dto.BaseRate,
            SurgeRate = dto.SurgeRate,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo
        };

        var created = await _repo.CreateAsync(rate);
        return MapRateToDto(created);
    }

    public async Task<RoomRateDto> UpdateAsync(int id, UpdateRoomRateDto dto)
    {
        var rate = await _repo.GetByIdAsync(id);
        if (rate == null) throw new NotFoundException($"Rate with ID {id} was not found.");

        if (dto.BaseRate.HasValue && dto.BaseRate.Value > 0) rate.BaseRate = dto.BaseRate.Value;
        if (dto.SurgeRate.HasValue && dto.SurgeRate.Value >= 0) rate.SurgeRate = dto.SurgeRate.Value;
        if (dto.EffectiveFrom.HasValue) rate.EffectiveFrom = dto.EffectiveFrom.Value;
        if (dto.EffectiveTo.HasValue) rate.EffectiveTo = dto.EffectiveTo.Value;

        var updated = await _repo.UpdateAsync(rate);
        return MapRateToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rate = await _repo.GetByIdAsync(id);
        if (rate == null) throw new NotFoundException($"Rate with ID {id} was not found.");
        return await _repo.DeleteAsync(id); // Hard delete per spec
    }

    public async Task<decimal> GetEffectiveRateAsync(int roomId, DateOnly date)
    {
        var rate = await _repo.GetEffectiveRateAsync(roomId, date);
        if (rate != null)
        {
            return rate.BaseRate + rate.SurgeRate; // BaseRate + SurgeRate
        }
        return 3500.00m;
    }

    private static RoomRateDto MapRateToDto(RoomRate r) => new()
    {
        RateId = r.RateId,
        RoomId = r.RoomId,
        BaseRate = r.BaseRate,
        SurgeRate = r.SurgeRate,
        EffectiveFrom = r.EffectiveFrom,
        EffectiveTo = r.EffectiveTo
    };
}

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceCatalogRepository _repo;

    public ServiceCatalogService(IServiceCatalogRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<RoomServiceItemDto>> GetAllAsync(bool availableOnly = false)
    {
        var items = await _repo.GetAllAsync(availableOnly);
        return items.Select(MapServiceToDto);
    }

    public async Task<RoomServiceItemDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Room service item with ID {id} was not found.");
        return MapServiceToDto(item);
    }

    public async Task<RoomServiceItemDto> CreateAsync(CreateRoomServiceItemDto dto)
    {
        var exists = await _repo.ExistsByNameAsync(dto.ServiceName.Trim());
        if (exists) throw new ConflictException($"Room service '{dto.ServiceName}' already exists.");

        var item = new RoomServiceItem
        {
            ServiceName = dto.ServiceName.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            IsAvailable = dto.IsAvailable
        };

        var created = await _repo.CreateAsync(item);
        return MapServiceToDto(created);
    }

    public async Task<RoomServiceItemDto> UpdateAsync(int id, UpdateRoomServiceItemDto dto)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Room service item with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.ServiceName))
        {
            var exists = await _repo.ExistsByNameAsync(dto.ServiceName.Trim(), id);
            if (exists) throw new ConflictException($"Room service '{dto.ServiceName}' already exists.");
            item.ServiceName = dto.ServiceName.Trim();
        }

        if (dto.Description != null) item.Description = dto.Description.Trim();
        if (dto.Price.HasValue && dto.Price.Value >= 0) item.Price = dto.Price.Value;
        if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;

        var updated = await _repo.UpdateAsync(item);
        return MapServiceToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Room service item with ID {id} was not found.");
        return await _repo.SoftDeleteAsync(id); // Soft delete: IsAvailable = false
    }

    private static RoomServiceItemDto MapServiceToDto(RoomServiceItem s) => new()
    {
        ServiceId = s.ServiceId,
        ServiceName = s.ServiceName,
        Description = s.Description,
        Price = s.Price,
        IsAvailable = s.IsAvailable
    };
}

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;

    public InventoryService(IInventoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<InventoryItemDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(MapInventoryToDto);
    }

    public async Task<InventoryItemDto> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Inventory item with ID {id} was not found.");
        return MapInventoryToDto(item);
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto)
    {
        var item = new InventoryItem
        {
            ItemName = dto.ItemName.Trim(),
            Category = dto.Category.Trim(),
            Quantity = dto.Quantity,
            Unit = dto.Unit.Trim(),
            ReorderLevel = dto.ReorderLevel,
            LastUpdated = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(item);
        return MapInventoryToDto(created);
    }

    public async Task<InventoryItemDto> UpdateAsync(int id, UpdateInventoryItemDto dto)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Inventory item with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.ItemName)) item.ItemName = dto.ItemName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Category)) item.Category = dto.Category.Trim();
        if (dto.Quantity.HasValue && dto.Quantity.Value >= 0) item.Quantity = dto.Quantity.Value;
        if (!string.IsNullOrWhiteSpace(dto.Unit)) item.Unit = dto.Unit.Trim();
        if (dto.ReorderLevel.HasValue && dto.ReorderLevel.Value >= 0) item.ReorderLevel = dto.ReorderLevel.Value;

        item.LastUpdated = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(item);
        return MapInventoryToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) throw new NotFoundException($"Inventory item with ID {id} was not found.");
        return await _repo.DeleteAsync(id); // Hard delete per spec
    }

    private static InventoryItemDto MapInventoryToDto(InventoryItem i) => new()
    {
        InventoryId = i.InventoryId,
        ItemName = i.ItemName,
        Category = i.Category,
        Quantity = i.Quantity,
        Unit = i.Unit,
        ReorderLevel = i.ReorderLevel,
        LastUpdated = i.LastUpdated
    };
}
