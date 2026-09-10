function initializeSignup() {
    const signupForm = document.getElementById("signupForm");
    if (!signupForm) {
        return;
    }

    const firstNameInput = document.getElementById("firstName");
    const lastNameInput = document.getElementById("lastName");
    const emailInput = document.getElementById("signupEmail");
    const passwordInput = document.getElementById("signupPassword");
    const confirmPasswordInput = document.getElementById("confirmPassword");
    const acceptTerms = document.getElementById("acceptTerms");
    const signupButton = document.getElementById("signupButton");
    const signupMessage = document.getElementById("signupMessage");
    const firstNameError = document.getElementById("firstNameError");
    const lastNameError = document.getElementById("lastNameError");
    const emailError = document.getElementById("signupEmailError");
    const passwordError = document.getElementById("signupPasswordError");
    const confirmPasswordError = document.getElementById("confirmPasswordError");
    const termsError = document.getElementById("termsError");
    const togglePassword = document.getElementById("toggleSignupPassword");
    const toggleConfirmPassword = document.getElementById("toggleConfirmPassword");

    togglePassword.addEventListener("click", () => {
        const visible = passwordInput.type === "text";
        passwordInput.type = visible ? "password" : "text";
        togglePassword.textContent = visible ? "👁" : "🙈";
    });

    toggleConfirmPassword.addEventListener("click", () => {
        const visible = confirmPasswordInput.type === "text";
        confirmPasswordInput.type = visible ? "password" : "text";
        toggleConfirmPassword.textContent = visible ? "👁" : "🙈";
    });

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    function validateSignup() {
        let valid = true;

        firstNameError.textContent = "";
        lastNameError.textContent = "";
        emailError.textContent = "";
        passwordError.textContent = "";
        confirmPasswordError.textContent = "";
        termsError.textContent = "";

        const firstName = firstNameInput.value.trim();
        const lastName = lastNameInput.value.trim();
        const email = emailInput.value.trim();
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        if (firstName === "") {
            firstNameError.textContent = "Ingresá tu nombre.";
            valid = false;
        }

        if (lastName === "") {
            lastNameError.textContent = "Ingresá tu apellido.";
            valid = false;
        }

        if (email === "") {
            emailError.textContent = "Ingresá tu correo electrónico.";
            valid = false;
        }
        else if (!isValidEmail(email)) {
            emailError.textContent = "Ingresá un correo válido.";
            valid = false;
        }

        if (password === "") {
            passwordError.textContent = "Ingresá una contraseña.";
            valid = false;
        }
        else if (password.length < 6) {
            passwordError.textContent = "La contraseña debe tener al menos 6 caracteres.";
            valid = false;
        }

        if (confirmPassword === "") {
            confirmPasswordError.textContent = "Repetí la contraseña.";
            valid = false;
        }
        else if (password !== confirmPassword) {
            confirmPasswordError.textContent = "Las contraseñas no coinciden.";
            valid = false;
        }

        if (!acceptTerms.checked) {
            termsError.textContent = "Debés aceptar los términos y condiciones.";
            valid = false;
        }

        return valid;
    }

    signupForm.addEventListener(
        "submit",
        async (event) => {
            event.preventDefault();

            signupMessage.className = "login-message";
            signupMessage.textContent = "";

            if (!validateSignup()) {
                return;
            }

            const signupData = {
                firstName: firstNameInput.value.trim(),
                lastName: lastNameInput.value.trim(),
                email: emailInput.value.trim(),
                password: passwordInput.value
            };

            signupButton.disabled = true;
            signupButton.textContent = "Creando cuenta...";

            try {
                const response =
                    await fetch(
                        "/api/auth/register",
                        {
                            method: "POST",
                            headers: {"Content-Type":"application/json"},
                            body: JSON.stringify(signupData)
                        }
                    );

                if (!response.ok) {
                    if (response.status === 409) {
                        throw new Error("Ya existe un usuario con ese correo.");

                    }
                    if (response.status === 400) {
                        throw new Error("Los datos ingresados no son válidos.");
                    }
                    throw new Error("No fue posible crear la cuenta.");
                }

                signupMessage.className = "login-message success";
                signupMessage.textContent = "Cuenta creada correctamente.";

                setTimeout(() => {
                    openLoginFromSignup();
                }, 900);
            }
            catch (error) {
                signupMessage.className = "login-message error";
                signupMessage.textContent = error.message;
            }
            finally {
                signupButton.disabled = false;
                signupButton.textContent = "Crear cuenta";
            }
        }
    );

    const goToLoginButton = document.getElementById("goToLoginBtn");
    goToLoginButton.addEventListener("click", openLoginFromSignup);
}

function closeSignup() {
    const container = document.getElementById("loginContainer");
    if (container) {
        container.innerHTML = "";
    }
    document.body.style.overflow = "";
}

async function openLoginFromSignup() {
    const container = document.getElementById("loginContainer");
    try {
        const response = await fetch("/login.html");
        if (!response.ok) {
            throw new Error("No se pudo cargar el login.");
        }
        const html = await response.text();
        container.innerHTML = html;

        initializeLogin();
        configureLoginCloseEvents();
    }
    catch (error) {
        console.error("Error cargando login:", error);
    }
}