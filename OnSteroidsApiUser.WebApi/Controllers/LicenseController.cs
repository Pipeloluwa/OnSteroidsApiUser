using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.License;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LicenseController(
    ILicenseService licenseService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly ILicenseService _licenseService = licenseService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpGet("status")]
    [Authorize]
    [ProducesResponseType(typeof(BaseSuccessResponse<LicenseStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status()
    {
        var (status, response) = await _licenseService.GetStatusAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("activate")]
    [Authorize]
    [ProducesResponseType(typeof(BaseSuccessResponse<LicenseStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
    {
        var (status, response) = await _licenseService.ActivateAsync(
            _authDetails.UserId,
            _authDetails.Email ?? string.Empty,
            request);
        return StatusCode(status, response);
    }

    [HttpPost("deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(BaseSuccessResponse<LicenseStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate()
    {
        var (status, response) = await _licenseService.DeactivateAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("issue")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseSuccessResponse<List<IssuedLicenseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Issue(
        [FromBody] IssueLicenseRequest request,
        [FromHeader(Name = "X-License-Admin-Key")] string? adminKey)
    {
        var (status, response) = await _licenseService.IssueAsync(request, adminKey);
        return StatusCode(status, response);
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeLicenseRequest request,
        [FromHeader(Name = "X-License-Admin-Key")] string? adminKey)
    {
        var (status, response) = await _licenseService.RevokeAsync(request, adminKey);
        return StatusCode(status, response);
    }
}
