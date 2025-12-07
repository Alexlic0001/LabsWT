using LabUI.Models;

namespace LabUI.Services
{
    public interface IProductService
    {
        Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1);
        Task<ResponseData<Dish>> GetProductByIdAsync(int id);
        Task<ResponseData<Dish>> UpdateProductAsync(int id, Dish product, IFormFile? formFile);
        Task<ResponseData<bool>> DeleteProductAsync(int id);
        Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile);
    }
}