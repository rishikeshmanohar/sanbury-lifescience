using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public interface IVyaparSyncService
{
    Task SyncInvoiceAsync(Order order);
}