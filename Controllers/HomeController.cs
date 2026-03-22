using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SanburyLifeScience.Web.Data;

namespace SanburyLifeScience.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Category.Contains(q) || x.Brand.Contains(q));

        var products = await query.OrderBy(x => x.Name).ToListAsync();
        return View(products);
    }
}