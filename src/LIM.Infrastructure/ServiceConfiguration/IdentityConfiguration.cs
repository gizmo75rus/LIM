using LIM.Infrastructure.Identity;
using LIM.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIM.Infrastructure.ServiceConfiguration;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentityService(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService>(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            
            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return ActivatorUtilities.CreateInstance<CurrentUserService>(sp);
            }
            return ActivatorUtilities.CreateInstance<SystemUserService>(sp);
        });
        
        // Или проще - всегда CurrentUserService, он сам обработает отсутствие HttpContext
        // services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return services;
    }
}