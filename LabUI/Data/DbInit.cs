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
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInit>>();

            try
            {
                // Проверяем существование администратора
                var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");

                if (adminUser == null)
                {
                    logger.LogInformation("Создание пользователя admin...");

                    // Создаем нового пользователя
                    var user = new AppUser
                    {
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true
                    };

                    // Создаем пользователя с паролем
                    var createResult = await userManager.CreateAsync(user, "123456");

                    if (createResult.Succeeded)
                    {
                        logger.LogInformation("Пользователь admin создан успешно!");

                        // Добавляем claim с ролью
                        var claimResult = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "admin"));

                        if (claimResult.Succeeded)
                        {
                            logger.LogInformation("Claim 'role: admin' добавлен успешно!");
                        }
                        else
                        {
                            logger.LogError("Ошибка при добавлении claim:");
                            foreach (var error in claimResult.Errors)
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

                    // Проверяем, есть ли уже claim с ролью admin
                    var existingClaims = await userManager.GetClaimsAsync(adminUser);
                    var adminClaim = existingClaims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.Role && c.Value == "admin");

                    if (adminClaim == null)
                    {
                        logger.LogInformation("Добавляем claim 'role: admin' существующему пользователю...");
                        var claimResult = await userManager.AddClaimAsync(adminUser,
                            new Claim(ClaimTypes.Role, "admin"));

                        if (claimResult.Succeeded)
                        {
                            logger.LogInformation("Claim добавлен успешно!");
                        }
                        else
                        {
                            logger.LogError("Ошибка при добавлении claim:");
                            foreach (var error in claimResult.Errors)
                            {
                                logger.LogError($" - {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("Claim 'role: admin' уже существует.");
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