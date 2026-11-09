"use strict";

async function apiWrite(path, method, body) {
    const url = new URL(path, location.origin);
    if (url.origin !== location.origin) {
        throw new Error("API requests must use the same origin.");
    }

    const token = document.querySelector('meta[name="csrf-token"]').content;
    const headers = { "X-CSRF-TOKEN": token };
    if (body !== undefined) {
        headers["Content-Type"] = "application/json";
    }

    return fetch(url, {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body),
        credentials: "same-origin"
    });
}
