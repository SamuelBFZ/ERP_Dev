@description('Azure region. The resource group rg-erp-demo-dev is in East US.')
param location string = 'centralus'

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

var frontendUrl = 'https://${staticSiteName}.azurestaticapps.net'
var connectionString = 'Host=${postgresServerName}.postgres.database.azure.com;Port=5432;Database=erp_db;Username=${administratorLogin};Password=${postgresAdminPassword};SSL Mode=Require'

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  kind: 'windows'
  sku: {
    name: 'B1'
    tier: 'Basic'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v10.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
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
      ]
    }
  }
}

output appUrl string = 'https://${appName}.azurewebsites.net'
output corsOrigin string = frontendUrl
