using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Domain;
using UserService.Infrastructure;
using UserService.Services;

using AuthDtos = UserService.Contracts.AuthDtos;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersDbContext _db;
    private readonly IJwtTokenService _jwt;

    public UsersController(UsersDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthDtos.RegisterRequest req)
    {
        var exists = await _db.Users.AnyAsync(x => x.Email == req.Email);
        if (exists)
        {
            return Conflict("Email already registered");
        }

        var user = new User
        {
            Email = req.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = String.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role!,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Me), new { }, new { user.Id, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthDtos.LoginResponse>> Login([FromBody] AuthDtos.LoginRequest req)
    {
        string email = req.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null) return Unauthorized();
        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized();
        var token = _jwt.Create(user);
        return new AuthDtos.LoginResponse(token, user.Id, user.Email, user.Role);
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