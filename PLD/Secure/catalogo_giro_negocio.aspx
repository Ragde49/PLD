<%@ Page Title="Catálogo Giro de Negocio" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_giro_negocio.aspx.vb" Inherits="PLD.catalogo_giro_negocio" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <div class="card">
        <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
            <span class="fw-bold">Giro de Negocio</span>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalGiro">
                <i class="fa fa-plus"></i> Capturar
            </button>
        </div>
        <table id="tablaGiros" class="table table-sm table-bordered w-100 mb-0">
            <thead class="table-primary">
                <tr>
                    <th>ID</th>
                    <th>Giro del negocio</th>
                    <th>Clasificación</th>
                    <th>Impacto</th>
                    <th>Probabilidad</th>
                    <th>Nivel de riesgo PLD</th>
                    <th>Estatus</th>
                    <th class="text-center">Acciones</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>
</div>

<!-- Modal Alta/Edición -->
<div class="modal fade" id="modalGiro" tabindex="-1" aria-labelledby="modalGiroLabel" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title" id="modalGiroLabel">Giro de Negocio</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <input type="hidden" id="idGiro" />

                <div class="mb-3">
                    <label for="txtDescripcion" class="form-label">Giro del negocio *</label>
                    <input type="text" id="txtDescripcion" class="form-control" maxlength="150">
                </div>

                <div class="mb-3">
                    <label for="ddlClasificacion" class="form-label">Clasificación de riesgo *</label>
                    <select id="ddlClasificacion" class="form-select">
                        <option value="">-- Selecciona --</option>
                        <option value="BAJO">BAJO</option>
                        <option value="MEDIO">MEDIO</option>
                        <option value="ALTO">ALTO</option>
                    </select>
                </div>

                <div class="mb-3">
                    <label for="txtImpacto" class="form-label">Impacto *</label>
                    <input type="text" id="txtImpacto" class="form-control" maxlength="50">
                </div>

                <div class="mb-3">
                    <label for="txtProbabilidad" class="form-label">Probabilidad *</label>
                    <input type="text" id="txtProbabilidad" class="form-control" maxlength="50">
                </div>

                <div class="mb-3">
                    <label for="txtNivelRiesgo" class="form-label">Nivel de riesgo P.L.D</label>
                    <input type="text" id="txtNivelRiesgo" class="form-control" maxlength="50">
                </div>

                <div class="form-check mb-3">
                    <input class="form-check-input" type="checkbox" id="chkActivo" checked>
                    <label class="form-check-label" for="chkActivo">Estatus activo</label>
                </div>
            </div>

            <div class="modal-footer">
                <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-primary btn-sm" id="btnGuardarGiro">
                    <i class="fa fa-save"></i> Guardar
                </button>
            </div>
        </div>
    </div>
</div>

<script>
    function riesgoTextoANumero(valor) {
        switch (valor.toUpperCase()) {
            case "BAJO": return 1;
            case "MEDIO": return 2;
            case "ALTO": return 3;
            default:
                const num = parseFloat(valor.replace(",", "."));
                return isNaN(num) ? 0 : num;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        cargarTabla();
        document.getElementById("btnGuardarGiro").addEventListener("click", guardarGiro);
    });

    function cargarTabla() {
        $('#tablaGiros').DataTable({
            processing: true,
            serverSide: false,
            destroy: true,
            ajax: {
                url: "/handlers/handler_giro_negocio.ashx?action=listar",
                type: "GET",
                dataSrc: "data"
            },
            columns: [
                { data: "id" },
                { data: "descripcion" },
                { data: "clasificacion_riesgo" },
                { data: "impacto" },
                { data: "probabilidad" },
                { data: "nivel_riesgo_pld" },
                {
                    data: "activo",
                    render: function (data) {
                        return data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>';
                    }
                },
                {
                    data: null,
                    className: "text-center",
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                            <button type="button" class="btn btn-sm btn-warning me-1" onclick='editarGiro(${JSON.stringify(row)})'>
                                <i class="fa fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-danger" onclick='eliminarGiro(${row.id})'>
                                <i class="fa fa-trash"></i>
                            </button>
                        `;
                    }
                }
            ],
            pageLength: 25,
            language: {
                url: "https://cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json"
            }
        });
    }

    function guardarGiro() {
        const id = document.getElementById("idGiro").value;
        const descripcion = document.getElementById("txtDescripcion").value.trim();
        const clasificacion = document.getElementById("ddlClasificacion").value;
        const impacto = riesgoTextoANumero(document.getElementById("txtImpacto").value.trim());
        const probabilidad = riesgoTextoANumero(document.getElementById("txtProbabilidad").value.trim());
        const nivel = riesgoTextoANumero(document.getElementById("txtNivelRiesgo").value.trim());
        const activo = document.getElementById("chkActivo").checked;

        if (!descripcion || !clasificacion || impacto === 0 || probabilidad === 0) {
            Swal.fire("Campos requeridos", "Llena todos los campos obligatorios marcados con *", "warning");
            return;
        }

        const datos = {
            accion: id ? "editar" : "guardar",
            id,
            descripcion,
            clasificacion_riesgo: clasificacion,
            impacto,
            probabilidad,
            nivel_riesgo_pld: nivel,
            activo
        };

        fetch("/handlers/handler_giro_negocio.ashx", {
            method: "POST",
            body: JSON.stringify(datos),
            headers: { "Content-Type": "application/json" }
        })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                Swal.fire("Éxito", "Registro guardado correctamente", "success");
                bootstrap.Modal.getInstance(document.getElementById("modalGiro")).hide();
                cargarTabla();
                limpiarModal();
            } else {
                Swal.fire("Error", res.error || "No se pudo guardar el registro", "error");
            }
        });
    }

    function editarGiro(data) {
        document.getElementById("idGiro").value = data.id;
        document.getElementById("txtDescripcion").value = data.descripcion;
        document.getElementById("ddlClasificacion").value = data.clasificacion_riesgo;
        document.getElementById("txtImpacto").value = data.impacto;
        document.getElementById("txtProbabilidad").value = data.probabilidad;
        document.getElementById("txtNivelRiesgo").value = data.nivel_riesgo_pld;
        document.getElementById("chkActivo").checked = data.activo;
        new bootstrap.Modal(document.getElementById("modalGiro")).show();
    }

    function limpiarModal() {
        document.getElementById("idGiro").value = "";
        document.getElementById("txtDescripcion").value = "";
        document.getElementById("ddlClasificacion").value = "";
        document.getElementById("txtImpacto").value = "";
        document.getElementById("txtProbabilidad").value = "";
        document.getElementById("txtNivelRiesgo").value = "";
        document.getElementById("chkActivo").checked = true;
    }

    function eliminarGiro(id) {
        Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará el giro del negocio.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                fetch("/handlers/handler_giro_negocio.ashx", {
                    method: "POST",
                    body: JSON.stringify({ accion: "eliminar", id }),
                    headers: { "Content-Type": "application/json" }
                })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        Swal.fire("Eliminado", "El registro ha sido eliminado correctamente.", "success");
                        cargarTabla();
                    } else {
                        Swal.fire("Error", res.error || "No se pudo eliminar el registro", "error");
                    }
                });
            }
        });
    }
</script>
</asp:Content>
