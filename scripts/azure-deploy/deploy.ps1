Write-Output 'Prepare for deployment'

$deployConfig = Get-Content -Raw -Path './deploy.config.json' | ConvertFrom-Json

Write-Output 'Configuration for deployment:'
Write-Output $deployConfig

$projectName = $deployConfig.projectName.ToLower()
$env = $deployConfig.env.ToLower()
$prefix = "$($projectName)$($env)"
$rgName = "$($prefix)rg"
$location = $deployConfig.resourceGroupConfig.location
$allowToUseExistedRg = $deployConfig.resourceGroupConfig.allowToUseExisted
$functionAppSettings = $deployConfig.functionAppConfig.appSettings
$deployName = "$prefix-deploy"
$templateFile = $deployConfig.armTemplateFileName
$emailsOnChallengeRequired = $deployConfig.appInsightsConfig.emailsOnChallengeRequired

# https://stackoverflow.com/questions/72061789/passing-an-array-of-objects-to-a-bicep-template-with-new-azresourcegroupdeployme
# https://stackoverflow.com/questions/3740128/pscustomobject-to-hashtable
$functionAppSettingsInBicepFormat = $functionAppSettings | ConvertTo-Json | ConvertFrom-Json -AsHashTable

if ($projectName -eq 'project01') {
    Write-Warning "Cannot use default Project name. Check configuration."
    return
}

Try {
    $rg = Get-AzResourceGroup -Name $rgName -Location $location -ErrorAction Stop
    Write-Output "Existed ResourceGroup (${rgName}) will be used"
}
Catch {
    if ($allowToUseExistedRg -eq $false) {
        throw
    }
    $rg = New-AzResourceGroup -Name $rgName -Location $location -Force
    Write-Output "New ResourceGroup (${rgName}) was created"
}

Write-Output $rg

New-AzResourceGroupDeployment `
    -Name $deployName `
    -ResourceGroupName $rgName `
    -TemplateFile $templateFile `
    -projectName $projectName `
    -prefix $prefix `
    -location $location `
    -env $env `
    -functionAppSettings $functionAppSettingsInBicepFormat `
    -emailsOnChallengeRequired $emailsOnChallengeRequired
