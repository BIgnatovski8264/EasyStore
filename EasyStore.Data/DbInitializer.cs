using Microsoft.Extensions.DependencyInjection;
using EasyStore.Data.Seed;
using EasyStore.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyStore.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        AppDbContext db = serviceProvider.GetRequiredService<AppDbContext>();

        await UserSeeder.SeedAsync(db);

        if (Environment.GetEnvironmentVariable("DROP_DB_ON_RUN") == "1")
        {
            Console.WriteLine("--- ИЗЧИСТВАНЕ НА ТАБЛИЦИТЕ ---");
            db.Products.RemoveRange(db.Products);
            db.Categories.RemoveRange(db.Categories);
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            List<Category> categories = new List<Category>
            {
                new Category { Name = "Плодове" },
                new Category { Name = "Зеленчуци" },
                new Category { Name = "Млечни продукти" },
                new Category { Name = "Месни продукти" },
                new Category { Name = "Тестени изделия" }
            };

            await db.Categories.AddRangeAsync(categories);
            await db.SaveChangesAsync();
            Console.WriteLine("--- КАТЕГОРИИТЕ СА ЗАРЕДЕНИ ---");
        }

        if (!await db.Products.AnyAsync())
        {
            List<Category> allCats = await db.Categories.ToListAsync();

            Category? catFruit = allCats.FirstOrDefault(c => c.Name == "Плодове");
            Category? catVeg = allCats.FirstOrDefault(c => c.Name == "Зеленчуци");
            Category? catDairy = allCats.FirstOrDefault(c => c.Name == "Млечни продукти");
            Category? catMeat = allCats.FirstOrDefault(c => c.Name == "Месни продукти");
            Category? catBread = allCats.FirstOrDefault(c => c.Name == "Тестени изделия");

            if (catFruit != null && catVeg != null && catDairy != null && catMeat != null && catBread != null)
            {
                List<Product> products = new List<Product>
                {
                    new Product { Name = "Ябълки (кг)", Price = 2.50m, StockQuantity = 100, CategoryId = catFruit.Id },
                    new Product { Name = "Банани (кг)", Price = 3.20m, StockQuantity = 80, CategoryId = catFruit.Id },
                    new Product { Name = "Портокали (кг)", Price = 2.80m, StockQuantity = 60, CategoryId = catFruit.Id },
                    new Product { Name = "Киви (бр)", Price = 0.90m, StockQuantity = 150, CategoryId = catFruit.Id },
                    new Product { Name = "Лимони (кг)", Price = 3.50m, StockQuantity = 40, CategoryId = catFruit.Id },

                    new Product { Name = "Домати (кг)", Price = 4.50m, StockQuantity = 50, CategoryId = catVeg.Id },
                    new Product { Name = "Краставици (кг)", Price = 3.80m, StockQuantity = 70, CategoryId = catVeg.Id },
                    new Product { Name = "Картофи (кг)", Price = 1.60m, StockQuantity = 200, CategoryId = catVeg.Id },
                    new Product { Name = "Лук (кг)", Price = 1.40m, StockQuantity = 120, CategoryId = catVeg.Id },
                    new Product { Name = "Моркови (кг)", Price = 1.80m, StockQuantity = 90, CategoryId = catVeg.Id },

                    new Product { Name = "Прясно мляко (бр)", Price = 2.90m, StockQuantity = 45, CategoryId = catDairy.Id },
                    new Product { Name = "Кисело мляко (бр)", Price = 1.50m, StockQuantity = 100, CategoryId = catDairy.Id },
                    new Product { Name = "Сирене (кг)", Price = 14.50m, StockQuantity = 25, CategoryId = catDairy.Id },
                    new Product { Name = "Кашкавал (кг)", Price = 18.90m, StockQuantity = 20, CategoryId = catDairy.Id },
                    new Product { Name = "Масло (бр)", Price = 5.60m, StockQuantity = 35, CategoryId = catDairy.Id },

                    new Product { Name = "Пилешко филе (кг)", Price = 12.50m, StockQuantity = 15, CategoryId = catMeat.Id },
                    new Product { Name = "Свински врат (кг)", Price = 15.20m, StockQuantity = 12, CategoryId = catMeat.Id },
                    new Product { Name = "Телешка кайма (кг)", Price = 13.80m, StockQuantity = 18, CategoryId = catMeat.Id },
                    new Product { Name = "Луканка (бр)", Price = 9.50m, StockQuantity = 30, CategoryId = catMeat.Id },
                    new Product { Name = "Кренвирши (кг)", Price = 8.40m, StockQuantity = 40, CategoryId = catMeat.Id },

                    new Product { Name = "Бял хляб (бр)", Price = 1.60m, StockQuantity = 60, CategoryId = catBread.Id },
                    new Product { Name = "Пълнозърнест хляб (бр)", Price = 1.90m, StockQuantity = 40, CategoryId = catBread.Id },
                    new Product { Name = "Баничка (бр)", Price = 2.20m, StockQuantity = 25, CategoryId = catBread.Id },
                    new Product { Name = "Кроасан (бр)", Price = 1.80m, StockQuantity = 50, CategoryId = catBread.Id },
                    new Product { Name = "Геврек (бр)", Price = 1.20m, StockQuantity = 30, CategoryId = catBread.Id }
                };

                await db.Products.AddRangeAsync(products);
                await db.SaveChangesAsync();
                Console.WriteLine("--- ПРОДУКТИТЕ СА ЗАРЕДЕНИ УСПЕШНО! ---");
            }
        }
    }
}
