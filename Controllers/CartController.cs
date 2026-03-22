using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Services;

namespace SanburyLifeScience.Web.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICartService _cartService;

    public CartController(AppDbContext db, ICartService cartService)
    {
        _db = db;
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        return View(_cartService.GetCart());
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product == null) return NotFound();

        _cartService.AddToCart(new CartItemDto
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            GstPercent = product.GstPercent,
            Quantity = quantity
        });

        return RedirectToAction("Index");
    }

    public IActionResult Remove(int productId)
    {
        _cartService.RemoveFromCart(productId);
        return RedirectToAction("Index");
    }
}