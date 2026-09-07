using OnSteroidsApiUser.Domain.Models.History;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IRequestHistoryRepository
{
    Task<RequestHistoryDto?> CreateAsync(Guid userId, string method, string url, string? snapshot, int? status, int? time, long? size);
    Task<IEnumerable<RequestHistoryDto>> GetAllByUserAsync(Guid userId, int limit = 100);
    Task DeleteAsync(Guid id, Guid userId);
    Task ClearAsync(Guid userId);
}
