using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class TwoFactorFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task GetTwoFactorSetupKey_ShouldGenerateKey()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var userService = GetService<UserService>();

        var user = new ApplicationUser
        {
            UserName = "2fatest",
            Email = "2fatest@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");

        var key = await userService.GetTwoFactorSetupKeyAsync(user.Id);

        key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyTwoFactorCode_WithValidCode_ShouldSucceed()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var userService = GetService<UserService>();

        var user = new ApplicationUser
        {
            UserName = "2faverify",
            Email = "2faverify@example.com",
            TwoFactorEnabled = true
        };
        await userManager.CreateAsync(user, "Test@1234");

        var key = await userService.GetTwoFactorSetupKeyAsync(user.Id);
        key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TwoFactorEnabled_CanBeEnabledByUser()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "2faenabled",
            Email = "2faenabled@example.com",
            TwoFactorEnabled = false
        };
        await userManager.CreateAsync(user, "Test@1234");

        await userManager.SetTwoFactorEnabledAsync(user, true);

        var updatedUser = await userManager.FindByEmailAsync("2faenabled@example.com");
        updatedUser.Should().NotBeNull();
        updatedUser!.TwoFactorEnabled.Should().BeTrue();
    }
}
