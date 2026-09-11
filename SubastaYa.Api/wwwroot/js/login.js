const loginForm = document.getElementById("loginForm");
const loginButton = document.getElementById("loginButton");
const loginError = document.getElementById("loginError");

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    loginError.textContent = "";
    loginButton.disabled = true;

    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    try {
        const response = await fetch("/api/v1/auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email,
                password
            })
        });

        const data = await response.json();

        if (!response.ok) {
            loginError.textContent =
                data.message || "[CODE-ERROR] - Error al iniciar sesión.";
            return;
        }

        localStorage.setItem(
            "subastaYaUser",
            JSON.stringify(data)
        );

        window.location.href = "/";
    } catch (error) {
        console.error(
            "[CODE-ERROR] - Error de conexión:",
            error
        );

        loginError.textContent =
            "[CODE-ERROR] - No se pudo conectar con el servidor.";
    } finally {
        loginButton.disabled = false;
    }
});