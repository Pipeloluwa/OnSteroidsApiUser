using OnSteroidsApiUser.Domain.Models.Import;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface IImportService
{
    Task<(int StatusCode, object Response)> ImportAsync(ImportPayloadRequest payload, Guid userId);
}
