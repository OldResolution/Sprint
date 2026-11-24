namespace HotelMS.HotelManagement.DTOs;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = "Single";
    public int Capacity { get; set; } = 1;
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
}

public class UpdateRoomDto
{
    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public int? Capacity { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
}

public class RoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal CurrentPrice { get; set; }
}

public class CreateRoomRateDto
{
    public int RoomId { get; set; }
    public decimal BaseRate { get; set; }
    public decimal SurgeRate { get; set; } = 0.00m;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public class UpdateRoomRateDto
{
    public decimal? BaseRate { get; set; }
    public decimal? SurgeRate { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public class RoomRateDto
{
    public int RateId { get; set; }
    public int RoomId { get; set; }
    public decimal BaseRate { get; set; }
    public decimal SurgeRate { get; set; }
    public decimal TotalRate => BaseRate + SurgeRate;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public class CreateRoomServiceItemDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class UpdateRoomServiceItemDto
{
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
}

public class RoomServiceItemDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateInventoryItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public string Unit { get; set; } = "pcs";
    public int ReorderLevel { get; set; } = 0;
}

public class UpdateInventoryItemDto
{
    public string? ItemName { get; set; }
    public string? Category { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? ReorderLevel { get; set; }
}

public class InventoryItemDto
{
    public int InventoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public DateTime LastUpdated { get; set; }
}
