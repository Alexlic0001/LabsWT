using System.Net.Http.Json;
using LabUI.Models;

namespace LabUI.Blazor
{
    public class ApiProductService : IProductService<Dish>
    {
        private readonly HttpClient _http;
        private List<Dish>? _dishes;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public event Action? ListChanged;

        public IEnumerable<Dish> Products => _dishes ?? new List<Dish>();
        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;

        public ApiProductService(HttpClient http)
        {
            _http = http;
        }

        public async Task GetProducts(int pageNo = 1, int pageSize = 3)
        {
            try
            {
                Console.WriteLine($"API: Запрос к {_http.BaseAddress}api/dishes?pageNo={pageNo}&pageSize={pageSize}");

                var queryData = new Dictionary<string, string>
        {
            { "pageNo", pageNo.ToString() },
            { "pageSize", pageSize.ToString() }
        };

                var query = Microsoft.AspNetCore.Http.QueryString.Create(queryData);
                var url = $"api/dishes{query.Value}";
                Console.WriteLine($"API: Полный URL: {url}");

                var response = await _http.GetAsync(url);
                Console.WriteLine($"API: Статус ответа: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API: Успешный ответ");
                    var responseData = await response.Content
                        .ReadFromJsonAsync<ResponseData<ListModel<Dish>>>();

                    Console.WriteLine($"API: Success={responseData?.Success}, Items count={responseData?.Data?.Items?.Count}");

                    if (responseData?.Success == true && responseData.Data != null)
                    {
                        _currentPage = responseData.Data.CurrentPage;
                        _totalPages = responseData.Data.TotalPages;
                        _dishes = responseData.Data.Items;
                        Console.WriteLine($"API: Установлено {_dishes?.Count} блюд");
                        ListChanged?.Invoke();
                    }
                }
                else
                {
                    Console.WriteLine($"API: Ошибка HTTP: {response.StatusCode}");
                    ResetData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API: Исключение: {ex.Message}");
                ResetData();
            }
        }

        private void ResetData()
        {
            _dishes = new List<Dish>();
            _currentPage = 1;
            _totalPages = 1;
            ListChanged?.Invoke();
        }
    }
}