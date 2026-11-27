@description('Region of the workspace and Application Insights. The App Service is in Australia Central.')
param location string = 'australiacentral'

@description('Log Analytics workspace name. Unique inside the resource group.')
param workspaceName string = 'log-erp-demo-dev'

@description('Application Insights name. Globally unique.')
param appInsightsName string = 'appi-erp-demo-dev'

@description('Web App name. Must match appservice.bicep.')
param appName string = 'app-erp-demo-dev'

@description('Flexible Server name. Must match postgres.bicep.')
param postgresServerName string = 'psql-erp-demo-dev'

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
  }
}

resource app 'Microsoft.Web/sites@2024-04-01' existing = {
  name: appName
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' existing = {
  name: postgresServerName
}

resource appDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diag-app-erp-demo-dev'
  scope: app
  properties: {
    workspaceId: workspace.id
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

resource postgresDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diag-psql-erp-demo-dev'
  scope: postgres
  properties: {
    workspaceId: workspace.id
    logs: [
      {
        category: 'PostgreSQLLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

output workspaceId string = workspace.id
output appInsightsName string = insights.name
