using System.Reflection;
using HotelMS.Reservations.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelMS.Reservations.Data;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options) { }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationServiceItem> ReservationServiceItems => Set<ReservationServiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Sequence for ReservationCode in SQL Server
        if (Database.IsSqlServer())
        {
            modelBuilder.HasSequence<int>("ReservationCodeSeq").StartsAt(1001).IncrementsBy(1);
        }
    }
}
