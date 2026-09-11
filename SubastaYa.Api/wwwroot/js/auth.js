function logout() {
    localStorage.removeItem("subastaYaUser");
    window.location.href = "/login.html";
}