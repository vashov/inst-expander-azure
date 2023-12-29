@minLength(3)
@maxLength(10)
param projectName string
@allowed(['dev', 'prod'])
param env string
param location string = resourceGroup().location
@minLength(3)
@maxLength(20)
param prefix string
param functionAppSettings array

var storageAccountForFilesName = '${prefix}sa'
var cosmodDbAccountName = '${prefix}ca'
var functionAppName = '${prefix}fn'

var tagValues = {
  project: projectName
  env: env
}

module storageAccountModule 'templates/storage-template.bicep' = {
  name: '${prefix}-storageModule-deploy'
  params: {
    storageAccountName: storageAccountForFilesName
    location: location
    tagValues: tagValues
  }
}

module cosmosModule 'templates/cosmosdb-template.bicep' = {
  name: '${prefix}-cosmosModule-deploy'
  params: {
    cosmosDbAccountName: cosmodDbAccountName
    location: location
    tagValues: tagValues
  }
}

module functionappModule 'templates/functionapp-template.bicep' = {
  name: '${prefix}-functionappModule-deploy'
  params: {
    functionAppName: functionAppName
    location: location
    tagValues: tagValues
    functionAppSettings: functionAppSettings
    storageAccountForFilesConnectionString: storageAccountModule.outputs.storageAccountValues.connectionString
    storageAccountForFilesContainerName: storageAccountModule.outputs.storageAccountValues.containerName
    cosmosDbAccountConnectionString: cosmosModule.outputs.cosmosDbConnectionString
  }
  dependsOn: [

  ]
}
