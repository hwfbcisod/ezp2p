using EasyP2P.Web.Data.Models;
using EasyP2P.Web.Enums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EasyP2P.Web.Services;

public interface IUserContextService
{
    string GetCurrentUser();
    string GetCurrentUserId();
    UserRole GetCurrentUserRole();
    bool HasPermission(string action);
    bool CanTransition(string fromState, string toState, string entityType);
    Task<bool> IsCurrentUserAsync(string userId);
    string GetCurrentUserDepartment();
    bool CanViewAllDepartments();
    bool CanViewAllUsers();
    List<string> GetAccessibleDepartments();
    bool CanViewEntity(string entityType, string createdBy, string department = null);

}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ILogger<UserContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    public string GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Identity?.Name ?? "Anonymous";
    }

    public string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    }

    public UserRole GetCurrentUserRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return UserRole.Requestor;

        var roleClaimValue = user.FindFirstValue("UserRole");
        if (Enum.TryParse<UserRole>(roleClaimValue, out var role))
            return role;

        return UserRole.Requestor;
    }

    public bool HasPermission(string action)
    {
        var role = GetCurrentUserRole();
        return PermissionMatrix.HasPermission(role, action);
    }

    public bool CanTransition(string fromState, string toState, string entityType)
    {
        var role = GetCurrentUserRole();
        return PermissionMatrix.CanTransition(role, fromState, toState, entityType);
    }

    public Task<bool> IsCurrentUserAsync(string userId)
    {
        var currentUserId = GetCurrentUserId();
        return Task.FromResult(currentUserId == userId);
    }

    public string GetCurrentUserDepartment()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue("Department") ?? "";
    }

    public bool CanViewAllDepartments()
    {
        var role = GetCurrentUserRole();
        return role == UserRole.Administrator || role == UserRole.Purchaser;
    }

    public bool CanViewAllUsers()
    {
        var role = GetCurrentUserRole();
        return role == UserRole.Administrator;
    }

    public List<string> GetAccessibleDepartments()
    {
        var role = GetCurrentUserRole();
        var currentDepartment = GetCurrentUserDepartment();

        return role switch
        {
            UserRole.Administrator => new List<string>(), // Empty = all departments
            UserRole.Purchaser => new List<string>(), // Empty = all departments
            UserRole.Approver => new List<string> { currentDepartment }, // Only own department
            UserRole.Requestor => new List<string> { currentDepartment }, // Only own department
            _ => new List<string> { currentDepartment }
        };
    }

    public bool CanViewEntity(string entityType, string createdBy, string department = null)
    {
        var role = GetCurrentUserRole();
        var currentUser = GetCurrentUser();
        var currentDepartment = GetCurrentUserDepartment();

        return role switch
        {
            UserRole.Administrator => true, // Admin sees everything
            UserRole.Purchaser => true, // Purchaser sees everything
            UserRole.Approver => department == currentDepartment, // Only same department
            UserRole.Requestor => createdBy == currentUser, // Only own items
            _ => false
        };
    }
}