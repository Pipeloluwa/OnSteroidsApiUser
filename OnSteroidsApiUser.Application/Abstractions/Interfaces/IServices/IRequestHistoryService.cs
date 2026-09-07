using OnSteroidsApiUser.Domain.Models.History;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IRequestHistoryService
{
    Task<(int StatusCode, object Response)> CreateHistoryAsync(CreateHistoryRequest request, Guid userId);
    Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId, int limit = 100);
    Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> ClearAsync(Guid userId);
}
