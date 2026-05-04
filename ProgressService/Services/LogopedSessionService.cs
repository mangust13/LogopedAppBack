using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Services;

public class LogopedSessionService : ILogopedSessionService
{
    private readonly ProgressDbContext _db;

    public LogopedSessionService(ProgressDbContext db)
    {
        _db = db;
    }

    public async Task<SessionDto> CreateAsync(int logopedId, CreateSessionDto dto)
    {
        var session = new LogopedSession
        {
            LogopedId = logopedId,
            ChildId = dto.ChildId,
            Date = dto.Date.ToUniversalTime(),
            Duration = dto.DurationMinutes.HasValue
                ? TimeSpan.FromMinutes(dto.DurationMinutes.Value)
                : null,
            Notes = dto.Notes?.Trim(),
            SoundsWorkedOn = dto.SoundsWorkedOn?.Trim().ToLower(),
        };

        _db.LogopedSessions.Add(session);
        await _db.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task<SessionDto> UpdateAsync(int logopedId, int sessionId, UpdateSessionDto dto)
    {
        var session = await _db.LogopedSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.LogopedId == logopedId)
            ?? throw new KeyNotFoundException("Session not found");

        session.Date = dto.Date.ToUniversalTime();
        session.Duration = dto.DurationMinutes.HasValue
            ? TimeSpan.FromMinutes(dto.DurationMinutes.Value)
            : null;
        session.Notes = dto.Notes?.Trim();
        session.SoundsWorkedOn = dto.SoundsWorkedOn?.Trim().ToLower();

        await _db.SaveChangesAsync();

        return ToDto(session);
    }

    public async Task DeleteAsync(int logopedId, int sessionId)
    {
        var session = await _db.LogopedSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.LogopedId == logopedId)
            ?? throw new KeyNotFoundException("Session not found");

        _db.LogopedSessions.Remove(session);
        await _db.SaveChangesAsync();
    }

    public async Task<List<SessionDto>> GetByChildAsync(int childId)
    {
        var sessions = await _db.LogopedSessions
            .Where(s => s.ChildId == childId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return sessions.Select(ToDto).ToList();
    }

    public async Task<List<SessionDto>> GetByLogopedAsync(int logopedId, int? childId = null)
    {
        var query = _db.LogopedSessions
            .Where(s => s.LogopedId == logopedId);

        if (childId.HasValue)
            query = query.Where(s => s.ChildId == childId.Value);

        var sessions = await query
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return sessions.Select(ToDto).ToList();
    }

    private static SessionDto ToDto(LogopedSession s) => new()
    {
        Id = s.Id,
        ChildId = s.ChildId,
        Date = s.Date,
        DurationMinutes = s.Duration.HasValue ? (int)s.Duration.Value.TotalMinutes : null,
        Notes = s.Notes,
        SoundsWorkedOn = string.IsNullOrWhiteSpace(s.SoundsWorkedOn)
            ? new List<string>()
            : s.SoundsWorkedOn.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList(),
        CreatedAt = s.CreatedAt,
    };
}