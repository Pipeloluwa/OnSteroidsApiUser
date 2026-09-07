using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface IRequestRepository
{
    Task<RequestDto?> CreateAsync(RequestDto request);
    Task<RequestDto?> UpdateAsync(RequestDto request);
    Task<RequestDto?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<RequestDto>> GetAllByCapsuleAsync(Guid capsuleId, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<RequestDto?> DuplicateAsync(Guid sourceId, Guid userId);

    Task<IEnumerable<RequestParamDto>> SyncParamsAsync(Guid requestId, string jsonData);
    Task<IEnumerable<RequestParamDto>> GetParamsByRequestAsync(Guid requestId);

    Task<IEnumerable<RequestHeaderDto>> SyncHeadersAsync(Guid requestId, string jsonData);
    Task<IEnumerable<RequestHeaderDto>> GetHeadersByRequestAsync(Guid requestId);

    Task<IEnumerable<RequestFormDataDto>> SyncFormDataAsync(Guid requestId, string jsonData);
    Task<IEnumerable<RequestFormDataDto>> GetFormDataByRequestAsync(Guid requestId);
}
