using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class SetupTests : IntegrationTestBase
{
    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
        var response = await Client.GetAsync("/");

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Roles_ShouldBeSeeded()
    {
        var roleManager = GetService<RoleManager<IdentityRole>>();

        (await roleManager.RoleExistsAsync("masterAdmin")).Should().BeTrue();
        (await roleManager.RoleExistsAsync("admin")).Should().BeTrue();
        (await roleManager.RoleExistsAsync("premium")).Should().BeTrue();
        (await roleManager.RoleExistsAsync("normal")).Should().BeTrue();
    }

    [Fact]
    public async Task MasterAdmin_ShouldBeSeeded()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        masterAdmin.Should().NotBeNull();
        masterAdmin!.TwoFactorEnabled.Should().BeFalse();
        (await userManager.IsInRoleAsync(masterAdmin, "masterAdmin")).Should().BeTrue();
    }
}
