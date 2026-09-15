const realtimeParameters =
    new URLSearchParams(window.location.search);

const realtimeAuctionId =
    realtimeParameters.get("auctionId");

function showRealtimeMessage(message) {
    const messageElement =
        document.getElementById("bid-message");

    if (!messageElement) {
        return;
    }

    messageElement.textContent = message;

    messageElement.classList.remove(
        "error",
        "hidden"
    );
}

async function reloadAuctionAndShowMessage(message) {
    if (typeof window.reloadAuctionFromRealtime === "function") {
        await window.reloadAuctionFromRealtime();
    }

    showRealtimeMessage(message);
}

async function retryRealtimeConnection() {
    try {
        await startRealtimeConnection();
    } catch (error) {
        console.error(
            "[CODE-ERROR] - error al reintentar SignalR.",
            error
        );
    }
}

async function startRealtimeConnection() {
    if (!realtimeAuctionId || !window.signalR) {
        return;
    }

    const connection =
        new signalR.HubConnectionBuilder()
            .withUrl("/hubs/auctions")
            .withAutomaticReconnect()
            .build();

    connection.on("BidPlaced", async () => {
        await reloadAuctionAndShowMessage(
            "Nueva puja recibida. La información se actualizó."
        );
    });

    connection.on("AuctionExtended", async () => {
        await reloadAuctionAndShowMessage(
            "La subasta se extendió por regla anti-sniping."
        );
    });

    connection.on("AuctionClosed", async () => {
        await reloadAuctionAndShowMessage(
            "La subasta fue cerrada automáticamente."
        );
    });

    connection.onreconnected(async () => {
        await connection.invoke(
            "JoinAuction",
            Number(realtimeAuctionId)
        );
    });

    try {
        await connection.start();

        await connection.invoke(
            "JoinAuction",
            Number(realtimeAuctionId)
        );
    } catch (error) {
        console.error(
            "[CODE-ERROR] - no se pudo conectar SignalR.",
            error
        );

        setTimeout(
            retryRealtimeConnection,
            5000
        );
    }
}

try {
    await startRealtimeConnection();
} catch (error) {
    console.error(
        "[CODE-ERROR] - error al iniciar SignalR.",
        error
    );
}