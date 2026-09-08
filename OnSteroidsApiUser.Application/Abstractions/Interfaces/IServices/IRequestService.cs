using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IRequestService
{
    Task<(int StatusCode, object Response)> SaveRequestStateAsync(SaveRequestStateRequest request, Guid userId);
    Task<(int StatusCode, object Response)> SaveBatchRequestStateAsync(List<SaveRequestStateRequest> requests, Guid userId);
    Task<(int StatusCode, object Response)> GetByIdAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> GetAllByCapsuleAsync(Guid capsuleId, Guid userId);
    Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId);
    Task<(int StatusCode, object Response)> BatchDeleteAsync(List<Guid> ids, Guid userId);
    Task<(int StatusCode, object Response)> DuplicateAsync(Guid sourceId, Guid userId);
    Task<(int StatusCode, object Response)> CreateExampleAsync(Guid requestId, CreateRequestExampleRequest request, Guid userId);
    Task<(int StatusCode, object Response)> GetExamplesByRequestAsync(Guid requestId, Guid userId);
    Task<(int StatusCode, object Response)> DeleteExampleAsync(Guid exampleId, Guid userId);
}
