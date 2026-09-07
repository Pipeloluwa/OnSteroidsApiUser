using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class VariableRepository(
    IDapperConnection dapperConnection,
    ILogger<VariableRepository> logger
) : IVariableRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<VariableRepository> _logger = logger;

    public async Task<VariableDto?> CreateAsync(Guid userId, string key, string value, bool isEnabled)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@VariableKey", key);
        p.Add("@VariableValue", value);
        p.Add("@IsEnabled", isEnabled);
        return await _dapper.Query<VariableDto>(p, "dbo.spVariable_Create");
    }

    public async Task<IEnumerable<VariableDto>> GetAllByUserAsync(Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        return await _dapper.QueryAll<VariableDto>(p, "dbo.spVariable_GetAllByUser");
    }

    public async Task<VariableDto?> UpdateAsync(Guid id, Guid userId, string? key, string? value, bool? isEnabled)
    {
        var p = new DynamicParameters();
        p.Add("@Id", id);
        p.Add("@UserId", userId);
        p.Add("@VariableKey", key);
        p.Add("@VariableValue", value);
        p.Add("@IsEnabled", isEnabled);
        return await _dapper.Query<VariableDto>(p, "dbo.spVariable_Update");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@Id", id);
        p.Add("@UserId", userId);
        await _dapper.Execute(p, "dbo.spVariable_Delete");
    }

    public async Task<IEnumerable<VariableDto>> SyncAsync(Guid userId, string jsonData)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@JsonData", jsonData);
        return await _dapper.QueryAll<VariableDto>(p, "dbo.spVariable_Sync");
    }
}
