using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TadidyVeApi.Data;
using TadidyVeApi.Models;
using TadidyVeApi.Dtos;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("scores")]
[Authorize]
public class ScoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScoresController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ScoreResponseDto>> AddScore([FromBody] AddScoreDto dto)
    {
        var playerId = int.Parse(User.FindFirst("id")!.Value);

        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return NotFound("Joueur non trouvé");

        var score = new Score
        {
            PlayerId = playerId,
            Value = dto.Value
        };

        _context.Scores.Add(score);

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

        return CreatedAtAction(nameof(GetScoresByPlayer), new { playerId }, response);
    }

    [HttpGet("{playerId}")]
    public async Task<ActionResult<IEnumerable<ScoreResponseDto>>> GetScoresByPlayer(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return NotFound("Joueur non trouvé");

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