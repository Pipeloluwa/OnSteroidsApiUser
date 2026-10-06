using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.WorkspaceState;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WorkspaceStateController(
    IWorkspaceStateService workspaceStateService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IWorkspaceStateService _service = workspaceStateService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertWorkspaceStateRequest request)
    {
        var (status, response) = await _service.UpsertAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var (status, response) = await _service.GetByUserAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var (status, response) = await _service.DeleteAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }
}
