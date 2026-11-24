using HotelMS.Reservations.Models;
using HotelMS.Reservations.Models.Enums;

namespace HotelMS.Reservations.Data;

public static class DbInitializer
{
    public static void Initialize(ReservationDbContext context)
    {
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            context.Database.EnsureCreated();
        }
        else
        {
            context.Database.EnsureCreated();
        }

        if (context.Guests.Any()) return;

        var guest1 = new Guest
        {
            GuestName = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "1234567890",
            Address = "123 Main St, New York, NY",
            IdentificationNumber = "ID-JD-1001"
        };

        var guest2 = new Guest
        {
            GuestName = "Jane Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "9876543210",
            Address = "456 Oak Ave, Los Angeles, CA",
            IdentificationNumber = "ID-JS-2002"
        };

        context.Guests.AddRange(guest1, guest2);
        context.SaveChanges();
    }
}
