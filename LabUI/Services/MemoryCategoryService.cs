using LabUI.Models;

namespace LabUI.Services
{
    public class MemoryCategoryService : ICategoryService
    {
        public Task<ResponseData<List<Category>>> GetCategoryListAsync()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Стартеры", NormalizedName = "starters" },
                new Category { Id = 2, Name = "Салаты", NormalizedName = "salads" },
                new Category { Id = 3, Name = "Основные блюда", NormalizedName = "main-courses" },
                new Category { Id = 4, Name = "Десерты", NormalizedName = "desserts" },
                new Category { Id = 5, Name = "Напитки", NormalizedName = "drinks" }
            };

            var result = new ResponseData<List<Category>>
            {
                Data = categories
            };

            return Task.FromResult(result);
        }
    }
}
