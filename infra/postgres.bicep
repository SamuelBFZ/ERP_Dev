@description('Azure region. The resource group rg-erp-demo-dev is in East US.')
param location string = 'centralus'

@description('Flexible Server name. Globally unique, lowercase, 3-63 characters.')
param postgresServerName string = 'psql-erp-demo-dev'

@description('Administrator login. Flexible Server rejects names such as postgres, admin, and root.')
param administratorLogin string = 'erpadmin'

@secure()
@description('Administrator password. Pass it on the CLI. Do not store it in a file.')
param postgresAdminPassword string

@description('Your public IPv4 address, for running the migration later. Leave empty to skip this firewall rule.')
param clientIp string = ''

resource server 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: administratorLogin
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: server
  name: 'erp_db'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: server
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowClient 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (!empty(clientIp)) {
  parent: server
  name: 'AllowClient'
  properties: {
    startIpAddress: clientIp
    endIpAddress: clientIp
  }
}

output postgresFqdn string = '${postgresServerName}.postgres.database.azure.com'
