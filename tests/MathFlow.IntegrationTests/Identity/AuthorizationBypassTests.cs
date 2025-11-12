using FluentAssertions;
using System.Net;

namespace MathFlow.IntegrationTests.Identity;

public class AuthorizationBypassTests : IntegrationTestBase
{
    [Fact]
    public async Task UnauthenticatedUser_CannotAccessManagePages()
    {
        var response = await Client.GetAsync("/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/account/login");
    }

    [Fact]
    public async Task NormalUser_CannotAccessAdminPages()
    {
        var response = await Client.GetAsync("/admin/users");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicPages_ShouldBeAccessibleWithoutAuth()
    {
        var loginResponse = await Client.GetAsync("/account/login");
        var registerResponse = await Client.GetAsync("/account/register");

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
