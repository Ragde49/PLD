<%@ Page Title="Catálogo de Estados" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_estados.aspx.vb" Inherits="PLD.catalogo_estados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-3">
        <!-- Cabecera tipo barra azul con título y botón Capturar a la derecha -->
        <div class="card">
            <div class="card-header bg-primary text-white d-flex align-items-center justify-content-between">
                <h5 class="mb-0"><i class="fas fa-list"></i> Créditos</h5>
                <div>
                    <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
                        <i class="fas fa-plus"></i> Capturar
                    </button>
                </div>
            </div>
            <div class="card-body">
                <!-- Tabla responsive - DataTable controlará length/search -->
                <div class="table-responsive">
                    <table id="tblEstados" class="table table-sm table-hover table-striped w-100">
                        <thead class="table-light">
                            <tr>
                                <th>ID</th>
                                <th>Descripción</th>
                                <th>Impacto</th>
                                <th>Ocurrencia</th>
                                <th>Nivel Riesgo P.L.D</th>
                                <th>Estatus</th>
                                <th>Mitigantes</th>
                                <th style="width:120px;">Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>

                <!-- Pie: info de paginación la controla DataTables -->
            </div>
        </div>

        <!-- Modal Captura (mantengo estructura previa) -->
        <div class="modal fade" id="modalCaptura" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Captura / Edición - Estado</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form id="formEstado" onsubmit="return false;">
                            <input type="hidden" id="txtId" />
                            <div class="mb-3">
                                <label class="form-label">Estado*</label>
                                <input type="text" id="txtDescripcion" class="form-control" />
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Clave</label>
                                <input type="text" id="txtClave" class="form-control" />
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Clave Estado</label>
                                <input type="text" id="txtClaveEstado" class="form-control" />
                            </div>

                            <div class="row">
                                <div class="col-md-4 mb-3">
                                    <label class="form-label">Impacto*</label>
                                    <input type="number" id="txtImpacto" class="form-control" min="0" step="1" />
                                </div>
                                <div class="col-md-4 mb-3">
                                    <label class="form-label">Probabilidad* (0-100)</label>
                                    <input type="number" id="txtProbabilidad" class="form-control" min="0" max="100" step="1" />
                                </div>
                                <div class="col-md-4 mb-3">
                                    <label class="form-label">Nivel de riesgo P.L.D*</label>
                                    <input type="number" id="txtNivelRiesgo" class="form-control" min="0" step="0.01" />
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" id="btnGuardar" class="btn btn-primary">
                            <i class="fas fa-save"></i> Guardar información
                        </button>
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times"></i> Cancelar
                        </button>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const URL_HANDLER = "/handlers/handler_catalogo_estados.ashx";
            let table = null;
            const modalEl = new bootstrap.Modal(document.getElementById('modalCaptura'));

            function mostrarMensaje(tipo, titulo, texto) {
                swal.fire(titulo, texto, tipo);
            }

            function cargarTabla() {
                fetch(URL_HANDLER + "?op=consultar")
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp.success) { mostrarMensaje('error', 'Error', 'Error en la respuesta del servidor'); return; }
                        const data = resp.data || [];

                        // Si ya existe tabla, refresca datos
                        if (table) {
                            table.clear();
                            table.rows.add(data);
                            table.draw();
                            return;
                        }

                        // Inicializa DataTable con DOM personalizado: length a la izquierda, search a la derecha
                        table = $('#tblEstados').DataTable({
                            data: data,
                            columns: [
                                { data: 'id' },
                                { data: 'descripcion' },
                                { data: 'impacto' },
                                { data: 'probabilidad', render: function (d) { return (d || 0) + '%' } },
                                { data: 'nivel_riesgo_pld', render: function (d) { return (d || 0).toFixed(2) } },
                                { data: 'estatus', render: function (d) { return d ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'; } },
                                { data: null, orderable: false, render: function (row) { return '<span class="badge bg-info text-dark">0</span>'; } }, // Mitigantes (placeholder)
                                {
                                    data: null,
                                    orderable: false,
                                    render: function (row) {
                                        return `
                                            <button type="button" class="btn btn-sm btn-outline-primary btnEditar me-1" data-id="${row.id}" title="Editar"><i class="fas fa-edit"></i></button>
                                            <button type="button" class="btn btn-sm btn-outline-danger btnEliminar" data-id="${row.id}" title="Eliminar"><i class="fas fa-trash"></i></button>
                                        `;
                                    }
                                }
                            ],
                            pageLength: 25,
                            responsive: true,
                            order: [[1, 'asc']],
                            lengthChange: true,
                            // Custom DOM: length (l) left, filter (f) right, table (t), info (i) and pagination (p)
                            dom:
                                '<"row mb-2"<"col-sm-6"l><"col-sm-6 text-end"f>>' +
                                'rt' +
                                '<"row mt-2"<"col-sm-6"i><"col-sm-6 text-end"p>>',
                            language: {
                                // Si tienes archivo spanish.json, puedes apuntarlo aquí
                                // url: '/scripts/datatables/spanish.json'
                                search: "Search:",
                                lengthMenu: "Show _MENU_ entries",
                                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                                paginate: {
                                    previous: "Previous",
                                    next: "Next"
                                }
                            }
                        });

                        // Delegated events
                        $('#tblEstados tbody').on('click', '.btnEditar', function () {
                            const id = $(this).data('id');
                            editarRegistro(id);
                        });
                        $('#tblEstados tbody').on('click', '.btnEliminar', function () {
                            const id = $(this).data('id');
                            eliminarRegistro(id);
                        });
                    })
                    .catch(err => {
                        console.error(err);
                        mostrarMensaje('error', 'Error', 'Error en la respuesta del servidor');
                    });
            }

            function limpiarFormulario() {
                document.getElementById('txtId').value = '';
                document.getElementById('txtDescripcion').value = '';
                document.getElementById('txtClave').value = '';
                document.getElementById('txtClaveEstado').value = '';
                document.getElementById('txtImpacto').value = '';
                document.getElementById('txtProbabilidad').value = '';
                document.getElementById('txtNivelRiesgo').value = '';
            }

            document.getElementById('btnCapturar').addEventListener('click', function () {
                limpiarFormulario();
                modalEl.show();
            });

            document.getElementById('btnGuardar').addEventListener('click', function () {
                const id = document.getElementById('txtId').value;
                const descripcion = document.getElementById('txtDescripcion').value.trim();
                const clave = document.getElementById('txtClave') ? document.getElementById('txtClave').value.trim() : '';
                const clave_estado = document.getElementById('txtClaveEstado') ? document.getElementById('txtClaveEstado').value.trim() : '';
                const impacto = parseInt(document.getElementById('txtImpacto').value || 0, 10);
                const probabilidad = parseInt(document.getElementById('txtProbabilidad').value || 0, 10);
                const nivel_riesgo_pld = parseFloat(document.getElementById('txtNivelRiesgo').value || 0);

                if (!descripcion) { mostrarMensaje('warning', 'Validación', 'Descripción es requerida'); return; }
                if (isNaN(impacto)) { mostrarMensaje('warning', 'Validación', 'Impacto debe ser numérico'); return; }
                if (isNaN(probabilidad)) { mostrarMensaje('warning', 'Validación', 'Probabilidad debe ser numérico'); return; }
                if (isNaN(nivel_riesgo_pld)) { mostrarMensaje('warning', 'Validación', 'Nivel de riesgo P.L.D debe ser numérico'); return; }

                const form = new URLSearchParams();
                form.append('descripcion', descripcion);
                form.append('clave', clave);
                form.append('clave_estado', clave_estado);
                form.append('impacto', impacto);
                form.append('probabilidad', probabilidad);
                form.append('nivel_riesgo_pld', nivel_riesgo_pld);

                let op = 'guardar';
                if (id && parseInt(id) > 0) {
                    op = 'actualizar';
                    form.append('id', id);
                }

                fetch(`${URL_HANDLER}?op=${op}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: form.toString()
                })
                    .then(r => r.json())
                    .then(resp => {
                        if (resp.success) {
                            modalEl.hide();
                            cargarTabla();
                            mostrarMensaje('success', 'Éxito', 'Registro guardado correctamente');
                        } else {
                            mostrarMensaje('error', 'Error', resp.message || 'Error al guardar');
                        }
                    })
                    .catch(err => {
                        console.error(err);
                        mostrarMensaje('error', 'Error', 'Error al guardar registro');
                    });
            });

            function editarRegistro(id) {
                fetch(`${URL_HANDLER}?op=obtener&id=${id}`)
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp.success) { mostrarMensaje('error', 'Error', 'No se pudo obtener el registro'); return; }
                        const d = resp.data;
                        document.getElementById('txtId').value = d.id;
                        document.getElementById('txtDescripcion').value = d.descripcion;
                        if (document.getElementById('txtClave')) document.getElementById('txtClave').value = d.clave || '';
                        if (document.getElementById('txtClaveEstado')) document.getElementById('txtClaveEstado').value = d.clave_estado || '';
                        document.getElementById('txtImpacto').value = d.impacto || 0;
                        document.getElementById('txtProbabilidad').value = d.probabilidad || 0;
                        document.getElementById('txtNivelRiesgo').value = d.nivel_riesgo_pld || 0;
                        modalEl.show();
                    }).catch(err => {
                        console.error(err);
                        mostrarMensaje('error', 'Error', 'Error al obtener registro');
                    });
            }

            function eliminarRegistro(id) {
                swal.fire({
                    title: 'Confirmar',
                    text: '¿Eliminar el registro?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, eliminar',
                    cancelButtonText: 'Cancelar'
                }).then(result => {
                    if (result.isConfirmed) {
                        fetch(`${URL_HANDLER}?op=eliminar&id=${id}`, { method: 'POST' })
                            .then(r => r.json())
                            .then(resp => {
                                if (resp.success) {
                                    cargarTabla();
                                    mostrarMensaje('success', 'Eliminado', 'Registro eliminado');
                                } else {
                                    mostrarMensaje('error', 'Error', resp.message || 'No se pudo eliminar');
                                }
                            }).catch(err => {
                                console.error(err);
                                mostrarMensaje('error', 'Error', 'Error al eliminar');
                            });
                    }
                });
            }

            // Carga inicial
            cargarTabla();
        });
    </script>
</asp:Content>
