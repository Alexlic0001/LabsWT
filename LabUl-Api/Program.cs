using LabUlApi.Data;
using Microsoft.EntityFrameworkCore;
// using LabUlApi.Data; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.AddControllers()
    .AddNewtonsoftJson(); 


var app = builder.Build();

app.UseStaticFiles(); 

app.UseHttpsRedirection();


app.UseStaticFiles();
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