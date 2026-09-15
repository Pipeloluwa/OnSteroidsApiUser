using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using OnSteroidsApiUser.Domain.Models.ScriptModels;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IScriptService
{
    Task<(int StatusCode, object Response)> GetMyScriptsAsync();
    Task<(int StatusCode, object Response)> CreateScriptAsync(ScriptDto script);
    Task<(int StatusCode, object Response)> UpdateScriptAsync(Guid id, string name, string content);
    Task<(int StatusCode, object Response)> DeleteScriptAsync(Guid id);
    Task<(int StatusCode, object Response)> ResetScriptAsync(Guid id);
}
