requireToken();

function fillSelect(select, rows, label) {
  select.innerHTML = "";
  const empty = document.createElement("option");
  empty.value = "";
  empty.textContent = "Select";
  select.appendChild(empty);

  for (const row of rows) {
    const option = document.createElement("option");
    option.value = row.id;
    option.textContent = label(row);
    select.appendChild(option);
  }
}

document.addEventListener("DOMContentLoaded", async () => {
  const message = document.getElementById("message");

  try {
    const customers = await api("/api/customers");
    const products = await api("/api/catalog");
    fillSelect(document.getElementById("customer-id"), customers, (row) => row.name);
    fillSelect(
      document.getElementById("product-id"),
      products.filter((row) => row.isActive),
      (row) => `${row.name} (${row.stockQuantity} in stock)`
    );
  } catch (error) {
    message.textContent = error.message;
    message.className = "error";
  }

  document.getElementById("sale-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    message.textContent = "";
    message.className = "";

    try {
      const result = await api("/api/sales", {
        method: "POST",
        body: JSON.stringify({
          customerId: document.getElementById("customer-id").value,
          productId: document.getElementById("product-id").value,
          quantity: Number(document.getElementById("quantity").value)
        })
      });
      message.textContent = `Sale saved. Total ${result.totalAmount}. Remaining stock ${result.remainingStock}.`;
      message.className = "ok";
      const products = await api("/api/catalog");
      fillSelect(
        document.getElementById("product-id"),
        products.filter((row) => row.isActive),
        (row) => `${row.name} (${row.stockQuantity} in stock)`
      );
    } catch (error) {
      message.textContent = error.message;
      message.className = "error";
    }
  });
});
