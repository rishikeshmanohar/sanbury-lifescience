namespace SanburyLifeScience.Web.Services;

public interface IPaymentGateway
{
    Task<string> CreatePaymentAsync(int orderId, decimal amount, string customerEmail);
    Task<bool> VerifyPaymentAsync(string paymentReference);
}