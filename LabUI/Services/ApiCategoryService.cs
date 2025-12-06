using LabUI.Models;
using System.Net.Http.Json;

namespace LabUI.Services
{
    public class ApiCategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiCategoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7002/api/";
        }

        public async Task<ResponseData<List<Category>>> GetCategoryListAsync()
        {
            var url = $"{_apiBaseUrl}categories";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseData<List<Category>>>(url);
                return response ?? new ResponseData<List<Category>>
                {
                    Success = false,
                    ErrorMessage = "Ошибка чтения ответа API"
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseData<List<Category>>
                {
                    Success = false,
                    ErrorMessage = $"Ошибка подключения к API: {ex.Message}"
                };
            }
        }
    }
}