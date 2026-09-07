using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IVariableRepository
{
    Task<VariableDto?> CreateAsync(Guid userId, string key, string value, bool isEnabled);
    Task<IEnumerable<VariableDto>> GetAllByUserAsync(Guid userId);
    Task<VariableDto?> UpdateAsync(Guid id, Guid userId, string? key, string? value, bool? isEnabled);
    Task DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<VariableDto>> SyncAsync(Guid userId, string jsonData);
}
