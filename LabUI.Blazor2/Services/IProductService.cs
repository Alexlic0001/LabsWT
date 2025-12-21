using LabUI.Models;

namespace LabUI.Blazor2.Services
{
    public interface IProductService<T> where T : class
    {
        event Action ListChanged;

        IEnumerable<T> Products { get; }
        int CurrentPage { get; }
        int TotalPages { get; }

        Task GetProducts(int pageNo = 1, int pageSize = 3);
    }
}