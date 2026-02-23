using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SportsStore.Controllers;
using SportsStore.Models;
using SportsStore.Services.Payments;
using Xunit;

namespace SportsStore.Tests
{
    public class OrderControllerTests
    {
        private static OrderController CreateController(
            Mock<IOrderRepository> repoMock,
            Cart cart,
            Mock<IStripePaymentService> stripeMock)
        {
            var loggerMock = new Mock<ILogger<OrderController>>();

            var controller = new OrderController(repoMock.Object, cart, stripeMock.Object, loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // URLs precisam existir para o controller construir success/cancel
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("localhost");

            return controller;
        }

        [Fact]
        public async Task Cannot_Checkout_Empty_Cart()
        {
            var repoMock = new Mock<IOrderRepository>();
            var stripeMock = new Mock<IStripePaymentService>();
            var cart = new Cart();

            var controller = CreateController(repoMock, cart, stripeMock);

            var result = await controller.Checkout(new Order());

            var view = Assert.IsType<ViewResult>(result);
            repoMock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.False(view.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Cannot_Checkout_Invalid_ShippingDetails()
        {
            var repoMock = new Mock<IOrderRepository>();
            var stripeMock = new Mock<IStripePaymentService>();

            var cart = new Cart();
            cart.AddItem(new Product { Name = "Test", Price = 10m }, 1);

            var controller = CreateController(repoMock, cart, stripeMock);
            controller.ModelState.AddModelError("error", "error");

            var result = await controller.Checkout(new Order());

            var view = Assert.IsType<ViewResult>(result);
            repoMock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.False(view.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Checkout_Redirects_To_Stripe_And_Does_Not_Save_Order_Yet()
        {
            var repoMock = new Mock<IOrderRepository>();
            var stripeMock = new Mock<IStripePaymentService>();

            var cart = new Cart();
            cart.AddItem(new Product { Name = "Test", Price = 10m }, 1);

            stripeMock
                .Setup(s => s.CreateCheckoutUrlAsync(It.IsAny<Cart>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://stripe.test/checkout");

            var controller = CreateController(repoMock, cart, stripeMock);

            var result = await controller.Checkout(new Order { Name = "A", Line1 = "L1", City = "C", State = "S", Country = "BR" });

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://stripe.test/checkout", redirect.Url);

            repoMock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
        }
    }

    internal class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}