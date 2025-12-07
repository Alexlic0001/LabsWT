using LabUI.Data;
using LabUI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// SQLite для Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// УПРОЩЕННАЯ НАСТРОЙКА - удаляем AddRazorPagesOptions
builder.Services.AddRazorPages();
// УДАЛИТЬ весь блок AddRazorPagesOptions

// Регистрация HttpClient для работы с API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7002/"); // URL вашего API проекта
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Регистрация API сервисов
builder.Services.AddHttpClient<ApiProductService>();
builder.Services.AddScoped<ICategoryService, ApiCategoryService>();
builder.Services.AddScoped<IProductService, ApiProductService>();

builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
})
 .AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

// Для Tag Helpers (если используются)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

builder.Services.AddAuthorization(opt =>
{
    //  для проверки
    opt.AddPolicy("admin", p => p.RequireRole("admin"));
});

builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

// Инициализация базы данных Identity
try
{
    await DbInit.SeedData(app);
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при инициализации базы данных: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// УПРОЩЕННАЯ НАСТРОЙКА МАРШРУТОВ - только один способ
app.MapRazorPages();
app.MapControllers(); // Если используете API контроллеры

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();