using FluentValidation;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Capsule;

using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class CapsuleService(
    ICapsuleRepository capsuleRepository,
    IRequestRepository requestRepository,
    IValidator<CreateCapsuleRequest> createValidator,
    IValidator<UpdateCapsuleRequest> updateValidator,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<CapsuleService> logger
) : ICapsuleService
{
    private readonly ICapsuleRepository _capsuleRepo = capsuleRepository;
    private readonly IRequestRepository _requestRepo = requestRepository;
    private readonly IValidator<CreateCapsuleRequest> _createValidator = createValidator;
    private readonly IValidator<UpdateCapsuleRequest> _updateValidator = updateValidator;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<CapsuleService> _logger = logger;

    public async Task<(int StatusCode, object Response)> CreateAsync(CreateCapsuleRequest request, Guid userId)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var capsule = await _capsuleRepo.CreateAsync(userId, request.Name.Trim());
        _logger.LogInformation("[{RequestId}] User {UserId} created capsule: {Capsule}", _authDetails.RequestId, userId, JsonSerializer.Serialize(capsule));
        return BaseResponseHelpers.ReturnSuccess("Capsule created successfully", capsule);
    }

    public async Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId)
    {
        var capsules = (await _capsuleRepo.GetAllByUserAsync(userId)).ToList();
        if (capsules.Count == 0)
        {
            // Seed a default capsule for this user if they don't have any
            var defaultCapsule = await _capsuleRepo.CreateAsync(userId, "My Capsule");
            if (defaultCapsule != null)
            {
                capsules.Add(defaultCapsule);
            }
        }

        return BaseResponseHelpers.ReturnSuccess("Capsules retrieved", capsules);
    }

    public async Task<(int StatusCode, object Response)> GetByIdAsync(Guid id, Guid userId)
    {
        var capsule = await _capsuleRepo.GetByIdAsync(id, userId);
        if (capsule == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("Capsule not found", null);
        }

        return BaseResponseHelpers.ReturnSuccess("Capsule retrieved", capsule);
    }

    public async Task<(int StatusCode, object Response)> UpdateAsync(Guid id, UpdateCapsuleRequest request, Guid userId)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid request", errors);
        }

        var capsule = await _capsuleRepo.UpdateAsync(id, userId, request.Name.Trim());
        if (capsule == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("Capsule not found", null);
        }

        _logger.LogInformation("[{RequestId}] User {UserId} updated capsule {CapsuleId}: {Capsule}", _authDetails.RequestId, userId, id, JsonSerializer.Serialize(capsule));
        return BaseResponseHelpers.ReturnSuccess("Capsule updated successfully", capsule);
    }

    public async Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId)
    {
        await _capsuleRepo.DeleteAsync(id, userId);
        _logger.LogInformation("[{RequestId}] User {UserId} deleted capsule {CapsuleId}", _authDetails.RequestId, userId, id);
        return BaseResponseHelpers.ReturnSuccess<object>("Capsule deleted successfully", null);
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SharedCapsuleDto> _sharedCapsules = new();

    public async Task<(int StatusCode, object Response)> BatchDeleteAsync(List<Guid> ids, Guid userId)
    {
        foreach (var id in ids)
        {
            await _capsuleRepo.DeleteAsync(id, userId);
        }
        _logger.LogInformation("[{RequestId}] User {UserId} batch-deleted {Count} capsules", _authDetails.RequestId, userId, ids.Count);
        return BaseResponseHelpers.ReturnSuccess<object>("Capsules deleted successfully", null);
    }

    public async Task<(int StatusCode, object Response)> ShareCapsuleAsync(Guid id, Guid userId)
    {
        var capsule = await _capsuleRepo.GetByIdAsync(id, userId);
        if (capsule == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("Capsule not found", null);
        }

        var requests = (await _requestRepo.GetAllByCapsuleAsync(id, userId)).ToList();
        foreach (var req in requests)
        {
            req.Params = (await _requestRepo.GetParamsByRequestAsync(req.Id)).ToList();
            req.Headers = (await _requestRepo.GetHeadersByRequestAsync(req.Id)).ToList();
            req.FormData = (await _requestRepo.GetFormDataByRequestAsync(req.Id)).ToList();
        }

        var token = Guid.NewGuid().ToString("N")[..12];
        var shared = new SharedCapsuleDto
        {
            Id = capsule.Id,
            Name = capsule.Name,
            ShareToken = token,
            CreatedAt = capsule.CreatedAt,
            Requests = requests
        };

        _sharedCapsules[token] = shared;

        return BaseResponseHelpers.ReturnSuccess("Capsule shared successfully", shared);
    }

    public async Task<(int StatusCode, object Response)> GetSharedCapsuleAsync(string token)
    {
        await Task.CompletedTask;
        if (_sharedCapsules.TryGetValue(token, out var shared))
        {
            return BaseResponseHelpers.ReturnSuccess("Shared capsule retrieved", shared);
        }
        return BaseResponseHelpers.ReturnNotFoundError("Shared capsule not found or expired", null);
    }
}
