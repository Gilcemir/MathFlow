using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Configuration;
using System.ComponentModel.DataAnnotations;

namespace MathFlow.Pages.Admin.Users;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleService _roleService;

    public EditModel(UserManager<ApplicationUser> userManager, RoleService roleService)
    {
        _userManager = userManager;
        _roleService = roleService;
    }

    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> CurrentRoles { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> AvailableRoles { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string SelectedRole { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        UserId = user.Id;
        UserName = user.UserName!;
        Email = user.Email!;
        CurrentRoles = (await _roleService.GetUserRolesAsync(user.Id)).ToList();

        AvailableRoles = new List<SelectListItem>
        {
            new SelectListItem { Value = "normal", Text = "Normal" },
            new SelectListItem { Value = "premium", Text = "Premium" },
            new SelectListItem { Value = "admin", Text = "Admin" }
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id);
        }

        var result = await _roleService.AssignRoleAsync(id, Input.SelectedRole);

        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Role updated successfully.";
            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return await OnGetAsync(id);
    }
}
