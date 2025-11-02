using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.IntegrationTests.Identity;

public class LockoutTests : IntegrationTestBase
{
    [Fact]
    public async Task User_WithLockoutEnabled_ShouldBeConfigured()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "lockouttest",
            Email = "lockouttest@example.com",
            LockoutEnabled = true
        };
        await userManager.CreateAsync(user, "Test@1234");

        var createdUser = await userManager.FindByEmailAsync("lockouttest@example.com");
        createdUser.Should().NotBeNull();
        createdUser!.LockoutEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task AccessFailedCount_ShouldIncrementOnFailedLogin()
    {
        var userManager = GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "failcount",
            Email = "failcount@example.com",
            LockoutEnabled = true
        };
        await userManager.CreateAsync(user, "Test@1234");

        await userManager.AccessFailedAsync(user);
        await userManager.AccessFailedAsync(user);

        var updatedUser = await userManager.FindByEmailAsync("failcount@example.com");
        updatedUser!.AccessFailedCount.Should().BeGreaterThan(0);
    }
}
