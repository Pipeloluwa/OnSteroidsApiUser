using OnSteroidsApiUser.Domain.Models.License;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface ILicenseRepository
{
    Task<LicenseKeyRecord?> CreateKeyAsync(LicenseKeyRecord record);
    Task<LicenseStatusDto?> GetStatusByUserIdAsync(Guid userId);
    Task<LicenseStatusDto?> ActivateAsync(Guid userId, string userEmail, string codeHash, string? rawCode, string? deviceLabel);
    Task<LicenseStatusDto?> DeactivateAsync(Guid userId);
    Task<int> RevokeAsync(Guid? licenseKeyId, string? codeHash);
}
