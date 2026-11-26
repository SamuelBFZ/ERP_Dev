param(
    [string] $ResourceGroupName = 'rg-erp-demo-dev',
    [string] $ServerName = 'psql-erp-demo-dev'
)

# Deletes the Flexible Server. The App Service plan and the Static Web App stay in the resource group.
az postgres flexible-server delete --resource-group $ResourceGroupName --name $ServerName --yes
