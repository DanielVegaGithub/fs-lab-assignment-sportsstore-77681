namespace SportsStore.Models
{
    public interface IPaymentService
    {
        PaymentResult ProcessPayment(Order order, Cart cart);
    }
}