using LabUI.Models;
using Microsoft.EntityFrameworkCore;

namespace LabUlApi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Выполнить миграции (создать БД если её нет)
            await context.Database.MigrateAsync();

            // URL API проекта (для формирования путей к изображениям)
            var apiUri = "https://localhost:7002/"; // Порт из launchSettings.json

            // Проверяем, есть ли уже данные в БД
            if (!context.Categories.Any() && !context.Dishes.Any())
            {
                // Добавляем категории
                var categories = new List<Category>
                {
                    new Category { Name = "Стартеры", NormalizedName = "starters" },
                    new Category { Name = "Салаты", NormalizedName = "salads" },
                    new Category { Name = "Основные блюда", NormalizedName = "main-courses" },
                    new Category { Name = "Десерты", NormalizedName = "desserts" },
                    new Category { Name = "Напитки", NormalizedName = "drinks" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Обновляем список категорий с Id из БД
                var savedCategories = await context.Categories.ToListAsync();

                // Добавляем блюда
                var dishes = new List<Dish>
                {
                    new Dish
                    {
                        Name = "Суп-харчо",
                        Description = "Острый и пряный грузинский суп на основе говядины и грецких орехов.",
                        Price = 15.99m,
                        Image = $"{apiUri}Images/soup.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "starters").Id
                    },
                    new Dish
                    {
                        Name = "Борщ",
                        Description = "Густой ароматный свекольный суп, традиционное славянское блюдо.",
                        Price = 12.50m,
                        Image = $"{apiUri}Images/borshch.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "starters").Id
                    },
                    new Dish
                    {
                        Name = "Цезарь",
                        Description = "Классический салат с листьями айсберга, гренками, сыром пармезан.",
                        Price = 18.75m,
                        Image = $"{apiUri}Images/caesar.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "salads").Id
                    },
                    new Dish
                    {
                        Name = "Стейк",
                        Description = "Говяжий стейк средней прожарки",
                        Price = 35.00m,
                        Image = $"{apiUri}Images/steak.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "main-courses").Id
                    },
                    new Dish
                    {
                        Name = "Тирамису",
                        Description = "Итальянский десерт из печенья савоярди, сыра маскарпоне и кофе.",
                        Price = 22.00m,
                        Image = $"{apiUri}Images/tiramisu.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "desserts").Id
                    },
                    new Dish
                    {
                        Name = "Мохито",
                        Description = "Освежающий коктейль с лаймом, мятой и ромом.",
                        Price = 9.99m,
                        Image = $"{apiUri}Images/mojito.jpg",
                        CategoryId = savedCategories.First(c => c.NormalizedName == "drinks").Id
                    }
                };

                await context.Dishes.AddRangeAsync(dishes);
                await context.SaveChangesAsync();

                Console.WriteLine("База данных успешно заполнена тестовыми данными.");
            }
            else
            {
                Console.WriteLine("База данных уже содержит данные. Инициализация пропущена.");
            }
        }
    }
}