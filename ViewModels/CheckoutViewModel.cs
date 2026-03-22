using System.ComponentModel.DataAnnotations;

namespace SanburyLifeScience.Web.ViewModels;

public class CheckoutViewModel
{
    [Required]
    public string BillingAddress { get; set; } = string.Empty;
}

