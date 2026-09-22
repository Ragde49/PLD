<%@ Page Title="Listas PLD / PEP" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="listas_pld.aspx.vb" Inherits="PLD.listas_pld" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h4 class="mb-0">Listas PLD / PEP</h4>
        <button type="button" id="btnRefrescar" class="btn btn-outline-primary btn-sm">Refrescar</button>
    </div>

    <div class="alert alert-info">
        Las coincidencias de este módulo son exactas por nombre normalizado, RFC o CURP. Una coincidencia requiere revisión y no confirma por sí sola que se trate de la misma persona.
    </div>

    <div class="card mb-3">
        <div class="card-header fw-semibold">Cargar nueva versión</div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">Lista *</label>
                    <select id="listaId" class="form-select"></select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Fecha recepción</label>
                    <input type="date" id="fechaRecepcion" class="form-control">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Referencia / comunicado</label>
                    <input type="text" id="referenciaFuente" class="form-control" maxlength="250">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Archivo XLSX o CSV *</label>
                    <input type="file" id="archivoLista" class="form-control" accept=".xlsx,.csv">
                </div>
                <div class="col-12">
                    <div class="form-text">La primera hoja debe contener NAME/NOMBRE/NOMBRE COMPLETO. RFC y CURP son opcionales. La carga queda histórica hasta marcarla como vigente.</div>
                </div>
                <div class="col-12 text-end">
                    <button type="button" id="btnImportar" class="btn btn-primary">Cargar versión</button>
                </div>
            </div>
        </div>
    </div>

    <div class="card mb-3">
        <div class="card-header fw-semibold">Versiones cargadas</div>
        <div class="card-body">
            <div class="table-responsive">
                <table id="tablaCargas" class="table table-sm table-striped align-middle w-100">
                    <thead class="table-dark">
                        <tr>
                            <th>Lista</th>
                            <th>Archivo</th>
                            <th>Referencia</th>
                            <th>Recepción</th>
                            <th>Registros</th>
                            <th>Estado</th>
                            <th>Cargado por</th>
                            <th>Fecha</th>
                            <th>Acción</th>
                        </tr>
                    </thead>
                    <tbody id="tbodyCargas"></tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="card mb-3">
        <div class="card-header fw-semibold">Consulta exacta</div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Nombre</label>
                    <input type="text" id="buscarNombre" class="form-control">
                </div>
                <div class="col-md-3">
                    <label class="form-label">RFC</label>
                    <input type="text" id="buscarRfc" class="form-control" maxlength="20">
                </div>
                <div class="col-md-3">
                    <label class="form-label">CURP</label>
                    <input type="text" id="buscarCurp" class="form-control" maxlength="30">
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <button type="button" id="btnBuscar" class="btn btn-success w-100">Consultar</button>
                </div>
                <div class="col-12">
                    <div id="resultadoConsulta" class="form-text"></div>
                </div>
            </div>

            <div class="table-responsive mt-3">
                <table class="table table-sm table-bordered align-middle w-100">
                    <thead class="table-light">
                        <tr>
                            <th>Lista</th>
                            <th>Coincidencia</th>
                            <th>Nombre</th>
                            <th>RFC</th>
                            <th>CURP</th>
                            <th>Archivo</th>
                            <th>Referencia</th>
                        </tr>
                    </thead>
                    <tbody id="tbodyResultados"></tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<script type="text/javascript">
document.addEventListener("DOMContentLoaded", function () {
    const H_ADMIN = "/handlers/handler_listas_pld.ashx";
    const H_QUERY = "/handlers/handler_consulta_listas_pld.ashx";

    function esc(v) {
        const d = document.createElement("div");
        d.textContent = v == null ? "" : String(v);
        return d.innerHTML;
    }

    function fecha(v) {
        if (!v) return "";
        if (typeof v === "string" && v.indexOf("/Date(") === 0) {
            const m = /\/Date\((\d+)\)\//.exec(v);
            if (m) return new Date(parseInt(m[1], 10)).toLocaleString("es-MX");
        }
        const d = new Date(v);
        return isNaN(d.getTime()) ? String(v) : d.toLocaleString("es-MX");
    }

    async function json(url, options) {
        const r = await fetch(url, options || {});
        const j = await r.json();
        if (!r.ok || !j.ok) throw new Error(j.mensaje || ("HTTP " + r.status));
        return j;
    }

    async function cargarCatalogos() {
        const j = await json(H_ADMIN + "?action=catalogos");
        const s = document.getElementById("listaId");
        s.innerHTML = '<option value="">— Seleccionar —</option>';
        (j.data || []).forEach(function (x) {
            const o = document.createElement("option");
            o.value = x.id;
            o.textContent = x.nombre;
            s.appendChild(o);
        });
    }

    async function cargarCargas() {
        const j = await json(H_ADMIN + "?action=cargas");
        const tb = document.getElementById("tbodyCargas");
        tb.innerHTML = "";
        (j.data || []).forEach(function (x) {
            const vigente = x.vigente === true || x.vigente === 1 || x.vigente === "1";
            const tr = document.createElement("tr");
            tr.innerHTML =
                "<td>" + esc(x.lista) + "</td>" +
                "<td>" + esc(x.nombre_archivo) + "</td>" +
                "<td>" + esc(x.referencia_fuente) + "</td>" +
                "<td>" + esc(x.fecha_recepcion || "") + "</td>" +
                "<td>" + esc(x.total_registros) + "</td>" +
                "<td>" + (vigente ? '<span class="badge bg-success">VIGENTE</span>' : '<span class="badge bg-secondary">HISTÓRICA</span>') + "</td>" +
                "<td>" + esc(x.creado_por) + "</td>" +
                "<td>" + esc(fecha(x.fecha_creacion)) + "</td>" +
                "<td>" + (vigente ? "—" : '<button type="button" class="btn btn-sm btn-outline-success btn-vigente" data-id="' + esc(x.id) + '">Marcar vigente</button>') + "</td>";
            tb.appendChild(tr);
        });
    }

    async function importar() {
        const listaId = document.getElementById("listaId").value;
        const file = document.getElementById("archivoLista").files[0];
        if (!listaId || !file) {
            Swal.fire("Listas PLD", "Selecciona la lista y el archivo.", "warning");
            return;
        }

        const fd = new FormData();
        fd.append("action", "importar");
        fd.append("lista_id", listaId);
        fd.append("fecha_recepcion", document.getElementById("fechaRecepcion").value || "");
        fd.append("referencia_fuente", document.getElementById("referenciaFuente").value || "");
        fd.append("archivo", file);

        try {
            const j = await json(H_ADMIN, { method: "POST", body: fd });
            Swal.fire("Listas PLD", j.mensaje + " Registros: " + j.total_registros, "success");
            document.getElementById("archivoLista").value = "";
            await cargarCargas();
        } catch (e) {
            Swal.fire("Listas PLD", e.message, "error");
        }
    }

    async function activar(id) {
        const r = await Swal.fire({
            title: "Marcar versión vigente",
            text: "La versión vigente anterior de esta misma lista quedará histórica.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, activar",
            cancelButtonText: "Cancelar"
        });
        if (!r.isConfirmed) return;

        const fd = new FormData();
        fd.append("action", "activar");
        fd.append("carga_id", id);
        try {
            const j = await json(H_ADMIN, { method: "POST", body: fd });
            Swal.fire("Listas PLD", j.mensaje, "success");
            await cargarCargas();
        } catch (e) {
            Swal.fire("Listas PLD", e.message, "error");
        }
    }

    async function buscar() {
        const fd = new FormData();
        fd.append("action", "buscar");
        fd.append("nombre", document.getElementById("buscarNombre").value.trim());
        fd.append("rfc", document.getElementById("buscarRfc").value.trim());
        fd.append("curp", document.getElementById("buscarCurp").value.trim());

        try {
            const j = await json(H_QUERY, { method: "POST", body: fd });
            document.getElementById("resultadoConsulta").textContent =
                "Consulta #" + j.consulta_id + " · Coincidencias: " + j.coincidencias + ". " + j.mensaje;
            const tb = document.getElementById("tbodyResultados");
            tb.innerHTML = "";
            (j.data || []).forEach(function (x) {
                const tr = document.createElement("tr");
                tr.innerHTML =
                    "<td>" + esc(x.lista) + "</td>" +
                    "<td><span class='badge bg-warning text-dark'>" + esc(x.tipo_coincidencia) + "</span></td>" +
                    "<td>" + esc(x.nombre) + "</td>" +
                    "<td>" + esc(x.rfc) + "</td>" +
                    "<td>" + esc(x.curp) + "</td>" +
                    "<td>" + esc(x.nombre_archivo) + "</td>" +
                    "<td>" + esc(x.referencia_fuente) + "</td>";
                tb.appendChild(tr);
            });
        } catch (e) {
            Swal.fire("Listas PLD", e.message, "error");
        }
    }

    document.getElementById("btnImportar").addEventListener("click", importar);
    document.getElementById("btnBuscar").addEventListener("click", buscar);
    document.getElementById("btnRefrescar").addEventListener("click", cargarCargas);
    document.getElementById("tablaCargas").addEventListener("click", function (e) {
        const b = e.target.closest(".btn-vigente");
        if (b) activar(b.getAttribute("data-id"));
    });

    Promise.all([cargarCatalogos(), cargarCargas()]).catch(function (e) {
        Swal.fire("Listas PLD", e.message, "error");
    });
});
</script>
</asp:Content>
