namespace TadidyVeApi.Dtos;

public class PlayerResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Bio { get; set; } = "";
    public string ProfilePicture { get; set; } = "";
    public int BestScore { get; set; }
    public DateTime CreatedAt { get; set; }
}