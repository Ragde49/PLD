<%@ Page Title="Catálogo Propietario Real" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_propietario_real.aspx.vb" Inherits="PLD.catalogo_propietario_real" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <!-- Card principal con header azul -->
        <div class="card">
            <div class="card-header bg-primary text-white d-flex align-items-center justify-content-between">
                <div class="d-flex align-items-center">
                    <i class="fas fa-list me-2"></i>
                    <strong>Catálogo - Propietario Real</strong>
                </div>
                <div>
                    <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
                        <i class="fas fa-plus"></i> Capturar
                    </button>
                </div>
            </div>

            <div class="card-body p-2">
                <!-- Tabla (DataTables) -->
                <div class="table-responsive">
                    <table id="tblCatalogo" class="table table-sm table-striped table-hover w-100">
                        <thead class="table-light">
                            <tr>
                                <th>ID</th>
                                <th>Descripción</th>
                                <th>Impacto</th>
                                <th>Ocurrencia</th>
                                <th>Nivel Riesgo P.L.D</th>
                                <th>Estatus</th>
                                <th>Mitigantes</th>
                                <th style="width:120px">Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal captura/edición -->
    <div class="modal fade" id="modalCatalogo" tabindex="-1" aria-labelledby="modalCatalogoLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="modalCatalogoLabel" class="modal-title">Captura Propietario Real</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <form id="formCatalogo" autocomplete="off" novalidate>
                        <input type="hidden" id="hdId" value="0" />

                        <div class="mb-3 row">
                            <label class="col-sm-3 col-form-label">Descripción*</label>
                            <div class="col-sm-9">
                                <input type="text" id="txtDescripcion" class="form-control" maxlength="150" />
                            </div>
                        </div>

                        <div class="mb-3 row">
                            <label class="col-sm-2 col-form-label">Impacto*</label>
                            <div class="col-sm-4">
                                <input type="text" id="txtImpacto" class="form-control" placeholder="ej. 50 o BAJO" />
                            </div>

                            <label class="col-sm-2 col-form-label">Probabilidad*</label>
                            <div class="col-sm-4">
                                <input type="text" id="txtProbabilidad" class="form-control" placeholder="ej. 20 o MEDIO" />
                            </div>
                        </div>

                        <div class="mb-3 row">
                            <label class="col-sm-3 col-form-label">Nivel de riesgo P.L.D.*</label>
                            <div class="col-sm-3">
                                <input type="text" id="txtNivelRiesgo" class="form-control" placeholder="ej. 10.00 (opcional)" />
                                <small class="form-text text-muted">Si queda vacío se calcula automáticamente.</small>
                            </div>

                            <label class="col-sm-3 col-form-label">Estatus</label>
                            <div class="col-sm-3 d-flex align-items-center">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="chkActivo" checked />
                                    <label class="form-check-label" for="chkActivo">Activo</label>
                                </div>
                            </div>
                        </div>

                        <div class="text-muted small">
                            Nota: los campos de riesgo deben ser numéricos o los valores BAJO/MEDIO/ALTO (se mapearán a números).
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" id="btnGuardar" class="btn btn-primary">
                        <i class="fas fa-check"></i> Guardar información
                    </button>
                    <button type="button" id="btnCancelarModal" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times"></i> Cancelar
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- JavaScript (mismo que acordamos: DataTables + fetch hacia handler) -->
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            const handlerUrl = "/handlers/handler_catalogo_propietario_real.ashx";
            let dt = null;
            const modalEl = document.getElementById('modalCatalogo');
            const bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });

            function riesgoTextoANumero(valor) {
                if (valor === null || valor === undefined) return 0;
                const v = String(valor).trim();
                if (v.length === 0) return 0;
                switch (v.toUpperCase()) {
                    case "BAJO": return 1;
                    case "MEDIO": return 2;
                    case "ALTO": return 3;
                    default:
                        const n = parseFloat(v.replace(",", "."));
                        return isNaN(n) ? 0 : n;
                }
            }

            function mostrarAlerta(icon, title, text) {
                swal.fire({ icon: icon, title: title, text: text });
            }

            function cargarDatos() {
                fetch(`${handlerUrl}?op=consultar`, { method: 'GET', credentials: 'same-origin' })
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp || resp.success === false) {
                            mostrarAlerta('error', 'Error', resp && resp.message ? resp.message : 'Error al cargar datos');
                            return;
                        }
                        const data = resp.data || [];

                        data.forEach(d => { if (d.mitigantes === undefined) d.mitigantes = 0; });

                        if (dt) {
                            dt.clear();
                            dt.rows.add(data);
                            dt.draw();
                        } else {
                            dt = $('#tblCatalogo').DataTable({
                                data: data,
                                pageLength: 25,
                                lengthMenu: [[10,25,50,100],[10,25,50,100]],
                                columns: [
                                    { data: 'id', width: '50px' },
                                    { data: 'descripcion' },
                                    { data: 'impacto', className: 'text-center', width: '80px' },
                                    {
                                        data: 'probabilidad',
                                        className: 'text-center',
                                        width: '90px',
                                        render: function (data) { var p = parseFloat(data) || 0; return p.toString().replace('.', ',') + '%'; }
                                    },
                                    {
                                        data: 'nivel_riesgo_pld',
                                        className: 'text-center',
                                        width: '110px',
                                        render: function (data) { var n = parseFloat(data) || 0; return n.toFixed(2); }
                                    },
                                    {
                                        data: 'activo',
                                        className: 'text-center',
                                        width: '90px',
                                        render: function (data) {
                                            if (data === true || data == 1) return '<span class="badge bg-success">Activo</span>';
                                            return '<span class="badge bg-secondary">Inactivo</span>';
                                        }
                                    },
                                    {
                                        data: 'mitigantes',
                                        className: 'text-center',
                                        width: '80px',
                                        render: function (data) { var n = parseInt(data) || 0; return '<span class="badge rounded-pill bg-info text-dark">' + n + '</span>'; }
                                    },
                                    {
                                        data: null,
                                        orderable: false,
                                        searchable: false,
                                        className: 'text-center',
                                        render: function (data, type, row) {
                                            return '' +
                                                '<button type="button" class="btn btn-sm btn-outline-primary btn-edit me-1" data-id="' + row.id + '" title="Editar"><i class="fas fa-edit"></i></button>' +
                                                '<button type="button" class="btn btn-sm btn-outline-danger btn-delete" data-id="' + row.id + '" title="Eliminar"><i class="fas fa-trash-alt"></i></button>';
                                        }
                                    }
                                ],
                                order: [[1, 'asc']],
                                responsive: true,
                                language: {
                                    search: "Search:",
                                    lengthMenu: "Show _MENU_ entries",
                                    paginate: { previous: "«", next: "»" },
                                    info: "Mostrando _START_ a _END_ de _TOTAL_ registros"
                                }
                            });
                        }
                    })
                    .catch(err => { console.error(err); mostrarAlerta('error', 'Error', 'Error en la respuesta del servidor'); });
            }

            function abrirModalNuevo() {
                document.getElementById('hdId').value = "0";
                document.getElementById('txtDescripcion').value = "";
                document.getElementById('txtImpacto').value = "";
                document.getElementById('txtProbabilidad').value = "";
                document.getElementById('txtNivelRiesgo').value = "";
                document.getElementById('chkActivo').checked = true;
                bsModal.show();
            }

            function abrirModalEditar(id) {
                fetch(`${handlerUrl}?op=obtener&id=${encodeURIComponent(id)}`, { method: 'GET', credentials: 'same-origin' })
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp || resp.success === false) { mostrarAlerta('error', 'Error', resp && resp.message ? resp.message : 'No se pudo obtener registro'); return; }
                        const d = resp.data;
                        document.getElementById('hdId').value = d.id || 0;
                        document.getElementById('txtDescripcion').value = d.descripcion || '';
                        document.getElementById('txtImpacto').value = (d.impacto !== undefined) ? d.impacto : '';
                        document.getElementById('txtProbabilidad').value = (d.probabilidad !== undefined) ? d.probabilidad : '';
                        document.getElementById('txtNivelRiesgo').value = (d.nivel_riesgo_pld !== undefined) ? (parseFloat(d.nivel_riesgo_pld).toFixed(2)) : '';
                        document.getElementById('chkActivo').checked = (d.activo === true || d.activo == 1);
                        bsModal.show();
                    })
                    .catch(err => { console.error(err); mostrarAlerta('error', 'Error', 'Error en la respuesta del servidor'); });
            }

            function guardarOActualizar() {
                const id = parseInt(document.getElementById('hdId').value || "0");
                const descripcion = document.getElementById('txtDescripcion').value.trim();
                const impactoRaw = document.getElementById('txtImpacto').value.trim();
                const probRaw = document.getElementById('txtProbabilidad').value.trim();
                const nivelRaw = document.getElementById('txtNivelRiesgo').value.trim();
                const activo = document.getElementById('chkActivo').checked ? 1 : 0;

                if (!descripcion) { mostrarAlerta('warning', 'Atención', 'La descripción es requerida.'); return; }

                const impacto = Math.max(0, Math.round(riesgoTextoANumero(impactoRaw)));
                const probabilidad = Math.max(0, Math.round(riesgoTextoANumero(probRaw)));
                let nivel;
                if (!nivelRaw) { nivel = (impacto * (probabilidad / 100)); } else { nivel = riesgoTextoANumero(nivelRaw); }
                nivel = parseFloat(nivel.toFixed(2));

                const payload = { descripcion: descripcion, impacto: impacto, probabilidad: probabilidad, nivel_riesgo_pld: nivel, activo: activo };
                const op = (id && id > 0) ? 'actualizar' : 'guardar';
                if (op === 'actualizar') payload.id = id;

                fetch(`${handlerUrl}?op=${op}`, {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                    .then(r => r.json())
                    .then(resp => {
                        if (!resp) { mostrarAlerta('error', 'Error', 'Respuesta inválida del servidor.'); return; }
                        if (resp.success) {
                            bsModal.hide();
                            mostrarAlerta('success', 'Ok', resp.message || 'Operación exitosa');
                            cargarDatos();
                        } else {
                            mostrarAlerta('error', 'Error', resp.message || 'No se pudo guardar');
                        }
                    })
                    .catch(err => { console.error(err); mostrarAlerta('error', 'Error', 'Fallo en la petición al servidor.'); });
            }

            function eliminarRegistro(id) {
                swal.fire({
                    title: 'Confirmar',
                    text: '¿Deseas desactivar este registro?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, desactivar',
                    cancelButtonText: 'Cancelar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        fetch(`${handlerUrl}?op=eliminar`, {
                            method: 'POST',
                            credentials: 'same-origin',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ id: id })
                        })
                            .then(r => r.json())
                            .then(resp => {
                                if (resp && resp.success) {
                                    mostrarAlerta('success', 'Ok', resp.message || 'Registro desactivado');
                                    cargarDatos();
                                } else {
                                    mostrarAlerta('error', 'Error', resp && resp.message ? resp.message : 'No se pudo desactivar');
                                }
                            })
                            .catch(err => { console.error(err); mostrarAlerta('error', 'Error', 'Error en la petición al servidor'); });
                    }
                });
            }

            document.getElementById('btnCapturar').addEventListener('click', function () { abrirModalNuevo(); });
            document.getElementById('btnGuardar').addEventListener('click', function () { guardarOActualizar(); });

            $('#tblCatalogo tbody').on('click', '.btn-edit', function () { const id = this.getAttribute('data-id'); abrirModalEditar(id); });
            $('#tblCatalogo tbody').on('click', '.btn-delete', function () { const id = this.getAttribute('data-id'); eliminarRegistro(id); });

            cargarDatos();
        });
    </script>
</asp:Content>
