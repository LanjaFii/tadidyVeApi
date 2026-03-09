namespace TadidyVeApi.Dtos;

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