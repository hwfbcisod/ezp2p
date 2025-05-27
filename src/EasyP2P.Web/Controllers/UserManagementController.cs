using EasyP2P.Web.Attributes;
using EasyP2P.Web.Data.Models;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyP2P.Web.Controllers;

[Authorize]
[RequiresPermission("ManageUsers")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        ILogger<UserManagementController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in _userManager.Users)
        {
            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        return View(userViewModels.OrderBy(x => x.FirstName).ThenBy(x => x.LastName));
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new UserManagementViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = GetRoleSelectList();
        ViewBag.Departments = GetDepartmentSelectList();
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Department = model.Department,
                Role = model.Role,
                IsActive = model.IsActive,
                EmailConfirmed = model.EmailConfirmed,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Administrator created user {Email} with role {Role}",
                    user.Email, user.Role);

                TempData["SuccessMessage"] = $"User '{user.Email}' created successfully!";
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Roles = GetRoleSelectList();
        ViewBag.Departments = GetDepartmentSelectList();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber
        };

        ViewBag.Roles = GetRoleSelectList();
        ViewBag.Departments = GetDepartmentSelectList();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            user.Email = model.Email;
            user.UserName = model.Email; // Keep username and email in sync
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Department = model.Department;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Administrator updated user {Email}", user.Email);
                TempData["SuccessMessage"] = $"User '{user.Email}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Roles = GetRoleSelectList();
        ViewBag.Departments = GetDepartmentSelectList();
        return View(model);
    }

    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new UserManagementViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("Administrator deleted user {Email}", user.Email);
            TempData["SuccessMessage"] = $"User '{user.Email}' deleted successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var status = user.IsActive ? "activated" : "deactivated";
            _logger.LogInformation("Administrator {Status} user {Email}", status, user.Email);
            TempData["SuccessMessage"] = $"User '{user.Email}' {status} successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update user status. Please try again.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        // Generate a new temporary password
        var tempPassword = GenerateTemporaryPassword();

        // Remove existing password and set new one
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (removeResult.Succeeded)
        {
            var addResult = await _userManager.AddPasswordAsync(user, tempPassword);
            if (addResult.Succeeded)
            {
                _logger.LogInformation("Administrator reset password for user {Email}", user.Email);
                TempData["SuccessMessage"] = $"Password reset successfully! Temporary password: {tempPassword}";
                TempData["TempPassword"] = tempPassword;
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to set new password. Please try again.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to reset password. Please try again.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private List<SelectListItem> GetRoleSelectList()
    {
        return Enum.GetValues<UserRole>()
            .Select(role => new SelectListItem
            {
                Value = role.ToString(),
                Text = role.ToString()
            })
            .ToList();
    }

    private List<SelectListItem> GetDepartmentSelectList()
    {
        return new List<SelectListItem>
        {
            new() { Value = "", Text = "Select Department" },
            new() { Value = "IT", Text = "IT" },
            new() { Value = "Finance", Text = "Finance" },
            new() { Value = "HR", Text = "HR" },
            new() { Value = "Operations", Text = "Operations" },
            new() { Value = "Marketing", Text = "Marketing" },
            new() { Value = "Sales", Text = "Sales" },
            new() { Value = "Legal", Text = "Legal" },
            new() { Value = "Procurement", Text = "Procurement" }
        };
    }

    private string GenerateTemporaryPassword()
    {
        // Generate a secure temporary password
        var random = new Random();
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        const string special = "!@#$%";

        var password = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        password += special[random.Next(special.Length)];
        password += random.Next(10, 99).ToString();

        return password;
    }
}