using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IVariableService
{
    Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId, Guid? capsuleId = null);
    Task<(int StatusCode, object Response)> SyncVariablesAsync(SyncVariablesRequest request, Guid userId);
}
