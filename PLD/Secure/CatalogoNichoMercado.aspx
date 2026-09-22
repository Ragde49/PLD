<%@ Page Title="Catálogo Nicho de Mercado" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CatalogoNichoMercado.aspx.vb" Inherits="PLD.CatalogoNichoMercado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <!-- Página / título general (igual que la captura que mostraste) -->
        <h4 class="mb-3">Consulta de catálogo</h4>

        <!-- Barra azul con subtítulo (ej: Créditos) y botón Capturar a la derecha -->
        <div class="card mb-3">
            <div class="card-header p-0">
                <div class="d-flex align-items-center justify-content-between bg-primary text-white p-2">
                    <div class="fw-bold ms-2">Créditos</div>
                    <div class="me-2">
                        <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
                            <i class="fas fa-plus"></i> Capturar
                        </button>
                    </div>
                </div>
            </div>

            <div class="card-body p-2">
                <!-- DataTables controls: left = length, right = search (DataTables default DOM) -->
                <div class="table-responsive">
                    <table id="tblNicho" class="table table-sm table-striped table-hover w-100 align-middle">
                        <thead class="table-light">
                            <tr>
                                <th style="width:50px">ID</th>
                                <th>Descripción</th>
                                <th style="width:90px">Impacto</th>
                                <th style="width:110px">Ocurrencia</th>
                                <th style="width:140px">Nivel Riesgo P.L.D</th>
                                <th style="width:100px">Estatus</th>
                                <th style="width:100px">Mitigantes</th>
                                <th style="width:110px">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- cargado por JS -->
                        </tbody>
                    </table>
                </div>

                <!-- footer con paginación (DataTables lo genera automáticamente; aquí queda espacio visual igual a la captura) -->
            </div>
        </div>
    </div>

    <!-- Modal Captura / Edición (sin cambios funcionales, solo clases estéticas ya definidas antes) -->
    <div class="modal fade" id="modalNicho" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 id="modalTitle" class="modal-title">Capturar Nicho</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
          </div>
          <div class="modal-body">
            <form id="formNicho" autocomplete="off" onsubmit="return false;">
                <input type="hidden" id="nicho_id" value="0" />
                <div class="row">
                    <div class="col-md-8 mb-3">
                        <label class="form-label">Nicho del mercado*</label>
                        <input type="text" id="descripcion" class="form-control" maxlength="300" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Clasificación*</label>
                        <select id="clasificacion" class="form-control">
                            <option value="">--------</option>
                            <option>INDUSTRIA</option>
                            <option>COMERCIO</option>
                            <option>SERVICIOS</option>
                            <option>CONSUMO</option>
                            <option>INDISTINTO</option>
                            <option>NO APLICA</option>
                        </select>
                    </div>

                    <div class="col-md-4 mb-3">
                        <label class="form-label">Impacto* (entero)</label>
                        <input type="number" id="impacto" class="form-control" min="0" step="1" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Probabilidad* (entero)</label>
                        <input type="number" id="probabilidad" class="form-control" min="0" step="1" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Nivel de riesgo P.L.D* (decimal)</label>
                        <input type="text" id="nivel_riesgo_pld" class="form-control" placeholder="0.00" />
                    </div>

                    <div class="col-md-12 mb-3">
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="estatus" checked>
                            <label class="form-check-label" for="estatus">Estatus (Activo)</label>
                        </div>
                    </div>
                </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" id="btnGuardar" class="btn btn-primary">
                <i class="fas fa-save"></i> Guardar información
            </button>
            <button type="button" id="btnCancelar" class="btn btn-danger" data-bs-dismiss="modal">
                <i class="fas fa-times"></i> Cancelar
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Estilos específicos para replicar el look de la captura (badges, botones de acción, tamaño filas) -->
    <style>
        /* reducir paddings de celdas para que quede compacto como la imagen */
        #tblNicho.table-sm tbody td, #tblNicho.table-sm thead th {
            padding-top: .35rem;
            padding-bottom: .35rem;
            vertical-align: middle;
        }

        /* Estatus badge verde similar a la imagen */
        .badge-estatus {
            display:inline-block;
            padding: .25rem .5rem;
            font-size: .75rem;
            border-radius: .35rem;
        }

        .badge-mitigantes {
            display:inline-block;
            min-width:28px;
            height:28px;
            line-height:28px;
            text-align:center;
            border-radius:14px;
            font-weight:700;
            color:#fff;
            background:#2db7f5; /* cyan like in screenshot */
            box-shadow:none;
        }

        /* Botones de acción: pequeños, icon-only, con borde */
        .btn-action {
            padding: .25rem .35rem;
            width:32px;
            height:32px;
            display:inline-flex;
            align-items:center;
            justify-content:center;
            border-radius:.35rem;
            font-size:14px;
        }
        .btn-action i { font-size:14px; }

        .btn-edit {
            color:#0d6efd;
            border:1px solid #0d6efd;
            background:transparent;
        }
        .btn-delete {
            color:#dc3545;
            border:1px solid #dc3545;
            background:transparent;
        }
        .btn-view {
            color:#0b7285;
            border:1px solid #0b7285;
            background:transparent;
        }

        /* Mantener el header azul sin bordes extra (como la captura) */
        .card-header .bg-primary { border-radius: .25rem .25rem 0 0; }

        /* Alineado del search a la derecha y tamaño del length (DataTables) */
        .dataTables_wrapper .dataTables_length { float: left; margin-right: 1rem; }
        .dataTables_wrapper .dataTables_filter { float: right; text-align: right; }
    </style>

    <!-- Script (sin cambios funcionales; solo actualiza el HTML generado para acciones y badgets) -->
    <script>
    document.addEventListener("DOMContentLoaded", function () {
        const handlerUrl = "/Handlers/handler_catalogo_nicho_mercado.ashx";
        let tbl;

        function initDataTable() {
            tbl = $('#tblNicho').DataTable({
                language: { url: '/assets/datatables/spanish.json' }, // ajusta ruta si necesario
                responsive: true,
                dom: '<"d-flex justify-content-between mb-2"<"dataTables_length"l><"dataTables_filter"f>>rtip',
                columns: [
                    { data: 'id' },
                    { data: 'descripcion' },
                    { data: 'impacto' },
                    { data: 'probabilidad', render: function(d){ return (d !== null && d !== undefined) ? (d + '%') : ''; } },
                    { data: 'nivel_riesgo_pld', render: function(d){ return (d !== null && d !== undefined) ? parseFloat(d).toFixed(2) : ''; } },
                    { data: 'estatus', render: renderEstatus },
                    { data: 'mitigantes', render: renderMitigantes },
                    { data: null, orderable: false, searchable: false, render: renderAcciones }
                ],
                order: [[1, 'asc']],
                pageLength: 25,
                lengthMenu: [[10,25,50,100],[10,25,50,100]],
                pagingType: 'simple_numbers'
            });
        }

        function renderEstatus(data, type, row) {
            const activo = data === true || data === 1 || data === '1';
            if (activo) {
                return '<span class="badge badge-estatus bg-success">Activo</span>';
            } else {
                return '<span class="badge badge-estatus bg-secondary">Inactivo</span>';
            }
        }

        function renderMitigantes(data) {
            const n = (typeof data !== 'undefined' && data !== null) ? data : 0;
            return '<span class="badge-mitigantes">' + n + '</span>';
        }

        function renderAcciones(data, type, row) {
            const id = row.id;
            // Botones con mismo orden y funcionalidad que antes, solo estilo visual actualizado
            return [
                '<button type="button" class="btn btn-action btn-view me-1" data-id="' + id + '" title="Ver"><i class="fas fa-pencil-alt" style="transform:rotate(-45deg)"></i></button>',
                '<button type="button" class="btn btn-action btn-edit me-1" data-id="' + id + '" title="Editar"><i class="fas fa-edit"></i></button>',
                '<button type="button" class="btn btn-action btn-delete" data-id="' + id + '" title="Eliminar"><i class="fas fa-trash-alt"></i></button>'
            ].join('');
        }

        function cargarDatos() {
            fetch(handlerUrl + '?op=consultar', { method: 'GET', credentials: 'same-origin' })
                .then(resp => {
                    if (!resp.ok) throw new Error('Error en la respuesta del servidor');
                    return resp.json();
                })
                .then(data => {
                    tbl.clear();
                    if (Array.isArray(data)) {
                        tbl.rows.add(data).draw();
                    } else if (data && data.data && Array.isArray(data.data)) {
                        tbl.rows.add(data.data).draw();
                    } else {
                        tbl.draw();
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire('Error', 'No se pudieron cargar los datos: ' + err.message, 'error');
                });
        }

        function abrirModalNuevo() {
            document.getElementById('modalTitle').innerText = 'Capturar Nicho';
            document.getElementById('nicho_id').value = 0;
            document.getElementById('descripcion').value = '';
            document.getElementById('clasificacion').value = '';
            document.getElementById('impacto').value = '';
            document.getElementById('probabilidad').value = '';
            document.getElementById('nivel_riesgo_pld').value = '';
            document.getElementById('estatus').checked = true;
            const myModal = new bootstrap.Modal(document.getElementById('modalNicho'));
            myModal.show();
        }

        function abrirModalEditar(id) {
            fetch(handlerUrl + '?op=obtener&id=' + encodeURIComponent(id), { method: 'GET', credentials: 'same-origin' })
                .then(resp => {
                    if (!resp.ok) throw new Error('Error en la respuesta del servidor');
                    return resp.json();
                })
                .then(payload => {
                    const row = payload && payload.success ? payload.data : payload;
                    if (!row) {
                        Swal.fire('Error', 'Registro no encontrado', 'error');
                        return;
                    }
                    document.getElementById('modalTitle').innerText = 'Editar Nicho #' + row.id;
                    document.getElementById('nicho_id').value = row.id || 0;
                    document.getElementById('descripcion').value = row.descripcion || '';
                    document.getElementById('clasificacion').value = row.clasificacion || '';
                    document.getElementById('impacto').value = (row.impacto !== undefined && row.impacto !== null) ? row.impacto : '';
                    document.getElementById('probabilidad').value = (row.probabilidad !== undefined && row.probabilidad !== null) ? row.probabilidad : '';
                    document.getElementById('nivel_riesgo_pld').value = (row.nivel_riesgo_pld !== undefined && row.nivel_riesgo_pld !== null) ? row.nivel_riesgo_pld : '';
                    document.getElementById('estatus').checked = row.estatus === true || row.estatus === 1 || row.activo === 1;
                    const myModal = new bootstrap.Modal(document.getElementById('modalNicho'));
                    myModal.show();
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire('Error', 'No se pudo obtener el registro: ' + err.message, 'error');
                });
        }

        function validarFormulario() {
            const desc = document.getElementById('descripcion').value.trim();
            const impacto = document.getElementById('impacto').value.trim();
            const prob = document.getElementById('probabilidad').value.trim();
            const nivel = document.getElementById('nivel_riesgo_pld').value.trim();

            if (!desc) { Swal.fire('Atención', 'La descripción es obligatoria', 'warning'); return false; }
            if (impacto === '' || isNaN(parseInt(impacto, 10))) { Swal.fire('Atención', 'Impacto debe ser un número entero', 'warning'); return false; }
            if (prob === '' || isNaN(parseInt(prob, 10))) { Swal.fire('Atención', 'Probabilidad debe ser un número entero', 'warning'); return false; }
            if (nivel === '' || isNaN(parseFloat(nivel.toString().replace(',', '.')))) { Swal.fire('Atención', 'Nivel de riesgo P.L.D debe ser numérico (ej. 7.50)', 'warning'); return false; }

            return true;
        }

        function riesgoTextoANumero(valor) {
            if (!valor) return 0;
            switch (valor.toString().toUpperCase()) {
                case "BAJO": return 1;
                case "MEDIO": return 2;
                case "ALTO": return 3;
                default:
                    const num = parseFloat(valor.toString().replace(",", "."));
                    return isNaN(num) ? 0 : num;
            }
        }

        function guardarRegistro() {
            if (!validarFormulario()) return;

            const payload = {
                id: parseInt(document.getElementById('nicho_id').value, 10) || 0,
                descripcion: document.getElementById('descripcion').value.trim(),
                clasificacion: document.getElementById('clasificacion').value,
                impacto: parseInt(document.getElementById('impacto').value, 10),
                probabilidad: parseInt(document.getElementById('probabilidad').value, 10),
                nivel_riesgo_pld: parseFloat(document.getElementById('nivel_riesgo_pld').value.toString().replace(',', '.')),
                estatus: document.getElementById('estatus').checked ? 1 : 0,
                user: "system"
            };

            fetch(handlerUrl + '?op=guardar', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(resp => {
                if (!resp.ok) throw new Error('Error en la respuesta del servidor');
                return resp.json();
            })
            .then(result => {
                if (result && result.success) {
                    Swal.fire('OK', result.message || 'Registro guardado', 'success');
                    const myModalEl = document.getElementById('modalNicho');
                    const modal = bootstrap.Modal.getInstance(myModalEl);
                    if (modal) modal.hide();
                    cargarDatos();
                } else {
                    Swal.fire('Error', (result && result.message) ? result.message : 'No se pudo guardar', 'error');
                }
            })
            .catch(err => {
                console.error(err);
                Swal.fire('Error', 'No se pudo guardar: ' + err.message, 'error');
            });
        }

        function eliminarRegistro(id) {
            Swal.fire({
                title: 'Eliminar',
                text: '¿Deseas eliminar el registro #' + id + '?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then((res) => {
                if (res.isConfirmed) {
                    fetch(handlerUrl + '?op=eliminar&id=' + encodeURIComponent(id), { method: 'POST', credentials: 'same-origin' })
                        .then(resp => {
                            if (!resp.ok) throw new Error('Error en la respuesta del servidor');
                            return resp.json();
                        })
                        .then(result => {
                            if (result && result.success) {
                                Swal.fire('Eliminado', result.message || 'Registro eliminado', 'success');
                                cargarDatos();
                            } else {
                                Swal.fire('Error', (result && result.message) ? result.message : 'No se pudo eliminar', 'error');
                            }
                        })
                        .catch(err => {
                            console.error(err);
                            Swal.fire('Error', 'No se pudo eliminar: ' + err.message, 'error');
                        });
                }
            });
        }

        // Delegación de eventos
        document.querySelector('#tblNicho tbody').addEventListener('click', function (e) {
            const btn = e.target.closest('button');
            if (!btn) return;
            const id = btn.getAttribute('data-id');
            if (btn.classList.contains('btn-edit')) {
                abrirModalEditar(id);
            } else if (btn.classList.contains('btn-delete')) {
                eliminarRegistro(id);
            } else if (btn.classList.contains('btn-view')) {
                abrirModalEditar(id);
            }
        });

        // Botones del toolbar/modal
        document.getElementById('btnCapturar').addEventListener('click', abrirModalNuevo);
        document.getElementById('btnGuardar').addEventListener('click', guardarRegistro);

        // Inicializar
        initDataTable();
        cargarDatos();
    });
    </script>
</asp:Content>
