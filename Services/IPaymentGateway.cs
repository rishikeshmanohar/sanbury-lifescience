namespace SanburyLifeScience.Web.Services;

public interface IPaymentGateway
{
    Task<PaymentOrderResult> CreateOrderAsync(int orderId, decimal amountInInr, string customerEmail);
    bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string gatewaySignature);
    bool VerifyWebhookSignature(string rawRequestBody, string webhookSignature);
    Task<bool> IsPaymentCapturedAsync(string gatewayPaymentId);
}

public sealed class PaymentOrderResult
{
    public required string GatewayOrderId { get; init; }
    public required long AmountInPaise { get; init; }
    public string Currency { get; init; } = "INR";
}
