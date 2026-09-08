using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Capsule;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CapsuleController(
    ICapsuleService capsuleService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly ICapsuleService _capsuleService = capsuleService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpPost]
    [ProducesResponseType(typeof(BaseSuccessResponse<CapsuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateCapsuleRequest request)
    {
        var (status, response) = await _capsuleService.CreateAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseSuccessResponse<IEnumerable<CapsuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var (status, response) = await _capsuleService.GetAllByUserAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<CapsuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var (status, response) = await _capsuleService.GetByIdAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<CapsuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCapsuleRequest request)
    {
        var (status, response) = await _capsuleService.UpdateAsync(id, request, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (status, response) = await _capsuleService.DeleteAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("batch-delete")]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchDelete([FromBody] Domain.Models.Request.BatchDeleteRequest request)
    {
        var (status, response) = await _capsuleService.BatchDeleteAsync(request.Ids, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("{id:guid}/share")]
    [ProducesResponseType(typeof(BaseSuccessResponse<SharedCapsuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Share(Guid id)
    {
        var (status, response) = await _capsuleService.ShareCapsuleAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [AllowAnonymous]
    [HttpGet("shared/{token}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<SharedCapsuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShared(string token)
    {
        var (status, response) = await _capsuleService.GetSharedCapsuleAsync(token);
        return StatusCode(status, response);
    }
}
