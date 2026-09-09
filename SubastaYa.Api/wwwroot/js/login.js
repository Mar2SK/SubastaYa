// ==========================================
// INICIALIZAR LOGIN
// ==========================================

function initializeLogin() {

    const loginForm =
        document.getElementById("loginForm");

    const emailInput =
        document.getElementById("email");

    const passwordInput =
        document.getElementById("password");

    const togglePasswordButton =
        document.getElementById("togglePassword");

    const loginButton =
        document.getElementById("loginButton");

    const loginMessage =
        document.getElementById("loginMessage");

    const emailError =
        document.getElementById("emailError");

    const passwordError =
        document.getElementById("passwordError");


    // Si el login todavía no existe,
    // no hacemos nada.
    if (
        !loginForm ||
        !emailInput ||
        !passwordInput
    ) {

        console.error(
            "No se encontraron los elementos del login."
        );

        return;
    }


    // ======================================
    // MOSTRAR / OCULTAR PASSWORD
    // ======================================

    if (togglePasswordButton) {

        togglePasswordButton.addEventListener(
            "click",
            () => {

                if (
                    passwordInput.type === "password"
                ) {

                    passwordInput.type = "text";

                    togglePasswordButton.textContent =
                        "🙈";

                }
                else {

                    passwordInput.type = "password";

                    togglePasswordButton.textContent =
                        "👁";

                }

            }
        );

    }


    // ======================================
    // VALIDACIÓN EMAIL
    // ======================================

    function isValidEmail(email) {

        const regex =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        return regex.test(email);

    }


    // ======================================
    // VALIDAR FORMULARIO
    // ======================================

    function validateForm() {

        let valid = true;

        emailError.textContent = "";
        passwordError.textContent = "";


        const email =
            emailInput.value.trim();

        const password =
            passwordInput.value;


        // EMAIL

        if (email === "") {

            emailError.textContent =
                "Ingresá tu correo electrónico.";

            valid = false;

        }
        else if (!isValidEmail(email)) {

            emailError.textContent =
                "Ingresá un correo válido.";

            valid = false;

        }


        // PASSWORD

        if (password === "") {

            passwordError.textContent =
                "Ingresá tu contraseña.";

            valid = false;

        }


        return valid;

    }


    // ======================================
    // LIMPIAR ERRORES
    // ======================================

    emailInput.addEventListener(
        "input",
        () => {

            emailError.textContent = "";

        }
    );


    passwordInput.addEventListener(
        "input",
        () => {

            passwordError.textContent = "";

        }
    );


    // ======================================
    // SUBMIT
    // ======================================

    loginForm.addEventListener(
        "submit",
        async (event) => {

            event.preventDefault();


            loginMessage.className =
                "login-message";

            loginMessage.textContent = "";


            if (!validateForm()) {

                return;

            }


            const loginData = {

                email:
                    emailInput.value.trim(),

                password:
                    passwordInput.value

            };


            loginButton.disabled = true;

            loginButton.textContent =
                "Ingresando...";


            try {

                const response =
                    await fetch(
                        "/api/auth/login",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body:
                                JSON.stringify(
                                    loginData
                                )
                        }
                    );


                if (!response.ok) {

                    if (response.status === 401) {

                        throw new Error(
                            "Correo o contraseña incorrectos."
                        );

                    }

                    if (response.status === 400) {

                        throw new Error(
                            "Los datos ingresados no son válidos."
                        );

                    }

                    throw new Error(
                        "No fue posible iniciar sesión."
                    );

                }


                const data =
                    await response.json();


                console.log(
                    "Login correcto:",
                    data
                );


                loginMessage.className =
                    "login-message success";

                loginMessage.textContent =
                    "Inicio de sesión exitoso.";


                /*
                Cuando implementemos JWT:

                localStorage.setItem(
                    "token",
                    data.token
                );
                */


                setTimeout(
                    () => {

                        closeLogin();

                    },
                    800
                );

            }
            catch (error) {

                loginMessage.className =
                    "login-message error";

                loginMessage.textContent =
                    error.message;

            }
            finally {

                loginButton.disabled = false;

                loginButton.textContent =
                    "Ingresar";

            }

        }
    );

}


// ==========================================
// CERRAR LOGIN
// ==========================================

function closeLogin() {

    const loginContainer =
        document.getElementById(
            "loginContainer"
        );

    if (loginContainer) {

        loginContainer.innerHTML = "";

    }

    document.body.style.overflow = "";

}