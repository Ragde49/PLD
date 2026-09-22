<%@ Page Title="Catálogo Moneda Divisa" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_moneda_divisa.aspx.vb" Inherits="PLD.catalogo_moneda_divisa" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-3">

        <!-- Barra superior estilo "Créditos" -->
        <div class="mb-3">
            <div class="d-flex align-items-center justify-content-between p-2 rounded" style="background:#0d6efd; color:white;">
                <div><strong>Consulta de catálogo</strong></div>
                <div>
                    <button type="button" id="btnNuevo" class="btn btn-success btn-sm">+ Capturar</button>
                </div>
            </div>
        </div>

        <!-- Card con DataTable -->
        <div class="card">
            <div class="card-body p-2">
                <div class="row mb-2">
                    <div class="col-6">
                        <!-- DataTables length control aparecerá aquí por dom -->
                    </div>
                    <div class="col-6 text-end">
                        <label class="me-2">Search:</label>
                        <input id="txtBuscar" class="form-control form-control-sm d-inline-block" style="width:180px;" placeholder=""/>
                    </div>
                </div>

                <div class="table-responsive">
                    <table id="tblCatalogo" class="table table-sm table-striped w-100">
                        <thead class="table-light">
                            <tr>
                                <th style="width:60px">ID</th>
                                <th>Descripción</th>
                                <th style="width:90px">Impacto</th>
                                <th style="width:110px">Ocurrencia</th>
                                <th style="width:140px">Nivel Riesgo P.L.D</th>
                                <th style="width:110px">Estatus</th>
                                <th style="width:90px">Mitigantes</th>
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
    <div class="modal fade" id="modalRegistro" tabindex="-1" aria-labelledby="modalRegistroLabel" aria-hidden="true">
        <div class="modal-dialog modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="modalRegistroLabel" class="modal-title">Capturar</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <input type="hidden" id="hdId" value="0" />

                    <div class="mb-2">
                        <label class="form-label">Descripción*</label>
                        <input type="text" id="descripcion" class="form-control form-control-sm" placeholder="Descripción..." />
                    </div>

                    <div class="mb-2 row">
                        <div class="col-6">
                            <label class="form-label">Impacto*</label>
                            <input type="number" id="impacto" class="form-control form-control-sm" min="0" step="1" />
                        </div>
                        <div class="col-6">
                            <label class="form-label">Ocurrencia (%) *</label>
                            <input type="number" id="ocurrencia" class="form-control form-control-sm" min="0" max="100" step="0.01" />
                        </div>
                    </div>

                    <div class="mb-2">
                        <label class="form-label">Nivel Riesgo P.L.D.</label>
                        <input type="text" id="nivel_riesgo_pld" class="form-control form-control-sm" placeholder="Se calculará si se deja vacío" />
                    </div>

                    <div class="mb-2 form-check">
                        <input class="form-check-input" type="checkbox" id="estatus" checked />
                        <label class="form-check-label" for="estatus">Activo</label>
                    </div>

                </div>
                <div class="modal-footer">
                    <button type="button" id="btnGuardar" class="btn btn-primary btn-sm">Guardar</button>
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Estilos específicos -->
    <style>
        /* Oculto filtro por defecto de DataTables (backup) */
        .dataTables_filter { display: none !important; }

        /* layout y truncado si hace falta */
        #tblCatalogo th, #tblCatalogo td { vertical-align: middle !important; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        #tblCatalogo td:nth-child(2), #tblCatalogo th:nth-child(2) { white-space: normal; max-width:420px; } /* descripción controlada */
        .badge-azul {
            background:#00bcd4; color:white; border-radius:50%; display:inline-block; width:26px; height:26px; text-align:center; line-height:26px; font-weight:700;
        }
        .btn-accion { padding:.28rem .38rem; width:36px; height:36px; display:inline-flex; align-items:center; justify-content:center; border-radius:4px; margin-left:6px; }
        .btn-editar { border:1px solid #0d6efd; color:#0d6efd; background:white; }
        .btn-eliminar { border:1px solid #dc3545; color:#dc3545; background:white; }
        .btn-mas { border:1px solid #ffc107; color:#212529; background:white; }

        /* Reduce paddings */
        #tblCatalogo.table-sm td, #tblCatalogo.table-sm th { padding: .48rem .6rem; }
    </style>

    <!-- Script completo: DataTable + CRUD (DOM modificado para quitar filtro por defecto) -->
    <script>
    document.addEventListener("DOMContentLoaded", function () {
        const handlerUrl = "/handlers/handler_catalogo_moneda_divisa.ashx";
        let table = null;
        const modalEl = document.getElementById('modalRegistro');
        const bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });

        function showLoading(msg){ swal.fire({ title: msg||'Cargando...', didOpen: ()=> swal.showLoading(), allowOutsideClick:false }); }
        function hideLoading(){ try{ swal.close(); } catch(e){} }
        function formatDecimal(n){ return (n === null || n === undefined) ? "0.00" : parseFloat(n).toFixed(2); }

        function initDatatable(){
            table = $('#tblCatalogo').DataTable({
                data: [],
                autoWidth: false,
                pageLength: 25,
                lengthMenu: [10,25,50,100],
                responsive: true,
                // DOM personalizado: l = length, r = processing, t = table, i = info, p = pagination
                // NO incluimos 'f' (filter) para evitar el search por defecto de DataTables
                dom: '<"top"l>rt<"bottom"ip><"clear">',
                columns: [
                    { data: 'id' },
                    { data: 'descripcion' },
                    { data: 'impacto', className: 'text-end' },
                    { data: 'ocurrencia', className: 'text-end', render: function(d){ return (d!==null && d!==undefined) ? (parseFloat(d).toFixed(2)+'%') : '0.00%'; } },
                    { data: 'nivel_riesgo_pld', className: 'text-end', render: function(d){ return formatDecimal(d); } },
                    { data: 'estatus', className: 'text-center', render: function(d){ return (d==1 || d==true) ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>'; } },
                    { data: 'mitigantes', className: 'text-center', render: function(d){ return '<span class="badge-azul">'+(d||0)+'</span>'; } },
                    { data: null, orderable:false, className:'text-center', render: function(row){
                        var btnEdit = '<button type="button" class="btn btn-accion btn-editar btn-editar-row" data-id="'+row.id+'" title="Editar">'+
                                      '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M12.146.854a.5.5 0 0 1 .708 0l2.292 2.292a.5.5 0 0 1 0 .708l-9.793 9.793a.5.5 0 0 1-.168.11l-4 1.5a.5.5 0 0 1-.65-.65l1.5-4a.5.5 0 0 1 .11-.168L12.146.854z"/></svg></button>';
                        var btnDel = '<button type="button" class="btn btn-accion btn-eliminar btn-eliminar-row" data-id="'+row.id+'" title="Eliminar">'+
                                      '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16"><path d="M5.5 5.5A.5.5 0 0 1 6 5h4a.5.5 0 0 1 .5.5v7A1.5 1.5 0 0 1 9 14h-2A1.5 1.5 0 0 1 5.5 12.5v-7z"/><path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9.5A2.5 2.5 0 0 1 10.5 16h-5A2.5 2.5 0 0 1 3 13.5V4H2.5a1 1 0 0 1 0-2H5V1.5A1.5 1.5 0 0 1 6.5 0h3A1.5 1.5 0 0 1 11 1.5V2h2.5a1 1 0 0 1 1 1z"/></svg></button>';
                        return btnEdit + btnDel;
                    } }
                ],
                language: { search: "Search:", lengthMenu: "Show _MENU_ entries", info: "Showing _START_ to _END_ of _TOTAL_ entries" },
                drawCallback: function(){
                    $('#tblCatalogo').find('tr').css('height','auto');
                }
            });

            // external search box binding (solo UNO: #txtBuscar)
            const txtBuscar = document.getElementById('txtBuscar');
            if (txtBuscar) {
                txtBuscar.addEventListener('input', function(e){
                    table.search(e.target.value).draw();
                });
            }

            // delegated event handlers
            $('#tblCatalogo tbody').on('click', 'button', function(){
                var $btn = $(this);
                var row = table.row($btn.closest('tr')).data();
                if ($btn.hasClass('btn-editar-row')) openEdit(row);
                else if ($btn.hasClass('btn-eliminar-row')) confirmEliminar(row);
            });
        }

        // cargar datos
        async function cargarDatos(){
            try{
                showLoading('Cargando...');
                const resp = await fetch(handlerUrl + '?op=consultar', { method: 'GET', credentials: 'same-origin' });
                const json = await resp.json();
                if (json.ok){
                    const datos = json.datos.map(function(x){
                        return {
                            id: x.id,
                            descripcion: x.moneda || x.descripcion || '',
                            impacto: x.impacto || 0,
                            ocurrencia: x.probabilidad || x.ocurrencia || 0,
                            nivel_riesgo_pld: x.nivel_riesgo_pld || 0,
                            estatus: (x.estatus==1 || x.estatus==true) ? 1 : 0,
                            mitigantes: x.mitigantes || 0
                        };
                    });
                    table.clear().rows.add(datos).draw();
                } else {
                    swal.fire('Error', json.mensaje || 'No se pudo cargar', 'error');
                }
            } catch(err){
                console.error(err);
                swal.fire('Error', 'Falla al obtener datos', 'error');
            } finally { hideLoading(); }
        }

        // abrir nuevo
        function abrirNuevo(){
            document.getElementById('hdId').value = 0;
            document.getElementById('modalRegistroLabel').innerText = "Capturar";
            document.getElementById('descripcion').value = '';
            document.getElementById('impacto').value = '';
            document.getElementById('ocurrencia').value = '';
            document.getElementById('nivel_riesgo_pld').value = '';
            document.getElementById('estatus').checked = true;
            bsModal.show();
        }

        // abrir edición
        function openEdit(row){
            if (!row) return;
            document.getElementById('hdId').value = row.id;
            document.getElementById('modalRegistroLabel').innerText = "Editar";
            document.getElementById('descripcion').value = row.descripcion || '';
            document.getElementById('impacto').value = row.impacto || '';
            document.getElementById('ocurrencia').value = row.ocurrencia || '';
            document.getElementById('nivel_riesgo_pld').value = row.nivel_riesgo_pld != null ? parseFloat(row.nivel_riesgo_pld).toFixed(2) : '';
            document.getElementById('estatus').checked = !!row.estatus;
            bsModal.show();
        }

        // confirmar eliminar
        function confirmEliminar(row){
            if(!row) return;
            swal.fire({
                title: '¿Eliminar?',
                text: 'ID ' + row.id + ' - ' + (row.descripcion || ''),
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar'
            }).then(function(result){
                if (result.isConfirmed) eliminar(row.id);
            });
        }

        async function eliminar(id){
            try{
                showLoading('Eliminando...');
                var fd = new FormData();
                fd.append('op','eliminar');
                fd.append('id', id);
                const resp = await fetch(handlerUrl, { method: 'POST', credentials: 'same-origin', body: fd });
                const json = await resp.json();
                if (json.ok){ swal.fire('Eliminado','Registro eliminado','success'); cargarDatos(); }
                else swal.fire('Error', json.mensaje || 'No se eliminó', 'error');
            } catch(e){
                console.error(e); swal.fire('Error','Falla al eliminar','error');
            } finally { hideLoading(); }
        }

        // guardar (insert / update)
        async function guardar(){
            const id = parseInt(document.getElementById('hdId').value) || 0;
            const descripcion = document.getElementById('descripcion').value.trim();
            const impactoRaw = document.getElementById('impacto').value.trim();
            const ocurrenciaRaw = document.getElementById('ocurrencia').value.trim();
            let nivel = document.getElementById('nivel_riesgo_pld').value.trim();
            const est = document.getElementById('estatus').checked ? 1 : 0;

            if (!descripcion){ swal.fire('Validación','Capture la descripción','warning'); return; }
            if (impactoRaw === ''){ swal.fire('Validación','Capture impacto','warning'); return; }
            if (ocurrenciaRaw === ''){ swal.fire('Validación','Capture ocurrencia (%)','warning'); return; }
            const impacto = parseFloat(impactoRaw);
            const ocurrencia = parseFloat(ocurrenciaRaw);
            if (!isFinite(impacto) || impacto < 0){ swal.fire('Validación','Impacto inválido','warning'); return; }
            if (!isFinite(ocurrencia) || ocurrencia < 0 || ocurrencia > 100){ swal.fire('Validación','Ocurrencia inválida (0-100)','warning'); return; }

            if (!nivel){
                nivel = ((impacto * ocurrencia)/100).toFixed(2);
                document.getElementById('nivel_riesgo_pld').value = nivel;
            } else {
                nivel = nivel.replace(',', '.');
                if (isNaN(parseFloat(nivel))){ swal.fire('Validación','Nivel inválido','warning'); return; }
            }

            try{
                showLoading('Guardando...');
                var fd = new FormData();
                fd.append('op', id>0 ? 'editar' : 'guardar');
                if (id>0) fd.append('id', id);
                fd.append('moneda', descripcion); // mapeo para el handler existente
                fd.append('clave', '');
                fd.append('impacto', Math.round(impacto));
                fd.append('probabilidad', Math.round(ocurrencia));
                fd.append('nivel_riesgo_pld', nivel);
                fd.append('estatus', est);

                const resp = await fetch(handlerUrl, { method: 'POST', credentials: 'same-origin', body: fd });
                const json = await resp.json();
                if (json.ok){
                    swal.fire('OK', json.mensaje || 'Guardado', 'success');
                    bsModal.hide();
                    cargarDatos();
                } else {
                    swal.fire('Error', json.mensaje || 'No se guardó', 'error');
                }
            } catch(e){
                console.error(e); swal.fire('Error','Falla al guardar','error');
            } finally { hideLoading(); }
        }

        // recalcular nivel
        function recalcularNivel(){
            var i = parseFloat(document.getElementById('impacto').value || 'NaN');
            var p = parseFloat(document.getElementById('ocurrencia').value || 'NaN');
            if (isFinite(i) && isFinite(p) && p>=0 && p<=100){
                document.getElementById('nivel_riesgo_pld').value = ((i*p)/100).toFixed(2);
            }
        }

        // Bind global buttons
        document.getElementById('btnNuevo').addEventListener('click', abrirNuevo);
        document.getElementById('btnGuardar').addEventListener('click', guardar);
        document.getElementById('impacto').addEventListener('input', recalcularNivel);
        document.getElementById('ocurrencia').addEventListener('input', recalcularNivel);

        // Init
        initDatatable();
        cargarDatos();
    });
    </script>
</asp:Content>
