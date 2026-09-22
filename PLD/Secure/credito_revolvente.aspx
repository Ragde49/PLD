<%@ Page Title="Crédito Revolvente" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="credito_revolvente.aspx.vb" Inherits="PLD.credito_revolvente" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <div>
            <h4 class="mb-1">Crédito Revolvente</h4>
            <div class="text-muted">Línea, disposiciones, pagos y saldo disponible.</div>
        </div>
        <div class="d-flex gap-2">
            <a href="solicitud_pf.aspx" class="btn btn-outline-secondary btn-sm">Volver a solicitudes</a>
            <button type="button" id="btnRefrescar" class="btn btn-primary btn-sm">Refrescar</button>
        </div>
    </div>

    <div id="alertaPagina" class="alert alert-warning d-none"></div>

    <div id="contenidoRevolvente" class="d-none">
        <div class="card mb-3">
            <div class="card-header bg-dark text-white">Identificación de la línea</div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-2"><strong>Solicitud</strong><div id="lblSolicitud">—</div></div>
                    <div class="col-md-4"><strong>Cliente</strong><div id="lblCliente">—</div></div>
                    <div class="col-md-3"><strong>Producto</strong><div id="lblProducto">—</div></div>
                    <div class="col-md-3"><strong>Tipo de crédito</strong><div id="lblTipoCredito">—</div></div>
                    <div class="col-md-3"><strong>RFC</strong><div id="lblRfc">—</div></div>
                    <div class="col-md-3"><strong>CURP</strong><div id="lblCurp">—</div></div>
                    <div class="col-md-3"><strong>Moneda</strong><div id="lblMoneda">—</div></div>
                    <div class="col-md-3"><strong>Estatus</strong><div id="lblEstatus">—</div></div>
                </div>
            </div>
        </div>

        <div class="row g-3 mb-3">
            <div class="col-md-3">
                <div class="card h-100"><div class="card-body">
                    <div class="text-muted">Límite autorizado</div>
                    <div id="kpiLimite" class="fs-4 fw-bold">—</div>
                </div></div>
            </div>
            <div class="col-md-3">
                <div class="card h-100"><div class="card-body">
                    <div class="text-muted">Capital dispuesto</div>
                    <div id="kpiDispuesto" class="fs-4 fw-bold">—</div>
                </div></div>
            </div>
            <div class="col-md-3">
                <div class="card h-100"><div class="card-body">
                    <div class="text-muted">Saldo utilizado</div>
                    <div id="kpiUtilizado" class="fs-4 fw-bold">—</div>
                </div></div>
            </div>
            <div class="col-md-3">
                <div class="card h-100"><div class="card-body">
                    <div class="text-muted">Disponible</div>
                    <div id="kpiDisponible" class="fs-4 fw-bold">—</div>
                </div></div>
            </div>
        </div>

        <div class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center">
                <span class="fw-semibold">Configuración de la línea</span>
                <button type="button" id="btnGuardarLinea" class="btn btn-primary btn-sm">Guardar línea</button>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-4">
                        <label for="montoAutorizado" class="form-label">Límite autorizado *</label>
                        <input type="number" id="montoAutorizado" class="form-control" min="0" step="0.01" />
                        <div class="form-text">No modifica el monto solicitado original.</div>
                    </div>
                    <div class="col-md-4">
                        <label for="vigenciaInicio" class="form-label">Vigencia inicio *</label>
                        <input type="date" id="vigenciaInicio" class="form-control" />
                    </div>
                    <div class="col-md-4">
                        <label for="vigenciaFin" class="form-label">Vigencia fin *</label>
                        <input type="date" id="vigenciaFin" class="form-control" />
                    </div>
                </div>
            </div>
        </div>

        <div class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center bg-primary text-white">
                <span class="fw-semibold">Disposiciones</span>
                <button type="button" id="btnNuevaDisposicion" class="btn btn-success btn-sm">Nueva disposición</button>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table id="tablaDisposiciones" class="table table-sm table-striped table-hover align-middle w-100">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Fecha</th>
                                <th>Referencia</th>
                                <th class="text-end">Monto</th>
                                <th>Estatus</th>
                                <th>Usuario</th>
                                <th>Observaciones</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>

        <div class="card">
            <div class="card-header bg-secondary text-white fw-semibold">Historial de movimientos</div>
            <div class="card-body">
                <div class="alert alert-info">
                    Esta vista es operativa. No sustituye el estado de cuenta contractual/regulatorio.
                </div>
                <div class="table-responsive">
                    <table id="tablaHistorial" class="table table-sm table-bordered table-hover align-middle w-100">
                        <thead>
                            <tr>
                                <th>Fecha</th>
                                <th>Movimiento</th>
                                <th class="text-end">Cargo</th>
                                <th class="text-end">Abono</th>
                                <th class="text-end">Capital abonado</th>
                                <th class="text-end">Saldo principal</th>
                                <th>Referencia</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="modalDisposicion" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Nueva disposición</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <div class="row g-3">
                    <div class="col-12">
                        <label for="dispMonto" class="form-label">Monto *</label>
                        <input type="number" id="dispMonto" class="form-control" min="0" step="0.01" />
                    </div>
                    <div class="col-12">
                        <label for="dispFecha" class="form-label">Fecha *</label>
                        <input type="datetime-local" id="dispFecha" class="form-control" />
                    </div>
                    <div class="col-12">
                        <label for="dispReferencia" class="form-label">Referencia</label>
                        <input type="text" id="dispReferencia" class="form-control" maxlength="100" />
                    </div>
                    <div class="col-12">
                        <label for="dispObservaciones" class="form-label">Observaciones</label>
                        <textarea id="dispObservaciones" class="form-control" rows="3" maxlength="1000"></textarea>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" id="btnAplicarDisposicion" class="btn btn-success">Aplicar disposición</button>
                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
            </div>
        </div>
    </div>
</div>

<script>
document.addEventListener("DOMContentLoaded", function () {
    const H_REV = "/handlers/handler_credito_revolvente.ashx";
    const params = new URLSearchParams(window.location.search);
    const solicitudId = parseInt(params.get("id") || params.get("solicitud_id") || "0", 10);
    const modal = new bootstrap.Modal(document.getElementById("modalDisposicion"));
    let resumen = null;

    function dinero(v) {
        if (v === null || v === undefined || v === "") return "—";
        return Number(v || 0).toLocaleString("es-MX", { style: "currency", currency: "MXN" });
    }

    function fecha(v) {
        if (!v) return "—";
        const d = new Date(v);
        return isNaN(d.getTime()) ? String(v) : d.toLocaleString("es-MX");
    }

    function fechaInput(v) {
        if (!v) return "";
        const d = new Date(v);
        if (isNaN(d.getTime())) return String(v).substring(0, 10);
        return d.toISOString().substring(0, 10);
    }

    function html(v) {
        return String(v ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    async function fetchJson(url, options) {
        const r = await fetch(url, options || {});
        const j = await r.json();
        if (!r.ok || !j.ok) throw new Error(j.mensaje || ("HTTP " + r.status));
        return j;
    }

    function mostrarError(mensaje) {
        const a = document.getElementById("alertaPagina");
        a.textContent = mensaje;
        a.classList.remove("d-none");
        document.getElementById("contenidoRevolvente").classList.add("d-none");
    }

    async function cargarResumen() {
        if (!(solicitudId > 0)) {
            mostrarError("Indica una solicitud válida.");
            return;
        }

        try {
            const j = await fetchJson(H_REV + "?action=resumen&solicitud_id=" + encodeURIComponent(solicitudId));
            resumen = j.data;

            document.getElementById("alertaPagina").classList.add("d-none");
            document.getElementById("contenidoRevolvente").classList.remove("d-none");

            document.getElementById("lblSolicitud").textContent = resumen.solicitud_id;
            document.getElementById("lblCliente").textContent = resumen.cliente || "—";
            document.getElementById("lblProducto").textContent = resumen.producto || "—";
            document.getElementById("lblTipoCredito").textContent = resumen.tipo_credito || "—";
            document.getElementById("lblRfc").textContent = resumen.rfc || "—";
            document.getElementById("lblCurp").textContent = resumen.curp || "—";
            document.getElementById("lblMoneda").textContent = [resumen.moneda_clave, resumen.moneda].filter(Boolean).join(" · ") || "—";
            document.getElementById("lblEstatus").textContent = resumen.estatus || "—";

            document.getElementById("kpiLimite").textContent = dinero(resumen.monto_autorizado);
            document.getElementById("kpiDispuesto").textContent = dinero(resumen.capital_dispuesto);
            document.getElementById("kpiUtilizado").textContent = dinero(resumen.saldo_utilizado);
            document.getElementById("kpiDisponible").textContent = dinero(resumen.disponible);

            document.getElementById("montoAutorizado").value = resumen.monto_autorizado ?? "";
            document.getElementById("vigenciaInicio").value = fechaInput(resumen.fecha_vigencia_inicio);
            document.getElementById("vigenciaFin").value = fechaInput(resumen.fecha_vigencia_fin);
        } catch (e) {
            mostrarError(e.message);
            throw e;
        }
    }

    async function cargarDisposiciones() {
        const j = await fetchJson(H_REV + "?action=listar_disposiciones&solicitud_id=" + encodeURIComponent(solicitudId));
        const tbody = document.querySelector("#tablaDisposiciones tbody");
        tbody.innerHTML = "";

        (j.data || []).forEach(function (x) {
            const activa = String(x.estatus || "").toUpperCase() === "APLICADA" && !!x.activo;
            const tr = document.createElement("tr");
            tr.innerHTML =
                "<td>" + html(x.id) + "</td>" +
                "<td>" + html(fecha(x.fecha_disposicion)) + "</td>" +
                "<td>" + html(x.referencia || "") + "</td>" +
                '<td class="text-end">' + html(dinero(x.monto)) + "</td>" +
                "<td>" + (activa ? '<span class="badge bg-success">APLICADA</span>' : '<span class="badge bg-secondary">' + html(x.estatus) + "</span>") + "</td>" +
                "<td>" + html(x.creado_por || "") + "</td>" +
                "<td>" + html(x.observaciones || x.motivo_reversa || "") + "</td>" +
                "<td>" + (activa ? '<button type="button" class="btn btn-outline-danger btn-sm btn-reversa" data-id="' + html(x.id) + '">Reversar</button>' : "") + "</td>";
            tbody.appendChild(tr);
        });
    }

    async function cargarHistorial() {
        const j = await fetchJson(H_REV + "?action=historial&solicitud_id=" + encodeURIComponent(solicitudId));
        const tbody = document.querySelector("#tablaHistorial tbody");
        tbody.innerHTML = "";

        (j.data || []).forEach(function (x) {
            const tr = document.createElement("tr");
            tr.innerHTML =
                "<td>" + html(fecha(x.fecha)) + "</td>" +
                "<td>" + html(x.tipo) + " #" + html(x.movimiento_id) + "</td>" +
                '<td class="text-end">' + (Number(x.cargo || 0) > 0 ? html(dinero(x.cargo)) : "—") + "</td>" +
                '<td class="text-end">' + (Number(x.abono || 0) > 0 ? html(dinero(x.abono)) : "—") + "</td>" +
                '<td class="text-end">' + (Number(x.capital_abonado || 0) > 0 ? html(dinero(x.capital_abonado)) : "—") + "</td>" +
                '<td class="text-end fw-semibold">' + html(dinero(x.saldo_principal)) + "</td>" +
                "<td>" + html(x.referencia || "") + "</td>";
            tbody.appendChild(tr);
        });
    }

    async function recargarTodo() {
        await cargarResumen();
        await Promise.all([cargarDisposiciones(), cargarHistorial()]);
    }

    document.getElementById("btnRefrescar").addEventListener("click", function () {
        recargarTodo().catch(function () {});
    });

    document.getElementById("btnGuardarLinea").addEventListener("click", async function () {
        const fd = new FormData();
        fd.append("action", "configurar_linea");
        fd.append("solicitud_id", solicitudId);
        fd.append("monto_autorizado", document.getElementById("montoAutorizado").value);
        fd.append("fecha_vigencia_inicio", document.getElementById("vigenciaInicio").value);
        fd.append("fecha_vigencia_fin", document.getElementById("vigenciaFin").value);

        try {
            const j = await fetchJson(H_REV, { method: "POST", body: fd });
            await recargarTodo();
            Swal.fire("Listo", j.mensaje, "success");
        } catch (e) {
            Swal.fire("No se guardó", e.message, "error");
        }
    });

    document.getElementById("btnNuevaDisposicion").addEventListener("click", function () {
        if (!resumen || !resumen.monto_autorizado) {
            Swal.fire("Falta configuración", "Primero guarda el límite autorizado y la vigencia.", "warning");
            return;
        }
        document.getElementById("dispMonto").value = "";
        document.getElementById("dispReferencia").value = "";
        document.getElementById("dispObservaciones").value = "";
        const ahora = new Date();
        ahora.setMinutes(ahora.getMinutes() - ahora.getTimezoneOffset());
        document.getElementById("dispFecha").value = ahora.toISOString().slice(0, 16);
        modal.show();
    });

    document.getElementById("btnAplicarDisposicion").addEventListener("click", async function () {
        const fd = new FormData();
        fd.append("action", "crear_disposicion");
        fd.append("solicitud_id", solicitudId);
        fd.append("monto", document.getElementById("dispMonto").value);
        fd.append("fecha_disposicion", document.getElementById("dispFecha").value);
        fd.append("referencia", document.getElementById("dispReferencia").value);
        fd.append("observaciones", document.getElementById("dispObservaciones").value);

        try {
            const j = await fetchJson(H_REV, { method: "POST", body: fd });
            modal.hide();
            await recargarTodo();
            Swal.fire("Listo", j.mensaje, "success");
        } catch (e) {
            Swal.fire("No se aplicó", e.message, "error");
        }
    });

    document.querySelector("#tablaDisposiciones tbody").addEventListener("click", async function (ev) {
        const btn = ev.target.closest(".btn-reversa");
        if (!btn) return;

        const r = await Swal.fire({
            title: "Reversar disposición",
            text: "La reversa se bloqueará si existen movimientos posteriores.",
            input: "textarea",
            inputLabel: "Motivo",
            inputPlaceholder: "Captura el motivo de la reversa",
            showCancelButton: true,
            confirmButtonText: "Reversar",
            cancelButtonText: "Cancelar",
            inputValidator: function (v) {
                if (!String(v || "").trim()) return "El motivo es obligatorio.";
            }
        });

        if (!r.isConfirmed) return;

        const fd = new FormData();
        fd.append("action", "reversar_disposicion");
        fd.append("disposicion_id", btn.getAttribute("data-id"));
        fd.append("motivo", r.value);

        try {
            const j = await fetchJson(H_REV, { method: "POST", body: fd });
            await recargarTodo();
            Swal.fire("Listo", j.mensaje, "success");
        } catch (e) {
            Swal.fire("No se reversó", e.message, "error");
        }
    });

    recargarTodo().catch(function () {});
});
</script>
</asp:Content>
