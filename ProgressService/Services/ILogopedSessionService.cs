using ProgressService.Contracts;

namespace ProgressService.Services;

public interface ILogopedSessionService
{
    Task<SessionDto> CreateAsync(int logopedId, CreateSessionDto dto);
    Task<SessionDto> UpdateAsync(int logopedId, int sessionId, UpdateSessionDto dto);
    Task DeleteAsync(int logopedId, int sessionId);
    Task<List<SessionDto>> GetByChildAsync(int childId);
    Task<List<SessionDto>> GetByLogopedAsync(int logopedId, int? childId = null);
}