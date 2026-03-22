namespace SanburyLifeScience.Web.Models;

public class Order
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    public decimal SubTotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public string PaymentStatus { get; set; } = "Pending";
    public string? PaymentReference { get; set; }
    public string? BillingAddress { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}