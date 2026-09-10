const realtimeParameters = new URLSearchParams(window.location.search);
const realtimeAuctionId = realtimeParameters.get("auctionId");

async function showRealtimeMessage(message) {
    const messageElement = document.getElementById("bid-message");

    messageElement.textContent = message;
    messageElement.classList.remove("error");
    messageElement.classList.remove("hidden");
}

async function startRealtimeConnection() {
    if (!realtimeAuctionId || !window.signalR) {
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/auctions")
        .withAutomaticReconnect()
        .build();

    connection.on("BidPlaced", async () => {
        await window.reloadAuctionFromRealtime();

        await showRealtimeMessage(
            "Nueva puja recibida. La información se actualizó.");
    });

    connection.on("AuctionExtended", async () => {
        await window.reloadAuctionFromRealtime();

        await showRealtimeMessage(
            "La subasta se extendió por regla anti-sniping.");
    });

    connection.on("AuctionClosed", async () => {
        await window.reloadAuctionFromRealtime();

        await showRealtimeMessage(
            "La subasta fue cerrada automáticamente.");
    });

    connection.onreconnected(async () => {
        await connection.invoke(
            "JoinAuction",
            Number(realtimeAuctionId));
    });

    try {
        await connection.start();

        await connection.invoke(
            "JoinAuction",
            Number(realtimeAuctionId));
    } catch (error) {
        console.error(
            "[CODE-ERROR] - no se pudo conectar SignalR.",
            error);

        setTimeout(startRealtimeConnection, 5000);
    }
}

startRealtimeConnection();