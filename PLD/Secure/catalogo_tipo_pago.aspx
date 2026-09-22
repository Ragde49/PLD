<%@ Page Title="Catálogo: Tipo de Pago" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_tipo_pago.aspx.vb" Inherits="PLD.catalogo_tipo_pago" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <h4>Consulta de catálogo</h4>

    <!-- Encabezado estilo tarjeta (barra azul con título y botón Capturar a la derecha) -->
    <div class="card mb-3">
        <div class="card-header p-0">
            <div class="d-flex align-items-center justify-content-between" style="background:#0d6efd; color:#fff; padding:.5rem 1rem;">
                <div style="font-weight:600;">
                    Créditos
                </div>
                <div>
                    <button type="button" id="btnNuevo" class="btn btn-success btn-sm">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
            </div>
        </div>

        <div class="card-body p-2">
            <!-- Tabla -->
            <div class="table-responsive">
                <table id="tblCatalogo" class="table table-sm table-striped table-hover w-100">
                    <thead class="table-light">
                        <tr>
                            <th>ID</th>
                            <th>Descripción</th>
                            <th class="text-center">Impacto</th>
                            <th class="text-center">Ocurrencia</th>
                            <th class="text-center">Nivel Riesgo P.L.D</th>
                            <th class="text-center">Estatus</th>
                            <th class="text-center">Mitigantes</th>
                            <th class="text-center">Acciones</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Renderizado por JS -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- Modal: Captura / Edición (NO form anidado) -->
<div class="modal fade" id="modalCaptura" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
    <div class="modal-dialog modal-md">
        <div class="modal-content">
            <div class="modal-header">
                <h5 id="modalTitle" class="modal-title">Nuevo Tipo de Pago</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
            </div>
            <div class="modal-body">
                <div id="formTipoPago" autocomplete="off">
                    <input type="hidden" id="txtId" value="0" />
                    <div class="mb-2">
                        <label class="form-label">Descripción *</label>
                        <input type="text" id="txtDescripcion" class="form-control" maxlength="200" required>
                    </div>

                    <div class="row">
                        <div class="col-4 mb-2">
                            <label class="form-label">Impacto *</label>
                            <input type="number" id="txtImpacto" class="form-control" min="0" step="1" required>
                        </div>
                        <div class="col-4 mb-2">
                            <label class="form-label">Probabilidad (%) *</label>
                            <input type="number" id="txtProbabilidad" class="form-control" min="0" max="100" step="0.01" required>
                        </div>
                        <div class="col-4 mb-2">
                            <label class="form-label">Nivel riesgo P.L.D. *</label>
                            <input type="text" id="txtNivelRiesgo" class="form-control" placeholder="ej. 12.50" required>
                        </div>
                    </div>

                    <div class="form-check mt-1">
                        <input class="form-check-input" type="checkbox" id="chkActivo" checked>
                        <label class="form-check-label" for="chkActivo">Estatus (activo)</label>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" id="btnGuardar" class="btn btn-primary">
                    <i class="fa fa-save"></i> Guardar información
                </button>
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                    <i class="fa fa-times"></i> Cancelar
                </button>
            </div>
        </div>
    </div>
</div>

<style>
    /* Mantener columnas según contenido y evitar wrapping */
    #tblCatalogo td, #tblCatalogo th { white-space: nowrap; vertical-align: middle; }
    .dt-center { text-align:center; }
    .badge-mitigante { display:inline-block; min-width:28px; height:22px; line-height:22px; text-align:center; border-radius:12px; background:#09b0ff; color:#fff; font-weight:600; }
    .action-btn { padding:4px 7px; margin-left:4px; }
</style>

<script type="text/javascript">
document.addEventListener("DOMContentLoaded", function() {

    const HANDLER_URL = "/handlers/handler_tipo_pago.ashx";
    let table;
    const modal = new bootstrap.Modal(document.getElementById('modalCaptura'), { backdrop: 'static' });

    function mostrarAviso(titulo, texto, tipo) {
        if (window.swal && typeof swal.fire === 'function') {
            swal.fire(titulo, texto, tipo);
        } else if (window.Swal && typeof Swal.fire === 'function') {
            Swal.fire(titulo, texto, tipo);
        } else {
            alert(titulo + "\n" + texto);
        }
    }

    // Inicializa DataTable y DOM: length (left), filter (right), table, info, pagination
    function initDataTable() {
        table = $('#tblCatalogo').DataTable({
            data: [],
            columns: [
                { data: 'id', className: 'dt-center' },
                { data: 'descripcion' },
                { data: 'impacto', className: 'dt-center' },
                { data: 'probabilidad', className: 'dt-center', render: function(d){ return (d === null || d === undefined || d === "") ? "" : d + "%"; } },
                { data: 'nivel_riesgo_pld', className: 'dt-center', render: function(d){ return (d === null || d === undefined || d === "") ? "" : Number(d).toFixed(2); } },
                { data: 'estatus', className: 'dt-center', render: function(d){
                    return (d === true || d === 1 || d === "1") ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>';
                }},
                { data: 'mitigantesCount', className: 'dt-center', render: function(d){ return '<span class="badge-mitigante">'+ (d||0) +'</span>'; } },
                { data: null, orderable: false, searchable: false, className: 'dt-center', render: function(data, type, row){
                    return '\
                        <button type="button" class="btn btn-outline-primary btn-sm action-btn btnEdit" title="Editar" data-id="'+ row.id +'"><i class="fa fa-pencil"></i></button>\
                        <button type="button" class="btn btn-outline-danger btn-sm action-btn btnDelete" title="Baja" data-id="'+ row.id +'"><i class="fa fa-trash"></i></button>';
                }}
            ],
            pageLength: 25,
            lengthChange: true,
            responsive: true,
            autoWidth: false,
            order: [[0, 'asc']],
            dom: '<"d-flex justify-content-between mb-2"<"dt-length"l><"dt-filter"f>>t<"d-flex justify-content-between mt-2"<"dt-info"i><"dt-pag"p>>',
            language: { url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json' }
        });

        // Delegación de eventos en la tabla
        $('#tblCatalogo tbody').on('click', '.btnEdit', function(){
            const id = this.getAttribute('data-id');
            editarRegistro(parseInt(id,10));
        });
        $('#tblCatalogo tbody').on('click', '.btnDelete', function(){
            const id = parseInt(this.getAttribute('data-id'),10);
            bajaRegistro(id);
        });
    }

    // Cargar datos desde handler
    async function cargarDatos() {
        try {
            const resp = await fetch(HANDLER_URL + '?op=consultar', { method: 'GET', credentials: 'same-origin' });
            if (!resp.ok) throw new Error('Error en la respuesta del servidor');
            const json = await resp.json();
            let filas = [];
            if (json && json.ok === true && Array.isArray(json.data)) filas = json.data;
            else if (Array.isArray(json)) filas = json;
            else filas = [];
            // Agregar campo mitigantesCount por compatibilidad (si no existe)
            filas = filas.map(r => {
                if (r.mitigantesCount === undefined) r.mitigantesCount = (r.mitigantes || 0);
                return r;
            });
            table.clear();
            table.rows.add(filas);
            table.draw();
        } catch (err) {
            console.error(err);
            mostrarAviso('Error', 'Error en la respuesta del servidor', 'error');
        }
    }

    // Nuevo
    document.getElementById('btnNuevo').addEventListener('click', function(){
        limpiarModal();
        document.getElementById('modalTitle').innerText = 'Nuevo Tipo de Pago';
        modal.show();
    });

    function limpiarModal() {
        document.getElementById('txtId').value = 0;
        document.getElementById('txtDescripcion').value = '';
        document.getElementById('txtImpacto').value = '';
        document.getElementById('txtProbabilidad').value = '';
        document.getElementById('txtNivelRiesgo').value = '';
        document.getElementById('chkActivo').checked = true;
    }

    // Editar
    async function editarRegistro(id) {
        try {
            const resp = await fetch(HANDLER_URL + '?op=getbyid&id=' + encodeURIComponent(id), { method: 'GET', credentials: 'same-origin' });
            if (!resp.ok) throw new Error('Error en la respuesta del servidor');
            const json = await resp.json();
            let obj = null;
            if (json && json.ok && json.data) obj = json.data;
            else if (json && json.id) obj = json;
            else { mostrarAviso('Error', 'Registro no encontrado', 'error'); return; }

            document.getElementById('txtId').value = obj.id || 0;
            document.getElementById('txtDescripcion').value = obj.descripcion || '';
            document.getElementById('txtImpacto').value = (obj.impacto !== undefined && obj.impacto !== null) ? obj.impacto : '';
            document.getElementById('txtProbabilidad').value = (obj.probabilidad !== undefined && obj.probabilidad !== null) ? obj.probabilidad : '';
            document.getElementById('txtNivelRiesgo').value = (obj.nivel_riesgo_pld !== undefined && obj.nivel_riesgo_pld !== null) ? Number(obj.nivel_riesgo_pld).toFixed(2) : '';
            document.getElementById('chkActivo').checked = (obj.estatus === true || obj.estatus === 1 || obj.estatus === "1");

            document.getElementById('modalTitle').innerText = 'Editar Tipo de Pago';
            modal.show();
        } catch (err) {
            console.error(err);
            mostrarAviso('Error', 'No se pudo obtener el registro.', 'error');
        }
    }

    // Guardar
    document.getElementById('btnGuardar').addEventListener('click', async function() {
        const id = parseInt(document.getElementById('txtId').value || "0", 10);
        const descripcion = document.getElementById('txtDescripcion').value.trim();
        const impactoStr = document.getElementById('txtImpacto').value;
        const probStr = document.getElementById('txtProbabilidad').value;
        const nivelStr = document.getElementById('txtNivelRiesgo').value;
        const activo = document.getElementById('chkActivo').checked ? 1 : 0;

        if (descripcion === '') { mostrarAviso('Atención', 'Descripción requerida', 'warning'); return; }
        const impacto = parseInt(impactoStr, 10);
        if (isNaN(impacto)) { mostrarAviso('Atención', 'Impacto debe ser número entero', 'warning'); return; }
        const probabilidad = parseFloat(String(probStr).replace(',', '.'));
        if (isNaN(probabilidad)) { mostrarAviso('Atención', 'Probabilidad debe ser numérica', 'warning'); return; }
        const nivel_riesgo_pld = parseFloat(String(nivelStr).replace(',', '.'));
        if (isNaN(nivel_riesgo_pld)) { mostrarAviso('Atención', 'Nivel riesgo PLD inválido', 'warning'); return; }

        const payload = new URLSearchParams();
        payload.append('op', id === 0 ? 'guardar' : 'editar');
        if (id !== 0) payload.append('id', id);
        payload.append('descripcion', descripcion);
        payload.append('impacto', impacto);
        payload.append('probabilidad', probabilidad);
        payload.append('nivel_riesgo_pld', nivel_riesgo_pld.toFixed(2));
        payload.append('estatus', activo);

        try {
            const resp = await fetch(HANDLER_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: payload.toString(),
                credentials: 'same-origin'
            });
            if (!resp.ok) throw new Error('Error en la respuesta del servidor (guardar).');
            const json = await resp.json();
            if (json && (json.ok === true || json.success === true)) {
                mostrarAviso('Éxito', json.mensaje || 'Registro guardado', 'success');
                modal.hide();
                limpiarModal();
                await cargarDatos();
            } else {
                mostrarAviso('Error', json.mensaje || 'No se pudo guardar', 'error');
            }
        } catch (err) {
            console.error(err);
            mostrarAviso('Error', 'No se pudo guardar el registro (revise consola).', 'error');
        }
    });

    // Baja (desactivar)
    async function bajaRegistro(id) {
        swal.fire({
            title: 'Confirmar',
            text: '¿Desea dar de baja este registro?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, dar de baja',
            cancelButtonText: 'Cancelar'
        }).then(async (result) => {
            if (result && result.isConfirmed) {
                try {
                    const resp = await fetch(HANDLER_URL, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: 'op=desactivar&id=' + encodeURIComponent(id),
                        credentials: 'same-origin'
                    });
                    if (!resp.ok) throw new Error('Error en la respuesta del servidor (desactivar).');
                    const json = await resp.json();
                    if (json && (json.ok === true || json.success === true)) {
                        mostrarAviso('Éxito', json.mensaje || 'Registro dado de baja', 'success');
                        await cargarDatos();
                    } else {
                        mostrarAviso('Error', json.mensaje || 'No se pudo dar de baja', 'error');
                    }
                } catch (err) {
                    console.error(err);
                    mostrarAviso('Error', 'Error al dar de baja (revise consola).', 'error');
                }
            }
        });
    }

    // Inicialización
    initDataTable();
    cargarDatos();

});
</script>

</asp:Content>
