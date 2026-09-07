using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Request;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class RequestService(
    IRequestRepository requestRepository,
    ICapsuleRepository capsuleRepository,
    ILogger<RequestService> logger
) : IRequestService
{
    private readonly IRequestRepository _requestRepo = requestRepository;
    private readonly ICapsuleRepository _capsuleRepo = capsuleRepository;
    private readonly ILogger<RequestService> _logger = logger;

    public async Task<(int StatusCode, object Response)> SaveRequestStateAsync(SaveRequestStateRequest req, Guid userId)
    {
        // 1. Resolve CapsuleId
        Guid capsuleId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(req.CapsuleId) && Guid.TryParse(req.CapsuleId, out var parsedCapId))
        {
            capsuleId = parsedCapId;
        }
        else
        {
            // Find existing user capsule or create one
            var existingCapsules = (await _capsuleRepo.GetAllByUserAsync(userId)).ToList();
            if (existingCapsules.Count > 0)
            {
                var match = !string.IsNullOrWhiteSpace(req.CapsuleName)
                    ? existingCapsules.FirstOrDefault(c => c.Name.Equals(req.CapsuleName, StringComparison.OrdinalIgnoreCase))
                    : null;
                capsuleId = match?.Id ?? existingCapsules[0].Id;
            }
            else
            {
                var newCapsule = await _capsuleRepo.CreateAsync(userId, req.CapsuleName ?? "My Capsule");
                if (newCapsule != null)
                {
                    capsuleId = newCapsule.Id;
                }
            }
        }

        // 2. Prepare RequestDto
        var dto = new RequestDto
        {
            CapsuleId = capsuleId,
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(req.Name) ? "New Request" : req.Name,
            Url = req.Url ?? string.Empty,
            Method = string.IsNullOrWhiteSpace(req.Method) ? "GET" : req.Method.ToUpperInvariant(),
            PayloadType = req.PayloadType ?? "params",
            BodyType = req.BodyType ?? "none",
            RawType = req.RawType ?? "JSON",
            RawBody = req.RawBody,
            RawBodyJson = req.RawBodyJson,
            RawBodyXml = req.RawBodyXml,
            AuthType = req.Auth?.Type ?? "none",
            AuthToken = req.Auth?.Token,
            PreRequestScript = req.Scripts?.PreRequest,
            PostResponseScript = req.Scripts?.PostResponse,
            EncryptionAlgorithm = req.Encryption?.Algorithm ?? "none",
            EncryptionKey = req.Encryption?.Key,
            AutoEncryptBody = req.Encryption?.AutoEncryptBody ?? false,
            AutoEncryptHeaders = req.Encryption?.AutoEncryptHeaders ?? false,
            EncryptionChannel = req.Encryption?.ChannelName,
            EncryptedHeaders = req.Encryption?.EncryptedHeaders != null
                ? JsonSerializer.Serialize(req.Encryption.EncryptedHeaders)
                : null,
            EncryptedBodyPaths = req.Encryption?.EncryptedBodyPaths != null
                ? JsonSerializer.Serialize(req.Encryption.EncryptedBodyPaths)
                : null,
            EncryptionScript = req.Encryption?.Script,
            FollowRedirects = req.Settings?.FollowRedirects ?? true,
            VerifySsl = req.Settings?.VerifySsl ?? true,
            EnableCookies = req.Settings?.EnableCookies ?? true,
            BypassCors = req.Settings?.BypassCors ?? true
        };

        RequestDto? savedReq = null;
        if (!string.IsNullOrWhiteSpace(req.Id) && Guid.TryParse(req.Id, out var existingId))
        {
            var existing = await _requestRepo.GetByIdAsync(existingId, userId);
            if (existing != null)
            {
                dto.Id = existingId;
                savedReq = await _requestRepo.UpdateAsync(dto);
            }
        }

        if (savedReq == null)
        {
            savedReq = await _requestRepo.CreateAsync(dto);
        }

        if (savedReq == null)
        {
            return BaseResponseHelpers.ReturnServerErrorData("Failed to save request state", null);
        }

        // Sync Child Collections (Params, Headers, FormData)
        // Params
        var paramsList = (req.Params ?? [])
            .Select((p, idx) => new { isEnabled = p.Enabled, key = p.Key, value = p.Value, sortOrder = idx })
            .ToList();
        var jsonParams = JsonSerializer.Serialize(paramsList);
        savedReq.Params = (await _requestRepo.SyncParamsAsync(savedReq.Id, jsonParams)).ToList();

        // Headers
        var headersList = (req.Headers ?? [])
            .Select((h, idx) => new { isEnabled = h.Enabled, key = h.Key, value = h.Value, sortOrder = idx })
            .ToList();
        var jsonHeaders = JsonSerializer.Serialize(headersList);
        savedReq.Headers = (await _requestRepo.SyncHeadersAsync(savedReq.Id, jsonHeaders)).ToList();

        // FormData
        var formDataList = (req.FormData ?? [])
            .Select((f, idx) => new { isEnabled = f.Enabled, key = f.Key, value = f.Value, type = f.Type, sortOrder = idx })
            .ToList();
        var jsonFormData = JsonSerializer.Serialize(formDataList);
        savedReq.FormData = (await _requestRepo.SyncFormDataAsync(savedReq.Id, jsonFormData)).ToList();

        _logger.LogInformation("Successfully saved request {RequestId} ({Name})", savedReq.Id, savedReq.Name);
        return BaseResponseHelpers.ReturnSuccess("Request saved successfully", savedReq);
    }

    public async Task<(int StatusCode, object Response)> GetByIdAsync(Guid id, Guid userId)
    {
        var req = await _requestRepo.GetByIdAsync(id, userId);
        if (req == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("Request not found", null);
        }

        req.Params = (await _requestRepo.GetParamsByRequestAsync(id)).ToList();
        req.Headers = (await _requestRepo.GetHeadersByRequestAsync(id)).ToList();
        req.FormData = (await _requestRepo.GetFormDataByRequestAsync(id)).ToList();

        return BaseResponseHelpers.ReturnSuccess("Request retrieved", req);
    }

    public async Task<(int StatusCode, object Response)> GetAllByCapsuleAsync(Guid capsuleId, Guid userId)
    {
        var requests = (await _requestRepo.GetAllByCapsuleAsync(capsuleId, userId)).ToList();
        foreach (var req in requests)
        {
            req.Params = (await _requestRepo.GetParamsByRequestAsync(req.Id)).ToList();
            req.Headers = (await _requestRepo.GetHeadersByRequestAsync(req.Id)).ToList();
            req.FormData = (await _requestRepo.GetFormDataByRequestAsync(req.Id)).ToList();
        }

        return BaseResponseHelpers.ReturnSuccess("Requests retrieved", requests);
    }

    public async Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId)
    {
        await _requestRepo.DeleteAsync(id, userId);
        return BaseResponseHelpers.ReturnSuccess<object>("Request deleted successfully", null);
    }

    public async Task<(int StatusCode, object Response)> DuplicateAsync(Guid sourceId, Guid userId)
    {
        var duplicated = await _requestRepo.DuplicateAsync(sourceId, userId);
        if (duplicated == null)
        {
            return BaseResponseHelpers.ReturnNotFoundError("Source request not found", null);
        }

        duplicated.Params = (await _requestRepo.GetParamsByRequestAsync(duplicated.Id)).ToList();
        duplicated.Headers = (await _requestRepo.GetHeadersByRequestAsync(duplicated.Id)).ToList();
        duplicated.FormData = (await _requestRepo.GetFormDataByRequestAsync(duplicated.Id)).ToList();

        return BaseResponseHelpers.ReturnSuccess("Request duplicated successfully", duplicated);
    }
}
