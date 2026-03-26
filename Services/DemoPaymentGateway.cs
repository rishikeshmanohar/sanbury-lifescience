namespace SanburyLifeScience.Web.Services;

public class DemoPaymentGateway : IPaymentGateway
{
    public Task<PaymentOrderResult> CreateOrderAsync(int orderId, decimal amountInInr, string customerEmail)
    {
        return Task.FromResult(new PaymentOrderResult
        {
            GatewayOrderId = $"DEMO-ORDER-{orderId}-{Guid.NewGuid():N}",
            AmountInPaise = (long)Math.Round(amountInInr * 100m),
            Currency = "INR"
        });
    }

    public bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string gatewaySignature)
    {
        return !string.IsNullOrWhiteSpace(gatewayOrderId) &&
               !string.IsNullOrWhiteSpace(gatewayPaymentId) &&
               !string.IsNullOrWhiteSpace(gatewaySignature);
    }

    public bool VerifyWebhookSignature(string rawRequestBody, string webhookSignature)
    {
        return !string.IsNullOrWhiteSpace(rawRequestBody) && !string.IsNullOrWhiteSpace(webhookSignature);
    }

    public Task<bool> IsPaymentCapturedAsync(string gatewayPaymentId)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(gatewayPaymentId));
    }
}
