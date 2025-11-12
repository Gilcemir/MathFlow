using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Configuration;
using MathFlow.Application.Services.Identity;

namespace MathFlow.Pages.Admin.Users;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserService _userService;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        UserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = roles.ToList()
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToPage();
        }

        try
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An unexpected error occurred while deleting the user. Message: {ex.Message}";
        }

        return RedirectToPage();
    }
}
