<%@ Page Title="Catálogo - Instrumento monetario" Language="vb" AutoEventWireup="false"
    MasterPageFile="~/Site.Master" CodeBehind="catalogo_instrumento_monetario.aspx.vb"
    Inherits="PLD.catalogo_instrumento_monetario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Font Awesome (si no está ya en la MasterPage) -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free/css/all.min.css" />

    <style>
        /* Barra azul de encabezado (como el screenshot) */
        .header-strip {
            background-color: #0d6efd;
            color: #fff;
            border-radius: .25rem .25rem 0 0;
            padding: .65rem .85rem;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }
        .dt-control-bar {
            padding: .75rem .85rem .25rem .85rem;
        }
        /* Botones de acción compactos */
        .btn-action {
            width: 28px;
            height: 28px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 0;
            border-width: 1px;
        }
        /* Píldora para mitigantes */
        .pill-mit {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 26px;
            height: 22px;
            border-radius: 10rem;
            font-size: .75rem;
        }
        /* Ajustes DataTables */
        table.dataTable tbody td {
            vertical-align: middle;
        }
        table.dataTable thead th {
            white-space: nowrap;
        }
    </style>

    <div class="container-fluid mt-4">
        <div class="card shadow-sm">
            <div class="header-strip">
                <div class="fw-semibold">Créditos</div> <!-- Título como el screenshot -->
                <button type="button" id="btnNuevo" class="btn btn-success btn-sm">
                    <i class="fas fa-plus"></i> <span class="ms-1">Capturar</span>
                </button>
            </div>

            <!-- Barra de controles de DataTables (mantiene Show entries + Search) -->
            <div class="dt-control-bar">
                <!-- DataTables inyecta aquí los controles por defecto (length + filter) -->
            </div>

            <div class="card-body pt-0">
                <div class="table-responsive">
                    <table id="tblInstrumentos" class="table table-sm table-striped table-bordered w-100 mb-0">
                        <thead class="table-light">
                            <tr>
                                <th style="width:60px;">ID</th>
                                <th>Descripción</th>
                                <th style="width:110px;">Impacto</th>
                                <th style="width:120px;">Ocurrencia</th>
                                <th style="width:160px;">Nivel Riesgo P.L.D</th>
                                <th style="width:110px;">Estatus</th>
                                <th style="width:110px;">Mitigantes</th>
                                <th style="width:120px;">Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
                <!-- Pie y paginación quedan como DataTables por default -->
            </div>
        </div>
    </div>

    <!-- Modal Captura/Edición -->
    <div class="modal fade" id="mdlCaptura" tabindex="-1" aria-labelledby="mdlCapturaLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header py-2">
                    <h5 class="modal-title" id="mdlCapturaLabel">Datos de identificación</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hidId" />
                    <div class="row g-3">
                        <div class="col-md-8">
                            <label for="txtDescripcion" class="form-label">Instrumento*</label>
                            <input type="text" class="form-control" id="txtDescripcion" maxlength="200" autocomplete="off" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtClave" class="form-label">Clave</label>
                            <input type="text" class="form-control" id="txtClave" maxlength="50" autocomplete="off" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtImpacto" class="form-label">Impacto* (entero)</label>
                            <input type="text" class="form-control" id="txtImpacto" inputmode="numeric" placeholder="Ej. 25" autocomplete="off" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtProbabilidad" class="form-label">Probabilidad* (entero)</label>
                            <input type="text" class="form-control" id="txtProbabilidad" inputmode="numeric" placeholder="Ej. 30" autocomplete="off" />
                        </div>
                        <div class="col-md-4">
                            <label for="txtNivelRiesgo" class="form-label">Nivel de riesgo P.L.D* (decimal 2)</label>
                            <input type="text" class="form-control" id="txtNivelRiesgo" inputmode="decimal" placeholder="Ej. 12.50" autocomplete="off" />
                        </div>
                        <div class="col-md-4 d-flex align-items-end">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="chkActivo" />
                                <label class="form-check-label" for="chkActivo">Estatus activo</label>
                            </div>
                        </div>
                    </div>
                    <div class="mt-2">
                        <small class="text-muted">Solo números. Impacto/Probabilidad: enteros. Nivel de riesgo: decimal (5,2).</small>
                    </div>
                </div>
                <div class="modal-footer py-2">
                    <button type="button" id="btnGuardar" class="btn btn-primary">
                        <i class="fas fa-check"></i> <span class="ms-1">Guardar información</span>
                    </button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> <span class="ms-1">Cancelar</span>
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
    document.addEventListener("DOMContentLoaded", function () {
        const URL_HANDLER = "/handlers/handler_catalogo_instrumento_monetario.ashx";

        // Colocar los controles de DataTables debajo de la barra azul
        const dtControlBar = document.querySelector('.dt-control-bar');

        const tabla = $('#tblInstrumentos').DataTable({
            processing: true,
            serverSide: true,
            responsive: true,
            autoWidth: false,
            lengthMenu: [[25, 50, 100], [25, 50, 100]], // como el screenshot (25 por default)
            dom: "<'row'<'col-sm-6'l><'col-sm-6'f>>" + // length (Show entries) izq, search der
                 "t" +
                 "<'row'<'col-sm-6'i><'col-sm-6'p>>",
            ajax: function (dt, callback) {
                const p = new URLSearchParams();
                p.set("op", "consultar");
                p.set("draw", dt.draw || 1);
                p.set("start", dt.start || 0);
                p.set("length", dt.length || 25);
                p.set("search[value]", (dt.search && dt.search.value) ? dt.search.value : "");
                fetch(`${URL_HANDLER}?${p.toString()}`, { method: "GET", cache: "no-store" })
                    .then(r => r.json())
                    .then(j => {
                        if (!j.ok) {
                            swal.fire('Error', j.message || 'Error en la respuesta del servidor', 'error');
                            callback({ draw: 1, recordsTotal: 0, recordsFiltered: 0, data: [] });
                            return;
                        }
                        callback({ draw: j.draw, recordsTotal: j.recordsTotal, recordsFiltered: j.recordsFiltered, data: j.data });
                        // Mover el contenedor de length+filter a nuestra barra
                        if (dtControlBar && !dtControlBar.dataset.bound) {
                            const wrapper = document.getElementById('tblInstrumentos').closest('.dataTables_wrapper');
                            const topRow = wrapper.querySelector('.row');
                            if (topRow) dtControlBar.appendChild(topRow);
                            dtControlBar.dataset.bound = "1";
                        }
                    })
                    .catch(err => {
                        console.error(err);
                        swal.fire('Error', 'No se pudo cargar la información', 'error');
                        callback({ draw: 1, recordsTotal: 0, recordsFiltered: 0, data: [] });
                    });
            },
            columns: [
                { data: 'id', className: 'text-center' },
                { data: 'descripcion' },
                { data: 'impacto', className: 'text-end' },
                {
                    data: 'ocurrencia', className: 'text-end',
                    render: function (d) {
                        if (d === null || d === undefined || d === "") return "";
                        const n = Number(d);
                        return isFinite(n) ? (n.toFixed(0) + '%') : d;
                    }
                },
                {
                    data: 'nivel_riesgo_pld', className: 'text-end',
                    render: d => (d === null || d === undefined) ? '0.00' : Number(d).toFixed(2)
                },
                {
                    data: 'estatus', className: 'text-center',
                    render: d => (parseInt(d, 10) === 1)
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-secondary">Inactivo</span>'
                },
                {
                    data: 'mitigantes', className: 'text-center',
                    render: function (d) {
                        const val = (d === null || d === undefined) ? 0 : parseInt(d, 10);
                        return `<span class="pill-mit px-2 bg-info text-white">${isNaN(val) ? 0 : val}</span>`;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    className: 'text-center',
                    render: function (row) {
                        const id = row.id;
                        return `
                            <div class="btn-group" role="group" aria-label="Acciones">
                                <button type="button" class="btn btn-outline-primary btn-action btn-editar" data-id="${id}" title="Editar">
                                    <i class="fas fa-pen"></i>
                                </button>
                                <button type="button" class="btn btn-outline-warning btn-action btn-toggle" data-id="${id}" title="Activar/Inactivar">
                                    <i class="fas fa-power-off"></i>
                                </button>
                            </div>`;
                    }
                }
            ],
            order: [[0, 'asc']],
            language: { url: "/scripts/datatables/spanish.json" }
        });

        // Botón Capturar (Nuevo)
        document.getElementById('btnNuevo').addEventListener('click', () => {
            limpiarFormulario();
            document.getElementById('mdlCapturaLabel').innerText = 'Capturar instrumento monetario';
            new bootstrap.Modal(document.getElementById('mdlCaptura')).show();
        });

        // Guardar
        document.getElementById('btnGuardar').addEventListener('click', async () => {
            const id = (document.getElementById('hidId').value || '').trim();
            const descripcion = (document.getElementById('txtDescripcion').value || '').trim();
            const clave = (document.getElementById('txtClave').value || '').trim();

            const impacto = toInt((document.getElementById('txtImpacto').value || '').trim());
            const probabilidad = toInt((document.getElementById('txtProbabilidad').value || '').trim());
            const nivel = toDecimal2((document.getElementById('txtNivelRiesgo').value || '').trim());
            const activo = document.getElementById('chkActivo').checked ? 1 : 0;

            if (!descripcion) { swal.fire('Validación', 'El campo Instrumento es obligatorio.', 'warning'); return; }
            if (!isFinite(impacto) || !Number.isInteger(impacto)) { swal.fire('Validación', 'Impacto debe ser entero.', 'warning'); return; }
            if (!isFinite(probabilidad) || !Number.isInteger(probabilidad)) { swal.fire('Validación', 'Probabilidad debe ser entero.', 'warning'); return; }
            if (!isFinite(nivel)) { swal.fire('Validación', 'Nivel de riesgo debe ser decimal válido.', 'warning'); return; }

            const form = new URLSearchParams();
            form.set("descripcion", descripcion);
            form.set("clave", clave);
            form.set("impacto", impacto);
            form.set("probabilidad", probabilidad);
            form.set("nivel_riesgo_pld", Number(nivel).toFixed(2));
            form.set("activo", String(activo));

            let url = URL_HANDLER + "?op=guardar";
            if (id) url = URL_HANDLER + "?op=actualizar&id=" + encodeURIComponent(id);

            try {
                const resp = await fetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                    body: form.toString()
                });
                const json = await resp.json();
                if (!json.ok) { swal.fire('Error', json.message || 'No se pudo guardar.', 'error'); return; }
                swal.fire('Correcto', json.message || 'Guardado.', 'success');
                bootstrap.Modal.getInstance(document.getElementById('mdlCaptura'))?.hide();
                tabla.ajax.reload(null, false);
            } catch (e) {
                console.error(e);
                swal.fire('Error', 'Error al guardar.', 'error');
            }
        });

        // Editar
        $('#tblInstrumentos').on('click', '.btn-editar', async function () {
            const id = this.getAttribute('data-id');
            try {
                const resp = await fetch(`${URL_HANDLER}?op=obtener&id=${encodeURIComponent(id)}`, { method: "GET", cache: "no-store" });
                const json = await resp.json();
                if (!json.ok || !json.data) { swal.fire('Error', json.message || 'No se pudo obtener el registro.', 'error'); return; }
                cargarFormulario(json.data);
                document.getElementById('mdlCapturaLabel').innerText = 'Editar instrumento monetario';
                new bootstrap.Modal(document.getElementById('mdlCaptura')).show();
            } catch (e) {
                console.error(e);
                swal.fire('Error', 'No se pudo obtener el registro.', 'error');
            }
        });

        // Toggle
        $('#tblInstrumentos').on('click', '.btn-toggle', async function () {
            const id = this.getAttribute('data-id');
            try {
                const resp = await fetch(`${URL_HANDLER}?op=toggle&id=${encodeURIComponent(id)}`, { method: "POST" });
                const json = await resp.json();
                if (!json.ok) { swal.fire('Error', json.message || 'No se pudo actualizar el estatus.', 'error'); return; }
                tabla.ajax.reload(null, false);
            } catch (e) {
                console.error(e);
                swal.fire('Error', 'No se pudo actualizar el estatus.', 'error');
            }
        });

        // Helpers
        function limpiarFormulario() {
            document.getElementById('hidId').value = '';
            document.getElementById('txtDescripcion').value = '';
            document.getElementById('txtClave').value = '';
            document.getElementById('txtImpacto').value = '';
            document.getElementById('txtProbabilidad').value = '';
            document.getElementById('txtNivelRiesgo').value = '';
            document.getElementById('chkActivo').checked = true;
        }
        function cargarFormulario(d) {
            document.getElementById('hidId').value = d.id || '';
            document.getElementById('txtDescripcion').value = d.descripcion || '';
            document.getElementById('txtClave').value = d.clave || '';
            document.getElementById('txtImpacto').value = d.impacto ?? '';
            document.getElementById('txtProbabilidad').value = (d.probabilidad ?? d.ocurrencia) ?? '';
            document.getElementById('txtNivelRiesgo').value = d.nivel_riesgo_pld ?? '';
            document.getElementById('chkActivo').checked = (parseInt(d.activo ?? d.estatus ?? 0, 10) === 1);
        }
        function toInt(v) {
            const s = (v || '').replace(',', '.').trim();
            const n = Number(s);
            return (Number.isInteger(n)) ? n : NaN;
        }
        function toDecimal2(v) {
            const s = (v || '').replace(',', '.').trim();
            const n = Number(s);
            return isNaN(n) ? NaN : Math.round(n * 100) / 100;
        }
    });
    </script>
</asp:Content>
