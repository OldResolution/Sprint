using HotelMS.HotelManagement.Models;
using HotelMS.HotelManagement.Models.Enums;

namespace HotelMS.HotelManagement.Data;

public static class DbInitializer
{
    public static void Initialize(HotelDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Rooms.Any())
        {
            var room1 = new Room { RoomNumber = "101", RoomType = RoomType.Deluxe, Capacity = 2, Status = RoomStatus.Available, IsActive = true };
            var room2 = new Room { RoomNumber = "102", RoomType = RoomType.Single, Capacity = 1, Status = RoomStatus.Available, IsActive = true };
            var room3 = new Room { RoomNumber = "103", RoomType = RoomType.Suite, Capacity = 4, Status = RoomStatus.Available, IsActive = true };
            var hall1 = new Room { RoomNumber = "HALL-A", RoomType = RoomType.Hall, Capacity = 100, Status = RoomStatus.Available, IsActive = true };

            context.Rooms.AddRange(room1, room2, room3, hall1);
            context.SaveChanges();

            context.RoomRates.AddRange(
                new RoomRate
                {
                    RoomId = room1.RoomId,
                    BaseRate = 3500.00m,
                    SurgeRate = 500.00m,
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EffectiveTo = null
                },
                new RoomRate
                {
                    RoomId = room2.RoomId,
                    BaseRate = 1800.00m,
                    SurgeRate = 200.00m,
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EffectiveTo = null
                },
                new RoomRate
                {
                    RoomId = room3.RoomId,
                    BaseRate = 6000.00m,
                    SurgeRate = 1000.00m,
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EffectiveTo = null
                },
                new RoomRate
                {
                    RoomId = hall1.RoomId,
                    BaseRate = 25000.00m,
                    SurgeRate = 5000.00m,
                    EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EffectiveTo = null
                }
            );

            context.RoomServices.AddRange(
                new RoomServiceItem { ServiceName = "Extra Pillows", Description = "Set of 2 soft pillows", Price = 100.00m, IsAvailable = true },
                new RoomServiceItem { ServiceName = "Bedroll Change", Description = "Fresh linens and bedsheet replacement", Price = 150.00m, IsAvailable = true },
                new RoomServiceItem { ServiceName = "Toiletries Kit", Description = "Toothbrush, shampoo, moisturizer and soap", Price = 200.00m, IsAvailable = true }
            );

            context.Inventory.AddRange(
                new InventoryItem { ItemName = "Bedsheets", Category = "Housekeeping", Quantity = 120, Unit = "pcs", ReorderLevel = 30, LastUpdated = DateTime.UtcNow },
                new InventoryItem { ItemName = "Towels", Category = "Housekeeping", Quantity = 200, Unit = "pcs", ReorderLevel = 50, LastUpdated = DateTime.UtcNow }
            );

            context.SaveChanges();
        }
    }
}
