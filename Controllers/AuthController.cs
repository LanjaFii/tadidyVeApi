// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TadidyVeApi.Data;
using TadidyVeApi.Models;
using BCrypt.Net;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<ActionResult<PlayerResponseDto>> Register([FromBody] RegisterDto dto)
    {
        // Vérifier si le username existe déjà
        if (await _context.Players.AnyAsync(p => p.Username == dto.Username))
        {
            return BadRequest("Nom d'utilisateur déjà utilisé");
        }

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

        // Ne pas renvoyer le PasswordHash dans la réponse
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

        if (player == null)
            return Unauthorized("Utilisateur non trouvé");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
            return Unauthorized("Mot de passe incorrect");

        // Ici tu peux ajouter un JWT plus tard
        return Ok(new { message = "Connecté !" });
    }
}

// DTOs
public class RegisterDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Bio { get; set; } = "";
    public string ProfilePicture { get; set; } = "";
}

public class LoginDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

// DTO pour renvoyer le joueur sans PasswordHash
public class PlayerResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Bio { get; set; } = "";
    public string ProfilePicture { get; set; } = "";
    public int BestScore { get; set; }
    public DateTime CreatedAt { get; set; }
}