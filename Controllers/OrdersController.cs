using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Services;
using SanburyLifeScience.Web.ViewModels;

namespace SanburyLifeScience.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ICartService _cartService;
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IVyaparSyncService _vyaparSyncService;
    private readonly AppDbContext _db;

    public OrdersController(
        ICartService cartService,
        IInvoiceService invoiceService,
        IPaymentGateway paymentGateway,
        IVyaparSyncService vyaparSyncService,
        AppDbContext db)
    {
        _cartService = cartService;
        _invoiceService = invoiceService;
        _paymentGateway = paymentGateway;
        _vyaparSyncService = vyaparSyncService;
        _db = db;
    }

    public IActionResult Checkout() => View();

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var cart = _cartService.GetCart();

        if (!cart.Any())
        {
            ModelState.AddModelError("", "Cart is empty");
            return View(model);
        }

        var order = await _invoiceService.CreateOrderFromCartAsync(userId, model.BillingAddress, cart);
        var paymentRef = await _paymentGateway.CreatePaymentAsync(order.Id, order.GrandTotal, email);

        order.PaymentReference = paymentRef;
        order.PaymentStatus = "Initiated";
        await _db.SaveChangesAsync();

        await _vyaparSyncService.SyncInvoiceAsync(order);
        _cartService.ClearCart();

        return RedirectToAction("Details", new { id = order.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _db.Orders
            .Where(x => x.AppUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(orders);
    }
}