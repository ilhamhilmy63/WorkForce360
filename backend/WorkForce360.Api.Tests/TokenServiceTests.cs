using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using WorkForce360.Api.Models;
using WorkForce360.Api.Services;

namespace WorkForce360.Api.Tests;

public class TokenServiceTests
{
    private static TokenService CreateService() => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:Key"] = "test-key-that-is-at-least-thirty-two-characters-long",
        ["Jwt:Issuer"] = "test-issuer", ["Jwt:Audience"] = "test-audience", ["Jwt:AccessTokenMinutes"] = "15"
    }).Build());

    [Fact]
    public void AccessToken_ContainsEmployeeRole_AndExpectedExpiry()
    {
        var user = new TripUser { Email = "tourist@example.com", FullName = "Test Tourist", Role = TripRole.Tourist };
        var before = DateTime.UtcNow.AddMinutes(14); var (token, expires) = CreateService().CreateTripAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwt.Subject); Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Tourist"); Assert.True(expires > before);
    }

    [Fact]
    public void RefreshToken_IsRandom_AndCanBeHashedDeterministically()
    {
        var first = TokenService.NewRefreshToken(); var second = TokenService.NewRefreshToken();
        Assert.NotEqual(first, second); Assert.Equal(TokenService.HashToken(first), TokenService.HashToken(first)); Assert.NotEqual(TokenService.HashToken(first), TokenService.HashToken(second));
    }
}
