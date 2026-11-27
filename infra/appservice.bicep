@description('Azure region.')
param location string = 'australiacentral'

@description('App Service plan name. Unique inside the resource group.')
param planName string = 'asp-erp-demo-dev'

@description('Web App name. Globally unique. The hostname is https://<name>.azurewebsites.net.')
param appName string = 'app-erp-demo-dev'

@description('Static Web App name. Used only to build Frontend__Url. Must match staticwebapp.bicep.')
param staticSiteName string = 'stapp-erp-demo-dev'

@description('Flexible Server name. Must match postgres.bicep.')
param postgresServerName string = 'psql-erp-demo-dev'

@description('Administrator login. Must match postgres.bicep.')
param administratorLogin string = 'erpadmin'

@secure()
@description('Same administrator password passed to postgres.bicep. Used only to build the connection string.')
param postgresAdminPassword string

@description('Container Registry name. Must match acr.bicep. Deploy acr.bicep first.')
param acrName string = 'acrerpdemodev'

@description('Tag of the image erp-api in the registry.')
param imageTag string = 'v1'

@description('Application Insights name. Must match observability.bicep. Deploy that file first.')
param appInsightsName string = 'appi-erp-demo-dev'

var frontendUrl = 'https://${staticSiteName}.azurestaticapps.net'
var connectionString = 'Host=${postgresServerName}.postgres.database.azure.com;Port=5432;Database=erp_db;Username=${administratorLogin};Password=${postgresAdminPassword};SSL Mode=Require'

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource insights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

var acrCredentials = acr.listCredentials()

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acr.properties.loginServer}/erp-api:${imageTag}'
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: connectionString
        }
        {
          name: 'Frontend__Url'
          value: frontendUrl
        }
        {
          name: 'Auth__Username'
          value: 'admin'
        }
        {
          name: 'Auth__Token'
          value: 'dev-token'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acr.properties.loginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acrCredentials.username
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acrCredentials.passwords[0].value
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: insights.properties.ConnectionString
        }
      ]
    }
  }
}

output appUrl string = 'https://${appName}.azurewebsites.net'
output corsOrigin string = frontendUrl
