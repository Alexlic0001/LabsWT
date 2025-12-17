using LabUI.Models;
using LabUlApi.Controllers;
using LabUlApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace LabUI.Tests
{
    public class DishesControllerTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _contextOptions;
        private readonly IWebHostEnvironment _environment;

        public DishesControllerTests()
        {
            
            _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            
            _environment = Substitute.For<IWebHostEnvironment>();
            _environment.WebRootPath.Returns("wwwroot");
            _environment.ContentRootPath.Returns(Directory.GetCurrentDirectory());

          
            using var context = new AppDbContext(_contextOptions);
            SeedDatabase(context);
        }

        [Fact]
        public async Task ControllerFiltersCategory()
        {
            // Arrange
            using var context = new AppDbContext(_contextOptions);
            var category = await context.Categories.FirstAsync(c => c.NormalizedName == "soups");
            var controller = new DishesController(context, _environment);

            // Act
            var response = await controller.GetDishes(category.NormalizedName);
            var responseData = response.Value;
            var dishesList = responseData!.Data!.Items;

            // Assert
            Assert.True(dishesList.All(d => d.CategoryId == category.Id));
            Assert.Equal(2, dishesList.Count);
        }

        [Theory]
        [InlineData(2, 3)] 
        [InlineData(3, 2)] 
        public async Task ControllerReturnsCorrectPagesCount(int pageSize, int expectedPages)
        {
            
            using var context = new AppDbContext(_contextOptions);
            var controller = new DishesController(context, _environment);

            // Act
            var response = await controller.GetDishes(null, 1, pageSize);
            var responseData = response.Value;
            var totalPages = responseData!.Data!.TotalPages;

            // Assert
            Assert.Equal(expectedPages, totalPages);
        }

        [Fact]
        public async Task ControllerReturnsCorrectPage()
        {
           
            using var context = new AppDbContext(_contextOptions);
            var controller = new DishesController(context, _environment);

            
            var response = await controller.GetDishes(null, 2, 3);
            var responseData = response.Value;
            var dishesList = responseData!.Data!.Items;
            var currentPage = responseData.Data.CurrentPage;

            // Assert
            Assert.Equal(2, currentPage);
            Assert.Equal(2, dishesList.Count); // На 2-й странице при размере 3 должно быть 2 блюда
        }

        private void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            
            var categories = new[]
            {
                new Category { Name = "Супы", NormalizedName = "soups" },
                new Category { Name = "Основные блюда", NormalizedName = "main-dishes" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            var dishes = new List<Dish>
            {
                new() { Name = "Суп-харчо", Description = "Острый суп", Price = 300, CategoryId = categories[0].Id },
                new() { Name = "Борщ", Description = "Красный борщ", Price = 250, CategoryId = categories[0].Id },
                new() { Name = "Плов", Description = "Узбекский плов", Price = 400, CategoryId = categories[1].Id },
                new() { Name = "Котлеты", Description = "Куриные котлеты", Price = 350, CategoryId = categories[1].Id },
                new() { Name = "Стейк", Description = "Говяжий стейк", Price = 500, CategoryId = categories[1].Id }
            };

            context.Dishes.AddRange(dishes);
            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = new AppDbContext(_contextOptions);
            context.Database.EnsureDeleted();
        }
    }
}