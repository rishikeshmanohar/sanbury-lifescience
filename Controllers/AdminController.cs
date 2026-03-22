using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.ProductCount = await _db.Products.CountAsync();
        ViewBag.OrderCount = await _db.Orders.CountAsync();
        ViewBag.UserCount = await _db.Users.CountAsync();
        return View();
    }

    public async Task<IActionResult> Products()
    {
        return View(await _db.Products.OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult CreateProduct() => View(new Product());

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return RedirectToAction("Products");
    }
}