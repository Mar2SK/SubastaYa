const storedUser = localStorage.getItem("subastaYaUser");

if (!storedUser) {
    window.location.href = "/login.html";
    throw new Error(
        "[CODE-ERROR] - No hay un usuario autenticado."
    );
}

const currentUser = JSON.parse(storedUser);

const parameters = new URLSearchParams(window.location.search);
const auctionId = parameters.get("auctionId");

const loadingMessage = document.getElementById("loading-message");
const errorMessage = document.getElementById("error-message");
const auctionDetail = document.getElementById("auction-detail");
const bidHistory = document.getElementById("bid-history");
const bidForm = document.getElementById("bid-form");
const bidMessage = document.getElementById("bid-message");
const bidAmountInput = document.getElementById("bid-amount");

let currentAuction = null;

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

function getStatusClass(status) {
    switch (status) {
        case "ACTIVA":
            return "status-active";

        case "PROGRAMADA":
            return "status-programmed";

        case "FINALIZADA":
            return "status-finished";

        case "DESIERTA":
            return "status-deserted";

        default:
            return "";
    }
}

function getRemainingTime(targetUtc, status) {
    if (
        status === "FINALIZADA" ||
        status === "DESIERTA"
    ) {
        return "Finalizada";
    }

    const remainingMilliseconds =
        new Date(targetUtc).getTime() - Date.now();

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

function renderAuction() {
    const timerLabel =
        currentAuction.status === "PROGRAMADA"
            ? "Comienza en:"
            : "Finaliza en:";

    const visiblePrice =
        currentAuction.highestBid ??
        currentAuction.basePrice;

    const minimumAmount =
        currentAuction.highestBid
            ? currentAuction.highestBid +
            currentAuction.minimumIncrement
            : currentAuction.basePrice;

    auctionDetail.innerHTML = `
        <article class="detail-card">
            <img
                src="${escapeHtml(currentAuction.imageUrl)}"
                alt="${escapeHtml(currentAuction.title)}"
                width="600"
                height="400">

            <div class="detail-content">
                <span class="category">
                    ${escapeHtml(currentAuction.categoryName)}
                </span>

                <h1>
                    ${escapeHtml(currentAuction.title)}
                </h1>

                <p class="description">
                    ${escapeHtml(currentAuction.description)}
                </p>

                <p class="details">
                    Publicada por
                    ${escapeHtml(currentAuction.sellerName)}
                </p>

                <div class="price-panel">
                    <span>Puja actual</span>

                    <strong>
                        ${formatCurrency(visiblePrice)}
                    </strong>

                    <small>
                        Incremento mínimo:
                        ${formatCurrency(
        currentAuction.minimumIncrement
    )}
                    </small>
                </div>

                <p class="timer detail-timer">
                    ${timerLabel}
                    <span id="detail-timer"></span>
                </p>

                <p class="details">
                    Estado:
                </p>

                <span class="status-badge ${getStatusClass(currentAuction.status)}">
                    ${escapeHtml(currentAuction.status)}
                </span>
            </div>
        </article>
    `;

    const isAvailable =
        currentAuction.status === "ACTIVA" &&
        new Date(currentAuction.startAtUtc).getTime() <= Date.now() &&
        new Date(currentAuction.endAtUtc).getTime() > Date.now();

    if (bidForm) {
        const submitButton =
            bidForm.querySelector("button[type='submit']");

        if (bidAmountInput) {
            bidAmountInput.disabled = !isAvailable;
        }

        if (submitButton) {
            submitButton.disabled = !isAvailable;
        }
    }

    if (bidMessage) {
        if (currentAuction.status === "PROGRAMADA") {
            bidMessage.textContent =
                `Esta subasta todavía no comenzó. ` +
                `Podrás ofertar en ${getRemainingTime(
                    currentAuction.startAtUtc,
                    currentAuction.status
                )}.`;

            bidMessage.classList.remove("hidden");
            bidMessage.classList.remove("error");
        }
        else if (!isAvailable) {
            bidMessage.textContent =
                "Esta subasta ya no está disponible para recibir ofertas.";

            bidMessage.classList.remove("hidden");
            bidMessage.classList.add("error");
        }
        else {
            bidMessage.textContent = "";
            bidMessage.classList.add("hidden");
            bidMessage.classList.remove("error");
        }
    }

    updateTimer();
}

function renderBidHistory() {
    bidHistory.innerHTML = "";

    if (
        !currentAuction.bids ||
        currentAuction.bids.length === 0
    ) {
        bidHistory.innerHTML =
            `<p class="message">
                Todavía no hay pujas.
            </p>`;

        return;
    }

    for (const idx_tk of currentAuction.bids) {
        bidHistory.insertAdjacentHTML(
            "beforeend",
            `
                <article class="history-item">
                    <div>
                        <strong>
                            ${escapeHtml(idx_tk.buyerAlias)}
                        </strong>

                        <p>
                            ${new Date(
                idx_tk.bidAtUtc
            ).toLocaleString("es-AR")}
                        </p>
                    </div>

                    <strong>
                        ${formatCurrency(idx_tk.amount)}
                    </strong>
                </article>
            `
        );
    }
}

function updateTimer() {
    if (!currentAuction) {
        return;
    }

    const timer =
        document.getElementById("detail-timer");

    if (!timer) {
        return;
    }

    if (currentAuction.status === "PROGRAMADA") {
        timer.textContent =
            getRemainingTime(
                currentAuction.startAtUtc,
                currentAuction.status
            );

        return;
    }

    timer.textContent =
        getRemainingTime(
            currentAuction.endAtUtc,
            currentAuction.status
        );
}

async function loadAuction() {
    if (!auctionId) {
        errorMessage.textContent =
            "[CODE-ERROR] - No se indicó una subasta.";

        errorMessage.classList.remove("hidden");
        loadingMessage.classList.add("hidden");

        return;
    }

    try {
        const response = await fetch(
            `/api/v1/auctions/${auctionId}`
        );

        if (!response.ok) {
            throw new Error(
                "[CODE-ERROR] - No se pudo cargar la subasta."
            );
        }

        currentAuction = await response.json();

        renderAuction();
        renderBidHistory();

        auctionDetail.classList.remove("hidden");
    } catch (error) {
        console.error(
            "[CODE-ERROR] -",
            error
        );

        errorMessage.textContent =
            "[CODE-ERROR] - No se pudo cargar la subasta solicitada.";

        errorMessage.classList.remove("hidden");
    } finally {
        loadingMessage.classList.add("hidden");
    }
}

if (bidForm) {
    bidForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (!currentAuction) {
            return;
        }
        
        const now = Date.now();
        
        const isAvailable =
            currentAuction.status === "ACTIVA" &&
            new Date(currentAuction.startAtUtc).getTime() <= now &&
            new Date(currentAuction.endAtUtc).getTime() > now;
        
        if (!isAvailable) {
            if (currentAuction.status === "PROGRAMADA") {
                bidMessage.textContent =
                    `Esta subasta todavía no comenzó. ` +
                    `Podrás ofertar en ${getRemainingTime(
                        currentAuction.startAtUtc,
                        currentAuction.status
                    )}.`;
            } else {
                bidMessage.textContent =
                    "Esta subasta ya no está disponible para recibir ofertas.";
            }
        
            bidMessage.classList.remove("hidden");
            bidMessage.classList.add("error");
        
            return;
        }

        const buyerId = currentUser.userId;
        const amount = Number(bidAmountInput.value);

        bidMessage.classList.add("hidden");

        try {
            const response = await fetch(
                `/api/v1/auctions/${auctionId}/bids`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        buyerId,
                        amount
                    })
                }
            );

            const data = await response.json();

            if (!response.ok) {
                throw new Error(
                    data.message ??
                    "[CODE-ERROR] - No se pudo registrar la puja."
                );
            }

            bidMessage.textContent =
                "¡Puja registrada correctamente!";

            bidMessage.classList.remove("hidden");
            bidMessage.classList.remove("error");

            await loadAuction();
        } catch (error) {
            console.error(
                "[CODE-ERROR] -",
                error
            );

            bidMessage.textContent =
                error.message;

            bidMessage.classList.add("error");
            bidMessage.classList.remove("hidden");
        }
    });
}

setInterval(updateTimer, 1000);

loadAuction();

window.reloadAuctionFromRealtime = async () => {
    await loadAuction();
};
const logoutButton = document.getElementById("logout-button");

if (logoutButton) {
    logoutButton.addEventListener("click", () => {
        localStorage.removeItem("subastaYaUser");
        window.location.href = "/";
    });
}