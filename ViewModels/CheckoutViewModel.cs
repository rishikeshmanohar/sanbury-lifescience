using System.ComponentModel.DataAnnotations;

namespace SanburyLifeScience.Web.ViewModels;

public class CheckoutViewModel
{
    [Required]
    public string BillingAddress { get; set; } = string.Empty;

    public int? OrderId { get; set; }
    public string? GatewayOrderId { get; set; }
    public long? AmountInPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string? RazorpayKeyId { get; set; }
    public string? CustomerEmail { get; set; }
    public bool ReadyForPayment { get; set; }
    public string? PaymentInitError { get; set; }
}

