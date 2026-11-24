using HotelMS.Auth.Models;
using HotelMS.Auth.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace HotelMS.Auth.Data;

public static class DbInitializer
{
    public static void Initialize(AuthDbContext context, IConfiguration config)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var seedUsername = config["SeedUser:Username"] ?? "admin";
            var seedPassword = config["SeedUser:Password"] ?? "Admin@123";

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = seedUsername,
                Role = UserRole.Owner
            };
            user.PasswordHash = hasher.HashPassword(user, seedPassword);

            context.Users.Add(user);
            context.SaveChanges();
        }
    }
}
