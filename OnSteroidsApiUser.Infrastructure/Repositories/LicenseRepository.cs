using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.License;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class LicenseRepository(
    IDapperConnection dapperConnection,
    ILogger<LicenseRepository> logger
) : ILicenseRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<LicenseRepository> _logger = logger;

    public async Task<LicenseKeyRecord?> CreateKeyAsync(LicenseKeyRecord record)
    {
        _logger.LogInformation("Issuing license key {MaskedCode} ({Tier}/{LicenseType}/{Cycle})", record.MaskedCode, record.Tier, record.LicenseType, record.BillingCycle);
        var p = new DynamicParameters();
        p.Add("@Id", record.Id != Guid.Empty ? record.Id : Guid.NewGuid());
        p.Add("@LicenseKey", record.LicenseKey);
        p.Add("@CodeHash", record.CodeHash);
        p.Add("@MaskedCode", record.MaskedCode);
        p.Add("@Tier", record.Tier);
        p.Add("@LicenseType", record.LicenseType);
        p.Add("@IndividualType", record.IndividualType);
        p.Add("@BillingCycle", record.BillingCycle);
        p.Add("@DurationDays", record.DurationDays);
        p.Add("@MaxActivations", record.MaxActivations);
        p.Add("@AllowedEmail", record.AllowedEmail);
        p.Add("@Notes", record.Notes);
        p.Add("@ExpiresAt", record.ExpiresAt);
        return await _dapper.Query<LicenseKeyRecord>(p, "dbo.spLicense_Create");
    }

    public async Task<LicenseStatusDto?> GetStatusByUserIdAsync(Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        return await _dapper.Query<LicenseStatusDto>(p, "dbo.spLicense_GetStatus");
    }

    public async Task<LicenseStatusDto?> ActivateAsync(Guid userId, string userEmail, string codeHash, string? rawCode, string? deviceLabel)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        p.Add("@UserEmail", userEmail);
        p.Add("@CodeHash", codeHash);
        p.Add("@NormalizedCode", rawCode);
        p.Add("@DeviceLabel", deviceLabel);
        return await _dapper.Query<LicenseStatusDto>(p, "dbo.spLicense_Activate");
    }

    public async Task<LicenseStatusDto?> DeactivateAsync(Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@UserId", userId);
        return await _dapper.Query<LicenseStatusDto>(p, "dbo.spLicense_Deactivate");
    }

    public async Task<int> RevokeAsync(Guid? licenseKeyId, string? codeHash)
    {
        var p = new DynamicParameters();
        p.Add("@LicenseKeyId", licenseKeyId);
        p.Add("@CodeHash", codeHash);
        return await _dapper.Execute(p, "dbo.spLicense_Revoke");
    }
}
