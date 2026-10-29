<#
.SYNOPSIS
  Bootstraps Azure OIDC federation for GitHub Actions (CD workflow) and injects
  AZURE_CLIENT_ID / AZURE_TENANT_ID / AZURE_SUBSCRIPTION_ID into the GitHub
  environments "dev" and "prod".

.PREREQUISITES
  az CLI (logged in: az login) and gh CLI (logged in: gh auth login, with admin rights on the repo).

.EXAMPLE
  ./bootstrap.ps1
  ./bootstrap.ps1 -DevSubscriptionId <guid> -ProdSubscriptionId <guid>
#>
[CmdletBinding()]
param(
    [string] $AppName = "mounty-python-github",
    [string] $Repo,                       # owner/name; auto-detected from git origin if omitted
    [string] $DevSubscriptionId,          # defaults to current az subscription
    [string] $ProdSubscriptionId,         # defaults to DevSubscriptionId
    [string] $DevEnvironment  = "dev",
    [string] $ProdEnvironment = "prod",
    [string] $ProdBranch      = "main",
    [switch] $SkipGitHub                  # only create Azure side and print values
)

$ErrorActionPreference = "Stop"

function Invoke-Native {
    param([scriptblock] $Command)
    $output = & $Command
    if ($LASTEXITCODE -ne 0) { throw "Command failed: $Command" }
    return $output
}

# --- Resolve inputs -------------------------------------------------------
if (-not $Repo) {
    $remote = (git remote get-url origin).Trim()
    if ($remote -match 'github\.com[:/](?<repo>[^/]+/[^/]+?)(\.git)?$') { $Repo = $Matches.repo }
    else { throw "Cannot detect GitHub repo from '$remote'. Pass -Repo owner/name." }
}

# GitHub may issue OIDC tokens with immutable IDs in the subject claim:
#   repo:<owner>@<ownerId>/<name>@<repoId>:environment:<env>
# Resolve the IDs so the federated credential subjects match what GitHub sends.
$repoInfo = Invoke-Native { gh api "repos/$Repo" } | ConvertFrom-Json
$ownerName, $repoName = $Repo -split '/', 2
$subjectRepo = "$ownerName@$($repoInfo.owner.id)/$repoName@$($repoInfo.id)"

$account  = Invoke-Native { az account show -o json } | ConvertFrom-Json
$tenantId = $account.tenantId
if (-not $DevSubscriptionId)  { $DevSubscriptionId  = $account.id }
if (-not $ProdSubscriptionId) { $ProdSubscriptionId = $DevSubscriptionId }

Write-Host "Repo:          $Repo"
Write-Host "Subject repo:  $subjectRepo"
Write-Host "Tenant:        $tenantId"
Write-Host "Dev sub:       $DevSubscriptionId"
Write-Host "Prod sub:      $ProdSubscriptionId"

# --- App registration + service principal (idempotent) --------------------
$clientId = Invoke-Native { az ad app list --display-name $AppName --query "[0].appId" -o tsv }
if (-not $clientId) {
    Write-Host "Creating app registration '$AppName'..."
    $clientId = Invoke-Native { az ad app create --display-name $AppName --query appId -o tsv }
}
$appObjectId = Invoke-Native { az ad app show --id $clientId --query id -o tsv }

$spId = Invoke-Native { az ad sp list --filter "appId eq '$clientId'" --query "[0].id" -o tsv }
if (-not $spId) {
    Write-Host "Creating service principal..."
    $spId = Invoke-Native { az ad sp create --id $clientId --query id -o tsv }
}

# --- Federated credentials (idempotent) -----------------------------------
# Subjects must match the "environment:" values in .github/workflows/cd.yml
# and the ID-based subject format emitted by GitHub Actions.
$credentials = @(
    @{ name = "github-$DevEnvironment";  subject = "repo:${subjectRepo}:environment:$DevEnvironment" },
    @{ name = "github-$ProdEnvironment"; subject = "repo:${subjectRepo}:environment:$ProdEnvironment" }
)

function Write-CredentialFile {
    param($Credential)
    $file = New-TemporaryFile
    @{
        name      = $Credential.name
        issuer    = "https://token.actions.githubusercontent.com"
        subject   = $Credential.subject
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json -Compress | Set-Content -Path $file -Encoding utf8
    return $file
}

$existing = Invoke-Native { az ad app federated-credential list --id $appObjectId -o json } | ConvertFrom-Json
foreach ($c in $credentials) {
    $current = $existing | Where-Object { $_.name -eq $c.name }

    if ($current -and $current.subject -eq $c.subject) {
        Write-Host "Federated credential '$($c.name)' is up to date."
        continue
    }

    $file = Write-CredentialFile $c
    try {
        if ($current) {
            Write-Host "Updating federated credential '$($c.name)' -> $($c.subject)"
            Invoke-Native {
                az ad app federated-credential update --id $appObjectId `
                    --federated-credential-id $current.id --parameters "@$file"
            } | Out-Null
        }
        else {
            Write-Host "Creating federated credential '$($c.name)' -> $($c.subject)"
            Invoke-Native { az ad app federated-credential create --id $appObjectId --parameters "@$file" } | Out-Null
        }
    }
    finally { Remove-Item $file -ErrorAction SilentlyContinue }
}

# --- Role assignments (aspire deploy creates RGs and role assignments) ----
foreach ($sub in ($DevSubscriptionId, $ProdSubscriptionId | Select-Object -Unique)) {
    $scope = "/subscriptions/$sub"
    foreach ($role in "Contributor", "User Access Administrator") {
        $found = Invoke-Native {
            az role assignment list --assignee $clientId --role $role --scope $scope --query "[].id" -o tsv
        }
        if ($found) { Write-Host "Role '$role' already assigned on $scope."; continue }

        Write-Host "Assigning '$role' on $scope..."
        # --assignee-object-id avoids Entra replication lag failures
        Invoke-Native {
            az role assignment create --assignee-object-id $spId --assignee-principal-type ServicePrincipal `
                --role $role --scope $scope
        } | Out-Null
    }
}

# --- GitHub environments + secrets ----------------------------------------
$targets = @(
    @{ env = $DevEnvironment;  sub = $DevSubscriptionId },
    @{ env = $ProdEnvironment; sub = $ProdSubscriptionId }
)

if ($SkipGitHub) {
    Write-Host "`nAZURE_CLIENT_ID=$clientId`nAZURE_TENANT_ID=$tenantId"
    $targets | ForEach-Object { Write-Host "AZURE_SUBSCRIPTION_ID ($($_.env))=$($_.sub)" }
    return
}

foreach ($t in $targets) {
    Write-Host "Configuring GitHub environment '$($t.env)'..."
    Invoke-Native { gh api --method PUT "repos/$Repo/environments/$($t.env)" } | Out-Null

    $secrets = @{
        AZURE_CLIENT_ID       = $clientId
        AZURE_TENANT_ID       = $tenantId
        AZURE_SUBSCRIPTION_ID = $t.sub
    }
    foreach ($name in $secrets.Keys) {
        Invoke-Native { gh secret set $name --env $t.env --repo $Repo --body $secrets[$name] } | Out-Null
        Write-Host "  set $name"
    }
}

Write-Host "`nDone."
Write-Host "Add required reviewers on '$ProdEnvironment' in GitHub > Settings > Environments."
