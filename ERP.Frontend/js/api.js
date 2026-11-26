function apiBase() {
  return API_URL.replace(/\/$/, "");
}

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (options.body) {
    headers["Content-Type"] = "application/json";
  }

  const token = getToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${apiBase()}${path}`, { ...options, headers });
  if (response.status === 401 && !path.endsWith("/api/auth/login")) {
    clearToken();
    window.location.href = loginPath();
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    let message = response.statusText;
    try {
      const body = await response.json();
      message = body.error || message;
    } catch {
      // The body was not JSON.
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
