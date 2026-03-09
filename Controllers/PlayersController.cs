using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TadidyVeApi.Data;
using TadidyVeApi.Models;
using TadidyVeApi.Dtos;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("players")]
[Authorize] // <- tous les endpoints nécessitent JWT
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerResponseDto>>> GetAllPlayers()
    {
        var players = await _context.Players
            .Select(p => new PlayerResponseDto
            {
                Id = p.Id,
                Username = p.Username,
                Bio = p.Bio,
                ProfilePicture = p.ProfilePicture,
                BestScore = p.BestScore,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Ok(players);
    }

    // GET /players/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerResponseDto>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Where(p => p.Id == id)
            .Select(p => new PlayerResponseDto
            {
                Id = p.Id,
                Username = p.Username,
                Bio = p.Bio,
                ProfilePicture = p.ProfilePicture,
                BestScore = p.BestScore,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (player == null) return NotFound("Joueur non trouvé");
        return Ok(player);
    }

    // PATCH /players/{id}
    [HttpPatch("{id}")]
    public async Task<ActionResult<PlayerResponseDto>> UpdatePlayer(int id, [FromBody] UpdatePlayerDto dto)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null) return NotFound("Joueur non trouvé");

        if (!string.IsNullOrEmpty(dto.Bio))
            player.Bio = dto.Bio;
        if (!string.IsNullOrEmpty(dto.ProfilePicture))
            player.ProfilePicture = dto.ProfilePicture;

        await _context.SaveChangesAsync();

        return Ok(new PlayerResponseDto
        {
            Id = player.Id,
            Username = player.Username,
            Bio = player.Bio,
            ProfilePicture = player.ProfilePicture,
            BestScore = player.BestScore,
            CreatedAt = player.CreatedAt
        });
    }
}