namespace OnSteroidsApiUser.Domain.Models.License;

public class ActivateLicenseRequest
{
    public string Code { get; set; } = string.Empty;
    public string? DeviceLabel { get; set; }
}

public class DeactivateLicenseRequest
{
    public string? Reason { get; set; }
}

public class IssueLicenseRequest
{
    /// <summary>"professional" or "enterprise"</summary>
    public string Tier { get; set; } = "professional";
    /// <summary>"individual", "professional", "enterprise", or "organization"</summary>
    public string? LicenseType { get; set; }
    /// <summary>"personal", "organization", "commercial", or null</summary>
    public string? IndividualType { get; set; }
    /// <summary>"monthly", "annual", "lifetime", or "trial"</summary>
    public string BillingCycle { get; set; } = "annual";
    public int? DurationDays { get; set; }
    public int MaxActivations { get; set; } = 1;
    public int Quantity { get; set; } = 1;
    /// <summary>Exact email or domain ending (e.g. "@company.com" or "company.com")</summary>
    public string? AllowedEmail { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class RevokeLicenseRequest
{
    public Guid? LicenseKeyId { get; set; }
    public string? Code { get; set; }
}

public class LicenseKeyRecord
{
    public Guid Id { get; set; }
    public string? LicenseKey { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public string MaskedCode { get; set; } = string.Empty;
    public string Tier { get; set; } = "professional";
    public string LicenseType { get; set; } = "enterprise";
    public string? IndividualType { get; set; }
    public string BillingCycle { get; set; } = "annual";
    public int? DurationDays { get; set; }
    public int MaxActivations { get; set; } = 1;
    public int ActiveActivations { get; set; } = 0;
    public string? AllowedEmail { get; set; }
    public string? Notes { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LicenseStatusDto
{
    public string SubscriptionTier { get; set; } = "community";
    public string Status { get; set; } = "free";
    public string? LicenseType { get; set; }
    public string? IndividualType { get; set; }
    public bool IsPersonalLicense { get; set; }
    public bool IsEmailOtpVerified { get; set; }
    public string? AccessSource { get; set; }
    public string? LicensedEmailPattern { get; set; }
    public string? BillingCycle { get; set; }
    public string? MaskedCode { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? ActiveActivations { get; set; }
    public int? MaxActivations { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
}

public class IssuedLicenseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string MaskedCode { get; set; } = string.Empty;
    public string Tier { get; set; } = "professional";
    public string LicenseType { get; set; } = "enterprise";
    public string? IndividualType { get; set; }
    public string BillingCycle { get; set; } = "annual";
    public int? DurationDays { get; set; }
    public int MaxActivations { get; set; }
    public string? AllowedEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
