using OnSteroidsApiUser.Domain.Models.License;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;

public interface ILicenseService
{
    Task<(int StatusCode, object Response)> GetStatusAsync(Guid userId);
    Task<(int StatusCode, object Response)> ActivateAsync(Guid userId, string userEmail, ActivateLicenseRequest request);
    Task<(int StatusCode, object Response)> DeactivateAsync(Guid userId);
    Task<(int StatusCode, object Response)> IssueAsync(IssueLicenseRequest request, string? adminKeyHeader);
    Task<(int StatusCode, object Response)> RevokeAsync(RevokeLicenseRequest request, string? adminKeyHeader);
}
