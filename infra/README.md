# Infra — first flow

Three Bicep files create the API, the site, and PostgreSQL in the existing resource group `rg-erp-demo-dev` (East US). They do not create that group, publish the API, upload the HTML, or run `dotnet ef database update`.

Each command uses Incremental mode, which is the default. Do not pass `--mode Complete`. A complete deployment deletes every resource in the group that the file does not contain.

There is no Key Vault. The administrator password is a secure parameter. Pass it on the command line. Do not write it into a file.

## Names

| Parameter | Default | Result |
|---|---|---|
| `appName` | `app-erp-demo-dev` | `https://app-erp-demo-dev.azurewebsites.net` |
| `planName` | `asp-erp-demo-dev` | Windows plan, SKU F1 |
| `staticSiteName` | `stapp-erp-demo-dev` | `https://stapp-erp-demo-dev.azurestaticapps.net` |
| `postgresServerName` | `psql-erp-demo-dev` | `psql-erp-demo-dev.postgres.database.azure.com` |

If Azure says a name is already taken, pass the same new value to every file that uses it. `appservice.bicep` builds the connection string and `Frontend__Url` from `postgresServerName` and `staticSiteName`.

## Postgres

`postgres.bicep` creates a Burstable `Standard_B1ms` server, PostgreSQL 16, 32 GB, 7-day backup, no high availability, and no geo-redundant backup. Public network access stays on because the F1 plan has no virtual network.

The firewall has two rules:

- `AllowAzureServices` is `0.0.0.0`–`0.0.0.0`. On Flexible Server that allows Azure services, including the API.
- `AllowClient` is created only when `clientIp` is a public IPv4 address. Use it later, when the migration runs from this machine.

The administrator login is `erpadmin`. The empty database is `erp_db`.

The password must be 8–128 characters and mix three of these groups: uppercase, lowercase, digits, symbols. Avoid `'`, `;`, and spaces so the connection string stays valid.

## App Service

`appservice.bicep` creates a Windows plan. F1 does not exist on Linux. The site uses HTTPS and the .NET 10 stack (`netFrameworkVersion` `v10.0`).

App settings match what `ERP.Backend/ERP.Api/Program.cs` reads:

- `ConnectionStrings__DefaultConnection` points at `erp_db` with `SSL Mode=Require`.
- `Frontend__Url` is `https://stapp-erp-demo-dev.azurestaticapps.net`, with no trailing slash.
- `Auth__Username` is `admin` and `Auth__Token` is `dev-token`.

## Static Web App

`staticwebapp.bicep` creates a Free Static Web App with no GitHub repository. `ERP.Frontend/js/config.js` is unchanged and still points at `http://localhost:5080`.

## Deploy

Run these from the repository root. Use the same password for the first two commands.

```powershell
az deployment group create --resource-group rg-erp-demo-dev --template-file infra/postgres.bicep --parameters postgresAdminPassword="<password>" clientIp="<your-public-ip>"

az deployment group create --resource-group rg-erp-demo-dev --template-file infra/appservice.bicep --parameters postgresAdminPassword="<password>"

az deployment group create --resource-group rg-erp-demo-dev --template-file infra/staticwebapp.bicep
```

Leave `clientIp` out to skip the client firewall rule.

Deploying `postgres.bicep` again creates the server and an empty `erp_db` when the server is gone. The `admin` user is inserted by the EF migration, not by Bicep.

## Delete the database

F1 and the Free Static Web App can stay up. The Flexible Server keeps billing until the server itself is deleted. Deleting only the `erp_db` database leaves the server and its disk in place.

```powershell
./infra/delete-postgres.ps1
```

That script runs `az postgres flexible-server delete` for `psql-erp-demo-dev`. It does not delete the resource group, the plan, or the Static Web App.
