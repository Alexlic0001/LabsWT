using LabUlApi.Data;
using Microsoft.EntityFrameworkCore;
// using LabUlApi.Data; // Раскомментируйте после создания AppDbContext

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Добавление сервисов
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Для работы с циклическими ссылками в моделях





var app = builder.Build();

app.UseStaticFiles(); // Разрешает доступ к wwwroot

app.UseHttpsRedirection();

// Добавьте для обслуживания статических файлов (изображений)
app.UseStaticFiles();

// Разрешить CORS для доступа из UI проекта (если нужно)
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();
app.MapControllers();

// Инициализация БД 
await DbInitializer.SeedData(app);
app.Run();

try
{
    await DbInitializer.SeedData(app);
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при инициализации БД: {ex.Message}");
}