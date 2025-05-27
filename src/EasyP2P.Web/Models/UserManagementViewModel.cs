using EasyP2P.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

/// <summary>
/// View model for displaying user information in the management interface.
/// </summary>
public class UserManagementViewModel
{
    public string Id { get; set; } = "";

    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Department")]
    public string? Department { get; set; }

    [Display(Name = "Role")]
    public UserRole Role { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Phone Confirmed")]
    public bool PhoneNumberConfirmed { get; set; }

    [Display(Name = "Two Factor Enabled")]
    public bool TwoFactorEnabled { get; set; }

    [Display(Name = "Lockout End")]
    public DateTimeOffset? LockoutEnd { get; set; }

    [Display(Name = "Lockout Enabled")]
    public bool LockoutEnabled { get; set; }

    [Display(Name = "Access Failed Count")]
    public int AccessFailedCount { get; set; }

    // Computed properties for UI
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
    public string StatusText => IsActive ? "Active" : "Inactive";

    public string RoleBadgeClass => Role switch
    {
        UserRole.Administrator => "bg-danger",
        UserRole.Purchaser => "bg-primary",
        UserRole.Approver => "bg-warning",
        UserRole.Requestor => "bg-info",
        _ => "bg-secondary"
    };

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;
}