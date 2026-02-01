using EasyStore.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyStore.Data.Seed;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@EasyStore.com"))
        {
            return;
        }

        PasswordHasher<User> hasher = new PasswordHasher<User>();

        User admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@EasyStore.com",
            Names = "Admin",
            Phone = "0872123199",
            Role = "Admin",
            PasswordHash = ""
        };

        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
