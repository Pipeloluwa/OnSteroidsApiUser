using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.Capsule;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class CapsuleRepository(
    IDapperConnection dapperConnection,
    ILogger<CapsuleRepository> logger
) : ICapsuleRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<CapsuleRepository> _logger = logger;

    public async Task<CapsuleDto?> CreateAsync(Guid userId, string name)
    {
        _logger.LogInformation("Creating capsule {Name} for user {UserId}", name, userId);
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@Name", name);
        return await _dapper.Query<CapsuleDto>(parameters, "dbo.spCapsule_Create");
    }

    public async Task<IEnumerable<CapsuleDto>> GetAllByUserAsync(Guid userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await _dapper.QueryAll<CapsuleDto>(parameters, "dbo.spCapsule_GetAllByUser");
    }

    public async Task<CapsuleDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", userId);
        return await _dapper.Query<CapsuleDto>(parameters, "dbo.spCapsule_GetById");
    }

    public async Task<CapsuleDto?> UpdateAsync(Guid id, Guid userId, string name)
    {
        _logger.LogInformation("Updating capsule {Id} for user {UserId}", id, userId);
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", userId);
        parameters.Add("@Name", name);
        return await _dapper.Query<CapsuleDto>(parameters, "dbo.spCapsule_Update");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        _logger.LogInformation("Deleting capsule {Id} for user {UserId}", id, userId);
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        parameters.Add("@UserId", userId);
        await _dapper.Execute(parameters, "dbo.spCapsule_Delete");
    }
}
