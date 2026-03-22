using System.ComponentModel.DataAnnotations;

namespace SanburyLifeScience.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public decimal Price { get; set; }
    public decimal GstPercent { get; set; }
    public int StockQuantity { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }
}