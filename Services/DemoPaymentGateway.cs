namespace SanburyLifeScience.Web.Services;

public class DemoPaymentGateway : IPaymentGateway
{
    public Task<string> CreatePaymentAsync(int orderId, decimal amount, string customerEmail)
    {
        return Task.FromResult($"DEMO-PAY-{orderId}-{Guid.NewGuid():N}");
    }

    public Task<bool> VerifyPaymentAsync(string paymentReference)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(paymentReference));
    }
}