using EasyP2P.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

/// <summary>
/// Input model for editing an existing user.
/// </summary>
public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = "";

    [Display(Name = "Department")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "Role")]
    public UserRole Role { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
}