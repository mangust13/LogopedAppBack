using BCrypt.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Contracts;
using UserService.Domain;
using UserService.Infrastructure;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("")]
public class UsersController(
    UsersDbContext db,
    IJwtTokenService jwt,
    IEmailService emailService,
    IValidator<AuthDtos.RegisterRequest> registerValidator,
    IValidator<AuthDtos.LoginRequest> loginValidator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthDtos.LoginResponse>> Register([FromBody] AuthDtos.RegisterRequest req)
    {
        var validation = await registerValidator.ValidateAsync(req);
        if (!validation.IsValid)
            return ValidationProblem(validation.ToValidationProblemDetails());

        var email = req.Email.Trim().ToLower();

        if (await db.Users.AnyAsync(x => x.Email == email))
            return Conflict(new { message = "Email вже зареєстрований" });

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role!,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        _ = emailService.SendWelcomeAsync(user.Email, user.Role);

        var token = jwt.Create(user);
        return new AuthDtos.LoginResponse(user.Id, user.Email, user.Role, token);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthDtos.LoginResponse>> Login([FromBody] AuthDtos.LoginRequest req)
    {
        var validation = await loginValidator.ValidateAsync(req);
        if (!validation.IsValid)
            return ValidationProblem(validation.ToValidationProblemDetails());

        var email = req.Email.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Невірний email або пароль" });

        var token = jwt.Create(user);
        return new AuthDtos.LoginResponse(user.Id, user.Email, user.Role, token);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.Claims
            .FirstOrDefault(c => c.Type.EndsWith("/nameidentifier") || c.Type == "sub")?.Value;
        if (!int.TryParse(sub, out var id)) return Unauthorized();

        var user = await db.Users.FindAsync(id);
        if (user is null) return Unauthorized();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
        });
    }

    [Authorize]
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var sub = User.Claims
            .FirstOrDefault(c => c.Type.EndsWith("/nameidentifier") || c.Type == "sub")?.Value;
        if (!int.TryParse(sub, out var id)) return Unauthorized();

        var user = await db.Users.FindAsync(id);
        if (user is null) return Unauthorized();

        user.FirstName = dto.FirstName?.Trim();
        user.LastName = dto.LastName?.Trim();

        await db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName,
        });
    }
}