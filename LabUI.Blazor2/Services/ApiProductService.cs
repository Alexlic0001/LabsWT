using System.Net.Http.Json;
using LabUI.Models;

namespace LabUI.Blazor2.Services
{
    public class ApiProductService(HttpClient Http) : IProductService<Dish>
    {
        private List<Dish> _dishes;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public IEnumerable<Dish> Products => _dishes;
        public int CurrentPage => _currentPage;
        public int TotalPages => _totalPages;

        public event Action ListChanged;

        public async Task GetProducts(int pageNo = 1, int pageSize = 3)
        {
            var queryData = new Dictionary<string, string>
            {
                { "pageNo", pageNo.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            var query = QueryString.Create(queryData);
            var result = await Http.GetAsync("api/dishes" + query.Value);

            if (result.IsSuccessStatusCode)
            {
                var responseData = await result.Content
                    .ReadFromJsonAsync<ResponseData<ListModel<Dish>>>();

                _currentPage = responseData.Data.CurrentPage;
                _totalPages = responseData.Data.TotalPages;
                _dishes = responseData.Data.Items;
                ListChanged?.Invoke();
            }
            else
            {
                _dishes = null;
                _currentPage = 1;
                _totalPages = 1;
                ListChanged?.Invoke();
            }
        }
    }
}