# Step 3 — Frontend

The site is static HTML, one stylesheet, and a few scripts. There is no React, Angular, or Vue. The browser never sees a database password.

## Pages

- `ERP.Frontend/index.html` links to the other pages.
- `ERP.Frontend/login.html` sends the username and password to `POST /api/auth/login` and stores the returned token in `sessionStorage`.
- `ERP.Frontend/pages/customers.html` lists customers and creates one.
- `ERP.Frontend/pages/catalog.html` lists products, creates a category, and creates a product with a starting quantity.
- `ERP.Frontend/pages/sales.html` sells one product to one customer.

`css/styles.css` is the only stylesheet. Scripts:

- `js/config.js` sets `API_URL`. Locally it is `http://localhost:5080`. Before the Azure upload, change it to the Web App URL from Terraform, for example `https://app-erp-dev-change01.azurewebsites.net`. No trailing slash is required.
- `js/api.js` calls `fetch` and adds `Authorization: Bearer <token>` when a token is stored.
- `js/auth.js` reads and clears the token. Pages under `pages/` send you back to login when the token is missing.
- `js/customers.js`, `js/catalog.js`, and `js/sales.js` fill the tables and forms.

A 401 from any route except login clears the token and returns to the login page. Other errors show the API `error` text on the page.

## Run it locally

Start the API from step 1 so it listens on port 5080. Then, from the repository root, serve the HTML:

```powershell
npx --yes serve "ERP.Frontend" -l 5500
```

Open `http://localhost:5500`. Use that host, not `127.0.0.1` and not a `file://` address. The API allows only the origin in `Frontend:Url`, which is `http://localhost:5500`.

Log in as `admin` / `dev-token`. Create a customer. Create a category, then a product with a stock quantity greater than zero. Open Sales and sell one unit. The catalog stock drops by that unit.

`npx serve` only uploads nothing. It reads the files in `ERP.Frontend` and serves them on your machine. Closing the terminal stops the site. The files stay where they are.
