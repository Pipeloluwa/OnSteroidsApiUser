using OnSteroidsApiUser.Domain.Models.WorkspaceState;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IWorkspaceStateRepository
{
    Task<WorkspaceStateDto?> UpsertAsync(Guid userId, WorkspaceStateDto state);
    Task<WorkspaceStateDto?> UpdateActiveTabAsync(Guid userId, UpdateActiveTabRequest request);
    Task<WorkspaceStateDto?> GetByUserAsync(Guid userId);
    Task DeleteAsync(Guid userId);
}
