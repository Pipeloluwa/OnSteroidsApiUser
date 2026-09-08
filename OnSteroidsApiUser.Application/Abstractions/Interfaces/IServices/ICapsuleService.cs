using OnSteroidsApiUser.Domain.Models.Capsule;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface ICapsuleService
{
    Task<(int StatusCode, object Response)> CreateAsync(CreateCapsuleRequest request, Guid userId);
    Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId);
    Task<(int StatusCode, object Response)> GetByIdAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> UpdateAsync(Guid id, UpdateCapsuleRequest request, Guid userId);
    Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> BatchDeleteAsync(List<Guid> ids, Guid userId);
    Task<(int StatusCode, object Response)> ShareCapsuleAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> GetSharedCapsuleAsync(string token);
}
