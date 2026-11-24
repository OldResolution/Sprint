using HotelMS.Reservations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelMS.Reservations.Data.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");
        builder.HasKey(g => g.GuestId);

        builder.Property(g => g.GuestName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(g => g.Address)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(g => g.IdentificationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(g => g.IdentificationNumber).IsUnique();
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.ReservationId);

        builder.Property(r => r.ReservationCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.ReservationCode).IsUnique();

        builder.Property(r => r.RoomId)
            .IsRequired();

        builder.Property(r => r.CheckInDateTime)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(r => r.CheckOutDateTime)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(r => r.NumberOfGuests)
            .IsRequired();

        builder.Property(r => r.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(r => r.ReservationStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(r => r.Guest)
            .WithMany(g => g.Reservations)
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.RoomId, r.CheckInDateTime, r.CheckOutDateTime });
    }
}

public class ReservationServiceItemConfiguration : IEntityTypeConfiguration<ReservationServiceItem>
{
    public void Configure(EntityTypeBuilder<ReservationServiceItem> builder)
    {
        builder.ToTable("ReservationServiceItems");
        builder.HasKey(rsi => rsi.ReservationServiceItemId);

        builder.Property(rsi => rsi.ServiceId)
            .IsRequired();

        builder.Property(rsi => rsi.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(rsi => rsi.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(rsi => rsi.TotalPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.HasOne(rsi => rsi.Reservation)
            .WithMany(r => r.ReservationServiceItems)
            .HasForeignKey(rsi => rsi.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.PaymentStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.PaymentDate)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TransactionReference)
            .IsUnique()
            .HasFilter("[TransactionReference] IS NOT NULL");
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.InvoiceId);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.IssueDate)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(i => i.SubTotal)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(i => i.TaxAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(i => i.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(i => i.InvoiceStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasOne(i => i.Reservation)
            .WithOne(r => r.Invoice)
            .HasForeignKey<Invoice>(i => i.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ReservationId).IsUnique();
    }
}
