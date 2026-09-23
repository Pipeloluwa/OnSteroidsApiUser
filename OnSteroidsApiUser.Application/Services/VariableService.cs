using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Variable;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class VariableService(
    IVariableRepository variableRepository,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<VariableService> logger
) : IVariableService
{
    private readonly IVariableRepository _variableRepo = variableRepository;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<VariableService> _logger = logger;

    public async Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId, Guid? capsuleId = null)
    {
        var variables = await _variableRepo.GetAllByUserAsync(userId, capsuleId);
        return BaseResponseHelpers.ReturnSuccess("Variables retrieved", variables);
    }

    public async Task<(int StatusCode, object Response)> SyncVariablesAsync(SyncVariablesRequest request, Guid userId)
    {
        var items = (request.Variables ?? [])
            .Select(v => new { id = v.Id, key = v.Key, value = v.Value, type = v.Type, enabled = v.Enabled })
            .ToList();

        var sanitizedItems = items.Select(v => new { v.id, v.key, value = "***", v.type, v.enabled }).ToList();
        _logger.LogInformation("[{RequestId}] Syncing {Count} variables for user {UserId}, capsule {CapsuleId}: {Variables}", _authDetails.RequestId, items.Count, userId, request.CapsuleId, JsonSerializer.Serialize(sanitizedItems));

        var json = JsonSerializer.Serialize(items);
        var result = await _variableRepo.SyncAsync(userId, request.CapsuleId, json);
        return BaseResponseHelpers.ReturnSuccess("Variables synced successfully", result);
    }
}
