const profileParameters = new URLSearchParams(window.location.search);
const currentUserId = profileParameters.get("userId") ?? "2";

const balanceCards = document.getElementById("balance-cards");
const depositForm = document.getElementById("deposit-form");
const depositAmount = document.getElementById("deposit-amount");
const depositMessage = document.getElementById("deposit-message");

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

function renderBalance(balance) {
    balanceCards.innerHTML = `
        <article class="balance-card">
            <span>Saldo total</span>
            <strong>${formatCurrency(balance.totalBalance)}</strong>
        </article>

        <article class="balance-card retained">
            <span>Saldo retenido</span>
            <strong>${formatCurrency(balance.heldBalance)}</strong>
        </article>

        <article class="balance-card available">
            <span>Saldo disponible</span>
            <strong>${formatCurrency(balance.availableBalance)}</strong>
        </article>
    `;
}

function renderBids(items) {
    const panel = document.getElementById("bids-panel");

    if (items.length === 0) {
        panel.innerHTML = "<p class=\"message\">Todavía no participaste en subastas.</p>";
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
                        <p>Mi mejor puja: ${formatCurrency(idx_tk.myHighestBid)}</p>
                        <p>Oferta actual: ${formatCurrency(idx_tk.currentHighestBid)}</p>
                    </div>
                    <span class="status-badge">${badge}</span>
                </article>
            `);
    }
}

function renderAuctions(items) {
    const panel = document.getElementById("auctions-panel");

    if (items.length === 0) {
        panel.innerHTML = "<p class=\"message\">Todavía no publicaste subastas.</p>";
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
                            ${idx_tk.highestBid
                ? formatCurrency(idx_tk.highestBid)
                : "Sin pujas"}
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
        panel.innerHTML = "<p class=\"message\">No hay movimientos.</p>";
        return;
    }

    panel.innerHTML = "";

    for (const idx_tk of items) {
        panel.insertAdjacentHTML(
            "beforeend",
            `
                <article class="transaction-item">
                    <div>
                        <strong>${escapeHtml(idx_tk.type)}</strong>
                        <p>
                            ${new Date(idx_tk.createdAtUtc)
                .toLocaleString("es-AR")}
                        </p>
                    </div>
                    <strong>${formatCurrency(idx_tk.amount)}</strong>
                </article>
            `);
    }
}

async function loadProfile() {
    try {
        const responses = await Promise.all([
            fetch(`/api/v1/wallets/${currentUserId}`),
            fetch(`/api/v1/wallets/${currentUserId}/transactions`),
            fetch(`/api/v1/users/${currentUserId}/bids`),
            fetch(`/api/v1/users/${currentUserId}/auctions`)
        ]);

        for (const idx_tk of responses) {
            if (!idx_tk.ok) {
                throw new Error("No se pudo cargar la información del perfil.");
            }
        }

        const balance = await responses[0].json();
        const transactions = await responses[1].json();
        const bids = await responses[2].json();
        const auctions = await responses[3].json();

        renderBalance(balance);
        renderTransactions(transactions);
        renderBids(bids);
        renderAuctions(auctions);
    } catch (error) {
        console.error("[CODE-ERROR] -", error);

        balanceCards.innerHTML =
            "<p class=\"message error\">No se pudo cargar el perfil.</p>";
    }
}

depositForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const amount = Number(depositAmount.value);

    try {
        const response = await fetch(
            `/api/v1/wallets/${currentUserId}/transactions`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ amount })
            });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message ?? "No se pudo acreditar saldo.");
        }

        depositMessage.textContent = "Saldo acreditado correctamente.";
        depositMessage.classList.remove("error");
        depositMessage.classList.remove("hidden");

        depositAmount.value = "";

        await loadProfile();
    } catch (error) {
        console.error("[CODE-ERROR] -", error);

        depositMessage.textContent = error.message;
        depositMessage.classList.add("error");
        depositMessage.classList.remove("hidden");
    }
});

const tabButtons = document.querySelectorAll(".tab-button");

for (const idx_tk of tabButtons) {
    idx_tk.addEventListener("click", () => {
        const panels = document.querySelectorAll(".tab-panel");

        for (const idx_tk of panels) {
            idx_tk.classList.add("hidden");
        }

        for (const idx_tk of tabButtons) {
            idx_tk.classList.remove("active");
        }

        document
            .getElementById(idx_tk.dataset.target)
            .classList.remove("hidden");

        idx_tk.classList.add("active");
    });
}

loadProfile();