const auctionGrid = document.getElementById("auction-grid");
const loadingMessage = document.getElementById("loading-message");
const errorMessage = document.getElementById("error-message");
const auctionCount = document.getElementById("auction-count");
const statusFilter = document.getElementById("status-filter");
const categoryFilter = document.getElementById("category-filter");
const minimumPriceFilter = document.getElementById(
    "minimum-price-filter");
const maximumPriceFilter = document.getElementById(
    "maximum-price-filter");
const orderFilter = document.getElementById("order-filter");
const applyFiltersButton = document.getElementById(
    "apply-filters-button");

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

function getRemainingTime(targetUtc) {
    const targetDate =
        new Date(targetUtc);

    if (Number.isNaN(targetDate.getTime())) {
        return "Fecha no disponible";
    }

    const remainingMilliseconds =
        targetDate.getTime() - Date.now();

    if (remainingMilliseconds <= 0) {
        return "0h 0m 0s";
    }

    const totalSeconds =
        Math.floor(remainingMilliseconds / 1000);

    const hours =
        Math.floor(totalSeconds / 3600);

    const minutes =
        Math.floor((totalSeconds % 3600) / 60);

    const seconds =
        totalSeconds % 60;

    return `${hours}h ${minutes}m ${seconds}s`;
}

function renderAuctions(auctions) {
    auctionGrid.innerHTML = "";

    for (const idx_tk of auctions) {
        const visiblePrice = idx_tk.highestBid ?? idx_tk.basePrice;

        auctionGrid.insertAdjacentHTML(
            "beforeend",
            `
                <a
    class="auction-card-link"
    href="auction.html?auctionId=${idx_tk.id}">
    <article
        class="auction-card"
        data-start-at="${idx_tk.startAtUtc}"
        data-end-at="${idx_tk.endAtUtc}"
        data-status="${idx_tk.status}">
        <img
            src="${escapeHtml(idx_tk.imageUrl)}"
            alt="${escapeHtml(idx_tk.title)}">
        <div class="card-content">
            <span class="category">
                ${escapeHtml(idx_tk.categoryName)}
            </span>
            <h3>${escapeHtml(idx_tk.title)}</h3>
            <p class="price">${formatCurrency(visiblePrice)}</p>
            <p class="details">
                ${idx_tk.bidCount} puja(s) ·
                ${escapeHtml(idx_tk.status)}
            </p>
            <p class="timer"></p>
        </div>
    </article>
</a>
            `);
    }

    updateTimers();
}

function updateTimers() {
    const cards =
        document.querySelectorAll(".auction-card");

    for (const idx_tk of cards) {
        const timer =
            idx_tk.querySelector(".timer");

        const status =
            idx_tk.dataset.status;

        if (status === "PROGRAMADA") {
            timer.textContent =
                `Comienza en: ${getRemainingTime(
                    idx_tk.dataset.startAt
                )}`;

            continue;
        }

        if (status === "ACTIVA") {
            timer.textContent =
                `Finaliza en: ${getRemainingTime(
                    idx_tk.dataset.endAt
                )}`;

            continue;
        }

        timer.textContent = "Finalizada";
    }
}

async function loadAuctions() {
    loadingMessage.classList.remove("hidden");
    errorMessage.classList.add("hidden");
    auctionGrid.innerHTML = "";

    const parameters = new URLSearchParams({
        page: "1",
        pageSize: "50",
        orderBy: orderFilter.value
    });

    if (statusFilter.value) {
        parameters.set("status", statusFilter.value);
    }

    if (categoryFilter.value) {
        parameters.set("categoryId", categoryFilter.value);
    }

    if (minimumPriceFilter.value) {
        parameters.set(
            "minimumPrice",
            minimumPriceFilter.value);
    }

    if (maximumPriceFilter.value) {
        parameters.set(
            "maximumPrice",
            maximumPriceFilter.value);
    }

    try {
        const response = await fetch(
            `/api/v1/auctions?${parameters.toString()}`);

        if (!response.ok) {
            throw new Error("No se pudieron cargar las subastas.");
        }

        const data = await response.json();

        auctionCount.textContent =
            `${data.totalItems} subasta(s) encontradas`;

        renderAuctions(data.items);
    } catch (error) {
        console.error("[CODE-ERROR] -", error);

        errorMessage.textContent =
            "No se pudieron cargar las subastas. Intentá nuevamente.";

        errorMessage.classList.remove("hidden");
    } finally {
        loadingMessage.classList.add("hidden");
    }
}

async function loadCategories() {
    try {
        const response = await fetch("/api/v1/categories");

        if (!response.ok) {
            throw new Error("No se pudieron cargar las categorías.");
        }

        const categories = await response.json();

        for (const idx_tk of categories) {
            const option = document.createElement("option");

            option.value = idx_tk.id;
            option.textContent = idx_tk.name;

            categoryFilter.appendChild(option);
        }
    } catch (error) {
        console.error("[CODE-ERROR] -", error);
    }
}

applyFiltersButton.addEventListener("click", loadAuctions);

setInterval(updateTimers, 1000);

await Promise.all([
    loadCategories(),
    loadAuctions()
]);