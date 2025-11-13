using Jering.Javascript.NodeJS;
using MathFlow.Infrastructure.Converters;
using MathFlow.Infrastructure.IdentityServer.Configuration;
using MathFlow.Infrastructure.IdentityServer.Models;
using MathFlow.Infrastructure.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace MathFlow.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddIdentityServices(configuration, environment);
        services.AddAuthorizationPolicies();

        services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();

        services.AddNodeJS();
        services.Configure<OutOfProcessNodeJSServiceOptions>(options =>
        {
            options.Concurrency = Concurrency.MultiProcess;
            options.ConcurrencyDegree = 4;
            options.EnableFileWatching = false;
        });
        services.Configure<NodeJSProcessOptions>(options =>
        {
            options.ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure/Converters/Scripts");
        });

        services.AddSingleton<OmmlToMathMLConverter>();

        return services;
    }
}
