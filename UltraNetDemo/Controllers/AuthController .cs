using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UltraNet.Core.Interfaces.JWT;
using UltraNet.Core.Interfaces.PasswordHasher;
using UltraNetDemo.Models;

namespace UltraNetDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly Dictionary<string, string> _users = new(); 

    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthController(IPasswordHasher hasher, ITokenGenerator tokenGenerator)
    {
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        if (_users.ContainsKey(request.Username))
            return BadRequest("User already exists");

        var hashed = _hasher.HashPassword(request.Password);
        _users.Add(request.Username,hashed);
        return Ok("User registered");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        if (!_users.TryGetValue(request.Username, out var hashed))
            return Unauthorized("User not found");

        if (!_hasher.VerifyPassword(hashed, request.Password))
            return Unauthorized("Invalid password");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("CustomClaim", "ValueTest")
        };

        var token = _tokenGenerator.GenerateToken(claims);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        return Ok(new { token, refreshToken });
    }
}