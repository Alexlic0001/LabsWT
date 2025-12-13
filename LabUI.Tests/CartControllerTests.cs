using LabUI.Controllers;
using LabUI.Models;
using LabUI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace LabUI.Tests
{
    public class CartControllerTests
    {
        private readonly IProductService _productService;
        private readonly DefaultHttpContext _httpContext;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _productService = Substitute.For<IProductService>();
            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();

            _controller = new CartController(_productService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async Task Add_AddsItemToCart()
        {
            // Arrange
            var dish = new Dish { Id = 1, Name = "Тест", Price = 100 };
            var response = new ResponseData<Dish>
            {
                Data = dish,
                Success = true
            };

            _productService.GetProductByIdAsync(1).Returns(Task.FromResult(response));

            // Act
            var result = await _controller.Add(1, "/Product");

            // Assert
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/Product", redirect.Url);

            // Используем Get<T>() вместо GetSession<T>()
            var cart = _httpContext.Session.Get<Cart>("cart");
            Assert.NotNull(cart);
            Assert.Equal(1, cart.Count);
            Assert.True(cart.CartItems.ContainsKey(1));
        }

        [Fact]
        public void Remove_RemovesItemFromCart()
        {
            // Arrange
            var cart = new Cart();
            cart.AddToCart(new Dish { Id = 1, Name = "Тест", Price = 100 });
            // Используем Set<T>() вместо SetSession()
            _httpContext.Session.Set("cart", cart);

            // Act
            var result = _controller.Remove(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            // Используем Get<T>() вместо GetSession<T>()
            var updatedCart = _httpContext.Session.Get<Cart>("cart");
            Assert.False(updatedCart!.CartItems.ContainsKey(1));
        }

        // Тестовая реализация сессии для тестов
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public string Id => "TestSessionId";
            public bool IsAvailable => true;
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear() => _store.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _store.Remove(key);

            public void Set(string key, byte[] value) => _store[key] = value;

            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }
    }
}