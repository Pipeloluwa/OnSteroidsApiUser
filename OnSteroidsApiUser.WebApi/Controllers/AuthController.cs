using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Auth;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    IAuthService authService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpPost("send-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var (status, response) = await _authService.SendOtpAsync(request);
        return StatusCode(status, response);
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseSuccessResponse<AuthLoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var (status, response) = await _authService.VerifyOtpAsync(request);
        return StatusCode(status, response);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(BaseSuccessResponse<UserAuthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var (status, response) = await _authService.GetCurrentUserAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var (status, response) = await _authService.LogoutAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }
}
