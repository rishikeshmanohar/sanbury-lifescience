using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order> CreateOrderFromCartAsync(int userId, string billingAddress, List<CartItemDto> cartItems)
    {
        var order = new Order
        {
            AppUserId = userId,
            BillingAddress = billingAddress,
            Status = "Created",
            PaymentStatus = "Pending"
        };

        foreach (var item in cartItems)
        {
            var lineBase = item.Price * item.Quantity;
            var lineGst = lineBase * item.GstPercent / 100m;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                GstPercent = item.GstPercent,
                LineTotal = lineBase + lineGst
            });

            order.SubTotal += lineBase;
            order.GstAmount += lineGst;
        }

        order.GrandTotal = order.SubTotal + order.GstAmount;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }
}