<%@ Page Title="Alertas P.L.D." Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="alertas_pld.aspx.vb" Inherits="PLD.alertas_pld" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid mt-4">

        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h4 class="mb-1">
                    <i class="fa fa-bullhorn me-2"></i>
                    Alertas P.L.D.
                </h4>
                <small class="text-muted">
                    Bandeja de revisión, seguimiento y generación de alertas PLD.
                </small>
            </div>

            <div class="d-flex gap-2">
                <button type="button" id="btnProbarGeneracion" class="btn btn-warning btn-sm d-none">
                    Probar generación
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
                        <label for="filtroEstatus" class="form-label">Estatus</label>
                        <select id="filtroEstatus" class="form-control">
                            <option value="0">Todos</option>
                            <option value="1">Nueva</option>
                            <option value="2">En revisión</option>
                            <option value="3">Confirmada</option>
                            <option value="4">Descartada</option>
                            <option value="5">Cerrada</option>
                        </select>
                    </div>

                    <div class="col-md-3">
                        <label for="filtroCategoria" class="form-label">Categoría</label>
                        <select id="filtroCategoria" class="form-control">
                            <option value="0">Todas</option>
                        </select>
                    </div>

                    <div class="col-md-4">
                        <label for="filtroBusqueda" class="form-label">Buscar</label>
                        <input type="text" id="filtroBusqueda" class="form-control" placeholder="Folio, cliente, RFC, motivo..." />
                    </div>

                    <div class="col-md-2 d-flex align-items-end">
                        <button type="button" id="btnBuscar" class="btn btn-success w-100">
                            <i class="fa fa-search me-1"></i>
                            Buscar
                        </button>
                    </div>

                </div>
            </div>
        </div>

        <div class="card shadow-sm">
            <div class="card-body">

                <div id="loaderAlertas" class="text-center py-4 d-none">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <div class="mt-2 text-muted">Cargando alertas...</div>
                </div>

                <div class="table-responsive">
                    <table id="tablaAlertas" class="table table-sm table-striped table-hover align-middle w-100">
                        <thead class="table-dark">
                            <tr>
                                <th>Folio</th>
                                <th>Fecha</th>
                                <th>Categoría</th>
                                <th>Motivo</th>
                                <th>Cliente</th>
                                <th>RFC</th>
                                <th>Nivel</th>
                                <th>Prioridad</th>
                                <th>Estatus</th>
                                <th>Asignado</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="tbodyAlertas">
                        </tbody>
                    </table>
                </div>

            </div>
        </div>

    </div>

    <!-- Modal Detalle -->
    <div class="modal fade" id="modalDetalleAlerta" tabindex="-1" aria-labelledby="modalDetalleAlertaLabel" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable">
            <div class="modal-content">

                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalDetalleAlertaLabel">
                        <i class="fa fa-eye me-2"></i>
                        Detalle de alerta
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <input type="hidden" id="detalleAlertaId" />

                    <div class="row g-3 mb-3">

                        <div class="col-md-3">
                            <label class="form-label">Folio</label>
                            <input type="text" id="detalleFolio" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Estatus</label>
                            <input type="text" id="detalleEstatus" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Categoría</label>
                            <input type="text" id="detalleCategoria" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Motivo</label>
                            <input type="text" id="detalleMotivo" class="form-control" readonly />
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Cliente</label>
                            <input type="text" id="detalleCliente" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">RFC</label>
                            <input type="text" id="detalleRfc" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Solicitud</label>
                            <input type="text" id="detalleSolicitud" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Impacto</label>
                            <input type="text" id="detalleImpacto" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Probabilidad</label>
                            <input type="text" id="detalleProbabilidad" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Nivel riesgo PLD</label>
                            <input type="text" id="detalleNivel" class="form-control" readonly />
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Prioridad</label>
                            <input type="text" id="detallePrioridad" class="form-control" readonly />
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Título</label>
                            <input type="text" id="detalleTitulo" class="form-control" readonly />
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Regla</label>
                            <input type="text" id="detalleRegla" class="form-control" readonly />
                        </div>

                        <div class="col-md-12">
                            <label class="form-label">Descripción</label>
                            <textarea id="detalleDescripcion" class="form-control" rows="3" readonly></textarea>
                        </div>

                        <div class="col-md-12">
                            <label class="form-label">Resolución actual</label>
                            <textarea id="detalleResolucionActual" class="form-control" rows="2" readonly></textarea>
                        </div>

                    </div>

                    <hr />

                    <h6 class="mb-2">
                        <i class="fa fa-search me-1"></i>
                        Investigación / análisis de Cumplimiento
                    </h6>

                    <div class="row g-3 mb-3">
                        <div class="col-md-3">
                            <label for="investigacionResultado" class="form-label">Resultado</label>
                            <select id="investigacionResultado" class="form-select">
                                <option value="EN_ANALISIS">En análisis</option>
                                <option value="JUSTIFICADA">Justificada</option>
                                <option value="NO_JUSTIFICADA">No justificada</option>
                            </select>
                        </div>
                        <div class="col-md-7">
                            <label for="investigacionComentario" class="form-label">Comentario de Cumplimiento *</label>
                            <textarea id="investigacionComentario" class="form-control" rows="2" placeholder="Documenta análisis, evidencia o conclusión..."></textarea>
                        </div>
                        <div class="col-md-2 d-flex align-items-end">
                            <button type="button" id="btnAgregarInvestigacion" class="btn btn-outline-primary w-100">
                                Agregar análisis
                            </button>
                        </div>
                        <div class="col-12">
                            <div class="form-text">Registrar un análisis no cambia automáticamente el estatus operativo de la alerta.</div>
                        </div>
                    </div>

                    <div class="table-responsive mb-4">
                        <table class="table table-sm table-bordered align-middle w-100">
                            <thead class="table-light">
                                <tr>
                                    <th>Fecha</th>
                                    <th>Resultado</th>
                                    <th>Categoría</th>
                                    <th>Origen</th>
                                    <th>Usuario</th>
                                    <th>Comentario</th>
                                </tr>
                            </thead>
                            <tbody id="tbodyInvestigacion"></tbody>
                        </table>
                    </div>

                    <hr />

                    <h6 class="mb-2">
                        <i class="fa fa-history me-1"></i>
                        Bitácora
                    </h6>

                    <div class="table-responsive">
                        <table class="table table-sm table-bordered align-middle w-100">
                            <thead class="table-light">
                                <tr>
                                    <th>Fecha</th>
                                    <th>Movimiento</th>
                                    <th>Anterior</th>
                                    <th>Nuevo</th>
                                    <th>Usuario</th>
                                    <th>Comentario</th>
                                </tr>
                            </thead>
                            <tbody id="tbodyBitacora">
                            </tbody>
                        </table>
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnAbrirAsignar" class="btn btn-info">
                        <i class="fa fa-user-check me-1"></i>
                        Asignar
                    </button>

                    <button type="button" id="btnAbrirCambioEstatus" class="btn btn-warning">
                        <i class="fa fa-exchange-alt me-1"></i>
                        Cambiar estatus
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cerrar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <!-- Modal Cambio Estatus -->
    <div class="modal fade" id="modalCambioEstatus" tabindex="-1" aria-labelledby="modalCambioEstatusLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">

                <div class="modal-header bg-warning">
                    <h5 class="modal-title" id="modalCambioEstatusLabel">
                        <i class="fa fa-exchange-alt me-2"></i>
                        Cambiar estatus
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <input type="hidden" id="estatusAlertaId" />

                    <div class="mb-3">
                        <label for="nuevoEstatus" class="form-label">Nuevo estatus</label>
                        <select id="nuevoEstatus" class="form-control">
                            <option value="1">Nueva</option>
                            <option value="2">En revisión</option>
                            <option value="3">Confirmada</option>
                            <option value="4">Descartada</option>
                            <option value="5">Cerrada</option>
                        </select>
                    </div>

                    <div class="mb-3">
                        <label for="comentarioEstatus" class="form-label">Comentario</label>
                        <textarea id="comentarioEstatus" class="form-control" rows="3" placeholder="Comentario del movimiento..."></textarea>
                    </div>

                    <div class="mb-3">
                        <label for="resolucionEstatus" class="form-label">Resolución</label>
                        <textarea id="resolucionEstatus" class="form-control" rows="3" placeholder="Resolución, si aplica..."></textarea>
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnGuardarEstatus" class="btn btn-warning">
                        <i class="fa fa-save me-1"></i>
                        Guardar cambio
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <!-- Modal Asignar -->
    <div class="modal fade" id="modalAsignarAlerta" tabindex="-1" aria-labelledby="modalAsignarAlertaLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">

                <div class="modal-header bg-info text-white">
                    <h5 class="modal-title" id="modalAsignarAlertaLabel">
                        <i class="fa fa-user-check me-2"></i>
                        Asignar alerta
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <input type="hidden" id="asignarAlertaId" />

                    <div class="mb-3">
                        <label for="asignadoA" class="form-label">Asignado a</label>
                        <input type="text" id="asignadoA" class="form-control" placeholder="Usuario responsable..." />
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnGuardarAsignacion" class="btn btn-info text-white">
                        <i class="fa fa-save me-1"></i>
                        Guardar asignación
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <!-- Modal Generar desde Solicitud -->
    <div class="modal fade" id="modalGenerarSolicitud" tabindex="-1" aria-labelledby="modalGenerarSolicitudLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">

                <div class="modal-header bg-warning">
                    <h5 class="modal-title" id="modalGenerarSolicitudLabel">
                        <i class="fa fa-magic me-2"></i>
                        Probar generación desde solicitud
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <div class="alert alert-info mb-3">
                        Esta prueba llama al handler con <strong>action=generar_desde_solicitud</strong>.
                    </div>

                    <div class="mb-3">
                        <label for="solicitudGenerarId" class="form-label">ID de solicitud</label>
                        <input type="number" id="solicitudGenerarId" class="form-control" min="1" placeholder="Ej. 1" />
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnEjecutarGeneracionSolicitud" class="btn btn-warning">
                        <i class="fa fa-play me-1"></i>
                        Ejecutar
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            const HANDLER = "/handlers/handler_alertas_pld.ashx";

            let tablaAlertas = null;
            let modalDetalleAlerta = null;
            let modalCambioEstatus = null;
            let modalAsignarAlerta = null;
            let modalGenerarSolicitud = null;

            const $ = window.jQuery;

            inicializarModales();
            inicializarEventos();
            inicializarTabla();
            cargarCategorias();
            cargarAlertas();

            function inicializarModales() {
                modalDetalleAlerta = new bootstrap.Modal(document.getElementById("modalDetalleAlerta"));
                modalCambioEstatus = new bootstrap.Modal(document.getElementById("modalCambioEstatus"));
                modalAsignarAlerta = new bootstrap.Modal(document.getElementById("modalAsignarAlerta"));
                modalGenerarSolicitud = new bootstrap.Modal(document.getElementById("modalGenerarSolicitud"));
            }

            function inicializarEventos() {
                document.getElementById("btnBuscar").addEventListener("click", function () {
                    cargarAlertas();
                });

                document.getElementById("btnRefrescar").addEventListener("click", function () {
                    cargarAlertas();
                });

                const btnProbarGeneracion = document.getElementById("btnProbarGeneracion");

                if (btnProbarGeneracion) {
                    btnProbarGeneracion.addEventListener("click", function () {
                        document.getElementById("solicitudGenerarId").value = "";
                        modalGenerarSolicitud.show();
                    });
                }

                document.getElementById("btnEjecutarGeneracionSolicitud").addEventListener("click", function () {
                    generarDesdeSolicitud();
                });

                document.getElementById("btnAbrirCambioEstatus").addEventListener("click", function () {
                    const alertaId = document.getElementById("detalleAlertaId").value;
                    document.getElementById("estatusAlertaId").value = alertaId;
                    document.getElementById("comentarioEstatus").value = "";
                    document.getElementById("resolucionEstatus").value = "";
                    modalCambioEstatus.show();
                });

                document.getElementById("btnGuardarEstatus").addEventListener("click", function () {
                    guardarCambioEstatus();
                });

                document.getElementById("btnAbrirAsignar").addEventListener("click", function () {
                    const alertaId = document.getElementById("detalleAlertaId").value;
                    document.getElementById("asignarAlertaId").value = alertaId;
                    document.getElementById("asignadoA").value = "";
                    modalAsignarAlerta.show();
                });

                document.getElementById("btnGuardarAsignacion").addEventListener("click", function () {
                    guardarAsignacion();
                });

                document.getElementById("btnAgregarInvestigacion").addEventListener("click", function () {
                    agregarInvestigacion();
                });

                document.getElementById("filtroBusqueda").addEventListener("keyup", function (e) {
                    if (e.key === "Enter") {
                        cargarAlertas();
                    }
                });
            }

            function inicializarTabla() {
                if ($ && $.fn.DataTable) {
                    tablaAlertas = $("#tablaAlertas").DataTable({
                        pageLength: 10,
                        responsive: true,
                        ordering: true,
                        order: [],
                        destroy: true,
                        language: {
                            url: "https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-MX.json"
                        },
                        columnDefs: [
                            { orderable: false, targets: [10] }
                        ]
                    });
                }
            }

            function mostrarLoader(mostrar) {
                const loader = document.getElementById("loaderAlertas");

                if (mostrar) {
                    loader.classList.remove("d-none");
                } else {
                    loader.classList.add("d-none");
                }
            }

            function mostrarError(mensaje) {
                Swal.fire("Error", mensaje || "Ocurrió un error inesperado.", "error");
            }

            function mostrarExito(mensaje) {
                Swal.fire("Correcto", mensaje || "Operación realizada correctamente.", "success");
            }

            function valorSeguro(valor) {
                if (valor === null || valor === undefined) {
                    return "";
                }

                return String(valor);
            }

            function numeroSeguro(valor, decimales) {
                const num = parseFloat(valor);

                if (isNaN(num)) {
                    return "0";
                }

                return num.toFixed(decimales);
            }

            function fechaCorta(valor) {
                if (!valor) {
                    return "";
                }

                let fecha = null;

                if (typeof valor === "string" && valor.indexOf("/Date(") === 0) {
                    const match = /\/Date\((\d+)\)\//.exec(valor);

                    if (match && match[1]) {
                        fecha = new Date(parseInt(match[1], 10));
                    }
                } else {
                    fecha = new Date(valor);
                }

                if (!fecha || isNaN(fecha.getTime())) {
                    return valorSeguro(valor);
                }

                return fecha.toLocaleString("es-MX", {
                    year: "numeric",
                    month: "2-digit",
                    day: "2-digit",
                    hour: "2-digit",
                    minute: "2-digit"
                });
            }

            function badgeEstatus(id, texto) {
                const estatus = parseInt(id || 0, 10);

                if (estatus === 1) {
                    return '<span class="badge bg-primary">Nueva</span>';
                }

                if (estatus === 2) {
                    return '<span class="badge bg-warning text-dark">En revisión</span>';
                }

                if (estatus === 3) {
                    return '<span class="badge bg-danger">Confirmada</span>';
                }

                if (estatus === 4) {
                    return '<span class="badge bg-secondary">Descartada</span>';
                }

                if (estatus === 5) {
                    return '<span class="badge bg-success">Cerrada</span>';
                }

                return '<span class="badge bg-light text-dark">' + valorSeguro(texto || "N/A") + '</span>';
            }

            function badgePrioridad(id) {
                const prioridad = parseInt(id || 0, 10);

                if (prioridad === 3) {
                    return '<span class="badge bg-danger">3</span>';
                }

                if (prioridad === 2) {
                    return '<span class="badge bg-warning text-dark">2</span>';
                }

                return '<span class="badge bg-info text-dark">1</span>';
            }

            function construirUrl(action, params) {
                const url = new URL(HANDLER, window.location.origin);
                url.searchParams.set("action", action);

                if (params) {
                    Object.keys(params).forEach(function (key) {
                        url.searchParams.set(key, params[key]);
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

            function cargarCategorias() {
                fetchJson(construirUrl("categorias"))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        const select = document.getElementById("filtroCategoria");
                        select.innerHTML = '<option value="0">Todas</option>';

                        (resp.data || []).forEach(function (item) {
                            const option = document.createElement("option");
                            option.value = item.id;
                            option.textContent = item.descripcion;
                            select.appendChild(option);
                        });
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function cargarAlertas() {
                mostrarLoader(true);

                const params = {
                    estatus: document.getElementById("filtroEstatus").value || "0",
                    categoria_id: document.getElementById("filtroCategoria").value || "0",
                    q: document.getElementById("filtroBusqueda").value || ""
                };

                fetchJson(construirUrl("bandeja", params))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        renderizarAlertas(resp.data || []);
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    })
                    .finally(function () {
                        mostrarLoader(false);
                    });
            }

            function renderizarAlertas(data) {
                if (tablaAlertas) {
                    tablaAlertas.clear();
                } else {
                    document.getElementById("tbodyAlertas").innerHTML = "";
                }

                data.forEach(function (item) {
                    const acciones =
                        '<div class="btn-group btn-group-sm" role="group">' +
                            '<button type="button" class="btn btn-outline-primary btn-ver" data-id="' + item.id + '" title="Ver detalle">' +
                                '<i class="fa fa-eye"></i>' +
                            '</button>' +
                        '</div>';

                    const fila = [
                        valorSeguro(item.folio),
                        fechaCorta(item.fecha_generacion),
                        valorSeguro(item.categoria),
                        valorSeguro(item.motivo),
                        valorSeguro(item.cliente_nombre),
                        valorSeguro(item.cliente_rfc),
                        numeroSeguro(item.nivel_riesgo_pld, 2),
                        badgePrioridad(item.prioridad),
                        badgeEstatus(item.estatus_alerta, item.estatus_alerta_texto),
                        valorSeguro(item.asignado_a),
                        acciones
                    ];

                    if (tablaAlertas) {
                        tablaAlertas.row.add(fila);
                    } else {
                        const tr = document.createElement("tr");
                        fila.forEach(function (col) {
                            const td = document.createElement("td");
                            td.innerHTML = col;
                            tr.appendChild(td);
                        });
                        document.getElementById("tbodyAlertas").appendChild(tr);
                    }
                });

                if (tablaAlertas) {
                    tablaAlertas.draw();
                }

                asignarEventosTabla();
            }

            function asignarEventosTabla() {
                const tabla = document.getElementById("tablaAlertas");

                if (!tabla) {
                    return;
                }

                tabla.removeEventListener("click", manejarClickTablaAlertas);
                tabla.addEventListener("click", manejarClickTablaAlertas);
            }

            function manejarClickTablaAlertas(e) {
                const boton = e.target.closest(".btn-ver");

                if (!boton) {
                    return;
                }

                const id = boton.getAttribute("data-id");

                if (!id || parseInt(id, 10) <= 0) {
                    Swal.fire("Error", "No se encontró el ID de la alerta.", "error");
                    return;
                }

                obtenerDetalle(id);
            }

            function obtenerDetalle(id) {
                fetchJson(construirUrl("obtener", { id: id }))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        llenarDetalle(resp.data);
                        cargarInvestigacion(id);
                        cargarBitacora(id);
                        modalDetalleAlerta.show();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function llenarDetalle(item) {
                document.getElementById("detalleAlertaId").value = valorSeguro(item.id);
                document.getElementById("detalleFolio").value = valorSeguro(item.folio);
                document.getElementById("detalleEstatus").value = valorSeguro(item.estatus_alerta_texto);
                document.getElementById("detalleCategoria").value = valorSeguro(item.categoria);
                document.getElementById("detalleMotivo").value = valorSeguro(item.motivo);
                document.getElementById("detalleCliente").value = valorSeguro(item.cliente_nombre);
                document.getElementById("detalleRfc").value = valorSeguro(item.cliente_rfc);
                document.getElementById("detalleSolicitud").value = valorSeguro(item.solicitud_id);
                document.getElementById("detalleImpacto").value = valorSeguro(item.impacto);
                document.getElementById("detalleProbabilidad").value = valorSeguro(item.probabilidad);
                document.getElementById("detalleNivel").value = numeroSeguro(item.nivel_riesgo_pld, 2);
                document.getElementById("detallePrioridad").value = valorSeguro(item.prioridad);
                document.getElementById("detalleTitulo").value = valorSeguro(item.titulo);
                document.getElementById("detalleRegla").value = valorSeguro(item.nombre_regla);
                document.getElementById("detalleDescripcion").value = valorSeguro(item.descripcion);
                document.getElementById("detalleResolucionActual").value = valorSeguro(item.resolucion);
                document.getElementById("nuevoEstatus").value = valorSeguro(item.estatus_alerta || 1);
            }

            function cargarBitacora(alertaId) {
                fetchJson(construirUrl("bitacora", { alerta_id: alertaId }))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        const tbody = document.getElementById("tbodyBitacora");
                        tbody.innerHTML = "";

                        if (!resp.data || resp.data.length === 0) {
                            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Sin movimientos.</td></tr>';
                            return;
                        }

                        resp.data.forEach(function (item) {
                            const tr = document.createElement("tr");

                            tr.innerHTML =
                                '<td>' + fechaCorta(item.fecha_movimiento) + '</td>' +
                                '<td>' + valorSeguro(item.tipo_movimiento) + '</td>' +
                                '<td>' + valorSeguro(item.estatus_anterior) + '</td>' +
                                '<td>' + valorSeguro(item.estatus_nuevo) + '</td>' +
                                '<td>' + valorSeguro(item.usuario) + '</td>' +
                                '<td>' + valorSeguro(item.comentario) + '</td>';

                            tbody.appendChild(tr);
                        });
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function badgeInvestigacion(resultado) {
                const r = valorSeguro(resultado).toUpperCase();
                if (r === "JUSTIFICADA") return '<span class="badge bg-success">Justificada</span>';
                if (r === "NO_JUSTIFICADA") return '<span class="badge bg-danger">No justificada</span>';
                return '<span class="badge bg-warning text-dark">En análisis</span>';
            }

            function cargarInvestigacion(alertaId) {
                fetchJson(construirUrl("investigacion", { alerta_id: alertaId }))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        const tbody = document.getElementById("tbodyInvestigacion");
                        tbody.innerHTML = "";

                        if (!resp.data || resp.data.length === 0) {
                            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Sin análisis registrados.</td></tr>';
                            return;
                        }

                        resp.data.forEach(function (item) {
                            const tr = document.createElement("tr");
                            tr.innerHTML =
                                '<td>' + fechaCorta(item.fecha_investigacion) + '</td>' +
                                '<td>' + badgeInvestigacion(item.resultado) + '</td>' +
                                '<td>' + valorSeguro(item.categoria_alerta) + '</td>' +
                                '<td>' + valorSeguro(item.origen_evento) + '</td>' +
                                '<td>' + valorSeguro(item.usuario) + '</td>' +
                                '<td>' + valorSeguro(item.comentario) + '</td>';
                            tbody.appendChild(tr);
                        });
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function agregarInvestigacion() {
                const alertaId = document.getElementById("detalleAlertaId").value;
                const resultado = document.getElementById("investigacionResultado").value;
                const comentario = document.getElementById("investigacionComentario").value.trim();

                if (!alertaId || parseInt(alertaId, 10) <= 0) {
                    mostrarError("No se encontró la alerta.");
                    return;
                }
                if (!comentario) {
                    mostrarError("Captura el comentario de investigación.");
                    return;
                }

                const formData = new FormData();
                formData.append("action", "agregar_investigacion");
                formData.append("alerta_id", alertaId);
                formData.append("resultado", resultado);
                formData.append("comentario", comentario);

                fetchJson(HANDLER, { method: "POST", body: formData })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        document.getElementById("investigacionComentario").value = "";
                        mostrarExito(resp.mensaje);
                        cargarInvestigacion(alertaId);
                        cargarBitacora(alertaId);
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function guardarCambioEstatus() {
                const alertaId = document.getElementById("estatusAlertaId").value;
                const nuevoEstatus = document.getElementById("nuevoEstatus").value;
                const comentario = document.getElementById("comentarioEstatus").value.trim();
                const resolucion = document.getElementById("resolucionEstatus").value.trim();

                if (!alertaId || parseInt(alertaId, 10) <= 0) {
                    mostrarError("No se encontró la alerta.");
                    return;
                }

                if (!nuevoEstatus || parseInt(nuevoEstatus, 10) <= 0) {
                    mostrarError("Selecciona un estatus válido.");
                    return;
                }

                const formData = new FormData();
                formData.append("action", "cambiar_estatus");
                formData.append("alerta_id", alertaId);
                formData.append("estatus_alerta", nuevoEstatus);
                formData.append("comentario", comentario);
                formData.append("resolucion", resolucion);

                fetchJson(HANDLER, {
                    method: "POST",
                    body: formData
                })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        mostrarExito(resp.mensaje);
                        modalCambioEstatus.hide();
                        modalDetalleAlerta.hide();
                        cargarAlertas();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function guardarAsignacion() {
                const alertaId = document.getElementById("asignarAlertaId").value;
                const asignadoA = document.getElementById("asignadoA").value.trim();

                if (!alertaId || parseInt(alertaId, 10) <= 0) {
                    mostrarError("No se encontró la alerta.");
                    return;
                }

                if (!asignadoA) {
                    mostrarError("Captura el usuario responsable.");
                    return;
                }

                const formData = new FormData();
                formData.append("action", "asignar");
                formData.append("alerta_id", alertaId);
                formData.append("asignado_a", asignadoA);

                fetchJson(HANDLER, {
                    method: "POST",
                    body: formData
                })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        mostrarExito(resp.mensaje);
                        modalAsignarAlerta.hide();
                        modalDetalleAlerta.hide();
                        cargarAlertas();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function generarDesdeSolicitud() {
                const solicitudId = document.getElementById("solicitudGenerarId").value;

                if (!solicitudId || parseInt(solicitudId, 10) <= 0) {
                    mostrarError("Captura un ID de solicitud válido.");
                    return;
                }

                const formData = new FormData();
                formData.append("action", "generar_desde_solicitud");
                formData.append("solicitud_id", solicitudId);

                fetchJson(HANDLER, {
                    method: "POST",
                    body: formData
                })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        Swal.fire(
                            "Evaluación finalizada",
                            "Alertas generadas: " + resp.generadas + ". Duplicadas omitidas: " + resp.omitidas_duplicado + ".",
                            "success"
                        );

                        modalGenerarSolicitud.hide();
                        cargarAlertas();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

        });
    </script>

</asp:Content>