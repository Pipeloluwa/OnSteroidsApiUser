using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.Import;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ImportController(
    IImportService importService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IImportService _importService = importService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    /// <summary>
    /// Imports one or multiple capsules with all their requests and optional variables in a single payload.
    /// Calls the existing CapsuleService, RequestService, and VariableService under the hood.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseSuccessResponse<ImportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import([FromBody] ImportPayloadRequest request)
    {
        var (status, response) = await _importService.ImportAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }
}
