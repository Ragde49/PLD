<%@ Page Title="Catálogo Aplicación de Pago" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4>Catálogo Aplicación de Pago</h4>

        <div class="table-responsive">
            <table id="tablaAplicacionPago" class="table table-sm table-bordered w-100">
                <thead>
                    <tr>
                        <th colspan="10" style="background-color: #0d6efd; color: white; padding: 8px 12px;">
                            <div class="d-flex justify-content-between align-items-center">
                                <span class="fw-bold">Aplicaciones de Pago</span>
                                <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalCaptura">
                                    <i class="fa fa-plus"></i> Capturar
                                </button>
                            </div>
                        </th>
                    </tr>
                    <tr>
                        <th>ID</th>
                        <th>Descripción</th>
                        <th>Subtipo</th>
                        <th>Tipo Crédito</th>
                        <th>Tipo Régimen</th>
                        <th>Nivel Riesgo PLD</th>
                        <th>Impacto</th>
                        <th>Probabilidad</th>
                        <th>Activo</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="modalCaptura" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">Captura Aplicación de Pago</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label>Descripción:</label>
                            <input type="text" class="form-control" id="txtDescripcion">
                        </div>
                        <div class="col-md-6">
                            <label>Subtipo:</label>
                            <input type="text" class="form-control" id="txtSubtipo">
                        </div>
                        <div class="col-md-6">
                            <label>Tipo Crédito:</label>
                            <select id="ddlTipoCredito" class="form-select"></select>
                        </div>
                        <div class="col-md-6">
                            <label>Tipo Régimen:</label>
                            <select id="ddlTipoRegimen" class="form-select">
                                <option value="Persona Física">Persona Física</option>
                                <option value="Persona Moral">Persona Moral</option>
                                <option value="Persona Física con Actividad Empresarial">Persona Física con Actividad Empresarial</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label>Impacto:</label>
                            <select id="ddlImpacto" class="form-select">
                                <option value="BAJO">BAJO</option>
                                <option value="MEDIO">MEDIO</option>
                                <option value="ALTO">ALTO</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label>Probabilidad:</label>
                            <select id="ddlProbabilidad" class="form-select">
                                <option value="BAJO">BAJO</option>
                                <option value="MEDIO">MEDIO</option>
                                <option value="ALTO">ALTO</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label>Nivel Riesgo PLD:</label>
                            <input type="number" class="form-control" step="0.01" id="txtNivelRiesgoPLD">
                        </div>
                        <div class="col-md-4">
                            <div class="form-check mt-4">
                                <input class="form-check-input" type="checkbox" id="chkActivo" checked>
                                <label class="form-check-label">Activo</label>
                            </div>
                        </div>
                    </div>
                    <input type="hidden" id="hdnIdRegistro" />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" id="btnGuardar" type="button">Guardar</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            cargarTipoCredito();

            function riesgoTextoANumero(valor) {
                switch (valor.toUpperCase()) {
                    case "BAJO": return 1;
                    case "MEDIO": return 2;
                    case "ALTO": return 3;
                    default: return 0;
                }
            }

            function cargarTipoCredito() {
                fetch("/handlers/handler_tipo_credito.ashx")
                    .then(r => r.json())
                    .then(data => {
                        const ddl = document.getElementById("ddlTipoCredito");
                        ddl.innerHTML = "";
                        data.forEach(item => {
                            ddl.innerHTML += `<option value="${item.descripcion}">${item.descripcion}</option>`;
                        });
                    });
            }

            function cargarDatos() {
                fetch("/handlers/handler_aplicacion_pago.ashx?accion=consultar")
                    .then(r => r.json())
                    .then(data => {
                        const tbody = document.querySelector("#tablaAplicacionPago tbody");
                        tbody.innerHTML = "";
                        data.data.forEach(row => {
                            tbody.innerHTML += `
                                <tr>
                                    <td>${row.id}</td>
                                    <td>${row.descripcion}</td>
                                    <td>${row.subtipo}</td>
                                    <td>${row.tipo_credito}</td>
                                    <td>${row.tipo_regimen}</td>
                                    <td>${row.nivel_riesgo_pld}</td>
                                    <td>${row.impacto}</td>
                                    <td>${row.probabilidad}</td>
                                    <td>${row.activo ? "Sí" : "No"}</td>
                                    <td>
                                        <button type="button" class="btn btn-sm btn-primary" onclick='editar(${JSON.stringify(row)})'>Editar</button>
                                    </td>
                                </tr>`;
                        });
                    });
            }

            function limpiarModal() {
                document.getElementById("txtDescripcion").value = "";
                document.getElementById("txtSubtipo").value = "";
                document.getElementById("txtNivelRiesgoPLD").value = "";
                document.getElementById("ddlImpacto").value = "BAJO";
                document.getElementById("ddlProbabilidad").value = "BAJO";
                document.getElementById("chkActivo").checked = true;
                document.getElementById("hdnIdRegistro").value = "";
            }

            document.querySelector('[data-bs-target="#modalCaptura"]').addEventListener("click", limpiarModal);

            document.getElementById("btnGuardar").addEventListener("click", function () {
                const payload = {
                    id: document.getElementById("hdnIdRegistro").value || 0,
                    descripcion: document.getElementById("txtDescripcion").value.trim(),
                    subtipo: document.getElementById("txtSubtipo").value.trim(),
                    tipo_credito: document.getElementById("ddlTipoCredito").value,
                    tipo_regimen: document.getElementById("ddlTipoRegimen").value,
                    impacto: riesgoTextoANumero(document.getElementById("ddlImpacto").value),
                    probabilidad: riesgoTextoANumero(document.getElementById("ddlProbabilidad").value),
                    nivel_riesgo_pld: parseFloat(document.getElementById("txtNivelRiesgoPLD").value) || 0,
                    activo: document.getElementById("chkActivo").checked
                };

                const accion = payload.id == 0 ? "guardar" : "editar";

                fetch(`/handlers/handler_aplicacion_pago.ashx?accion=${accion}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                })
                .then(r => r.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire("Éxito", "Registro guardado correctamente", "success");
                        cargarDatos();
                        bootstrap.Modal.getInstance(document.getElementById("modalCaptura")).hide();
                    } else {
                        Swal.fire("Error", data.mensaje || "Ocurrió un error", "error");
                    }
                });
            });

            window.editar = function (data) {
                document.getElementById("txtDescripcion").value = data.descripcion;
                document.getElementById("txtSubtipo").value = data.subtipo;
                document.getElementById("ddlTipoCredito").value = data.tipo_credito;
                document.getElementById("ddlTipoRegimen").value = data.tipo_regimen;
                document.getElementById("ddlImpacto").value = getTextoNivel(data.impacto);
                document.getElementById("ddlProbabilidad").value = getTextoNivel(data.probabilidad);
                document.getElementById("txtNivelRiesgoPLD").value = data.nivel_riesgo_pld;
                document.getElementById("chkActivo").checked = data.activo;
                document.getElementById("hdnIdRegistro").value = data.id;
                new bootstrap.Modal(document.getElementById("modalCaptura")).show();
            };

            function getTextoNivel(num) {
                switch (parseInt(num)) {
                    case 1: return "BAJO";
                    case 2: return "MEDIO";
                    case 3: return "ALTO";
                    default: return "BAJO";
                }
            }

            cargarDatos();
        });
    </script>
</asp:Content>
