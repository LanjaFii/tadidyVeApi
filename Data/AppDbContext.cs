using Microsoft.EntityFrameworkCore;
using TadidyVeApi.Models;

namespace TadidyVeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
}