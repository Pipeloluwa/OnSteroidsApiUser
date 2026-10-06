using OnSteroidsApiUser.Domain.Models.WorkspaceState;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IWorkspaceStateService
{
    Task<(int StatusCode, object Response)> UpsertAsync(UpsertWorkspaceStateRequest request, Guid userId);
    Task<(int StatusCode, object Response)> GetByUserAsync(Guid userId);
    Task<(int StatusCode, object Response)> DeleteAsync(Guid userId);
}
