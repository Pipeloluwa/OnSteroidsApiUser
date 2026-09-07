using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnSteroidsApiUser.Application.Features.Helpers;

public class JwtTokenHelper(IOptions<AppSettings> appSettings) : IJwtTokenHelper
{
    private readonly AppSettings _appSettings = appSettings.Value;

    public (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string email)
    {
        var jwtSettings = _appSettings.JWTSettings
            ?? throw new InvalidOperationException("JWTSettings not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key
            ?? "DefaultVeryLongSecretKeyForOnSteroidsApplication123456789!@#$%^&*()"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.TokenExpiryMinutes > 0
            ? jwtSettings.TokenExpiryMinutes
            : 1440);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}
