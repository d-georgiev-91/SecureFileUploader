using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureFileUploader.Services;
using SecureFileUploader.Web.Models;
using SecureFileUploader.Web.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureFileUploader.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IUserService userService, IOptions<JwtConfig> jwtConfig, IMapper mapper) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var canLogin = await userService.LoginAsync(mapper.Map<Services.Models.User>(user));

        if (canLogin)
        {
            return Ok(new { Token = GenerateJwtToken(user.Username) });
        }

        return Unauthorized();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        await userService.RegisterAsync(mapper.Map<Services.Models.User>(user));
        return Ok();
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Value.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Value.Issuer,
            audience: jwtConfig.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtConfig.Value.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
