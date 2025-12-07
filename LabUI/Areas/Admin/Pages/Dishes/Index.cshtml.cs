using LabUI.Models;
using LabUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabUI.Areas.Admin.Pages.Dishes
{
    [Authorize(Roles = "admin")]
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Dish> Dish { get; set; } = default!;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        public async Task OnGetAsync(int? pageNo = 1)
        {
            var response = await _productService.GetProductListAsync(null, pageNo.Value);
            if (response.Success)
            {
                Dish = response.Data.Items;
                CurrentPage = response.Data.CurrentPage;
                TotalPages = response.Data.TotalPages;

                // ДОБАВЬТЕ ОТЛАДКУ
                Console.WriteLine($"Получено блюд: {Dish.Count}");
                foreach (var dish in Dish)
                {
                    Console.WriteLine($"Блюдо: {dish.Name}, Image URL: {dish.Image}");

                    // Проверьте доступность изображения
                    if (!string.IsNullOrEmpty(dish.Image))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var imageResponse = await httpClient.GetAsync(dish.Image);
                            Console.WriteLine($"  Изображение доступно: {imageResponse.IsSuccessStatusCode}, Status: {imageResponse.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Ошибка проверки изображения: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}