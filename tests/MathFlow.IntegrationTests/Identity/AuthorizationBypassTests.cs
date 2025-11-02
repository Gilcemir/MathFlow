using FluentAssertions;
using System.Net;

namespace MathFlow.IntegrationTests.Identity;

public class AuthorizationBypassTests : IntegrationTestBase
{
    [Fact]
    public async Task UnauthenticatedUser_CannotAccessManagePages()
    {
        var response = await Client.GetAsync("/Identity/Manage/Index");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Account/Login");
    }

    [Fact]
    public async Task NormalUser_CannotAccessAdminPages()
    {
        var response = await Client.GetAsync("/Admin/Users/Index");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicPages_ShouldBeAccessibleWithoutAuth()
    {
        var loginResponse = await Client.GetAsync("/Account/Login");
        var registerResponse = await Client.GetAsync("/Account/Register");

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
