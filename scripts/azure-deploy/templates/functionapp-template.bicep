@minLength(3)
@maxLength(22)
param functionAppName string
param location string
param tagValues object
param functionAppSettings array
param storageAccountForFilesConnectionString string
param storageAccountForFilesContainerName string
param cosmosDbAccountConnectionString string
param appInsightsInstrumentationKey string
param appInsightsConnectionString string

var serverfarmsAppServicePlanName = '${functionAppName}asp'
var storageAccountForFunctionAppName = '${functionAppName}sa'
var siteFunctionAppName = functionAppName
var functionTagValues = union(tagValues, {
  function: functionAppName
})

var siteFinctionTagValues = functionTagValues

resource serverfarmsAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: serverfarmsAppServicePlanName
  location: location
  tags: functionTagValues
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
    capacity: 0
  }
  kind: 'functionapp'
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
}

resource siteFunctionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: siteFunctionAppName
  location: location
  tags: siteFinctionTagValues
  kind: 'functionapp'
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${siteFunctionAppName}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${siteFunctionAppName}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: resourceId('Microsoft.Web/serverfarms', serverfarmsAppServicePlanName)
    reserved: false
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 200
      minimumElasticInstanceCount: 0
      appSettings: union(functionAppSettings, [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountForFunctionAppName};AccountKey=${storageAccountForFunctionApp.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountForFunctionAppName};AccountKey=${storageAccountForFunctionApp.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: siteFunctionAppName
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: '1'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'Azure:CosmosDbConnectionString'
          value: cosmosDbAccountConnectionString
        }
        {
          name: 'Azure:StorageBlobConnectionString'
          value: storageAccountForFilesConnectionString
        }
        {
          name: 'Azure:StorageBlobContainerName'
          value: storageAccountForFilesContainerName
        }
      ])
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    containerSize: 1536
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
  dependsOn: [
    serverfarmsAppServicePlan
  ]
}

resource siteFunctionAppFtp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-01-01' = {
  parent: siteFunctionApp
  name: 'ftp'
  properties: {
    allow: true
  }
}

resource siteFunctionAppScm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-01-01' = {
  parent: siteFunctionApp
  name: 'scm'
  properties: {
    allow: true
  }
}

resource siteFunctionAppWeb 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: siteFunctionApp
  name: 'web'
  properties: {
    numberOfWorkers: 1
    defaultDocuments: [
      'Default.htm'
      'Default.html'
      'Default.asp'
      'index.htm'
      'index.html'
      'iisstart.htm'
      'default.aspx'
      'index.php'
    ]
    netFrameworkVersion: 'v6.0'
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    remoteDebuggingVersion: 'VS2019'
    httpLoggingEnabled: false
    acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    detailedErrorLoggingEnabled: false
    publishingUsername: '$${functionAppName}'
    scmType: 'None'
    use32BitWorkerProcess: false
    webSocketsEnabled: false
    alwaysOn: false
    managedPipelineMode: 'Integrated'
    virtualApplications: [
      {
        virtualPath: '/'
        physicalPath: 'site\\wwwroot'
        preloadEnabled: false
      }
    ]
    loadBalancing: 'LeastRequests'
    experiments: {
      rampUpRules: []
    }
    autoHealEnabled: false
    vnetRouteAllEnabled: false
    vnetPrivatePortsCount: 0
    publicNetworkAccess: 'Enabled'
    cors: {
      allowedOrigins: [
        'https://portal.azure.com'
      ]
      supportCredentials: false
    }
    localMySqlEnabled: false
    ipSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: false
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.2'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    functionAppScaleLimit: 200
    functionsRuntimeScaleMonitoringEnabled: false
    minimumElasticInstanceCount: 0
    azureStorageAccounts: {}
  }
}

resource siteFunctionAppAzurewebsitesHostBinding 'Microsoft.Web/sites/hostNameBindings@2023-01-01' = {
  parent: siteFunctionApp
  name: '${siteFunctionAppName}.azurewebsites.net'
  properties: {
    siteName: siteFunctionAppName
    hostNameType: 'Verified'
  }
}

// -------------------------------------------------------
// Storage Account Definition
// -------------------------------------------------------

resource storageAccountForFunctionApp 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountForFunctionAppName
  location: location
  tags: functionTagValues
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
  properties: {
    defaultToOAuthAuthentication: true
    allowCrossTenantReplication: false
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}

resource storageAccountForFunctionAppDefaultBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccountForFunctionApp
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: false
    }
  }
}

resource storageAccountForFunctionAppDefaultFileService 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccountForFunctionApp
  name: 'default'
  properties: {
    protocolSettings: {
      smb: {}
    }
    cors: {
      corsRules: []
    }
    shareDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource storageAccountForFunctionAppDefaultQueueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storageAccountForFunctionApp
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource storageAccountForFunctionAppDefaultTableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccountForFunctionApp
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource storageAccountForFunctionAppAzureWebjobsHostsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccountForFunctionAppDefaultBlobService
  name: 'azure-webjobs-hosts'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

resource storageAccountForFunctionAppAzureWebjobsSecretsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccountForFunctionAppDefaultBlobService
  name: 'azure-webjobs-secrets'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
}

resource storageAccountForFunctionAppFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  parent: storageAccountForFunctionAppDefaultFileService
  name:  siteFunctionAppName //'followee-fnb88b' // TODO: check
  properties: {
    shareQuota: 5120
    enabledProtocols: 'SMB'
  }
}
