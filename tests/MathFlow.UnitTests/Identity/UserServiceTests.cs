using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.UnitTests.Identity;

public class UserServiceTests : IdentityTestBase
{
    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUserWithNormalRole()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), MathFlow.Application.Services.Identity.Roles.Normal)
            .Returns(IdentityResult.Success);

        var service = new UserService(userManager, signInManager, logger);

        var result = await service.RegisterUserAsync("test@example.com", "Password@123", "testuser");

        result.Succeeded.Should().BeTrue();
        await userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => 
                u.Email == "test@example.com" && 
                u.TwoFactorEnabled == true), 
            "Password@123");
        await userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), MathFlow.Application.Services.Identity.Roles.Normal);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldReturnFailure_WhenCreationFails()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(errors));

        var service = new UserService(userManager, signInManager, logger);

        var result = await service.RegisterUserAsync("test@example.com", "weak", "testuser");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Description == "Password too weak");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTwoFactorRequired_WhenUserHas2FAEnabled()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var user = new ApplicationUser 
        { 
            Email = "test@example.com", 
            TwoFactorEnabled = true 
        };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);
        
        userManager.GetTwoFactorEnabledAsync(user)
            .Returns(true);
        
        userManager.CheckPasswordAsync(user, "Password@123")
            .Returns(true);

        var service = new UserService(userManager, signInManager, logger);

        var result = await service.LoginAsync("test@example.com", "Password@123", false);

        result.RequiresTwoFactor.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailed_WhenUserNotFound()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns((ApplicationUser)null!);

        var service = new UserService(userManager, signInManager, logger);

        var result = await service.LoginAsync("nonexistent@example.com", "Password@123", false);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyTwoFactorCodeAsync_ShouldReturnTrue_WhenCodeIsValid()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var user = new ApplicationUser { Email = "test@example.com" };

        userManager.FindByEmailAsync("test@example.com")
            .Returns(user);
        
        userManager.VerifyTwoFactorTokenAsync(
            user, 
            Arg.Any<string>(), 
            "123456")
            .Returns(true);
        
        signInManager.SignInAsync(user, true, null)
            .Returns(Task.CompletedTask);

        var service = new UserService(userManager, signInManager, logger);

        var result = await service.VerifyTwoFactorCodeAsync("test@example.com", "123456");

        result.Should().BeTrue();
        await signInManager.Received(1).SignInAsync(user, true, null);
    }

    [Fact]
    public async Task GetTwoFactorSetupKeyAsync_ShouldGenerateKey_WhenNotExists()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var user = new ApplicationUser { Id = "user123" };

        userManager.FindByIdAsync("user123")
            .Returns(user);
        
        userManager.GetAuthenticatorKeyAsync(user)
            .Returns((string)null!, "NEWKEY123");
        
        userManager.ResetAuthenticatorKeyAsync(user)
            .Returns(IdentityResult.Success);

        var service = new UserService(userManager, signInManager, logger);

        var key = await service.GetTwoFactorSetupKeyAsync("user123");

        key.Should().Be("NEWKEY123");
        await userManager.Received(1).ResetAuthenticatorKeyAsync(user);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowException_WhenEmailIsInvalid()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var service = new UserService(userManager, signInManager, logger);

        var act = async () => await service.RegisterUserAsync("invalidemail", "Password@123", "testuser");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid email format*");
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowException_WhenUserNameIsEmpty()
    {
        var userManager = CreateMockUserManager();
        var signInManager = CreateMockSignInManager(userManager);
        var logger = CreateMockLogger<UserService>();

        var service = new UserService(userManager, signInManager, logger);

        var act = async () => await service.RegisterUserAsync("test@example.com", "Password@123", "");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Username cannot be empty*");
    }
}
