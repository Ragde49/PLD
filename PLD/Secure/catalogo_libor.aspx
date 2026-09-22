<%@ Page Title="Catálogo Tasa LIBOR" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_libor.aspx.vb" Inherits="PLD.catalogo_libor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Ajustes visuales para parecerse al formato entregado */
        .card-header.libor-header {
            background-color: #0d6efd; /* azul */
            color: #fff;
        }
        .badge-estado { font-size: 0.8rem; padding: 0.35rem 0.55rem; }
        .mitigantes-badge { min-width: 26px; display: inline-block; text-align: center; }
        .action-btn { width: 36px; height: 32px; padding: 4px 6px; }
        table.dataTable td, table.dataTable th { vertical-align: middle; }
    </style>

    <div class="container-fluid mt-4">
        <h4 class="mb-3">Consulta de catálogo</h4>

        <div class="card mb-3">
            <div class="card-header libor-header d-flex justify-content-between align-items-center">
                <strong>Créditos</strong>
                <div>
                    <button id="btnCapturar" type="button" class="btn btn-success btn-sm">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
            </div>

            <div class="card-body p-2">
                <div class="table-responsive">
                    <table id="tblLibor" class="table table-striped table-hover table-sm w-100">
                        <thead class="table-light">
                            <tr>
                                <th>ID</th>
                                <th>Descripción</th>
                                <th>Impacto</th>
                                <th>Ocurrencia</th>
                                <th>Nivel Riesgo P.L.D</th>
                                <th>Estatus</th>
                                <th>Mitigantes</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Poblamiento por JS a través del handler -->
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="card-footer text-muted small">
                Los datos se cargan desde el handler: <code>/handlers/handler_catalogo_libor.ashx</code>
            </div>
        </div>
    </div>

    <!-- Modal Captura/Edición (mismo modal adaptado) -->
    <div class="modal fade" id="modalCaptura" tabindex="-1" aria-labelledby="modalCapturaLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="modalCapturaLabel" class="modal-title">Capturar</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <form id="formLibor" onsubmit="return false;">
                        <input type="hidden" id="hidId" value="0" />

                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="descripcion" class="form-label">Descripción*</label>
                                    <input type="text" id="descripcion" class="form-control" placeholder="Descripción" />
                                </div>
                            </div>

                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="impacto" class="form-label">Impacto*</label>
                                    <input type="number" id="impacto" class="form-control" placeholder="50" min="0" />
                                </div>
                            </div>

                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="ocurrencia" class="form-label">Ocurrencia*</label>
                                    <input type="text" id="ocurrencia" class="form-control" placeholder="25%" />
                                </div>
                            </div>
                        </div>

                        <div class="row align-items-end">
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="nivel_riesgo_pld" class="form-label">Nivel Riesgo P.L.D</label>
                                    <input type="text" id="nivel_riesgo_pld" class="form-control" placeholder="12.50" />
                                </div>
                            </div>

                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="fecha" class="form-label">Fecha*</label>
                                    <input type="text" id="fecha" class="form-control" placeholder="YYYY-MM-DD" autocomplete="off" />
                                </div>
                            </div>

                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label d-block">Estatus</label>
                                    <div class="form-check form-switch mt-1">
                                        <input class="form-check-input" type="checkbox" id="estatus" checked />
                                        <label class="form-check-label" for="estatus">Activo</label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="text-end">
                            <button id="btnGuardar" type="button" class="btn btn-primary">
                                <i class="fa fa-save"></i> Guardar información
                            </button>
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                <i class="fa fa-times"></i> Cancelar
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>

    <!-- Script: DataTables + lógica (mantengo handler /fetch/ y modal) -->
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            const handlerUrl = '/handlers/handler_catalogo_libor.ashx';
            let dt = null;
            let modoEdicion = false;

            // Inicializar date input/daterangepicker
            if (typeof $ === 'function' && $.fn.daterangepicker) {
                $('#fecha').daterangepicker({
                    singleDatePicker: true,
                    showDropdowns: true,
                    locale: { format: 'YYYY-MM-DD' }
                });
            } else {
                try { document.getElementById('fecha').type = 'date'; } catch (e) { }
            }

            // Inicializar DataTable con estilo similar al ejemplo
            function initTable() {
                if (!window.jQuery || !$.fn.DataTable) {
                    console.error("DataTables no está disponible.");
                    return;
                }
                dt = $('#tblLibor').DataTable({
                    paging: true,
                    pageLength: 25,
                    searching: true,
                    info: true,
                    autoWidth: false,
                    lengthChange: true,
                    columnDefs: [
                        { targets: 0, width: '4%' },   // ID
                        { targets: 1, width: '30%' },  // Descripción
                        { targets: 2, width: '8%', className: 'text-center' }, // Impacto
                        { targets: 3, width: '8%', className: 'text-center' }, // Ocurrencia
                        { targets: 4, width: '12%', className: 'text-center' },// Nivel Riesgo P.L.D
                        { targets: 5, width: '8%', className: 'text-center' }, // Estatus
                        { targets: 6, width: '8%', className: 'text-center' }, // Mitigantes
                        { targets: 7, orderable: false, searchable: false, width: '10%', className: 'text-center' } // Acciones
                    ],
                    order: [[0, 'asc']],
                    language: {
                        url: '/scripts/datatables/spanish.json'
                    }
                });
            }

            initTable();

            // Cargar datos desde handler (adapto campos al formato del ejemplo)
            function cargarDatos() {
                fetch(handlerUrl + '?op=consultar', { credentials: 'include' })
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp || !resp.success) {
                            swal.fire('Error', 'Error al cargar datos: ' + (resp ? resp.message : 'Respuesta inválida'), 'error');
                            return;
                        }

                        // Mapeo para tabla: id, descripcion, impacto, ocurrencia, nivel_riesgo_pld, estatus, mitigantes, acciones
                        const rows = resp.data.map(function (r) {
                            // Intentar mapear campos conocidos; si no existen, usar valores por defecto
                            const id = r.id || 0;
                            const descripcion = r.descripcion || r.fecha || ''; // si no hay descripción, muestro fecha (fallback)
                            const impacto = (r.impacto !== undefined) ? r.impacto : (r.tasa_interes !== undefined ? r.tasa_interes : '');
                            const ocurrencia = (r.ocurrencia !== undefined) ? r.ocurrencia : (r.puntos_adicionales !== undefined ? ( (parseFloat(r.puntos_adicionales) * 100) + '%' ) : '');
                            const nivel = (r.nivel_riesgo_pld !== undefined) ? r.nivel_riesgo_pld : (r.valor_tasa_interes !== undefined ? r.valor_tasa_interes : '');
                            const estBadge = r.estatus ? '<span class="badge bg-success badge-estado">Activo</span>' : '<span class="badge bg-secondary badge-estado">Inactivo</span>';
                            const mitigantesCount = (r.mitigantes !== undefined) ? r.mitigantes : 0;

                            const mitigantesHtml = '<span class="badge bg-info text-white mitigantes-badge">' + mitigantesCount + '</span>';

                            const acciones =
                                '<button type="button" class="btn btn-outline-primary btn-sm action-btn btn-edit me-1" data-id="' + id + '" title="Editar"><i class="fa fa-pencil"></i></button>' +
                                '<button type="button" class="btn btn-outline-danger btn-sm action-btn btn-delete" data-id="' + id + '" title="Eliminar"><i class="fa fa-trash"></i></button>';

                            return [
                                id,
                                descripcion,
                                impacto,
                                ocurrencia,
                                (nivel !== null && nivel !== undefined) ? parseFloat(nivel).toFixed(2) : '',
                                estBadge,
                                mitigantesHtml,
                                acciones
                            ];
                        });

                        if (dt) {
                            dt.clear();
                            dt.rows.add(rows);
                            dt.draw();
                        } else {
                            const tbody = document.querySelector('#tblLibor tbody');
                            tbody.innerHTML = '';
                            rows.forEach(r => {
                                const tr = document.createElement('tr');
                                r.forEach(c => {
                                    const td = document.createElement('td');
                                    td.innerHTML = c;
                                    tr.appendChild(td);
                                });
                                tbody.appendChild(tr);
                            });
                        }
                    })
                    .catch(err => {
                        console.error(err);
                        swal.fire('Error', 'No fue posible obtener datos del servidor.', 'error');
                    });
            }

            // Abrir modal para captura (botón + Capturar)
            document.getElementById('btnCapturar').addEventListener('click', function () {
                modoEdicion = false;
                resetForm();
                const modal = new bootstrap.Modal(document.getElementById('modalCaptura'), { backdrop: 'static' });
                modal.show();
            });

            // Guardar (en esta versión guardamos campos mapeados: descripcion, impacto, ocurrencia, nivel_riesgo_pld, fecha, estatus)
            document.getElementById('btnGuardar').addEventListener('click', function () {
                const id = parseInt(document.getElementById('hidId').value || '0');
                const descripcion = (document.getElementById('descripcion').value || '').trim();
                const impacto = parseFloat(document.getElementById('impacto').value || '0');
                const ocurrencia = (document.getElementById('ocurrencia').value || '').trim();
                const nivel = (document.getElementById('nivel_riesgo_pld').value || '').trim();
                const fecha = document.getElementById('fecha').value;
                const estatus = document.getElementById('estatus').checked ? '1' : '0';

                if (!descripcion) {
                    swal.fire('Atención', 'La descripción es requerida.', 'warning');
                    return;
                }
                if (!fecha) {
                    swal.fire('Atención', 'La fecha es requerida.', 'warning');
                    return;
                }

                const payload = new URLSearchParams();
                // Nota: el handler original espera tasa_interes/puntos/valor, pero para mantener compatibilidad
                // mapeamos los valores a los nombres que el handler entiende:
                // - tasa_interes <- impacto
                // - puntos_adicionales <- ocurrencia (si es numérico)
                // - valor_tasa_interes <- nivel_riesgo_pld
                payload.append('tasa_interes', impacto.toString());
                // intentar extraer porcentaje numérico de ocurrencia (ej "25%")
                const ocurrNum = String(ocurrencia).replace('%', '').trim();
                payload.append('puntos_adicionales', isNaN(parseFloat(ocurrNum)) ? '0' : parseFloat(ocurrNum).toString());
                payload.append('valor_tasa_interes', nivel.toString());
                payload.append('fecha', fecha);
                payload.append('estatus', estatus);

                const op = modoEdicion ? 'actualizar' : 'guardar';
                if (modoEdicion) payload.append('id', id.toString());

                fetch(handlerUrl + '?op=' + op, {
                    method: 'POST',
                    body: payload,
                    credentials: 'include',
                    headers: { 'Accept': 'application/json' }
                })
                    .then(r => r.json())
                    .then(resp => {
                        if (resp && resp.success) {
                            swal.fire('OK', resp.message || 'Operación correcta.', 'success');
                            const modalEl = document.getElementById('modalCaptura');
                            const modal = bootstrap.Modal.getInstance(modalEl);
                            if (modal) modal.hide();
                            cargarDatos();
                        } else {
                            swal.fire('Error', (resp && resp.message) ? resp.message : 'Error al guardar.', 'error');
                        }
                    })
                    .catch(err => {
                        console.error(err);
                        swal.fire('Error', 'Error al comunicarse con el servidor.', 'error');
                    });
            });

            // Delegación: editar / eliminar / toggle (edición se hace llamando al handler 'obtener' y precargando el modal)
            document.querySelector('#tblLibor tbody').addEventListener('click', function (ev) {
                const btn = ev.target.closest('button');
                if (!btn) return;
                const id = btn.getAttribute('data-id');
                if (!id) return;

                if (btn.classList.contains('btn-edit')) {
                    modoEdicion = true;
                    fetch(handlerUrl + '?op=obtener&id=' + encodeURIComponent(id), { credentials: 'include' })
                        .then(r => r.json())
                        .then(resp => {
                            if (resp && resp.success) {
                                const d = resp.data;
                                // Mapear campos (tener en cuenta que handler original no tiene descripcion/impacto etc. por defecto)
                                document.getElementById('hidId').value = d.id || '0';
                                document.getElementById('descripcion').value = d.descripcion || d.fecha || '';
                                document.getElementById('impacto').value = d.impacto || d.tasa_interes || '';
                                // ocurrencia puede venir en puntos_adicionales
                                document.getElementById('ocurrencia').value = (d.ocurrencia || d.puntos_adicionales) ? (d.puntos_adicionales + '%') : '';
                                document.getElementById('nivel_riesgo_pld').value = d.nivel_riesgo_pld || d.valor_tasa_interes || '';
                                document.getElementById('fecha').value = d.fecha || '';
                                document.getElementById('estatus').checked = d.estatus ? true : false;

                                const modal = new bootstrap.Modal(document.getElementById('modalCaptura'), { backdrop: 'static' });
                                modal.show();
                            } else {
                                swal.fire('Error', resp.message || 'Registro no encontrado.', 'error');
                            }
                        })
                        .catch(err => {
                            console.error(err);
                            swal.fire('Error', 'No fue posible obtener el registro.', 'error');
                        });
                } else if (btn.classList.contains('btn-delete')) {
                    swal.fire({
                        title: 'Confirmar eliminación',
                        text: '¿Deseas eliminar este registro? (Operación destructiva)',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: 'Sí, eliminar',
                        cancelButtonText: 'Cancelar'
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            // No existe 'eliminar' en handler original: vamos a usar toggle para marcar inactivo
                            fetch(handlerUrl + '?op=toggle&id=' + encodeURIComponent(id), { credentials: 'include' })
                                .then(r => r.json())
                                .then(resp => {
                                    if (resp && resp.success) {
                                        swal.fire('OK', resp.message || 'Operación correcta.', 'success');
                                        cargarDatos();
                                    } else {
                                        swal.fire('Error', resp.message || 'No fue posible cambiar estatus.', 'error');
                                    }
                                }).catch(err => {
                                    console.error(err);
                                    swal.fire('Error', 'Error de comunicación con el servidor.', 'error');
                                });
                        }
                    });
                }
            });

            function resetForm() {
                document.getElementById('hidId').value = '0';
                document.getElementById('descripcion').value = '';
                document.getElementById('impacto').value = '';
                document.getElementById('ocurrencia').value = '';
                document.getElementById('nivel_riesgo_pld').value = '';
                document.getElementById('fecha').value = '';
                document.getElementById('estatus').checked = true;
            }

            // Carga inicial
            cargarDatos();

            // Exponer utilidades para debug
            window._PLD_libor = { cargarDatos: cargarDatos, resetForm: resetForm };
        });
    </script>
</asp:Content>
