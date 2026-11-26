using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;
using OnSteroidsApiUser.Domain.Models.License;

namespace OnSteroidsApiUser.Application.Services;

public class LicenseService(
    ILicenseRepository licenseRepository,
    IAuthDetailsHelper authDetailsHelper,
    IOptions<AppSettings> appSettings,
    ILogger<LicenseService> logger
) : ILicenseService
{
    private const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    private readonly ILicenseRepository _licenseRepo = licenseRepository;
    private readonly IAuthDetailsHelper _authDetails = authDetailsHelper;
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly ILogger<LicenseService> _logger = logger;

    public async Task<(int StatusCode, object Response)> GetStatusAsync(Guid userId)
    {
        var status = await _licenseRepo.GetStatusByUserIdAsync(userId) ?? new LicenseStatusDto
        {
            SubscriptionTier = "community",
            Status = "free",
            AccessSource = "free",
            IsActive = false,
            IsExpired = false
        };
        SanitizeStatus(status);
        return BaseResponseHelpers.ReturnSuccess("License status retrieved.", status);
    }

    public async Task<(int StatusCode, object Response)> ActivateAsync(Guid userId, string userEmail, ActivateLicenseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError(
                "Registration code is required.",
                ["Please enter your OnSteroid registration code."]);
        }

        var trimmedInput = request.Code.Trim().ToUpperInvariant().Replace(" ", "");
        string normalizedCode;
        if (trimmedInput.StartsWith("ONST", StringComparison.Ordinal))
        {
            if (!TryNormalizeAndValidateCode(request.Code, out normalizedCode, out _, out var validationError))
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData(
                    validationError ?? "Invalid registration code format.",
                    [validationError ?? "Invalid registration code format."]);
            }
        }
        else
        {
            // Support custom SQL-inserted admin license keys cleanly while still hashing consistently
            if (trimmedInput.Length < 8)
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData(
                    "Invalid registration code format.",
                    ["Expected ONST-PRO/ENT-... or a valid license key of at least 8 characters."]);
            }
            normalizedCode = trimmedInput;
        }

        var codeHash = ComputeSha256Hex(normalizedCode);

        try
        {
            var status = await _licenseRepo.ActivateAsync(
                userId,
                (userEmail ?? string.Empty).Trim().ToLowerInvariant(),
                codeHash,
                normalizedCode,
                string.IsNullOrWhiteSpace(request.DeviceLabel) ? null : request.DeviceLabel.Trim()[..Math.Min(150, request.DeviceLabel.Trim().Length)]);

            if (status == null)
            {
                return BaseResponseHelpers.ReturnValidationSemanticErrorData(
                    "Invalid or unavailable registration code.", null);
            }

            SanitizeStatus(status);

            _logger.LogInformation(
                "[{RequestId}] User {UserId} activated {Tier} ({LicenseType}/{IndividualType}) license {MaskedCode}",
                _authDetails.RequestId, userId, status.SubscriptionTier, status.LicenseType, status.IndividualType, status.MaskedCode);

            return BaseResponseHelpers.ReturnSuccess(
                $"Activated {FormatTierLabel(status.SubscriptionTier)} plan successfully!",
                status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{RequestId}] License activation rejected for user {UserId}", _authDetails.RequestId, userId);
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(ex.Message, [ex.Message]);
        }
    }

    public async Task<(int StatusCode, object Response)> DeactivateAsync(Guid userId)
    {
        var status = await _licenseRepo.DeactivateAsync(userId) ?? new LicenseStatusDto
        {
            SubscriptionTier = "community",
            Status = "free",
            AccessSource = "free",
            IsActive = false,
            IsExpired = false
        };
        SanitizeStatus(status);

        _logger.LogInformation("[{RequestId}] User {UserId} deactivated active license seat", _authDetails.RequestId, userId);
        return BaseResponseHelpers.ReturnSuccess("License deactivated. Account switched to Community (Free).", status);
    }

    public async Task<(int StatusCode, object Response)> IssueAsync(IssueLicenseRequest request, string? adminKeyHeader)
    {
        if (!IsValidAdminKey(adminKeyHeader))
        {
            return BaseResponseHelpers.ReturnUnauthorizedError("Valid X-License-Admin-Key header is required.", null);
        }

        var tier = (request?.Tier ?? string.Empty).Trim().ToLowerInvariant();
        if (tier != "professional" && tier != "enterprise")
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError(
                "Tier must be 'professional' or 'enterprise'.",
                ["Tier must be 'professional' or 'enterprise'."]);
        }

        var licenseType = string.IsNullOrWhiteSpace(request?.LicenseType)
            ? (tier == "enterprise" ? "enterprise" : "individual")
            : request!.LicenseType!.Trim().ToLowerInvariant();
        if (tier == "professional" && licenseType == "professional")
        {
            licenseType = "individual";
        }
        if (licenseType != "individual" && licenseType != "professional" && licenseType != "enterprise" && licenseType != "organization")
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError(
                "LicenseType must be 'individual', 'professional', 'enterprise', or 'organization'.",
                ["Unsupported LicenseType."]);
        }

        var individualType = string.IsNullOrWhiteSpace(request?.IndividualType)
            ? (tier == "professional" || licenseType == "individual" ? "personal" : null)
            : request!.IndividualType!.Trim().ToLowerInvariant();

        if (IsPersonalIndividualLicense(licenseType, individualType, tier) && (tier == "enterprise" || licenseType == "enterprise"))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(
                "Personal individual licenses cannot be issued as Enterprise tier or bypass Enterprise restrictions.",
                ["Personal individual licenses are restricted to non-Enterprise tiers."]);
        }

        var cycle = (request?.BillingCycle ?? "annual").Trim().ToLowerInvariant();
        if (cycle != "monthly" && cycle != "annual" && cycle != "lifetime" && cycle != "trial")
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError(
                "BillingCycle must be monthly, annual, lifetime, or trial.",
                ["Unsupported billing cycle."]);
        }

        int? durationDays = request?.DurationDays;
        if (!durationDays.HasValue)
        {
            durationDays = cycle switch
            {
                "trial" => 14,
                "monthly" => 30,
                "annual" => 365,
                _ => null
            };
        }

        var cleanAllowedEmail = string.IsNullOrWhiteSpace(request?.AllowedEmail)
            ? null
            : request!.AllowedEmail!.Trim().ToLowerInvariant();

        if (IsPersonalIndividualLicense(licenseType, individualType, tier) && IsDomainPattern(cleanAllowedEmail))
        {
            return BaseResponseHelpers.ReturnValidationSemanticErrorData(
                "Personal individual licenses cannot be scoped to an organization email domain.",
                ["Use LicenseType 'enterprise' or 'organization' with IndividualType 'organization' for domain-wide licenses."]);
        }

        var maxSeats = IsPersonalIndividualLicense(licenseType, individualType, tier)
            ? 1
            : (request?.MaxActivations > 0
                ? request.MaxActivations
                : (tier == "enterprise"
                    ? Math.Max(1, _appSettings.LicenseSettings?.DefaultMaxActivationsEnterprise ?? 10)
                    : Math.Max(1, _appSettings.LicenseSettings?.DefaultMaxActivationsPro ?? 2)));

        var qty = Math.Clamp(request?.Quantity ?? 1, 1, 50);

        var issued = new List<IssuedLicenseDto>(qty);
        for (int i = 0; i < qty; i++)
        {
            var rawCode = GenerateCode(tier);
            var codeHash = ComputeSha256Hex(rawCode);
            var masked = MaskCode(rawCode);

            var created = await _licenseRepo.CreateKeyAsync(new LicenseKeyRecord
            {
                Id = Guid.NewGuid(),
                LicenseKey = rawCode,
                CodeHash = codeHash,
                MaskedCode = masked,
                Tier = tier,
                LicenseType = licenseType,
                IndividualType = individualType,
                BillingCycle = cycle,
                DurationDays = durationDays,
                MaxActivations = maxSeats,
                AllowedEmail = cleanAllowedEmail,
                Notes = request?.Notes,
                ExpiresAt = request?.ExpiresAt
            });

            if (created != null)
            {
                issued.Add(new IssuedLicenseDto
                {
                    Id = created.Id,
                    Code = rawCode,
                    MaskedCode = masked,
                    Tier = created.Tier,
                    LicenseType = created.LicenseType,
                    IndividualType = created.IndividualType,
                    BillingCycle = created.BillingCycle,
                    DurationDays = created.DurationDays,
                    MaxActivations = created.MaxActivations,
                    AllowedEmail = created.AllowedEmail,
                    ExpiresAt = created.ExpiresAt,
                    CreatedAt = created.CreatedAt
                });
            }
        }

        _logger.LogInformation("Issued {Count} {Tier} ({LicenseType}/{IndividualType}) license key(s)", issued.Count, tier, licenseType, individualType);
        return BaseResponseHelpers.ReturnSuccess($"Issued {issued.Count} registration key(s).", issued);
    }

    public async Task<(int StatusCode, object Response)> RevokeAsync(RevokeLicenseRequest request, string? adminKeyHeader)
    {
        if (!IsValidAdminKey(adminKeyHeader))
        {
            return BaseResponseHelpers.ReturnUnauthorizedError("Valid X-License-Admin-Key header is required.", null);
        }

        string? codeHash = null;
        if (!string.IsNullOrWhiteSpace(request?.Code))
        {
            var clean = request.Code.Trim().ToUpperInvariant().Replace(" ", "");
            codeHash = ComputeSha256Hex(clean);
        }

        if (request?.LicenseKeyId == null && string.IsNullOrEmpty(codeHash))
        {
            return BaseResponseHelpers.ReturnValidationSyntaxError(
                "Provide either LicenseKeyId or Code to revoke.", null);
        }

        await _licenseRepo.RevokeAsync(request?.LicenseKeyId, codeHash);
        return BaseResponseHelpers.ReturnSuccess<object>("License revoked cleanly.", null);
    }

    private bool IsValidAdminKey(string? providedKey)
    {
        var configured = _appSettings.LicenseSettings?.AdminMasterKey;
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = Environment.GetEnvironmentVariable("ONSTEROIDS_LICENSE_ADMIN_KEY");
        }
        if (string.IsNullOrWhiteSpace(configured) || string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(configured.Trim());
        var actualBytes = Encoding.UTF8.GetBytes(providedKey.Trim());
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    public static string GenerateCode(string tier)
    {
        var tag = tier.Equals("enterprise", StringComparison.OrdinalIgnoreCase) ? "ENT" : "PRO";
        var g1 = RandomSegment(4);
        var g2 = RandomSegment(4);
        var g3 = RandomSegment(4);
        var check = ComputeChecksumSegment($"ONST-{tag}-{g1}-{g2}-{g3}");
        return $"ONST-{tag}-{g1}-{g2}-{g3}-{check}";
    }

    public static bool TryNormalizeAndValidateCode(string input, out string normalized, out string tier, out string? error)
    {
        normalized = string.Empty;
        tier = "community";
        error = null;

        var stripped = new string((input ?? string.Empty)
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        // Expected canonical compact length: ONST(4) + PRO/ENT(3) + 4*4(16) = 23 chars
        if (stripped.Length != 23 || !stripped.StartsWith("ONST", StringComparison.Ordinal))
        {
            error = "Invalid format. Expected ONST-PRO-XXXX-XXXX-XXXX-XXXX or ONST-ENT-XXXX-XXXX-XXXX-XXXX.";
            return false;
        }

        var tag = stripped.Substring(4, 3);
        if (tag != "PRO" && tag != "ENT")
        {
            error = "Invalid plan prefix. Registration code must contain -PRO- or -ENT-.";
            return false;
        }

        var g1 = stripped.Substring(7, 4);
        var g2 = stripped.Substring(11, 4);
        var g3 = stripped.Substring(15, 4);
        var g4 = stripped.Substring(19, 4);

        var body = $"ONST-{tag}-{g1}-{g2}-{g3}";
        var expectedCheck = ComputeChecksumSegment(body);
        if (!string.Equals(g4, expectedCheck, StringComparison.Ordinal))
        {
            error = "Registration code checksum failed. Please check for typos and try again.";
            return false;
        }

        normalized = $"{body}-{g4}";
        tier = tag == "ENT" ? "enterprise" : "professional";
        return true;
    }

    private static string RandomSegment(int len)
    {
        Span<char> chars = stackalloc char[len];
        for (int i = 0; i < len; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }
        return new string(chars);
    }

    private static string ComputeChecksumSegment(string payload)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"OnSteroidLicense::{payload}"));
        Span<char> check = stackalloc char[4];
        for (int i = 0; i < 4; i++)
        {
            check[i] = Alphabet[hash[i] % Alphabet.Length];
        }
        return new string(check);
    }

    private static string ComputeSha256Hex(string normalizedCode)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedCode));
        return Convert.ToHexString(hash);
    }

    private static string MaskCode(string normalizedCode)
    {
        var parts = normalizedCode.Split('-');
        return parts.Length == 6
            ? $"{parts[0]}-{parts[1]}-••••-••••-{parts[4]}-{parts[5]}"
            : "ONST-••••-••••";
    }

    public static bool IsPersonalIndividualLicense(string? licenseType, string? individualType, string? tier = null)
    {
        var ind = (individualType ?? string.Empty).Trim().ToLowerInvariant();
        if (ind == "personal") return true;
        var lic = (licenseType ?? string.Empty).Trim().ToLowerInvariant();
        if (lic == "individual" && string.IsNullOrEmpty(ind)) return true;
        var cleanTier = (tier ?? string.Empty).Trim().ToLowerInvariant();
        return cleanTier == "professional" && ind != "organization";
    }

    public static bool IsDomainPattern(string? emailPattern)
    {
        if (string.IsNullOrWhiteSpace(emailPattern)) return false;
        var clean = emailPattern.Trim().ToLowerInvariant();
        if (clean.StartsWith("*@", StringComparison.Ordinal)) clean = clean[1..];
        var atIdx = clean.IndexOf('@');
        return atIdx <= 0; // either "@domain.com" (atIdx == 0) or "domain.com" (atIdx == -1)
    }

    public static string EnforceEffectiveTier(string? tier, string? licenseType, string? individualType)
    {
        var cleanTier = (tier ?? "community").Trim().ToLowerInvariant();
        if (IsPersonalIndividualLicense(licenseType, individualType, cleanTier) && cleanTier == "enterprise")
        {
            return "professional";
        }
        return cleanTier is "enterprise" or "professional" ? cleanTier : "community";
    }

    public static void SanitizeStatus(LicenseStatusDto status)
    {
        var rawTier = (status.SubscriptionTier ?? "community").Trim().ToLowerInvariant();
        var cleanLic = string.IsNullOrWhiteSpace(status.LicenseType) ? null : status.LicenseType.Trim().ToLowerInvariant();
        var cleanInd = string.IsNullOrWhiteSpace(status.IndividualType) ? null : status.IndividualType.Trim().ToLowerInvariant();
        var isPersonal = IsPersonalIndividualLicense(cleanLic, cleanInd, rawTier);
        var effectiveTier = EnforceEffectiveTier(rawTier, cleanLic, cleanInd);
        if (effectiveTier == "professional" && isPersonal)
        {
            cleanLic ??= "individual";
            cleanInd ??= "personal";
        }
        status.LicenseType = cleanLic;
        status.IndividualType = cleanInd;
        status.IsPersonalLicense = isPersonal;
        status.SubscriptionTier = effectiveTier;
        status.IsActive = status.SubscriptionTier is "professional" or "enterprise"
            && string.Equals(status.Status, "active", StringComparison.OrdinalIgnoreCase)
            && !status.IsExpired;
    }

    private static string FormatTierLabel(string tier) => tier switch
    {
        "enterprise" => "Enterprise",
        "professional" => "Professional",
        _ => "Community"
    };
}
