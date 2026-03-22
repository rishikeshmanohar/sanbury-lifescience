using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public interface IAuthService
{
    Task<AppUser?> ValidateUserAsync(string email, string password);
    Task<AppUser> RegisterAsync(AppUser user, string password);
}