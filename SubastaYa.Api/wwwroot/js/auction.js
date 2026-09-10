const placeBidButton = document.getElementById("placeBidButton");
const bidAmountInput = document.getElementById("bidAmount");
const currentBidElement = document.getElementById("currentBid");
const minimumBidElement = document.getElementById("minimumBid");
const bidStatusElement = document.getElementById("bidStatus");
const bidMessage = document.getElementById("bidMessage");

let currentBid = 45000;
let minimumIncrement = 5000;

if (placeBidButton) {
    placeBidButton.addEventListener(
        "click",
        () => {
            bidMessage.textContent = "";
            const bidAmount = Number(bidAmountInput.value);
            const minimumBid = currentBid + minimumIncrement;
            if (!bidAmount) {
                bidMessage.textContent = "Ingresá un monto para ofertar.";
                return;
            }

            if (bidAmount < minimumBid) {
                bidMessage.textContent = `La oferta mínima es $${minimumBid.toLocaleString("es-AR")}.`;
                return;
            }

            currentBid = bidAmount;
            currentBidElement.textContent = `$${currentBid.toLocaleString("es-AR")}`;
            minimumBidElement.textContent = `$${(currentBid + minimumIncrement).toLocaleString("es-AR")}`;
            bidStatusElement.textContent = "Liderando";
            bidMessage.textContent = "Oferta realizada correctamente.";
            bidMessage.style.color = "#70d99a";
            bidAmountInput.value = "";
        }
    );
}

const shareButton = document.getElementById("shareAuctionBtn");

if (shareButton) {
    shareButton.addEventListener(
        "click",
        async () => {
            try {
                await navigator.clipboard.writeText(
                    window.location.href
                );
                shareButton.textContent = "✓ Enlace copiado";
                setTimeout(
                    () => {
                        shareButton.textContent = "⤴ Compartir";

                    },
                    1500
                );
            }
            catch (error) {
                console.error("No se pudo copiar el enlace.", error);
            }
        }
    );
}

const timerElement = document.getElementById("auctionTimer");

let remainingSeconds = 18 * 60 + 42;

function updateTimer() {
    if (!timerElement) {
        return;
    }

    const minutes = Math.floor(remainingSeconds / 60);
    const seconds = remainingSeconds % 60;

    timerElement.textContent = `00:${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;

    if (remainingSeconds <= 60) {
        timerElement.classList.add("critical-time");
    }

    if (remainingSeconds > 0) {
        remainingSeconds--;
    }
}

updateTimer();

setInterval(updateTimer, 1000);