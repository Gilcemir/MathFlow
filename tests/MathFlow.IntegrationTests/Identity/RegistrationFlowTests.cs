using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class RegistrationFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com",
            TwoFactorEnabled = false,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Test@1234");
        await userManager.AddToRoleAsync(user, "normal");

        result.Succeeded.Should().BeTrue();
        var createdUser = await userManager.FindByEmailAsync("test@example.com");
        createdUser.Should().NotBeNull();
        createdUser!.TwoFactorEnabled.Should().BeFalse();
        createdUser!.EmailConfirmed.Should().BeTrue();
        (await userManager.IsInRoleAsync(createdUser, "normal")).Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldFail()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "testuser2",
            Email = "test2@example.com"
        };
        var result = await userManager.CreateAsync(user, "weak");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldFail()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user1 = new ApplicationUser
        {
            UserName = "user1",
            Email = "duplicate@example.com"
        };
        await userManager.CreateAsync(user1, "Test@1234");

        var user2 = new ApplicationUser
        {
            UserName = "user2",
            Email = "duplicate@example.com"
        };
        var result = await userManager.CreateAsync(user2, "Test@1234");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code.Contains("Duplicate"));
    }
}
