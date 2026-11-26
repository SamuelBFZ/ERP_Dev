requireToken();

function fillCategories(categories) {
  const select = document.getElementById("category-id");
  const current = select.value;
  select.innerHTML = "";
  const empty = document.createElement("option");
  empty.value = "";
  empty.textContent = "Select";
  select.appendChild(empty);

  for (const category of categories) {
    const option = document.createElement("option");
    option.value = category.id;
    option.textContent = category.name;
    select.appendChild(option);
  }

  select.value = current;
}

async function loadCatalog() {
  const items = await api("/api/catalog");
  const body = document.getElementById("catalog-body");
  body.replaceChildren();

  for (const item of items) {
    const row = document.createElement("tr");
    for (const value of [item.name, item.categoryName, String(item.price), String(item.stockQuantity), item.isActive ? "yes" : "no"]) {
      const cell = document.createElement("td");
      cell.textContent = value;
      row.appendChild(cell);
    }
    body.appendChild(row);
  }
}

document.addEventListener("DOMContentLoaded", () => {
  const message = document.getElementById("message");

  Promise.all([loadCatalog(), api("/api/catalog/categories").then(fillCategories)]).catch((error) => {
    message.textContent = error.message;
    message.className = "error";
  });

  document.getElementById("category-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    message.textContent = "";
    message.className = "";

    try {
      const category = await api("/api/catalog/categories", {
        method: "POST",
        body: JSON.stringify({
          name: document.getElementById("category-name").value,
          description: document.getElementById("category-description").value
        })
      });
      event.target.reset();
      const categories = await api("/api/catalog/categories");
      fillCategories(categories);
      document.getElementById("category-id").value = category.id;
      message.textContent = "Category saved.";
      message.className = "ok";
    } catch (error) {
      message.textContent = error.message;
      message.className = "error";
    }
  });

  document.getElementById("product-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    message.textContent = "";
    message.className = "";

    try {
      await api("/api/catalog/products", {
        method: "POST",
        body: JSON.stringify({
          name: document.getElementById("product-name").value,
          description: document.getElementById("product-description").value,
          price: Number(document.getElementById("price").value),
          categoryId: document.getElementById("category-id").value,
          initialQuantity: Number(document.getElementById("initial-quantity").value)
        })
      });
      event.target.reset();
      message.textContent = "Product saved.";
      message.className = "ok";
      await loadCatalog();
    } catch (error) {
      message.textContent = error.message;
      message.className = "error";
    }
  });
});
