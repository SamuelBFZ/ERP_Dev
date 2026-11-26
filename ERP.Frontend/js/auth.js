function getToken() {
  return sessionStorage.getItem("token");
}

function setToken(token) {
  sessionStorage.setItem("token", token);
}

function clearToken() {
  sessionStorage.removeItem("token");
}

function loginPath() {
  const path = window.location.pathname.replace(/\\/g, "/");
  return path.includes("/pages/") ? "../login.html" : "login.html";
}

function requireToken() {
  if (!getToken()) {
    window.location.href = loginPath();
  }
}

document.addEventListener("DOMContentLoaded", () => {
  const logout = document.getElementById("logout");
  if (logout) {
    logout.addEventListener("click", () => {
      clearToken();
      window.location.href = loginPath();
    });
  }

  const form = document.getElementById("login-form");
  if (!form) {
    return;
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const message = document.getElementById("message");
    message.textContent = "";
    message.className = "";

    try {
      const result = await api("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({
          username: document.getElementById("username").value,
          password: document.getElementById("password").value
        })
      });
      setToken(result.token);
      window.location.href = "index.html";
    } catch (error) {
      message.textContent = error.message;
      message.className = "error";
    }
  });
});
