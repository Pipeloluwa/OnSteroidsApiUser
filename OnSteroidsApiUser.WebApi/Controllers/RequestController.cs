using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RequestController(
    IRequestService requestService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IRequestService _requestService = requestService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    /// <summary>
    /// Saves or updates the entire request and child components (params, headers, formData).
    /// Called only when the user clicks the Save button in the frontend.
    /// </summary>
    [HttpPost("save")]
    [ProducesResponseType(typeof(BaseSuccessResponse<RequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Save([FromBody] SaveRequestStateRequest request)
    {
        var (status, response) = await _requestService.SaveRequestStateAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpGet("capsule/{capsuleId:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<IEnumerable<RequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByCapsule(Guid capsuleId)
    {
        var (status, response) = await _requestService.GetAllByCapsuleAsync(capsuleId, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<RequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var (status, response) = await _requestService.GetByIdAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (status, response) = await _requestService.DeleteAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("duplicate/{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<RequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var (status, response) = await _requestService.DuplicateAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }
}
