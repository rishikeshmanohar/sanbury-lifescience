using System.Text.Json;
using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public class VyaparSyncService : IVyaparSyncService
{
    private readonly IWebHostEnvironment _env;

    public VyaparSyncService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task SyncInvoiceAsync(Order order)
    {
        var exportDir = Path.Combine(_env.WebRootPath, "exports");
        Directory.CreateDirectory(exportDir);

        var path = Path.Combine(exportDir, $"invoice_{order.Id}.json");
        var json = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }
}