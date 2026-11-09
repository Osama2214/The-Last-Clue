"use strict";

function createGameConnection() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/game")
        .withAutomaticReconnect()
        .build();
    const subscriptions = new Map();

    async function subscribe(method, id) {
        await connection.invoke(method, id);
        subscriptions.set(method, id);
    }

    connection.onreconnected(async () => {
        try {
            for (const [method, id] of subscriptions) {
                await connection.invoke(method, id);
            }
        } catch {
            await connection.stop();
        }
    });

    return {
        start: () => connection.start(),
        stop: () => connection.stop(),
        on: (eventName, handler) => connection.on(eventName, handler),
        joinRoom: id => subscribe("JoinRoom", id),
        joinGame: id => subscribe("JoinGame", id)
    };
}
