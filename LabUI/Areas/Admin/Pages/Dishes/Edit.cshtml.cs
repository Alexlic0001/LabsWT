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

        public EditModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
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

            // Загружаем категории
            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                var updateResponse = await _productService.UpdateProductAsync(Dish.Id, Dish, Image);

                if (!updateResponse.Success)
                {
                    ModelState.AddModelError(string.Empty, updateResponse.ErrorMessage ?? "Ошибка при обновлении");
                    await LoadCategoriesAsync();
                    return Page();
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
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