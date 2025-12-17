using LabUI.Controllers;
using LabUI.Models;
using LabUI.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace LabUI.Tests
{
    public class ProductControllerTests
    {
        private IProductService _productService; // убрать readonly
        private ICategoryService _categoryService; // убрать readonly

        public ProductControllerTests()
        {
            SetupData();
        }

        [Fact]
        public async Task IndexPutsCategoriesToViewData()
        {
            
            var controller = new ProductController(_productService, _categoryService);

            
            var response = await controller.Index(null);

         
            var view = Assert.IsType<ViewResult>(response);
            var categories = Assert.IsType<List<Category>>(view.ViewData["categories"]);
            Assert.Equal(3, categories.Count);
            Assert.Equal("Все", view.ViewData["currentCategory"]);
        }

        [Fact]
        public async Task IndexSetsCorrectCurrentCategory()
        {
            
            var categories = await _categoryService.GetCategoryListAsync();
            var currentCategory = categories.Data![0];
            var controller = new ProductController(_productService, _categoryService);

           
            var response = await controller.Index(currentCategory.NormalizedName);

           
            var view = Assert.IsType<ViewResult>(response);
            Assert.Equal(currentCategory.Name, view.ViewData["currentCategory"]);
        }

        [Fact]
        public async Task IndexReturnsNotFound()
        {
            
            string errorMessage = "Test error";
            var categoriesResponse = new ResponseData<List<Category>>
            {
                Success = false,
                ErrorMessage = errorMessage
            };

            _categoryService.GetCategoryListAsync().Returns(Task.FromResult(categoriesResponse));
            var controller = new ProductController(_productService, _categoryService);

           
            var response = await controller.Index(null);

           
            var result = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal(errorMessage, result.Value);
        }

        private void SetupData()
        {
            
            _categoryService = Substitute.For<ICategoryService>();
            var categoriesResponse = new ResponseData<List<Category>>
            {
                Data = new List<Category>
                {
                    new() { Id = 1, Name = "Супы", NormalizedName = "soups" },
                    new() { Id = 2, Name = "Основные блюда", NormalizedName = "main-dishes" },
                    new() { Id = 3, Name = "Десерты", NormalizedName = "desserts" }
                },
                Success = true
            };

            _categoryService.GetCategoryListAsync().Returns(Task.FromResult(categoriesResponse));

            
            _productService = Substitute.For<IProductService>();
            var dishesResponse = new ResponseData<ListModel<Dish>>
            {
                Data = new ListModel<Dish>
                {
                    Items = new List<Dish>
                    {
                        new() { Id = 1, Name = "Суп-харчо", CategoryId = 1 },
                        new() { Id = 2, Name = "Борщ", CategoryId = 1 },
                        new() { Id = 3, Name = "Плов", CategoryId = 2 },
                        new() { Id = 4, Name = "Торт", CategoryId = 3 }
                    },
                    CurrentPage = 1,
                    TotalPages = 1
                },
                Success = true
            };

            _productService.GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(Task.FromResult(dishesResponse));
        }
    }
}