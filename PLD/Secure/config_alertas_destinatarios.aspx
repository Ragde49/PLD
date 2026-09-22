<%@ Page Title="Destinatarios de Alertas P.L.D." Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="config_alertas_destinatarios.aspx.vb" Inherits="PLD.config_alertas_destinatarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid mt-4">

        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h4 class="mb-1">
                    <i class="fa fa-envelope-open-text me-2"></i>
                    Configuración de destinatarios de Alertas P.L.D.
                </h4>
                <small class="text-muted">
                    Selecciona qué puestos recibirán correo cuando se generen alertas por categoría, motivo o regla.
                </small>
            </div>

            <div class="d-flex gap-2">
                <button type="button" id="btnNuevaConfig" class="btn btn-success btn-sm">
                    <i class="fa fa-plus me-1"></i>
                    Nueva configuración
                </button>

                <button type="button" id="btnRefrescar" class="btn btn-primary btn-sm">
                    <i class="fa fa-sync-alt me-1"></i>
                    Refrescar
                </button>
            </div>
        </div>

        <div class="card shadow-sm mb-3">
            <div class="card-body">
                <div class="row g-3">

                    <div class="col-md-3">
                        <label for="filtroCategoria" class="form-label">Categoría</label>
                        <select id="filtroCategoria" class="form-control">
                            <option value="">Todas</option>
                        </select>
                    </div>

                    <div class="col-md-3">
                        <label for="filtroMotivo" class="form-label">Motivo</label>
                        <select id="filtroMotivo" class="form-control">
                            <option value="">Todos</option>
                        </select>
                    </div>

                    <div class="col-md-3">
                        <label for="filtroRegla" class="form-label">Regla</label>
                        <select id="filtroRegla" class="form-control">
                            <option value="">Todas</option>
                        </select>
                    </div>

                    <div class="col-md-2">
                        <label for="filtroPuesto" class="form-label">Puesto</label>
                        <select id="filtroPuesto" class="form-control">
                            <option value="">Todos</option>
                        </select>
                    </div>

                    <div class="col-md-1 d-flex align-items-end">
                        <button type="button" id="btnBuscar" class="btn btn-success w-100">
                            <i class="fa fa-search"></i>
                        </button>
                    </div>

                </div>
            </div>
        </div>

        <div class="card shadow-sm">
            <div class="card-body">

                <div id="loaderConfig" class="text-center py-4 d-none">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <div class="mt-2 text-muted">Cargando configuraciones...</div>
                </div>

                <div class="table-responsive">
                    <table id="tablaConfig" class="table table-sm table-striped table-hover align-middle w-100">
                        <thead class="table-dark">
                            <tr>
                                <th>ID</th>
                                <th>Categoría</th>
                                <th>Motivo</th>
                                <th>Regla</th>
                                <th>Puesto</th>
                                <th>Usuarios</th>
                                <th>Con correo</th>
                                <th>Enviar</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="tbodyConfig">
                        </tbody>
                    </table>
                </div>

            </div>
        </div>

    </div>

    <!-- Modal configuración -->
    <div class="modal fade" id="modalConfig" tabindex="-1" aria-labelledby="modalConfigLabel" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable">
            <div class="modal-content">

                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title" id="modalConfigLabel">
                        <i class="fa fa-envelope me-2"></i>
                        Configurar destinatario por puesto
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <input type="hidden" id="configId" value="0" />

                    <div class="alert alert-info">
                        Puedes configurar el envío por <strong>categoría completa</strong>, por <strong>motivo</strong> o por <strong>regla específica</strong>.
                        La regla es el nivel más específico.
                    </div>

                    <div class="row g-3">

                        <div class="col-md-4">
                            <label for="configCategoria" class="form-label">Categoría</label>
                            <select id="configCategoria" class="form-control">
                                <option value="">Selecciona...</option>
                            </select>
                        </div>

                        <div class="col-md-4">
                            <label for="configMotivo" class="form-label">Motivo</label>
                            <select id="configMotivo" class="form-control">
                                <option value="">Opcional</option>
                            </select>
                        </div>

                        <div class="col-md-4">
                            <label for="configRegla" class="form-label">Regla</label>
                            <select id="configRegla" class="form-control">
                                <option value="">Opcional</option>
                            </select>
                        </div>

                        <div class="col-md-6">
                            <label for="configPuesto" class="form-label">Puesto que recibirá correo *</label>
                            <select id="configPuesto" class="form-control">
                                <option value="">Selecciona...</option>
                            </select>
                        </div>

                        <div class="col-md-3 d-flex align-items-end">
                            <div class="form-check">
                                <input type="checkbox" id="configEnviarCorreo" class="form-check-input" checked />
                                <label for="configEnviarCorreo" class="form-check-label">Enviar correo</label>
                            </div>
                        </div>

                        <div class="col-md-3 d-flex align-items-end">
                            <button type="button" id="btnLimpiarSeleccion" class="btn btn-outline-secondary w-100">
                                <i class="fa fa-eraser me-1"></i>
                                Limpiar alcance
                            </button>
                        </div>

                    </div>

                    <hr />

                    <div class="row g-3">
                        <div class="col-md-12">
                            <div class="card border-primary">
                                <div class="card-header bg-primary text-white">
                                    <i class="fa fa-info-circle me-1"></i>
                                    Alcance seleccionado
                                </div>
                                <div class="card-body">
                                    <div id="resumenAlcance" class="text-muted">
                                        Sin selección.
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnGuardarConfig" class="btn btn-success">
                        <i class="fa fa-save me-1"></i>
                        Guardar
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cerrar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            const HANDLER = "/handlers/handler_config_alertas_destinatarios.ashx";

            let modalConfig = null;
            let tablaConfig = null;

            let catalogos = {
                categorias: [],
                motivos: [],
                reglas: [],
                puestos: []
            };

            const $ = window.jQuery;

            inicializar();
            
            function inicializar() {
                modalConfig = new bootstrap.Modal(document.getElementById("modalConfig"));

                inicializarEventos();
                inicializarTabla();
                cargarCatalogos();
            }

            function inicializarEventos() {
                document.getElementById("btnNuevaConfig").addEventListener("click", function () {
                    limpiarFormulario();
                    modalConfig.show();
                });

                document.getElementById("btnRefrescar").addEventListener("click", function () {
                    consultar();
                });

                document.getElementById("btnBuscar").addEventListener("click", function () {
                    consultar();
                });

                document.getElementById("btnGuardarConfig").addEventListener("click", function () {
                    guardar();
                });

                document.getElementById("btnLimpiarSeleccion").addEventListener("click", function () {
                    document.getElementById("configCategoria").value = "";
                    document.getElementById("configMotivo").value = "";
                    document.getElementById("configRegla").value = "";
                    cargarMotivosSelect("configMotivo", 0, "Opcional");
                    cargarReglasSelect("configRegla", 0, 0, "Opcional");
                    actualizarResumenAlcance();
                });

                document.getElementById("filtroCategoria").addEventListener("change", function () {
                    const categoriaId = entero(this.value);
                    cargarMotivosSelect("filtroMotivo", categoriaId, "Todos");
                    cargarReglasSelect("filtroRegla", categoriaId, 0, "Todas");
                });

                document.getElementById("filtroMotivo").addEventListener("change", function () {
                    const categoriaId = entero(document.getElementById("filtroCategoria").value);
                    const motivoId = entero(this.value);
                    cargarReglasSelect("filtroRegla", categoriaId, motivoId, "Todas");
                });

                document.getElementById("configCategoria").addEventListener("change", function () {
                    const categoriaId = entero(this.value);
                    document.getElementById("configMotivo").value = "";
                    document.getElementById("configRegla").value = "";
                    cargarMotivosSelect("configMotivo", categoriaId, "Opcional");
                    cargarReglasSelect("configRegla", categoriaId, 0, "Opcional");
                    actualizarResumenAlcance();
                });

                document.getElementById("configMotivo").addEventListener("change", function () {
                    const categoriaId = entero(document.getElementById("configCategoria").value);
                    const motivoId = entero(this.value);
                    document.getElementById("configRegla").value = "";
                    cargarReglasSelect("configRegla", categoriaId, motivoId, "Opcional");
                    actualizarResumenAlcance();
                });

                document.getElementById("configRegla").addEventListener("change", function () {
                    const reglaId = entero(this.value);

                    if (reglaId > 0) {
                        const regla = buscarPorId(catalogos.reglas, reglaId);

                        if (regla) {
                            document.getElementById("configCategoria").value = regla.categoria_id || "";
                            cargarMotivosSelect("configMotivo", entero(regla.categoria_id), "Opcional");
                            document.getElementById("configMotivo").value = regla.motivo_id || "";
                            cargarReglasSelect("configRegla", entero(regla.categoria_id), entero(regla.motivo_id), "Opcional");
                            document.getElementById("configRegla").value = regla.id || "";
                        }
                    }

                    actualizarResumenAlcance();
                });

                document.getElementById("configPuesto").addEventListener("change", actualizarResumenAlcance);

                document.getElementById("tablaConfig").addEventListener("click", function (e) {
                    const btnEditar = e.target.closest(".btn-editar");
                    const btnEliminar = e.target.closest(".btn-eliminar");

                    if (btnEditar) {
                        editar(btnEditar.getAttribute("data-id"));
                    }

                    if (btnEliminar) {
                        eliminar(btnEliminar.getAttribute("data-id"));
                    }
                });
            }

            function inicializarTabla() {
                if ($ && $.fn.DataTable) {
                    tablaConfig = $("#tablaConfig").DataTable({
                        pageLength: 10,
                        responsive: true,
                        ordering: true,
                        order: [[0, "desc"]],
                        destroy: true,
                        language: {
                            url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-MX.json"
                        },
                        columnDefs: [
                            { orderable: false, targets: [8] }
                        ]
                    });
                }
            }

            function mostrarLoader(mostrar) {
                const loader = document.getElementById("loaderConfig");

                if (mostrar) {
                    loader.classList.remove("d-none");
                } else {
                    loader.classList.add("d-none");
                }
            }

            function mostrarError(mensaje) {
                Swal.fire("Error", mensaje || "Ocurrió un error inesperado.", "error");
            }

            function mostrarOk(mensaje) {
                Swal.fire("Correcto", mensaje || "Operación realizada correctamente.", "success");
            }

            function valorSeguro(valor) {
                if (valor === null || valor === undefined) {
                    return "";
                }

                return String(valor);
            }

            function escaparHtml(texto) {
                return valorSeguro(texto)
                    .replace(/&/g, "&amp;")
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                    .replace(/"/g, "&quot;")
                    .replace(/'/g, "&#039;");
            }

            function entero(valor) {
                const n = parseInt(valor, 10);
                return isNaN(n) ? 0 : n;
            }

            function boolBadge(valor) {
                if (valor === true || valor === 1 || valor === "1") {
                    return '<span class="badge bg-success">Sí</span>';
                }

                return '<span class="badge bg-secondary">No</span>';
            }

            function buscarPorId(lista, id) {
                id = entero(id);

                for (let i = 0; i < (lista || []).length; i++) {
                    if (entero(lista[i].id) === id) {
                        return lista[i];
                    }
                }

                return null;
            }

            function construirUrl(action, params) {
                const url = new URL(HANDLER, window.location.origin);
                url.searchParams.set("action", action);

                if (params) {
                    Object.keys(params).forEach(function (key) {
                        if (params[key] !== null && params[key] !== undefined) {
                            url.searchParams.set(key, params[key]);
                        }
                    });
                }

                return url.toString();
            }

            function fetchJson(url, options) {
                return fetch(url, options || {})
                    .then(function (response) {
                        if (!response.ok) {
                            throw new Error("Error HTTP " + response.status);
                        }

                        return response.json();
                    });
            }

            function cargarCatalogos() {
                mostrarLoader(true);

                fetchJson(construirUrl("catalogos"))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        catalogos = resp.data || catalogos;

                        cargarSelectsIniciales();
                        consultar();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    })
                    .finally(function () {
                        mostrarLoader(false);
                    });
            }

            function cargarSelectsIniciales() {
                cargarCategoriasSelect("filtroCategoria", "Todas");
                cargarCategoriasSelect("configCategoria", "Selecciona...");

                cargarMotivosSelect("filtroMotivo", 0, "Todos");
                cargarMotivosSelect("configMotivo", 0, "Opcional");

                cargarReglasSelect("filtroRegla", 0, 0, "Todas");
                cargarReglasSelect("configRegla", 0, 0, "Opcional");

                cargarPuestosSelect("filtroPuesto", "Todos");
                cargarPuestosSelect("configPuesto", "Selecciona...");
            }

            function cargarCategoriasSelect(id, placeholder) {
                const select = document.getElementById(id);
                select.innerHTML = "";

                const opt = document.createElement("option");
                opt.value = "";
                opt.textContent = placeholder || "Selecciona...";
                select.appendChild(opt);

                (catalogos.categorias || []).forEach(function (item) {
                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = item.descripcion;
                    select.appendChild(option);
                });
            }

            function cargarMotivosSelect(id, categoriaId, placeholder) {
                const select = document.getElementById(id);
                select.innerHTML = "";

                const opt = document.createElement("option");
                opt.value = "";
                opt.textContent = placeholder || "Todos";
                select.appendChild(opt);

                (catalogos.motivos || []).forEach(function (item) {
                    if (categoriaId > 0 && entero(item.categoria_id) !== categoriaId) {
                        return;
                    }

                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = "[" + item.categoria + "] " + item.motivo;
                    select.appendChild(option);
                });
            }

            function cargarReglasSelect(id, categoriaId, motivoId, placeholder) {
                const select = document.getElementById(id);
                select.innerHTML = "";

                const opt = document.createElement("option");
                opt.value = "";
                opt.textContent = placeholder || "Todas";
                select.appendChild(opt);

                (catalogos.reglas || []).forEach(function (item) {
                    if (categoriaId > 0 && entero(item.categoria_id) !== categoriaId) {
                        return;
                    }

                    if (motivoId > 0 && entero(item.motivo_id) !== motivoId) {
                        return;
                    }

                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = "[" + item.tipo_disparador + "] " + item.nombre_regla;
                    select.appendChild(option);
                });
            }

            function cargarPuestosSelect(id, placeholder) {
                const select = document.getElementById(id);
                select.innerHTML = "";

                const opt = document.createElement("option");
                opt.value = "";
                opt.textContent = placeholder || "Selecciona...";
                select.appendChild(opt);

                (catalogos.puestos || []).forEach(function (item) {
                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = item.puesto;
                    select.appendChild(option);
                });
            }

            function consultar() {
                mostrarLoader(true);

                const params = {
                    categoria_id: document.getElementById("filtroCategoria").value || "",
                    motivo_id: document.getElementById("filtroMotivo").value || "",
                    regla_id: document.getElementById("filtroRegla").value || "",
                    puesto_id: document.getElementById("filtroPuesto").value || ""
                };

                fetchJson(construirUrl("consultar", params))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        renderizarTabla(resp.data || []);
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    })
                    .finally(function () {
                        mostrarLoader(false);
                    });
            }

            function renderizarTabla(data) {
                if (tablaConfig) {
                    tablaConfig.clear();
                } else {
                    document.getElementById("tbodyConfig").innerHTML = "";
                }

                data.forEach(function (item) {
                    const acciones =
                        '<div class="btn-group btn-group-sm" role="group">' +
                            '<button type="button" class="btn btn-outline-primary btn-editar" data-id="' + item.id + '" title="Editar">' +
                                '<i class="fa fa-edit"></i>' +
                            '</button>' +
                            '<button type="button" class="btn btn-outline-danger btn-eliminar" data-id="' + item.id + '" title="Eliminar">' +
                                '<i class="fa fa-trash"></i>' +
                            '</button>' +
                        '</div>';

                    const fila = [
                        item.id,
                        escaparHtml(item.categoria || "Todas / No específico"),
                        escaparHtml(item.motivo || "Todos / No específico"),
                        escaparHtml(item.nombre_regla || "Todas / No específico"),
                        escaparHtml(item.puesto),
                        item.total_usuarios || 0,
                        item.total_con_email || 0,
                        boolBadge(item.enviar_correo),
                        acciones
                    ];

                    if (tablaConfig) {
                        tablaConfig.row.add(fila);
                    } else {
                        const tr = document.createElement("tr");

                        fila.forEach(function (col) {
                            const td = document.createElement("td");
                            td.innerHTML = col;
                            tr.appendChild(td);
                        });

                        document.getElementById("tbodyConfig").appendChild(tr);
                    }
                });

                if (tablaConfig) {
                    tablaConfig.draw();
                }
            }

            function limpiarFormulario() {
                document.getElementById("configId").value = "0";
                document.getElementById("configCategoria").value = "";
                document.getElementById("configMotivo").value = "";
                document.getElementById("configRegla").value = "";
                document.getElementById("configPuesto").value = "";
                document.getElementById("configEnviarCorreo").checked = true;

                cargarMotivosSelect("configMotivo", 0, "Opcional");
                cargarReglasSelect("configRegla", 0, 0, "Opcional");

                actualizarResumenAlcance();
            }

            function editar(id) {
                id = entero(id);

                if (id <= 0) {
                    mostrarError("ID inválido.");
                    return;
                }

                fetchJson(construirUrl("consultar", {}))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        const data = resp.data || [];
                        let item = null;

                        for (let i = 0; i < data.length; i++) {
                            if (entero(data[i].id) === id) {
                                item = data[i];
                                break;
                            }
                        }

                        if (!item) {
                            mostrarError("No se encontró la configuración.");
                            return;
                        }

                        cargarFormulario(item);
                        modalConfig.show();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function cargarFormulario(item) {
                document.getElementById("configId").value = item.id || "0";
                document.getElementById("configCategoria").value = item.categoria_id || "";

                cargarMotivosSelect("configMotivo", entero(item.categoria_id), "Opcional");
                document.getElementById("configMotivo").value = item.motivo_id || "";

                cargarReglasSelect("configRegla", entero(item.categoria_id), entero(item.motivo_id), "Opcional");
                document.getElementById("configRegla").value = item.regla_id || "";

                document.getElementById("configPuesto").value = item.puesto_id || "";
                document.getElementById("configEnviarCorreo").checked = item.enviar_correo === true || item.enviar_correo === 1 || item.enviar_correo === "1";

                actualizarResumenAlcance();
            }

            function actualizarResumenAlcance() {
                const categoriaId = entero(document.getElementById("configCategoria").value);
                const motivoId = entero(document.getElementById("configMotivo").value);
                const reglaId = entero(document.getElementById("configRegla").value);
                const puestoId = entero(document.getElementById("configPuesto").value);

                const categoria = buscarPorId(catalogos.categorias, categoriaId);
                const motivo = buscarPorId(catalogos.motivos, motivoId);
                const regla = buscarPorId(catalogos.reglas, reglaId);
                const puesto = buscarPorId(catalogos.puestos, puestoId);

                let nivel = "Sin selección";
                let detalle = "Debes seleccionar al menos categoría, motivo o regla.";

                if (regla) {
                    nivel = "Regla específica";
                    detalle = regla.nombre_regla;
                } else if (motivo) {
                    nivel = "Motivo específico";
                    detalle = motivo.motivo;
                } else if (categoria) {
                    nivel = "Categoría completa";
                    detalle = categoria.descripcion;
                }

                let html = "";
                html += '<div><strong>Alcance:</strong> ' + escaparHtml(nivel) + '</div>';
                html += '<div><strong>Detalle:</strong> ' + escaparHtml(detalle) + '</div>';
                html += '<div><strong>Puesto:</strong> ' + escaparHtml(puesto ? puesto.puesto : "Sin puesto seleccionado") + '</div>';

                document.getElementById("resumenAlcance").innerHTML = html;
            }

            function validar() {
                const categoriaId = entero(document.getElementById("configCategoria").value);
                const motivoId = entero(document.getElementById("configMotivo").value);
                const reglaId = entero(document.getElementById("configRegla").value);
                const puestoId = entero(document.getElementById("configPuesto").value);

                if (categoriaId <= 0 && motivoId <= 0 && reglaId <= 0) {
                    mostrarError("Debes seleccionar al menos una categoría, motivo o regla.");
                    return false;
                }

                if (puestoId <= 0) {
                    mostrarError("Debes seleccionar el puesto que recibirá el correo.");
                    return false;
                }

                return true;
            }

            function guardar() {
                if (!validar()) {
                    return;
                }

                const formData = new FormData();
                formData.append("action", "guardar");
                formData.append("id", document.getElementById("configId").value || "0");
                formData.append("categoria_id", document.getElementById("configCategoria").value || "");
                formData.append("motivo_id", document.getElementById("configMotivo").value || "");
                formData.append("regla_id", document.getElementById("configRegla").value || "");
                formData.append("puesto_id", document.getElementById("configPuesto").value || "");
                formData.append("enviar_correo", document.getElementById("configEnviarCorreo").checked ? "1" : "0");

                fetchJson(HANDLER, {
                    method: "POST",
                    body: formData
                })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        mostrarOk(resp.mensaje);
                        modalConfig.hide();
                        consultar();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function eliminar(id) {
                id = entero(id);

                if (id <= 0) {
                    mostrarError("ID inválido.");
                    return;
                }

                Swal.fire({
                    title: "Eliminar configuración",
                    text: "La configuración será desactivada. ¿Deseas continuar?",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Sí, eliminar",
                    cancelButtonText: "No"
                }).then(function (result) {
                    if (!result.isConfirmed) {
                        return;
                    }

                    const formData = new FormData();
                    formData.append("action", "eliminar");
                    formData.append("id", id);

                    fetchJson(HANDLER, {
                        method: "POST",
                        body: formData
                    })
                        .then(function (resp) {
                            if (!resp.ok) {
                                mostrarError(resp.mensaje);
                                return;
                            }

                            mostrarOk(resp.mensaje);
                            consultar();
                        })
                        .catch(function (error) {
                            mostrarError(error.message);
                        });
                });
            }

        });
    </script>

</asp:Content>