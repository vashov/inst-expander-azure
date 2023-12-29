@minLength(3)
@maxLength(20)
param prefix string
param location string
param tagValues object
param emailsOnChallengeRequired array

var appInsightsName = '${prefix}ai'
var appInsightsWorkspaceName = '${prefix}ws'
var actionGroupSendAlertOnChallengeName = '${prefix}alertchallenge'
var scheduledQueryRuleChallengeRequiredName = '${prefix}qrcr'

var appInsightsTags = union(tagValues, {
  appinsights: appInsightsName
})

output appInsights object = {
  connectionString: appInsights.properties.ConnectionString
  instrumentationKey: appInsights.properties.InstrumentationKey
}

resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: appInsightsWorkspaceName
  location: location
  tags: appInsightsTags
  identity: {
    type: 'SystemAssigned'
    userAssignedIdentities: null
  }
  properties: {
    defaultDataCollectionRuleResourceId: null
    features: {
      clusterResourceId: null
      disableLocalAuth: false
      enableDataExport: false
      enableLogAccessUsingOnlyResourcePermissions: false
      immediatePurgeDataOn30Days: true
    }
    forceCmkForQuery: false
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30
    sku: {
      capacityReservationLevel: null
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

resource appInsights 'microsoft.insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    Request_Source: 'IbizaAIExtensionEnablementBlade'
    RetentionInDays: 90
    WorkspaceResourceId: resourceId('Microsoft.OperationalInsights/workspaces', appInsightsWorkspaceName)
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  dependsOn: [
    appInsightsWorkspace
  ]
}

resource actionGroupSendAlertOnChallenge 'microsoft.insights/actionGroups@2023-01-01' = {
  name: actionGroupSendAlertOnChallengeName
  location: 'Global'
  tags: appInsightsTags
  properties: {
    groupShortName: 'MailOnChllng'
    enabled: true
    emailReceivers: [for email in emailsOnChallengeRequired: {
      name: 'Send email_-EmailAction-'
      emailAddress: email
      useCommonAlertSchema: true
    }]
  }
}

resource scheduledQueryRuleChallengeRequired 'microsoft.insights/scheduledqueryrules@2023-03-15-preview' = {
  name: scheduledQueryRuleChallengeRequiredName
  location: location
  tags: appInsightsTags
  kind: 'LogAlert'
  properties: {
    displayName: 'Scheduled Query Rule Instagram Challenge Required'
    severity: 2
    enabled: true
    evaluationFrequency: 'PT15M'
    scopes: [
      resourceId('microsoft.insights/components', appInsightsName)
    ]
    targetResourceTypes: [
      'microsoft.insights/components'
    ]
    windowSize: 'PT15M'
    criteria: {
      allOf: [
        {
          query: 'traces \n| project \n    timestamp,\n    message,\n    severityLevel\n| where severityLevel == 2 \nand timestamp > ago(15m)\nand (message contains "challenge_required" or message contains "Challenge is required")\n| order by timestamp desc  \n'
          timeAggregation: 'Count'
          dimensions: []
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    autoMitigate: false
    actions: {
      actionGroups: [
        resourceId('microsoft.insights/actionGroups', actionGroupSendAlertOnChallengeName)
      ]
    }
  }
  dependsOn: [
    appInsights
    actionGroupSendAlertOnChallenge
  ]
}
