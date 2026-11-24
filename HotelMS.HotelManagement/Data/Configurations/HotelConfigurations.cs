using HotelMS.HotelManagement.Models;
using HotelMS.HotelManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelMS.HotelManagement.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.RoomId);
        builder.Property(r => r.RoomNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(r => r.RoomNumber).IsUnique();

        builder.Property(r => r.RoomType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(r => r.RoomRates)
            .WithOne(rate => rate.Room)
            .HasForeignKey(rate => rate.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomRateConfiguration : IEntityTypeConfiguration<RoomRate>
{
    public void Configure(EntityTypeBuilder<RoomRate> builder)
    {
        builder.ToTable("RoomRates");
        builder.HasKey(r => r.RateId);
        builder.Property(r => r.BaseRate).HasPrecision(10, 2).IsRequired();
        builder.Property(r => r.SurgeRate).HasPrecision(10, 2).HasDefaultValue(0.00m);
        builder.Property(r => r.EffectiveFrom).IsRequired();

        builder.HasIndex(r => new { r.RoomId, r.EffectiveFrom }).IsUnique();
    }
}

public class RoomServiceItemConfiguration : IEntityTypeConfiguration<RoomServiceItem>
{
    public void Configure(EntityTypeBuilder<RoomServiceItem> builder)
    {
        builder.ToTable("RoomServices");
        builder.HasKey(s => s.ServiceId);
        builder.Property(s => s.ServiceName).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.ServiceName).IsUnique();
        builder.Property(s => s.Description).HasMaxLength(250);
        builder.Property(s => s.Price).HasPrecision(10, 2).IsRequired();
        builder.Property(s => s.IsAvailable).IsRequired().HasDefaultValue(true);
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("Inventory");
        builder.HasKey(i => i.InventoryId);
        builder.Property(i => i.ItemName).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Category).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Quantity).IsRequired().HasDefaultValue(0);
        builder.Property(i => i.ReorderLevel).IsRequired().HasDefaultValue(0);
        builder.Property(i => i.LastUpdated).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
