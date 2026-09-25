using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkForce360.Api.Models;

namespace WorkForce360.Api.Services;
public class TokenService(IConfiguration config)
{
    public (string Token, DateTime ExpiresAt) CreateTripAccessToken(TripUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(config.GetValue("Jwt:AccessTokenMinutes", 60));
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(ClaimTypes.Role, user.Role.ToString()), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, expires: expires, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
    public static string NewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
