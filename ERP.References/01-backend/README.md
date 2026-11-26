# Step 1 — Backend

The API is one ASP.NET Core project, `ERP.Backend/ERP.Api`, on .NET 10. Module folders keep customers, catalog, sales, inventory, and login apart. They are folders in the same project, not separate services.

The behavior is small:

- Login checks the username and returns the token stored in configuration.
- The other routes accept that token in the `Authorization` header.
- You can create a customer, create a category, create a product with a starting stock, list the catalog, and sell one product.
- A sale writes the sale, one sale line, a lower stock quantity, and one inventory movement. It refuses the sale when the stock is too low.

## Login

`POST /api/auth/login` reads this JSON:

```json
{ "username": "admin", "password": "dev-token" }
```

The password is compared with `Auth:Token` in configuration. Locally that value is `dev-token` in `appsettings.json`. The username must match `Auth:Username` (`admin`) or an active row in `auth.users`. The response is:

```json
{ "token": "dev-token" }
```

There is no JWT library and no password hash check. The column `auth.users.password_hash` exists because the table is part of the model. The login action does not read it.

Every other `/api/...` route requires:

```http
Authorization: Bearer dev-token
```

`BearerTokenMiddleware` does that comparison. `/`, `/swagger`, and `/api/auth/login` stay open.

## Routes

| Method | Path | What it does |
|---|---|---|
| POST | `/api/auth/login` | Returns the configured token. |
| GET | `/api/customers` | Lists customers. |
| POST | `/api/customers` | Creates a customer. Email must be unique when it is present. |
| GET | `/api/catalog/categories` | Lists categories for the product form. |
| POST | `/api/catalog/categories` | Creates a category. |
| POST | `/api/catalog/products` | Creates a product and one inventory row. `initialQuantity` is the starting stock. |
| GET | `/api/catalog` | Returns each product with its category name and stock. |
| POST | `/api/sales` | Sells one product to one customer. |

`POST /api/sales` body:

```json
{ "customerId": "...", "productId": "...", "quantity": 1 }
```

The sale is stored as `completed`. The line subtotal is price times quantity. Stock goes down by that quantity. The movement type is `out`. One `SaveChanges` writes all of those rows.

Failed calls return `{ "error": "..." }` with status 400 or 401.

## Files

- `Program.cs` registers the database, controllers, Swagger, CORS from `Frontend:Url`, and the token middleware.
- `Data/ErpDbContext.cs` maps every table. The seed user lives here too.
- `Data/ErpDbContextFactory.cs` lets `dotnet ef` build the model.
- `Modules/Authentication` holds the user, the login request, the middleware, and `AuthController`.
- `Modules/Customers` holds the customer and `CustomersController`.
- `Modules/Catalog` holds categories, products, and `CatalogController`.
- `Modules/Inventory` holds the stock row and the movement. There is no inventory controller. Sales updates those rows.
- `Modules/Sales` holds the sale, the line, and `SalesController`.
- `appsettings.json` holds the local token, the username, and the frontend origin `http://localhost:5500`.
- `appsettings.Development.json` holds the local Postgres connection string. It is gitignored because it contains a password. Copy it from `appsettings.Development.json.example`.
- `nuget.config` at the repository root forces restore to use nuget.org.

CORS allows one origin, `Frontend:Url`. Azure replaces that setting later. The app does not redirect HTTP to HTTPS itself. The App Service setting `https_only` does that at the platform.

## Commands

From the repository root:

```powershell
Copy-Item "ERP.Backend\ERP.Api\appsettings.Development.json.example" "ERP.Backend\ERP.Api\appsettings.Development.json"
dotnet build "ERP.Backend\ERP.Api\ERP.Api.csproj"
dotnet run --project "ERP.Backend\ERP.Api\ERP.Api.csproj"
```

`dotnet build` compiles into `ERP.Backend/ERP.Api/bin/`. It does not create database tables and it does not call Azure.

`dotnet run` starts the API at `http://localhost:5080`. Swagger is at `http://localhost:5080/swagger`. The root URL returns `{ "name": "ERP API", "status": "ok" }`.

The database step has to finish before login can see the seeded user and before a sale can be saved. A login with username `admin` and password `dev-token` can still return the token when the configured username matches, even before the seed row exists. Creating a sale needs that row.
