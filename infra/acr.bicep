@description('Azure region.')
param location string = 'centralus'

@description('Container Registry name. Globally unique, 5-50 alphanumeric characters.')
param acrName string = 'acrerpdemodev'

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output loginServer string = acr.properties.loginServer
