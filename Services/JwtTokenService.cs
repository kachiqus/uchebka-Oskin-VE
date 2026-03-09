using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace oskin_site.Services;

public sealed class JwtTokenService
{
    private readonly IConfiguration _cfg;

    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(int userId, string username, string role)
    {
        var jwt = _cfg.GetSection("Jwt");

        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];
        var key = jwt["Key"];
        var expMin = int.TryParse(jwt["ExpireMinutes"], out var m) ? m : 120;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role ?? "User")
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
