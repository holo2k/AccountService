(function () {
    // Конфигурация событий
    const events = [
        {
            name: 'AccountOpened',
            example: {
                eventId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                type: "AccountOpened",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                    causationId: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
                },
                payload: {
                    accountId: "c5d4e3f2-1a2b-4c3d-5e6f-7a8b9c0d1e2f",
                    ownerId: "a8f9d482-7e3a-4c54-b5a2-1e0e8f9d482e",
                    currency: "RUB",
                    type: "Checking"
                }
            }
        },
        {
            name: 'TransferCompleted',
            example: {
                eventId: "d3d94468-7397-4a1f-8b85-1d88d6c6a7d3",
                type: "TransferCompleted",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "d3d94468-7397-4a1f-8b85-1d88d6c6a7d3",
                    causationId: "d3d94468-7397-4a1f-8b85-1d88d6c6a7d3"
                },
                payload: {
                    sourceAccountId: "a8f9d482-7e3a-4c54-b5a2-1e0e8f9d482e",
                    destinationAccountId: "b7e8c9a1-6d2f-4b8c-9a1d-3e0f8e7d6c5b",
                    amount: 150.75,
                    currency: "USD",
                    transferId: "c5d4e3f2-1a2b-4c3d-5e6f-7a8b9c0d1e2f"
                }
            }
        },
        {
            name: 'MoneyCredited',
            example: {
                eventId: "e7f8c9b0-1a2b-4c3d-5e6f-7a8b9c0d1e2f",
                type: "MoneyCredited",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "e7f8c9b0-1a2b-4c3d-5e6f-7a8b9c0d1e2f",
                    causationId: "e7f8c9b0-1a2b-4c3d-5e6f-7a8b9c0d1e2f"
                },
                payload: {
                    accountId: "d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9g",
                    amount: 200.00,
                    currency: "EUR",
                    operationId: "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
                }
            }
        },
        {
            name: 'MoneyDebited',
            example: {
                eventId: "f1e2d3c4-5b6a-7c8d-9e0f-1a2b3c4d5e6f",
                type: "MoneyDebited",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "f1e2d3c4-5b6a-7c8d-9e0f-1a2b3c4d5e6f",
                    causationId: "f1e2d3c4-5b6a-7c8d-9e0f-1a2b3c4d5e6f"
                },
                payload: {
                    accountId: "d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9g",
                    amount: 50.25,
                    currency: "EUR",
                    operationId: "b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
                    reason: "Payment for services"
                }
            }
        },
        {
            name: 'InterestAccrued',
            example: {
                eventId: "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
                type: "InterestAccrued",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
                    causationId: "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
                },
                payload: {
                    accountId: "d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9g",
                    periodFrom: "2025-09-01",
                    periodTo: "2025-09-02",
                    amount: 5.75
                }
            }
        },
        {
            name: 'ClientBlocked',
            example: {
                eventId: "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
                type: "ClientBlocked",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
                    causationId: "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"
                },
                payload: {
                    clientId: "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
                }
            }
        },
        {
            name: 'ClientUnblocked',
            example: {
                eventId: "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f",
                type: "ClientUnblocked",
                occurredAt: "2025-08-05T12:34:56Z",
                meta: {
                    version: "v1",
                    source: "account-service",
                    correlationId: "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f",
                    causationId: "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
                },
                payload: {
                    clientId: "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"
                }
            }
        }
    ];

    const envelopeExample = {
        eventId: "guid",
        type: "SomeEvent",
        occurredAt: "2025-08-05T12:34:56Z",
        meta: {
            version: "v1",
            source: "account-service",
            correlationId: "guid",
            causationId: "guid"
        },
        payload: { /* данные события */ }
    };


    function createAccordion(title, json) {
        const container = document.createElement('div');
        container.style.border = "1px solid #e0e0e0";
        container.style.borderRadius = "4px";
        container.style.marginBottom = "8px";
        container.style.overflow = "hidden";

        const header = document.createElement('div');
        header.style.background = "#f7f7f7";
        header.style.padding = "10px 15px";
        header.style.cursor = "pointer";
        header.style.userSelect = "none";
        header.style.display = "flex";
        header.style.alignItems = "center";

        const arrow = document.createElement('span');
        arrow.textContent = "▶"; 
        arrow.style.marginRight = "8px";
        arrow.style.transition = "transform 0.2s ease";

        const titleSpan = document.createElement('span');
        titleSpan.textContent = title;

        header.appendChild(arrow);
        header.appendChild(titleSpan);

        const content = document.createElement('pre');
        content.style.margin = "0";
        content.style.padding = "15px";
        content.style.display = "none";
        content.style.background = "#fff";
        content.textContent = JSON.stringify(json, null, 2);

        header.addEventListener('click', () => {
            const isOpen = content.style.display === 'block';
            content.style.display = isOpen ? 'none' : 'block';
            arrow.textContent = isOpen ? "▶" : "▼";
        });

        container.appendChild(header);
        container.appendChild(content);

        return container;
    }

    function insertEvents() {
        const infoContainer = document.querySelector('.information-container');
        if (!infoContainer) return false;

        if (document.getElementById('events-section')) return true;

        const eventsSection = document.createElement('section');
        eventsSection.id = 'events-section';
        eventsSection.classList.add('custom-events');
        eventsSection.innerHTML = `<h2>События</h2>
            <div id="events-container"></div>`;

        infoContainer.appendChild(eventsSection);

        const container = document.getElementById('events-container');

        const envelopeDiv = createAccordion("Общая оболочка события (envelope)", envelopeExample);
        container.appendChild(envelopeDiv);

        events.forEach(e => {
            const eventDiv = createAccordion(e.name, e.example);
            container.appendChild(eventDiv);
        });

        return true;
    }

    const interval = setInterval(() => {
        if (insertEvents()) clearInterval(interval);
    }, 300);

})();