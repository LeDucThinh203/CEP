using CEP.Backend.Helpers;
using CEP.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CEP.Backend.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Ensure database created or migrations applied
        await context.Database.MigrateAsync();

        // Seed Admin user if not exists
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // Seed some sample customers if empty for convenient testing
        if (!await context.Customers.AnyAsync())
        {
            var sampleCustomers = new List<Customer>
            {
                new Customer
                {
                    CustomerCode = "CUST001",
                    FullName = "Nguyễn Văn An",
                    Email = "an.nguyen@example.com",
                    PhoneNumber = "0901234567",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    CustomerCode = "CUST002",
                    FullName = "Trần Thị Bích",
                    Email = "bich.tran@example.com",
                    PhoneNumber = "0912345678",
                    DateOfBirth = new DateTime(1993, 5, 20),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    CustomerCode = "CUST003",
                    FullName = "Lê Hoàng Cường",
                    Email = "cuong.le@example.com",
                    PhoneNumber = "0987654321",
                    DateOfBirth = new DateTime(1988, 11, 30),
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Customers.AddRangeAsync(sampleCustomers);
            await context.SaveChangesAsync();
        }
    }
}
