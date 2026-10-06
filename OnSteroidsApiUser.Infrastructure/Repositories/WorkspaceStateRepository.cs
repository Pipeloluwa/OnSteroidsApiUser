using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.WorkspaceState;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class WorkspaceStateRepository(
    IDapperConnection dapperConnection,
    ILogger<WorkspaceStateRepository> logger
) : IWorkspaceStateRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<WorkspaceStateRepository> _logger = logger;

    public async Task<WorkspaceStateDto?> UpsertAsync(Guid userId, WorkspaceStateDto state)
    {
        _logger.LogInformation("Upserting workspace state for user {UserId}", userId);
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@ActiveCapsuleId", state.ActiveCapsuleId);
        p.Add("@ActiveCapsuleName", state.ActiveCapsuleName);
        p.Add("@ActiveTabId", state.ActiveTabId);
        p.Add("@OpenTabIds", state.OpenTabIds);
        p.Add("@TabStates", state.TabStates);
        p.Add("@Responses", state.Responses);
        p.Add("@AutoAuthEnabled", state.AutoAuthEnabled);
        p.Add("@AutoAuthEndpointId", state.AutoAuthEndpointId);
        p.Add("@AutoSaveEnabled", state.AutoSaveEnabled);
        return await _dapper.Query<WorkspaceStateDto>(p, "dbo.spUserWorkspaceState_Upsert");
    }

    public async Task<WorkspaceStateDto?> GetByUserAsync(Guid userId)
    {
        _logger.LogInformation("Getting workspace state for user {UserId}", userId);
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        return await _dapper.Query<WorkspaceStateDto>(p, "dbo.spUserWorkspaceState_GetByUser");
    }

    public async Task DeleteAsync(Guid userId)
    {
        _logger.LogInformation("Deleting workspace state for user {UserId}", userId);
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        await _dapper.Execute(p, "dbo.spUserWorkspaceState_Delete");
    }
}
