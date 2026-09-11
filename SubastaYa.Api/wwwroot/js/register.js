const registerForm = document.getElementById("registerForm");
const registerButton = document.getElementById("registerButton");
const registerMessage = document.getElementById("registerMessage");

registerForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    registerButton.disabled = true;
    registerMessage.textContent = "";

    const email = document.getElementById("email").value.trim();
    const name = document.getElementById("name").value.trim();
    const password = document.getElementById("password").value;

    try {
        const response = await fetch("/api/v1/auth/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email,
                name,
                password
            })
        });

        const data = await response.json();

        if (!response.ok) {
            registerMessage.textContent =
                data.message ||
                "[CODE-ERROR] - No se pudo crear la cuenta.";

            return;
        }

        localStorage.setItem(
            "subastaYaUser",
            JSON.stringify(data)
        );

        window.location.href = "/";
    } catch (error) {
        console.error(
            "[CODE-ERROR] - Error al registrar usuario.",
            error);

        registerMessage.textContent =
            "[CODE-ERROR] - No se pudo conectar con el servidor.";
    } finally {
        registerButton.disabled = false;
    }
});