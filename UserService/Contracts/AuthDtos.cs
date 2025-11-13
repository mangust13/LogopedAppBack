namespace UserService.Contracts;

public class AuthDtos
{
    public record RegisterRequest(string Email, string Password, string? Role);
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, int UserId, string Email, string Role);
}