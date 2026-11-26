using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Request;
using System.Text.Json;

namespace OnSteroidsApiUser.Application.Services;

public class RequestService(
    IRequestRepository requestRepository,
    ICapsuleRepository capsuleRepository,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<RequestService> logger
) : IRequestService
{
    private readonly IRequestRepository _requestRepo = requestRepository;
    private readonly ICapsuleRepository _capsuleRepo = capsuleRepository;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<RequestService> _logger = logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static void HydrateJsonFields(RequestDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.CustomDocBlocksJson))
        {
            try
            {
                dto.CustomDocBlocks = JsonSerializer.Deserialize<List<CustomDocBlockDto>>(dto.CustomDocBlocksJson, _jsonOptions) ?? [];
            }
            catch
            {
                dto.CustomDocBlocks ??= [];
            }
        }
        else
        {
            dto.CustomDocBlocks ??= [];
        }

        if (!string.IsNullOrWhiteSpace(dto.UrlEncodedDataJson))
        {
            try
            {
                dto.UrlEncodedData = JsonSerializer.Deserialize<List<KeyValueItem>>(dto.UrlEncodedDataJson, _jsonOptions) ?? [];
            }
            catch
            {
                dto.UrlEncodedData ??= [];
            }
        }
        else
        {
            dto.UrlEncodedData ??= [];
        }

        if (!string.IsNullOrWhiteSpace(dto.TriggerConfigJson))
        {
            try
            {
                dto.TriggerConfig = JsonSerializer.Deserialize<TriggerConfigDto>(dto.TriggerConfigJson, _jsonOptions);
            }
            catch
            {
                // leave null on malformed payload
            }
        }
    }

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
            Description = req.Description,
            DocNotes = req.DocNotes,
            TestRailPreconditions = req.TestRailPreconditions,
            TestRailSteps = req.TestRailSteps,
            TestRailExpected = req.TestRailExpected,
            CustomDocBlocksJson = req.CustomDocBlocks != null
                ? JsonSerializer.Serialize(req.CustomDocBlocks, _jsonOptions)
                : null,
            Url = req.Url ?? string.Empty,
            Method = string.IsNullOrWhiteSpace(req.Method) ? "GET" : req.Method.ToUpperInvariant(),
            PayloadType = req.PayloadType ?? "params",
            BodyType = req.BodyType ?? "none",
            RawType = req.RawType ?? "JSON",
            RawBody = req.RawBody,
            RawBodyJson = req.RawBodyJson,
            RawBodyXml = req.RawBodyXml,
            RawBodyText = req.RawBodyText,
            UrlEncodedDataJson = req.UrlEncodedData != null
                ? JsonSerializer.Serialize(req.UrlEncodedData, _jsonOptions)
                : null,
            GraphqlQuery = req.GraphqlQuery,
            GraphqlVariables = req.GraphqlVariables,
            AuthType = req.Auth?.Type ?? "none",
            AuthToken = req.Auth?.Token,
            PreRequestScript = req.Scripts?.PreRequest,
            PostResponseScript = req.Scripts?.PostResponse,
            TestScript = req.Scripts?.TestScript,
            TestScriptEnabled = req.Scripts?.TestScriptEnabled ?? false,
            PreRequestLanguage = req.PreRequestLanguage ?? req.Scripts?.PreRequestLanguage ?? "javascript",
            PostResponseLanguage = req.PostResponseLanguage ?? req.Scripts?.PostResponseLanguage ?? "javascript",
            TestScriptLanguage = req.TestScriptLanguage ?? req.Scripts?.TestScriptLanguage ?? "javascript",
            EncryptionAlgorithm = req.Encryption?.Algorithm ?? "none",
            EncryptionKey = req.Encryption?.Key,
            AutoEncryptBody = req.Encryption?.AutoEncryptBody ?? false,
            AutoEncryptHeaders = req.Encryption?.AutoEncryptHeaders ?? false,
            EncryptionChannelName = req.Encryption?.ChannelName,
            EncryptedHeaders = req.Encryption?.EncryptedHeaders != null
                ? JsonSerializer.Serialize(req.Encryption.EncryptedHeaders)
                : null,
            EncryptedBodyPaths = req.Encryption?.EncryptedBodyPaths != null
                ? JsonSerializer.Serialize(req.Encryption.EncryptedBodyPaths)
                : null,
            EncryptionScript = req.Encryption?.Script,
            EncryptionScriptLanguage = req.EncryptionScriptLanguage ?? req.Encryption?.ScriptLanguage ?? "javascript",
            FollowRedirects = req.Settings?.FollowRedirects ?? true,
            VerifySsl = req.Settings?.VerifySsl ?? true,
            EnableCookies = req.Settings?.EnableCookies ?? true,
            BypassCors = req.Settings?.BypassCors ?? true,
            PostTriggerTabId = req.PostTriggerTabId,
            TriggerConfigJson = req.TriggerConfig != null
                ? JsonSerializer.Serialize(req.TriggerConfig, _jsonOptions)
                : null,
            AutoAuthEnabled = req.AutoAuthEnabled ?? false
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
            if (!string.IsNullOrWhiteSpace(req.Id) && Guid.TryParse(req.Id, out var newGuid))
            {
                dto.Id = newGuid;
            }
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

        // Preserve request payload values if the connected DB schema has not yet been migrated to the new columns
        savedReq.Description ??= dto.Description;
        savedReq.DocNotes ??= dto.DocNotes;
        savedReq.TestRailPreconditions ??= dto.TestRailPreconditions;
        savedReq.TestRailSteps ??= dto.TestRailSteps;
        savedReq.TestRailExpected ??= dto.TestRailExpected;
        savedReq.CustomDocBlocksJson ??= dto.CustomDocBlocksJson;
        savedReq.RawBodyText ??= dto.RawBodyText;
        savedReq.UrlEncodedDataJson ??= dto.UrlEncodedDataJson;
        savedReq.GraphqlQuery ??= dto.GraphqlQuery;
        savedReq.GraphqlVariables ??= dto.GraphqlVariables;
        savedReq.PreRequestLanguage ??= dto.PreRequestLanguage;
        savedReq.PostResponseLanguage ??= dto.PostResponseLanguage;
        savedReq.TestScriptLanguage ??= dto.TestScriptLanguage;
        savedReq.EncryptionScriptLanguage ??= dto.EncryptionScriptLanguage;
        savedReq.EncryptionChannelName ??= dto.EncryptionChannelName;
        savedReq.TriggerConfigJson ??= dto.TriggerConfigJson;
        HydrateJsonFields(savedReq);

        var sanitizedReq = new
        {
            savedReq.Id,
            savedReq.CapsuleId,
            savedReq.UserId,
            savedReq.Name,
            savedReq.Url,
            savedReq.Method,
            savedReq.PayloadType,
            savedReq.BodyType,
            savedReq.RawType,
            savedReq.AuthType,
            AuthToken = string.IsNullOrEmpty(savedReq.AuthToken) ? null : "***",
            savedReq.EncryptionAlgorithm,
            EncryptionKey = string.IsNullOrEmpty(savedReq.EncryptionKey) ? null : "***",
            ParamsCount = savedReq.Params.Count,
            HeadersCount = savedReq.Headers.Count,
            FormDataCount = savedReq.FormData.Count,
            savedReq.UpdatedAt
        };
        _logger.LogInformation("[{RequestId}] Successfully saved request {RequestIdModel} ({Name}): {Model}", _authDetails.RequestId, savedReq.Id, savedReq.Name, JsonSerializer.Serialize(sanitizedReq));
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
        HydrateJsonFields(req);

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
            HydrateJsonFields(req);
        }

        return BaseResponseHelpers.ReturnSuccess("Requests retrieved", requests);
    }

    public async Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId)
    {
        await _requestRepo.DeleteAsync(id, userId);
        _logger.LogInformation("[{RequestId}] User {UserId} deleted request {RequestIdModel}", _authDetails.RequestId, userId, id);
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
        HydrateJsonFields(duplicated);

        _logger.LogInformation("[{RequestId}] User {UserId} duplicated request {SourceId} to {NewId}", _authDetails.RequestId, userId, sourceId, duplicated.Id);
        return BaseResponseHelpers.ReturnSuccess("Request duplicated successfully", duplicated);
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, List<RequestExampleDto>> _examples = new();

    public async Task<(int StatusCode, object Response)> SaveBatchRequestStateAsync(List<SaveRequestStateRequest> requests, Guid userId)
    {
        var savedList = new List<RequestDto>();
        foreach (var req in requests)
        {
            var (status, response) = await SaveRequestStateAsync(req, userId);
            if (status == 200 && response is Domain.Models.Common.BaseModels.Responses.BaseSuccessResponse<RequestDto> success && success.data != null)
            {
                savedList.Add(success.data);
            }
        }
        return BaseResponseHelpers.ReturnSuccess("Batch requests saved successfully", savedList);
    }

    public async Task<(int StatusCode, object Response)> BatchDeleteAsync(List<Guid> ids, Guid userId)
    {
        foreach (var id in ids)
        {
            await _requestRepo.DeleteAsync(id, userId);
            _examples.TryRemove(id, out _);
        }
        _logger.LogInformation("[{RequestId}] User {UserId} batch-deleted {Count} requests", _authDetails.RequestId, userId, ids.Count);
        return BaseResponseHelpers.ReturnSuccess<object>("Requests deleted successfully", null);
    }

    public async Task<(int StatusCode, object Response)> CreateExampleAsync(Guid requestId, CreateRequestExampleRequest request, Guid userId)
    {
        await Task.CompletedTask;
        var example = new RequestExampleDto
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Name = string.IsNullOrWhiteSpace(request.Name) ? "Example" : request.Name.Trim(),
            RequestSnapshot = request.RequestSnapshot,
            ResponseStatus = request.ResponseStatus,
            ResponseBody = request.ResponseBody,
            ResponseHeaders = request.ResponseHeaders,
            CreatedAt = DateTime.UtcNow
        };

        _examples.AddOrUpdate(requestId, [example], (_, list) =>
        {
            lock (list)
            {
                list.Add(example);
                return list;
            }
        });

        _logger.LogInformation("[{RequestId}] User {UserId} created example {ExampleId} for request {ReqId}", _authDetails.RequestId, userId, example.Id, requestId);
        return BaseResponseHelpers.ReturnSuccess("Example created successfully", example);
    }

    public async Task<(int StatusCode, object Response)> GetExamplesByRequestAsync(Guid requestId, Guid userId)
    {
        await Task.CompletedTask;
        var list = _examples.TryGetValue(requestId, out var exList) ? exList.ToList() : [];
        return BaseResponseHelpers.ReturnSuccess("Examples retrieved", list);
    }

    public async Task<(int StatusCode, object Response)> DeleteExampleAsync(Guid exampleId, Guid userId)
    {
        await Task.CompletedTask;
        foreach (var kvp in _examples)
        {
            lock (kvp.Value)
            {
                kvp.Value.RemoveAll(e => e.Id == exampleId);
            }
        }
        return BaseResponseHelpers.ReturnSuccess<object>("Example deleted successfully", null);
    }
}

