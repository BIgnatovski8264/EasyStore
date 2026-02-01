using EasyStore.Data.Interfaces;
using EasyStore.Data.Repositories;
using EasyStore.Domain.Interfaces;
using EasyStore.Domain.Services;

namespace EasyStore.API.Extensions;
internal static class ServiceExtension
{
    internal static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}