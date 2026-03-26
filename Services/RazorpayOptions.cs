namespace SanburyLifeScience.Web.Services;

public class RazorpayOptions
{
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.razorpay.com";
}
