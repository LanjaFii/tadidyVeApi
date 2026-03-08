// Controllers/PlayersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TadidyVeApi.Data;
using TadidyVeApi.Models;

namespace TadidyVeApi.Controllers;

[ApiController]
[Route("players")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        return await _context.Players.ToListAsync();
    }

    // Ajoute dans PlayersController
    [HttpPost("register")]
    public async Task<ActionResult<Player>> Register([FromBody] Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlayers), new { id = player.Id }, player);
    }
}