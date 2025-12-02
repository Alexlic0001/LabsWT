using Microsoft.EntityFrameworkCore;
using LabUlApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=LabUlApi.db";

// Используем SQLite вместо SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();

// УБРАТЬ AddOpenApi() и использовать ТОЛЬКО Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// АВТОМАТИЧЕСКОЕ СОЗДАНИЕ БАЗЫ ДАННЫХ И ЗАПОЛНЕНИЕ ДАННЫМИ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Создаем базу данных и таблицы если их нет
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("Database created successfully!");

    // Заполняем данными если таблицы пустые
    if (!db.Categories.Any())
    {
        var categories = new Category[]
        {
            new Category { Name = "Стартеры", NormalizedName = "starters" },
            new Category { Name = "Салаты", NormalizedName = "salads" },
            new Category { Name = "Супы", NormalizedName = "soups" },
            new Category { Name = "Основные блюда", NormalizedName = "main-dishes" },
            new Category { Name = "Напитки", NormalizedName = "drinks" },
            new Category { Name = "Десерты", NormalizedName = "desserts" }
        };

        await db.Categories.AddRangeAsync(categories);
        await db.SaveChangesAsync();

        var dishes = new List<Dish>
        {
            new Dish
            {
                Name = "Суп-харчо",
                Description = "Очень острый, невкусный",
                Calories = 200,
                Category = categories.FirstOrDefault(c => c.NormalizedName == "soups"),
                Image = "/Images/Суп.jpg"
            },
            new Dish
            {
                Name = "Борщ",
                Description = "Много сала, без сметаны",
                Calories = 330,
                Category = categories.FirstOrDefault(c => c.NormalizedName == "soups"),
                Image = "/Images/Борщ.jpg"
            },
            new Dish
            {
                Name = "Котлета пожарская",
                Description = "Хлеб - 80%, Морковь - 20%",
                Calories = 635,
                Category = categories.FirstOrDefault(c => c.NormalizedName == "main-dishes"),
                Image = "/Images/Котлета.jpg"
            },
            new Dish
            {
                Name = "Макароны по-флотски",
                Description = "С охотничьей колбаской",
                Calories = 524,
                Category = categories.FirstOrDefault(c => c.NormalizedName == "main-dishes"),
                Image = "/Images/Макароны.jpg"
            },
            new Dish
            {
                Name = "Компот",
                Description = "Быстро растворимый, 2 литра",
                Calories = 180,
                Category = categories.FirstOrDefault(c => c.NormalizedName == "drinks"),
                Image = "/Images/Компот.jpg"
            }
        };

        await db.Dishes.AddRangeAsync(dishes);
        await db.SaveChangesAsync();

        Console.WriteLine("Database seeded with initial data!");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // УБРАТЬ MapOpenApi() и использовать ТОЛЬКО Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();