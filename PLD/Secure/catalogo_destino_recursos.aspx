<%@ Page Title="Catálogo Destino de Recursos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_destino_recursos.aspx.vb" Inherits="PLD.catalogo_destino_recursos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Estilo para reproducir los botones de acciones como en el catálogo "Créditos" */
        .action-btn {
            width: 34px;
            height: 26px;
            padding: 0;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border-radius: 6px;
            line-height: 1;
        }
        .action-btn svg { width: 14px; height: 14px; }

        .action-edit {
            border: 2px solid #007bff;
            color: #007bff;
            background: #fff;
        }

        .action-delete {
            border: 2px solid #dc3545;
            color: #dc3545;
            background: #fff;
        }

        /* small spacing like reference */
        .action-cell .action-btn { margin-right: 6px; }

        /* badge style for mitigantes like reference (blue rounded) */
        .mitigantes-badge {
            background: #00c0ef;
            color: #fff;
            padding: 6px 8px;
            border-radius: 12px;
            display: inline-block;
            min-width: 28px;
            text-align: center;
            font-weight: 600;
        }

        /* keep table compact like reference */
        .table-sm th, .table-sm td { vertical-align: middle; }
    </style>

    <div class="container-fluid mt-4">
        <h4 class="mb-3">Consulta de catálogo</h4>

        <!-- Header ribbon (formato autorizado similar a 'Créditos') -->
        <div class="mb-2">
            <div class="d-flex align-items-center justify-content-between rounded" style="background:#0d6efd;color:#fff;padding:8px 12px;">
                <div style="font-weight:600;">Destinos</div>
                <div>
                    <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
                        <span style="font-weight:600; font-size:13px;">+ Capturar</span>
                    </button>
                </div>
            </div>
        </div>

        <!-- DataTable container -->
        <div class="card">
            <div class="card-body">
                <div class="table-responsive">
                    <table id="tblDestinos" class="table table-sm table-striped w-100">
                        <thead class="table-light">
                            <tr>
                                <th style="width:60px">ID</th>
                                <th>Descripción</th>
                                <th style="width:100px">Impacto</th>
                                <th style="width:110px">Ocurrencia</th>
                                <th style="width:120px">Nivel Riesgo P.L.D</th>
                                <th style="width:100px">Estatus</th>
                                <th style="width:100px">Mitigantes</th>
                                <th style="width:110px">Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="tblBody"></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal (captura/edición) -->
    <div class="modal fade" id="modalDestino" tabindex="-1" aria-labelledby="modalDestinoLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="modalDestinoLabel" class="modal-title">Capturar destino de recursos</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hdId" value="0" />

                    <div class="mb-3">
                        <label class="form-label">Descripción*</label>
                        <textarea id="txtDescripcion" class="form-control" rows="2" maxlength="250"></textarea>
                    </div>

                    <div class="row">
                        <div class="col-md-4 mb-2">
                            <label class="form-label">Valor*</label>
                            <select id="selValor" class="form-control">
                                <option value="">-- Seleccionar --</option>
                                <option>Si Identifica</option>
                                <option>Identifica Parcialmente</option>
                                <option>No Identifica</option>
                            </select>
                        </div>
                        <div class="col-md-2 mb-2">
                            <label class="form-label">Impacto*</label>
                            <input id="txtImpacto" type="number" class="form-control" min="0" step="1" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label class="form-label">Ocurrencia*</label>
                            <input id="txtOcurrencia" type="text" class="form-control" placeholder="ej. 25%" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label class="form-label">Nivel riesgo P.L.D*</label>
                            <input id="txtNivel" type="number" class="form-control" step="0.01" min="0" />
                        </div>
                    </div>

                    <div class="row align-items-center">
                        <div class="col-md-4 mb-2">
                            <label class="form-label">Mitigantes</label>
                            <input id="txtMitigantes" type="number" class="form-control" min="0" step="1" value="0" />
                        </div>
                        <div class="col-md-4 mb-2">
                            <label class="form-label">Activo</label>
                            <div>
                                <input id="chkActivo" type="checkbox" checked />
                                <label for="chkActivo" class="ms-1">Activo</label>
                            </div>
                        </div>
                        <div class="col-md-4 text-end">
                            <small class="text-muted">Campos obligatorios con *</small>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="button" id="btnGuardar" class="btn btn-primary">Guardar información</button>
                    <button type="button" id="btnCancelar" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const HANDLER = "/handlers/handler_catalogo_destino_recursos.ashx";

            function escapeHtml(s) {
                if (s === null || s === undefined) return "";
                return String(s)
                    .replaceAll('&', '&amp;')
                    .replaceAll('<', '&lt;')
                    .replaceAll('>', '&gt;')
                    .replaceAll('"', '&quot;')
                    .replaceAll("'", '&#039;');
            }

            // botones SVG (plantillas)
            const svgEdit = '<svg viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg" aria-hidden="true"><path d="M12.146.854a.5.5 0 0 1 .708 0l2.292 2.292a.5.5 0 0 1 0 .708l-9.193 9.193a.5.5 0 0 1-.168.11l-4 1.5a.5.5 0 0 1-.65-.65l1.5-4a.5.5 0 0 1 .11-.168l9.193-9.193zM11.207 2L3 10.207V13h2.793L14 4.793 11.207 2z"/></svg>';
            const svgTrash = '<svg viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg" aria-hidden="true"><path d="M5.5 5.5a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0v-6a.5.5 0 0 1 .5-.5zm5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0v-6a.5.5 0 0 1 .5-.5z"/><path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4H2.5a1 1 0 1 1 0-2H5l.5-.5A1 1 0 0 1 6.293 1h3.414a1 1 0 0 1 .793.5L11 2h2.5a1 1 0 0 1 1 1zM5 4v9h6V4H5z"/></svg>';

            // Cargar y renderizar tabla como referencia Créditos
            async function loadAndRender() {
                try {
                    const resp = await fetch(HANDLER + '?op=consultar');
                    const json = await resp.json();
                    if (!json.ok) {
                        await Swal.fire('Error', json.mensaje || 'Error al consultar', 'error');
                        return;
                    }
                    const rows = json.data || [];

                    // destruir DT si existe
                    if ($.fn.dataTable.isDataTable('#tblDestinos')) {
                        $('#tblDestinos').DataTable().clear().destroy();
                    }
                    $('#tblBody').empty();

                    // poblar
                    for (const r of rows) {
                        const estatusText = (r.estatus && r.estatus.toString().trim() !== '') ? r.estatus : (r.activo ? 'Activo' : 'Inactivo');
                        const estatusHtml = (estatusText.toLowerCase().indexOf('activo') >= 0)
                            ? '<span class="badge bg-success">Activo</span>'
                            : '<span class="badge bg-secondary">Inactivo</span>';

                        const mitigantesHtml = `<span class="mitigantes-badge">${r.mitigantes ?? 0}</span>`;

                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td>${r.id}</td>
                            <td>${escapeHtml(r.descripcion)}</td>
                            <td class="text-end">${r.impacto ?? ''}</td>
                            <td>${escapeHtml(r.ocurrencia)}</td>
                            <td class="text-end">${(r.nivel_riesgo_pld !== null && r.nivel_riesgo_pld !== undefined) ? parseFloat(r.nivel_riesgo_pld).toFixed(2) : ''}</td>
                            <td>${estatusHtml}</td>
                            <td class="text-center">${mitigantesHtml}</td>
                            <td class="text-center action-cell">
                                <button type="button" class="action-btn action-edit btn-edit" data-id="${r.id}" title="Editar">${svgEdit}</button>
                                <button type="button" class="action-btn action-delete btn-delete" data-id="${r.id}" title="Eliminar">${svgTrash}</button>
                            </td>
                        `;
                        document.getElementById('tblBody').appendChild(tr);
                    }

                    // inicializar DataTable
                    $('#tblDestinos').DataTable({
                        paging: true,
                        pageLength: 25,
                        lengthChange: true,
                        searching: true,
                        ordering: true,
                        order: [[1, 'asc']],
                        columnDefs: [
                            { orderable: false, targets: [7] },
                            { className: 'text-end', targets: [2,4] },
                            { className: 'text-center', targets: [6,7] }
                        ],
                        dom: '<"row"<"col-sm-6"l><"col-sm-6"f>>rt<"row"<"col-sm-6"i><"col-sm-6"p>>',
                        responsive: true,
                        destroy: true
                    });

                    attachRowEvents();
                } catch (err) {
                    console.error(err);
                    await Swal.fire('Error', 'No se pudo conectar al servidor', 'error');
                }
            }

            function attachRowEvents() {
                document.querySelectorAll('.btn-edit').forEach(b => {
                    b.removeEventListener('click', onEdit); b.addEventListener('click', onEdit);
                });
                document.querySelectorAll('.btn-delete').forEach(b => {
                    b.removeEventListener('click', onDelete); b.addEventListener('click', onDelete);
                });
            }

            async function onEdit(e) {
                const id = e.currentTarget.getAttribute('data-id');
                if (!id) return;
                try {
                    const resp = await fetch(HANDLER + '?op=getbyid&id=' + id);
                    const json = await resp.json();
                    if (!json.ok) return Swal.fire('Error', json.mensaje || 'No encontrado', 'error');
                    const r = json.data;
                    document.getElementById('hdId').value = r.id;
                    document.getElementById('txtDescripcion').value = r.descripcion || '';
                    document.getElementById('selValor').value = r.valor || '';
                    document.getElementById('txtImpacto').value = r.impacto || '';
                    document.getElementById('txtOcurrencia').value = r.ocurrencia || '';
                    document.getElementById('txtNivel').value = (r.nivel_riesgo_pld !== null && r.nivel_riesgo_pld !== undefined) ? r.nivel_riesgo_pld : '';
                    document.getElementById('txtMitigantes').value = r.mitigantes ?? 0;
                    document.getElementById('chkActivo').checked = !!r.activo;

                    document.getElementById('modalDestinoLabel').innerText = 'Editar destino de recursos';
                    const modal = new bootstrap.Modal(document.getElementById('modalDestino'), {});
                    modal.show();
                } catch (err) {
                    console.error(err);
                    Swal.fire('Error', 'No se pudo obtener el registro', 'error');
                }
            }

            async function onDelete(e) {
                const id = e.currentTarget.getAttribute('data-id');
                if (!id) return;
                const res = await Swal.fire({
                    title: 'Confirmar',
                    text: '¿Desactivar este registro?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sí, desactivar',
                    cancelButtonText: 'Cancelar'
                });
                if (!res.isConfirmed) return;
                try {
                    const resp = await fetch(HANDLER + '?op=eliminar&id=' + id, { method: 'POST' });
                    const json = await resp.json();
                    if (json.ok) {
                        await Swal.fire('OK', json.mensaje || 'Desactivado', 'success');
                        loadAndRender();
                    } else {
                        await Swal.fire('Error', json.mensaje || 'No se pudo desactivar', 'error');
                    }
                } catch (err) {
                    console.error(err);
                    Swal.fire('Error', 'Error en la conexión', 'error');
                }
            }

            // Nuevo
            document.getElementById('btnCapturar').addEventListener('click', function () {
                resetModal();
                document.getElementById('modalDestinoLabel').innerText = 'Capturar destino de recursos';
                new bootstrap.Modal(document.getElementById('modalDestino'), {}).show();
            });

            // Guardar
            document.getElementById('btnGuardar').addEventListener('click', async function () {
                const id = parseInt(document.getElementById('hdId').value || 0);
                const descripcion = document.getElementById('txtDescripcion').value.trim();
                const valor = document.getElementById('selValor').value.trim();
                const impacto = parseInt(document.getElementById('txtImpacto').value || 0);
                const ocurrencia = document.getElementById('txtOcurrencia').value.trim();
                const nivel = document.getElementById('txtNivel').value || '';
                const mitigantes = parseInt(document.getElementById('txtMitigantes').value || 0);
                const activo = document.getElementById('chkActivo').checked ? 1 : 0;

                if (!descripcion) return Swal.fire('Atención', 'La descripción es obligatoria', 'warning');

                const fd = new FormData();
                fd.append('op', 'guardar');
                if (id > 0) fd.append('id', id);
                fd.append('descripcion', descripcion);
                fd.append('valor', valor);
                if (!isNaN(impacto)) fd.append('impacto', impacto);
                fd.append('ocurrencia', ocurrencia);
                fd.append('nivel_riesgo_pld', nivel);
                fd.append('mitigantes', mitigantes);
                fd.append('activo', activo);

                try {
                    const resp = await fetch(HANDLER, { method: 'POST', body: fd });
                    const json = await resp.json();
                    if (json.ok) {
                        await Swal.fire('OK', json.mensaje || 'Guardado', 'success');
                        bootstrap.Modal.getInstance(document.getElementById('modalDestino'))?.hide();
                        resetModal();
                        loadAndRender();
                    } else {
                        await Swal.fire('Error', json.mensaje || 'No se pudo guardar', 'error');
                    }
                } catch (err) {
                    console.error(err);
                    Swal.fire('Error', 'Error al guardar (conexión)', 'error');
                }
            });

            document.getElementById('btnCancelar').addEventListener('click', resetModal);

            function resetModal() {
                document.getElementById('hdId').value = 0;
                document.getElementById('txtDescripcion').value = '';
                document.getElementById('selValor').value = '';
                document.getElementById('txtImpacto').value = '';
                document.getElementById('txtOcurrencia').value = '';
                document.getElementById('txtNivel').value = '';
                document.getElementById('txtMitigantes').value = '0';
                document.getElementById('chkActivo').checked = true;
            }

            // Inicial
            loadAndRender();
        });
    </script>
</asp:Content>
