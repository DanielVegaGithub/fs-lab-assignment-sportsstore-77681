using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportsStore.Models;

namespace SportsStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository repository;
        private readonly Cart cart;
        private readonly ILogger<OrderController> logger;

        public OrderController(IOrderRepository repoService, Cart cartService, ILogger<OrderController> loggerService)
        {
            repository = repoService;
            cart = cartService;
            logger = loggerService;
        }

        public ViewResult Checkout()
        {
            logger.LogInformation("Checkout page opened. CartItemCount: {CartItemCount}", cart.Lines.Count());

            return View(new Order());
        }

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            logger.LogInformation(
                "Checkout submitted. CartItemCount: {CartItemCount}, CustomerName: {CustomerName}",
                cart.Lines.Count(),
                order.Name);

            if (cart.Lines.Count() == 0)
            {
                logger.LogWarning("Checkout failed because cart was empty");

                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (!ModelState.IsValid)
            {
                logger.LogWarning(
                    "Checkout validation failed. CartItemCount: {CartItemCount}, CustomerName: {CustomerName}",
                    cart.Lines.Count(),
                    order.Name);

                return View();
            }

            order.Lines = cart.Lines.ToArray();

            logger.LogInformation(
                "Creating order. CartItemCount: {CartItemCount}, CustomerName: {CustomerName}, TotalValue: {TotalValue}",
                order.Lines.Count(),
                order.Name,
                order.Lines.Sum(l => l.Product.Price * l.Quantity));

            repository.SaveOrder(order);

            logger.LogInformation(
                "Order created successfully. OrderId: {OrderId}, CartItemCount: {CartItemCount}, CustomerName: {CustomerName}",
                order.OrderID,
                order.Lines.Count(),
                order.Name);

            cart.Clear();

            logger.LogInformation("Cart cleared after successful order creation. OrderId: {OrderId}", order.OrderID);

            return RedirectToPage("/Completed", new { orderId = order.OrderID });
        }
    }
}