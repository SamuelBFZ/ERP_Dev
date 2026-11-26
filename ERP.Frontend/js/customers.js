requireToken();

async function loadCustomers() {
  const customers = await api("/api/customers");
  const body = document.getElementById("customers-body");
  body.innerHTML = "";

  for (const customer of customers) {
    const row = document.createElement("tr");
    for (const value of [customer.name, customer.email || "", customer.phone || "", customer.address || ""]) {
      const cell = document.createElement("td");
      cell.textContent = value;
      row.appendChild(cell);
    }
    body.appendChild(row);
  }
}

document.addEventListener("DOMContentLoaded", () => {
  const message = document.getElementById("message");
  loadCustomers().catch((error) => {
    message.textContent = error.message;
    message.className = "error";
  });

  document.getElementById("customer-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    message.textContent = "";
    message.className = "";

    try {
      await api("/api/customers", {
        method: "POST",
        body: JSON.stringify({
          name: document.getElementById("name").value,
          email: document.getElementById("email").value,
          phone: document.getElementById("phone").value,
          address: document.getElementById("address").value
        })
      });
      event.target.reset();
      message.textContent = "Customer saved.";
      message.className = "ok";
      await loadCustomers();
    } catch (error) {
      message.textContent = error.message;
      message.className = "error";
    }
  });
});
