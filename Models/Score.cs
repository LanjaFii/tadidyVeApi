namespace TadidyVeApi.Models;

public class Score
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}