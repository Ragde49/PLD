<%@ Page Title="Seguridad" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <ul class="nav nav-tabs" id="seguridadTabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" id="tab-usuarios-tab" data-bs-toggle="tab" data-bs-target="#tab-usuarios" type="button" role="tab">Usuarios</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="tab-roles-tab" data-bs-toggle="tab" data-bs-target="#tab-roles" type="button" role="tab">Roles</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="tab-puestos-tab" data-bs-toggle="tab" data-bs-target="#tab-puestos" type="button" role="tab">Puestos</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="tab-accesos-tab" data-bs-toggle="tab" data-bs-target="#tab-accesos" type="button" role="tab">Accesos</button>
        </li>
    </ul>

    <div class="tab-content pt-3">
        <div class="tab-pane fade show active" id="tab-usuarios" role="tabpanel">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <strong>Usuarios</strong>
                    <button type="button" class="btn btn-success btn-sm" id="btnNuevoUsuario">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table id="tablaUsuarios" class="table table-sm table-bordered table-hover w-100">
                            <thead class="table-light">
                                <tr>
                                    <th>ID</th>
                                    <th>Usuario</th>
                                    <th>Nombre</th>
                                    <th>Email</th>
                                    <th>Rol</th>
                                    <th>Puesto</th>
                                    <th>Estatus</th>
                                    <th>Ultimo acceso</th>
                                    <th class="text-center">Acciones</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="tab-pane fade" id="tab-roles" role="tabpanel">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <strong>Roles</strong>
                    <button type="button" class="btn btn-success btn-sm" id="btnNuevoRol">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table id="tablaRoles" class="table table-sm table-bordered table-hover w-100">
                            <thead class="table-light">
                                <tr>
                                    <th>ID</th>
                                    <th>Rol</th>
                                    <th>Descripcion</th>
                                    <th>Estatus</th>
                                    <th class="text-center">Acciones</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="tab-pane fade" id="tab-puestos" role="tabpanel">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <strong>Puestos</strong>
                    <button type="button" class="btn btn-success btn-sm" id="btnNuevoPuesto">
                        <i class="fa fa-plus"></i> Capturar
                    </button>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table id="tablaPuestos" class="table table-sm table-bordered table-hover w-100">
                            <thead class="table-light">
                                <tr>
                                    <th>ID</th>
                                    <th>Puesto</th>
                                    <th>Descripcion</th>
                                    <th>Estatus</th>
                                    <th class="text-center">Acciones</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="tab-pane fade" id="tab-accesos" role="tabpanel">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white d-flex flex-column flex-lg-row gap-2 justify-content-between align-items-lg-center">
                    <strong>Accesos por rol</strong>
                    <div class="d-flex gap-2 align-items-center">
                        <select id="selectRolPermisos" class="form-select form-select-sm" style="min-width: 260px;"></select>
                        <button type="button" class="btn btn-light btn-sm" id="btnSeleccionarTodo">Todo</button>
                        <button type="button" class="btn btn-light btn-sm" id="btnLimpiarPermisos">Limpiar</button>
                        <button type="button" class="btn btn-success btn-sm" id="btnGuardarPermisos">Guardar</button>
                    </div>
                </div>
                <div class="card-body">
                    <div id="arbolPermisos" class="permissions-tree"></div>
                </div>
            </div>
        </div>
    </div>
</div>

<style>
    .permissions-tree {
        border: 1px solid #d8e2dc;
        border-radius: 8px;
        overflow: hidden;
    }

    .permissions-empty {
        padding: 1rem;
        color: #6c757d;
        text-align: center;
    }

    .permission-node {
        border-bottom: 1px solid #edf2ef;
    }

    .permission-node:last-child {
        border-bottom: 0;
    }

    .permission-row {
        display: flex;
        align-items: center;
        gap: .65rem;
        min-height: 42px;
        padding: .55rem .75rem;
    }

    .permission-row.category {
        background: #f6faf7;
        font-weight: 700;
    }

    .permission-row.page {
        background: #fff;
    }

    .permission-title {
        flex: 1;
        min-width: 0;
    }

    .permission-route {
        display: block;
        color: #6c757d;
        font-size: .78rem;
        font-weight: 400;
        word-break: break-all;
    }

    .permission-children {
        border-top: 1px solid #edf2ef;
    }
</style>

<div class="modal fade" id="modalUsuario" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Usuario</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <input type="hidden" id="usuarioId" />
                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label" for="usuarioLogin">Usuario</label>
                        <input type="text" id="usuarioLogin" class="form-control" maxlength="80" />
                    </div>
                    <div class="col-md-8">
                        <label class="form-label" for="usuarioNombre">Nombre</label>
                        <input type="text" id="usuarioNombre" class="form-control" maxlength="150" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label" for="usuarioEmail">Email</label>
                        <input type="email" id="usuarioEmail" class="form-control" maxlength="150" />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label" for="usuarioRol">Rol</label>
                        <select id="usuarioRol" class="form-select"></select>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label" for="usuarioPuesto">Puesto</label>
                        <select id="usuarioPuesto" class="form-select"></select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label" for="usuarioPassword">Contrasena</label>
                        <input type="password" id="usuarioPassword" class="form-control" autocomplete="new-password" />
                    </div>
                    <div class="col-md-3 d-flex align-items-end">
                        <div class="form-check form-switch mb-2">
                            <input class="form-check-input" type="checkbox" id="usuarioDebeCambiar" checked />
                            <label class="form-check-label" for="usuarioDebeCambiar">Cambiar al entrar</label>
                        </div>
                    </div>
                    <div class="col-md-3 d-flex align-items-end">
                        <div class="form-check form-switch mb-2">
                            <input class="form-check-input" type="checkbox" id="usuarioActivo" checked />
                            <label class="form-check-label" for="usuarioActivo">Activo</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success" id="btnGuardarUsuario">Guardar</button>
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="modalRol" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-md modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Rol</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <input type="hidden" id="rolId" />
                <div class="mb-3">
                    <label class="form-label" for="rolNombre">Rol</label>
                    <input type="text" id="rolNombre" class="form-control" maxlength="100" />
                </div>
                <div class="mb-3">
                    <label class="form-label" for="rolDescripcion">Descripcion</label>
                    <textarea id="rolDescripcion" class="form-control" rows="3" maxlength="250"></textarea>
                </div>
                <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" id="rolActivo" checked />
                    <label class="form-check-label" for="rolActivo">Activo</label>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success" id="btnGuardarRol">Guardar</button>
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="modalPuesto" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-md modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header bg-primary text-white">
                <h5 class="modal-title">Puesto</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <input type="hidden" id="puestoId" />
                <div class="mb-3">
                    <label class="form-label" for="puestoNombre">Puesto</label>
                    <input type="text" id="puestoNombre" class="form-control" maxlength="150" />
                </div>
                <div class="mb-3">
                    <label class="form-label" for="puestoDescripcion">Descripcion</label>
                    <textarea id="puestoDescripcion" class="form-control" rows="3" maxlength="250"></textarea>
                </div>
                <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" id="puestoActivo" checked />
                    <label class="form-check-label" for="puestoActivo">Activo</label>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success" id="btnGuardarPuesto">Guardar</button>
            </div>
        </div>
    </div>
</div>

<script>
    document.addEventListener("DOMContentLoaded", function () {
        const URL = "/handlers/seguridad_handler.ashx";
        let roles = [];
        let puestos = [];
        let tablaUsuarios = null;
        let tablaRoles = null;
        let tablaPuestos = null;

        function post(action, payload) {
            return fetch(URL, {
                method: "POST",
                credentials: "same-origin",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(Object.assign({ action: action }, payload || {}))
            }).then(function (r) { return r.json(); });
        }

        function showError(resp, fallback) {
            Swal.fire("Aviso", (resp && (resp.mensaje || resp.message)) || fallback || "No se pudo completar la operacion.", "warning");
        }

        function okMsg(text) {
            Swal.fire("Correcto", text || "Operacion realizada correctamente.", "success");
        }

        function esc(value) {
            return $("<div>").text(value == null ? "" : value).html();
        }

        function fecha(value) {
            if (!value) return "";
            if (typeof value === "string") {
                const m = /\/Date\((\d+)\)\//.exec(value);
                if (m) return new Date(parseInt(m[1], 10)).toLocaleString("es-MX");
            }
            const d = new Date(value);
            return isNaN(d.getTime()) ? "" : d.toLocaleString("es-MX");
        }

        function badgeActivo(value) {
            return value ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>';
        }

        function fillSelect(select, rows, valueField, textField, includeEmpty) {
            select.innerHTML = includeEmpty ? '<option value="">Selecciona...</option>' : "";
            rows.forEach(function (r) {
                const opt = document.createElement("option");
                opt.value = r[valueField];
                opt.textContent = r[textField];
                select.appendChild(opt);
            });
        }

        function cargarCatalogos() {
            return Promise.all([
                post("roles_listar"),
                post("puestos_listar")
            ]).then(function (results) {
                roles = results[0].ok ? results[0].data : [];
                puestos = results[1].ok ? results[1].data : [];
                fillSelect(document.getElementById("usuarioRol"), roles.filter(function (r) { return r.activo; }), "id", "rol", true);
                fillSelect(document.getElementById("usuarioPuesto"), puestos.filter(function (p) { return p.activo; }), "id", "puesto", true);
                fillSelect(document.getElementById("selectRolPermisos"), roles, "id", "rol", true);
            });
        }

        function cargarUsuarios() {
            post("usuarios_listar").then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                if (tablaUsuarios) tablaUsuarios.destroy();
                tablaUsuarios = $("#tablaUsuarios").DataTable({
                    data: resp.data,
                    pageLength: 25,
                    lengthChange: false,
                    columns: [
                        { data: "id" },
                        { data: "usuario", render: esc },
                        { data: "nombre", render: esc },
                        { data: "email", render: esc },
                        { data: "rol", render: esc },
                        { data: "puesto", render: esc },
                        { data: "activo", render: badgeActivo },
                        { data: "ultimo_acceso", render: fecha },
                        {
                            data: null,
                            className: "text-center",
                            orderable: false,
                            render: function (row) {
                                const next = row.activo ? "false" : "true";
                                const icon = row.activo ? "fa-ban" : "fa-check";
                                return '<button type="button" class="btn btn-sm btn-warning me-1 btnEditarUsuario" data-id="' + row.id + '" title="Editar"><i class="fa fa-edit"></i></button>' +
                                    '<button type="button" class="btn btn-sm btn-secondary btnToggleUsuario" data-id="' + row.id + '" data-activo="' + next + '" title="Cambiar estatus"><i class="fa ' + icon + '"></i></button>';
                            }
                        }
                    ],
                    language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json" }
                });
            });
        }

        function cargarRoles() {
            post("roles_listar").then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                roles = resp.data;
                if (tablaRoles) tablaRoles.destroy();
                tablaRoles = $("#tablaRoles").DataTable({
                    data: resp.data,
                    pageLength: 25,
                    lengthChange: false,
                    columns: [
                        { data: "id" },
                        { data: "rol", render: esc },
                        { data: "descripcion", render: esc },
                        { data: "activo", render: badgeActivo },
                        {
                            data: null,
                            className: "text-center",
                            orderable: false,
                            render: function (row) {
                                const next = row.activo ? "false" : "true";
                                const icon = row.activo ? "fa-ban" : "fa-check";
                                return '<button type="button" class="btn btn-sm btn-warning me-1 btnEditarRol" data-id="' + row.id + '" title="Editar"><i class="fa fa-edit"></i></button>' +
                                    '<button type="button" class="btn btn-sm btn-secondary btnToggleRol" data-id="' + row.id + '" data-activo="' + next + '" title="Cambiar estatus"><i class="fa ' + icon + '"></i></button>';
                            }
                        }
                    ],
                    language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json" }
                });
                fillSelect(document.getElementById("selectRolPermisos"), roles, "id", "rol", true);
                fillSelect(document.getElementById("usuarioRol"), roles.filter(function (r) { return r.activo; }), "id", "rol", true);
            });
        }

        function cargarPuestos() {
            post("puestos_listar").then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                puestos = resp.data;
                if (tablaPuestos) tablaPuestos.destroy();
                tablaPuestos = $("#tablaPuestos").DataTable({
                    data: resp.data,
                    pageLength: 25,
                    lengthChange: false,
                    columns: [
                        { data: "id" },
                        { data: "puesto", render: esc },
                        { data: "descripcion", render: esc },
                        { data: "activo", render: badgeActivo },
                        {
                            data: null,
                            className: "text-center",
                            orderable: false,
                            render: function (row) {
                                const next = row.activo ? "false" : "true";
                                const icon = row.activo ? "fa-ban" : "fa-check";
                                return '<button type="button" class="btn btn-sm btn-warning me-1 btnEditarPuesto" data-id="' + row.id + '" title="Editar"><i class="fa fa-edit"></i></button>' +
                                    '<button type="button" class="btn btn-sm btn-secondary btnTogglePuesto" data-id="' + row.id + '" data-activo="' + next + '" title="Cambiar estatus"><i class="fa ' + icon + '"></i></button>';
                            }
                        }
                    ],
                    language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json" }
                });
                fillSelect(document.getElementById("usuarioPuesto"), puestos.filter(function (p) { return p.activo; }), "id", "puesto", true);
            });
        }

        function cargarPermisos() {
            const rolId = document.getElementById("selectRolPermisos").value;
            const container = document.getElementById("arbolPermisos");
            if (!rolId) {
                container.innerHTML = '<div class="permissions-empty">Selecciona un rol para ver sus accesos.</div>';
                return;
            }

            post("permisos_por_rol", { rol_id: rolId }).then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                renderArbolPermisos(resp.data || []);
            });
        }

        function renderArbolPermisos(rows) {
            const container = document.getElementById("arbolPermisos");
            if (!rows.length) {
                container.innerHTML = '<div class="permissions-empty">No hay paginas de menu configuradas.</div>';
                return;
            }

            const byParent = {};
            rows.forEach(function (row) {
                const parentKey = row.parent_id == null ? "root" : String(row.parent_id);
                if (!byParent[parentKey]) byParent[parentKey] = [];
                byParent[parentKey].push(row);
            });

            Object.keys(byParent).forEach(function (key) {
                byParent[key].sort(function (a, b) {
                    const ao = parseInt(a.orden || 0, 10);
                    const bo = parseInt(b.orden || 0, 10);
                    if (ao !== bo) return ao - bo;
                    return String(a.titulo || "").localeCompare(String(b.titulo || ""));
                });
            });

            function renderChildren(parentKey, level) {
                const children = byParent[parentKey] || [];
                let html = "";
                children.forEach(function (node) {
                    html += renderNode(node, level);
                });
                return html;
            }

            function renderNode(node, level) {
                const childHtml = renderChildren(String(node.menu_id), level + 1);
                const padding = 12 + (level * 22);

                if (!node.page_id) {
                    if (!childHtml) return "";
                    return '' +
                        '<div class="permission-node" data-menu-id="' + node.menu_id + '">' +
                            '<div class="permission-row category" style="padding-left:' + padding + 'px">' +
                                '<input type="checkbox" class="form-check-input permiso-grupo" data-menu-id="' + node.menu_id + '" />' +
                                '<span class="permission-title">' + esc(node.titulo) + '</span>' +
                            '</div>' +
                            '<div class="permission-children">' + childHtml + '</div>' +
                        '</div>';
                }

                return '' +
                    '<div class="permission-node" data-menu-id="' + node.menu_id + '">' +
                        '<div class="permission-row page" style="padding-left:' + padding + 'px">' +
                            '<input type="checkbox" class="form-check-input permiso-check" value="' + node.page_id + '"' + (node.permitido ? " checked" : "") + ' />' +
                            '<span class="permission-title">' +
                                esc(node.titulo) +
                                '<span class="permission-route">' + esc(node.ruta || "") + '</span>' +
                            '</span>' +
                        '</div>' +
                    '</div>';
            }

            container.innerHTML = renderChildren("root", 0) || '<div class="permissions-empty">No hay paginas de menu configuradas.</div>';
            actualizarGruposPermisos();
        }

        function actualizarGruposPermisos() {
            document.querySelectorAll(".permiso-grupo").forEach(function (grupo) {
                const node = grupo.closest(".permission-node");
                const checks = node ? node.querySelectorAll(".permiso-check") : [];
                const total = checks.length;
                const checked = Array.from(checks).filter(function (c) { return c.checked; }).length;
                grupo.checked = total > 0 && checked === total;
                grupo.indeterminate = checked > 0 && checked < total;
            });
        }

        document.getElementById("btnNuevoUsuario").addEventListener("click", function () {
            document.getElementById("usuarioId").value = "";
            document.getElementById("usuarioLogin").value = "";
            document.getElementById("usuarioNombre").value = "";
            document.getElementById("usuarioEmail").value = "";
            document.getElementById("usuarioRol").value = "";
            document.getElementById("usuarioPuesto").value = "";
            document.getElementById("usuarioPassword").value = "";
            document.getElementById("usuarioDebeCambiar").checked = true;
            document.getElementById("usuarioActivo").checked = true;
            bootstrap.Modal.getOrCreateInstance(document.getElementById("modalUsuario")).show();
        });

        document.getElementById("btnGuardarUsuario").addEventListener("click", function () {
            const payload = {
                id: document.getElementById("usuarioId").value || 0,
                usuario: document.getElementById("usuarioLogin").value.trim(),
                nombre: document.getElementById("usuarioNombre").value.trim(),
                email: document.getElementById("usuarioEmail").value.trim(),
                rol_id: document.getElementById("usuarioRol").value,
                puesto_id: document.getElementById("usuarioPuesto").value,
                password: document.getElementById("usuarioPassword").value,
                debe_cambiar_password: document.getElementById("usuarioDebeCambiar").checked,
                activo: document.getElementById("usuarioActivo").checked
            };
            post("usuario_guardar", payload).then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                bootstrap.Modal.getInstance(document.getElementById("modalUsuario")).hide();
                okMsg(resp.mensaje);
                cargarUsuarios();
            });
        });

        document.addEventListener("click", function (e) {
            const editarUsuario = e.target.closest(".btnEditarUsuario");
            if (editarUsuario) {
                post("usuario_obtener", { id: editarUsuario.dataset.id }).then(function (resp) {
                    if (!resp.ok) { showError(resp); return; }
                    const u = resp.data;
                    document.getElementById("usuarioId").value = u.id;
                    document.getElementById("usuarioLogin").value = u.usuario || "";
                    document.getElementById("usuarioNombre").value = u.nombre || "";
                    document.getElementById("usuarioEmail").value = u.email || "";
                    document.getElementById("usuarioRol").value = u.rol_id || "";
                    document.getElementById("usuarioPuesto").value = u.puesto_id || "";
                    document.getElementById("usuarioPassword").value = "";
                    document.getElementById("usuarioDebeCambiar").checked = !!u.debe_cambiar_password;
                    document.getElementById("usuarioActivo").checked = !!u.activo;
                    bootstrap.Modal.getOrCreateInstance(document.getElementById("modalUsuario")).show();
                });
            }

            const toggleUsuario = e.target.closest(".btnToggleUsuario");
            if (toggleUsuario) {
                post("usuario_toggle", { id: toggleUsuario.dataset.id, activo: toggleUsuario.dataset.activo === "true" }).then(function (resp) {
                    if (!resp.ok) { showError(resp); return; }
                    cargarUsuarios();
                });
            }
        });

        document.getElementById("btnNuevoRol").addEventListener("click", function () {
            document.getElementById("rolId").value = "";
            document.getElementById("rolNombre").value = "";
            document.getElementById("rolDescripcion").value = "";
            document.getElementById("rolActivo").checked = true;
            bootstrap.Modal.getOrCreateInstance(document.getElementById("modalRol")).show();
        });

        document.getElementById("btnGuardarRol").addEventListener("click", function () {
            post("rol_guardar", {
                id: document.getElementById("rolId").value || 0,
                rol: document.getElementById("rolNombre").value.trim(),
                descripcion: document.getElementById("rolDescripcion").value.trim(),
                activo: document.getElementById("rolActivo").checked
            }).then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                bootstrap.Modal.getInstance(document.getElementById("modalRol")).hide();
                okMsg(resp.mensaje);
                cargarRoles();
            });
        });

        document.addEventListener("click", function (e) {
            const editarRol = e.target.closest(".btnEditarRol");
            if (editarRol) {
                const row = roles.find(function (r) { return String(r.id) === String(editarRol.dataset.id); });
                if (!row) return;
                document.getElementById("rolId").value = row.id;
                document.getElementById("rolNombre").value = row.rol || "";
                document.getElementById("rolDescripcion").value = row.descripcion || "";
                document.getElementById("rolActivo").checked = !!row.activo;
                bootstrap.Modal.getOrCreateInstance(document.getElementById("modalRol")).show();
            }

            const toggleRol = e.target.closest(".btnToggleRol");
            if (toggleRol) {
                post("rol_toggle", { id: toggleRol.dataset.id, activo: toggleRol.dataset.activo === "true" }).then(function (resp) {
                    if (!resp.ok) { showError(resp); return; }
                    cargarRoles();
                });
            }
        });

        document.getElementById("btnNuevoPuesto").addEventListener("click", function () {
            document.getElementById("puestoId").value = "";
            document.getElementById("puestoNombre").value = "";
            document.getElementById("puestoDescripcion").value = "";
            document.getElementById("puestoActivo").checked = true;
            bootstrap.Modal.getOrCreateInstance(document.getElementById("modalPuesto")).show();
        });

        document.getElementById("btnGuardarPuesto").addEventListener("click", function () {
            post("puesto_guardar", {
                id: document.getElementById("puestoId").value || 0,
                puesto: document.getElementById("puestoNombre").value.trim(),
                descripcion: document.getElementById("puestoDescripcion").value.trim(),
                activo: document.getElementById("puestoActivo").checked
            }).then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                bootstrap.Modal.getInstance(document.getElementById("modalPuesto")).hide();
                okMsg(resp.mensaje);
                cargarPuestos();
            });
        });

        document.addEventListener("click", function (e) {
            const editarPuesto = e.target.closest(".btnEditarPuesto");
            if (editarPuesto) {
                const row = puestos.find(function (p) { return String(p.id) === String(editarPuesto.dataset.id); });
                if (!row) return;
                document.getElementById("puestoId").value = row.id;
                document.getElementById("puestoNombre").value = row.puesto || "";
                document.getElementById("puestoDescripcion").value = row.descripcion || "";
                document.getElementById("puestoActivo").checked = !!row.activo;
                bootstrap.Modal.getOrCreateInstance(document.getElementById("modalPuesto")).show();
            }

            const togglePuesto = e.target.closest(".btnTogglePuesto");
            if (togglePuesto) {
                post("puesto_toggle", { id: togglePuesto.dataset.id, activo: togglePuesto.dataset.activo === "true" }).then(function (resp) {
                    if (!resp.ok) { showError(resp); return; }
                    cargarPuestos();
                });
            }
        });

        document.getElementById("selectRolPermisos").addEventListener("change", cargarPermisos);
        document.getElementById("btnSeleccionarTodo").addEventListener("click", function () {
            document.querySelectorAll(".permiso-check").forEach(function (c) { c.checked = true; });
            actualizarGruposPermisos();
        });
        document.getElementById("btnLimpiarPermisos").addEventListener("click", function () {
            document.querySelectorAll(".permiso-check").forEach(function (c) { c.checked = false; });
            actualizarGruposPermisos();
        });
        document.getElementById("arbolPermisos").addEventListener("change", function (e) {
            const grupo = e.target.closest(".permiso-grupo");
            if (grupo) {
                const node = grupo.closest(".permission-node");
                if (node) {
                    node.querySelectorAll(".permiso-check").forEach(function (c) {
                        c.checked = grupo.checked;
                    });
                }
                actualizarGruposPermisos();
                return;
            }

            if (e.target.closest(".permiso-check")) {
                actualizarGruposPermisos();
            }
        });
        document.getElementById("btnGuardarPermisos").addEventListener("click", function () {
            const rolId = document.getElementById("selectRolPermisos").value;
            if (!rolId) { Swal.fire("Aviso", "Selecciona un rol.", "warning"); return; }
            const ids = Array.from(document.querySelectorAll(".permiso-check:checked")).map(function (c) { return parseInt(c.value, 10); });
            post("permisos_guardar", { rol_id: rolId, pagina_ids: ids }).then(function (resp) {
                if (!resp.ok) { showError(resp); return; }
                okMsg(resp.mensaje);
                cargarPermisos();
            });
        });

        cargarCatalogos().then(function () {
            cargarUsuarios();
            cargarRoles();
            cargarPuestos();
        });
    });
</script>
</asp:Content>
