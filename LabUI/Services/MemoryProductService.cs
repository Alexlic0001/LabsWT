using LabUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LabUI.Services
{
    public class MemoryProductService : IProductService
    {
        private List<Dish> _dishes;
        private List<Category> _categories;
        private readonly IConfiguration _config;

        public MemoryProductService(IConfiguration config, ICategoryService categoryService)
        {
            _config = config;
            var categoryResponse = categoryService.GetCategoryListAsync().Result;
            _categories = categoryResponse.Data ?? new List<Category>();
            SetupData();
        }

        private void SetupData()
        {
            _dishes = new List<Dish>
            {
                new Dish
                {
                    Id = 1,
                    Name = "Суп-харчо",
                    Description = "Острый и пряный грузинский суп на основе говядины и грецких орехов.",
                    Price = 15.99m,
                    Image = "images/soup.jpg",
                    CategoryId = _categories.Find(c => c.NormalizedName == "starters")?.Id ?? 1
                },
                new Dish
                {
                    Id = 2,
                    Name = "Борщ",
                    Description = "Густой ароматный свекольный суп, традиционное славянское блюдо.",
                    Price = 12.50m,
                    Image = "images/borshch.jpg",
                    CategoryId = _categories.Find(c => c.NormalizedName == "starters")?.Id ?? 1
                },
                new Dish
                {
                    Id = 3,
                    Name = "Цезарь",
                    Description = "Классический салат с листьями айсберга, гренками, сыром пармезан и соусом на основе майонеза, яиц и анчоусов.",
                    Price = 18.75m,
                    Image = "images/caesar.jpg",
                    CategoryId = _categories.Find(c => c.NormalizedName == "salads")?.Id ?? 2
                },
                new Dish
                {
                    Id = 4,
                    Name = "Стейк",
                    Description = "Говяжий стейк средней прожарки",
                    Price = 35.00m,
                    Image = "images/steak.jpg",
                    CategoryId = _categories.Find(c => c.NormalizedName == "main-courses")?.Id ?? 3
                }
            };
        }

        public Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            int? categoryId = null;

            if (categoryNormalizedName != null)
            {
                categoryId = _categories.Find(c => c.NormalizedName.Equals(categoryNormalizedName))?.Id;
            }
            var filteredDishes = _dishes.Where(d => categoryId == null || d.CategoryId.Equals(categoryId)).ToList();
            // Получаем размер страницы из конфигурации
            int pageSize = _config.GetValue<int>("ItemsPerPage", 3);

            // Вычисляем общее количество страниц
            int totalPages = (int)Math.Ceiling(filteredDishes.Count / (double)pageSize);

            // Получаем данные для текущей страницы
            var pagedDishes = filteredDishes
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            var data = _dishes.Where(d => categoryId == null || d.CategoryId.Equals(categoryId)).ToList();

            var model = new ListModel<Dish>
            {
                Items = pagedDishes,
                CurrentPage = pageNo,
                TotalPages = totalPages,
                TotalCount = filteredDishes.Count
            };

            var result = new ResponseData<ListModel<Dish>>
            {
                Data = model
            };

            if (filteredDishes.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Нет объектов в выбранной категории";
            }

            return Task.FromResult(result);
        }

        public Task<ResponseData<Dish>> GetProductByIdAsync(int id)
        {
            var dish = _dishes.FirstOrDefault(d => d.Id == id);
            var result = new ResponseData<Dish>
            {
                Data = dish,
                Success = dish != null,
                ErrorMessage = dish == null ? "Блюдо не найдено" : null
            };

            return Task.FromResult(result);
        }

        public Task UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            var existingDish = _dishes.FirstOrDefault(d => d.Id == id);
            if (existingDish != null)
            {
                existingDish.Name = product.Name;
                existingDish.Description = product.Description;
                existingDish.Price = product.Price;
                existingDish.CategoryId = product.CategoryId;
            }
            return Task.CompletedTask;
        }

        public Task DeleteProductAsync(int id)
        {
            var dish = _dishes.FirstOrDefault(d => d.Id == id);
            if (dish != null)
            {
                _dishes.Remove(dish);
            }
            return Task.CompletedTask;
        }

        public Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            product.Id = _dishes.Max(d => d.Id) + 1;
            _dishes.Add(product);

            var result = new ResponseData<Dish>
            {
                Data = product
            };

            return Task.FromResult(result);
        }
    }
}