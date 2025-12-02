using LabUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabUI.Controllers
{
    [Route("Catalog")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [Route("")] // Маршрут: /Catalog
        [Route("{category?}")] // Маршрут: /Catalog/{category}
        public async Task<IActionResult> Index(string? category, int pageNo = 1)
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (!categoriesResponse.Success)
                return NotFound(categoriesResponse.ErrorMessage);

            ViewData["categories"] = categoriesResponse.Data;

            var currentCategory = category == null
                ? "Все"
                : categoriesResponse.Data?.FirstOrDefault(c => c.NormalizedName == category)?.Name ?? "Все";
            ViewData["currentCategory"] = currentCategory;

            var productResponse = await _productService.GetProductListAsync(category, pageNo);
            if (!productResponse.Success)
                ViewData["Error"] = productResponse.ErrorMessage;

            return View(productResponse.Data);
        }

        [Route("Details/{id:int}")] // Маршрут: /Catalog/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var productResponse = await _productService.GetProductByIdAsync(id);
            if (!productResponse.Success || productResponse.Data == null)
                return NotFound();

            return View(productResponse.Data);
        }
    }
}