using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.ScriptModels;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class ScriptRepository(
    IDapperConnection dapperConnection,
    ILogger<ScriptRepository> logger
) : IScriptRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<ScriptRepository> _logger = logger;

    public async Task<IEnumerable<ScriptDto>> GetByUserIdAsync(Guid userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await _dapper.QueryAll<ScriptDto>(parameters, "dbo.spScript_GetByUserId");
    }

    public async Task<ScriptDto?> CreateAsync(ScriptDto script)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", script.Id);
        parameters.Add("@UserId", script.UserId);
        parameters.Add("@Type", script.Type);
        parameters.Add("@Name", script.Name);
        parameters.Add("@Content", script.Content);
        parameters.Add("@IsDefault", script.IsDefault);
        parameters.Add("@OriginalContent", script.OriginalContent);
        
        return await _dapper.Query<ScriptDto>(parameters, "dbo.spScript_Create");
    }

    public async Task<ScriptDto?> UpdateAsync(Guid id, string name, string content)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@Name", name);
        parameters.Add("@Content", content);
        
        return await _dapper.Query<ScriptDto>(parameters, "dbo.spScript_Update");
    }

    public async Task DeleteAsync(Guid id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        await _dapper.Execute(parameters, "dbo.spScript_Delete");
    }

    public async Task<ScriptDto?> ResetAsync(Guid id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        return await _dapper.Query<ScriptDto>(parameters, "dbo.spScript_Reset");
    }

    public async Task<IEnumerable<ScriptDto>> SeedDefaultsForUserAsync(Guid userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await _dapper.QueryAll<ScriptDto>(parameters, "dbo.spScript_SeedDefaultsForUser");
    }
}
