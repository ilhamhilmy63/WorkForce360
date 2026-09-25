using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using WorkForce360.Api.Data;using WorkForce360.Api.Models;using WorkForce360.Api.Services;
namespace WorkForce360.Api.Controllers;
[ApiController,Route("api/auth")]
public class TripAuthController(TripDbContext db,TokenService tokens):ControllerBase
{
 public record RegisterDto(string FullName,string Email,string Password,string? Nationality,string? PassportLast4);public record LoginDto(string Email,string Password);
 [HttpPost("register")]public async Task<IActionResult> Register(RegisterDto r){var email=r.Email.Trim().ToLower();if(await db.Users.AnyAsync(x=>x.Email==email))return Conflict(new ProblemDetails{Title="Account already exists"});if(r.Password.Length<8)return ValidationProblem("Password must contain at least 8 characters.");var user=new TripUser{FullName=r.FullName.Trim(),Email=email,PasswordHash=BCrypt.Net.BCrypt.HashPassword(r.Password),Role=TripRole.Tourist};var tourist=new Tourist{User=user,Nationality=r.Nationality,PassportNumberMasked=string.IsNullOrWhiteSpace(r.PassportLast4)?null:$"****{r.PassportLast4[^Math.Min(4,r.PassportLast4.Length)..]}"};db.Add(tourist);await db.SaveChangesAsync();return StatusCode(201,await Issue(user));}
 [HttpPost("login")]public async Task<IActionResult> Login(LoginDto r){var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLower()&&x.IsActive);if(u is null||!BCrypt.Net.BCrypt.Verify(r.Password,u.PasswordHash))return Unauthorized(new ProblemDetails{Title="Invalid email or password"});return Ok(await Issue(u));}
 private async Task<object> Issue(TripUser u){var(token,expires)=tokens.CreateTripAccessToken(u);var refresh=TokenService.NewRefreshToken();u.RefreshTokenHash=TokenService.HashToken(refresh);u.RefreshTokenExpiresAt=DateTime.UtcNow.AddDays(7);await db.SaveChangesAsync();return new{accessToken=token,refreshToken=refresh,expiresAt=expires,user=new{u.Id,u.Email,name=u.FullName,role=u.Role.ToString()}};}
}
