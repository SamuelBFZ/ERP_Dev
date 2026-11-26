@description('Azure region. The resource group rg-erp-demo-dev is in East US.')
param location string = 'centralus'

@description('Static Web App name. Globally unique. The hostname is https://<name>.azurestaticapps.net.')
param staticSiteName string = 'stapp-erp-demo-dev'

resource staticSite 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticSiteName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

output frontendUrl string = 'https://${staticSiteName}.azurestaticapps.net'
