using FluentValidation;

namespace UserService.Contracts;

public class AuthDtos
{
    public record RegisterRequest(string Email, string Password, string? Role);
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(int UserId, string Email, string Role, string Token);
}

public class RegisterRequestValidator : AbstractValidator<AuthDtos.RegisterRequest>
{
    private static readonly HashSet<string> AllowedRoles = ["User", "Logoped"];

    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обовʼязковим")
            .EmailAddress().WithMessage("Невірний формат email")
            .MaximumLength(200).WithMessage("Email не може перевищувати 200 символів");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль є обовʼязковим")
            .MinimumLength(6).WithMessage("Пароль має містити щонайменше 6 символів")
            .MaximumLength(100).WithMessage("Пароль не може перевищувати 100 символів");

        RuleFor(x => x.Role)
            .Must(r => r is null || AllowedRoles.Contains(r))
            .WithMessage("Роль має бути 'User' або 'Logoped'");
    }
}

public class LoginRequestValidator : AbstractValidator<AuthDtos.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обовʼязковим")
            .EmailAddress().WithMessage("Невірний формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль є обовʼязковим");
    }
}