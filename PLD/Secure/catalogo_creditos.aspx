<%@ Page Title="Catálogo de créditos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="catalogo_creditos.aspx.vb" Inherits="PLD.catalogo_creditos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <h4 class="mb-3">Consulta de catálogo</h4>

        <div class="card">
            <!-- Barra estándar -->
            <div class="card-header d-flex justify-content-between align-items-center bg-primary text-white">
                <div class="fw-semibold"><i class="simple-icon-flag me-2"></i>Créditos</div>
                <div>
                    <button id="btnCapturar" type="button" class="btn btn-success btn-sm">
                        <span class="me-1">+</span> Capturar
                    </button>
                </div>
            </div>

            <div class="card-body">
                <div class="table-responsive">
                    <table id="tblCreditos" class="table table-sm table-striped table-bordered w-100">
                        <thead>
                            <tr>
                                <th style="width:70px;">ID</th>
                                <th>Descripción</th>
                                <th>Revolvente</th>
                                <th>Impacto</th>
                                <th>Ocurrencia</th>
                                <th>Nivel Riesgo P.L.D</th>
                                <th>Estatus</th>
                                <th>Mitigantes</th>
                                <th style="width:120px;">Acciones</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal: Capturar / Editar -->
    <div class="modal fade" id="mdlCredito" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 id="mdlTitulo" class="modal-title">Capturar crédito</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <input type="hidden" id="hidId" />

                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label">Tipo de crédito*</label>
                            <input id="txtDescripcion" type="text" class="form-control" maxlength="150" autocomplete="off" />
                        </div>

                        <div class="col-md-3 d-flex align-items-end">
                            <div class="form-check mb-2">
                                <input id="chkRevolvente" class="form-check-input" type="checkbox" />
                                <label for="chkRevolvente" class="form-check-label">Crédito revolvente</label>
                            </div>
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Impacto* (entero)</label>
                            <input id="inpImpacto" type="number" class="form-control" min="0" step="1" placeholder="Ej. 50" />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Probabilidad* (%)</label>
                            <input id="inpProbabilidad" type="number" class="form-control" min="0" max="100" step="1" placeholder="0 a 100" />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Nivel de riesgo P.L.D*</label>
                            <input id="inpRiesgo" type="number" class="form-control" step="0.01" placeholder="Ej. 12.50" />
                            <div class="form-text">Se autocalcula = Impacto × Prob / 100 (editable).</div>
                        </div>

                        <div class="col-md-5">
                            <label class="form-label">Tipo de estado de cuenta*</label>
                            <select id="selTipoEstadoCuenta" class="form-control">
                                <option value="">-- Selecciona --</option>
                            </select>
                        </div>

                        <div class="col-md-2 d-flex align-items-end">
                            <div class="form-check">
                                <input id="chkActivo" class="form-check-input" type="checkbox" checked />
                                <label for="chkActivo" class="form-check-label">Activo</label>
                            </div>
                        </div>

                        <div class="col-md-2">
                            <label class="form-label">Mitigantes</label>
                            <input id="inpMitigantes" type="number" class="form-control" min="0" step="1" value="0" />
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button id="btnGuardar" type="button" class="btn btn-primary">Guardar</button>
                    <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                </div>
            </div>
        </div>
    </div>

    <script>
    document.addEventListener("DOMContentLoaded", function () {
        // === Constantes de handlers (único ajuste de este cambio) ===
        const URL_CREDITOS    = "/handlers/catalogo_creditos_pld.ashx";
        const URL_TIPO_ESTADO = "/handlers/handler_tipo_estado_cuenta.ashx"; // <- CORRECTO

        const $ = window.jQuery;
        const sw = window.swal || window.Swal;

        // Autocálculo
        function calcularRiesgo() {
            const im = parseInt(document.getElementById("inpImpacto").value || "0", 10);
            const pr = parseInt(document.getElementById("inpProbabilidad").value || "0", 10);
            const rz = (im * pr / 100).toFixed(2);
            document.getElementById("inpRiesgo").value = rz;
        }
        document.getElementById("inpImpacto").addEventListener("input", calcularRiesgo);
        document.getElementById("inpProbabilidad").addEventListener("input", calcularRiesgo);

        // Badges
        const badgeEstatus = b => b ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-warning text-dark">Inactivo</span>';
        const badgeMitig   = m => `<span class="badge bg-info text-dark">${parseInt(m||0,10)||0}</span>`;

        // Íconos inline (sin dependencia de fuentes)
        const svgEdit  = '<svg width="14" height="14" viewBox="0 0 24 24"><path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04a1 1 0 0 0 0-1.41l-2.34-2.34a1 1 0 0 0-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/></svg>';
        const svgTrash = '<svg width="14" height="14" viewBox="0 0 24 24"><path d="M9 3v1H4v2h16V4h-5V3H9zm1 6v9H8V9h2zm6 0v9h-2V9h2z"/></svg>';
        const svgCheck = '<svg width="14" height="14" viewBox="0 0 24 24"><path d="M9 16.17l-3.88-3.88-1.41 1.41L9 19 20.29 7.71l-1.41-1.41z"/></svg>';

        // DataTable
        const dt = $("#tblCreditos").DataTable({
            data: [],
            columns: [
                { data: "id" },
                { data: "descripcion" },
                { data: "es_revolvente", render: d => d ? '<span class="badge bg-primary">Sí</span>' : '<span class="badge bg-light text-dark">No</span>' },
                { data: "impacto" },
                { data: "probabilidad_txt" },
                { data: "riesgo" },
                { data: "estatus", render: d => badgeEstatus(parseInt(d,10)===1), orderable:false },
                { data: "mitigantes", render: d => badgeMitig(d), orderable:false },
                {
                    data: null, orderable: false, className: "text-end",
                    render: function(row){
                        const id = row.id;
                        const est = row.estatus === 1;
                        const btnEdit = `<button type="button" class="btn btn-outline-primary btn-sm me-1 btn-edit" data-id="${id}" title="Editar">${svgEdit}</button>`;
                        const btnDown = `<button type="button" class="btn btn-outline-danger btn-sm me-1 btn-desact" data-id="${id}" title="Desactivar">${svgTrash}</button>`;
                        const btnUp   = `<button type="button" class="btn btn-outline-success btn-sm me-1 btn-activ" data-id="${id}" title="Activar">${svgCheck}</button>`;
                        return est ? (btnEdit + btnDown) : (btnEdit + btnUp);
                    }
                }
            ],
            order: [[0, "asc"]],
            pageLength: 25,
            lengthMenu: [25, 50, 100],
            dom: "lfrtip"
        });

        // Mapeo fila
        function mapeaFila(r){
            const est = (r.estatus === true || r.estatus === 1 || r.estatus === "1");
            const prob = parseInt((r.probabilidad ?? r.ocurrencia ?? 0), 10) || 0;
            return {
                id: r.id ?? r.ID ?? 0,
                descripcion: r.descripcion ?? r.nombre_credito ?? "",
                es_revolvente: (r.es_revolvente === true || r.es_revolvente === 1 || r.es_revolvente === "1"),
                impacto: parseInt(r.impacto ?? 0, 10) || 0,
                probabilidad: prob,
                probabilidad_txt: prob === 0 ? "" : (prob + "%"),
                riesgo: (parseFloat(r.nivel_riesgo_pld ?? 0) || 0).toFixed(2),
                estatus: est ? 1 : 0,
                mitigantes: parseInt(r.mitigantes ?? 0, 10) || 0
            };
        }

        // Carga lista
        async function cargarCreditos(){
            try{
                const rsp = await fetch(`${URL_CREDITOS}?op=consultar`, { cache: "no-store" });
                if(!rsp.ok) throw new Error("HTTP "+rsp.status);
                const json = await rsp.json();
                const arr = Array.isArray(json) ? json : (json.data || []);
                dt.clear().rows.add((arr || []).map(mapeaFila)).draw();
            }catch(err){
                console.error("[cargarCreditos]", err);
                sw && sw.fire("Sin datos","No se pudo cargar la lista (handler).","info");
            }
        }

        // Catálogo tipo de estado de cuenta
        async function cargarTiposEstadoCuenta(){
            try{
                const rsp = await fetch(`${URL_TIPO_ESTADO}?op=consultar`, { cache: "no-store" });
                if(!rsp.ok) throw new Error("HTTP "+rsp.status);
                const data = await rsp.json(); // [{id, descripcion}]
                const sel = document.getElementById("selTipoEstadoCuenta");
                sel.innerHTML = `<option value="">-- Selecciona --</option>` + (data || []).map(d => `<option value="${d.id}">${d.descripcion}</option>`).join("");
            }catch(e){
                console.warn("[cargarTiposEstadoCuenta] catálogo no disponible aún:", e);
            }
        }

        // Modal
        const mdl = new bootstrap.Modal(document.getElementById("mdlCredito"));
        function limpiarForm(){
            document.getElementById("hidId").value = "";
            document.getElementById("txtDescripcion").value = "";
            document.getElementById("chkRevolvente").checked = false;
            document.getElementById("inpImpacto").value = "";
            document.getElementById("inpProbabilidad").value = "";
            document.getElementById("inpRiesgo").value = "";
            document.getElementById("selTipoEstadoCuenta").value = "";
            document.getElementById("chkActivo").checked = true;
            document.getElementById("inpMitigantes").value = "0";
        }
        document.getElementById("btnCapturar").addEventListener("click", function(){
            limpiarForm();
            document.getElementById("mdlTitulo").textContent = "Capturar crédito";
            mdl.show();
        });

        $("#tblCreditos").on("click", ".btn-edit", async function(){
            const id = parseInt(this.getAttribute("data-id"),10);
            try{
                const rsp = await fetch(`${URL_CREDITOS}?op=obtener&id=${id}`);
                if(!rsp.ok) throw new Error("HTTP "+rsp.status);
                const r = await rsp.json();
                if (r.ok === false) throw new Error(r.mensaje || "No encontrado");

                document.getElementById("hidId").value = r.id;
                document.getElementById("txtDescripcion").value = r.descripcion ?? "";
                document.getElementById("chkRevolvente").checked = (r.es_revolvente === true || r.es_revolvente === 1 || r.es_revolvente === "1");
                document.getElementById("inpImpacto").value = r.impacto ?? 0;
                document.getElementById("inpProbabilidad").value = r.probabilidad ?? 0;
                document.getElementById("inpRiesgo").value = r.nivel_riesgo_pld ?? 0;
                document.getElementById("selTipoEstadoCuenta").value = r.tipo_estado_cuenta_id ?? "";
                document.getElementById("chkActivo").checked = (r.estatus === 1);
                document.getElementById("inpMitigantes").value = r.mitigantes ?? 0;

                document.getElementById("mdlTitulo").textContent = `Editar crédito #${id}`;
                mdl.show();
            }catch(e){
                console.error(e);
                sw && sw.fire("Error","No se pudo obtener el detalle.","error");
            }
        });

        $("#tblCreditos").on("click", ".btn-desact", async function(){
            const id = parseInt(this.getAttribute("data-id"),10);
            const ok = await (sw ? sw.fire({ title:"Confirmación", text:`¿Desactivar crédito #${id}?`, icon:"question", showCancelButton:true, confirmButtonText:"Sí", cancelButtonText:"No" }) : Promise.resolve({ isConfirmed:true }));
            if(!ok.isConfirmed) return;
            try{
                const rsp = await fetch(`${URL_CREDITOS}?op=desactivar`, {
                    method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ id })
                });
                const js = await rsp.json();
                if(js.ok === false) throw new Error(js.mensaje || "No se pudo desactivar");
                await cargarCreditos();
                sw && sw.fire("Listo","Crédito desactivado.","success");
            }catch(e){
                console.error(e);
                sw && sw.fire("Error","No se pudo desactivar.","error");
            }
        });

        $("#tblCreditos").on("click", ".btn-activ", async function(){
            const id = parseInt(this.getAttribute("data-id"),10);
            const ok = await (sw ? sw.fire({ title:"Confirmación", text:`¿Activar crédito #${id}?`, icon:"question", showCancelButton:true, confirmButtonText:"Sí", cancelButtonText:"No" }) : Promise.resolve({ isConfirmed:true }));
            if(!ok.isConfirmed) return;
            try{
                const rsp = await fetch(`${URL_CREDITOS}?op=activar`, {
                    method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ id })
                });
                const js = await rsp.json();
                if(js.ok === false) throw new Error(js.mensaje || "No se pudo activar");
                await cargarCreditos();
                sw && sw.fire("Listo","Crédito activado.","success");
            }catch(e){
                console.error(e);
                sw && sw.fire("Error","No se pudo activar.","error");
            }
        });

        document.getElementById("btnGuardar").addEventListener("click", async function(){
            const id = parseInt(document.getElementById("hidId").value || "0", 10);
            const descripcion = (document.getElementById("txtDescripcion").value || "").trim();
            const esRevolvente = document.getElementById("chkRevolvente").checked ? 1 : 0;
            const impacto = parseInt(document.getElementById("inpImpacto").value || "0", 10);
            const prob = parseInt(document.getElementById("inpProbabilidad").value || "0", 10);
            const riesgo = parseFloat(document.getElementById("inpRiesgo").value || "0");
            const tipoEstado = parseInt(document.getElementById("selTipoEstadoCuenta").value || "0", 10);
            const activo = document.getElementById("chkActivo").checked ? 1 : 0;
            const mitigantes = parseInt(document.getElementById("inpMitigantes").value || "0", 10);

            if (!descripcion){ sw && sw.fire("Falta información","Indica el Tipo de crédito.","warning"); return; }
            if (!Number.isInteger(impacto)){ sw && sw.fire("Dato inválido","Impacto debe ser entero.","warning"); return; }
            if (!Number.isInteger(prob) || prob < 0 || prob > 100){ sw && sw.fire("Dato inválido","Probabilidad debe ser 0 a 100.","warning"); return; }
            if (isNaN(riesgo)){ sw && sw.fire("Dato inválido","Nivel de riesgo debe ser numérico.","warning"); return; }
            if (!(tipoEstado > 0)){ sw && sw.fire("Falta información","Selecciona Tipo de estado de cuenta.","warning"); return; }

            const payload = { id, descripcion, es_revolvente: esRevolvente, impacto, probabilidad: prob, nivel_riesgo_pld: riesgo, tipo_estado_cuenta_id: tipoEstado, mitigantes, estatus: activo };

            try{
                const op = id > 0 ? "actualizar" : "guardar";
                const rsp = await fetch(`${URL_CREDITOS}?op=${op}`, {
                    method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify(payload)
                });
                const js = await rsp.json();
                if (js.ok === false) throw new Error(js.mensaje || "No guardó");
                mdl.hide();
                await cargarCreditos();
                sw && sw.fire("Listo","Registro guardado.","success");
            }catch(e){
                console.error(e);
                sw && sw.fire("Error","No se pudo guardar.","error");
            }
        });

        // Init
        cargarTiposEstadoCuenta();
        cargarCreditos();
    });
    </script>
</asp:Content>
