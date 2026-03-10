using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace SportsStore.Models
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings settings;
        private readonly ILogger<StripePaymentService> logger;

        public StripePaymentService(
            IOptions<StripeSettings> options,
            ILogger<StripePaymentService> loggerService)
        {
            settings = options.Value;
            logger = loggerService;
        }

        public PaymentResult ProcessPayment(Order order, Cart cart)
        {
            decimal total = cart.ComputeTotalValue();

            string paymentMethod = (order.Name?.Contains("FAIL", StringComparison.OrdinalIgnoreCase) ?? false)
                ? "pm_card_chargeDeclined"
                : "pm_card_visa";

            logger.LogInformation(
                "Starting Stripe payment processing. CustomerName: {CustomerName}, CartItemCount: {CartItemCount}, TotalValue: {TotalValue}, Currency: {Currency}, PaymentMethod: {PaymentMethod}",
                order.Name,
                cart.Lines.Sum(l => l.Quantity),
                total,
                settings.Currency,
                paymentMethod);

            try
            {
                StripeConfiguration.ApiKey = settings.SecretKey;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(total * 100),
                    Currency = settings.Currency,
                    PaymentMethod = paymentMethod,
                    Confirm = true,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    },
                    Description = $"SportsStore order for {order.Name}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["CustomerName"] = order.Name ?? string.Empty,
                        ["CartItemCount"] = cart.Lines.Sum(l => l.Quantity).ToString(),
                        ["OrderSource"] = "SportsStore"
                    }
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = service.Create(options);

                logger.LogInformation(
                    "Stripe payment processed. PaymentIntentId: {PaymentIntentId}, Status: {PaymentStatus}, Amount: {Amount}, Currency: {Currency}",
                    paymentIntent.Id,
                    paymentIntent.Status,
                    total,
                    settings.Currency);

                return new PaymentResult
                {
                    Succeeded = paymentIntent.Status == "succeeded",
                    Cancelled = paymentIntent.Status == "canceled",
                    Status = paymentIntent.Status,
                    PaymentIntentId = paymentIntent.Id,
                    ConfirmationId = paymentIntent.LatestChargeId,
                    Amount = total,
                    Currency = settings.Currency
                };
            }
            catch (StripeException ex)
            {
                logger.LogError(
                    ex,
                    "Stripe payment failed. CustomerName: {CustomerName}, CartItemCount: {CartItemCount}, TotalValue: {TotalValue}",
                    order.Name,
                    cart.Lines.Sum(l => l.Quantity),
                    total);

                return new PaymentResult
                {
                    Succeeded = false,
                    Cancelled = false,
                    Status = "failed",
                    Amount = total,
                    Currency = settings.Currency,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected payment processing error. CustomerName: {CustomerName}, CartItemCount: {CartItemCount}, TotalValue: {TotalValue}",
                    order.Name,
                    cart.Lines.Sum(l => l.Quantity),
                    total);

                return new PaymentResult
                {
                    Succeeded = false,
                    Cancelled = false,
                    Status = "failed",
                    Amount = total,
                    Currency = settings.Currency,
                    ErrorMessage = "Unexpected payment error"
                };
            }
        }
    }
}