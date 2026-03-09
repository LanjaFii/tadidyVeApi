using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TadidyVeApi.Data;
using TadidyVeApi.Models;
using TadidyVeApi.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BCrypt.Net;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<ActionResult<PlayerResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Players.AnyAsync(p => p.Username == dto.Username))
            return BadRequest("Nom d'utilisateur déjà utilisé");

        var player = new Player
        {
            Username = dto.Username,
            Bio = dto.Bio,
            ProfilePicture = dto.ProfilePicture,
            BestScore = 0,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        var response = new PlayerResponseDto
        {
            Id = player.Id,
            Username = player.Username,
            Bio = player.Bio,
            ProfilePicture = player.ProfilePicture,
            BestScore = player.BestScore,
            CreatedAt = player.CreatedAt
        };

        return Ok(response);
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Username == dto.Username);
        if (player == null) return Unauthorized("Utilisateur non trouvé");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
            return Unauthorized("Mot de passe incorrect");

        // Générer JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", player.Id.ToString()),
                new Claim("username", player.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(5),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return Ok(new { token = jwt });
    }
}