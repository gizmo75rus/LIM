using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LIM.Infrastructure.Identity;

public class AuthManager
{
    public const string ISSUER = "D.Ilyushchenko";
    public const string AUDIENCE = "LIM.Infrastructure";
    private const string _KEY = "43BA460C-3E214C138513-5401D5CCD5ED";
    private const int _LIFETIME = 1;

    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_KEY));
    }

    public static string GenerateToken(List<Claim> claims)
    { 
        JwtSecurityToken token = new(
            ISSUER,
            AUDIENCE,
            claims,
            expires : DateTime.Now.AddDays(_LIFETIME),
            signingCredentials : new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

}