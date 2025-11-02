using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using MathFlow.Application.Services.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.UnitTests.Identity;

public class RoleServiceTests : IdentityTestBase
{
    [Fact]
    public async Task AssignRoleAsync_ShouldAssignRole_WhenUserExists()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

        userManager.FindByIdAsync("user123")
            .Returns(user);
        
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { Roles.Normal });
        
        userManager.AddToRoleAsync(user, Roles.Premium)
            .Returns(IdentityResult.Success);

        var service = new RoleService(userManager, logger);

        var result = await service.AssignRoleAsync("user123", Roles.Premium);

        result.Succeeded.Should().BeTrue();
        await userManager.Received(1).AddToRoleAsync(user, Roles.Premium);
        await userManager.DidNotReceive().RemoveFromRolesAsync(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>());
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnSuccess_WhenUserAlreadyHasRole()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

        userManager.FindByIdAsync("user123")
            .Returns(user);
        
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { Roles.Premium });

        var service = new RoleService(userManager, logger);

        var result = await service.AssignRoleAsync("user123", Roles.Premium);

        result.Succeeded.Should().BeTrue();
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        userManager.FindByIdAsync(Arg.Any<string>())
            .Returns((ApplicationUser)null!);

        var service = new RoleService(userManager, logger);

        var result = await service.AssignRoleAsync("nonexistent", "premium");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Description == "User not found");
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFailure_WhenRoleIsInvalid()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var service = new RoleService(userManager, logger);

        var result = await service.AssignRoleAsync("user123", "invalidRole");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Description.Contains("Invalid role"));
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnRoles_WhenUserExists()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var user = new ApplicationUser { Id = "user123" };

        userManager.FindByIdAsync("user123")
            .Returns(user);
        
        userManager.GetRolesAsync(user)
            .Returns(new List<string> { "premium", "admin" });

        var service = new RoleService(userManager, logger);

        var roles = await service.GetUserRolesAsync("user123");

        roles.Should().HaveCount(2);
        roles.Should().Contain("premium");
        roles.Should().Contain("admin");
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnEmptyList_WhenUserNotFound()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        userManager.FindByIdAsync(Arg.Any<string>())
            .Returns((ApplicationUser)null!);

        var service = new RoleService(userManager, logger);

        var roles = await service.GetUserRolesAsync("nonexistent");

        roles.Should().BeEmpty();
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldThrowException_WhenUserIdIsEmpty()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var service = new RoleService(userManager, logger);

        var act = async () => await service.AssignRoleAsync("", "premium");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldThrowException_WhenRoleNameIsEmpty()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var service = new RoleService(userManager, logger);

        var act = async () => await service.AssignRoleAsync("user123", "");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Role name cannot be empty*");
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldThrowException_WhenUserIdIsEmpty()
    {
        var userManager = CreateMockUserManager();
        var logger = CreateMockLogger<RoleService>();

        var service = new RoleService(userManager, logger);

        var act = async () => await service.GetUserRolesAsync("");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User ID cannot be empty*");
    }
}
