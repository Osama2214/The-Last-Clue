"use strict";

for (const form of document.querySelectorAll("[data-auth-form]")) {
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const action = form.dataset.authForm;
        const submit = form.querySelector('button[type="submit"]');
        const error = form.querySelector("[data-form-error]") || document.querySelector("[data-global-error]");
        const fields = form.querySelectorAll("[data-field-error]");
        const body = action === "logout" ? undefined : Object.fromEntries(new FormData(form));
        error.hidden = true;
        submit.disabled = true;
        form.setAttribute("aria-busy", "true");
        for (const field of fields) {
            field.textContent = "";
            form.elements.namedItem(field.dataset.fieldError)?.removeAttribute("aria-invalid");
        }

        try {
            const response = await apiWrite("/api/v1/auth/" + action, "POST", body);
            if (response.ok) {
                location.assign(action === "register" ? "/Account/Login" : "/");
                return;
            }

            const problem = await response.json();
            error.textContent = problem.message || "The request could not be completed.";
            error.hidden = false;
            for (const field of fields) {
                const message = problem.fields?.[field.dataset.fieldError];
                if (message) {
                    field.textContent = message;
                    form.elements.namedItem(field.dataset.fieldError)?.setAttribute("aria-invalid", "true");
                }
            }
        } catch {
            error.textContent = "Could not reach the server. Try again.";
            error.hidden = false;
        } finally {
            form.removeAttribute("aria-busy");
            submit.disabled = false;
        }
    });
}
