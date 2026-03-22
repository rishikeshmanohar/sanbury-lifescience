using System.Text.Json;

namespace SanburyLifeScience.Web.Services;

public class CartService : ICartService
{
    private const string CartKey = "CART";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItemDto> GetCart()
    {
        var session = _httpContextAccessor.HttpContext!.Session;
        var json = session.GetString(CartKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<CartItemDto>()
            : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
    }

    public void AddToCart(CartItemDto item)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existing == null)
            cart.Add(item);
        else
            existing.Quantity += item.Quantity;

        Save(cart);
    }

    public void RemoveFromCart(int productId)
    {
        var cart = GetCart();
        cart.RemoveAll(x => x.ProductId == productId);
        Save(cart);
    }

    public void ClearCart() => Save(new List<CartItemDto>());

    private void Save(List<CartItemDto> cart)
    {
        _httpContextAccessor.HttpContext!.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }
}