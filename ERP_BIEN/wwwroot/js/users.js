
document.addEventListener('DOMContentLoaded', () => {

            // ============================
            // ✅ TOAST GLOBAL USERS (CORRECTO)
            // ============================
            function showToast(message, type = "success") {
                let toastEl = document.getElementById("usersToast");

                if (!toastEl) {
                    const container = document.createElement("div");
                    container.className = "toast-container position-fixed bottom-0 end-0 p-3";
                    container.style.zIndex = "9999";

                    container.innerHTML = `
                <div id="usersToast" class="toast text-white border-0 shadow-lg">
                    <div class="d-flex">
                        <div class="toast-body fw-semibold" id="usersToastMsg"></div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>`;

                    document.body.appendChild(container);
                    toastEl = document.getElementById("usersToast");
                }

                const msgEl = document.getElementById("usersToastMsg");
                if (msgEl) msgEl.textContent = message;

                toastEl.className = "toast text-white border-0 shadow-lg";
                if (type === "success") toastEl.classList.add("bg-success");
                if (type === "error") toastEl.classList.add("bg-danger");
                if (type === "info") toastEl.classList.add("bg-primary");

                new bootstrap.Toast(toastEl, { delay: 2500 }).show();
            }

    // ============================
    // ✅ TOAST BACKEND
    // ============================
    const serverToastMsg = '@(TempData["ToastMsg"] ?? "")';
        const serverToastType = '@(TempData["ToastType"] ?? "")';
        if (serverToastMsg) {
            showToast(serverToastMsg, serverToastType || "info");
    }

        // ============================
        // ✅ SIN VALIDACIÓN (QUITADO)
        // ============================

        const createForm = document.querySelector('#createModal form');
        const editForm   = document.querySelector('#editModal form');

        console.log("CreateForm:", createForm);
        console.log("EditForm:", editForm);

        createForm?.addEventListener("submit", function () {
            console.log("Submit CREATE OK");
    });

        editForm?.addEventListener("submit", function () {

            console.log("Submit EDIT OK");

        // Botón guardar
        const btn = this.querySelector(".btn-save-pro");

        if (btn) {

            // Deshabilitar para evitar doble submit
            btn.disabled = true;

        // Cambiar contenido visual
        btn.innerHTML = `
        <span class="spinner-border spinner-border-sm"></span>
        Guardando...
        `;
    }
});
    // ============================
    // TOOLTIPS
    // ============================
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));

        let lastDetailsUser = null;

    // ============================
    // CLICK EN FILA → DETALLES
    // ============================
    document.querySelectorAll(".user-row").forEach(row => {
            row.addEventListener("click", async (e) => {
                if (e.target.closest("button") || e.target.closest("[data-bs-toggle]")) return;
                const id = row.dataset.id;
                if (!id) return;
                await loadDetails(id);
            });
    });

    // ============================
    // EDITAR
    // ============================
    document.querySelectorAll(".edit-btn").forEach(btn => {
            btn.addEventListener("click", async (e) => {
                e.stopPropagation();
                const id = btn.dataset.id;
                if (!id) return;

                try {
                    const res = await fetch(`/Users/Details/${id}`);
                    if (!res.ok) throw new Error("Error fetching details");
                    const u = await res.json();

                    fillEditModal(u);
                    new bootstrap.Modal(document.getElementById("editModal")).show();
                } catch {
                    showToast("No se pudo cargar el usuario ❌", "error");
                }
            });
    });

        // ============================
        // DELETE (PRO + ROBUSTO)
        // ============================
        let userIdToDelete = null;

document.querySelectorAll(".delete-btn").forEach(btn => {
            btn.addEventListener("click", (e) => {
                e.stopPropagation();

                userIdToDelete = btn.dataset.id;

                // ✅ OPCIONAL: mostrar info del usuario en el modal (si existen estos IDs)
                // (Si no existen, no pasa nada)
                const row = document.querySelector(`tr[data-id="${userIdToDelete}"]`);
                const fullName = row?.querySelector("td:nth-child(1)")?.innerText?.trim();
                const domain = row?.querySelector("td:nth-child(2)")?.innerText?.trim();

                const nameEl = document.getElementById("deleteUserName");
                const domEl = document.getElementById("deleteUserDomain");
                if (nameEl) nameEl.textContent = fullName || "—";
                if (domEl) domEl.textContent = domain || "";

                // ✅ Abrir modal (evita instancias duplicadas)
                bootstrap.Modal.getOrCreateInstance(document.getElementById("deleteModal")).show();
            });
});

document.getElementById("confirmDeleteBtn")?.addEventListener("click", async () => {
    if (!userIdToDelete) return;

        const btn = document.getElementById("confirmDeleteBtn");
        const oldBtnText = btn?.innerText;

        // ✅ Evitar doble click
        if (btn) {
            btn.disabled = true;
        btn.innerText = "Eliminando...";
    }

        // ✅ Antiforgery token (desde tu input global)
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        try {
        const response = await fetch("/Users/Delete", {
            method: "POST",
        headers: {
            "Content-Type": "application/json",
        "RequestVerificationToken": token
            },
        body: JSON.stringify({id: userIdToDelete })
        });

        if (!response.ok) throw new Error();

        // ✅ Cerrar modal
        bootstrap.Modal.getOrCreateInstance(document.getElementById("deleteModal")).hide();

        // ✅ Quitar fila
        document.querySelector(`tr[data-id="${userIdToDelete}"]`)?.remove();

        showToast("Usuario eliminado correctamente ✅", "success");

        // ✅ reset
        userIdToDelete = null;

    } catch {
            showToast("Error al eliminar usuario ❌", "error");
    } finally {
        if (btn) {
            btn.disabled = false;
        btn.innerText = oldBtnText || "Eliminar";
        }
    }
});


// ============================
// TOGGLE ACTIVO (usa pill-active/pill-inactive)
// ============================
document.querySelectorAll(".toggle-active-btn").forEach(btn => {
            btn.addEventListener("click", async function (e) {
                e.stopPropagation();

                const id = this.dataset.id;
                const oldText = this.innerText;

                this.disabled = true;
                this.innerText = "Guardando...";

                try {
                    const response = await fetch(`/Users/ToggleActive?id=${id}`, { method: "POST" });
                    if (!response.ok) throw new Error();

                    const data = await response.json();
                    const isActive = !!data.isActive;

                    // Texto
                    this.innerText = isActive ? "Activo" : "Inactivo";

                    // ✅ Clases correctas (verde/rojo)
                    this.classList.remove("pill-active", "pill-inactive");
                    this.classList.add(isActive ? "pill-active" : "pill-inactive");

                    showToast(isActive ? "Usuario activado ✅" : "Usuario desactivado ✅", "info");
                } catch {
                    this.innerText = oldText;
                    showToast("Error al cambiar el estado ❌", "error");
                } finally {
                    this.disabled = false;
                }
            });
});

    // ============================
    // EXPORT EXCEL
    // ============================
    document.getElementById("btnExportExcel")?.addEventListener("click", () => {
            showToast("Generando archivo Excel...", "info");
    });

        // ============================
        // DETAILS (PRO)
        // ============================
        async function loadDetails(id) {
    try {
        const res = await fetch(`/Users/Details/${id}`);
        if (!res.ok) throw new Error();
        const u = await res.json();

        lastDetailsUser = u;

        // Nombre + dominio
        const fullName = `${u.name ?? ''} ${u.lastName ?? ''}`.trim();
        document.getElementById("det-name").innerText = fullName || "—";
        document.getElementById("det-domain").innerText = u.domainUser ?? "—";

        // Equipo (badge-team)
        const teamEl = document.getElementById("det-team");
        if (teamEl) {
            teamEl.innerHTML = u.teamName
                ? `<span class="badge-team">${escapeHtml(u.teamName)}</span>`
                : `<span class="text-muted">Sin equipo</span>`;
        }

        // Rol (badge-role)
        const roleEl = document.getElementById("det-role");
        if (roleEl) {
            roleEl.innerHTML = `<span class="badge-role">${escapeHtml(u.roleName ?? "Sin rol")}</span>`;
        }

        // Estado (badge)
        const statusEl = document.getElementById("det-status");
        if (statusEl) {
            statusEl.innerHTML = u.isActive
                ? `<span class="badge bg-success px-3 py-2">✅ Activo</span>`
                : `<span class="badge bg-secondary px-3 py-2">⛔ Inactivo</span>`;
        }

        // Abrir modal
        new bootstrap.Modal(document.getElementById("detailsModal")).show();

    } catch {
            showToast("No se pudieron cargar los detalles ❌", "error");
    }
}

        // Helper para evitar problemas si algún texto trae caracteres raros
        function escapeHtml(str) {
    return (str ?? "").toString()
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}


        function fillEditModal(u) {
            document.getElementById("edit-id").value = u.id ?? '';
        document.getElementById("edit-name").value = u.name ?? '';
        document.getElementById("edit-lastname").value = u.lastName ?? '';
        document.getElementById("edit-domain").value = u.domainUser ?? '';
        document.getElementById("edit-team").value = u.teamId ?? '';
        document.getElementById("edit-role").value = u.roleId ?? '';
        document.getElementById("edit-isactive").value = u.isActive ? "true" : "false";

        const full = `${u.name ?? ""} ${u.lastName ?? ""}`.trim();
  document.getElementById("edit-preview-name").textContent = full || "—";
  document.getElementById("edit-preview-domain").textContent = u.domainUser ?? "—";
}


});
document.querySelectorAll(".details-btn").forEach(btn => {
    btn.addEventListener("click", async (e) => {
        e.stopPropagation();
        const id = btn.dataset.id;
        if (!id) return;
        await loadDetails(id);
    });
});

