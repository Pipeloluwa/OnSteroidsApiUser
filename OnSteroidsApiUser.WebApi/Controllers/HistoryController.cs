using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.History;

namespace OnSteroidsApiUser.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HistoryController(
    IRequestHistoryService historyService,
    IAuthDetailsHelper authDetails
) : ControllerBase
{
    private readonly IRequestHistoryService _historyService = historyService;
    private readonly IAuthDetailsHelper _authDetails = authDetails;

    [HttpGet]
    [ProducesResponseType(typeof(BaseSuccessResponse<IEnumerable<RequestHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100)
    {
        var (status, response) = await _historyService.GetAllByUserAsync(_authDetails.UserId, limit);
        return StatusCode(status, response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseSuccessResponse<RequestHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateHistoryRequest request)
    {
        var (status, response) = await _historyService.CreateHistoryAsync(request, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (status, response) = await _historyService.DeleteAsync(id, _authDetails.UserId);
        return StatusCode(status, response);
    }

    [HttpDelete("clear")]
    [ProducesResponseType(typeof(BaseSuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear()
    {
        var (status, response) = await _historyService.ClearAsync(_authDetails.UserId);
        return StatusCode(status, response);
    }
}
