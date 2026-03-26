using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SanburyLifeScience.Web.Services;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly RazorpayOptions _options;

    public RazorpayPaymentGateway(HttpClient httpClient, IOptions<RazorpayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (_httpClient.BaseAddress == null)
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<PaymentOrderResult> CreateOrderAsync(int orderId, decimal amountInInr, string customerEmail)
    {
        EnsureConfigured();

        if (amountInInr <= 0)
            throw new InvalidOperationException("Razorpay payment amount must be greater than zero.");

        var amountInPaise = (long)Math.Round(amountInInr * 100m, MidpointRounding.AwayFromZero);
        var receipt = $"order_{orderId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var payload = new
        {
            amount = amountInPaise,
            currency = "INR",
            receipt,
            notes = new
            {
                app_order_id = orderId.ToString(),
                customer_email = customerEmail
            }
        };

        using var request = BuildRequest(HttpMethod.Post, "v1/orders");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Razorpay order creation failed: {response.StatusCode} {body}");

        using var json = JsonDocument.Parse(body);
        var gatewayOrderId = json.RootElement.GetProperty("id").GetString();
        var currency = json.RootElement.TryGetProperty("currency", out var currencyElement)
            ? currencyElement.GetString() ?? "INR"
            : "INR";

        if (string.IsNullOrWhiteSpace(gatewayOrderId))
            throw new InvalidOperationException("Razorpay returned an empty order id.");

        return new PaymentOrderResult
        {
            GatewayOrderId = gatewayOrderId,
            AmountInPaise = amountInPaise,
            Currency = currency
        };
    }

    public bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string gatewaySignature)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(gatewayOrderId) ||
            string.IsNullOrWhiteSpace(gatewayPaymentId) ||
            string.IsNullOrWhiteSpace(gatewaySignature))
            return false;

        var payload = $"{gatewayOrderId}|{gatewayPaymentId}";
        var expected = ComputeHexHmac(_options.KeySecret, payload);
        return SecureEquals(expected, gatewaySignature);
    }

    public bool VerifyWebhookSignature(string rawRequestBody, string webhookSignature)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            return false;

        if (string.IsNullOrWhiteSpace(rawRequestBody) || string.IsNullOrWhiteSpace(webhookSignature))
            return false;

        var expected = ComputeHexHmac(_options.WebhookSecret, rawRequestBody);
        return SecureEquals(expected, webhookSignature);
    }

    public async Task<bool> IsPaymentCapturedAsync(string gatewayPaymentId)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(gatewayPaymentId))
            return false;

        using var request = BuildRequest(HttpMethod.Get, $"v1/payments/{gatewayPaymentId}");
        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return false;

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var status = json.RootElement.TryGetProperty("status", out var statusElement)
            ? statusElement.GetString()
            : null;

        return string.Equals(status, "captured", StringComparison.OrdinalIgnoreCase);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string relativeUrl)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.KeyId}:{_options.KeySecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        return request;
    }

    private static string ComputeHexHmac(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var message = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(message);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool SecureEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left.Trim());
        var rightBytes = Encoding.UTF8.GetBytes(right.Trim());
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private void EnsureConfigured()
    {
        if (LooksLikePlaceholder(_options.KeyId) || LooksLikePlaceholder(_options.KeySecret))
            throw new InvalidOperationException("Razorpay credentials are missing or placeholder values. Set real Razorpay:KeyId and Razorpay:KeySecret.");

        if (!_options.KeyId.StartsWith("rzp_", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Razorpay KeyId format looks invalid. It should start with 'rzp_'.");
    }

    private static bool LooksLikePlaceholder(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        var value = input.Trim();
        return value.Contains("...", StringComparison.Ordinal) ||
               value.Contains("your_", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("xxxxx", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("...", StringComparison.Ordinal) ||
               value.Equals("test", StringComparison.OrdinalIgnoreCase);
    }
}
