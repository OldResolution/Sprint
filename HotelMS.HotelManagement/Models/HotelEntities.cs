using HotelMS.HotelManagement.Models.Enums;

namespace HotelMS.HotelManagement.Models;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType RoomType { get; set; } = RoomType.Single;
    public int Capacity { get; set; } = 1;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public bool IsActive { get; set; } = true; // Soft delete

    public ICollection<RoomRate> RoomRates { get; set; } = new List<RoomRate>();
}

public class RoomRate
{
    public int RateId { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public decimal BaseRate { get; set; }
    public decimal SurgeRate { get; set; } = 0.00m;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public class RoomServiceItem
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true; // Soft delete
}

public class InventoryItem
{
    public int InventoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public string Unit { get; set; } = "pcs";
    public int ReorderLevel { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
