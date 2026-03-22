using Microsoft.EntityFrameworkCore;
using SanburyLifeScience.Web.Data;
using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AppUser?> ValidateUserAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null) return null;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<AppUser> RegisterAsync(AppUser user, string password)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}