using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class MasterAdminProtectionTests : IntegrationTestBase
{
    [Fact]
    public async Task MasterAdmin_ShouldNotHave2FAEnabled()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        masterAdmin.Should().NotBeNull();
        masterAdmin!.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task MasterAdmin_ShouldHaveMasterAdminRole()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        masterAdmin.Should().NotBeNull();
        var roles = await userManager.GetRolesAsync(masterAdmin!);
        roles.Should().Contain("masterAdmin");
    }

    [Fact]
    public async Task RemoveRole_CannotRemoveMasterAdminRole()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        var result = await roleService.RemoveRoleAsync(masterAdmin!.Id, "masterAdmin");

        result.Succeeded.Should().BeFalse();
        var roles = await userManager.GetRolesAsync(masterAdmin);
        roles.Should().Contain("masterAdmin");
    }

    [Fact]
    public async Task MasterAdmin_CanHaveAdditionalRoles()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        var result = await roleService.AssignRoleAsync(masterAdmin!.Id, "admin");

        result.Succeeded.Should().BeTrue();
        var roles = await userManager.GetRolesAsync(masterAdmin);
        roles.Should().Contain("masterAdmin");
        roles.Should().Contain("admin");
    }
}
