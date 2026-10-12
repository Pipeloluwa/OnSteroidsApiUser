using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.Capsule;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.Import;
using OnSteroidsApiUser.Domain.Models.Request;
using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Application.Services;

public class ImportService(
    ICapsuleService capsuleService,
    IRequestService requestService,
    IVariableService variableService,
    IAuthDetailsHelper authDetailsHelper,
    ILogger<ImportService> logger
) : IImportService
{
    private readonly ICapsuleService _capsuleService = capsuleService;
    private readonly IRequestService _requestService = requestService;
    private readonly IVariableService _variableService = variableService;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly ILogger<ImportService> _logger = logger;

    public async Task<(int StatusCode, object Response)> ImportAsync(ImportPayloadRequest payload, Guid userId)
    {
        if (payload == null)
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError("Invalid import payload", ["Payload cannot be null."]);
        }

        // Normalize capsules list: if payload.Capsules is empty but payload.Requests is provided, wrap into a capsule
        var capsuleItems = new List<ImportCapsuleItemRequest>(payload.Capsules ?? []);
        if (capsuleItems.Count == 0 && payload.Requests != null && payload.Requests.Count > 0)
        {
            capsuleItems.Add(new ImportCapsuleItemRequest
            {
                Name = string.IsNullOrWhiteSpace(payload.Name) ? "Imported Capsule" : payload.Name.Trim(),
                AutoAuthEnabled = payload.AutoAuthEnabled ?? "off",
                AutoAuthEndpointId = payload.AutoAuthEndpointId,
                Requests = payload.Requests
            });
        }

        if (capsuleItems.Count == 0)
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError("No capsules or requests provided in import payload", ["Import payload must contain at least one capsule or request."]);
        }

        var importedCapsules = new List<ImportedCapsuleResultDto>();
        int totalRequests = 0;

        foreach (var capItem in capsuleItems)
        {
            Guid? capGuid = null;
            if (!string.IsNullOrWhiteSpace(capItem.Id) && Guid.TryParse(capItem.Id, out var parsedGuid))
            {
                capGuid = parsedGuid;
            }

            var createCapReq = new CreateCapsuleRequest
            {
                Id = capGuid,
                Name = string.IsNullOrWhiteSpace(capItem.Name) ? "Imported Capsule" : capItem.Name.Trim(),
                AutoAuthEnabled = capItem.AutoAuthEnabled ?? "off",
                AutoAuthEndpointId = capItem.AutoAuthEndpointId
            };

            // Call already created CapsuleService
            var (capStatus, capResponse) = await _capsuleService.CreateAsync(createCapReq, userId);
            if (capStatus != 200 || capResponse is not BaseSuccessResponse<CapsuleDto> capSuccess || capSuccess.data == null)
            {
                _logger.LogWarning("[{RequestId}] Failed to create capsule '{Name}' during import", _authDetails.RequestId, createCapReq.Name);
                continue;
            }

            var createdCapDto = capSuccess.data;
            var createdCapIdStr = createdCapDto.Id.ToString();
            var importedRequests = new List<RequestDto>();

            // For each request in the capsule, call already created RequestService
            foreach (var reqItem in capItem.Requests ?? [])
            {
                reqItem.CapsuleId = createdCapIdStr;
                reqItem.CapsuleName = createdCapDto.Name;
                if (string.IsNullOrWhiteSpace(reqItem.Name))
                {
                    reqItem.Name = "Imported Request";
                }

                var (reqStatus, reqResponse) = await _requestService.SaveRequestStateAsync(reqItem, userId);
                if (reqStatus == 200 && reqResponse is BaseSuccessResponse<RequestDto> reqSuccess && reqSuccess.data != null)
                {
                    importedRequests.Add(reqSuccess.data);
                    totalRequests++;
                }
                else
                {
                    _logger.LogWarning("[{RequestId}] Failed to save request '{Name}' during import for capsule {CapsuleId}", _authDetails.RequestId, reqItem.Name, createdCapIdStr);
                }
            }

            // Sync variables for this capsule if provided
            if (payload.Variables != null && payload.Variables.Count > 0)
            {
                await _variableService.SyncVariablesAsync(new SyncVariablesRequest
                {
                    CapsuleId = createdCapDto.Id,
                    Variables = payload.Variables
                }, userId);
            }

            importedCapsules.Add(new ImportedCapsuleResultDto
            {
                Capsule = createdCapDto,
                Requests = importedRequests
            });
        }

        var result = new ImportResultDto
        {
            CapsulesCount = importedCapsules.Count,
            RequestsCount = totalRequests,
            Capsules = importedCapsules
        };

        _logger.LogInformation("[{RequestId}] Successfully imported {CapsuleCount} capsules and {RequestCount} requests for user {UserId}",
            _authDetails.RequestId, result.CapsulesCount, result.RequestsCount, userId);

        return BaseResponseHelpers.ReturnSuccess("Import completed successfully", result);
    }
}
