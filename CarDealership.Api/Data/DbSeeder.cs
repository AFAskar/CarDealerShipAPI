using CarDealership.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace CarDealership.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        context.Database.EnsureCreated();

        // Seed Admin User
        var adminEmail = "admin@dealership.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Role = "Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
        }

        // Seed Vehicles
        if (!context.Vehicles.Any())
        {
            var vehicles = new List<Vehicle>
            {
                new() { Make = "Toyota", Model = "Camry", Year = 2022, Price = 25000, IsAvailable = true },
                new() { Make = "Honda", Model = "Civic", Year = 2023, Price = 27000, IsAvailable = true },
                new() { Make = "Ford", Model = "Mustang", Year = 2021, Price = 35000, IsAvailable = true },
                new() { Make = "Chevrolet", Model = "Malibu", Year = 2020, Price = 22000, IsAvailable = true },
                new() { Make = "Tesla", Model = "Model 3", Year = 2023, Price = 45000, IsAvailable = true },
                new() { Make = "BMW", Model = "3 Series", Year = 2022, Price = 42000, IsAvailable = true },
                new() { Make = "Audi", Model = "A4", Year = 2023, Price = 44000, IsAvailable = true },
                new() { Make = "Mercedes-Benz", Model = "C-Class", Year = 2022, Price = 46000, IsAvailable = true },
                new() { Make = "Nissan", Model = "Altima", Year = 2021, Price = 24000, IsAvailable = true },
                new() { Make = "Hyundai", Model = "Sonata", Year = 2022, Price = 26000, IsAvailable = true }
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }
    }
}
