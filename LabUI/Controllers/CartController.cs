using LabUI.Models;
using LabUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            return View(cart.CartItems);
        }

        [Route("[controller]/add/{id:int}")]
        public async Task<IActionResult> Add(int id, string returnUrl)
        {
            var data = await _productService.GetProductByIdAsync(id);
            if (data.Success)
            {
                var cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
                cart.AddToCart(data.Data!);
                HttpContext.Session.Set("cart", cart);
            }

            return Redirect(returnUrl);
        }

        [Route("[controller]/remove/{id:int}")]
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.RemoveItems(id);
            HttpContext.Session.Set("cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("cart");
            return RedirectToAction("Index");
        }
    }
}