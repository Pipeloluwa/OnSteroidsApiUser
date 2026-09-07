using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VariableController(
    IVariableService variableService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IVariableService _variableService = variableService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpGet]
    [ProducesResponseType(typeof(BaseSuccessResponse<IEnumerable<VariableDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var (status, response) = await _variableService.GetAllByUserAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("sync")]
    [ProducesResponseType(typeof(BaseSuccessResponse<IEnumerable<VariableDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync([FromBody] SyncVariablesRequest request)
    {
        var (status, response) = await _variableService.SyncVariablesAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }
}
