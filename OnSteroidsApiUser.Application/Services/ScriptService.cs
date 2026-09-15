using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.ScriptModels;

namespace OnSteroidsApiUser.Application.Services;

public class ScriptService(
    IScriptRepository scriptRepository,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<ScriptService> logger
) : IScriptService
{
    private readonly IScriptRepository _scriptRepository = scriptRepository;
    private readonly IAuthDetailsHelper _authDetailsHelper = authDetailsHelper;
    private readonly ILogger<ScriptService> _logger = logger;

    public async Task<(int StatusCode, object Response)> GetMyScriptsAsync()
    {
        var userId = _authDetailsHelper.UserId;
        if (userId == Guid.Empty)
            return BaseResponseHelpers.ReturnUnauthorizedError("User not authenticated", null);

        var scripts = (await _scriptRepository.GetByUserIdAsync(userId)).ToList();
        
        if (!scripts.Any(s => s.IsDefault))
        {
            _logger.LogInformation("Seeding default scripts for user {UserId}", userId);
            scripts = (await _scriptRepository.SeedDefaultsForUserAsync(userId)).ToList();
        }

        return BaseResponseHelpers.ReturnSuccess("Scripts retrieved", scripts);
    }

    public async Task<(int StatusCode, object Response)> CreateScriptAsync(ScriptDto script)
    {
        var userId = _authDetailsHelper.UserId;
        if (userId == Guid.Empty)
            return BaseResponseHelpers.ReturnUnauthorizedError("User not authenticated", null);

        script.Id = Guid.NewGuid();
        script.UserId = userId;
        script.IsDefault = false;
        
        var created = await _scriptRepository.CreateAsync(script);
        if (created == null)
            return BaseResponseHelpers.ReturnServerErrorData("Failed to create script", null);
            
        return BaseResponseHelpers.ReturnSuccess("Script created", created);
    }

    public async Task<(int StatusCode, object Response)> UpdateScriptAsync(Guid id, string name, string content)
    {
        var updated = await _scriptRepository.UpdateAsync(id, name, content);
        if (updated == null)
            return BaseResponseHelpers.ReturnNotFoundError("Failed to update script, it may not exist", null);
            
        return BaseResponseHelpers.ReturnSuccess("Script updated", updated);
    }

    public async Task<(int StatusCode, object Response)> DeleteScriptAsync(Guid id)
    {
        await _scriptRepository.DeleteAsync(id);
        return BaseResponseHelpers.ReturnSuccess<object>("Deleted successfully", null);
    }

    public async Task<(int StatusCode, object Response)> ResetScriptAsync(Guid id)
    {
        var reset = await _scriptRepository.ResetAsync(id);
        if (reset == null)
            return BaseResponseHelpers.ReturnValidationSemanticErrorData("Failed to reset script or script is not a default script", null);
            
        return BaseResponseHelpers.ReturnSuccess("Script reset successfully", reset);
    }
}
