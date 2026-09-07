using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.History;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class RequestHistoryRepository(
    IDapperConnection dapperConnection,
    ILogger<RequestHistoryRepository> logger
) : IRequestHistoryRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<RequestHistoryRepository> _logger = logger;

    public async Task<RequestHistoryDto?> CreateAsync(Guid userId, string method, string url, string? snapshot, int? status, int? time, long? size)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@Method", method);
        p.Add("@Url", url);
        p.Add("@RequestSnapshot", snapshot);
        p.Add("@ResponseStatus", status);
        p.Add("@ResponseTime", time);
        p.Add("@ResponseSize", size);
        return await _dapper.Query<RequestHistoryDto>(p, "dbo.spRequestHistory_Create");
    }

    public async Task<IEnumerable<RequestHistoryDto>> GetAllByUserAsync(Guid userId, int limit = 100)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@Limit", limit);
        return await _dapper.QueryAll<RequestHistoryDto>(p, "dbo.spRequestHistory_GetAllByUser");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@Id", id);
        p.Add("@UserId", userId);
        await _dapper.Execute(p, "dbo.spRequestHistory_Delete");
    }

    public async Task ClearAsync(Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        await _dapper.Execute(p, "dbo.spRequestHistory_Clear");
    }
}
