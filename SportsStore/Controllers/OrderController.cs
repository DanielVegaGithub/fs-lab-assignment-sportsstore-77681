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
        private readonly IPaymentService paymentService;

        public OrderController(
            IOrderRepository repoService,
            Cart cartService,
            ILogger<OrderController> loggerService,
            IPaymentService paymentService)
        {
            repository = repoService;
            cart = cartService;
            logger = loggerService;
            this.paymentService = paymentService;
        }

        public ViewResult Checkout()
        {
            logger.LogInformation(
                "Checkout page opened. CartItemCount: {CartItemCount}",
                cart.Lines.Count());

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
                "Starting payment before order creation. CustomerName: {CustomerName}, CartItemCount: {CartItemCount}, TotalValue: {TotalValue}",
                order.Name,
                order.Lines.Sum(l => l.Quantity),
                order.Lines.Sum(l => l.Product.Price * l.Quantity));

            PaymentResult paymentResult = paymentService.ProcessPayment(order, cart);

            if (!paymentResult.Succeeded)
            {
                order.PaymentStatus = paymentResult.Cancelled ? "Cancelled" : "Failed";
                order.PaymentIntentId = paymentResult.PaymentIntentId;
                order.PaymentConfirmationId = paymentResult.ConfirmationId;
                order.PaymentAmount = paymentResult.Amount;
                order.PaymentCurrency = paymentResult.Currency;
                order.PaymentProcessedAtUtc = DateTime.UtcNow;

                logger.LogWarning(
                    "Payment was not successful. Status: {PaymentStatus}, CustomerName: {CustomerName}, ErrorMessage: {ErrorMessage}",
                    paymentResult.Status,
                    order.Name,
                    paymentResult.ErrorMessage);

                ModelState.AddModelError("", paymentResult.ErrorMessage ?? "Payment failed. Please try again.");

                return View(order);
            }

            order.PaymentStatus = "Succeeded";
            order.PaymentIntentId = paymentResult.PaymentIntentId;
            order.PaymentConfirmationId = paymentResult.ConfirmationId;
            order.PaymentAmount = paymentResult.Amount;
            order.PaymentCurrency = paymentResult.Currency;
            order.PaymentProcessedAtUtc = DateTime.UtcNow;

            logger.LogInformation(
                "Payment succeeded. PaymentIntentId: {PaymentIntentId}, ConfirmationId: {ConfirmationId}, Amount: {Amount}, Currency: {Currency}",
                order.PaymentIntentId,
                order.PaymentConfirmationId,
                order.PaymentAmount,
                order.PaymentCurrency);

            logger.LogInformation(
                "Creating order after successful payment. CustomerName: {CustomerName}, CartItemCount: {CartItemCount}",
                order.Name,
                order.Lines.Count());

            repository.SaveOrder(order);

            logger.LogInformation(
                "Order created successfully. OrderId: {OrderId}, PaymentStatus: {PaymentStatus}, CustomerName: {CustomerName}",
                order.OrderID,
                order.PaymentStatus,
                order.Name);

            cart.Clear();

            logger.LogInformation(
                "Cart cleared after successful order creation. OrderId: {OrderId}",
                order.OrderID);

            return RedirectToPage("/Completed", new { orderId = order.OrderID });
        }
    }
}