using System.Security.Claims;
using LIM.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace LIM.UnitTests.Infrastructure;

public class CurrentUserServiceTests
{
    [Fact]
    public void GetUserId_FromJwt_ReturnsCorrectValue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "12345"),
            new(ClaimTypes.Name, "john.doe"),
            new(ClaimTypes.Email, "john@lab.com"),
            new(ClaimTypes.Role, "admin"),
            new(ClaimTypes.Role, "biologist")
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = user;
        
        var httpContextAccessor = new HttpContextAccessor 
        { 
            HttpContext = httpContext 
        };
        
        var logger = new NullLogger<CurrentUserService>();
        var service = new CurrentUserService(httpContextAccessor, logger);
        
        // Assert
        Assert.Equal("12345", service.UserId);
        Assert.Equal("john.doe", service.UserName);
        Assert.Equal("john@lab.com", service.Email);
        Assert.Contains("admin", service.Roles);
        Assert.Contains("biologist", service.Roles);
        Assert.True(service.IsAuthenticated);
        Assert.True(service.IsInRole("admin"));
    }
    
    [Fact]
    public void SystemUserService_ReturnsSystemUser()
    {
        var service = new SystemUserService();
        
        Assert.Equal("system", service.UserId);
        Assert.Equal("system", service.UserName);
        Assert.True(service.IsAuthenticated);
        Assert.Contains("system", service.Roles);
    }
}