namespace SanburyLifeScience.Web.Services;

public interface ICartService
{
    List<CartItemDto> GetCart();
    void AddToCart(CartItemDto item);
    void RemoveFromCart(int productId);
    void ClearCart();
}