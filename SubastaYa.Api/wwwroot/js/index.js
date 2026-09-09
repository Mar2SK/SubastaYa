const searchButton = document.getElementById("btnSearch");

if (searchButton) {
    searchButton.addEventListener("click", () => {
        const searchValue = document.getElementById("search").value;
        console.log("Buscar:", searchValue);
    });
}

const openLoginButton = document.getElementById("openLoginBtn");
const loginContainer = document.getElementById("loginContainer");

if (openLoginButton && loginContainer) {
    openLoginButton.addEventListener(
        "click",
        async () => {
            try {
                const response = await fetch("/login.html");
                if (!response.ok) {
                    throw new Error(
                        "No se pudo cargar login.html"
                    );
                }

                const html = await response.text();
                loginContainer.innerHTML = html;
                document.body.style.overflow = "hidden";
                initializeLogin();
                const overlay = document.getElementById("loginOverlay");
                const closeLoginButton = document.getElementById("closeLoginBtn");
                
                if (closeLoginButton) {
                    closeLoginButton.addEventListener("click",closeLogin);
                }

                if (overlay) {
                    overlay.addEventListener(
                        "click",
                        (event) => {
                            if (
                                event.target === overlay
                            ) {
                                closeLogin();
                            }
                        }
                    );
                }
            }
            catch (error) {
                console.error("Error cargando el login:", error);
            }
        }
    );
}

document.addEventListener(
    "keydown",
    (event) => {
        if (event.key === "Escape") {
            const overlay = document.getElementById("loginOverlay");

            if (overlay) {
                closeLogin();
            }
        }
    }
);