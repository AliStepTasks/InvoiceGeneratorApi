using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceGeneratorApi.Auth;

public class JwtService : IJwtService
{
    private readonly JwtConfig _jwtConfig;

    public JwtService(JwtConfig jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }

    public string GenerateSecurityToken(string email)
    {
        var claims = new[]
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            expires: DateTime.Now.AddMinutes(_jwtConfig.ExpireInMinutes),
            signingCredentials: creds,
            claims: claims
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return accessToken;
    }
}
