using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IVariableRepository
{
    Task<VariableDto?> CreateAsync(Guid userId, Guid? capsuleId, string key, string value, string type, bool isEnabled);
    Task<IEnumerable<VariableDto>> GetAllByUserAsync(Guid userId, Guid? capsuleId = null);
    Task<VariableDto?> UpdateAsync(Guid id, Guid userId, Guid? capsuleId, string? key, string? value, string? type, bool? isEnabled);
    Task DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<VariableDto>> SyncAsync(Guid userId, Guid? capsuleId, string jsonData);
}
