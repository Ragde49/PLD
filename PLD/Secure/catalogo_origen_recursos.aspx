<%@ Page Title="Catálogo Origen de los recursos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_origen_recursos.aspx.vb" Inherits="PLD.catalogo_origen_recursos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4>Consulta de catálogo</h4>

        <div class="card mb-3">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <div><strong>Créditos</strong></div>
                <div>
                    <button type="button" id="btnAdd" class="btn btn-success btn-sm">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
            </div>

            <div class="card-body">
                <div class="row mb-2">
                    <div class="col-sm-6 d-flex align-items-center">
                        <label class="me-2">Show
                            <select id="selectPageLength" class="form-control form-control-sm d-inline-block" style="width:85px;">
                                <option value="10">10</option>
                                <option value="25" selected>25</option>
                                <option value="50">50</option>
                            </select>
                        </label>
                        <span class="ms-2">entries</span>
                    </div>
                    <div class="col-sm-6 d-flex justify-content-end">
                        <div class="input-group" style="width:250px;">
                            <input type="text" id="searchBox" class="form-control form-control-sm" placeholder="Search">
                            <button type="button" id="btnSearch" class="btn btn-outline-secondary btn-sm"><i class="fa fa-search"></i></button>
                        </div>
                    </div>
                </div>

                <div class="table-responsive">
                    <table id="tblOrigenRecursos" class="table table-sm table-striped table-hover w-100">
                        <thead class="thead-light">
                            <tr>
                                <th style="width:60px;">ID</th>
                                <th>Descripción</th>
                                <th style="width:90px;" class="text-center">Impacto</th>
                                <th style="width:110px;" class="text-center">Ocurrencia</th>
                                <th style="width:140px;" class="text-center">Nivel Riesgo P.L.D</th>
                                <th style="width:110px;" class="text-center">Estatus</th>
                                <th style="width:120px;" class="text-center">Mitigantes</th>
                                <th style="width:140px;" class="text-center">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- llenado desde JS -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal Alta / Edición -->
    <div class="modal fade" id="modalForm" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog modal-md">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="modalTitle">Capturar Origen</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form id="formOrigen" onsubmit="return false;">
                <input type="hidden" id="txtId" value="0">
                <div class="mb-2">
                    <label class="form-label">Origen*</label>
                    <input type="text" id="txtOrigen" class="form-control" maxlength="150" required>
                </div>

                <div class="row">
                    <div class="col-6 mb-2">
                        <label class="form-label">Impacto (INT)*</label>
                        <input type="number" id="txtImpacto" class="form-control" min="0" step="1" required>
                    </div>
                    <div class="col-6 mb-2">
                        <label class="form-label">Probabilidad (%) (INT)*</label>
                        <input type="number" id="txtProbabilidad" class="form-control" min="0" max="100" step="1" required>
                    </div>
                </div>

                <div class="mb-2">
                    <label class="form-label">Nivel de riesgo P.L.D. (DECIMAL 5,2)*</label>
                    <input type="number" id="txtNivelRiesgo" class="form-control" step="0.01" min="0" required>
                    <small class="text-muted">Por defecto: Impacto × (Probabilidad / 100). Puedes editar manualmente.</small>
                </div>

                <div class="form-check mb-2">
                    <input class="form-check-input" type="checkbox" id="chkActivo" checked>
                    <label class="form-check-label" for="chkActivo">Activo</label>
                </div>

            </form>
          </div>
          <div class="modal-footer">
            <button type="button" id="btnSave" class="btn btn-primary">Guardar</button>
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
          </div>
        </div>
      </div>
    </div>

    <style>
        .badge-estatus-inactivo { background:#f0ad4e; color:#fff; }
        .badge-estatus-activo { background:#28a745; color:#fff; }
        .badge-mitigantes { background:#17a2b8; color:#fff; border-radius:50%; padding:6px 8px; }
        .dt-center { text-align:center; }
        .action-btn { margin-left:4px; margin-right:4px; }
    </style>

    <script>
    document.addEventListener("DOMContentLoaded", function () {
        const handlerUrl = '/handlers/handler_origen_recursos.ashx';
        let table = null;
        let bootstrapModal = null;

        function computeNivelPorDefecto(impacto, probabilidadPercent) {
            const nivel = (Number(impacto) || 0) * ((Number(probabilidadPercent) || 0) / 100);
            return Number(nivel.toFixed(2));
        }
        function formatPercent(n) { if (n === null || n === undefined) return ''; return parseInt(n, 10) + '%'; }
        function formatDecimal2(n) { if (n === null || n === undefined) return ''; return Number(n).toFixed(2); }

        function actionsRenderer(data, type, row) {
            const id = row.id;
            const editBtn = `<button type="button" class="btn btn-outline-primary btn-sm action-btn btn-edit" data-id="${id}" title="Editar"><i class="fa fa-pencil"></i></button>`;
            const deleteBtn = `<button type="button" class="btn btn-outline-danger btn-sm action-btn btn-delete" data-id="${id}" title="Eliminar"><i class="fa fa-trash"></i></button>`;
            if (row.estatus == 1 || row.estatus === true) {
                const checkDisabled = `<button type="button" class="btn btn-outline-success btn-sm action-btn" title="Activo" disabled><i class="fa fa-check"></i></button>`;
                return `<div class="d-flex justify-content-center">${editBtn}${checkDisabled}${deleteBtn}</div>`;
            } else {
                const activateBtn = `<button type="button" class="btn btn-success btn-sm action-btn btn-activate" data-id="${id}" title="Activar"><i class="fa fa-check"></i></button>`;
                return `<div class="d-flex justify-content-center">${editBtn}${activateBtn}${deleteBtn}</div>`;
            }
        }

        async function loadList() {
            try {
                const resp = await fetch(`${handlerUrl}?op=listar`, { method: 'GET', credentials: 'same-origin' });
                if (!resp.ok) throw new Error('HTTP ' + resp.status);
                const data = await resp.json();
                if (!Array.isArray(data)) { swal.fire('Error','Respuesta inesperada del servidor al listar.'); return; }
                initOrReloadTable(data);
            } catch (err) {
                console.error(err);
                swal.fire('Error', 'No se pudo cargar la lista. Revisa la consola.', 'error');
            }
        }

        function initOrReloadTable(dataArray) {
            // destruir si existe y limpiar DOM para evitar duplicados
            if ($.fn.DataTable.isDataTable('#tblOrigenRecursos')) {
                $('#tblOrigenRecursos').DataTable().clear().destroy();
                document.querySelector('#tblOrigenRecursos tbody').innerHTML = '';
            }

            table = $('#tblOrigenRecursos').DataTable({
                data: dataArray,
                columns: [
                    { data: 'id', className: 'dt-center' },
                    { data: 'origen', render: function(d){ return d ? String(d).toUpperCase() : ''; } },
                    { data: 'impacto', className: 'dt-center' },
                    { data: 'probabilidad', className: 'dt-center', render: function(d){ return formatPercent(d); } },
                    { data: 'nivel_riesgo_pld', className: 'dt-center', render: function(d){ return formatDecimal2(d); } },
                    { data: 'estatus', className: 'dt-center', render: function(d){ return d == 1 || d === true ? '<span class="badge badge-estatus-activo">Activo</span>' : '<span class="badge badge-estatus-inactivo">Inactivo</span>'; } },
                    { data: null, className: 'dt-center', orderable:false, searchable:false, render: function(data,type,row){ return `<span class="badge-mitigantes">0</span>`; } },
                    { data: null, orderable: false, searchable:false, render: actionsRenderer }
                ],
                pageLength: Number(document.getElementById('selectPageLength').value) || 25,
                order: [[0,'asc']],
                responsive: true,
                // QUITAMOS 'l' y 'f' para NO duplicar controls (tu HTML ya tiene Show/search)
                dom: "rt<'row'<'col-sm-6'i><'col-sm-6'p>>",
                // Desactivamos los controles incorporados
                searching: false,
                lengthChange: false,
                language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json" }
            });
        }

        // Event handlers
        document.getElementById('btnAdd').addEventListener('click', function () {
            document.getElementById('modalTitle').innerText = 'Capturar Origen';
            document.getElementById('txtId').value = 0;
            document.getElementById('txtOrigen').value = '';
            document.getElementById('txtImpacto').value = '';
            document.getElementById('txtProbabilidad').value = '';
            document.getElementById('txtNivelRiesgo').value = '';
            document.getElementById('chkActivo').checked = true;
            const modalEl = document.getElementById('modalForm');
            bootstrapModal = bootstrap.Modal.getOrCreateInstance(modalEl);
            bootstrapModal.show();
        });

        // Editar
        $('#tblOrigenRecursos tbody').on('click', '.btn-edit', async function () {
            const id = this.getAttribute('data-id');
            try {
                const resp = await fetch(`${handlerUrl}?op=getbyid&id=${encodeURIComponent(id)}`, { method: 'GET', credentials: 'same-origin' });
                if (!resp.ok) throw new Error('HTTP ' + resp.status);
                const row = await resp.json();
                if (!row || !row.id) { swal.fire('Error','Registro no encontrado'); return; }
                document.getElementById('modalTitle').innerText = 'Editar Origen';
                document.getElementById('txtId').value = row.id || 0;
                document.getElementById('txtOrigen').value = row.origen || '';
                document.getElementById('txtImpacto').value = row.impacto ?? '';
                document.getElementById('txtProbabilidad').value = row.probabilidad ?? '';
                document.getElementById('txtNivelRiesgo').value = row.nivel_riesgo_pld ?? '';
                document.getElementById('chkActivo').checked = row.estatus == 1 || row.estatus === true;
                const modalEl = document.getElementById('modalForm');
                bootstrapModal = bootstrap.Modal.getOrCreateInstance(modalEl);
                bootstrapModal.show();
            } catch (err) {
                console.error(err);
                swal.fire('Error', 'No se pudo obtener el registro para edición.', 'error');
            }
        });

        // Activar
        $('#tblOrigenRecursos tbody').on('click', '.btn-activate', async function () {
            const id = this.getAttribute('data-id');
            try {
                const respRow = await fetch(`${handlerUrl}?op=getbyid&id=${encodeURIComponent(id)}`, { method: 'GET', credentials: 'same-origin' });
                if (!respRow.ok) throw new Error('HTTP ' + respRow.status);
                const row = await respRow.json();
                if (!row || !row.id) { swal.fire('Error','Registro no encontrado'); return; }
                const payload = { id: row.id, origen: row.origen, impacto: row.impacto, probabilidad: row.probabilidad, nivel_riesgo_pld: row.nivel_riesgo_pld, activo: 1 };
                const resp = await fetch(`${handlerUrl}?op=editar`, { method: 'POST', headers: {'Content-Type':'application/json'}, credentials:'same-origin', body: JSON.stringify(payload) });
                const j = await resp.json();
                if (j.ok) { swal.fire('Listo','Registro activado','success'); await loadList(); } else { swal.fire('Error', j.mensaje || 'No se pudo activar', 'error'); }
            } catch (err) { console.error(err); swal.fire('Error', 'Fallo al activar. Revisa consola.', 'error'); }
        });

        // Eliminar lógico
        $('#tblOrigenRecursos tbody').on('click', '.btn-delete', function () {
            const id = this.getAttribute('data-id');
            swal.fire({ title:'Confirmar', text:'¿Eliminar este registro?', icon:'warning', showCancelButton:true, confirmButtonText:'Sí, eliminar', cancelButtonText:'Cancelar' })
            .then(async (res) => {
                if (res.isConfirmed) {
                    try {
                        const resp = await fetch(`${handlerUrl}?op=eliminar&id=${encodeURIComponent(id)}`, { method: 'POST', credentials: 'same-origin' });
                        if (!resp.ok) throw new Error('HTTP ' + resp.status);
                        const j = await resp.json();
                        if (j.ok) { swal.fire('Eliminado', j.mensaje || 'Registro eliminado', 'success'); await loadList(); } else { swal.fire('Error', j.mensaje || 'No se pudo eliminar', 'error'); }
                    } catch (err) { console.error(err); swal.fire('Error', 'Fallo al eliminar. Revisa consola.', 'error'); }
                }
            });
        });

        // Guardar / Editar desde modal
        document.getElementById('btnSave').addEventListener('click', async function () {
            const id = Number(document.getElementById('txtId').value || 0);
            const origen = (document.getElementById('txtOrigen').value || '').trim();
            let impacto = Number(document.getElementById('txtImpacto').value || 0);
            let probabilidad = Number(document.getElementById('txtProbabilidad').value || 0);
            let nivel = Number(document.getElementById('txtNivelRiesgo').value || 0);
            const activo = document.getElementById('chkActivo').checked ? 1 : 0;

            if (!origen) { swal.fire('Validación','Origen es requerido','warning'); return; }
            if (!Number.isFinite(impacto) || impacto < 0) { swal.fire('Validación','Impacto inválido','warning'); return; }
            if (!Number.isFinite(probabilidad) || probabilidad < 0 || probabilidad > 100) { swal.fire('Validación','Probabilidad debe ser 0-100','warning'); return; }

            if (!nivel || nivel === 0) {
                nivel = computeNivelPorDefecto(impacto, probabilidad);
                document.getElementById('txtNivelRiesgo').value = nivel.toFixed(2);
            }

            const payload = { id: id, origen: origen, impacto: Math.trunc(impacto), probabilidad: Math.trunc(probabilidad), nivel_riesgo_pld: Number(nivel.toFixed(2)), activo: activo };

            try {
                const op = id && id > 0 ? 'editar' : 'guardar';
                const resp = await fetch(`${handlerUrl}?op=${op}`, { method: 'POST', headers: {'Content-Type':'application/json'}, credentials: 'same-origin', body: JSON.stringify(payload) });
                if (!resp.ok) throw new Error('HTTP ' + resp.status);
                const j = await resp.json();
                if (j.ok) { swal.fire('Listo', j.mensaje || 'Guardado correctamente', 'success'); bootstrap.Modal.getInstance(document.getElementById('modalForm'))?.hide(); await loadList(); } else { swal.fire('Error', j.mensaje || 'No se pudo guardar', 'error'); }
            } catch (err) { console.error(err); swal.fire('Error','Fallo al guardar. Revisa consola.','error'); }
        });

        // SEARCH: usamos la API de DataTables (client-side) para no recargar el servidor y evitar duplicados visuales
        document.getElementById('btnSearch').addEventListener('click', function () {
            const q = document.getElementById('searchBox').value.trim();
            if (table) { table.search(q).draw(); }
        });
        document.getElementById('searchBox').addEventListener('keyup', function (e) {
            // búsqueda en tiempo real (Enter o cada tecla)
            if (table) { table.search(this.value.trim()).draw(); }
        });

        // PageLength control (nuestro select)
        document.getElementById('selectPageLength').addEventListener('change', function () {
            const v = Number(this.value) || 25;
            if (table) { table.page.len(v).draw(); }
        });

        // Auto-calc nivel modal
        function maybeAutoComputeNivel() {
            const impacto = Number(document.getElementById('txtImpacto').value || 0);
            const prob = Number(document.getElementById('txtProbabilidad').value || 0);
            const currentNivel = Number(document.getElementById('txtNivelRiesgo').value || 0);
            const calc = computeNivelPorDefecto(impacto, prob);
            if (!currentNivel || Math.abs(currentNivel - calc) < 0.005) { document.getElementById('txtNivelRiesgo').value = calc.toFixed(2); }
        }
        document.getElementById('txtImpacto').addEventListener('input', maybeAutoComputeNivel);
        document.getElementById('txtProbabilidad').addEventListener('input', maybeAutoComputeNivel);

        // Inicial: cargamos UNA vez (trae todos los registros y permite searching client-side)
        loadList();

    }); // DOMContentLoaded
    </script>

    <!-- Nota: Site.Master debe cargar jQuery, Bootstrap 5, DataTables y SweetAlert2 -->
</asp:Content>
