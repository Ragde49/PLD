<%@ Page Title="Catálogo Tasa TIE" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_tasa_tie.aspx.vb" Inherits="PLD.catalogo_tasa_tie" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4 class="mb-3">Catálogo de Tasa TIE</h4>

        <!-- Card header azul + botón Capturar -->
        <div class="card mb-3">
            <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                <div><i class="fas fa-flag"></i> Lista de tasas</div>
                <div>
                    <button type="button" id="btnCapturar" class="btn btn-sm btn-success">
                        <i class="fas fa-plus"></i> Capturar
                    </button>
                </div>
            </div>

            <div class="card-body">
                <!-- Aquí eliminé los controles duplicados. DataTables proveerá Show / Search / Pagination -->
                <div class="table-responsive">
                    <table id="tblCatalogo" class="table table-sm table-striped table-hover w-100">
                        <thead class="table-light">
                            <tr>
                                <th style="width:70px">ID</th>
                                <th>Tasa</th>
                                <th>Fecha</th>
                                <th style="width:120px">Activo</th>
                                <th style="width:120px">Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Se llena por JS -->
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="card-footer text-muted small">
                Consulta de catálogo Tasa TIE
            </div>
        </div>
    </div>

    <!-- Modal Captura / Edición -->
    <div class="modal fade" id="modalCaptura" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-md modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-light">
                    <h5 class="modal-title" id="modalTitle">Capturar tasa</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hidId" value="" />
                    <div class="mb-3">
                        <label for="txtTasa" class="form-label">Tasa de interés*</label>
                        <input type="text" id="txtTasa" class="form-control" placeholder="Ej. 5.00" />
                    </div>
                    <div class="mb-3">
                        <label for="txtFecha" class="form-label">Fecha*</label>
                        <input type="text" id="txtFecha" class="form-control" placeholder="YYYY-MM-DD" readonly />
                    </div>
                    <div class="mb-3 form-check">
                        <input type="checkbox" id="chkEstatus" class="form-check-input" checked />
                        <label for="chkEstatus" class="form-check-label">Activo</label>
                    </div>
                    <div class="small text-muted">Campos con * obligatorios.</div>
                </div>
                <div class="modal-footer">
                    <button type="button" id="btnGuardar" class="btn btn-primary"><i class="fas fa-check"></i> Guardar información</button>
                    <button type="button" id="btnCancelar" class="btn btn-secondary" data-bs-dismiss="modal"><i class="fas fa-times"></i> Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- JavaScript: DOMContentLoaded -->
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            // Variables
            const handlerUrl = "/handlers/handler_catalogo_tasa_tie.ashx";
            let dt = null;

            // Inicializar daterangepicker en formato YYYY-MM-DD (single date)
            if (typeof $ !== "undefined" && $.fn.daterangepicker) {
                $('#txtFecha').daterangepicker({
                    singleDatePicker: true,
                    showDropdowns: true,
                    locale: { format: 'YYYY-MM-DD' }
                });
            }

            // Inicializar DataTable (usar controles por defecto: show/search/pagination)
            function initDataTable() {
                if (dt) {
                    dt.destroy();
                }

                dt = $('#tblCatalogo').DataTable({
                    paging: true,
                    searching: true,
                    info: true,
                    pageLength: 25,
                    lengthChange: true,
                    columnDefs: [
                        { orderable: false, targets: [4] } // Acciones no ordenable
                    ],
                    order: [[2, 'desc']],
                    language: (typeof $.fn.DataTable !== 'undefined' && $.fn.DataTable) ? {
                        url: '/scripts/datatables/spanish.json' // si existe
                    } : {}
                });
            }

            // Cargar datos desde handler -> llenar tabla
            async function cargarDatos() {
                try {
                    const res = await fetch(handlerUrl + '?op=consultar', { cache: 'no-store' });
                    if (!res.ok) throw new Error('Error en la respuesta del servidor.');
                    const json = await res.json();
                    if (!json.success) throw new Error(json.message || 'Respuesta sin éxito.');

                    // Vaciar tbody y poblar
                    const tbody = document.querySelector('#tblCatalogo tbody');
                    tbody.innerHTML = '';

                    json.data.forEach(item => {
                        const tr = document.createElement('tr');

                        const tasaText = (item.tasa !== null && item.tasa !== undefined) ? parseFloat(item.tasa).toFixed(2) : '';
                        const fechaText = item.fecha || '';

                        tr.innerHTML = `
                            <td class="align-middle">${item.id}</td>
                            <td class="align-middle"><span class="badge bg-info">${tasaText}</span></td>
                            <td class="align-middle">${fechaText}</td>
                            <td class="align-middle text-center">
                                ${(item.estatus) ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'}
                            </td>
                            <td class="align-middle text-center">
                                <button type="button" class="btn btn-sm btn-info btn-edit" data-id="${item.id}" title="Editar"><i class="fas fa-edit"></i></button>
                                <button type="button" class="btn btn-sm btn-danger btn-delete" data-id="${item.id}" title="Eliminar"><i class="fas fa-trash"></i></button>
                            </td>
                        `;
                        tbody.appendChild(tr);
                    });

                    // (Re)inicializar DataTable
                    if ($ && $.fn.DataTable) {
                        initDataTable();
                    }
                } catch (err) {
                    console.error(err);
                    swal.fire('Error', err.message || 'Error cargando datos', 'error');
                }
            }

            // Abrir modal para captura nueva
            function abrirCaptura() {
                document.getElementById('hidId').value = '';
                document.getElementById('txtTasa').value = '';
                if (typeof $ !== "undefined" && $.fn.daterangepicker) {
                    $('#txtFecha').data('daterangepicker').setStartDate(moment().format('YYYY-MM-DD'));
                } else {
                    document.getElementById('txtFecha').value = '';
                }
                document.getElementById('chkEstatus').checked = true;
                document.getElementById('modalTitle').innerText = 'Capturar tasa';
                var modalEl = document.getElementById('modalCaptura');
                var bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
                bsModal.show();
            }

            // Obtener registro para editar
            async function obtenerYEditar(id) {
                try {
                    const res = await fetch(handlerUrl + '?op=obtener&id=' + encodeURIComponent(id), { cache: 'no-store' });
                    if (!res.ok) throw new Error('Error en la respuesta del servidor.');
                    const json = await res.json();
                    if (!json.success) throw new Error(json.message || 'Registro no encontrado.');

                    const item = json.data;
                    document.getElementById('hidId').value = item.id;
                    document.getElementById('txtTasa').value = (item.tasa !== null && item.tasa !== undefined) ? parseFloat(item.tasa).toFixed(2) : '';
                    if (typeof $ !== "undefined" && $.fn.daterangepicker && item.fecha) {
                        $('#txtFecha').data('daterangepicker').setStartDate(item.fecha);
                    } else {
                        document.getElementById('txtFecha').value = item.fecha || '';
                    }
                    document.getElementById('chkEstatus').checked = item.estatus === true || item.estatus === 1;

                    document.getElementById('modalTitle').innerText = 'Editar tasa';
                    var modalEl = document.getElementById('modalCaptura');
                    var bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
                    bsModal.show();
                } catch (err) {
                    console.error(err);
                    swal.fire('Error', err.message || 'No se pudo obtener el registro', 'error');
                }
            }

            // Guardar o actualizar
            async function guardarRegistro() {
                const id = document.getElementById('hidId').value;
                const tasaStr = document.getElementById('txtTasa').value.trim().replace(',', '.');
                const fechaStr = document.getElementById('txtFecha').value.trim();
                const estatus = document.getElementById('chkEstatus').checked;

                // Validaciones frontend
                if (!tasaStr) {
                    swal.fire('Atención', 'Capture la tasa.', 'warning');
                    return;
                }
                const tasaNum = parseFloat(tasaStr);
                if (isNaN(tasaNum)) {
                    swal.fire('Atención', 'La tasa debe ser un número válido.', 'warning');
                    return;
                }
                if (!fechaStr) {
                    swal.fire('Atención', 'Capture la fecha.', 'warning');
                    return;
                }

                try {
                    const form = new URLSearchParams();
                    form.append('tasa', tasaNum.toFixed(2));
                    form.append('fecha', fechaStr);
                    form.append('estatus', estatus ? 'true' : 'false');

                    let url = handlerUrl + '?op=guardar';
                    if (id && id !== '') {
                        url = handlerUrl + '?op=actualizar';
                        form.append('id', id);
                    }

                    const res = await fetch(url, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                        body: form.toString()
                    });

                    if (!res.ok) throw new Error('Error en la respuesta del servidor.');
                    const json = await res.json();
                    if (!json.success) throw new Error(json.message || 'Error guardando registro.');

                    // cerrar modal
                    var modalEl = document.getElementById('modalCaptura');
                    var bsModal = bootstrap.Modal.getInstance(modalEl);
                    if (bsModal) bsModal.hide();

                    swal.fire('Correcto', json.message || 'Guardado correctamente.', 'success');
                    cargarDatos();
                } catch (err) {
                    console.error(err);
                    swal.fire('Error', err.message || 'Error al guardar', 'error');
                }
            }

            // Eliminar con confirmación (borrado físico — handler hace DELETE)
            async function eliminarRegistro(id) {
                swal.fire({
                    title: '¿Eliminar?',
                    text: 'Esta acción eliminará el registro. ¿Desea continuar?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, eliminar',
                    cancelButtonText: 'Cancelar'
                }).then(async (result) => {
                    if (!result.isConfirmed) return;
                    try {
                        const res = await fetch(handlerUrl + '?op=eliminar&id=' + encodeURIComponent(id), {
                            method: 'POST',
                            cache: 'no-store'
                        });
                        if (!res.ok) throw new Error('Error en la respuesta del servidor.');
                        const json = await res.json();
                        if (!json.success) throw new Error(json.message || 'No se pudo eliminar.');

                        swal.fire('Eliminado', json.message || 'Registro eliminado.', 'success');
                        cargarDatos();
                    } catch (err) {
                        console.error(err);
                        swal.fire('Error', err.message || 'No fue posible eliminar', 'error');
                    }
                });
            }

            // Delegación de eventos para botones de acción en la tabla
            document.querySelector('#tblCatalogo tbody').addEventListener('click', function (e) {
                const btn = e.target.closest('button');
                if (!btn) return;
                if (btn.classList.contains('btn-edit')) {
                    const id = btn.getAttribute('data-id');
                    obtenerYEditar(id);
                } else if (btn.classList.contains('btn-delete')) {
                    const id = btn.getAttribute('data-id');
                    eliminarRegistro(id);
                }
            });

            // Botones superiores
            document.getElementById('btnCapturar').addEventListener('click', function () {
                abrirCaptura();
            });

            document.getElementById('btnGuardar').addEventListener('click', function () {
                guardarRegistro();
            });

            // Inicial carga
            cargarDatos();
        });
    </script>
</asp:Content>
