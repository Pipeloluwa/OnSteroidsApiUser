using OnSteroidsApiUser.Domain.Models.ScriptModels;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IScriptRepository
{
    Task<IEnumerable<ScriptDto>> GetByUserIdAsync(Guid userId);
    Task<ScriptDto?> CreateAsync(ScriptDto script);
    Task<ScriptDto?> UpdateAsync(Guid id, string name, string content);
    Task DeleteAsync(Guid id);
    Task<ScriptDto?> ResetAsync(Guid id);
    Task<IEnumerable<ScriptDto>> SeedDefaultsForUserAsync(Guid userId);
}
