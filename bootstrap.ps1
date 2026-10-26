#Requires -Version 7
<#
.SYNOPSIS
    One-time bootstrap for the MountyPython CD pipeline.
 
.DESCRIPTION
    Creates the service principal GitHub Actions needs to run `aspire deploy`.
    Idempotent - safe to re-run.
 
    All infrastructure (resource group, ACR, ACA environment, PostgreSQL,
    Key Vault, managed identity) is provisioned automatically by `aspire deploy`
    on every CD run - no shared registry or manual Azure setup required.
 
.PARAMETER SubscriptionId
    Target Azure subscription ID.  Prompted interactively if omitted.
 
.PARAMETER SpName
    Display name for the service principal.  Defaults to 'sp-mountypython-cicd'.
 
.EXAMPLE
    ./bootstrap.ps1 -SubscriptionId "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
#>
param(
    [string] $SubscriptionId = "",
    [string] $SpName         = "sp-mountypython-cicd"
)
 
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
 
function Write-Step([string] $msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok([string]   $msg) { Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Warn([string] $msg) { Write-Host "    !!  $msg" -ForegroundColor Yellow }
 
# ---------------------------------------------------------------------------
# 0 - Prerequisites
# ---------------------------------------------------------------------------
Write-Step "Checking prerequisites"
 
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw "Azure CLI not found.  Install with: winget install --id Microsoft.AzureCLI"
}
Write-Ok "Azure CLI found"
 
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Warn "Not logged in - running az login..."
    az login | Out-Null
    $account = az account show | ConvertFrom-Json
}
Write-Ok "Logged in as $($account.user.name)"
 
# ---------------------------------------------------------------------------
# 1 - Select subscription
# ---------------------------------------------------------------------------
Write-Step "Selecting subscription"
 
if (-not $SubscriptionId) {
    $SubscriptionId = Read-Host "  Enter subscription ID"
}
az account set --subscription $SubscriptionId | Out-Null
$sub = az account show | ConvertFrom-Json
Write-Ok "Using: $($sub.name) ($SubscriptionId)"
 
# ---------------------------------------------------------------------------
# 2 - Service principal
# ---------------------------------------------------------------------------
Write-Step "Creating / verifying service principal '$SpName'"
 
$spRaw = az ad sp create-for-rbac `
    --name   $SpName `
    --role   Contributor `
    --scopes "/subscriptions/$SubscriptionId" `
    --output json 2>$null
 
if ($spRaw) {
    $spObj    = $spRaw | ConvertFrom-Json
    $clientId = $spObj.appId
    $secret   = $spObj.password
    $tenant   = $spObj.tenant
    Write-Ok "Service principal created (appId: $clientId)"
} else {
    # SP already exists - reset the credential to obtain a fresh secret.
    Write-Warn "Service principal '$SpName' already exists - resetting credential..."
    $clientId = (az ad sp list --display-name $SpName --query "[0].appId" -o tsv)
    $secret   = (az ad app credential reset --id $clientId --query password -o tsv)
    $tenant   = $sub.tenantId
    Write-Ok "Credential reset (appId: $clientId)"
}
 
# ---------------------------------------------------------------------------
# 3 - Role: User Access Administrator
# ---------------------------------------------------------------------------
Write-Step "Ensuring 'User Access Administrator' role on subscription"
 
# aspire deploy generates Bicep that assigns AcrPull to the ACA managed identity.
# That requires Microsoft.Authorization/roleAssignments/write - UAA provides it
# without granting full Owner.
$scope     = "/subscriptions/$SubscriptionId"
$uaaExists = az role assignment list `
    --assignee $clientId `
    --role     "User Access Administrator" `
    --scope    $scope `
    --query    "[0].id" -o tsv 2>$null
 
if ($uaaExists) {
    Write-Ok "Role already assigned"
} else {
    az role assignment create `
        --assignee $clientId `
        --role     "User Access Administrator" `
        --scope    $scope `
        --output   none | Out-Null
    Write-Ok "Role assigned"
}
 
# ---------------------------------------------------------------------------
# 4 - Set GitHub secrets (optional - requires gh CLI)
# ---------------------------------------------------------------------------
Write-Step "GitHub secrets"
 
if (Get-Command gh -ErrorAction SilentlyContinue) {
    Write-Host "  gh CLI found - setting secrets automatically..." -ForegroundColor Cyan
    $remote = git -C $PSScriptRoot remote get-url origin 2>$null
    if ($remote) {
        $repoName = $remote -replace '.*github\.com[:/]', '' -replace '\.git$', ''
        gh secret set AZURE_CLIENT_ID       --body $clientId        --repo $repoName
        gh secret set AZURE_CLIENT_SECRET   --body $secret          --repo $repoName
        gh secret set AZURE_TENANT_ID       --body $tenant          --repo $repoName
        gh secret set AZURE_SUBSCRIPTION_ID --body $SubscriptionId  --repo $repoName
        Write-Ok "All 4 secrets set on $repoName"
    } else {
        Write-Warn "Could not determine GitHub repo from git remote - set secrets manually"
    }
} else {
    Write-Warn "gh CLI not found - set the following secrets manually in GitHub"
}
 
# Always print values for verification
Write-Host ""
Write-Host "  Settings > Secrets and variables > Actions" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  AZURE_CLIENT_ID       = $clientId"        -ForegroundColor White
Write-Host "  AZURE_CLIENT_SECRET   = $secret"          -ForegroundColor Yellow
Write-Host "  AZURE_TENANT_ID       = $tenant"          -ForegroundColor White
Write-Host "  AZURE_SUBSCRIPTION_ID = $SubscriptionId"  -ForegroundColor White
Write-Host ""
Write-Host "  *** Save AZURE_CLIENT_SECRET now - it cannot be retrieved later ***" -ForegroundColor Red
 
# ---------------------------------------------------------------------------
# 5 - Reminder: GitHub environments
# ---------------------------------------------------------------------------
Write-Host ""
Write-Step "Reminder: create GitHub environments"
Write-Host "  Settings > Environments:"
Write-Host "    dev   - optional required reviewers"
Write-Host "    prod  - required reviewers recommended"
Write-Host ""
Write-Host "Bootstrap complete. Push to the 'dev' branch to trigger the first deploy." -ForegroundColor Green
