using LabUI.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LabUI.Services
{
    public class ApiProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<ApiProductService> _logger;

        public ApiProductService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiProductService> logger)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7002/api/";
            _logger = logger;
        }

        // Метод должен соответствовать интерфейсу: GetProductListAsync(string?, int)
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

        public async Task<ResponseData<Dish>> UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            var responseData = new ResponseData<Dish>();

            try
            {
                _logger.LogInformation($"Обновление блюда ID: {id}");

                // 1. Обновляем блюдо
                var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}dishes/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Ошибка при обновлении блюда: {response.StatusCode}, {errorContent}");

                    responseData.Success = false;
                    responseData.ErrorMessage = $"Ошибка при обновлении: {response.StatusCode}";
                    return responseData;
                }

                // 2. Обновляем изображение, если оно есть
                if (formFile != null)
                {
                    _logger.LogInformation($"Обновление изображения для блюда ID: {id}");

                    var imageResult = await UpdateProductImageAsync(id, formFile);
                    if (!imageResult)
                    {
                        _logger.LogWarning($"Не удалось обновить изображение для блюда ID: {id}");
                        // Не прерываем операцию, только логируем
                    }
                }

                // 3. Получаем обновленное блюдо для возврата
                var updatedDishResponse = await GetProductByIdAsync(id);
                if (updatedDishResponse.Success)
                {
                    responseData.Data = updatedDishResponse.Data;
                }

                responseData.Success = true;
                _logger.LogInformation($"Блюдо ID: {id} успешно обновлено");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении блюда ID: {id}");
                responseData.Success = false;
                responseData.ErrorMessage = ex.Message;
            }

            return responseData;
        }

        private async Task<bool> UpdateProductImageAsync(int productId, IFormFile imageFile)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                using var imageStream = imageFile.OpenReadStream();
                using var streamContent = new StreamContent(imageStream);

                formData.Add(streamContent, "image", imageFile.FileName);

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}dishes/{productId}/image", formData);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении изображения для продукта {productId}");
                return false;
            }
        }

        public async Task<ResponseData<bool>> DeleteProductAsync(int id)
        {
            var responseData = new ResponseData<bool>();

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}dishes/{id}");

                responseData.Success = response.IsSuccessStatusCode;
                responseData.Data = response.IsSuccessStatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    responseData.ErrorMessage = $"Ошибка удаления: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении блюда ID: {id}");
                responseData.Success = false;
                responseData.ErrorMessage = ex.Message;
            }

            return responseData;
        }

        public async Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            var responseData = new ResponseData<Dish>();

            try
            {
                // 1. Создаем блюдо
                var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}dishes", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Ошибка при создании блюда: {response.StatusCode}, {errorContent}");

                    responseData.Success = false;
                    responseData.ErrorMessage = $"Ошибка при создании: {response.StatusCode}";
                    return responseData;
                }

                // 2. Получаем созданное блюдо
                var createdDish = await response.Content.ReadFromJsonAsync<Dish>();
                if (createdDish != null)
                {
                    responseData.Data = createdDish;

                    // 3. Добавляем изображение, если оно есть
                    if (formFile != null)
                    {
                        await UpdateProductImageAsync(createdDish.Id, formFile);

                        // Обновляем блюдо с новым URL изображения
                        var updatedDish = await GetProductByIdAsync(createdDish.Id);
                        if (updatedDish.Success)
                        {
                            responseData.Data = updatedDish.Data;
                        }
                    }
                }

                responseData.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании блюда");
                responseData.Success = false;
                responseData.ErrorMessage = ex.Message;
            }

            return responseData;
        }
    }
}