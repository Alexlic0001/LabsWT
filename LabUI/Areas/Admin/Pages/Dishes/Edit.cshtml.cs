using LabUI.Models;
using LabUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LabUI.Areas.Admin.Pages.Dishes
{
    [Authorize(Roles = "admin")]
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IProductService productService,
                        ICategoryService categoryService,
                        ILogger<EditModel> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        [BindProperty]
        public Dish Dish { get; set; } = default!;

        [BindProperty]
        public IFormFile? Image { get; set; }

        public SelectList? Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var response = await _productService.GetProductByIdAsync(id.Value);
            if (!response.Success || response.Data == null)
            {
                return NotFound();
            }

            Dish = response.Data;

            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("=== НАЧАЛО OnPostAsync ===");
            _logger.LogInformation($"ID блюда: {Dish?.Id}");
            _logger.LogInformation($"Название: {Dish?.Name}");
            _logger.LogInformation($"Изображение получено: {Image != null}");
            _logger.LogInformation($"Имя файла: {Image?.FileName}");
            _logger.LogInformation($"Размер файла: {Image?.Length} байт");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Модель невалидна");
                foreach (var error in ModelState)
                {
                    _logger.LogWarning($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                _logger.LogInformation("Вызов UpdateProductAsync...");
                var updateResponse = await _productService.UpdateProductAsync(Dish.Id, Dish, Image);

                _logger.LogInformation($"Результат UpdateProductAsync: Success={updateResponse.Success}");
                _logger.LogInformation($"Ошибка: {updateResponse.ErrorMessage}");
                _logger.LogInformation($"Данные: {updateResponse.Data?.Name}, Image URL: {updateResponse.Data?.Image}");

                if (!updateResponse.Success)
                {
                    _logger.LogError($"Ошибка при обновлении: {updateResponse.ErrorMessage}");
                    ModelState.AddModelError(string.Empty, updateResponse.ErrorMessage ?? "Ошибка при обновлении блюда");
                    await LoadCategoriesAsync();
                    return Page();
                }

                _logger.LogInformation("Успешно сохранено, редирект на Index");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Исключение в OnPostAsync");
                ModelState.AddModelError(string.Empty, $"Ошибка при обновлении: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
            }
            finally
            {
                _logger.LogInformation("=== КОНЕЦ OnPostAsync ===");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (categoriesResponse.Success && categoriesResponse.Data != null)
            {
                Categories = new SelectList(categoriesResponse.Data, "Id", "Name", Dish?.CategoryId);
            }
            else
            {
                Categories = new SelectList(new List<Category>(), "Id", "Name");
            }
        }
    }
}