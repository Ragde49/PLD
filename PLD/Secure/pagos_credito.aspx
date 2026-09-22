<%@ Page Title="Pagos de Crédito P.L.D." Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="pagos_credito.aspx.vb" Inherits="PLD.pagos_credito" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .buscador-referencia {
            position: relative;
        }

        .buscador-referencia-resultados {
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            z-index: 1080;
            max-height: 320px;
            overflow-y: auto;
            margin-top: .25rem;
            background: #fff;
            border: 1px solid #d8dee6;
            border-radius: .5rem;
            box-shadow: 0 .75rem 2rem rgba(16, 24, 40, .14);
        }

        .buscador-referencia-opcion {
            width: 100%;
            padding: .7rem .85rem;
            border: 0;
            border-bottom: 1px solid #eef1f4;
            background: #fff;
            text-align: left;
        }

        .buscador-referencia-opcion:last-child {
            border-bottom: 0;
        }

        .buscador-referencia-opcion:hover,
        .buscador-referencia-opcion.active {
            background: #eef8f1;
        }

        .referencia-seleccionada {
            border-left: 4px solid #198754;
            background: #f3fbf6;
        }
    </style>

    <div class="container-fluid mt-4">

        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h4 class="mb-1">
                    <i class="fa fa-money-bill-wave me-2"></i>
                    Pagos de Crédito P.L.D.
                </h4>
                <small class="text-muted">
                    Captura, aplicación y monitoreo PLD de pagos asociados a solicitudes de crédito.
                </small>
            </div>

            <div class="d-flex gap-2">
                <button type="button" id="btnNuevoPago" class="btn btn-success btn-sm">
                    <i class="fa fa-plus me-1"></i>
                    Capturar pago
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

                    <div class="col-md-4">
                        <label for="filtroReferenciaCredito" class="form-label">Cliente o crédito</label>
                        <div class="buscador-referencia">
                            <input type="search" id="filtroReferenciaCredito" class="form-control" autocomplete="off"
                                placeholder="Nombre, RFC, CURP o solicitud..." />
                            <input type="hidden" id="filtroSolicitudId" />
                            <input type="hidden" id="filtroClienteId" />
                            <div id="filtroReferenciaResultados" class="buscador-referencia-resultados d-none" role="listbox"></div>
                        </div>
                        <div id="filtroReferenciaSeleccionada" class="form-text">Escribe al menos 2 caracteres y selecciona una opción.</div>
                    </div>

                    <div class="col-md-2">
                        <label for="filtroEstatus" class="form-label">Estatus</label>
                        <select id="filtroEstatus" class="form-control">
                            <option value="">Todos</option>
                            <option value="APLICADO">APLICADO</option>
                            <option value="CANCELADO">CANCELADO</option>
                            <option value="DEVUELTO">DEVUELTO</option>
                        </select>
                    </div>

                    <div class="col-md-4">
                        <label for="filtroBusqueda" class="form-label">Buscar</label>
                        <input type="text" id="filtroBusqueda" class="form-control" placeholder="Folio, referencia, estatus..." />
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

                <div id="loaderPagos" class="text-center py-4 d-none">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <div class="mt-2 text-muted">Cargando pagos...</div>
                </div>

                <div class="table-responsive">
                    <table id="tablaPagos" class="table table-sm table-striped table-hover align-middle w-100">
                        <thead class="table-dark">
                            <tr>
                                <th>Folio</th>
                                <th>Fecha</th>
                                <th>Solicitud</th>
                                <th>Tipo pago</th>
                                <th>Canal</th>
                                <th>Moneda</th>
                                <th>Monto</th>
                                <th>Saldo después</th>
                                <th>PLD</th>
                                <th>Estatus</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody id="tbodyPagos">
                        </tbody>
                    </table>
                </div>

            </div>
        </div>

    </div>

    <!-- Modal Capturar Pago -->
    <div class="modal fade" id="modalPago" tabindex="-1" aria-labelledby="modalPagoLabel" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable">
            <div class="modal-content">

                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title" id="modalPagoLabel">
                        <i class="fa fa-plus-circle me-2"></i>
                        Capturar pago
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">

                    <div class="alert alert-info">
                        Al aplicar el pago, el handler ejecutará automáticamente el motor de Alertas P.L.D. Fase 2.
                    </div>

                    <div class="row g-3">

                        <div class="col-md-6">
                            <label for="pagoReferenciaCredito" class="form-label">Cliente o crédito *</label>
                            <div class="buscador-referencia">
                                <input type="search" id="pagoReferenciaCredito" class="form-control" autocomplete="off"
                                    placeholder="Busca por solicitud, cliente, RFC o CURP..." />
                                <input type="hidden" id="pagoSolicitudId" />
                                <input type="hidden" id="pagoClienteId" />
                                <div id="pagoReferenciaResultados" class="buscador-referencia-resultados d-none" role="listbox"></div>
                            </div>
                            <div id="pagoReferenciaAyuda" class="form-text">Escribe al menos 2 caracteres y elige un crédito.</div>
                        </div>

                        <div class="col-12 d-none" id="pagoCreditoResumenContenedor">
                            <div class="referencia-seleccionada rounded p-3" id="pagoCreditoResumen"></div>
                        </div>

                        <div class="col-md-3">
                            <label for="pagoFecha" class="form-label">Fecha pago</label>
                            <input type="datetime-local" id="pagoFecha" class="form-control" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoReferencia" class="form-label">Referencia</label>
                            <input type="text" id="pagoReferencia" class="form-control" maxlength="100" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoTipoPagoId" class="form-label">Tipo pago *</label>
                            <select id="pagoTipoPagoId" class="form-control">
                                <option value="">Selecciona...</option>
                            </select>
                        </div>

                        <div class="col-md-3">
                            <label for="pagoCanalPagoId" class="form-label">Canal pago *</label>
                            <select id="pagoCanalPagoId" class="form-control">
                                <option value="">Selecciona...</option>
                            </select>
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMonedaId" class="form-label">Moneda *</label>
                            <select id="pagoMonedaId" class="form-control">
                                <option value="">Selecciona...</option>
                            </select>
                        </div>

                        <div class="col-md-3">
                            <label for="pagoAplicacionPagoId" class="form-label">Aplicación pago</label>
                            <select id="pagoAplicacionPagoId" class="form-control">
                                <option value="">General / Sin especificar</option>
                            </select>
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMonto" class="form-label">Monto pago *</label>
                            <input type="number" id="pagoMonto" class="form-control" min="0" step="0.01" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoTipoCambio" class="form-label">Tipo cambio *</label>
                            <input type="number" id="pagoTipoCambio" class="form-control" min="0" step="0.000001" value="1" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoEquivalenteMxn" class="form-label">Equivalente MXN</label>
                            <input type="number" id="pagoEquivalenteMxn" class="form-control" min="0" step="0.01" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoEquivalenteUsd" class="form-label">Equivalente USD</label>
                            <input type="number" id="pagoEquivalenteUsd" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoCapital" class="form-label">Capital</label>
                            <input type="number" id="pagoMontoCapital" class="form-control" min="0" step="0.01" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoInteres" class="form-label">Interés</label>
                            <input type="number" id="pagoMontoInteres" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoIva" class="form-label">IVA</label>
                            <input type="number" id="pagoMontoIva" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoMoratorio" class="form-label">Moratorio</label>
                            <input type="number" id="pagoMontoMoratorio" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoComisiones" class="form-label">Comisiones</label>
                            <input type="number" id="pagoMontoComisiones" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoOtros" class="form-label">Otros</label>
                            <input type="number" id="pagoMontoOtros" class="form-control" min="0" step="0.01" value="0" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoMontoCreditoOriginal" class="form-label">Monto crédito original</label>
                            <input type="number" id="pagoMontoCreditoOriginal" class="form-control bg-light" min="0" step="0.01" readonly />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoSaldoAntes" class="form-label">Saldo antes</label>
                            <input type="number" id="pagoSaldoAntes" class="form-control bg-light" min="0" step="0.01" readonly />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoSaldoDespues" class="form-label">Saldo después</label>
                            <input type="number" id="pagoSaldoDespues" class="form-control" min="0" step="0.01" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoFijoContractual" class="form-label">Pago fijo contractual</label>
                            <input type="number" id="pagoFijoContractual" class="form-control" min="0" step="0.01" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoNumeroPago" class="form-label">Número pago</label>
                            <input type="number" id="pagoNumeroPago" class="form-control bg-light" min="1" readonly />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoPlazoTotal" class="form-label">Plazo total</label>
                            <input type="number" id="pagoPlazoTotal" class="form-control bg-light" min="0" readonly />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoPorcentajePlazo" class="form-label">% plazo transcurrido</label>
                            <input type="number" id="pagoPorcentajePlazo" class="form-control" min="0" max="100" step="0.0001" />
                        </div>

                        <div class="col-md-3">
                            <label for="pagoPorcentajePagado" class="form-label">% pagado crédito</label>
                            <input type="number" id="pagoPorcentajePagado" class="form-control" min="0" step="0.0001" />
                        </div>

                        <div class="col-md-12">
                            <div class="card border-warning">
                                <div class="card-header bg-warning text-dark">
                                    <i class="fa fa-shield-alt me-1"></i>
                                    Indicadores P.L.D.
                                </div>
                                <div class="card-body">
                                    <div class="row g-3">

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoEsEfectivo" class="form-check-input" />
                                                <label for="pagoEsEfectivo" class="form-check-label">Es efectivo</label>
                                            </div>
                                        </div>

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoEsMonedaExtranjera" class="form-check-input" />
                                                <label for="pagoEsMonedaExtranjera" class="form-check-label">Moneda extranjera</label>
                                            </div>
                                        </div>

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoEsExcedente" class="form-check-input" />
                                                <label for="pagoEsExcedente" class="form-check-label">Pago excedente</label>
                                            </div>
                                        </div>

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoEsLiquidacion" class="form-check-input" />
                                                <label for="pagoEsLiquidacion" class="form-check-label">Liquidación</label>
                                            </div>
                                        </div>

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoEsLiquidacionAnticipada" class="form-check-input" />
                                                <label for="pagoEsLiquidacionAnticipada" class="form-check-label">Liquidación anticipada</label>
                                            </div>
                                        </div>

                                        <div class="col-md-3">
                                            <div class="form-check">
                                                <input type="checkbox" id="pagoRequiereDevolucion" class="form-check-input" />
                                                <label for="pagoRequiereDevolucion" class="form-check-label">Requiere devolución</label>
                                            </div>
                                        </div>

                                        <div class="col-md-6">
                                            <label for="pagoMotivoDevolucion" class="form-label">Motivo devolución</label>
                                            <input type="text" id="pagoMotivoDevolucion" class="form-control" maxlength="300" />
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="col-md-12">
                            <label for="pagoObservaciones" class="form-label">Observaciones</label>
                            <textarea id="pagoObservaciones" class="form-control" rows="3"></textarea>
                        </div>

                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnGuardarPago" class="btn btn-success">
                        <i class="fa fa-save me-1"></i>
                        Aplicar pago
                    </button>

                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <!-- Modal Detalle -->
    <div class="modal fade" id="modalDetallePago" tabindex="-1" aria-labelledby="modalDetallePagoLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-scrollable">
            <div class="modal-content">

                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="modalDetallePagoLabel">
                        <i class="fa fa-eye me-2"></i>
                        Detalle de pago
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                </div>

                <div class="modal-body">
                    <div id="detallePagoContenido"></div>
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cerrar
                    </button>
                </div>

            </div>
        </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            const HANDLER = "/handlers/handler_pagos_credito.ashx";

            let tablaPagos = null;
            let modalPago = null;
            let modalDetallePago = null;
            let catalogos = {
                tipo_pago: [],
                canal_pago: [],
                monedas: [],
                aplicacion_pago: []
            };

            const $ = window.jQuery;

            inicializarModales();
            inicializarEventos();
            inicializarAutocompletados();
            inicializarTabla();
            cargarCatalogos();
            cargarPagos();

            function inicializarModales() {
                modalPago = new bootstrap.Modal(document.getElementById("modalPago"));
                modalDetallePago = new bootstrap.Modal(document.getElementById("modalDetallePago"));
            }

            function inicializarEventos() {
                document.getElementById("btnNuevoPago").addEventListener("click", function () {
                    limpiarFormularioPago();
                    modalPago.show();
                    setTimeout(function () {
                        document.getElementById("pagoReferenciaCredito").focus();
                    }, 250);
                });

                document.getElementById("btnRefrescar").addEventListener("click", function () {
                    cargarPagos();
                });

                document.getElementById("btnBuscar").addEventListener("click", function () {
                    const textoReferencia = document.getElementById("filtroReferenciaCredito").value.trim();
                    const solicitudId = document.getElementById("filtroSolicitudId").value;
                    const clienteId = document.getElementById("filtroClienteId").value;

                    if (textoReferencia && !solicitudId && !clienteId) {
                        mostrarError("Selecciona un cliente o crédito de la lista de resultados.");
                        return;
                    }
                    cargarPagos();
                });

                document.getElementById("filtroBusqueda").addEventListener("keyup", function (e) {
                    if (e.key === "Enter") {
                        cargarPagos();
                    }
                });

                document.getElementById("btnGuardarPago").addEventListener("click", function () {
                    guardarPago();
                });

                document.getElementById("tablaPagos").addEventListener("click", function (e) {
                    const btnVer = e.target.closest(".btn-ver-pago");
                    const btnCancelar = e.target.closest(".btn-cancelar-pago");

                    if (btnVer) {
                        obtenerPago(btnVer.getAttribute("data-id"));
                    }

                    if (btnCancelar) {
                        cancelarPago(btnCancelar.getAttribute("data-id"));
                    }
                });

                const camposCalculo = [
                    "pagoMonto",
                    "pagoTipoCambio",
                    "pagoMontoCapital",
                    "pagoSaldoAntes",
                    "pagoMontoCreditoOriginal",
                    "pagoFijoContractual"
                ];

                camposCalculo.forEach(function (id) {
                    const el = document.getElementById(id);
                    if (el) {
                        el.addEventListener("input", recalcularPago);
                    }
                });

                document.getElementById("pagoTipoPagoId").addEventListener("change", detectarFlagsPLD);
                document.getElementById("pagoMonedaId").addEventListener("change", detectarFlagsPLD);
            }

            function inicializarAutocompletados() {
                configurarAutocompletado({
                    inputId: "filtroReferenciaCredito",
                    resultadosId: "filtroReferenciaResultados",
                    modo: "filtro",
                    alSeleccionar: seleccionarFiltroReferencia,
                    alLimpiar: limpiarFiltroReferencia
                });

                configurarAutocompletado({
                    inputId: "pagoReferenciaCredito",
                    resultadosId: "pagoReferenciaResultados",
                    modo: "captura",
                    alSeleccionar: seleccionarCreditoPago,
                    alLimpiar: limpiarCreditoPago
                });
            }

            function configurarAutocompletado(config) {
                const input = document.getElementById(config.inputId);
                const resultados = document.getElementById(config.resultadosId);
                let temporizador = null;
                let controlador = null;
                let indiceActivo = -1;

                function cerrarResultados() {
                    resultados.classList.add("d-none");
                    resultados.innerHTML = "";
                    indiceActivo = -1;
                    input.setAttribute("aria-expanded", "false");
                }

                function activarOpcion(indice) {
                    const opciones = Array.from(resultados.querySelectorAll(".buscador-referencia-opcion"));
                    if (!opciones.length) return;

                    indiceActivo = Math.max(0, Math.min(indice, opciones.length - 1));
                    opciones.forEach(function (opcion, i) {
                        opcion.classList.toggle("active", i === indiceActivo);
                    });
                    opciones[indiceActivo].scrollIntoView({ block: "nearest" });
                }

                function mostrarEstado(texto) {
                    resultados.innerHTML = "";
                    const estado = document.createElement("div");
                    estado.className = "px-3 py-2 text-muted small";
                    estado.textContent = texto;
                    resultados.appendChild(estado);
                    resultados.classList.remove("d-none");
                    input.setAttribute("aria-expanded", "true");
                }

                function renderizarResultados(items) {
                    resultados.innerHTML = "";
                    indiceActivo = -1;

                    if (!items.length) {
                        mostrarEstado("No se encontraron coincidencias.");
                        return;
                    }

                    items.forEach(function (item) {
                        const opcion = document.createElement("button");
                        opcion.type = "button";
                        opcion.className = "buscador-referencia-opcion";
                        opcion.setAttribute("role", "option");

                        const titulo = document.createElement("div");
                        titulo.className = "fw-semibold";
                        titulo.textContent = item.tipo === "cliente"
                            ? "Cliente #" + valorSeguro(item.cliente_id) + " · " + valorSeguro(item.cliente_nombre)
                            : "Solicitud #" + valorSeguro(item.solicitud_credito_id) + " · " + valorSeguro(item.cliente_nombre);

                        const detalle = document.createElement("div");
                        detalle.className = "small text-muted mt-1";

                        if (item.tipo === "cliente") {
                            detalle.textContent = "RFC: " + (valorSeguro(item.rfc) || "Sin RFC") +
                                (item.curp ? " · CURP: " + valorSeguro(item.curp) : "") +
                                " · Todos sus créditos";
                        } else {
                            detalle.textContent = "RFC: " + (valorSeguro(item.rfc) || "Sin RFC") +
                                " · Monto: $" + numero(item.monto_solicitado, 2) +
                                " · Saldo: $" + numero(item.saldo_vigente, 2) +
                                " · " + valorSeguro(item.estatus_solicitud);
                        }

                        opcion.appendChild(titulo);
                        opcion.appendChild(detalle);
                        opcion.addEventListener("click", function () {
                            input.value = etiquetaReferencia(item);
                            cerrarResultados();
                            config.alSeleccionar(item);
                        });
                        resultados.appendChild(opcion);
                    });

                    resultados.classList.remove("d-none");
                    input.setAttribute("aria-expanded", "true");
                }

                function buscar() {
                    const q = input.value.trim();
                    if (q.length < 2) {
                        cerrarResultados();
                        return;
                    }

                    if (controlador) controlador.abort();
                    controlador = new AbortController();
                    mostrarEstado("Buscando...");

                    fetchJson(construirUrl("buscar_referencias", {
                        q: q,
                        modo: config.modo,
                        limite: 20
                    }), { signal: controlador.signal })
                        .then(function (resp) {
                            if (!resp.ok) throw new Error(resp.mensaje || "No fue posible buscar créditos.");
                            renderizarResultados(resp.data || []);
                        })
                        .catch(function (error) {
                            if (error.name === "AbortError") return;
                            mostrarEstado(error.message);
                        });
                }

                input.setAttribute("role", "combobox");
                input.setAttribute("aria-autocomplete", "list");
                input.setAttribute("aria-expanded", "false");
                input.setAttribute("aria-controls", config.resultadosId);

                input.addEventListener("input", function () {
                    config.alLimpiar();
                    if (temporizador) clearTimeout(temporizador);

                    if (input.value.trim().length < 2) {
                        if (controlador) controlador.abort();
                        cerrarResultados();
                        return;
                    }

                    temporizador = setTimeout(buscar, 300);
                });

                input.addEventListener("keydown", function (e) {
                    const opciones = Array.from(resultados.querySelectorAll(".buscador-referencia-opcion"));
                    if (e.key === "ArrowDown" && opciones.length) {
                        e.preventDefault();
                        activarOpcion(indiceActivo + 1);
                    } else if (e.key === "ArrowUp" && opciones.length) {
                        e.preventDefault();
                        activarOpcion(indiceActivo <= 0 ? opciones.length - 1 : indiceActivo - 1);
                    } else if (e.key === "Enter" && indiceActivo >= 0 && opciones[indiceActivo]) {
                        e.preventDefault();
                        opciones[indiceActivo].click();
                    } else if (e.key === "Escape") {
                        cerrarResultados();
                    }
                });

                document.addEventListener("click", function (e) {
                    if (e.target !== input && !resultados.contains(e.target)) cerrarResultados();
                });
            }

            function etiquetaReferencia(item) {
                if (item.tipo === "cliente") {
                    return "Cliente #" + valorSeguro(item.cliente_id) + " · " + valorSeguro(item.cliente_nombre);
                }
                return "Solicitud #" + valorSeguro(item.solicitud_credito_id) + " · " + valorSeguro(item.cliente_nombre);
            }

            function seleccionarFiltroReferencia(item) {
                const texto = document.getElementById("filtroReferenciaSeleccionada");

                if (item.tipo === "cliente") {
                    document.getElementById("filtroSolicitudId").value = "";
                    document.getElementById("filtroClienteId").value = item.cliente_id || "";
                    texto.textContent = "Mostrando pagos de todos los créditos de " + valorSeguro(item.cliente_nombre) + ".";
                } else {
                    document.getElementById("filtroSolicitudId").value = item.solicitud_credito_id || "";
                    document.getElementById("filtroClienteId").value = "";
                    texto.textContent = "Filtrando por la solicitud #" + valorSeguro(item.solicitud_credito_id) + ".";
                }
            }

            function limpiarFiltroReferencia() {
                document.getElementById("filtroSolicitudId").value = "";
                document.getElementById("filtroClienteId").value = "";
                document.getElementById("filtroReferenciaSeleccionada").textContent =
                    "Escribe al menos 2 caracteres y selecciona una opción.";
            }

            function seleccionarCreditoPago(item) {
                document.getElementById("pagoSolicitudId").value = item.solicitud_credito_id || "";
                document.getElementById("pagoClienteId").value = item.cliente_id || "";
                const esRevolvente = item.es_revolvente === true || item.es_revolvente === 1 || item.es_revolvente === "1";
                document.getElementById("pagoMontoCreditoOriginal").value = numero(esRevolvente ? item.monto_autorizado : item.monto_solicitado, 2);
                document.getElementById("pagoSaldoAntes").value = numero(item.saldo_vigente, 2);
                document.getElementById("pagoSaldoDespues").value = numero(item.saldo_vigente, 2);
                document.getElementById("pagoNumeroPago").value = item.siguiente_numero_pago || 1;
                document.getElementById("pagoPlazoTotal").value = item.plazo || "";

                if (item.moneda_id) document.getElementById("pagoMonedaId").value = item.moneda_id;
                if (item.canal_pago_id) document.getElementById("pagoCanalPagoId").value = item.canal_pago_id;

                const resumen = document.getElementById("pagoCreditoResumen");
                resumen.textContent = "Solicitud #" + valorSeguro(item.solicitud_credito_id) +
                    " · " + valorSeguro(item.cliente_nombre) +
                    " · RFC: " + (valorSeguro(item.rfc) || "Sin RFC") +
                    " · Tipo: " + (valorSeguro(item.tipo_credito) || "—") +
                    " · Saldo vigente: $" + numero(item.saldo_vigente, 2) +
                    (esRevolvente ? " · Disponible: $" + numero(item.disponible, 2) : "") +
                    " · Estatus: " + valorSeguro(item.estatus_solicitud);
                document.getElementById("pagoCreditoResumenContenedor").classList.remove("d-none");
                document.getElementById("pagoReferenciaAyuda").textContent = "Crédito seleccionado correctamente.";
                detectarFlagsPLD();
                recalcularPago();
            }

            function limpiarCreditoPago() {
                document.getElementById("pagoSolicitudId").value = "";
                document.getElementById("pagoClienteId").value = "";
                document.getElementById("pagoMontoCreditoOriginal").value = "";
                document.getElementById("pagoSaldoAntes").value = "";
                document.getElementById("pagoSaldoDespues").value = "";
                document.getElementById("pagoNumeroPago").value = "";
                document.getElementById("pagoPlazoTotal").value = "";
                document.getElementById("pagoCreditoResumenContenedor").classList.add("d-none");
                document.getElementById("pagoCreditoResumen").textContent = "";
                document.getElementById("pagoReferenciaAyuda").textContent =
                    "Escribe al menos 2 caracteres y elige un crédito.";
            }

            function inicializarTabla() {
                if ($ && $.fn.DataTable) {
                    tablaPagos = $("#tablaPagos").DataTable({
                        pageLength: 10,
                        responsive: true,
                        ordering: true,
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
                const loader = document.getElementById("loaderPagos");

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

            function numero(valor, decimales) {
                const n = parseFloat(valor);

                if (isNaN(n)) {
                    return "0.00";
                }

                return n.toFixed(decimales);
            }

            function numeroRaw(valor) {
                const n = parseFloat(valor);

                if (isNaN(n)) {
                    return 0;
                }

                return n;
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

            function fechaInputAhora() {
                const f = new Date();
                f.setMinutes(f.getMinutes() - f.getTimezoneOffset());
                return f.toISOString().slice(0, 16);
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

            function badgeBoolean(valor, textoSi, textoNo) {
                if (valor === true || valor === 1 || valor === "1") {
                    return '<span class="badge bg-warning text-dark">' + textoSi + '</span>';
                }

                return '<span class="badge bg-light text-dark">' + textoNo + '</span>';
            }

            function badgeEstatus(estatus) {
                const e = valorSeguro(estatus).toUpperCase();

                if (e === "APLICADO") {
                    return '<span class="badge bg-success">APLICADO</span>';
                }

                if (e === "CANCELADO") {
                    return '<span class="badge bg-secondary">CANCELADO</span>';
                }

                if (e === "DEVUELTO") {
                    return '<span class="badge bg-info text-dark">DEVUELTO</span>';
                }

                return '<span class="badge bg-light text-dark">' + valorSeguro(estatus) + '</span>';
            }

            function cargarCatalogos() {
                fetchJson(construirUrl("catalogos"))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        catalogos = resp.data || catalogos;
                        llenarSelects();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function llenarSelects() {
                llenarSelect("pagoTipoPagoId", catalogos.tipo_pago, "id", "descripcion", "Selecciona...");
                llenarSelect("pagoCanalPagoId", catalogos.canal_pago, "id", "canal", "Selecciona...");
                llenarSelectMonedas();
                llenarSelectAplicacionPago();
            }

            function llenarSelect(id, data, valueField, textField, placeholder) {
                const select = document.getElementById(id);
                select.innerHTML = "";

                const opt = document.createElement("option");
                opt.value = "";
                opt.textContent = placeholder || "Selecciona...";
                select.appendChild(opt);

                (data || []).forEach(function (item) {
                    const option = document.createElement("option");
                    option.value = item[valueField];
                    option.textContent = item[textField];
                    select.appendChild(option);
                });
            }

            function llenarSelectMonedas() {
                const select = document.getElementById("pagoMonedaId");
                select.innerHTML = '<option value="">Selecciona...</option>';

                (catalogos.monedas || []).forEach(function (item) {
                    const option = document.createElement("option");
                    option.value = item.id;
                    option.setAttribute("data-clave", valorSeguro(item.clave));
                    option.textContent = valorSeguro(item.moneda) + (item.clave ? " (" + item.clave + ")" : "");
                    select.appendChild(option);
                });
            }

            function llenarSelectAplicacionPago() {
                const select = document.getElementById("pagoAplicacionPagoId");
                select.innerHTML = '<option value="">General / Sin especificar</option>';

                (catalogos.aplicacion_pago || []).forEach(function (item) {
                    const option = document.createElement("option");
                    option.value = item.id;
                    option.textContent = valorSeguro(item.descripcion) + (item.subtipo ? " - " + item.subtipo : "");
                    select.appendChild(option);
                });
            }

            function cargarPagos() {
                mostrarLoader(true);

                const params = {
                    solicitud_credito_id: document.getElementById("filtroSolicitudId").value || "",
                    cliente_id: document.getElementById("filtroClienteId").value || "",
                    estatus: document.getElementById("filtroEstatus").value || "",
                    q: document.getElementById("filtroBusqueda").value || ""
                };

                fetchJson(construirUrl("consultar", params))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        renderizarPagos(resp.data || []);
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    })
                    .finally(function () {
                        mostrarLoader(false);
                    });
            }

            function renderizarPagos(data) {
                if (tablaPagos) {
                    tablaPagos.clear();
                } else {
                    document.getElementById("tbodyPagos").innerHTML = "";
                }

                data.forEach(function (item) {
                    const flags =
                        badgeBoolean(item.es_efectivo, "Efectivo", "No efectivo") + " " +
                        (item.es_liquidacion ? '<span class="badge bg-danger">Liquidación</span>' : '') + " " +
                        (item.requiere_devolucion ? '<span class="badge bg-warning text-dark">Devolución</span>' : '');

                    const acciones =
                        '<div class="btn-group btn-group-sm" role="group">' +
                            '<button type="button" class="btn btn-outline-primary btn-ver-pago" data-id="' + item.id + '" title="Ver detalle">' +
                                '<i class="fa fa-eye"></i>' +
                            '</button>' +
                            '<button type="button" class="btn btn-outline-danger btn-cancelar-pago" data-id="' + item.id + '" title="Cancelar pago">' +
                                '<i class="fa fa-ban"></i>' +
                            '</button>' +
                        '</div>';

                    const fila = [
                        valorSeguro(item.folio_pago),
                        fechaCorta(item.fecha_pago),
                        valorSeguro(item.solicitud_credito_id),
                        valorSeguro(item.tipo_pago),
                        valorSeguro(item.canal_pago),
                        valorSeguro(item.moneda),
                        "$" + numero(item.monto_pago, 2),
                        "$" + numero(item.saldo_despues_pago, 2),
                        flags,
                        badgeEstatus(item.estatus),
                        acciones
                    ];

                    if (tablaPagos) {
                        tablaPagos.row.add(fila);
                    } else {
                        const tr = document.createElement("tr");

                        fila.forEach(function (col) {
                            const td = document.createElement("td");
                            td.innerHTML = col;
                            tr.appendChild(td);
                        });

                        document.getElementById("tbodyPagos").appendChild(tr);
                    }
                });

                if (tablaPagos) {
                    tablaPagos.draw();
                }
            }

            function limpiarFormularioPago() {
                document.getElementById("pagoReferenciaCredito").value = "";
                document.getElementById("pagoReferenciaResultados").classList.add("d-none");
                document.getElementById("pagoReferenciaResultados").innerHTML = "";
                limpiarCreditoPago();
                document.getElementById("pagoFecha").value = fechaInputAhora();
                document.getElementById("pagoReferencia").value = "";
                document.getElementById("pagoTipoPagoId").value = "";
                document.getElementById("pagoCanalPagoId").value = "";
                document.getElementById("pagoMonedaId").value = "1";
                document.getElementById("pagoAplicacionPagoId").value = "";
                document.getElementById("pagoMonto").value = "";
                document.getElementById("pagoTipoCambio").value = "1";
                document.getElementById("pagoEquivalenteMxn").value = "";
                document.getElementById("pagoEquivalenteUsd").value = "0";
                document.getElementById("pagoMontoCapital").value = "";
                document.getElementById("pagoMontoInteres").value = "0";
                document.getElementById("pagoMontoIva").value = "0";
                document.getElementById("pagoMontoMoratorio").value = "0";
                document.getElementById("pagoMontoComisiones").value = "0";
                document.getElementById("pagoMontoOtros").value = "0";
                document.getElementById("pagoMontoCreditoOriginal").value = "";
                document.getElementById("pagoSaldoAntes").value = "";
                document.getElementById("pagoSaldoDespues").value = "";
                document.getElementById("pagoFijoContractual").value = "";
                document.getElementById("pagoNumeroPago").value = "";
                document.getElementById("pagoPlazoTotal").value = "";
                document.getElementById("pagoPorcentajePlazo").value = "";
                document.getElementById("pagoPorcentajePagado").value = "";
                document.getElementById("pagoEsEfectivo").checked = false;
                document.getElementById("pagoEsMonedaExtranjera").checked = false;
                document.getElementById("pagoEsExcedente").checked = false;
                document.getElementById("pagoEsLiquidacion").checked = false;
                document.getElementById("pagoEsLiquidacionAnticipada").checked = false;
                document.getElementById("pagoRequiereDevolucion").checked = false;
                document.getElementById("pagoMotivoDevolucion").value = "";
                document.getElementById("pagoObservaciones").value = "";
            }

            function recalcularPago() {
                const monto = numeroRaw(document.getElementById("pagoMonto").value);
                const tipoCambio = numeroRaw(document.getElementById("pagoTipoCambio").value) || 1;
                const capital = numeroRaw(document.getElementById("pagoMontoCapital").value || monto);
                const saldoAntes = numeroRaw(document.getElementById("pagoSaldoAntes").value);
                const montoCredito = numeroRaw(document.getElementById("pagoMontoCreditoOriginal").value);

                if (monto > 0) {
                    document.getElementById("pagoEquivalenteMxn").value = numero(monto * tipoCambio, 2);

                    if (!document.getElementById("pagoMontoCapital").value) {
                        document.getElementById("pagoMontoCapital").value = numero(monto, 2);
                    }
                }

                if (saldoAntes > 0) {
                    const saldoDespues = Math.max(saldoAntes - capital, 0);
                    document.getElementById("pagoSaldoDespues").value = numero(saldoDespues, 2);
                    document.getElementById("pagoEsExcedente").checked = monto > saldoAntes;
                    document.getElementById("pagoEsLiquidacion").checked = saldoDespues <= 0;
                }

                if (montoCredito > 0 && monto > 0) {
                    document.getElementById("pagoPorcentajePagado").value = numero((monto / montoCredito) * 100, 4);
                }

                detectarFlagsPLD();
            }

            function detectarFlagsPLD() {
                const tipoPagoSelect = document.getElementById("pagoTipoPagoId");
                const monedaSelect = document.getElementById("pagoMonedaId");

                const tipoTexto = tipoPagoSelect.options[tipoPagoSelect.selectedIndex] ? tipoPagoSelect.options[tipoPagoSelect.selectedIndex].text.toUpperCase() : "";
                const monedaOption = monedaSelect.options[monedaSelect.selectedIndex];
                const claveMoneda = monedaOption ? valorSeguro(monedaOption.getAttribute("data-clave")).toUpperCase() : "";

                document.getElementById("pagoEsEfectivo").checked = tipoTexto.indexOf("EFECTIVO") >= 0;

                if (claveMoneda && claveMoneda !== "MXN" && claveMoneda !== "MXP" && claveMoneda !== "MXV") {
                    document.getElementById("pagoEsMonedaExtranjera").checked = true;
                } else {
                    document.getElementById("pagoEsMonedaExtranjera").checked = false;
                }

                const esLiquidacion = document.getElementById("pagoEsLiquidacion").checked;
                const porcentajePlazo = numeroRaw(document.getElementById("pagoPorcentajePlazo").value);

                document.getElementById("pagoEsLiquidacionAnticipada").checked = esLiquidacion && porcentajePlazo < 100;
            }

            function validarPago() {
                if (numeroRaw(document.getElementById("pagoSolicitudId").value) <= 0) {
                    mostrarError("Busca y selecciona un crédito válido.");
                    return false;
                }

                if (numeroRaw(document.getElementById("pagoTipoPagoId").value) <= 0) {
                    mostrarError("Selecciona el tipo de pago.");
                    return false;
                }

                if (numeroRaw(document.getElementById("pagoCanalPagoId").value) <= 0) {
                    mostrarError("Selecciona el canal de pago.");
                    return false;
                }

                if (numeroRaw(document.getElementById("pagoMonedaId").value) <= 0) {
                    mostrarError("Selecciona la moneda.");
                    return false;
                }

                if (numeroRaw(document.getElementById("pagoMonto").value) <= 0) {
                    mostrarError("El monto del pago debe ser mayor a cero.");
                    return false;
                }

                if (numeroRaw(document.getElementById("pagoTipoCambio").value) <= 0) {
                    mostrarError("El tipo de cambio debe ser mayor a cero.");
                    return false;
                }

                return true;
            }

            function guardarPago() {
                if (!validarPago()) {
                    return;
                }

                const formData = new FormData();
                formData.append("action", "guardar_aplicar");
                formData.append("solicitud_credito_id", document.getElementById("pagoSolicitudId").value);
                formData.append("cliente_id", document.getElementById("pagoClienteId").value);
                formData.append("fecha_pago", document.getElementById("pagoFecha").value);
                formData.append("referencia_pago", document.getElementById("pagoReferencia").value);
                formData.append("tipo_pago_id", document.getElementById("pagoTipoPagoId").value);
                formData.append("canal_pago_id", document.getElementById("pagoCanalPagoId").value);
                formData.append("moneda_id", document.getElementById("pagoMonedaId").value);
                formData.append("aplicacion_pago_id", document.getElementById("pagoAplicacionPagoId").value);
                formData.append("monto_pago", document.getElementById("pagoMonto").value);
                formData.append("tipo_cambio", document.getElementById("pagoTipoCambio").value);
                formData.append("monto_equivalente_mxn", document.getElementById("pagoEquivalenteMxn").value);
                formData.append("monto_equivalente_usd", document.getElementById("pagoEquivalenteUsd").value);
                formData.append("monto_capital", document.getElementById("pagoMontoCapital").value);
                formData.append("monto_interes", document.getElementById("pagoMontoInteres").value);
                formData.append("monto_iva", document.getElementById("pagoMontoIva").value);
                formData.append("monto_moratorio", document.getElementById("pagoMontoMoratorio").value);
                formData.append("monto_comisiones", document.getElementById("pagoMontoComisiones").value);
                formData.append("monto_otros", document.getElementById("pagoMontoOtros").value);
                formData.append("monto_credito_original", document.getElementById("pagoMontoCreditoOriginal").value);
                formData.append("saldo_antes_pago", document.getElementById("pagoSaldoAntes").value);
                formData.append("saldo_despues_pago", document.getElementById("pagoSaldoDespues").value);
                formData.append("pago_fijo_contractual", document.getElementById("pagoFijoContractual").value);
                formData.append("numero_pago_en_credito", document.getElementById("pagoNumeroPago").value);
                formData.append("plazo_total_credito", document.getElementById("pagoPlazoTotal").value);
                formData.append("porcentaje_plazo_transcurrido", document.getElementById("pagoPorcentajePlazo").value);
                formData.append("porcentaje_pagado_credito", document.getElementById("pagoPorcentajePagado").value);
                formData.append("es_efectivo", document.getElementById("pagoEsEfectivo").checked ? "1" : "0");
                formData.append("es_moneda_extranjera", document.getElementById("pagoEsMonedaExtranjera").checked ? "1" : "0");
                formData.append("es_pago_excedente", document.getElementById("pagoEsExcedente").checked ? "1" : "0");
                formData.append("es_liquidacion", document.getElementById("pagoEsLiquidacion").checked ? "1" : "0");
                formData.append("es_liquidacion_anticipada", document.getElementById("pagoEsLiquidacionAnticipada").checked ? "1" : "0");
                formData.append("requiere_devolucion", document.getElementById("pagoRequiereDevolucion").checked ? "1" : "0");
                formData.append("motivo_devolucion", document.getElementById("pagoMotivoDevolucion").value);
                formData.append("observaciones", document.getElementById("pagoObservaciones").value);

                fetchJson(HANDLER, {
                    method: "POST",
                    body: formData
                })
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        let mensaje = resp.mensaje || "Pago aplicado correctamente.";

                        if (resp.alertas_ok) {
                            mensaje += "<br><br><strong>Alertas PLD:</strong> " + valorSeguro(resp.alertas_mensaje);
                        } else {
                            mensaje += "<br><br><strong>Atención:</strong> el pago se guardó, pero el motor PLD no confirmó ejecución.";
                        }

                        Swal.fire({
                            title: "Pago aplicado",
                            html: mensaje,
                            icon: resp.alertas_ok ? "success" : "warning"
                        });

                        modalPago.hide();
                        cargarPagos();
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function obtenerPago(id) {
                if (!id || parseInt(id, 10) <= 0) {
                    mostrarError("ID de pago inválido.");
                    return;
                }

                fetchJson(construirUrl("obtener", { id: id }))
                    .then(function (resp) {
                        if (!resp.ok) {
                            mostrarError(resp.mensaje);
                            return;
                        }

                        mostrarDetallePago(resp.data);
                    })
                    .catch(function (error) {
                        mostrarError(error.message);
                    });
            }

            function mostrarDetallePago(item) {
                const html =
                    '<div class="row g-3">' +
                        crearCampoDetalle("Folio", item.folio_pago) +
                        crearCampoDetalle("Solicitud", item.solicitud_credito_id) +
                        crearCampoDetalle("Cliente", item.cliente_id) +
                        crearCampoDetalle("Fecha pago", fechaCorta(item.fecha_pago)) +
                        crearCampoDetalle("Tipo pago", item.tipo_pago) +
                        crearCampoDetalle("Canal", item.canal_pago) +
                        crearCampoDetalle("Moneda", item.moneda) +
                        crearCampoDetalle("Monto pago", "$" + numero(item.monto_pago, 2)) +
                        crearCampoDetalle("Equivalente MXN", "$" + numero(item.monto_equivalente_mxn, 2)) +
                        crearCampoDetalle("Equivalente USD", "$" + numero(item.monto_equivalente_usd, 2)) +
                        crearCampoDetalle("Saldo antes", "$" + numero(item.saldo_antes_pago, 2)) +
                        crearCampoDetalle("Saldo después", "$" + numero(item.saldo_despues_pago, 2)) +
                        crearCampoDetalle("Efectivo", item.es_efectivo ? "Sí" : "No") +
                        crearCampoDetalle("Moneda extranjera", item.es_moneda_extranjera ? "Sí" : "No") +
                        crearCampoDetalle("Liquidación", item.es_liquidacion ? "Sí" : "No") +
                        crearCampoDetalle("Liquidación anticipada", item.es_liquidacion_anticipada ? "Sí" : "No") +
                        crearCampoDetalle("Requiere devolución", item.requiere_devolucion ? "Sí" : "No") +
                        crearCampoDetalle("Estatus", item.estatus_pago) +
                        '<div class="col-md-12">' +
                            '<label class="form-label">Observaciones</label>' +
                            '<textarea class="form-control" rows="3" readonly>' + valorSeguro(item.observaciones) + '</textarea>' +
                        '</div>' +
                    '</div>';

                document.getElementById("detallePagoContenido").innerHTML = html;
                modalDetallePago.show();
            }

            function crearCampoDetalle(label, valor) {
                return '' +
                    '<div class="col-md-3">' +
                        '<label class="form-label">' + label + '</label>' +
                        '<input type="text" class="form-control" readonly value="' + escaparHtml(valorSeguro(valor)) + '" />' +
                    '</div>';
            }

            function escaparHtml(texto) {
                return texto
                    .replace(/&/g, "&amp;")
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                    .replace(/"/g, "&quot;")
                    .replace(/'/g, "&#039;");
            }

            function cancelarPago(id) {
                if (!id || parseInt(id, 10) <= 0) {
                    mostrarError("ID de pago inválido.");
                    return;
                }

                Swal.fire({
                    title: "Cancelar pago",
                    text: "Esta acción marcará el pago como cancelado. ¿Deseas continuar?",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Sí, cancelar",
                    cancelButtonText: "No"
                }).then(function (result) {
                    if (!result.isConfirmed) {
                        return;
                    }

                    const formData = new FormData();
                    formData.append("action", "cancelar");
                    formData.append("id", id);
                    formData.append("motivo", "Cancelado desde módulo de pagos.");

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
                            cargarPagos();
                        })
                        .catch(function (error) {
                            mostrarError(error.message);
                        });
                });
            }

        });
    </script>

</asp:Content>
