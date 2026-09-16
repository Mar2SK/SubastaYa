const storedUser = localStorage.getItem("subastaYaUser");

if (!storedUser) {
    window.location.href = "login.html";
}

let currentUser = null;

try {
    currentUser = JSON.parse(storedUser);
} catch (error) {
    console.error(
        "[CODE-ERROR] - no se pudo recuperar la sesión del usuario.",
        error);

    localStorage.removeItem("subastaYaUser");
    window.location.href = "login.html";
}

const currentUserId = currentUser?.userId;

function formatCurrency(amount) {
    return new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
        maximumFractionDigits: 0
    }).format(amount);
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#039;");
}

function renderBids(items) {
    const panel = document.getElementById("bids-panel");

    if (items.length === 0) {
        panel.innerHTML =
            "<p class=\"message\">Todavía no participaste en subastas.</p>";
        return;
    }

    panel.innerHTML = "";

    for (const idx_tk of items) {
        const badge = idx_tk.hasWon
            ? "Ganada"
            : idx_tk.isLeading
                ? "Liderando"
                : "Superado";

        panel.insertAdjacentHTML(
            "beforeend",
            `
            <article class="activity-item">
                <img
                    src="${escapeHtml(idx_tk.imageUrl)}"
                    alt="${escapeHtml(idx_tk.title)}">

                <div>
                    <h3>${escapeHtml(idx_tk.title)}</h3>
                    <p>
                        Mi mejor puja:
                        ${formatCurrency(idx_tk.myHighestBid)}
                    </p>
                    <p>
                        Oferta actual:
                        ${formatCurrency(idx_tk.currentHighestBid)}
                    </p>
                </div>

                <span class="status-badge">
                    ${badge}
                </span>
            </article>
            `);
    }
}

function renderAuctions(items) {
    const panel = document.getElementById("auctions-panel");

    if (items.length === 0) {
        panel.innerHTML =
            "<p class=\"message\">Todavía no publicaste subastas.</p>";
        return;
    }

    panel.innerHTML = "";

    for (const idx_tk of items) {
        panel.insertAdjacentHTML(
            "beforeend",
            `
            <article class="activity-item">
                <img
                    src="${escapeHtml(idx_tk.imageUrl)}"
                    alt="${escapeHtml(idx_tk.title)}">

                <div>
                    <h3>${escapeHtml(idx_tk.title)}</h3>
                    <p>${idx_tk.bidCount} puja(s)</p>
                    <p>
                        Mayor oferta:
                        ${
                            idx_tk.highestBid
                                ? formatCurrency(idx_tk.highestBid)
                                : "Sin pujas"
                        }
                    </p>
                </div>

                <span class="status-badge">
                    ${escapeHtml(idx_tk.status)}
                </span>
            </article>
            `);
    }
}

function renderTransactions(items) {
    const panel = document.getElementById("transactions-panel");

    if (items.length === 0) {
        panel.innerHTML =
            "<p class=\"message\">Todavía no tenés movimientos.</p>";
        return;
    }

    panel.innerHTML = "";

    for (const idx_tk of items) {
        panel.insertAdjacentHTML(
            "beforeend",
            `
            <article class="transaction-item">
                <p>
                    <strong>${escapeHtml(idx_tk.type)}</strong>
                </p>
                <p>
                    ${formatCurrency(idx_tk.amount)}
                </p>
                <p>
                    ${escapeHtml(idx_tk.createdAtUtc)}
                </p>
            </article>
            `);
    }
}

async function loadBids() {
    const response = await fetch(
        `/api/v1/users/${currentUserId}/bids`);

    if (!response.ok) {
        throw new Error("No se pudieron cargar las pujas.");
    }

    const items = await response.json();
    renderBids(items);
}

async function loadAuctions() {
    const response = await fetch(
        `/api/v1/users/${currentUserId}/auctions`);

    if (!response.ok) {
        throw new Error("No se pudieron cargar las publicaciones.");
    }

    const items = await response.json();
    renderAuctions(items);
}

async function loadTransactions() {
    const response = await fetch(
        `/api/v1/wallets/${currentUserId}/transactions`);

    if (!response.ok) {
        throw new Error("No se pudieron cargar los movimientos.");
    }

    const items = await response.json();
    renderTransactions(items);
}

function configureTabs() {
    const buttons = document.querySelectorAll(".tab-button");

    for (const idx_tk of buttons) {
        idx_tk.addEventListener("click", () => {
            const targetId = idx_tk.dataset.target;

            const panels = document.querySelectorAll(".tab-panel");

            for (const idx_tk of panels) {
                idx_tk.classList.add("hidden");
            }

            const activePanel = document.getElementById(targetId);

            if (activePanel) {
                activePanel.classList.remove("hidden");
            }

            const allButtons = document.querySelectorAll(".tab-button");

            for (const idx_tk of allButtons) {
                idx_tk.classList.remove("active");
            }

            idx_tk.classList.add("active");
        });
    }
}

function configureLogout() {
    const logoutButton = document.getElementById("logout-button");

    if (!logoutButton) {
        return;
    }

    logoutButton.addEventListener("click", () => {
        localStorage.removeItem("subastaYaUser");
        window.location.href = "/";
    });
}

async function initializeActivities() {
    try {
        configureTabs();
        configureLogout();

        await Promise.all([
            loadBids(),
            loadAuctions(),
            loadTransactions()
        ]);
    } catch (error) {
        console.error(
            "[CODE-ERROR] - no se pudieron cargar las actividades.",
            error);

        document.getElementById("bids-panel").innerHTML =
            "<p class=\"message error\">No se pudieron cargar las actividades.</p>";
    }
}

const tabButtons =
    document.querySelectorAll(".activities-tab");

for (const idx_tk of tabButtons) {
    idx_tk.addEventListener("click", () => {
        const panels =
            document.querySelectorAll(".tab-panel");

        for (const idx_tk of panels) {
            idx_tk.classList.add("hidden");
        }

        for (const idx_tk of tabButtons) {
            idx_tk.classList.remove("active");
        }

        const target =
            document.getElementById(idx_tk.dataset.target);

        if (target) {
            target.classList.remove("hidden");
        }

        idx_tk.classList.add("active");
    });
}

initializeActivities();
