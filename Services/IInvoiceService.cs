using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public interface IInvoiceService
{
    Task<Order> CreateOrderFromCartAsync(int userId, string billingAddress, List<CartItemDto> cartItems);
}