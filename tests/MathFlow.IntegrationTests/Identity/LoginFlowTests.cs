using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class LoginFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task User_CanBeCreatedWithValidPassword()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "logintest",
            Email = "logintest@example.com",
            TwoFactorEnabled = false,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Test@1234");

        result.Succeeded.Should().BeTrue();
        var createdUser = await userManager.FindByEmailAsync("logintest@example.com");
        createdUser.Should().NotBeNull();
    }

    [Fact]
    public async Task PasswordValidation_ShouldWorkCorrectly()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "pwcheck",
            Email = "pwcheck@example.com"
        };
        await userManager.CreateAsync(user, "Test@1234");

        var validPassword = await userManager.CheckPasswordAsync(user, "Test@1234");
        var invalidPassword = await userManager.CheckPasswordAsync(user, "WrongPassword");

        validPassword.Should().BeTrue();
        invalidPassword.Should().BeFalse();
    }

    [Fact]
    public async Task MasterAdmin_ShouldNotHave2FAEnabled()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();
        var masterAdmin = await userManager.FindByEmailAsync("admin@mathflow.local");

        masterAdmin.Should().NotBeNull();
        masterAdmin!.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task User_EmailConfirmation_CanBeSet()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "emailtest",
            Email = "emailtest@example.com",
            EmailConfirmed = false
        };
        await userManager.CreateAsync(user, "Test@1234");

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        var updatedUser = await userManager.FindByEmailAsync("emailtest@example.com");
        updatedUser!.EmailConfirmed.Should().BeTrue();
    }
}
