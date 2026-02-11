using System.Security.Claims;
using LIM.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LIM.Infrastructure.Identity;

/// <summary>
/// Реализация ICurrentUserService для ASP.NET Core с JWT
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User?.FindFirst("sub")?.Value; // JWT стандарт

    public string? UserName =>
        User?.FindFirst(ClaimTypes.Name)?.Value ??
        User?.FindFirst("unique_name")?.Value;

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value ??
        User?.FindFirst("email")?.Value;

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var roles = User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new();

            // JWT часто использует "role" без namespace
            roles.AddRange(User?.FindAll("role").Select(c => c.Value) ?? Array.Empty<string>());

            return roles.Distinct().ToList().AsReadOnly();
        }
    }

    public IReadOnlyCollection<Claim> Claims =>
        User?.Claims.ToList().AsReadOnly() ??
        new List<Claim>().AsReadOnly();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? GetClaim(string claimType)
    {
        return User?.FindFirst(claimType)?.Value;
    }

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }

    public string? CorrelationId
    {
        get
        {
            // Для трассировки запросов
            return _httpContextAccessor.HttpContext?
                       .Request.Headers["X-Correlation-ID"].FirstOrDefault()
                   ?? _httpContextAccessor.HttpContext?
                       .TraceIdentifier;
        }
    }
}