using System.ComponentModel.DataAnnotations;
using SanburyLifeScience.Web.Models;

namespace SanburyLifeScience.Web.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? GstNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public UserRole Role { get; set; }
}