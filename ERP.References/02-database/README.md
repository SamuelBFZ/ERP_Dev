# Step 2 — Database

There is one PostgreSQL database, `erp_db`. Entity Framework Core creates the schemas and tables. Terraform, in step 4, only creates the server and an empty database.

## Tables

The relationships are: a user creates sales, a customer makes sales, a sale contains sale lines, a product is included in sale lines, a category groups products, and a product has one stock row and many movements.

**auth.users** — `id` uuid primary key, `username` varchar(100) unique, `password_hash` varchar(255), `is_active` boolean, `created_at` timestamp.

**customers.customers** — `id`, `name` varchar(150), `email` varchar(150) unique, `phone` varchar(30), `address` varchar(255), `created_at`. Email, phone, and address may be null.

**catalog.categories** — `id`, `name` varchar(100), `description` varchar(255).

**catalog.products** — `id`, `category_id` foreign key, `name` varchar(150), `description`, `price` numeric(12,2), `is_active`.

**inventory.inventory** — `id`, `product_id` foreign key and unique, `quantity`, `updated_at`. One row per product.

**inventory.inventory_movements** — `id`, `product_id` foreign key, `movement_type` varchar(20), `quantity`, `created_at`.

**sales.sales** — `id`, `customer_id` foreign key, `user_id` foreign key, `sale_date`, `total_amount` numeric(12,2), `status` varchar(30).

**sales.sale_details** — `id`, `sale_id` foreign key, `product_id` foreign key, `quantity`, `unit_price` numeric(12,2), `subtotal` numeric(12,2).

Deleting a sale deletes its lines. The other foreign keys do not cascade.

## Seed

The migration inserts one user:

- id `11111111-1111-1111-1111-111111111111`
- username `admin`
- password_hash `dev-token`
- is_active `true`

That hash column is stored text. The API password check uses `Auth:Token`, not this column. Products, customers, and sales are created through the API.

## Local Postgres

Install Docker, or use a Postgres 16 server you already have. With Docker:

```powershell
docker run --name erp-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=erp_db -p 5432:5432 -d postgres:16
```

That command starts a container and creates the empty database `erp_db` on port 5432. It does not create the ERP schemas. Those come from EF.

Copy the connection string file if it is not already present:

```powershell
Copy-Item "ERP.Backend\ERP.Api\appsettings.Development.json.example" "ERP.Backend\ERP.Api\appsettings.Development.json"
```

The example points at `Host=localhost;Port=5432;Database=erp_db;Username=postgres;Password=postgres`.

## EF commands

The migration is already in the repository:

- `ERP.Backend/ERP.Api/Migrations/20261109225055_InitialErp.cs`
- `ERP.Backend/ERP.Api/Migrations/20261109225055_InitialErp.Designer.cs`
- `ERP.Backend/ERP.Api/Migrations/ErpDbContextModelSnapshot.cs`

`dotnet ef migrations add InitialErp` is the command that created those files. It read `ErpDbContext` and wrote C# only. It did not open Postgres. Do not run that same name again. A later model change uses a new name, for example `dotnet ef migrations add AddPhoneNotes --project "ERP.Backend\ERP.Api\ERP.Api.csproj"`, which writes another migration file and still does not update a server.

The EF tool is declared in `dotnet-tools.json` at the repository root. Restore it, then apply the migration:

```powershell
dotnet tool restore
dotnet ef database update --project "ERP.Backend\ERP.Api\ERP.Api.csproj"
```

`dotnet tool restore` downloads `dotnet-ef` 10.0.0 into the local tool cache. It does not touch a database.

`dotnet ef database update` opens the connection string from `appsettings.Development.json`, or from the environment variable `ConnectionStrings__DefaultConnection` when that is set. It creates the five schemas, the eight tables, the foreign keys, and the admin user in whichever server that string points at.

Run `database update` again and it does nothing if the migration is already applied. The history is stored in `public."__EFMigrationsHistory"`.

## What a sale writes

After a product exists with stock, `POST /api/sales` changes three places in the same save:

- a new row in `sales.sales`
- a new row in `sales.sale_details`
- `inventory.inventory.quantity` decreases
- a new row in `inventory.inventory_movements` with `movement_type` `out`

Creating a product inserts `catalog.products` and `inventory.inventory`. It does not insert a movement.

## Azure later

Step 4 creates the Flexible Server and the empty database `erp_db`. You run the same `dotnet ef database update` against that server after Terraform opens the firewall for your IP. The migration files stay the same. Only the connection string changes.
