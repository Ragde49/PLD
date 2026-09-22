<%@ Page Title="Catálogo Nacionalidades" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_nacionalidades.aspx.vb" Inherits="PLD.catalogo_nacionalidades" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4 class="mb-3">Consulta de catálogo</h4>

        <div class="card border-primary">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <span><i class="fas fa-flag"></i> Nacionalidades</span>
                <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalCaptura">
                    <i class="fas fa-plus"></i> Capturar
                </button>
            </div>
            <div class="card-body">
                <table id="tablaNacionalidades" class="table table-bordered table-sm table-striped w-100">
                    <thead class="table-light">
                        <tr>
                            <th>ID</th>
                            <th>Nacionalidad</th>
                            <th>País</th>
                            <th>Impacto</th>
                            <th>Ocurrencia</th>
                            <th>Nivel Riesgo</th>
                            <th>Estatus</th>
                            <th>Mitigantes</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Modal Captura -->
    <div class="modal fade" id="modalCaptura" tabindex="-1" aria-labelledby="modalCapturaLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-primary">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">Capturar nacionalidad</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <form id="formNacionalidad">
                        <input type="hidden" id="idNacionalidad">
                        <div class="mb-2">
                            <label class="form-label">País*</label>
                            <input type="text" id="pais" class="form-control form-control-sm" required>
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Nacionalidad*</label>
                            <input type="text" id="nacionalidad" class="form-control form-control-sm" required>
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Clave</label>
                            <input type="text" id="clave" class="form-control form-control-sm">
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Impacto*</label>
                            <input type="number" id="impacto" class="form-control form-control-sm" required>
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Probabilidad*</label>
                            <input type="number" id="probabilidad" class="form-control form-control-sm" required>
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Nivel de riesgo P.L.D.*</label>
                            <input type="number" step="0.01" id="nivel_riesgo_pld" class="form-control form-control-sm" required>
                        </div>
                        <div class="form-check mb-2">
                            <input class="form-check-input" type="checkbox" id="activo" checked>
                            <label class="form-check-label">Estatus activo</label>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary btn-sm" id="btnGuardar">
                        <i class="fas fa-save"></i> Guardar
                    </button>
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- JS embebido -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            cargarTabla();

            document.getElementById("btnGuardar").addEventListener("click", guardarNacionalidad);
        });

        function cargarTabla() {
            fetch("/handlers/handler_nacionalidades.ashx?op=select")
                .then(res => res.json())
                .then(data => {
                    $('#tablaNacionalidades').DataTable({
                        destroy: true,
                        data: data.data,
                        columns: [
                            { data: "id" },
                            { data: "nacionalidad" },
                            {
                                data: "pais",
                                render: d => `<span class="badge bg-info text-white">${d}</span>`
                            },
                            { data: "impacto" },
                            { data: "probabilidad" },
                            {
                                data: "nivel_riesgo_pld",
                                render: d => parseFloat(d).toFixed(2)
                            },
                            {
                                data: "activo",
                                render: d => d ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-danger">Inactivo</span>'
                            },
                            {
                                data: null,
                                orderable: false,
                                render: () => `<i class="fas fa-exclamation-circle text-warning"></i>`
                            },
                            {
                                data: null,
                                orderable: false,
                                render: function (data, type, row) {
                                    return `
                                        <button type="button" class="btn btn-outline-primary btn-sm me-1" onclick='editar(${JSON.stringify(row)})'>
                                            <i class="fas fa-edit"></i>
                                        </button>
                                        <button type="button" class="btn btn-outline-danger btn-sm" onclick='eliminar(${row.id})'>
                                            <i class="fas fa-trash-alt"></i>
                                        </button>
                                    `;
                                }
                            }
                        ],
                        language: {
                            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
                        },
                        pageLength: 25
                    });
                })
                .catch(err => {
                    console.error("🧨 ERROR EN FETCH:", err);
                    Swal.fire("Error", "Error al cargar los datos.", "error");
                });
        }

        function guardarNacionalidad() {
            const id = document.getElementById("idNacionalidad").value;
            const data = {
                id: id || 0,
                pais: document.getElementById("pais").value.trim(),
                nacionalidad: document.getElementById("nacionalidad").value.trim(),
                clave: document.getElementById("clave").value.trim(),
                impacto: document.getElementById("impacto").value.trim(),
                probabilidad: document.getElementById("probabilidad").value.trim(),
                nivel_riesgo_pld: document.getElementById("nivel_riesgo_pld").value.trim(),
                activo: document.getElementById("activo").checked ? 1 : 0,
                op: id ? "update" : "insert"
            };

            fetch("/handlers/handler_nacionalidades.ashx", {
                method: "POST",
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(data)
            })
                .then(res => res.json())
                .then(response => {

                    if (response.success) {
                        Swal.fire("Éxito", "Registro guardado correctamente.", "success");

                        const form = document.getElementById("formNacionalidad");
                        if (form) {
                            form.reset();
                        } else {
                            console.warn("🧨 El formulario 'formNacionalidad' no existe.");
                        }

                        document.getElementById("idNacionalidad").value = "";
                        bootstrap.Modal.getInstance(document.getElementById("modalCaptura")).hide();
                        cargarTabla();
                    } else {
                        Swal.fire("Error", response.error || "No se pudo guardar", "error");
                    }
                })
                .catch(err => {
                    console.error("🧨 ERROR GUARDANDO:", err);
                    debugger; // <- aquí también, por si es un fallo de red
                    Swal.fire("Error", "Error de red o del servidor.", "error");
                });
        }



        function editar(row) {
            document.getElementById("idNacionalidad").value = row.id;
            document.getElementById("pais").value = row.pais;
            document.getElementById("nacionalidad").value = row.nacionalidad;
            document.getElementById("clave").value = row.clave || "";
            document.getElementById("impacto").value = row.impacto;
            document.getElementById("probabilidad").value = row.probabilidad;
            document.getElementById("nivel_riesgo_pld").value = row.nivel_riesgo_pld;
            document.getElementById("activo").checked = row.activo === true;

            new bootstrap.Modal(document.getElementById("modalCaptura")).show();
        }

        function eliminar(id) {
            Swal.fire({
                title: "¿Eliminar?",
                text: "El registro será desactivado.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar"
            }).then(result => {
                if (result.isConfirmed) {
                    fetch("/handlers/handler_nacionalidades.ashx", {
                        method: "POST",
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: new URLSearchParams({ op: "delete", id })
                    })
                        .then(res => res.json())
                        .then(response => {
                            if (response.success) {
                                Swal.fire("Eliminado", "El registro fue desactivado.", "success");
                                cargarTabla();
                            } else {
                                Swal.fire("Error", response.error || "No se pudo eliminar", "error");
                            }
                        })
                        .catch(err => {
                            console.error("🧨 ERROR:", err);
                            Swal.fire("Error", "Error al intentar eliminar", "error");
                        });
                }
            });
        }
    </script>
</asp:Content>
