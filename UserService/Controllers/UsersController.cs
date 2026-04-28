using BCrypt.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Contracts;
using UserService.Domain;
using UserService.Infrastructure;
using UserService.Services;

using AuthDtos = UserService.Contracts.AuthDtos;

namespace UserService.Controllers;

[ApiController]
[Route("")]
public class UsersController : ControllerBase
{
    private readonly UsersDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IValidator<AuthDtos.RegisterRequest> _registerValidator;
    private readonly IValidator<AuthDtos.LoginRequest> _loginValidator;

    public UsersController(
        UsersDbContext db,
        IJwtTokenService jwt,
        IValidator<AuthDtos.RegisterRequest> registerValidator,
        IValidator<AuthDtos.LoginRequest> loginValidator)
    {
        _db = db;
        _jwt = jwt;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthDtos.LoginResponse>> Register([FromBody] AuthDtos.RegisterRequest req)
    {
        var validation = await _registerValidator.ValidateAsync(req);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var exists = await _db.Users.AnyAsync(x => x.Email == req.Email.Trim().ToLower());
        if (exists)
            return Conflict(new { message = "Email вже зареєстрований" });

        var user = new User
        {
            Email = req.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role!,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.Create(user);
        return new AuthDtos.LoginResponse(user.Id, user.Email, user.Role, token);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthDtos.LoginResponse>> Login([FromBody] AuthDtos.LoginRequest req)
    {
        var validation = await _loginValidator.ValidateAsync(req);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var email = req.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null)
            return Unauthorized(new { message = "Невірний email або пароль" });

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized(new { message = "Невірний email або пароль" });

        var token = _jwt.Create(user);
        return new AuthDtos.LoginResponse(user.Id, user.Email, user.Role, token);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier") || c.Type == "sub")?.Value;
        if (!int.TryParse(sub, out var id)) return Unauthorized();
        var user = await _db.Users.FindAsync(id);
        if (user is null) return Unauthorized();
        return Ok(new { user.Id, user.Email, user.Role, user.CreatedAt });
    }
}