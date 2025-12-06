using LabUI.Models;
using System.Net.Http.Json;

namespace LabUI.Services
{
    public class ApiProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiProductService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7002/api/";
        }

        public async Task<ResponseData<ListModel<Dish>>> GetProductListAsync(
            string? categoryNormalizedName,
            int pageNo = 1)
        {
            var queryParams = new List<string>();

            if (pageNo > 1)
                queryParams.Add($"pageNo={pageNo}");

            if (!string.IsNullOrEmpty(categoryNormalizedName))
                queryParams.Add($"category={categoryNormalizedName}");

            var queryString = queryParams.Any()
                ? "?" + string.Join("&", queryParams)
                : "";

            var url = $"{_apiBaseUrl}dishes{queryString}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseData<ListModel<Dish>>>(url);
                return response ?? new ResponseData<ListModel<Dish>>
                {
                    Success = false,
                    ErrorMessage = "Ошибка чтения ответа API"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseData<ListModel<Dish>>
                {
                    Success = false,
                    ErrorMessage = $"Ошибка подключения к API: {ex.Message}"
                };
            }
        }

        public async Task<ResponseData<Dish>> GetProductByIdAsync(int id)
        {
            var url = $"{_apiBaseUrl}dishes/{id}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseData<Dish>>(url);
                return response ?? new ResponseData<Dish>
                {
                    Success = false,
                    ErrorMessage = "Блюдо не найдено"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseData<Dish>
                {
                    Success = false,
                    ErrorMessage = $"Ошибка подключения к API: {ex.Message}"
                };
            }
        }

        // Остальные методы можно оставить как заглушки
        public Task UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            return Task.CompletedTask;
        }

        public Task DeleteProductAsync(int id)
        {
            return Task.CompletedTask;
        }

        public Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            return Task.FromResult(new ResponseData<Dish>
            {
                Data = product,
                Success = true
            });
        }
    }
}