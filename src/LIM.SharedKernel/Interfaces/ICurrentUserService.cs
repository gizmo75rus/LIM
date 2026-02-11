using System.Security.Claims;

namespace LIM.SharedKernel.Interfaces;

/// <summary>
/// Сервис получения информации о текущем аутентифицированном пользователе
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID пользователя (из ClaimTypes.NameIdentifier или sub)
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Имя пользователя (из ClaimTypes.Name или unique_name)
    /// </summary>
    string? UserName { get; }
    
    /// <summary>
    /// Email пользователя (из ClaimTypes.Email)
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Роли пользователя (из ClaimTypes.Role)
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }
    
    /// <summary>
    /// Все claims пользователя
    /// </summary>
    IReadOnlyCollection<Claim> Claims { get; }
    
    /// <summary>
    /// Аутентифицирован ли пользователь
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Получить значение claim по типу
    /// </summary>
    string? GetClaim(string claimType);
    
    /// <summary>
    /// Проверить наличие роли
    /// </summary>
    bool IsInRole(string role);
    
    /// <summary>
    /// ID сессии/запроса для корреляции
    /// </summary>
    string? CorrelationId { get; }
}