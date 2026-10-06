using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.WorkspaceState;

namespace OnSteroidsApiUser.Application.Services;

public class WorkspaceStateService(
    IWorkspaceStateRepository workspaceStateRepository,
    ILogger<WorkspaceStateService> logger
) : IWorkspaceStateService
{
    private readonly IWorkspaceStateRepository _repo = workspaceStateRepository;
    private readonly ILogger<WorkspaceStateService> _logger = logger;

    public async Task<(int StatusCode, object Response)> UpsertAsync(UpsertWorkspaceStateRequest request, Guid userId)
    {
        var dto = new WorkspaceStateDto
        {
            UserId = userId,
            ActiveCapsuleId = request.ActiveCapsuleId,
            ActiveCapsuleName = request.ActiveCapsuleName,
            ActiveTabId = request.ActiveTabId,
            OpenTabIds = request.OpenTabIds,
            TabStates = request.TabStates,
            Responses = request.Responses,
            AutoAuthEnabled = request.AutoAuthEnabled ?? "off",
            AutoAuthEndpointId = request.AutoAuthEndpointId,
            AutoSaveEnabled = request.AutoSaveEnabled
        };

        var result = await _repo.UpsertAsync(userId, dto);
        _logger.LogInformation("Upserted workspace state for user {UserId}", userId);
        return BaseResponseHelpers.ReturnSuccess("Workspace state saved", result);
    }

    public async Task<(int StatusCode, object Response)> GetByUserAsync(Guid userId)
    {
        var state = await _repo.GetByUserAsync(userId);
        if (state == null)
        {
            return BaseResponseHelpers.ReturnSuccess<WorkspaceStateDto?>("No workspace state found", null);
        }
        return BaseResponseHelpers.ReturnSuccess("Workspace state retrieved", state);
    }

    public async Task<(int StatusCode, object Response)> DeleteAsync(Guid userId)
    {
        await _repo.DeleteAsync(userId);
        _logger.LogInformation("Deleted workspace state for user {UserId}", userId);
        return BaseResponseHelpers.ReturnSuccess<object>("Workspace state deleted", null);
    }
}
