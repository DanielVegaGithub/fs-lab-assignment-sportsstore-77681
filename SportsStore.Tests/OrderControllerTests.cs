using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SportsStore.Controllers;
using SportsStore.Models;
using Xunit;

namespace SportsStore.Tests
{
    public class OrderControllerTests
    {
        [Fact]
        public void Cannot_Checkout_Empty_Cart()
        {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Mock<IPaymentService> paymentMock = new Mock<IPaymentService>();
            Cart cart = new Cart();
            Order order = new Order();

            OrderController target = new OrderController(
                mock.Object,
                cart,
                Mock.Of<ILogger<OrderController>>(),
                paymentMock.Object);

            ViewResult? result = target.Checkout(order) as ViewResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Mock<IPaymentService> paymentMock = new Mock<IPaymentService>();
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            OrderController target = new OrderController(
                mock.Object,
                cart,
                Mock.Of<ILogger<OrderController>>(),
                paymentMock.Object);

            target.ModelState.AddModelError("error", "error");

            ViewResult? result = target.Checkout(new Order()) as ViewResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Never);
            Assert.True(string.IsNullOrEmpty(result?.ViewName));
            Assert.False(result?.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Can_Checkout_And_Submit_Order()
        {
            Mock<IOrderRepository> mock = new Mock<IOrderRepository>();
            Mock<IPaymentService> paymentMock = new Mock<IPaymentService>();

            paymentMock.Setup(p => p.ProcessPayment(It.IsAny<Order>(), It.IsAny<Cart>()))
                .Returns(new PaymentResult
                {
                    Succeeded = true,
                    Status = "succeeded",
                    PaymentIntentId = "pi_test_123",
                    ConfirmationId = "ch_test_123",
                    Amount = 100,
                    Currency = "usd"
                });

            Cart cart = new Cart();
            cart.AddItem(new Product { Price = 10 }, 1);

            OrderController target = new OrderController(
                mock.Object,
                cart,
                Mock.Of<ILogger<OrderController>>(),
                paymentMock.Object);

            RedirectToPageResult? result =
                target.Checkout(new Order { Name = "Test User" }) as RedirectToPageResult;

            mock.Verify(m => m.SaveOrder(It.IsAny<Order>()), Times.Once);
            Assert.Equal("/Completed", result?.PageName);
        }
    }
}