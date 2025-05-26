using EasyP2P.Web.Enums;
using Microsoft.AspNetCore.Identity;

namespace EasyP2P.Web.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public UserRole Role { get; set; } = UserRole.Requestor;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
