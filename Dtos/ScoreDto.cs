namespace TadidyVeApi.Dtos;

public class ScoreResponseDto
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddScoreDto
{
    public int PlayerId { get; set; }
    public int Value { get; set; }
}