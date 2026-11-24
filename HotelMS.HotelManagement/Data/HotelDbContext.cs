using HotelMS.HotelManagement.Data.Configurations;
using HotelMS.HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.HotelManagement.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomRate> RoomRates => Set<RoomRate>();
    public DbSet<RoomServiceItem> RoomServices => Set<RoomServiceItem>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new RoomRateConfiguration());
        modelBuilder.ApplyConfiguration(new RoomServiceItemConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
    }
}
