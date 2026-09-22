<%@ Page Title="Canal pago" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_canal_pago.aspx.vb" Inherits="PLD.catalogo_canal_pago" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4>Catálogo: Canal de pago</h4>

        <div class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center" style="background-color:#0d6efd; color:white;">
                <div><strong>Canales de pago</strong></div>
                <div>
                    <button type="button" id="btnNuevo" class="btn btn-success btn-sm">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
            </div>
            <div class="card-body p-2">
                <div class="table-responsive">
                    <table id="tblCanales" class="table table-sm table-striped w-100">
                        <thead>
                            <tr>
                                <th style="width:60px">ID</th>
                                <th>Descripción</th>
                                <th style="width:90px">Impacto</th>
                                <th style="width:120px">Ocurrencia</th>
                                <th style="width:130px">Nivel Riesgo P.L.D.</th>
                                <th style="width:110px">Estatus</th>
                                <th style="width:150px">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- renderizado por JS -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- Modal Alta / Edit -->
        <div class="modal fade" id="modalCaptura" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 id="modalTitle" class="modal-title">Capturar canal</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form id="frmCanal" onsubmit="return false;">
                            <input type="hidden" id="hdId" value="0" />

                            <div class="row">
                                <div class="col-md-8">
                                    <div class="mb-2">
                                        <label class="form-label">Descripción *</label>
                                        <input id="txtDescripcion" class="form-control" type="text" maxlength="150" required />
                                    </div>
                                </div>

                                <div class="col-md-4">
                                    <div class="mb-2">
                                        <label class="form-label">Estatus</label><br />
                                        <input id="chkActivo" type="checkbox" checked />
                                        <label for="chkActivo" class="ms-1">Activo</label>
                                    </div>
                                </div>
                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-md-3">
                                    <label class="form-label">Impacto *</label>
                                    <input id="numImpacto" class="form-control" type="number" step="any" min="0" required />
                                    <small class="form-text text-muted">Valor numérico (ej. 25)</small>
                                </div>

                                <div class="col-md-3">
                                    <label class="form-label">Probabilidad (%) *</label>
                                    <input id="numProbabilidad" class="form-control" type="number" step="any" min="0" max="100" required />
                                    <small class="form-text text-muted">Ej. 30 → 30%</small>
                                </div>

                                <div class="col-md-3">
                                    <label class="form-label">Nivel Riesgo P.L.D.</label>
                                    <input id="numNivelRiesgo" class="form-control" type="text" readonly />
                                    <small class="form-text text-muted">Calculado automáticamente</small>
                                </div>

                                <div class="col-md-3">
                                    <label class="form-label">Impacto (texto opcional)</label>
                                    <select id="selImpactoTexto" class="form-control">
                                        <option value="">-- Opcional --</option>
                                        <option value="BAJO">BAJO</option>
                                        <option value="MEDIO">MEDIO</option>
                                        <option value="ALTO">ALTO</option>
                                    </select>
                                    <small class="form-text text-muted">Si eliges BAJO/MEDIO/ALTO se convertirá a número antes de enviar</small>
                                </div>
                            </div>

                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" id="btnGuardar" class="btn btn-primary">
                            <i class="fa fa-check"></i> Guardar información
                        </button>
                        <button type="button" id="btnCancelar" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fa fa-times"></i> Cancelar
                        </button>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const handlerUrl = '/handlers/handler_catalogo_canal_pago.ashx';
            let dataTable = null;
            const tableBody = document.querySelector('#tblCanales tbody');

            // modalInstance global
            let modalInstance = null;

            // Helpers
            function riesgoTextoANumero(valor) {
                if (!valor) return 0;
                switch (String(valor).toUpperCase()) {
                    case "BAJO": return 1;
                    case "MEDIO": return 2;
                    case "ALTO": return 3;
                    default:
                        const num = parseFloat(String(valor).replace(",", "."));
                        return isNaN(num) ? 0 : num;
                }
            }

            function calcularNivelRiesgo(impacto, prob) {
                impacto = parseFloat(impacto) || 0;
                prob = parseFloat(prob) || 0;
                const nivel = impacto * (prob / 100);
                return isNaN(nivel) ? 0 : parseFloat(nivel.toFixed(2));
            }

            function showToastOk(titulo, texto) {
                if (typeof swal === 'undefined' || !swal.fire) {
                    alert(titulo + "\n" + texto);
                    return;
                }
                swal.fire(titulo, texto, 'success');
            }
            function showError(titulo, texto) {
                if (typeof swal === 'undefined' || !swal.fire) {
                    alert(titulo + "\n" + texto);
                    return;
                }
                swal.fire({ icon: 'error', title: titulo, text: texto });
            }
            function showConfirm(titulo, texto) {
                return swal.fire({
                    title: titulo,
                    text: texto,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí',
                    cancelButtonText: 'Cancelar'
                });
            }

            // Abrir modal nuevo
            document.getElementById('btnNuevo').addEventListener('click', function () {
                document.getElementById('modalTitle').innerText = 'Capturar canal';
                document.getElementById('hdId').value = 0;
                document.getElementById('txtDescripcion').value = '';
                document.getElementById('chkActivo').checked = true;
                document.getElementById('numImpacto').value = '';
                document.getElementById('numProbabilidad').value = '';
                document.getElementById('numNivelRiesgo').value = '';
                document.getElementById('selImpactoTexto').value = '';

                modalInstance = new bootstrap.Modal(document.getElementById('modalCaptura'));
                modalInstance.show();
            });

            // Inputs nivel
            document.getElementById('numImpacto').addEventListener('input', actualizarNivel);
            document.getElementById('numProbabilidad').addEventListener('input', actualizarNivel);
            document.getElementById('selImpactoTexto').addEventListener('change', function () {
                const texto = this.value;
                if (texto) {
                    const val = riesgoTextoANumero(texto);
                    document.getElementById('numImpacto').value = val;
                }
                actualizarNivel();
            });
            function actualizarNivel() {
                const impacto = document.getElementById('numImpacto').value;
                const prob = document.getElementById('numProbabilidad').value;
                document.getElementById('numNivelRiesgo').value = calcularNivelRiesgo(impacto, prob);
            }

            // ================= cargarDatos =================
            async function cargarDatos() {
                try {
                    const res = await fetch(handlerUrl + '?op=consultar', { method: 'GET', credentials: 'same-origin' });
                    if (!res.ok) {
                        const txt = await res.text();
                        console.error('Respuesta HTTP no ok:', res.status, txt);
                        showError('Error', 'Respuesta del servidor: ' + res.status);
                        throw new Error('HTTP ' + res.status);
                    }

                    const text = await res.text();
                    console.log('Respuesta cruda handler (consultar):', text);

                    let payload;
                    try {
                        payload = JSON.parse(text);
                    } catch (err) {
                        console.error('No es JSON válido:', err);
                        showError('Error', 'El servidor no devolvió JSON válido. Revisa el handler.');
                        throw err;
                    }

                    // Normalizar a array de objetos
                    let data = [];
                    if (Array.isArray(payload)) {
                        data = payload;
                    } else if (payload && (payload.ok === true || payload.ok === "true") && Array.isArray(payload.data)) {
                        data = payload.data;
                    } else if (payload && (payload.ok === false || payload.ok === "false")) {
                        const msg = payload.mensaje || payload.message || 'El handler devolvió error.';
                        showError('Error del servidor', msg);
                        throw new Error(msg);
                    } else if (payload && typeof payload === 'object') {
                        // intentar convertir si es objeto indexado numéricamente
                        const keys = Object.keys(payload);
                        if (keys.length > 0 && keys.every(k => !isNaN(parseInt(k)))) {
                            data = keys.map(k => payload[k]);
                        } else {
                            showError('Respuesta inesperada', 'El servidor devolvió un objeto que no es un listado.');
                            throw new Error('Payload inesperado');
                        }
                    } else {
                        showError('Error', 'Respuesta inesperada del servidor.');
                        throw new Error('Payload desconocido');
                    }

                    // Preparar filas para DataTable (array de arrays)
                    const rowsData = data.map(item => {
                        const accionesHtml =
                            '<button type="button" class="btn btn-sm btn-outline-primary btn-ver me-1" data-id="' + (item.id || '') + '" title="Ver"><i class="fa fa-search"></i></button>' +
                            '<button type="button" class="btn btn-sm btn-outline-warning btn-editar me-1" data-id="' + (item.id || '') + '" title="Editar"><i class="fa fa-pencil"></i></button>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger btn-borrar" data-id="' + (item.id || '') + '" title="Dar de baja"><i class="fa fa-trash"></i></button>';

                        const activoBadge = (item.activo === true || item.activo === 1 || item.activo === '1')
                            ? '<span class="badge bg-success">Activo</span>'
                            : '<span class="badge bg-secondary">Inactivo</span>';

                        return [
                            item.id || '',
                            item.canal || '',
                            (item.impacto != null) ? item.impacto : '',
                            (item.probabilidad != null) ? (item.probabilidad + '%') : '',
                            (item.nivel_riesgo_pld != null) ? (parseFloat(item.nivel_riesgo_pld).toFixed(2)) : '',
                            activoBadge,
                            accionesHtml
                        ];
                    });

                    // Si ya existe DataTable -> actualizar rows usando API (clear + rows.add + draw)
                    if (dataTable && $.fn.dataTable && $.fn.dataTable.isDataTable('#tblCanales')) {
                        try {
                            dataTable.clear();
                            dataTable.rows.add(rowsData);
                            dataTable.draw(false); // false = no resetear paging
                            return true;
                        } catch (errUpdate) {
                            console.warn('Error actualizando DataTable, fallback a recrear la tabla:', errUpdate);
                            try { dataTable.destroy(); } catch (eDestroy) { console.warn('Destroy fallback falló', eDestroy); }
                            dataTable = null;
                            // continuar para crear nueva instancia abajo
                        }
                    }

                    // No hay instancia previa -> crear DataTable con data
                    try {
                        // vaciar tbody (por seguridad)
                        tableBody.innerHTML = '';

                        // inicializar con data
                        dataTable = $('#tblCanales').DataTable({
                            data: rowsData,
                            columns: [
                                { title: "ID" },
                                { title: "Descripción" },
                                { title: "Impacto", className: "text-center" },
                                { title: "Ocurrencia", className: "text-center" },
                                { title: "Nivel Riesgo P.L.D.", className: "text-center" },
                                { title: "Estatus", className: "text-center" },
                                { title: "Acciones", orderable: false, className: "text-center" }
                            ],
                            "pageLength": 25,
                            "language": { "url": "/scripts/datatables/spanish.json" },
                            "order": [[0, "asc"]],
                            "autoWidth": false,
                            "responsive": true
                        });

                        dataTable.draw(false);
                        return true;
                    } catch (errInit) {
                        console.error('Error inicializando DataTable:', errInit);
                        showError('Error', 'No fue posible inicializar la tabla.');
                        throw errInit;
                    }

                } catch (err) {
                    console.error('cargarDatos fallo:', err);
                    throw err;
                }
            } // fin cargarDatos

            // ================= Guardar (nuevo/editar) =================
            document.getElementById('btnGuardar').addEventListener('click', async function () {
                const id = parseInt(document.getElementById('hdId').value || 0);
                const canal = document.getElementById('txtDescripcion').value.trim();
                if (!canal) { showError('Validación', 'La descripción es requerida'); return; }

                let impactoRaw = document.getElementById('numImpacto').value;
                const impactoTexto = document.getElementById('selImpactoTexto').value;
                if (impactoTexto) impactoRaw = riesgoTextoANumero(impactoTexto);

                const impacto = parseFloat(String(impactoRaw).replace(',', '.')) || 0;
                const prob = parseFloat(String(document.getElementById('numProbabilidad').value).replace(',', '.')) || 0;
                const nivel = calcularNivelRiesgo(impacto, prob);
                const activo = document.getElementById('chkActivo').checked ? 1 : 0;

                const payload = new URLSearchParams();
                payload.append('op', id === 0 ? 'guardar' : 'editar');
                payload.append('id', id);
                payload.append('canal', canal);
                payload.append('impacto', impacto);
                payload.append('probabilidad', prob);
                payload.append('nivel_riesgo_pld', nivel);
                payload.append('activo', activo);

                try {
                    const res = await fetch(handlerUrl, {
                        method: 'POST',
                        credentials: 'same-origin',
                        body: payload,
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                    });
                    if (!res.ok) {
                        const txt = await res.text();
                        console.error('Guardar HTTP no ok:', res.status, txt);
                        showError('Error', 'Respuesta del servidor: ' + res.status);
                        return;
                    }
                    const obj = await res.json();
                    if (obj.ok) {
                        // recargar datos y esperar que termine
                        try {
                            await cargarDatos();
                        } catch (err) {
                            console.warn('La recarga devolvió error:', err);
                        }

                        // Cerrar modal
                        if (modalInstance) {
                            try { modalInstance.hide(); } catch (e) { console.warn('modal hide fallo', e); }
                        } else {
                            const modalEl = document.getElementById('modalCaptura');
                            try { bootstrap.Modal.getInstance(modalEl)?.hide(); } catch (e) { }
                        }

                        showToastOk('Éxito', obj.mensaje || 'Registro guardado');
                    } else {
                        showError('Error', obj.mensaje || 'No fue posible guardar.');
                    }
                } catch (err) {
                    console.error('Error en guardar:', err);
                    showError('Error', err.message || err);
                }
            });

            // ================= Delegación botones tabla =================
            document.getElementById('tblCanales').addEventListener('click', async function (e) {
                const btn = e.target.closest('button');
                if (!btn) return;
                const id = btn.getAttribute('data-id');
                if (!id) return;

                if (btn.classList.contains('btn-ver')) {
                    await verRegistro(id);
                } else if (btn.classList.contains('btn-editar')) {
                    await editarRegistro(id);
                } else if (btn.classList.contains('btn-borrar')) {
                    const conf = await showConfirm('Confirmar', '¿Deseas dar de baja este canal?');
                    if (conf.isConfirmed) {
                        await desactivarRegistro(id);
                    }
                }
            });

            // ================= verRegistro =================
            async function verRegistro(id) {
                try {
                    const res = await fetch(handlerUrl + '?op=getbyid&id=' + encodeURIComponent(id), { method: 'GET', credentials: 'same-origin' });
                    if (!res.ok) throw new Error('Error en la respuesta del servidor');
                    const obj = await res.json();
                    if (!obj.ok) { showError('Error', obj.mensaje || 'Registro no encontrado'); return; }
                    const item = obj.data;

                    document.getElementById('modalTitle').innerText = 'Ver canal';
                    document.getElementById('hdId').value = item.id;
                    document.getElementById('txtDescripcion').value = item.canal || '';
                    document.getElementById('chkActivo').checked = (item.activo === true || item.activo === 1);
                    document.getElementById('numImpacto').value = item.impacto != null ? item.impacto : '';
                    document.getElementById('numProbabilidad').value = item.probabilidad != null ? item.probabilidad : '';
                    document.getElementById('numNivelRiesgo').value = item.nivel_riesgo_pld != null ? parseFloat(item.nivel_riesgo_pld).toFixed(2) : '';
                    document.getElementById('selImpactoTexto').value = '';

                    // abrir modal y dejar inputs deshabilitados
                    modalInstance = new bootstrap.Modal(document.getElementById('modalCaptura'));
                    document.querySelectorAll('#frmCanal input, #frmCanal select, #btnGuardar').forEach(el => el.disabled = true);
                    modalInstance.show();

                    // al cerrar re-habilitar
                    var modalEl = document.getElementById('modalCaptura');
                    modalEl.addEventListener('hidden.bs.modal', function () {
                        document.querySelectorAll('#frmCanal input, #frmCanal select, #btnGuardar').forEach(el => el.disabled = false);
                    }, { once: true });

                } catch (err) {
                    console.error('verRegistro error:', err);
                    showError('Error', err.message || err);
                }
            }

            // ================= editarRegistro =================
            async function editarRegistro(id) {
                try {
                    const res = await fetch(handlerUrl + '?op=getbyid&id=' + encodeURIComponent(id), { method: 'GET', credentials: 'same-origin' });
                    if (!res.ok) throw new Error('Error en la respuesta del servidor');
                    const obj = await res.json();
                    if (!obj.ok) { showError('Error', obj.mensaje || 'Registro no encontrado'); return; }
                    const item = obj.data;

                    document.getElementById('modalTitle').innerText = 'Editar canal';
                    document.getElementById('hdId').value = item.id;
                    document.getElementById('txtDescripcion').value = item.canal || '';
                    document.getElementById('chkActivo').checked = (item.activo === true || item.activo === 1);
                    document.getElementById('numImpacto').value = item.impacto != null ? item.impacto : '';
                    document.getElementById('numProbabilidad').value = item.probabilidad != null ? item.probabilidad : '';
                    document.getElementById('numNivelRiesgo').value = item.nivel_riesgo_pld != null ? parseFloat(item.nivel_riesgo_pld).toFixed(2) : '';
                    document.getElementById('selImpactoTexto').value = '';

                    modalInstance = new bootstrap.Modal(document.getElementById('modalCaptura'));
                    modalInstance.show();

                } catch (err) {
                    console.error('editarRegistro error:', err);
                    showError('Error', err.message || err);
                }
            }

            // ================= desactivarRegistro =================
            async function desactivarRegistro(id) {
                try {
                    const payload = new URLSearchParams();
                    payload.append('op', 'desactivar');
                    payload.append('id', id);

                    const res = await fetch(handlerUrl, {
                        method: 'POST',
                        credentials: 'same-origin',
                        body: payload,
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                    });
                    if (!res.ok) {
                        const txt = await res.text();
                        console.error('Desactivar HTTP no ok:', res.status, txt);
                        showError('Error', 'Respuesta del servidor: ' + res.status);
                        return;
                    }
                    const obj = await res.json();
                    if (obj.ok) {
                        try { await cargarDatos(); } catch (e) { console.warn('recarga tras desactivar fallo', e); }
                        showToastOk('Éxito', obj.mensaje || 'Registro dado de baja');
                    } else {
                        showError('Error', obj.mensaje || 'No fue posible dar de baja');
                    }
                } catch (err) {
                    console.error('desactivarRegistro error:', err);
                    showError('Error', err.message || err);
                }
            }

            // Inicio: cargar datos (maneja errores en consola)
            cargarDatos().catch(err => console.warn('Carga inicial falló:', err));
        });
    </script>
</asp:Content>
