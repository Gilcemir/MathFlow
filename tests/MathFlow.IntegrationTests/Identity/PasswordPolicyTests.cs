using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class PasswordPolicyTests : IntegrationTestBase
{
    [Theory]
    [InlineData("short", false)]
    [InlineData("nouppercase1!", false)]
    [InlineData("NoSpecial1", false)]
    [InlineData("Valid@1234", true)]
    [InlineData("AnotherValid!Pass1", true)]
    [InlineData("Complex@Pass123", true)]
    public async Task PasswordPolicy_ShouldEnforceRequirements(string password, bool shouldSucceed)
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = $"pwtest{Guid.NewGuid()}",
            Email = $"pwtest{Guid.NewGuid()}@example.com"
        };

        var result = await userManager.CreateAsync(user, password);

        result.Succeeded.Should().Be(shouldSucceed);
    }

    [Fact]
    public async Task PasswordPolicy_ShouldRequireMinimum8Characters()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "shortpw",
            Email = "shortpw@example.com"
        };

        var result = await userManager.CreateAsync(user, "Short1!");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code.Contains("PasswordTooShort"));
    }

    [Fact]
    public async Task PasswordPolicy_ShouldRequireUppercase()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "nouppercase",
            Email = "nouppercase@example.com"
        };

        var result = await userManager.CreateAsync(user, "nouppercase1!");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code.Contains("PasswordRequiresUpper"));
    }

    [Fact]
    public async Task PasswordPolicy_ShouldRequireNonAlphanumeric()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "nospecial",
            Email = "nospecial@example.com"
        };

        var result = await userManager.CreateAsync(user, "NoSpecial123");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code.Contains("PasswordRequiresNonAlphanumeric"));
    }
}
