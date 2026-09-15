using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Domain.Models.ScriptModels;

namespace OnSteroidsApiUser.WebApi.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class ScriptsController(IScriptService scriptService) : ControllerBase
{
    private readonly IScriptService _scriptService = scriptService;

    [HttpGet]
    public async Task<IActionResult> GetMyScripts()
    {
        var (statusCode, response) = await _scriptService.GetMyScriptsAsync();
        return StatusCode(statusCode, response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateScript([FromBody] ScriptDto script)
    {
        var (statusCode, response) = await _scriptService.CreateScriptAsync(script);
        return StatusCode(statusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateScript(Guid id, [FromBody] ScriptDto script)
    {
        var (statusCode, response) = await _scriptService.UpdateScriptAsync(id, script.Name, script.Content);
        return StatusCode(statusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScript(Guid id)
    {
        var (statusCode, response) = await _scriptService.DeleteScriptAsync(id);
        return StatusCode(statusCode, response);
    }

    [HttpPost("{id}/reset")]
    public async Task<IActionResult> ResetScript(Guid id)
    {
        var (statusCode, response) = await _scriptService.ResetScriptAsync(id);
        return StatusCode(statusCode, response);
    }
}
