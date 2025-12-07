using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LabUI.Data
{
    public class DbInit
    {
        public static async Task SeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInit>>();

            try
            {
                // 1. Создаем роль "admin" если ее нет
                if (!await roleManager.RoleExistsAsync("admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("admin"));
                    logger.LogInformation("Роль 'admin' создана.");
                }

                // 2. Проверяем существование администратора
                var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");

                if (adminUser == null)
                {
                    logger.LogInformation("Создание пользователя admin...");

                    // Создаем нового пользователя
                    var user = new AppUser
                    {
                        UserName = "admin", // Важно: UserName без @gmail.com
                        Email = "admin@gmail.com",
                        EmailConfirmed = true
                    };

                    // Создаем пользователя с паролем
                    var createResult = await userManager.CreateAsync(user, "123456");

                    if (createResult.Succeeded)
                    {
                        logger.LogInformation("Пользователь admin создан успешно!");

                        // Добавляем пользователя в роль admin
                        var addToRoleResult = await userManager.AddToRoleAsync(user, "admin");

                        if (addToRoleResult.Succeeded)
                        {
                            logger.LogInformation("Пользователь добавлен в роль 'admin' успешно!");
                        }
                        else
                        {
                            logger.LogError("Ошибка при добавлении в роль:");
                            foreach (var error in addToRoleResult.Errors)
                            {
                                logger.LogError($" - {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        logger.LogError("Ошибка при создании пользователя:");
                        foreach (var error in createResult.Errors)
                        {
                            logger.LogError($" - {error.Description}");
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Пользователь admin уже существует.");

                    // Проверяем, находится ли пользователь в роли admin
                    var isInRole = await userManager.IsInRoleAsync(adminUser, "admin");

                    if (!isInRole)
                    {
                        logger.LogInformation("Добавляем пользователя в роль 'admin'...");
                        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "admin");

                        if (addToRoleResult.Succeeded)
                        {
                            logger.LogInformation("Пользователь добавлен в роль 'admin' успешно!");
                        }
                        else
                        {
                            logger.LogError("Ошибка при добавлении в роль:");
                            foreach (var error in addToRoleResult.Errors)
                            {
                                logger.LogError($" - {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("Пользователь уже находится в роли 'admin'.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при инициализации базы данных");
            }
        }
    }
}