<#
.SYNOPSIS
    Issues OnSteroid Pro or Enterprise registration keys via the admin licensing API.

.EXAMPLE
    .\issue-license.ps1 -ApiBaseUrl "http://localhost:5231" -AdminKey $env:ONSTEROIDS_LICENSE_ADMIN_KEY -Tier enterprise -LicenseType enterprise -IndividualType organization -BillingCycle annual -MaxActivations 25 -AllowedEmail "@company.com" -Notes "Company Domain License"
    .\issue-license.ps1 -ApiBaseUrl "http://localhost:5231" -AdminKey $env:ONSTEROIDS_LICENSE_ADMIN_KEY -Tier professional -LicenseType individual -IndividualType personal -BillingCycle annual -AllowedEmail "jane@gmail.com" -Notes "Personal Pro"
#>
param(
    [string]$ApiBaseUrl = "http://localhost:5231",
    [Parameter(Mandatory = $true)]
    [string]$AdminKey,
    [ValidateSet("professional", "enterprise")]
    [string]$Tier = "professional",
    [ValidateSet("individual", "professional", "enterprise", "organization")]
    [string]$LicenseType = "",
    [ValidateSet("", "personal", "organization", "commercial", "team")]
    [string]$IndividualType = "",
    [ValidateSet("monthly", "annual", "lifetime", "trial")]
    [string]$BillingCycle = "annual",
    [Nullable[int]]$DurationDays = $null,
    [int]$MaxActivations = 0,
    [int]$Quantity = 1,
    [string]$AllowedEmail = "",
    [string]$Notes = ""
)

$ErrorActionPreference = "Stop"

$body = @{
    tier           = $Tier
    billingCycle   = $BillingCycle
    maxActivations = $MaxActivations
    quantity       = $Quantity
}

if (-not [string]::IsNullOrWhiteSpace($LicenseType)) {
    $body.licenseType = $LicenseType.Trim()
}
if (-not [string]::IsNullOrWhiteSpace($IndividualType)) {
    $body.individualType = $IndividualType.Trim()
}
if ($null -ne $DurationDays) {
    $body.durationDays = $DurationDays
}
if (-not [string]::IsNullOrWhiteSpace($AllowedEmail)) {
    $body.allowedEmail = $AllowedEmail.Trim()
}
if (-not [string]::IsNullOrWhiteSpace($Notes)) {
    $body.notes = $Notes.Trim()
}

$endpoint = ($ApiBaseUrl.TrimEnd('/')) + "/api/v1/license/issue"
$headers = @{
    "Content-Type"        = "application/json"
    "X-License-Admin-Key" = $AdminKey
}

$response = Invoke-RestMethod -Uri $endpoint -Method Post -Headers $headers -Body ($body | ConvertTo-Json -Depth 5)
$response.data | Format-Table -Property Code, Tier, LicenseType, IndividualType, BillingCycle, DurationDays, MaxActivations, AllowedEmail, ExpiresAt
