$authResult = Connect-AzureAD

$tenantId = $authResult.TenantId.ToString()
$tenantDomain = $authResult.TenantDomain

$userAccountId = $authResult.Account.Id
$user = Get-AzureADUser -ObjectId $userAccountId

$appDisplayName = "Power BI Service Principal Demo App"

# create app secret
$newGuid = New-Guid
$appSecret = ([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes(($newGuid))))+"="
$startDate = Get-Date	
$passwordCredential = New-Object -TypeName Microsoft.Open.AzureAD.Model.PasswordCredential
$passwordCredential.StartDate = $startDate
$passwordCredential.EndDate = $startDate.AddYears(1)
$passwordCredential.KeyId = $newGuid
$passwordCredential.Value = $appSecret 

# create Azure AD Application
$aadApplication = New-AzureADApplication `
                        -DisplayName $appDisplayName `
                        -PublicClient $false `
                        -AvailableToOtherTenants $false `
                        -Homepage $replyUrl `
                        -PasswordCredentials $passwordCredential

# create applicaiton's service principal 
$appId = $aadApplication.AppId
$appObjectId = $aadApplication.ObjectId

Write-Host "appObjectId: $appObjectId"

# assign current user as owner
Add-AzureADApplicationOwner -ObjectId $aadApplication.ObjectId -RefObjectId $user.ObjectId

$outputFile = "$PSScriptRoot\PowerBI-ServicePrincipal-DemoApp.txt"
Write-Host "Writing info to $outputFile"
Out-File -FilePath $outputFile -InputObject "Application $appId"
Out-File -FilePath $outputFile -Append -InputObject "Application Secret: $appSecret"

Notepad $outputFile