function logout() {
    localStorage.removeItem("subastaYaUser");

    window.location.href = "/login.html";
}

function updateNavigation() {
    const storedUser =
        localStorage.getItem("subastaYaUser");

    const guestNavigation =
        document.getElementById("guest-navigation");

    const authenticatedNavigation =
        document.getElementById("authenticated-navigation");

    // Algunas páginas pueden no tener estos elementos
    if (!guestNavigation || !authenticatedNavigation) {
        return;
    }

    if (storedUser) {
        // Usuario autenticado
        guestNavigation.classList.add("hidden");
        authenticatedNavigation.classList.remove("hidden");
    } else {
        // Usuario sin autenticar
        guestNavigation.classList.remove("hidden");
        authenticatedNavigation.classList.add("hidden");
    }
}

document.addEventListener(
    "DOMContentLoaded",
    updateNavigation
);