namespace SanburyLifeScience.Web.Services;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal GstPercent { get; set; }
    public int Quantity { get; set; }
}