using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class RoleManagementTests : IntegrationTestBase
{
    [Fact]
    public async Task AssignRole_ShouldAddRoleToUser()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var user = new ApplicationUser
        {
            UserName = "roletest",
            Email = "roletest@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "normal");

        var result = await roleService.AssignRoleAsync(user.Id, "premium");

        result.Succeeded.Should().BeTrue();
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().Contain("premium");
        roles.Should().Contain("normal");
    }

    [Fact]
    public async Task RemoveRole_ShouldRemoveRoleFromUser()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var user = new ApplicationUser
        {
            UserName = "removerole",
            Email = "removerole@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "normal");
        await userManager.AddToRoleAsync(user, "premium");

        var result = await roleService.RemoveRoleAsync(user.Id, "normal");

        result.Succeeded.Should().BeTrue();
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().Contain("premium");
        roles.Should().NotContain("normal");
    }

    [Fact]
    public async Task RemoveRole_CannotRemoveMasterAdmin()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var user = new ApplicationUser
        {
            UserName = "mastertest",
            Email = "mastertest@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "masterAdmin");
        await userManager.AddToRoleAsync(user, "admin");

        var result = await roleService.RemoveRoleAsync(user.Id, "masterAdmin");

        result.Succeeded.Should().BeFalse();
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().Contain("masterAdmin");
        roles.Should().Contain("admin");
    }

    [Fact]
    public async Task GetUserRoles_ShouldReturnAllRoles()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "multirole",
            Email = "multirole@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "normal");
        await userManager.AddToRoleAsync(user, "premium");

        var roles = await userManager.GetRolesAsync(user);

        roles.Should().HaveCount(2);
        roles.Should().Contain("normal");
        roles.Should().Contain("premium");
    }

    [Fact]
    public async Task AssignRole_WithInvalidRole_ShouldFail()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var user = new ApplicationUser
        {
            UserName = "invalidrole",
            Email = "invalidrole@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");

        var result = await roleService.AssignRoleAsync(user.Id, "nonexistent");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AssignRole_AlreadyHasRole_ShouldSucceed()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var roleService = GetService<RoleService>();

        var user = new ApplicationUser
        {
            UserName = "duplicaterole",
            Email = "duplicaterole@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "normal");

        var result = await roleService.AssignRoleAsync(user.Id, "normal");

        result.Succeeded.Should().BeTrue();
        var roles = await userManager.GetRolesAsync(user);
        roles.Should().ContainSingle("normal");
    }
}
