using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Models;
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
    private readonly RazorpayOptions _razorpayOptions;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        ICartService cartService,
        IInvoiceService invoiceService,
        IPaymentGateway paymentGateway,
        IVyaparSyncService vyaparSyncService,
        IOptions<RazorpayOptions> razorpayOptions,
        IWebHostEnvironment env,
        ILogger<OrdersController> logger,
        AppDbContext db)
    {
        _cartService = cartService;
        _invoiceService = invoiceService;
        _paymentGateway = paymentGateway;
        _vyaparSyncService = vyaparSyncService;
        _db = db;
        _razorpayOptions = razorpayOptions.Value;
        _env = env;
        _logger = logger;
    }

    public IActionResult Checkout() => View(new CheckoutViewModel());

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

        PaymentOrderResult paymentOrder;
        try
        {
            paymentOrder = await _paymentGateway.CreateOrderAsync(order.Id, order.GrandTotal, email);
        }
        catch (Exception ex)
        {
            order.PaymentStatus = "Failed";
            order.Status = "PaymentInitFailed";
            await _db.SaveChangesAsync();

            _logger.LogError(ex, "Razorpay payment initialization failed for order {OrderId}", order.Id);
            model.PaymentInitError = BuildCheckoutInitError(ex);
            ModelState.AddModelError("", model.PaymentInitError);
            return View(model);
        }

        order.PaymentReference = paymentOrder.GatewayOrderId;
        order.PaymentStatus = "Initiated";
        order.Status = "PaymentPending";
        await _db.SaveChangesAsync();

        model.OrderId = order.Id;
        model.GatewayOrderId = paymentOrder.GatewayOrderId;
        model.AmountInPaise = paymentOrder.AmountInPaise;
        model.Currency = paymentOrder.Currency;
        model.CustomerEmail = email;
        model.RazorpayKeyId = _razorpayOptions.KeyId;
        model.ReadyForPayment = true;

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RazorpayCallback(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
    {
        if (string.IsNullOrWhiteSpace(razorpay_order_id) ||
            string.IsNullOrWhiteSpace(razorpay_payment_id) ||
            string.IsNullOrWhiteSpace(razorpay_signature))
            return RedirectToAction(nameof(MyOrders));

        var order = await FindOrderByGatewayOrderIdAsync(razorpay_order_id);
        if (order == null) return RedirectToAction(nameof(MyOrders));

        var isSignatureValid = _paymentGateway.VerifyCheckoutSignature(
            razorpay_order_id,
            razorpay_payment_id,
            razorpay_signature);

        if (!isSignatureValid)
        {
            await MarkOrderFailedAsync(order, razorpay_order_id, razorpay_payment_id, "Signature verification failed");
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        var isCaptured = await _paymentGateway.IsPaymentCapturedAsync(razorpay_payment_id);
        if (isCaptured)
        {
            await MarkOrderPaidAsync(order, razorpay_order_id, razorpay_payment_id);
            ClearCartIfCurrentUserOwnsOrder(order);
        }
        else
        {
            await MarkOrderVerificationPendingAsync(order, razorpay_order_id, razorpay_payment_id);
        }

        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RazorpayWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["X-Razorpay-Signature"].ToString();

        if (!_paymentGateway.VerifyWebhookSignature(payload, signature))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(payload))
            return Ok();

        using var json = JsonDocument.Parse(payload);

        var eventName = GetJsonValue(json.RootElement, "event");
        var gatewayOrderId = GetJsonValue(json.RootElement, "payload", "payment", "entity", "order_id");
        var gatewayPaymentId = GetJsonValue(json.RootElement, "payload", "payment", "entity", "id");
        var paymentStatus = GetJsonValue(json.RootElement, "payload", "payment", "entity", "status");

        if (string.IsNullOrWhiteSpace(gatewayOrderId) || string.IsNullOrWhiteSpace(gatewayPaymentId))
            return Ok();

        var order = await FindOrderByGatewayOrderIdAsync(gatewayOrderId);
        if (order == null) return Ok();

        if (string.Equals(eventName, "payment.captured", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(paymentStatus, "captured", StringComparison.OrdinalIgnoreCase))
        {
            await MarkOrderPaidAsync(order, gatewayOrderId, gatewayPaymentId);
            return Ok();
        }

        if (string.Equals(eventName, "payment.failed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(paymentStatus, "failed", StringComparison.OrdinalIgnoreCase))
        {
            await MarkOrderFailedAsync(order, gatewayOrderId, gatewayPaymentId, "Payment failed");
        }

        return Ok();
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

    private async Task<Order?> FindOrderByGatewayOrderIdAsync(string gatewayOrderId)
    {
        var order = await _db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.PaymentReference == gatewayOrderId);

        if (order != null)
            return order;

        return await _db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.PaymentReference != null && x.PaymentReference.StartsWith(gatewayOrderId + "|"));
    }

    private async Task MarkOrderPaidAsync(Order order, string gatewayOrderId, string gatewayPaymentId)
    {
        var wasAlreadyPaid = string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase);

        order.PaymentStatus = "Paid";
        order.Status = "Confirmed";
        order.PaymentReference = $"{gatewayOrderId}|{gatewayPaymentId}";

        await _db.SaveChangesAsync();

        if (!wasAlreadyPaid)
            await _vyaparSyncService.SyncInvoiceAsync(order);
    }

    private async Task MarkOrderVerificationPendingAsync(Order order, string gatewayOrderId, string gatewayPaymentId)
    {
        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            return;

        order.PaymentStatus = "VerificationPending";
        order.Status = "PaymentVerificationPending";
        order.PaymentReference = $"{gatewayOrderId}|PENDING:{gatewayPaymentId}";
        await _db.SaveChangesAsync();
    }

    private async Task MarkOrderFailedAsync(Order order, string gatewayOrderId, string gatewayPaymentId, string reason)
    {
        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            return;

        order.PaymentStatus = "Failed";
        order.Status = "PaymentFailed";
        order.PaymentReference = $"{gatewayOrderId}|FAILED:{gatewayPaymentId}|{reason}";
        await _db.SaveChangesAsync();
    }

    private void ClearCartIfCurrentUserOwnsOrder(Order order)
    {
        if (User.Identity?.IsAuthenticated != true)
            return;

        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out var userId) && userId == order.AppUserId)
            _cartService.ClearCart();
    }

    private static string? GetJsonValue(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out var next))
                return null;

            current = next;
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
    }

    private string BuildCheckoutInitError(Exception ex)
    {
        if (ex.Message.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("credentials are missing", StringComparison.OrdinalIgnoreCase))
        {
            return "Razorpay keys are not configured correctly. Use real KeyId/KeySecret (not placeholders) and restart the app.";
        }

        if (_env.IsDevelopment())
            return $"Razorpay init failed: {ex.Message}";

        return "Unable to initialize Razorpay payment. Please verify Razorpay keys and try again.";
    }
}
