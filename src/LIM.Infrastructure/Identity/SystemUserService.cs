using System.Security.Claims;
using LIM.SharedKernel.Interfaces;

namespace LIM.Infrastructure.Identity;

/// <summary>
/// Используется для фоновых задач, миграций, тестов
/// </summary>
public class SystemUserService : ICurrentUserService
{
    public string? UserId => "system";
    public string? UserName => "system";
    public string? Email => null;
    public IReadOnlyCollection<string> Roles => new[] { "system" }.ToList().AsReadOnly();
    public IReadOnlyCollection<Claim> Claims => new List<Claim>().AsReadOnly();
    public bool IsAuthenticated => true; // system считается аутентифицированным
    public string? GetClaim(string claimType) => null;
    public bool IsInRole(string role) => role == "system";
    public string? CorrelationId => Guid.NewGuid().ToString();
}