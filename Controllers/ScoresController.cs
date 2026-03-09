using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TadidyVeApi.Data;
using TadidyVeApi.Models;
using TadidyVeApi.Dtos;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("scores")]
public class ScoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScoresController(AppDbContext context)
    {
        _context = context;
    }

    // POST /scores
    [HttpPost]
    public async Task<ActionResult<ScoreResponseDto>> AddScore([FromBody] AddScoreDto dto)
    {
        var player = await _context.Players.FindAsync(dto.PlayerId);
        if (player == null)
            return NotFound("Joueur non trouvé");

        var score = new Score
        {
            PlayerId = dto.PlayerId,
            Value = dto.Value
        };

        _context.Scores.Add(score);

        // Mettre à jour le BestScore du joueur si nécessaire
        if (dto.Value > player.BestScore)
            player.BestScore = dto.Value;

        await _context.SaveChangesAsync();

        var response = new ScoreResponseDto
        {
            Id = score.Id,
            PlayerId = score.PlayerId,
            Value = score.Value,
            CreatedAt = score.CreatedAt
        };

        return CreatedAtAction(nameof(GetScoresByPlayer), new { playerId = dto.PlayerId }, response);
    }

    // GET /scores/{playerId}
    [HttpGet("{playerId}")]
    public async Task<ActionResult<IEnumerable<ScoreResponseDto>>> GetScoresByPlayer(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
            return NotFound("Joueur non trouvé");

        var scores = await _context.Scores
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.Value)
            .Select(s => new ScoreResponseDto
            {
                Id = s.Id,
                PlayerId = s.PlayerId,
                Value = s.Value,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(scores);
    }

    // GET /scores/top
    [HttpGet("top")]
    public async Task<ActionResult<IEnumerable<PlayerResponseDto>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        var topPlayers = await _context.Players
            .OrderByDescending(p => p.BestScore)
            .Take(limit)
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

        return Ok(topPlayers);
    }
}