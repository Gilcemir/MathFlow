using MathFlow.Application.Services.Converters;
using MathFlow.Application.Services.Identity;

namespace MathFlow.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddSingleton<WordProcessor>();

        return services;
    }
}
