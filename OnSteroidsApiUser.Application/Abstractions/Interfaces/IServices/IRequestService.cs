using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IRequestService
{
    Task<(int StatusCode, object Response)> SaveRequestStateAsync(SaveRequestStateRequest request, Guid userId);
    Task<(int StatusCode, object Response)> GetByIdAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> GetAllByCapsuleAsync(Guid capsuleId, Guid userId);
    Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> DuplicateAsync(Guid sourceId, Guid userId);
}
