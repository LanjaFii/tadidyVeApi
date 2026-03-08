namespace TadidyVeApi.Models;

public class Player
{
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public string Bio { get; set; } = "";

    public string ProfilePicture { get; set; } = "";

    public int BestScore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string PasswordHash { get; set; } = "";
}