using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Variable;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class VariableService(
    IVariableRepository variableRepository,
    ILogger<VariableService> logger
) : IVariableService
{
    private readonly IVariableRepository _variableRepo = variableRepository;
    private readonly ILogger<VariableService> _logger = logger;

    public async Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId)
    {
        var variables = await _variableRepo.GetAllByUserAsync(userId);
        return BaseResponseHelpers.ReturnSuccess("Variables retrieved", variables);
    }

    public async Task<(int StatusCode, object Response)> SyncVariablesAsync(SyncVariablesRequest request, Guid userId)
    {
        var items = (request.Variables ?? [])
            .Select(v => new { id = v.Id, key = v.Key, value = v.Value, enabled = v.Enabled })
            .ToList();

        var json = JsonSerializer.Serialize(items);
        _logger.LogInformation("Syncing {Count} variables for user {UserId}", items.Count, userId);

        var result = await _variableRepo.SyncAsync(userId, json);
        return BaseResponseHelpers.ReturnSuccess("Variables synced successfully", result);
    }
}
