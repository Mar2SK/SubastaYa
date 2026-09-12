const storedUser = localStorage.getItem("subastaYaUser");

if (!storedUser) {
    window.location.href = "/login.html";
    throw new Error(
        "[CODE-ERROR] - No hay un usuario autenticado."
    );
}

const currentUser = JSON.parse(storedUser);

const auctionForm =
    document.getElementById("auction-form");

const categorySelect =
    document.getElementById("categoryId");

const publishButton =
    document.getElementById("publish-button");

const publishMessage =
    document.getElementById("publish-message");

function showMessage(message, isError = false) {
    publishMessage.textContent = message;

    publishMessage.classList.toggle(
        "error",
        isError
    );

    publishMessage.classList.remove("hidden");
}

function toUtcIso(localDateTime) {
    const localDate = new Date(localDateTime);

    return localDate.toISOString();
}

async function loadCategories() {
    try {
        const response =
            await fetch("/api/v1/categories");

        if (!response.ok) {
            throw new Error(
                "[CODE-ERROR] - No se pudieron cargar las categorías."
            );
        }

        const categories =
            await response.json();

        for (const idx_tk of categories) {
            const option =
                document.createElement("option");

            option.value = idx_tk.id;
            option.textContent = idx_tk.name;

            categorySelect.appendChild(option);
        }
    } catch (error) {
        console.error(
            "[CODE-ERROR] -",
            error
        );

        showMessage(
            "[CODE-ERROR] - No se pudieron cargar las categorías.",
            true
        );
    }
}

auctionForm.addEventListener(
    "submit",
    async (event) => {
        event.preventDefault();

        publishMessage.classList.add("hidden");
        publishButton.disabled = true;

        const startAt =
            document.getElementById("startAt").value;

        const endAt =
            document.getElementById("endAt").value;

        const startDate =
            new Date(startAt);

        const endDate =
            new Date(endAt);

        if (endDate <= startDate) {
            showMessage(
                "[CODE-ERROR] - La fecha de finalización debe ser posterior al inicio.",
                true
            );

            publishButton.disabled = false;
            return;
        }

        if (endDate <= new Date()) {
            showMessage(
                "[CODE-ERROR] - La fecha de finalización debe estar en el futuro.",
                true
            );

            publishButton.disabled = false;
            return;
        }

        const request = {
            sellerId: currentUser.userId,
            categoryId: Number(categorySelect.value),
            title:
                document.getElementById("title").value.trim(),
            description:
                document.getElementById("description").value.trim(),
            imageUrl:
                document.getElementById("imageUrl").value.trim(),
            basePrice:
                Number(
                    document.getElementById("basePrice").value
                ),
            minimumIncrement:
                Number(
                    document.getElementById(
                        "minimumIncrement"
                    ).value
                ),
            startAtUtc: toUtcIso(startAt),
            endAtUtc: toUtcIso(endAt)
        };

        if (
            request.basePrice <= 0 ||
            request.minimumIncrement <= 0
        ) {
            showMessage(
                "[CODE-ERROR] - El precio y el incremento deben ser mayores a cero.",
                true
            );

            publishButton.disabled = false;
            return;
        }

        try {
            const response = await fetch(
                "/api/v1/auctions",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(request)
                }
            );

            const data = await response.json();

            if (!response.ok) {
                throw new Error(
                    data.message ??
                    "[CODE-ERROR] - No se pudo publicar la subasta."
                );
            }

            showMessage(
                "¡Subasta publicada correctamente!"
            );

            auctionForm.reset();

            setTimeout(() => {
                window.location.href =
                    `/auction.html?auctionId=${data.id}`;
            }, 800);

        } catch (error) {
            console.error(
                "[CODE-ERROR] -",
                error
            );

            showMessage(
                error.message,
                true
            );
        } finally {
            publishButton.disabled = false;
        }
    }
);

loadCategories();