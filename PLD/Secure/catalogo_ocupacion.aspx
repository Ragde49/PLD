<%@ Page Title="Catálogo de Ocupaciones" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_ocupacion.aspx.vb" Inherits="PLD.catalogo_ocupacion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4>Consulta de catálogo</h4>

        <div class="card border-primary">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <span>Ocupación</span>
                <button type="button" class="btn btn-success btn-sm" id="btnCapturar">
                    <i class="fa fa-plus"></i> Capturar
                </button>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table id="tablaOcupaciones" class="table table-sm table-bordered table-hover w-100">
                        <thead class="table-primary">
                            <tr>
                                <th>ID</th>
                                <th>Descripción</th>
                                <th>Clasificación</th>
                                <th>Impacto</th>
                                <th>Ocurrencia</th>
                                <th>Nivel de riesgo P.L.D</th>
                                <th>Estatus</th>
                                <th>Mitigantes</th>
                                <th class="text-center" style="width: 80px;">Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="modalCaptura" tabindex="-1" aria-labelledby="modalCapturaLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalCapturaLabel">Capturar Ocupación</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="ocupacionId">

                    <div class="mb-3">
                        <label for="descripcion" class="form-label">Ocupación</label>
                        <input type="text" class="form-control" id="descripcion" maxlength="150">
                    </div>

                    <div class="mb-3">
                        <label for="clasificacion_riesgo" class="form-label">Clasificación de riesgo</label>
                        <select id="clasificacion_riesgo" class="form-select">
                            <option value="">Selecciona...</option>
                            <option value="BAJO">BAJO</option>
                            <option value="MEDIO">MEDIO</option>
                            <option value="ALTO">ALTO</option>
                        </select>
                    </div>

                    <div class="mb-3">
                        <label for="impacto" class="form-label">Impacto</label>
                        <select id="impacto" class="form-select">
                            <option value="">Selecciona...</option>
                            <option value="BAJO">BAJO</option>
                            <option value="MEDIO">MEDIO</option>
                            <option value="ALTO">ALTO</option>
                        </select>
                    </div>

                    <div class="mb-3">
                        <label for="probabilidad" class="form-label">Probabilidad</label>
                        <select id="probabilidad" class="form-select">
                            <option value="">Selecciona...</option>
                            <option value="BAJO">BAJO</option>
                            <option value="MEDIO">MEDIO</option>
                            <option value="ALTO">ALTO</option>
                        </select>
                    </div>

                    <div class="mb-3">
                        <label for="nivel_riesgo_pld" class="form-label">Nivel de riesgo P.L.D</label>
                        <select id="nivel_riesgo_pld" class="form-select">
                            <option value="">Selecciona...</option>
                            <option value="BAJO">BAJO</option>
                            <option value="MEDIO">MEDIO</option>
                            <option value="ALTO">ALTO</option>
                        </select>
                    </div>

                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" id="activo" checked>
                        <label class="form-check-label" for="activo">Activo</label>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
                    <button type="button" class="btn btn-success btn-sm" id="btnGuardar">Guardar</button>
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
            default: return 0;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        let tabla;

        function cargarTabla() {
            fetch('/handlers/handler_ocupacion.ashx?action=listar')
                .then(res => res.json())
                .then(data => {
                    if (tabla) tabla.destroy();

                    tabla = new DataTable('#tablaOcupaciones', {
                        data: data.data,
                        pageLength: 25,
                        searching: false,
                        lengthChange: false,
                        columns: [
                            { data: 'id' },
                            { data: 'descripcion' },
                            { data: 'clasificacion_riesgo' },
                            { data: 'impacto' },
                            { data: 'probabilidad' },
                            { data: 'nivel_riesgo_pld' },
                            {
                                data: 'activo',
                                render: function (val) {
                                    return val ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>';
                                }
                            },
                            {
                                data: null,
                                render: () => `<span class="badge bg-warning text-dark">0</span>`
                            },
                            {
                                data: null,
                                className: "text-center",
                                render: function (row) {
                                    return `
                                        <button type="button" class="btn btn-sm btn-warning btnEditar" data-id="${row.id}" title="Editar">
                                            <i class="fa fa-edit"></i>
                                        </button>
                                        <button type="button" class="btn btn-sm btn-danger btnEliminar" data-id="${row.id}" title="Eliminar">
                                            <i class="fa fa-trash"></i>
                                        </button>
                                    `;
                                }
                            }
                        ],
                        language: {
                            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
                        }
                    });
                });
        }

        function limpiarModal() {
            document.getElementById("ocupacionId").value = "";
            document.getElementById("descripcion").value = "";
            document.getElementById("clasificacion_riesgo").value = "";
            document.getElementById("impacto").value = "";
            document.getElementById("probabilidad").value = "";
            document.getElementById("nivel_riesgo_pld").value = "";
            document.getElementById("activo").checked = true;
        }

        document.getElementById("btnCapturar").addEventListener("click", () => {
            limpiarModal();
            new bootstrap.Modal(document.getElementById('modalCaptura')).show();
        });

        document.getElementById("btnGuardar").addEventListener("click", () => {
            const payload = {
                action: document.getElementById("ocupacionId").value ? "editar" : "guardar",
                id: document.getElementById("ocupacionId").value,
                descripcion: document.getElementById("descripcion").value.trim(),
                clasificacion_riesgo: document.getElementById("clasificacion_riesgo").value,
                impacto: riesgoTextoANumero(document.getElementById("impacto").value),
                probabilidad: riesgoTextoANumero(document.getElementById("probabilidad").value),
                nivel_riesgo_pld: riesgoTextoANumero(document.getElementById("nivel_riesgo_pld").value),
                activo: document.getElementById("activo").checked
            };

            if (!payload.descripcion || !payload.clasificacion_riesgo || !payload.impacto || !payload.probabilidad) {
                Swal.fire("Campos requeridos", "Completa todos los campos obligatorios", "warning");
                return;
            }

            fetch('/handlers/handler_ocupacion.ashx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(r => r.json())
            .then(res => {
                if (res.success) {
                    Swal.fire("Correcto", "Registro guardado correctamente", "success");
                    bootstrap.Modal.getInstance(document.getElementById('modalCaptura')).hide();
                    cargarTabla();
                } else {
                    Swal.fire("Error", res.error || "Ocurrió un error", "error");
                }
            });
        });

        document.addEventListener("click", function (e) {
            if (e.target.closest(".btnEditar")) {
                const id = e.target.closest(".btnEditar").dataset.id;
                fetch(`/handlers/handler_ocupacion.ashx?action=listar`)
                    .then(r => r.json())
                    .then(data => {
                        const ocupacion = data.data.find(x => x.id == id);
                        document.getElementById("ocupacionId").value = ocupacion.id;
                        document.getElementById("descripcion").value = ocupacion.descripcion;
                        document.getElementById("clasificacion_riesgo").value = ocupacion.clasificacion_riesgo;
                        document.getElementById("impacto").value = impactoTexto(ocupacion.impacto);
                        document.getElementById("probabilidad").value = impactoTexto(ocupacion.probabilidad);
                        document.getElementById("nivel_riesgo_pld").value = impactoTexto(ocupacion.nivel_riesgo_pld);
                        document.getElementById("activo").checked = ocupacion.activo;
                        new bootstrap.Modal(document.getElementById('modalCaptura')).show();
                    });
            }

            if (e.target.closest(".btnEliminar")) {
                const id = e.target.closest(".btnEliminar").dataset.id;
                Swal.fire({
                    title: '¿Eliminar ocupación?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, eliminar',
                    cancelButtonText: 'Cancelar'
                }).then(result => {
                    if (result.isConfirmed) {
                        fetch('/handlers/handler_ocupacion.ashx', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ action: "eliminar", id })
                        })
                        .then(r => r.json())
                        .then(res => {
                            if (res.success) {
                                Swal.fire("Eliminado", "Ocupación eliminada correctamente", "success");
                                cargarTabla();
                            } else {
                                Swal.fire("Error", res.error || "No se pudo eliminar", "error");
                            }
                        });
                    }
                });
            }
        });

        function impactoTexto(valor) {
            valor = parseInt(valor);
            switch (valor) {
                case 1: return "BAJO";
                case 2: return "MEDIO";
                case 3: return "ALTO";
                default: return "";
            }
        }

        cargarTabla();
    });
</script>

</asp:Content>
