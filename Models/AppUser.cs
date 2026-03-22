using System.ComponentModel.DataAnnotations;

namespace SanburyLifeScience.Web.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? GstNumber { get; set; }

    [MaxLength(20)]
    public string? LicenseNumber { get; set; }

    public UserRole Role { get; set; }
    public bool IsApproved { get; set; } = true;
}