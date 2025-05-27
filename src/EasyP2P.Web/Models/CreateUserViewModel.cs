using EasyP2P.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;
/// <summary>
/// Input model for creating a new user.
/// </summary>
public class CreateUserViewModel
{
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

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please confirm the password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";

    [Display(Name = "Department")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "Role")]
    public UserRole Role { get; set; } = UserRole.Requestor;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; } = true;
}