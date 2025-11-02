using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.UnitTests.Identity;

public abstract class IdentityTestBase
{
    protected UserManager<ApplicationUser> CreateMockUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
        return userManager;
    }

    protected SignInManager<ApplicationUser> CreateMockSignInManager(
        UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        
        var signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor,
            claimsFactory,
            null, null, null, null);
        
        return signInManager;
    }

    protected ILogger<T> CreateMockLogger<T>()
    {
        return Substitute.For<ILogger<T>>();
    }
}
