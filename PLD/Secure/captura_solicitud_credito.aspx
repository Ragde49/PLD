<%@ Page Title="Captura de Solicitud de Crédito" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="captura_solicitud_credito.aspx.vb" Inherits="PLD.captura_solicitud_credito" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid mt-4">
    <h4 class="mb-3">Captura de Solicitud de Crédito</h4>

    <!-- Encabezado / Estado -->
    <div class="card mb-3">
        <div class="card-body d-flex align-items-center gap-3 flex-wrap">
            <div><span class="text-muted">Solicitud ID:</span> <span id="lblSolicitudId" class="fw-bold">—</span></div>
            <div><span class="text-muted">Estatus:</span> <span id="lblEstatus" class="badge bg-secondary">—</span></div>
            <div class="ms-auto d-flex gap-2">
                <a id="btnIrRevolvente" href="#" class="btn btn-outline-primary btn-sm d-none">Administrar revolvente</a>
                <button type="button" id="btnNuevo" class="btn btn-outline-secondary btn-sm">Nuevo</button>
                <button type="button" id="btnFinalizar" class="btn btn-success btn-sm">Finalizar</button>
            </div>
        </div>
    </div>

    <!-- Tabs -->
    <ul class="nav nav-tabs" id="tabsSolicitud" role="tablist">
        <li class="nav-item" role="presentation">
            <button type="button" class="nav-link active" id="tab-operacion" data-bs-toggle="tab" data-bs-target="#panel-operacion" role="tab">Solicitud de Operación</button>
        </li>
        <li class="nav-item" role="presentation">
            <button type="button" class="nav-link" id="tab-identidad" data-bs-toggle="tab" data-bs-target="#panel-identidad" role="tab">Identidad del Cliente</button>
        </li>
        <li class="nav-item" role="presentation">
            <button type="button" class="nav-link" id="tab-contacto" data-bs-toggle="tab" data-bs-target="#panel-contacto" role="tab">Datos de Contacto</button>
        </li>
        <li class="nav-item" role="presentation">
            <button type="button" class="nav-link" id="tab-pld" data-bs-toggle="tab" data-bs-target="#panel-pld" role="tab">PLD</button>
        </li>
        <li class="nav-item" role="presentation">
            <button type="button" class="nav-link" id="tab-amortizacion" data-bs-toggle="tab" data-bs-target="#panel-amortizacion" role="tab">Amortización CONDUSEF</button>
        </li>
    </ul>

    <div class="tab-content border-start border-end border-bottom p-3">
        <!-- ================= TAB: OPERACIÓN ================= -->
        <div class="tab-pane fade show active" id="panel-operacion" role="tabpanel" aria-labelledby="tab-operacion">
            <form class="row g-3" id="frmOperacion">
                <div class="col-md-4">
                    <label class="form-label">Producto financiero</label>
                    <select class="form-select" id="producto_financiero_id"></select>
                    <div id="productoTipoCreditoAyuda" class="form-text"></div>
                </div>
                <div class="col-md-4">
                    <label class="form-label">Periodicidad</label>
                    <select class="form-select" id="producto_financiero_periodo_id" disabled></select>
                    <div class="form-text">Selecciona Mensual/Quincenal/etc. para fijar plazo y tasa.</div>
                </div>
                <div class="col-md-4">
                    <label class="form-label">Canal de pago</label>
                    <select class="form-select" id="canal_pago_id"></select>
                </div>
                <div class="col-md-4">
                    <label class="form-label">Destino de recursos</label>
                    <select class="form-select" id="destino_recursos_id"></select>
                </div>
                <div class="col-md-4">
                    <label class="form-label">Origen de recursos</label>
                    <select class="form-select" id="origen_recursos_id"></select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Moneda</label>
                    <select class="form-select" id="moneda_id"></select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Monto solicitado</label>
                    <input type="number" step="0.01" min="0" class="form-control" id="monto_solicitado" placeholder="0.00">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Plazo (meses)</label>
                    <input type="number" min="0" class="form-control bg-light" id="plazo" placeholder="12" readonly>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Tasa entrada (%)</label>
                    <input type="number" step="0.000001" min="0" class="form-control bg-light" id="tasa_entrada" placeholder="24.5" readonly>
                </div>

                <div class="col-md-3 form-check mt-4">
                    <input type="checkbox" class="form-check-input" id="permitir_pagos_anticipados">
                    <label class="form-check-label" for="permitir_pagos_anticipados">Permitir pagos anticipados</label>
                </div>

                <div class="col-12">
                    <label class="form-label">Observaciones</label>
                    <textarea class="form-control" id="observaciones" rows="2" maxlength="1000"></textarea>
                </div>

                <div class="col-12 d-flex gap-2">
                    <button type="button" id="btnGuardarOperacion" class="btn btn-primary">Guardar operación</button>
                    <button type="button" id="btnAplicarPLD" class="btn btn-warning">Aplicar PLD</button>
                </div>
            </form>
        </div>

        <!-- ================= TAB: IDENTIDAD DEL CLIENTE ================= -->
        <div class="tab-pane fade" id="panel-identidad" role="tabpanel" aria-labelledby="tab-identidad">
            <div class="card mb-3">
                <div class="card-header d-flex align-items-center justify-content-between">
                    <span>Datos de Identidad — Persona Física</span>
                    <div class="d-flex gap-2">
                        <button type="button" id="btnBuscarPorRFC" class="btn btn-outline-secondary btn-sm">Cargar por RFC</button>
                        <button type="button" id="btnBuscarPorCURP" class="btn btn-outline-secondary btn-sm">Cargar por CURP</button>
                        <button type="button" id="btnGuardarCliente" class="btn btn-primary btn-sm">Guardar identidad</button>
                    </div>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <!-- Identificadores -->
                        <div class="col-md-3">
                            <label class="form-label">RFC <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="cli_rfc" maxlength="13" placeholder="GODE561231GR8" required>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">CURP</label>
                            <input type="text" class="form-control" id="cli_curp" maxlength="18" placeholder="GODE561231HDFRRN09">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Cliente ID</label>
                            <input type="text" class="form-control" id="cli_id" readonly>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Tipo persona</label>
                            <select class="form-select" id="cli_persona_tipo" disabled>
                                <option value="PF" selected>Persona Física</option>
                            </select>
                        </div>

                        <!-- Nombre -->
                        <div class="col-md-3">
                            <label class="form-label">Primer nombre <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="cli_primer_nombre" maxlength="100" required>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Segundo nombre</label>
                            <input type="text" class="form-control" id="cli_segundo_nombre" maxlength="100">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Apellido paterno <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="cli_ap_paterno" maxlength="100" required>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Apellido materno</label>
                            <input type="text" class="form-control" id="cli_ap_materno" maxlength="100">
                        </div>

                        <!-- Datos personales -->
                        <div class="col-md-3">
                            <label class="form-label">Fecha nacimiento <span class="text-danger">*</span></label>
                            <input type="date" class="form-control" id="cli_fecha_nac" required>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Sexo <span class="text-danger">*</span></label>
                            <select class="form-select" id="cli_sexo" required>
                                <option value="">— Seleccionar —</option>
                                <option value="M">Masculino</option>
                                <option value="F">Femenino</option>
                            </select>
                        </div>

                        <!-- LIGADOS A CATÁLOGOS -->
                        <div class="col-md-3">
                            <label class="form-label">Nacionalidad</label>
                            <select class="form-select" id="cli_nacionalidad"></select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">País de nacimiento</label>
                            <select class="form-select" id="cli_pais_nacimiento"></select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Entidad de nacimiento</label>
                            <select class="form-select" id="cli_entidad_nacimiento"></select>
                        </div>

                        <!-- Estado civil / dependientes / escolaridad -->
                        <div class="col-md-3">
                            <label class="form-label">Estado civil <span class="text-danger">*</span></label>
                            <select id="cli_estado_civil" class="form-select" required>
                                <option value="">—</option>
                                <option value="SOLTERO">Soltero(a)</option>
                                <option value="CASADO">Casado(a)</option>
                                <option value="UNION_LIBRE">Unión libre</option>
                                <option value="DIVORCIADO">Divorciado(a)</option>
                                <option value="VIUDO">Viudo(a)</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Régimen matrimonial</label>
                            <select id="cli_regimen_matrimonial" class="form-select">
                                <option value="">—</option>
                                <option value="SOCIEDAD_CONYUGAL">Sociedad conyugal</option>
                                <option value="SEPARACION_BIENES">Separación de bienes</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Dependientes</label>
                            <input type="number" id="cli_dependientes" class="form-control" min="0" step="1" value="0">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Escolaridad</label>
                            <select id="cli_escolaridad" class="form-select">
                                <option value="">—</option>
                                <option value="PRIMARIA">Primaria</option>
                                <option value="SECUNDARIA">Secundaria</option>
                                <option value="BACHILLERATO">Bachillerato</option>
                                <option value="LICENCIATURA">Licenciatura</option>
                                <option value="POSGRADO">Posgrado</option>
                            </select>
                        </div>

                        <!-- Actividad e ingresos -->
                        <div class="col-md-4">
                            <label class="form-label">Actividad económica</label>
                            <select class="form-select" id="cli_actividad_economica"></select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Puesto</label>
                            <input type="text" id="cli_puesto" class="form-control" maxlength="120">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Empresa</label>
                            <input type="text" id="cli_empresa" class="form-control" maxlength="150">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Antigüedad laboral (años)</label>
                            <input type="number" id="cli_antiguedad_anios" class="form-control" min="0" step="1" value="0">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Ingreso mensual (MXN)</label>
                            <input type="number" step="0.01" min="0" class="form-control" id="cli_ingreso_mensual" placeholder="0.00">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Otros ingresos (MXN)</label>
                            <input type="number" step="0.01" min="0" class="form-control" id="cli_otros_ingresos" placeholder="0.00">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Origen otros ingresos</label>
                            <input type="text" id="cli_origen_otros_ingresos" class="form-control" maxlength="200">
                        </div>

                        <div class="col-12 mt-2">
                            <div class="border rounded p-3 bg-light">
                                <div class="fw-semibold mb-2">Perfil transaccional esperado</div>
                                <div class="row g-3">
                                    <div class="col-md-4">
                                        <label class="form-label">Pagos esperados por mes</label>
                                        <input type="number" id="cli_perfil_pagos_mensuales" class="form-control" min="0" step="1" placeholder="0">
                                    </div>
                                    <div class="col-md-4">
                                        <label class="form-label">Monto mensual esperado</label>
                                        <input type="number" id="cli_perfil_monto_mensual" class="form-control" min="0" step="0.01" placeholder="0.00">
                                    </div>
                                    <div class="col-md-4 d-flex align-items-end">
                                        <div class="form-text mb-2">Estos valores representan el comportamiento declarado/esperado del cliente y se comparan contra pagos reales para PLD.</div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- PEP y consentimiento -->
                        <div class="col-md-3 form-check mt-4">
                            <input type="checkbox" class="form-check-input" id="cli_pep">
                            <label class="form-check-label" for="cli_pep">Persona Políticamente Expuesta (PEP)</label>
                        </div>
                        <div class="col-md-3 form-check mt-4">
                            <input type="checkbox" class="form-check-input" id="cli_acepta_avisos">
                            <label class="form-check-label" for="cli_acepta_avisos">Acepta avisos / privacidad</label>
                        </div>

                        <!-- Identificación oficial -->
                        <div class="col-md-3">
                            <label class="form-label">Identificación</label>
                            <select id="cli_ident_tipo" class="form-select">
                                <option value="">—</option>
                                <option value="INE">INE</option>
                                <option value="PASAPORTE">Pasaporte</option>
                                <option value="CEDULA">Cédula</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">No. Identificación</label>
                            <input type="text" id="cli_ident_numero" class="form-control" maxlength="30">
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Vigencia identificación</label>
                            <input type="date" id="cli_ident_vigencia" class="form-control">
                        </div>

                        <div class="col-md-6 d-none">
                            <label class="form-label">Razón social</label>
                            <input type="text" class="form-control" id="cli_razon_social" maxlength="200">
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">Estatus</label>
                            <input type="text" id="cli_estatus" class="form-control" value="ACTIVO" readonly>
                        </div>
                        <div class="col-12">
                            <small class="text-muted">Nota: estos datos alimentan el KYC y pueden cruzarse con listas PEP/sanciones.</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ================= TAB: CONTACTO ================= -->
        <div class="tab-pane fade" id="panel-contacto" role="tabpanel" aria-labelledby="tab-contacto">
            <div class="card mb-3">
                <div class="card-header">Contacto de la Solicitud</div>
                <div class="card-body row g-3 align-items-end">
                    <div class="col-md-4">
                        <label class="form-label">Medio de contacto (catálogo)</label>
                        <select id="medio_contacto_id" class="form-select"></select>
                    </div>
                    <div class="col-md-3 form-check mt-4">
                        <input type="checkbox" class="form-check-input" id="consentimiento_comunicacion">
                        <label class="form-check-label" for="consentimiento_comunicacion">Consentimiento de comunicación</label>
                    </div>
                    <div class="col-md-5 d-flex gap-2 justify-content-end">
                        <button type="button" id="btnCrearContacto" class="btn btn-outline-primary">Crear/Obtener contacto</button>
                        <button type="button" id="btnActualizarContacto" class="btn btn-primary">Actualizar contacto</button>
                    </div>
                    <div class="col-12">
                        <small class="text-muted">contacto_id: <span id="lblContactoId">—</span></small>
                    </div>
                </div>
            </div>

            <!-- Teléfonos -->
            <div class="card mb-3">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Teléfonos</span>
                    <div class="d-flex gap-2">
                        <select id="tel_tipo" class="form-select form-select-sm" style="width:auto;">
                            <option value="MOVIL">MÓVIL</option>
                            <option value="CASA">CASA</option>
                            <option value="TRABAJO">TRABAJO</option>
                        </select>
                        <input type="text" id="tel_numero" class="form-control form-control-sm" placeholder="8112345678" style="width:160px;">
                        <input type="text" id="tel_ext" class="form-control form-control-sm" placeholder="Ext" style="width:90px;">
                        <div class="form-check form-check-inline">
                            <input type="checkbox" id="tel_principal" class="form-check-input">
                            <label class="form-check-label" for="tel_principal">Principal</label>
                        </div>
                        <button type="button" id="btnAgregarTel" class="btn btn-sm btn-outline-primary">Agregar</button>
                    </div>
                </div>
                <div class="card-body p-0">
                    <table class="table table-sm table-striped mb-0 w-100" id="tblTelefonos">
                        <thead class="table-light">
                            <tr><th>Tipo</th><th>Número</th><th>Ext</th><th>Principal</th><th>Activo</th><th>Acciones</th></tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>

            <!-- Emails -->
            <div class="card mb-3">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Emails</span>
                    <div class="d-flex gap-2">
                        <input type="email" id="email_val" class="form-control form-control-sm" placeholder="cliente@correo.com" style="width:220px;">
                        <div class="form-check form-check-inline">
                            <input type="checkbox" id="email_principal" class="form-check-input">
                            <label class="form-check-label" for="email_principal">Principal</label>
                        </div>
                        <button type="button" id="btnAgregarEmail" class="btn btn-sm btn-outline-primary">Agregar</button>
                    </div>
                </div>
                <div class="card-body p-0">
                    <table class="table table-sm table-striped mb-0 w-100" id="tblEmails">
                        <thead class="table-light">
                            <tr><th>Email</th><th>Principal</th><th>Activo</th><th>Acciones</th></tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>

            <!-- Domicilio -->
            <div class="card mb-3">
                <div class="card-header">Domicilio</div>
                <div class="card-body">
                    <div class="row g-2 align-items-end">
                        <div class="col-md-4">
                            <label class="form-label">Calle</label>
                            <input type="text" id="dom_calle" class="form-control">
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">No. Ext</label>
                            <input type="text" id="dom_ext" class="form-control">
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">No. Int</label>
                            <input type="text" id="dom_int" class="form-control">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Colonia</label>
                            <input type="text" id="dom_colonia" class="form-control">
                        </div>

                        <div class="col-md-3">
                            <label class="form-label">País</label>
                            <select id="dom_pais" class="form-select"></select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Estado</label>
                            <select id="dom_estado" class="form-select"></select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Municipio</label>
                            <select id="dom_municipio" class="form-select"></select>
                        </div>

                        <div class="col-md-2">
                            <label class="form-label">CP</label>
                            <input type="text" id="dom_cp" class="form-control">
                        </div>
                        <div class="col-md-3 form-check mt-4">
                            <input type="checkbox" id="dom_principal" class="form-check-input" checked>
                            <label class="form-check-label" for="dom_principal">Principal</label>
                        </div>
                        <div class="col-md-4 d-flex justify-content-end">
                            <button type="button" id="btnGuardarDom" class="btn btn-outline-primary">Guardar domicilio</button>
                        </div>
                    </div>
                    <hr>
                    <table class="table table-sm table-striped mb-0 w-100" id="tblDomicilios">
                        <thead class="table-light">
                            <tr><th>Calle</th><th>Colonia</th><th>Municipio</th><th>Estado</th><th>CP</th><th>Principal</th><th>Activo</th><th>Acciones</th></tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- ================= TAB: PLD ================= -->
        <div class="tab-pane fade" id="panel-pld" role="tabpanel" aria-labelledby="tab-pld">
            <div class="row g-3">
                <div class="col-md-3">
                    <div class="card text-center">
                        <div class="card-header">Puntaje Total</div>
                        <div class="card-body">
                            <div id="lblPuntaje" class="display-6">0.0000</div>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="card text-center">
                        <div class="card-header">Nivel (PLD)</div>
                        <div class="card-body">
                            <div id="lblNivelPLD" class="display-6">0.00</div>
                            <div id="lblMetodoPLD" style="font-size:12px; color:#666; margin-top:4px;"></div>
                            <div id="lblFechaPLD"style="font-size:12px; color:#666; margin-top:4px;"></div>
                        </div>
                    </div>
                </div>

                <!-- ====== NUEVO: Tipo de riesgo (PF) ====== -->
                <div class="col-md-3">
                    <div class="card text-center">
                        <div class="card-header">Tipo de riesgo</div>
                        <div class="card-body">
                            <div id="lblTipoRiesgo" class="display-6">—</div>
                        </div>
                    </div>
                </div>

                <div class="col-md-6 d-flex align-items-end justify-content-end">
                    <button type="button" id="btnRecalcularPLD" class="btn btn-warning me-2">Recalcular PLD</button>
                    <button type="button" id="btnRefrescarPLD" class="btn btn-outline-secondary">Refrescar</button>
                </div>
                <div class="col-12">
                    <table class="table table-sm table-striped w-100" id="tblPLDDetalle">
                        <thead class="table-light">
                            <tr>
                                <th>Origen</th><th>Referencia</th><th>Impacto</th><th>Prob.</th><th>Nivel</th><th>Contribución</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                    <div class="mt-2 text-end">
                        <small class="text-muted">Factores: <span id="lblFactores">0</span> | Suma nivel: <span id="lblSumaNivel">0.00</span> | Promedio: <span id="lblPromNivel">0.0000</span></small>
                    </div>
                </div>
            </div>
        </div>

        <!-- ================= TAB: AMORTIZACIÓN CONDUSEF ================= -->
        <div class="tab-pane fade" id="panel-amortizacion" role="tabpanel" aria-labelledby="tab-amortizacion">
            <div class="row g-3">
                <div class="col-12">
                    <div class="d-flex flex-wrap justify-content-between align-items-center gap-2">
                        <h5 class="mb-0">Tabla de amortización (CONDUSEF)</h5>
                        <button type="button" id="btnDescargarAmortizacion" class="btn btn-outline-success btn-sm">Descargar PDF</button>
                    </div>
                    <small class="text-muted">Se genera con base en la solicitud y periodicidad seleccionada.</small>
                </div>

                <div class="col-12">
                    <div class="card">
                        <div class="card-header">Cargos iniciales (Día 0)</div>
                        <div class="card-body">
                            <div class="row g-2">
                                <div class="col-md-4">
                                    <label class="form-label mb-1">Comisión por apertura</label>
                                    <div id="lblComisionAperturaDia0" class="fw-semibold">$0.00</div>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label mb-1">IVA comisión por apertura</label>
                                    <div id="lblIvaComisionAperturaDia0" class="fw-semibold">$0.00</div>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label mb-1">Total cargos iniciales</label>
                                    <div id="lblTotalCargosDia0" class="fw-bold">$0.00</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-12">
                    <div class="table-responsive">
                        <table class="table table-sm table-striped w-100" id="tblAmortizacionCondusef">
                            <thead class="table-light">
                                <tr>
                                    <th>Número de Pagos</th>
                                    <th>Fecha</th>
                                    <th>Saldo inicial</th>
                                    <th>Capital + Interés</th>
                                    <th>Capital</th>
                                    <th>Interés</th>
                                    <th>IVA de los Intereses</th>
                                    <th id="thPagoPeriodo">Pago Mensual</th>
                                    <th>Saldo pendiente de pago</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<script src="<%= ResolveUrl("~/assets/js/jspdf.umd.min.js") %>"></script>
<script src="<%= ResolveUrl("~/assets/js/jspdf.plugin.autotable.min.js") %>"></script>
<script>
    // =================== Config ===================
    const H_SOL = '/handlers/solicitud_credito_handler.ashx';
    const H_CAT = '/handlers/catalogos_handler.ashx';
    const H_CNT = '/handlers/contacto_solicitud_handler.ashx';
    const H_PLD = '/handlers/pld_detalle_handler.ashx';
    const H_CLI = '/handlers/clientes_handler.ashx';
    const PDF_LOGO_URL = '<%= ResolveUrl("~/assets/images/logo-dark.png") %>';
    const PDF_THEME = {
        brand: [17, 163, 76],
        brandDark: [6, 53, 25],
        brandSoft: [233, 248, 237],
        accent: [201, 235, 208],
        text: [12, 18, 14],
        muted: [79, 94, 84],
        border: [186, 216, 192],
        highlight: [244, 251, 246]
    };
    const H_ALERTAS = '/handlers/handler_alertas_pld.ashx';

    // =================== Utils ===================
    function num(val, def = 0) { const n = parseFloat((val ?? '').toString()); return isNaN(n) ? def : n; }
    function int(val, def = 0) { const n = parseInt((val ?? '').toString(), 10); return isNaN(n) ? def : n; }
    function money(val) {
        const n = Number(val ?? 0);
        return n.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
    }
    function moneyPlain(val) {
        const n = Number(val ?? 0);
        return n.toLocaleString('es-MX', { style: 'currency', currency: 'MXN', minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
    function fmt(dtTicks) {
        if (!dtTicks) return '—';
        try {
            if (typeof dtTicks === 'string' && dtTicks.startsWith('/Date(')) {
                const ms = parseInt(dtTicks.substring(6), 10);
                return new Date(ms).toLocaleString();
            }
            const d = new Date(dtTicks);
            return isNaN(d.getTime()) ? '—' : d.toLocaleString();
        } catch (e) { return '—'; }
    }
    function parseServerDate(value) {
        if (!value) return null;
        if (value instanceof Date) return isNaN(value.getTime()) ? null : value;
        if (typeof value === 'string' && value.startsWith('/Date(')) {
            const ms = parseInt(value.replace(/[^\d-]/g, ''), 10);
            if (!isNaN(ms)) {
                const dDotNet = new Date(ms);
                return isNaN(dDotNet.getTime()) ? null : dDotNet;
            }
        }
        const d = new Date(value);
        return isNaN(d.getTime()) ? null : d;
    }
    function fmtDate(value) {
        const d = parseServerDate(value);
        return d ? d.toLocaleDateString('es-MX') : '';
    }
    function fmtDateLong(value) {
        const d = parseServerDate(value);
        return d ? d.toLocaleDateString('es-MX', { day: '2-digit', month: 'long', year: 'numeric' }) : '—';
    }
    function cleanText(value, fallback = '—') {
        const txt = (value ?? '').toString().replace(/\s+/g, ' ').trim();
        return txt || fallback;
    }
    function qs(name) {
        const url = new URL(window.location.href);
        return url.searchParams.get(name);
    }
    function setSelect(id, val) {
        const el = document.getElementById(id);
        if (!el) return;
        const v = (val == null ? '' : val.toString());
        if (v === '') { el.value = ''; return; }
        const opt = Array.from(el.options).find(o => o.value == v);
        if (opt) el.value = v;
    }
    function setSelectByValueOrText(id, value, text) {
        const el = document.getElementById(id);
        if (!el) return;
        if (value != null && value !== '') {
            const opt = Array.from(el.options).find(o => o.value == value);
            if (opt) { el.value = value; return; }
        }
        if (text) {
            const opt2 = Array.from(el.options).find(o => o.text === text);
            if (opt2) { el.value = opt2.value; }
        }
    }
    function setEstatus(e) {
        const lbl = document.getElementById('lblEstatus');
        lbl.textContent = e || '—';
        lbl.className = 'badge ' + (e === 'BORRADOR' ? 'bg-secondary' : (e === 'FINALIZADA' ? 'bg-success' : 'bg-info'));
    }
    function normalizePeriodo(tipoPeriodo) {
        const txt = (tipoPeriodo || '').toString().trim().toUpperCase();
        if (txt.includes('SEMAN')) return 'SEMANAL';
        if (txt.includes('QUINC')) return 'QUINCENAL';
        return 'MENSUAL';
    }
    function resolveFechaAmortizacion(row) {
        const raw = cleanText(row?.fecha, '');
        if (raw) return raw;
        const numeroPago = int(row?.numero_pago, 0);
        if (numeroPago <= 0) return '';
        const baseDate = parseServerDate(SOL_DATA?.fecha_creacion);
        if (!baseDate) return '';

        const fecha = new Date(baseDate.getTime());
        switch (normalizePeriodo(AMORT_DATA?.tipo_periodo)) {
            case 'SEMANAL':
                fecha.setDate(fecha.getDate() + (7 * numeroPago));
                break;
            case 'QUINCENAL':
                fecha.setDate(fecha.getDate() + (15 * numeroPago));
                break;
            default:
                fecha.setMonth(fecha.getMonth() + numeroPago);
                break;
        }
        return fecha.toLocaleDateString('es-MX');
    }

    // ===== Helpers PLD/DOM: robustos para no “perder” valores =====
    function selectedText(id) {
        const el = document.getElementById(id);
        if (!el) return '';
        return (el.options[el.selectedIndex]?.text || '').toString().trim();
    }
    function isMexico(selectId) {
        const txt = selectedText(selectId);
        if (!txt) return false;
        const norm = txt.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toUpperCase();
        return norm === "MEXICO";
    }
    function setDisabled(id, disabled) {
        const el = document.getElementById(id);
        if (!el) return;
        el.disabled = !!disabled;
    }
    function setOptionsPlaceholder(id, text) {
        const el = document.getElementById(id);
        if (!el) return;
        el.innerHTML = `<option value="">${text}</option>`;
    }





    // =================== Periodicidades de producto ===================
    let PERIODOS_CACHE = [];

    function resetPeriodosUI() {
        PERIODOS_CACHE = [];
        const el = document.getElementById('producto_financiero_periodo_id');
        if (el) {
            el.innerHTML = '<option value="">— Selecciona producto —</option>';
            el.value = '';
            el.disabled = true;
        }
        const plazoEl = document.getElementById('plazo');
        const tasaEl = document.getElementById('tasa_entrada');
        if (plazoEl) plazoEl.value = '';
        if (tasaEl) tasaEl.value = '';
    }

    async function cargarPeriodosProducto(productoFinancieroId, preselectPeriodoId) {
        resetPeriodosUI();
        const pf = int(productoFinancieroId, 0);
        if (pf <= 0) return;

        const elPeriodo = document.getElementById('producto_financiero_periodo_id');
        if (!elPeriodo) return;

        elPeriodo.disabled = true;
        elPeriodo.innerHTML = '<option value="">Cargando...</option>';

        const url = `${H_SOL}?action=periodos_producto&producto_financiero_id=${encodeURIComponent(pf)}`;
        const r = await fetch(url, { credentials: 'same-origin' });
        const j = await r.json();

        if (!j || !j.ok) {
            elPeriodo.innerHTML = '<option value="">— Sin periodos —</option>';
            elPeriodo.disabled = true;
            return swal.fire('Producto', j?.message || 'No se pudieron cargar las periodicidades del producto.', 'warning');
        }

        // Autollenar moneda desde el maestro (si viene)
        if (j.moneda_id) {
            setSelect('moneda_id', j.moneda_id);
        }

        const periodos = j.data || [];
        PERIODOS_CACHE = periodos;

        elPeriodo.innerHTML = '<option value="">— Seleccionar periodicidad —</option>';
        periodos.forEach(p => {
            const pid = int(p.id, 0);
            const tipo = (p.tipo_periodo || '').toString();
            const plazo = int(p.plazo, 0);

            // tasa_interes viene como fracción (0.21 = 21%)
            const tasaFrac = num(p.tasa_interes ?? p.tasa_intereses ?? 0, 0);
            const tasaPct = tasaFrac * 100;

            const opt = document.createElement('option');
            opt.value = pid;
            opt.textContent = tipo || (`Periodo ${pid}`);
            opt.dataset.plazo = plazo.toString();
            opt.dataset.tasaPct = isNaN(tasaPct) ? '0' : tasaPct.toFixed(6);
            elPeriodo.appendChild(opt);
        });

        elPeriodo.disabled = false;

        if (int(preselectPeriodoId, 0) > 0) {
            setSelect('producto_financiero_periodo_id', preselectPeriodoId);
            onPeriodoProductoChange();
        }
    }

    function productoSeleccionadoEsRevolvente() {
        const el = document.getElementById('producto_financiero_id');
        if (!el || !el.value || el.selectedIndex < 0) return false;
        const opt = el.options[el.selectedIndex];
        return opt?.dataset?.esRevolvente === '1';
    }

    function actualizarUIRevolvente() {
        const esRev = productoSeleccionadoEsRevolvente() || !!SOL_DATA?.es_revolvente;
        const estatus = (SOL_DATA?.estatus || document.getElementById('lblEstatus')?.textContent || '').toString().trim().toUpperCase();
        const ayuda = document.getElementById('productoTipoCreditoAyuda');
        const tabAmort = document.getElementById('tab-amortizacion');
        const btnRev = document.getElementById('btnIrRevolvente');

        if (ayuda) {
            ayuda.textContent = esRev
                ? 'Producto revolvente: el monto capturado representa la línea solicitada. No aplica tabla fija de amortización.'
                : '';
            ayuda.className = esRev ? 'form-text text-primary fw-semibold' : 'form-text';
        }

        if (tabAmort) {
            tabAmort.classList.toggle('d-none', esRev);
        }

        if (btnRev) {
            const mostrar = esRev && !!SOL_ID && estatus === 'FINALIZADA';
            btnRev.classList.toggle('d-none', !mostrar);
            btnRev.href = mostrar ? ('credito_revolvente.aspx?id=' + encodeURIComponent(SOL_ID)) : '#';
        }

        if (esRev) limpiarAmortizacionUI();
    }

    function onProductoFinancieroChange() {
        const pf = int(document.getElementById('producto_financiero_id')?.value, 0);
        actualizarUIRevolvente();
        cargarPeriodosProducto(pf, null);
    }

    function onPeriodoProductoChange() {
        const el = document.getElementById('producto_financiero_periodo_id');
        if (!el || !el.value) {
            const plazoEl = document.getElementById('plazo');
            const tasaEl = document.getElementById('tasa_entrada');
            if (plazoEl) plazoEl.value = '';
            if (tasaEl) tasaEl.value = '';
            return;
        }

        const opt = el.options[el.selectedIndex];
        const plazo = int(opt.dataset.plazo, 0);
        const tasaPct = num(opt.dataset.tasaPct, 0);

        document.getElementById('plazo').value = plazo > 0 ? plazo : '';
        document.getElementById('tasa_entrada').value = tasaPct > 0 ? tasaPct.toFixed(6) : '';
    }

    // =================== Estado ===================
    let SOL_ID = null;
    let CONTACTO_ID = null;
    let AMORT_DATA = null;
    let SOL_DATA = null;
    let CLIENTE_DATA = null;
    let CONTACTO_DATA = null;
    let TELEFONOS_DATA = [];
    let EMAILS_DATA = [];
    let DOMICILIOS_DATA = [];
    let PRODUCTOS_DATA = [];

    // =================== Init ===================
    document.addEventListener("DOMContentLoaded", function () {
        // Cargar combos de catálogos
        cargarCombos();

        // Wire selects (producto/periodicidad)
        const elPF = document.getElementById('producto_financiero_id');
        if (elPF) elPF.addEventListener('change', onProductoFinancieroChange);

        const elPer = document.getElementById('producto_financiero_periodo_id');
        if (elPer) elPer.addEventListener('change', onPeriodoProductoChange);

        // Wire botones
        document.getElementById('btnNuevo').addEventListener('click', limpiarTodo);
        document.getElementById('btnFinalizar').addEventListener('click', finalizarSolicitud);
        document.getElementById('btnGuardarOperacion').addEventListener('click', guardarOperacion);
        document.getElementById('btnAplicarPLD').addEventListener('click', aplicarPLD);
        document.getElementById('btnRecalcularPLD').addEventListener('click', aplicarPLD);
        document.getElementById('btnRefrescarPLD').addEventListener('click', cargarPLDDetalle);
        document.getElementById('btnDescargarAmortizacion').addEventListener('click', descargarTablaAmortizacionPDF);
        document.getElementById('tab-amortizacion').addEventListener('click', () => { cargarAmortizacionCondusef(false); });

        // Contacto
        document.getElementById('btnCrearContacto').addEventListener('click', crearOBtenerContacto);
        document.getElementById('btnActualizarContacto').addEventListener('click', actualizarContacto);
        document.getElementById('btnAgregarTel').addEventListener('click', agregarTelefono);
        document.getElementById('btnAgregarEmail').addEventListener('click', agregarEmail);
        document.getElementById('btnGuardarDom').addEventListener('click', guardarDomicilio);

        // Identidad
        document.getElementById('btnGuardarCliente').addEventListener('click', guardarCliente);
        document.getElementById('btnBuscarPorRFC').addEventListener('click', () => buscarCliente('rfc'));
        document.getElementById('btnBuscarPorCURP').addEventListener('click', () => buscarCliente('curp'));

        // Dependencias de combos (identidad)
        document.addEventListener('change', async (e) => {
            if (e.target && e.target.id === 'cli_pais_nacimiento') {
                await actualizarEntidadNacimiento();
            }
        });
        // Dependencias de combos (domicilio)
        document.addEventListener('change', async (e) => {
            if (e.target && e.target.id === 'dom_pais') {
                const paisId = document.getElementById('dom_pais').value;

                // Regla: Estado/Municipio domicilio SOLO aplican si País domicilio = México
                if (!paisId || !isMexico('dom_pais')) {
                    setOptionsPlaceholder('dom_estado', '— No aplica (solo México) —');
                    setOptionsPlaceholder('dom_municipio', '— No aplica (solo México) —');
                    setDisabled('dom_estado', true);
                    setDisabled('dom_municipio', true);
                    return;
                }

                setDisabled('dom_estado', false);
                setDisabled('dom_municipio', false);

                await cargarEstados('dom_estado', paisId);
                document.getElementById('dom_municipio').innerHTML = '<option value="">— Selecciona estado —</option>';
            }
            if (e.target && e.target.id === 'dom_estado') {
                const edoId = document.getElementById('dom_estado').value;

                if (document.getElementById('dom_municipio')?.disabled) {
                    // No aplica (solo México)
                    return;
                }

                if (!edoId) {
                    document.getElementById('dom_municipio').innerHTML = '<option value="">— Selecciona estado —</option>';
                    return;
                }

                await cargarMunicipios('dom_municipio', edoId);
            }
        });

        // Si viene ?id= cargar solicitud
        const idQS = int(qs('id'), 0);
        if (idQS > 0) {
            SOL_ID = idQS;
            // NOTA: la carga de la solicitud se ejecuta al final de cargarCombos()
            // para asegurar que todos los <select> ya estén poblados.
            // cargarSolicitud();
        } else {
            setEstatus('BORRADOR');
        }
    });

    // =================== Helpers de catálogos (NUEVOS) ===================
    async function cargarNacionalidades(selectId, preselect = '') {
        const el = document.getElementById(selectId);
        el.innerHTML = '<option value="">Cargando nacionalidades...</option>';
        const r = await fetch(H_CAT + '?action=nacionalidades&pageSize=500');
        const j = await r.json();
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        }
    }
    async function cargarPaises(selectId, preselect = '') {
        const el = document.getElementById(selectId);
        el.innerHTML = '<option value="">Cargando países...</option>';
        const r = await fetch(H_CAT + '?action=paises');
        const j = await r.json();
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        }
    }
    async function cargarEstados(selectId, paisId, preselect = '') {
        const el = document.getElementById(selectId);
        if (!paisId) { el.innerHTML = '<option value="">— Selecciona país —</option>'; return; }
        el.innerHTML = '<option value="">Cargando estados...</option>';
        const r = await fetch(H_CAT + `?action=estados&pais_id=${encodeURIComponent(paisId)}`);
        const j = await r.json();
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        }
    }

    async function actualizarEntidadNacimiento(estadoId = '', estadoTexto = '') {
        const paisId = document.getElementById('cli_pais_nacimiento')?.value || '';

        if (!paisId || !isMexico('cli_pais_nacimiento')) {
            setOptionsPlaceholder('cli_entidad_nacimiento', '— No aplica (solo México) —');
            setDisabled('cli_entidad_nacimiento', true);
            return;
        }

        setDisabled('cli_entidad_nacimiento', true);
        await cargarEstados('cli_entidad_nacimiento', paisId);
        setSelectByValueOrText('cli_entidad_nacimiento', estadoId, estadoTexto);
        setDisabled('cli_entidad_nacimiento', false);
    }
    async function cargarMunicipios(selectId, estadoId, preselect = '') {
        const el = document.getElementById(selectId);
        if (!estadoId) { el.innerHTML = '<option value="">— Selecciona estado —</option>'; return; }
        el.innerHTML = '<option value="">Cargando municipios...</option>';
        const r = await fetch(H_CAT + `?action=municipios&estado_id=${encodeURIComponent(estadoId)}`);
        const j = await r.json();
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        }
    }
    async function cargarActividadesEconomicas(selectId, preselect = '') {
        const el = document.getElementById(selectId);
        if (!el) return;
        el.innerHTML = '<option value="">Cargando actividades...</option>';
        const r = await fetch(H_CAT + '?action=actividades_economicas&pageSize=1000');
        const j = await r.json();
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        }
    }

    // ===== NUEVOS: Ocupaciones + Crédito PLD (si existen en el ASPX) =====
    async function cargarOcupaciones(selectId, preselect = '') {
        const el = document.getElementById(selectId);
        if (!el) return;
        el.innerHTML = '<option value="">Cargando ocupaciones...</option>';

        // Fallback: el handler puede exponer distintos action; intentamos varios.
        const actions = ['ocupaciones', 'ocupacion', 'catalogo_ocupacion'];
        let j = null;

        for (const a of actions) {
            try {
                const r = await fetch(H_CAT + `?action=${encodeURIComponent(a)}&pageSize=2000`);
                j = await r.json();
                if (j && j.ok) break;
            } catch (e) { /* try next */ }
        }

        el.innerHTML = '<option value="">— Seleccionar —</option>';
        if (j && j.ok) {
            (j.data || []).forEach(x => {
                el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
            });
            if (preselect) setSelect(selectId, preselect);
        } else {
            el.innerHTML = '<option value="">(Sin datos)</option>';
        }
    }

    async function cargarCreditosPLD(selectId, preselect = '') {
        const el = document.getElementById(selectId);
        if (!el) return;
        el.innerHTML = '<option value="">Cargando crédito PLD...</option>';
        try {
            const r = await fetch(H_CAT + '?action=creditos_pld&pageSize=1000');
            const j = await r.json();
            el.innerHTML = '<option value="">— Seleccionar —</option>';
            if (j.ok) {
                (j.data || []).forEach(x => {
                    el.insertAdjacentHTML('beforeend', `<option value="${x.id}">${x.descripcion}</option>`);
                });
                if (preselect) setSelect(selectId, preselect);
            } else {
                el.innerHTML = '<option value="">(Sin datos)</option>';
            }
        } catch (e) {
            el.innerHTML = '<option value="">(Sin catálogo)</option>';
        }
    }

    // =================== Combo genérico (FALTABA) ===================
    async function cargarCombo(url, elId, valField, txtField) {
        const el = document.getElementById(elId);
        if (!el) return;

        el.innerHTML = '<option value="">Cargando...</option>';
        const r = await fetch(url);
        const j = await r.json();

        if (!j.ok) {
            el.innerHTML = '<option value="">Error</option>';
            return;
        }

        const data = j.data || [];
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        for (const row of data) {
            el.insertAdjacentHTML('beforeend', `<option value="${row[valField]}">${(row[txtField] || '').toString()}</option>`);
        }
    }


    async function cargarProductosFinancieros() {
        const el = document.getElementById('producto_financiero_id');
        if (!el) return;

        el.innerHTML = '<option value="">Cargando...</option>';
        const r = await fetch(H_CAT + '?action=producto_financiero');
        const j = await r.json();

        if (!j.ok) {
            el.innerHTML = '<option value="">Error</option>';
            PRODUCTOS_DATA = [];
            return;
        }

        PRODUCTOS_DATA = j.data || [];
        el.innerHTML = '<option value="">— Seleccionar —</option>';
        for (const row of PRODUCTOS_DATA) {
            const opt = document.createElement('option');
            opt.value = row.id;
            opt.textContent = (row.descripcion || '').toString();
            opt.dataset.esRevolvente = (row.es_revolvente === true || row.es_revolvente === 1 || row.es_revolvente === '1') ? '1' : '0';
            opt.dataset.tipoCredito = (row.tipo_credito || '').toString();
            el.appendChild(opt);
        }
    }

    async function cargarCombos() {
        try {
            await cargarProductosFinancieros();
            await cargarCombo(H_CAT + '?action=canales_pago', 'canal_pago_id', 'id', 'descripcion');
            await cargarCombo(H_CAT + '?action=destinos_recursos', 'destino_recursos_id', 'id', 'descripcion');
            await cargarCombo(H_CAT + '?action=origen_recursos', 'origen_recursos_id', 'id', 'descripcion');  // <-- SOLO AQUÍ
            await cargarCombo(H_CAT + '?action=monedas', 'moneda_id', 'id', 'descripcion');
            await cargarCombo(H_CAT + '?action=medios_contacto', 'medio_contacto_id', 'id', 'descripcion');

            // Periodicidades dependen del producto
            resetPeriodosUI();

            // Identidad / Domicilio (handler nuevo)
            await cargarNacionalidades('cli_nacionalidad');
            await cargarPaises('cli_pais_nacimiento');

            setOptionsPlaceholder('cli_entidad_nacimiento', '— No aplica (solo México) —');
            setDisabled('cli_entidad_nacimiento', true);
            await cargarActividadesEconomicas('cli_actividad_economica');

            // NUEVO: Catálogos PLD faltantes (si existen en el ASPX)
            await cargarOcupaciones('cli_ocupacion');
            await cargarOcupaciones('ocupacion_id');
            await cargarCreditosPLD('credito_pld_id');
            await cargarPaises('dom_pais');

            setOptionsPlaceholder('dom_estado', '— No aplica (solo México) —');
            setOptionsPlaceholder('dom_municipio', '— No aplica (solo México) —');
            setDisabled('dom_estado', true);
            setDisabled('dom_municipio', true);

            // ======================================================
            // PARCHE CRÍTICO: cargar la solicitud SOLO DESPUÉS DE QUE
            // TODOS los combos ya fueron llenados correctamente.
            // ======================================================
            if (SOL_ID) {
                await cargarSolicitud();
            }

        } catch (e) {
            swal.fire('Catálogos', 'No fue posible cargar catálogos: ' + e, 'error');
        }
    }



    // =================== Operación ===================
    function leerOperacion() {
        return {
            producto_financiero_id: int(document.getElementById('producto_financiero_id').value, 0),
            producto_financiero_periodo_id: int(document.getElementById('producto_financiero_periodo_id').value, 0),
            canal_pago_id: int(document.getElementById('canal_pago_id').value, 0),
            destino_recursos_id: int(document.getElementById('destino_recursos_id').value, 0),
            origen_recursos_id: int(document.getElementById('origen_recursos_id').value, 0),   // <-- NUEVO
            moneda_id: int(document.getElementById('moneda_id').value, 0),
            monto_solicitado: num(document.getElementById('monto_solicitado').value, 0),
            plazo: int(document.getElementById('plazo').value, 0),
            tasa_entrada: num(document.getElementById('tasa_entrada').value, 0),
            observaciones: (document.getElementById('observaciones').value || '').toString().trim(),
            permitir_pagos_anticipados: !!document.getElementById('permitir_pagos_anticipados')?.checked
        };
    }

    async function guardarOperacion() {
        const p = leerOperacion();

        if (p.producto_financiero_id <= 0) {
            return swal.fire('Operación', 'Selecciona el producto financiero.', 'warning');
        }
        if (p.producto_financiero_periodo_id <= 0) {
            return swal.fire('Operación', 'Selecciona la periodicidad del producto.', 'warning');
        }
        if (p.monto_solicitado <= 0) {
            return swal.fire('Operación', 'Captura un monto válido.', 'warning');
        }
        if (p.plazo <= 0) {
            return swal.fire('Operación', 'Captura el plazo.', 'warning');
        }
        if (p.origen_recursos_id <= 0) {
            return swal.fire('Operación', 'Selecciona el origen de recursos.', 'warning');
        }


        try {
            if (!SOL_ID) {
                // ==============================
                // CREAR SOLICITUD
                // ==============================
                const params = new URLSearchParams();
                params.append('producto_financiero_id', p.producto_financiero_id);
                params.append('producto_financiero_periodo_id', p.producto_financiero_periodo_id);

                if (p.canal_pago_id > 0) params.append('canal_pago_id', p.canal_pago_id);
                if (p.destino_recursos_id > 0) params.append('destino_recursos_id', p.destino_recursos_id);
                if (p.origen_recursos_id > 0) params.append('origen_recursos_id', p.origen_recursos_id);   // NUEVO
                if (p.moneda_id > 0) params.append('moneda_id', p.moneda_id);

                params.append('monto_solicitado', p.monto_solicitado);
                params.append('plazo', p.plazo);
                params.append('tasa_entrada', p.tasa_entrada);
                params.append('permitir_pagos_anticipados', p.permitir_pagos_anticipados ? '1' : '0');

                if (p.observaciones)
                    params.append('observaciones', p.observaciones);

                const r = await fetch(H_SOL + '?action=crear', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                    body: params.toString()
                });

                const j = await r.json();
                if (!j.ok) return swal.fire('Operación', j.message || 'No fue posible guardar.', 'error');

                if (j.solicitud_id)
                    SOL_ID = j.solicitud_id;

            } else {
                // ==============================
                // ACTUALIZAR SOLICITUD
                // ==============================
                const params = new URLSearchParams();
                params.append('solicitud_id', SOL_ID);
                params.append('producto_financiero_id', p.producto_financiero_id);
                params.append('producto_financiero_periodo_id', p.producto_financiero_periodo_id);

                // Estos NO estaban antes — AHORA SE GUARDAN CORRECTAMENTE
                if (p.canal_pago_id > 0) params.append('canal_pago_id', p.canal_pago_id);
                if (p.destino_recursos_id > 0) params.append('destino_recursos_id', p.destino_recursos_id);
                if (p.origen_recursos_id > 0) params.append('origen_recursos_id', p.origen_recursos_id); // << CRÍTICO
                if (p.moneda_id > 0) params.append('moneda_id', p.moneda_id);

                params.append('monto_solicitado', p.monto_solicitado);
                params.append('plazo', p.plazo);
                params.append('tasa_entrada', p.tasa_entrada);
                params.append('permitir_pagos_anticipados', p.permitir_pagos_anticipados ? '1' : '0');

                if (p.observaciones)
                    params.append('observaciones', p.observaciones);

                const r = await fetch(H_SOL + '?action=actualizar_operacion', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                    body: params.toString()
                });

                const j = await r.json();
                if (!j.ok) return swal.fire('Operación', j.message || 'No fue posible guardar.', 'error');
            }

            await cargarSolicitud();
            swal.fire('Operación', 'Datos guardados.', 'success');

        } catch (e) {
            swal.fire('Operación', 'Error: ' + e, 'error');
        }
    }


    async function cargarSolicitud() {
        if (!SOL_ID) return;

        try {
            const r = await fetch(H_SOL + '?action=obtener&solicitud_id=' + SOL_ID);
            const j = await r.json();

            if (!j.ok)
                return swal.fire('Solicitud', 'No se pudo obtener la solicitud.', 'error');

            // ============================
            // NUEVO MODELO DE DATOS
            // ============================
            const sol = j.data.solicitud;
            const cli = j.data.cliente;
            const cnt = j.data.contacto;
            SOL_DATA = sol || null;
            CLIENTE_DATA = cli || null;
            CONTACTO_DATA = cnt || null;
            TELEFONOS_DATA = j.data.telefonos || [];
            EMAILS_DATA = j.data.emails || [];
            DOMICILIOS_DATA = j.data.domicilios || [];

            // ============================
            // ENCABEZADO
            // ============================
            document.getElementById('lblSolicitudId').textContent = sol.id;
            setEstatus(sol.estatus || 'BORRADOR');

            // ============================
            // OPERACIÓN
            // ============================
            setSelect('producto_financiero_id', sol.producto_financiero_id);
            actualizarUIRevolvente();
            await cargarPeriodosProducto(sol.producto_financiero_id, sol.producto_financiero_periodo_id);
            setSelect('canal_pago_id', sol.canal_pago_id);
            setSelect('destino_recursos_id', sol.destino_recursos_id);
            setSelect('origen_recursos_id', sol.origen_recursos_id);
            setSelect('moneda_id', sol.moneda_id);

            document.getElementById('monto_solicitado').value = sol.monto_solicitado ?? '';
            // Plazo y tasa se fijan desde la periodicidad; si es legacy sin periodo, mostramos lo guardado.
            if (!sol.producto_financiero_periodo_id) {
                document.getElementById('plazo').value = sol.plazo ?? '';
                document.getElementById('tasa_entrada').value = sol.tasa_entrada ?? '';
            }
            document.getElementById('observaciones').value = sol.observaciones ?? '';
            document.getElementById('permitir_pagos_anticipados').checked = !!sol.permitir_pagos_anticipados;

            // ============================
            // PLD RESUMEN
            // ============================
            document.getElementById('lblPuntaje').textContent = (sol.pld_puntaje_total ?? 0).toFixed(4);
            document.getElementById('lblNivelPLD').textContent = (sol.pld_nivel_riesgo_pld ?? 0).toFixed(2);
            document.getElementById('lblMetodoPLD').textContent = sol.pld_metodologia_version ?? '—';
            document.getElementById('lblFechaPLD').textContent = fmt(sol.pld_fecha_calculo);

            // ===== NUEVO: Tipo de riesgo (si el backend ya lo expone en la solicitud) =====
            document.getElementById('lblTipoRiesgo').textContent = (sol.pld_tipo_riesgo ?? '—').toString();

            // ============================
            // IDENTIDAD DEL CLIENTE
            // ============================
            if (cli) {
                document.getElementById('cli_id').value = cli.id;
                await pintarCliente(cli);
            }

            // ============================
            // CONTACTO COMPLETO
            // ============================
            if (cnt) {
                CONTACTO_ID = cnt.id;
                document.getElementById('lblContactoId').textContent = cnt.id;

                setSelect('medio_contacto_id', cnt.medio_contacto_id);
                document.getElementById('consentimiento_comunicacion').checked = !!cnt.consentimiento_comunicacion;

                // Pintar teléfonos
                const tbTel = document.querySelector('#tblTelefonos tbody');
                tbTel.innerHTML = '';
                TELEFONOS_DATA.forEach(t => tbTel.insertAdjacentHTML('beforeend', filaTelefonoHTML(t)));

                // Pintar emails
                const tbMail = document.querySelector('#tblEmails tbody');
                tbMail.innerHTML = '';
                EMAILS_DATA.forEach(m => tbMail.insertAdjacentHTML('beforeend', filaEmailHTML(m)));

                // Pintar domicilios
                const tbDom = document.querySelector('#tblDomicilios tbody');
                tbDom.innerHTML = '';
                DOMICILIOS_DATA.forEach(d => tbDom.insertAdjacentHTML('beforeend', filaDomHTML(d)));
            } else {
                CONTACTO_ID = null;
                CONTACTO_DATA = null;
                TELEFONOS_DATA = [];
                EMAILS_DATA = [];
                DOMICILIOS_DATA = [];
        PRODUCTOS_DATA = PRODUCTOS_DATA || [];
                document.getElementById('lblContactoId').textContent = '—';
            }

            // ============================
            // PLD DETALLE
            // ============================
            await cargarPLDDetalle();
            if (!productoSeleccionadoEsRevolvente() && !SOL_DATA?.es_revolvente) {
                await cargarAmortizacionCondusef(false);
            } else {
                limpiarAmortizacionUI();
            }
            actualizarUIRevolvente();

        } catch (e) {
            swal.fire('Solicitud', 'Error: ' + e, 'error');
        }
    }


    function limpiarTodo() {
        SOL_ID = null;
        CONTACTO_ID = null;
        SOL_DATA = null;
        CLIENTE_DATA = null;
        CONTACTO_DATA = null;
        TELEFONOS_DATA = [];
        EMAILS_DATA = [];
        DOMICILIOS_DATA = [];

        document.getElementById('lblSolicitudId').textContent = '—';
        setEstatus('BORRADOR');

        // Operación
        for (const id of ['producto_financiero_id', 'canal_pago_id', 'destino_recursos_id', 'moneda_id']) {
            setSelect(id, '');
        }

        document.getElementById('monto_solicitado').value = '';
        document.getElementById('plazo').value = '';
        document.getElementById('tasa_entrada').value = '';
        document.getElementById('observaciones').value = '';
        document.getElementById('permitir_pagos_anticipados').checked = false;

        // Identidad
        const idsTxt = [
            'cli_id', 'cli_rfc', 'cli_curp', 'cli_primer_nombre', 'cli_segundo_nombre', 'cli_ap_paterno', 'cli_ap_materno',
            'cli_fecha_nac', 'cli_puesto', 'cli_empresa', 'cli_ingreso_mensual', 'cli_otros_ingresos', 'cli_origen_otros_ingresos',
            'cli_perfil_pagos_mensuales', 'cli_perfil_monto_mensual',
            'cli_ident_numero', 'cli_ident_vigencia', 'cli_estatus'
        ];
        idsTxt.forEach(i => {
            const el = document.getElementById(i);
            if (el) { el.value = (i === 'cli_estatus') ? 'ACTIVO' : ''; }
        });

        const idsNum = ['cli_dependientes', 'cli_antiguedad_anios'];
        idsNum.forEach(i => {
            const el = document.getElementById(i);
            if (el) { el.value = 0; }
        });

        const idsSel = [
            'cli_persona_tipo', 'cli_estado_civil', 'cli_regimen_matrimonial', 'cli_escolaridad', 'cli_ident_tipo',
            'cli_nacionalidad', 'cli_pais_nacimiento', 'cli_entidad_nacimiento', 'cli_actividad_economica'
        ];
        idsSel.forEach(i => {
            const el = document.getElementById(i);
            if (el) { el.value = ''; }
        });

        document.getElementById('cli_pep').checked = false;
        document.getElementById('cli_acepta_avisos').checked = false;
        document.getElementById('cli_razon_social').value = '';

        // Contacto
        document.getElementById('medio_contacto_id').value = '';
        document.getElementById('consentimiento_comunicacion').checked = false;
        document.getElementById('lblContactoId').textContent = '—';
        document.querySelector('#tblTelefonos tbody').innerHTML = '';
        document.querySelector('#tblEmails tbody').innerHTML = '';
        document.querySelector('#tblDomicilios tbody').innerHTML = '';

        // Domicilio
        ['dom_pais', 'dom_estado', 'dom_municipio'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
        ['dom_calle', 'dom_ext', 'dom_int', 'dom_colonia', 'dom_cp'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
        document.getElementById('dom_principal').checked = true;

        // PLD
        document.getElementById('lblPuntaje').textContent = '0.0000';
        document.getElementById('lblNivelPLD').textContent = '0.00';
        document.getElementById('lblMetodoPLD').textContent = '—';   // CORREGIDO
        document.getElementById('lblFechaPLD').textContent = '—';
        document.getElementById('lblTipoRiesgo').textContent = '—';  // NUEVO
        document.querySelector('#tblPLDDetalle tbody').innerHTML = '';
        document.getElementById('lblFactores').textContent = '0';
        document.getElementById('lblSumaNivel').textContent = '0.00';
        document.getElementById('lblPromNivel').textContent = '0.0000';

        limpiarAmortizacionUI();
    }


    // =================== Identidad del Cliente ===================
    function leerCliente() {
        const anios = int(document.getElementById('cli_antiguedad_anios')?.value, 0);

        const selNac = document.getElementById('cli_nacionalidad');
        const nacionalidad_id = selNac?.value || null;
        const nacionalidad_txt = selNac?.options[selNac.selectedIndex]?.text || null;

        const selPaisN = document.getElementById('cli_pais_nacimiento');
        const pais_nac_id = selPaisN?.value || null;
        const pais_nac_txt = selPaisN?.options[selPaisN.selectedIndex]?.text || null;

        const selEntN = document.getElementById('cli_entidad_nacimiento');
        const ent_nac_id = selEntN?.value || null;
        const ent_nac_txt = selEntN?.options[selEntN.selectedIndex]?.text || null;

        const selAct = document.getElementById('cli_actividad_economica');
        const act_id = selAct?.value || null;
        const act_txt = selAct?.options[selAct.selectedIndex]?.text || null;

        return {
            persona_tipo: 'PF',
            primer_nombre: (document.getElementById('cli_primer_nombre').value || '').trim(),
            segundo_nombre: (document.getElementById('cli_segundo_nombre').value || '').trim() || null,
            ap_paterno: (document.getElementById('cli_ap_paterno').value || '').trim(),
            ap_materno: (document.getElementById('cli_ap_materno').value || '').trim(),
            fecha_nacimiento: (document.getElementById('cli_fecha_nac').value || '').trim() || null,
            sexo: (document.getElementById('cli_sexo').value || '').trim() || null,
            curp: (document.getElementById('cli_curp').value || '').trim() || null,
            rfc: (document.getElementById('cli_rfc').value || '').trim(),

            nacionalidad: nacionalidad_txt,
            nacionalidad_id: nacionalidad_id,
            pais_nacimiento: pais_nac_txt,
            pais_nacimiento_id: pais_nac_id,
            entidad_nacimiento: ent_nac_txt,
            estado_nacimiento_id: ent_nac_id,

            estado_civil: (document.getElementById('cli_estado_civil').value || '').trim() || null,
            regimen_matrimonial: (document.getElementById('cli_regimen_matrimonial').value || '').trim() || null,
            dependientes: int(document.getElementById('cli_dependientes').value, 0),
            escolaridad: (document.getElementById('cli_escolaridad').value || '').trim() || null,

            actividad_economica: act_txt,
            actividad_economica_id: act_id,
            puesto: (document.getElementById('cli_puesto').value || '').trim() || null,
            empresa: (document.getElementById('cli_empresa').value || '').trim() || null,
            antiguedad_meses: anios * 12,

            ingreso_mensual: num(document.getElementById('cli_ingreso_mensual').value, 0),
            otros_ingresos: num(document.getElementById('cli_otros_ingresos').value, 0),
            origen_otros_ingresos: (document.getElementById('cli_origen_otros_ingresos').value || '').trim() || null,
            perfil_pagos_mensuales_esperados: document.getElementById('cli_perfil_pagos_mensuales').value === '' ? null : int(document.getElementById('cli_perfil_pagos_mensuales').value, 0),
            perfil_monto_mensual_esperado: document.getElementById('cli_perfil_monto_mensual').value === '' ? null : num(document.getElementById('cli_perfil_monto_mensual').value, 0),

            pep: !!document.getElementById('cli_pep').checked,
            acepta_avisos: !!document.getElementById('cli_acepta_avisos').checked,
            ident_tipo: (document.getElementById('cli_ident_tipo').value || '').trim() || null,
            ident_numero: (document.getElementById('cli_ident_numero').value || '').trim() || null,
            ident_vigencia: (document.getElementById('cli_ident_vigencia').value || '').trim() || null
        };
    }
    async function guardarCliente() {
        const cliId = int(document.getElementById('cli_id').value, 0);
        const p = leerCliente();

        if (!p.rfc || !p.primer_nombre || !p.ap_paterno || !p.fecha_nacimiento || !p.sexo || !p.estado_civil) {
            return swal.fire(
                'Identidad',
                'Completa los campos obligatorios: RFC, Primer nombre, Apellido paterno, Fecha de nacimiento, Sexo y Estado civil.',
                'warning'
            );
        }
        if (p.perfil_pagos_mensuales_esperados !== null && p.perfil_pagos_mensuales_esperados < 0) {
            return swal.fire('Perfil transaccional', 'Los pagos esperados por mes no pueden ser negativos.', 'warning');
        }
        if (p.perfil_monto_mensual_esperado !== null && p.perfil_monto_mensual_esperado < 0) {
            return swal.fire('Perfil transaccional', 'El monto mensual esperado no puede ser negativo.', 'warning');
        }

        try {
            let url = H_CLI + (cliId > 0 ? '?action=actualizar' : '?action=crear');
            const body = (cliId > 0) ? Object.assign({ id: cliId }, p) : p;

            const r = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            const j = await r.json();
            if (!j.ok) return swal.fire('Identidad', j.message || 'No fue posible guardar.', 'error');

            // 1) Actualizar el input local con el id generado
            if (!cliId && j.cliente_id) {
                document.getElementById('cli_id').value = j.cliente_id;
            }

            // 2) Actualizar solicitud_credito.cliente_id
            if (SOL_ID && j.cliente_id) {
                await fetch(H_SOL + "?action=actualizar_relaciones", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        solicitud_id: SOL_ID,
                        cliente_id: j.cliente_id
                    })
                });

                // 3) Refrescar datos de pantalla
                await cargarSolicitud();
            }

            swal.fire('Identidad', 'Datos guardados.', 'success');

        } catch (e) {
            swal.fire('Identidad', 'Error: ' + e, 'error');
        }
    }

    async function buscarCliente(tipo) {
        const val = (tipo === 'rfc')
            ? (document.getElementById('cli_rfc').value || '').trim()
            : (document.getElementById('cli_curp').value || '').trim();

        if (!val)
            return swal.fire('Identidad', 'Captura un valor para buscar.', 'warning');

        try {
            const r = await fetch(H_CLI + `?action=listar&search=${encodeURIComponent(val)}&page=1&pageSize=1`);
            const j = await r.json();

            if (!j.ok || !j.data || j.data.length === 0)
                return swal.fire('Identidad', 'Cliente no encontrado.', 'info');

            // Cliente encontrado
            const d = j.data[0];

            // Pintamos el ID en pantalla
            document.getElementById('cli_id').value = d.id || '';

            // Obtenemos sus datos completos
            const r2 = await fetch(H_CLI + `?action=obtener&id=${d.id}`);
            const j2 = await r2.json();
            if (!j2.ok) return;

            // Pintar datos en el formulario
            await pintarCliente(j2.data);

            // ===============================
            // PARCHE CRÍTICO:
            // Asignar automáticamente el cliente a la solicitud
            // ===============================
            if (SOL_ID && d.id) {
                await fetch(H_SOL + "?action=actualizar_relaciones", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        solicitud_id: SOL_ID,
                        cliente_id: d.id
                    })
                });

                // Refrescar la solicitud para evitar inconsistencias
                await cargarSolicitud();
            }

            swal.fire('Identidad', 'Cliente cargado.', 'success');

        } catch (e) {
            swal.fire('Identidad', 'Error: ' + e, 'error');
        }
    }


    async function pintarCliente(d) {
        if (!d) return;

        // ================== CAMPOS DIRECTOS ==================
        document.getElementById('cli_id').value = d.id_cliente || d.id || '';
        document.getElementById('cli_rfc').value = d.rfc || '';
        document.getElementById('cli_curp').value = d.curp || '';
        document.getElementById('cli_primer_nombre').value = d.primer_nombre || '';
        document.getElementById('cli_segundo_nombre').value = d.segundo_nombre || '';
        document.getElementById('cli_ap_paterno').value =
            d.apellido_paterno || d.ap_paterno || '';
        document.getElementById('cli_ap_materno').value = d.apellido_materno || d.ap_materno || '';

        // Fecha nacimiento
        let f = '';
        if (d.fecha_nacimiento) {
            if (typeof d.fecha_nacimiento === 'string' && d.fecha_nacimiento.startsWith('/Date(')) {
                const ms = parseInt(d.fecha_nacimiento.substring(6), 10);
                f = new Date(ms).toISOString().slice(0, 10);
            } else if (typeof d.fecha_nacimiento === 'string' && d.fecha_nacimiento.length >= 10) {
                f = d.fecha_nacimiento.slice(0, 10);
            }
        }
        document.getElementById('cli_fecha_nac').value = f;
        document.getElementById('cli_sexo').value = d.sexo || '';

        // ================== CAMPOS QUE NO DEPENDEN DE CATÁLOGOS ==================
        // Estado civil / régimen / dependientes / escolaridad
        setTimeout(() => {
            document.getElementById('cli_estado_civil').value = (d.estado_civil || '').trim().toUpperCase();
            document.getElementById('cli_regimen_matrimonial').value = d.regimen_matrimonial || '';
            document.getElementById('cli_dependientes').value = d.dependientes ?? 0;
            document.getElementById('cli_escolaridad').value = d.escolaridad || '';
        }, 150);

        // Antigüedad laboral (meses → años)
        const meses = parseInt(d.antiguedad_meses || d.antiguedad_empleo || '0', 10);
        document.getElementById('cli_antiguedad_anios').value = isNaN(meses) ? 0 : Math.floor(meses / 12);

        // Ingresos
        document.getElementById('cli_ingreso_mensual').value = d.ingreso_mensual ?? '';
        document.getElementById('cli_otros_ingresos').value = d.otros_ingresos ?? '';
        document.getElementById('cli_origen_otros_ingresos').value = d.origen_otros_ingresos || '';
        document.getElementById('cli_perfil_pagos_mensuales').value = d.perfil_pagos_mensuales_esperados ?? '';
        document.getElementById('cli_perfil_monto_mensual').value = d.perfil_monto_mensual_esperado ?? '';

        // Empresa / puesto
        document.getElementById('cli_empresa').value = d.empresa || '';
        document.getElementById('cli_puesto').value = d.puesto || '';

        // PEP / Avisos
        document.getElementById('cli_pep').checked = !!d.puesto_politico || !!d.pep;
        document.getElementById('cli_acepta_avisos').checked = !!d.acepta_aviso_privacidad || !!d.acepta_avisos;

        // ================== Identificación ==================
        document.getElementById('cli_ident_tipo').value = d.tipo_identificacion || d.ident_tipo || '';
        document.getElementById('cli_ident_numero').value = d.numero_identificacion || d.ident_numero || '';

        let fv = '';
        if (d.vigencia_identificacion) {
            if (typeof d.vigencia_identificacion === 'string' && d.vigencia_identificacion.startsWith('/Date(')) {
                const ms = parseInt(d.vigencia_identificacion.substring(6), 10);
                fv = new Date(ms).toISOString().slice(0, 10);
            } else if (typeof d.vigencia_identificacion === 'string' && d.vigencia_identificacion.length >= 10) {
                fv = d.vigencia_identificacion.slice(0, 10);
            }
        }
        document.getElementById('cli_ident_vigencia').value = fv;

        document.getElementById('cli_estatus').value = d.estatus || 'ACTIVO';

        // ================== CAMPOS DEPENDIENTES DE CATÁLOGOS ==================
        // Carga en orden: nacionalidad → país nacimiento → estado nacimiento → actividad económica
        // ---- NACIONALIDAD ----
        await cargarNacionalidades('cli_nacionalidad');
        setSelectByValueOrText('cli_nacionalidad', d.nacionalidad_id, d.nacionalidad);

        // ---- PAÍS NACIMIENTO / ESTADO NACIMIENTO ----
        await cargarPaises('cli_pais_nacimiento');
        setSelectByValueOrText('cli_pais_nacimiento', d.pais_nacimiento_id, d.pais_nacimiento);
        await actualizarEntidadNacimiento(d.estado_nacimiento_id, d.entidad_nacimiento);

        // ---- ACTIVIDAD ECONÓMICA ----
        await cargarActividadesEconomicas('cli_actividad_economica');
        setSelectByValueOrText('cli_actividad_economica', d.actividad_economica_id, d.actividad_economica);
    }


    // =================== Contacto (igual) ===================

    // Stub para evitar error y permitir que el resto del DOMContentLoaded se ejecute
    async function actualizarContacto() {
        if (!CONTACTO_ID) {
            return swal.fire('Contacto', 'Primero crea/obtén el contacto.', 'warning');
        }

        const medioId = int(document.getElementById('medio_contacto_id').value, 0);
        if (medioId <= 0) {
            return swal.fire('Contacto', 'Selecciona el Medio de contacto antes de actualizar.', 'warning');
        }

        try {
            const payload = {
                contacto_id: CONTACTO_ID,              // <-- CLAVE CORRECTA (ANTES ESTABA "id")
                solicitud_id: SOL_ID || null,          // <-- opcional, no estorba y ayuda a validar
                medio_contacto_id: medioId,
                consentimiento_comunicacion: !!document.getElementById('consentimiento_comunicacion').checked
            };

            const r = await fetch(H_CNT + '?action=actualizar_contacto', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const j = await r.json();
            if (!j.ok)
                return swal.fire('Contacto', j.message || 'No fue posible actualizar el contacto.', 'error');

            await obtenerContacto(); // vuelve a pintar y deja el select con el valor ya guardado
            swal.fire('Contacto', 'Contacto actualizado.', 'success');

        } catch (e) {
            swal.fire('Contacto', 'Error: ' + e, 'error');
        }
    }



    async function crearOBtenerContacto() {
        if (!SOL_ID) return swal.fire('Contacto', 'Primero guarda la operación.', 'warning');
        try {
            const r = await fetch(H_CNT + '?action=crear_contacto', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({
                    solicitud_id: SOL_ID,
                    medio_contacto_id: int(document.getElementById('medio_contacto_id').value, 0) || null,
                    consentimiento_comunicacion: !!document.getElementById('consentimiento_comunicacion').checked
                })
            });
            const j = await r.json();
            if (!j.ok) return swal.fire('Contacto', j.message || 'No fue posible crear/obtener.', 'error');
            const d = j.data;
            CONTACTO_ID = d.id;
            document.getElementById('lblContactoId').textContent = CONTACTO_ID;
            swal.fire('Contacto', 'Contacto listo.', 'success');
            await obtenerContacto();
        } catch (e) { swal.fire('Contacto', 'Error: ' + e, 'error'); }
    }
    async function obtenerContacto() {
        if (!SOL_ID) return;
        try {
            const r = await fetch(H_CNT + '?action=obtener_contacto&solicitud_id=' + SOL_ID);
            const j = await r.json();
            if (!j.ok) {
                document.getElementById('lblContactoId').textContent = '—';
                CONTACTO_ID = null;
                return;
            }
            const c = j.data.contacto;
            CONTACTO_ID = c.id;
            CONTACTO_DATA = c;
            TELEFONOS_DATA = j.data.telefonos || [];
            EMAILS_DATA = j.data.emails || [];
            document.getElementById('lblContactoId').textContent = CONTACTO_ID;
            setSelect('medio_contacto_id', c.medio_contacto_id);
            document.getElementById('consentimiento_comunicacion').checked = !!c.consentimiento_comunicacion;

            const tbTel = document.querySelector('#tblTelefonos tbody'); tbTel.innerHTML = '';
            TELEFONOS_DATA.forEach(t => { tbTel.insertAdjacentHTML('beforeend', filaTelefonoHTML(t)); });

            const tbMail = document.querySelector('#tblEmails tbody'); tbMail.innerHTML = '';
            EMAILS_DATA.forEach(m => { tbMail.insertAdjacentHTML('beforeend', filaEmailHTML(m)); });

            await listarDomicilios();
        } catch (e) { console.warn(e); }
    }
    function filaTelefonoHTML(t) {
        return `<tr>
        <td>${t.tipo_telefono || ''}</td>
        <td>${t.numero || ''}</td>
        <td>${t.extension || ''}</td>
        <td>${t.es_principal ? 'Sí' : 'No'}</td>
        <td>${t.activo ? 'Sí' : 'No'}</td>
        <td class="text-nowrap">
            <button type="button" class="btn btn-sm btn-outline-success me-1" onclick="setPrincipalTel(${t.id})">Principal</button>
            <button type="button" class="btn btn-sm btn-outline-${t.activo ? 'danger' : 'primary'}" onclick="toggleTel(${t.id}, ${t.activo ? 0 : 1})">${t.activo ? 'Desactivar' : 'Activar'}</button>
        </td>
    </tr>`;
    }
    async function agregarTelefono() {
        if (!CONTACTO_ID) return swal.fire('Teléfonos', 'Primero crea/obtén el contacto.', 'warning');
        const tipo = document.getElementById('tel_tipo').value;
        const numero = (document.getElementById('tel_numero').value || '').trim();
        const extension = (document.getElementById('tel_ext').value || '').trim();
        const principal = !!document.getElementById('tel_principal').checked;
        if (!numero) return swal.fire('Teléfonos', 'Captura el número.', 'warning');
        try {
            const r = await fetch(H_CNT + '?action=upsert_telefono', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({
                    contacto_id: CONTACTO_ID, tipo_telefono: tipo, numero: numero, extension: extension || null, es_principal: principal
                })
            });
            const j = await r.json();
            if (!j.ok) return swal.fire('Teléfonos', j.message || 'No se pudo agregar.', 'error');
            await obtenerContacto();
            document.getElementById('tel_numero').value = '';
            document.getElementById('tel_ext').value = '';
            document.getElementById('tel_principal').checked = false;
        } catch (e) { swal.fire('Teléfonos', 'Error: ' + e, 'error'); }
    }
    async function setPrincipalTel(id) {
        try {
            const r = await fetch(H_CNT + '?action=set_principal_telefono', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id }) });
            const j = await r.json();
            if (!j.ok) return swal.fire('Teléfonos', j.message || 'No fue posible.', 'error');
            await obtenerContacto();
        } catch (e) { swal.fire('Teléfonos', 'Error: ' + e, 'error'); }
    }
    async function toggleTel(id, activo) {
        try {
            const r = await fetch(H_CNT + '?action=toggle_telefono', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id, activo: !!activo }) });
            const j = await r.json();
            if (!j.ok) return swal.fire('Teléfonos', j.message || 'No fue posible cambiar estado.', 'error');
            await obtenerContacto();
        } catch (e) { swal.fire('Teléfonos', 'Error: ' + e, 'error'); }
    }
    function filaEmailHTML(m) {
        return `<tr>
        <td>${m.email || ''}</td>
        <td>${m.es_principal ? 'Sí' : 'No'}</td>
        <td>${m.activo ? 'Sí' : 'No'}</td>
        <td class="text-nowrap">
            <button type="button" class="btn btn-sm btn-outline-success me-1" onclick="setPrincipalEmail(${m.id})">Principal</button>
            <button type="button" class="btn btn-sm btn-outline-${m.activo ? 'danger' : 'primary'}" onclick="toggleEmail(${m.id}, ${m.activo ? 0 : 1})">${m.activo ? 'Desactivar' : 'Activar'}</button>
        </td>
    </tr>`;
    }
    async function agregarEmail() {
        if (!CONTACTO_ID) return swal.fire('Emails', 'Primero crea/obtén el contacto.', 'warning');
        const email = (document.getElementById('email_val').value || '').trim();
        if (!email) return swal.fire('Emails', 'Captura el email.', 'warning');
        try {
            const r = await fetch(H_CNT + '?action=upsert_email', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({
                    contacto_id: CONTACTO_ID, email: email, es_principal: !!document.getElementById('email_principal').checked
                })
            });
            const j = await r.json();
            if (!j.ok) return swal.fire('Emails', j.message || 'No se pudo agregar.', 'error');
            await obtenerContacto();
            document.getElementById('email_val').value = '';
            document.getElementById('email_principal').checked = false;
        } catch (e) { swal.fire('Emails', 'Error: ' + e, 'error'); }
    }
    async function setPrincipalEmail(id) {
        try {
            const r = await fetch(H_CNT + '?action=set_principal_email', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id }) });
            const j = await r.json();
            if (!j.ok) return swal.fire('Emails', j.message || 'No fue posible.', 'error');
            await obtenerContacto();
        } catch (e) { swal.fire('Emails', 'Error: ' + e, 'error'); }
    }
    async function toggleEmail(id, activo) {
        try {
            const r = await fetch(H_CNT + '?action=toggle_email', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id, activo: !!activo }) });
            const j = await r.json();
            if (!j.ok) return swal.fire('Emails', j.message || 'No fue posible cambiar estado.', 'error');
            await obtenerContacto();
        } catch (e) { swal.fire('Emails', 'Error: ' + e, 'error'); }
    }

    // Domicilio
    function filaDomHTML(d) {
        return `<tr>
        <td>${d.calle || ''} ${d.num_ext || ''}${d.num_int ? ('/' + d.num_int) : ''}</td>
        <td>${d.colonia || ''}</td>
        <td>${d.municipio || ''}</td>
        <td>${d.estado || ''}</td>
        <td>${d.cp || ''}</td>
        <td>${d.es_principal ? 'Sí' : 'No'}</td>
        <td>${d.activo ? 'Sí' : 'No'}</td>
        <td class="text-nowrap">
            <button type="button" class="btn btn-sm btn-outline-success me-1" onclick="setPrincipalDom(${d.id})">Principal</button>
            <button type="button" class="btn btn-sm btn-outline-${d.activo ? 'danger' : 'primary'}" onclick="toggleDom(${d.id}, ${d.activo ? 0 : 1})">${d.activo ? 'Desactivar' : 'Activar'}</button>
        </td>
    </tr>`;
    }
    async function guardarDomicilio() {
        if (!CONTACTO_ID) return swal.fire('Domicilio', 'Primero crea/obtén el contacto.', 'warning');

        const selPais = document.getElementById('dom_pais');
        const pais_id = selPais?.value || null;
        const pais_txt = selPais?.options[selPais.selectedIndex]?.text || '';

        const selEdo = document.getElementById('dom_estado');
        const estado_id = selEdo?.value || null;
        const estado_txt = selEdo?.options[selEdo.selectedIndex]?.text || '';

        const selMun = document.getElementById('dom_municipio');
        const municipio_id = selMun?.value || null;
        const municipio_txt = selMun?.options[selMun.selectedIndex]?.text || '';

        const payload = {
            contacto_id: CONTACTO_ID,
            calle: (document.getElementById('dom_calle').value || '').trim(),
            num_ext: (document.getElementById('dom_ext').value || '').trim(),
            num_int: (document.getElementById('dom_int').value || '').trim() || null,
            colonia: (document.getElementById('dom_colonia').value || '').trim(),
            municipio: municipio_txt,
            municipio_id: municipio_id,
            estado: estado_txt,
            estado_id: estado_id,
            pais: pais_txt,
            pais_id: pais_id,
            cp: (document.getElementById('dom_cp').value || '').trim(),
            es_principal: !!document.getElementById('dom_principal').checked
        };
        if (!payload.calle || !payload.num_ext || !payload.colonia || !payload.municipio || !payload.estado || !payload.pais || !payload.cp) {
            return swal.fire('Domicilio', 'Todos los campos marcados son obligatorios.', 'warning');
        }
        try {
            const r = await fetch(H_CNT + '?action=upsert_domicilio', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            const j = await r.json();
            if (!j.ok) return swal.fire('Domicilio', j.message || 'No fue posible guardar.', 'error');
            await listarDomicilios();
            swal.fire('Domicilio', 'Guardado.', 'success');
        } catch (e) { swal.fire('Domicilio', 'Error: ' + e, 'error'); }
    }
    async function listarDomicilios() {
        if (!CONTACTO_ID) return;
        const r = await fetch(H_CNT + `?action=listar_domicilios&contacto_id=${CONTACTO_ID}&page=1&pageSize=20&solo_activos=0`);
        const j = await r.json();
        if (!j.ok) return;
        const tb = document.querySelector('#tblDomicilios tbody'); tb.innerHTML = '';
        DOMICILIOS_DATA = j.data || [];
        DOMICILIOS_DATA.forEach(d => tb.insertAdjacentHTML('beforeend', filaDomHTML(d)));
    }
    async function setPrincipalDom(id) {
        const r = await fetch(H_CNT + '?action=set_principal_domicilio', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id }) });
        const j = await r.json();
        if (!j.ok) return swal.fire('Domicilio', j.message || 'No fue posible.', 'error');
        await listarDomicilios();
    }
    async function toggleDom(id, activo) {
        const r = await fetch(H_CNT + '?action=toggle_domicilio', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id, activo: !!activo }) });
        const j = await r.json();
        if (!j.ok) return swal.fire('Domicilio', j.message || 'No fue posible cambiar estado.', 'error');
        await listarDomicilios();
    }

    // =================== PLD ===================
    async function aplicarPLD() {

        if (!SOL_ID) {
            return swal.fire('PLD', 'Primero guarda la operación.', 'warning');
        }

        const cliId = parseInt(document.getElementById('cli_id')?.value || '0', 10);
        if (cliId <= 0) {
            return swal.fire('PLD', 'Debes guardar la identidad del cliente antes de calcular el PLD.', 'warning');
        }

        // =========================
        // Factores (13+)
        // =========================
        const pfId = int(document.getElementById('producto_financiero_id')?.value, 0);
        const canalId = int(document.getElementById('canal_pago_id')?.value, 0);
        const destId = int(document.getElementById('destino_recursos_id')?.value, 0);
        const medioId = int(document.getElementById('medio_contacto_id')?.value, 0);
        const monedaId = int(document.getElementById('moneda_id')?.value, 0);
        const origenOpId = int(document.getElementById('origen_recursos_id')?.value, 0); // operación

        // Identidad
        const ocupacionId = int(document.getElementById('cli_ocupacion')?.value || document.getElementById('ocupacion_id')?.value, 0);
        const actividadId = int(document.getElementById('cli_actividad_economica')?.value, 0);
        const origenCliId = int(document.getElementById('cli_origen_recursos')?.value, 0); // si existe como factor separado
        const nacionalidadId = int(document.getElementById('cli_nacionalidad')?.value, 0);

        const paisNacId = int(document.getElementById('cli_pais_nacimiento')?.value, 0);
        const edoNacId = int(document.getElementById('cli_entidad_nacimiento')?.value, 0); // solo México

        // Domicilios activos (ya guardados): se consideran TODOS los activos, no el formulario "en captura"
        let domiciliosActivos = [];
        let domiciliosPayload = [];
        let paisDomId = 0, edoDomId = 0, munDomId = 0;

        try {
            if (!CONTACTO_ID) {
                return swal.fire('PLD', 'Primero crea/obtén el contacto para calcular el PLD.', 'warning');
            }

            const rDom = await fetch(H_CNT + `?action=listar_domicilios&contacto_id=${CONTACTO_ID}&page=1&pageSize=200&solo_activos=1`);
            const jDom = await rDom.json();

            if (!jDom.ok) {
                return swal.fire('PLD', jDom.message || 'No fue posible obtener los domicilios activos.', 'error');
            }

            domiciliosActivos = (jDom.data || []).filter(d => int(d.activo, 1) === 1);

            if (domiciliosActivos.length === 0) {
                return swal.fire('PLD', 'Debes tener al menos un domicilio ACTIVO para calcular/recalcular el PLD.', 'warning');
            }

            domiciliosPayload = domiciliosActivos.map(d => ({
                pais_id: int(d.pais_id || d.paisId || d.pais_domicilio_id || 0, 0),
                estado_id: int(d.estado_id || d.estadoId || d.estado_domicilio_id || 0, 0),
                municipio_id: int(d.municipio_id || d.municipioId || d.municipio_domicilio_id || 0, 0),
                pais: (d.pais || '').toString().trim(),
                estado: (d.estado || '').toString().trim(),
                municipio: (d.municipio || '').toString().trim(),
                activo: int(d.activo, 1)
            }));

            // Compatibilidad con el handler (campos single), usando el primer activo
            paisDomId = int(domiciliosPayload[0].pais_id, 0);
            edoDomId = int(domiciliosPayload[0].estado_id, 0);
            munDomId = int(domiciliosPayload[0].municipio_id, 0);

        } catch (e) {
            return swal.fire('PLD', 'Error obteniendo domicilios activos: ' + e, 'error');
        }

        // Crédito PLD (si existe en el ASPX)
        const creditoPldId = int(document.getElementById('credito_pld_id')?.value, 0);

        // =========================
        // Validaciones de negocio
        // =========================
        if (pfId <= 0) return swal.fire('PLD', 'Selecciona Producto financiero.', 'warning');
        if (canalId <= 0) return swal.fire('PLD', 'Selecciona Canal de pago.', 'warning');
        if (destId <= 0) return swal.fire('PLD', 'Selecciona Destino de recursos.', 'warning');
        if (medioId <= 0) return swal.fire('PLD', 'Selecciona Medio de contacto.', 'warning');
        if (monedaId <= 0) return swal.fire('PLD', 'Selecciona Moneda/Divisa.', 'warning');
        if (origenOpId <= 0) return swal.fire('PLD', 'Selecciona Origen de recursos (operación).', 'warning');

        if (nacionalidadId <= 0) return swal.fire('PLD', 'Selecciona Nacionalidad.', 'warning');
        if (paisNacId <= 0) return swal.fire('PLD', 'Selecciona País de nacimiento.', 'warning');
        if (actividadId <= 0) return swal.fire('PLD', 'Selecciona Actividad económica.', 'warning');

        // Ocupación: solo validamos si existe el select en el DOM
        if (document.getElementById('cli_ocupacion') || document.getElementById('ocupacion_id')) {
            if (ocupacionId <= 0) return swal.fire('PLD', 'Selecciona Ocupación.', 'warning');
        }

        // Crédito PLD: solo validamos si existe el select en el DOM
        if (document.getElementById('credito_pld_id')) {
            if (creditoPldId <= 0) return swal.fire('PLD', 'Selecciona Crédito PLD.', 'warning');
        }

        const nacEsMexico = isMexico('cli_pais_nacimiento');
        if (nacEsMexico && edoNacId <= 0) {
            return swal.fire('PLD', 'Selecciona Estado de nacimiento (México).', 'warning');
        }

        // =========================
        // Validación domicilios activos (regla México vs extranjero)
        // =========================
        const normTxt = (s) => (s || '').toString().toUpperCase()
            .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
            .replace(/\s+/g, ' ').trim();

        for (const d of domiciliosPayload) {
            if ((int(d.pais_id, 0) <= 0) && !d.pais) {
                return swal.fire('PLD', 'Cada domicilio ACTIVO debe tener País capturado.', 'warning');
            }

            const paisNorm = normTxt(d.pais);

            if (paisNorm === 'MEXICO') {
                const edoOk = (int(d.estado_id, 0) > 0) || (d.estado && !normTxt(d.estado).includes('NO APLICA'));
                const munOk = (int(d.municipio_id, 0) > 0) || (d.municipio && !normTxt(d.municipio).includes('NO APLICA'));

                if (!edoOk) return swal.fire('PLD', 'En domicilios de México, el Estado es obligatorio.', 'warning');
                if (!munOk) return swal.fire('PLD', 'En domicilios de México, el Municipio es obligatorio.', 'warning');
            }
        }

        // =========================
        // Payload (POST x-www-form-urlencoded)
        // =========================
        const p = new URLSearchParams();
        p.append('action', 'calcular');
        p.append('solicitud_id', SOL_ID);

        // Operación
        p.append('producto_financiero_id', pfId);
        p.append('canal_pago_id', canalId);
        p.append('destino_recursos_id', destId);
        p.append('medio_contacto_id', medioId);
        p.append('moneda_id', monedaId);
        p.append('origen_recursos_id', origenOpId);

        // Identidad
        p.append('ocupacion_id', ocupacionId);
        p.append('actividad_economica_id', actividadId);
        p.append('nacionalidad_id', nacionalidadId);

        // Si existe como factor separado
        p.append('origen_recursos_cliente_id', origenCliId || 0);

        // Nacimiento (país siempre; estado sólo si México)
        p.append('pais_id', paisNacId);
        p.append('estado_nacimiento_id', nacEsMexico ? edoNacId : 0);

        // Domicilio (compatibilidad + lista completa)
        const domEsMexico = normTxt((domiciliosPayload[0] || {}).pais) === 'MEXICO';
        p.append('pais_domicilio_id', paisDomId);
        p.append('estado_domicilio_id', domEsMexico ? edoDomId : 0);
        p.append('municipio_domicilio_id', domEsMexico ? munDomId : 0);

        // Lista completa de domicilios activos (para sumar todos)
        p.append('domicilios', JSON.stringify(domiciliosPayload));

        // Crédito PLD (si aplica)
        p.append('credito_pld_id', creditoPldId || 0);

        try {
            const r = await fetch('/handlers/calcular_pld.ashx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                body: p.toString()
            });

            const j = await r.json();
            if (!j.ok) {
                return swal.fire('PLD', j.message || 'No fue posible calcular el PLD.', 'error');
            }

            // ===== NUEVO: pintar tipo de riesgo (devuelto por el handler calcular_pld.ashx) =====
            document.getElementById('lblTipoRiesgo').textContent = (j.tipo_riesgo || '—').toString();

            // =========================
            // Pintar resultados (SIN TOCAR promedio/suma como ya estaban)
            // =========================
            const detalle = Array.isArray(j.data) ? j.data : (Array.isArray(j.factores) ? j.factores : []);

            document.getElementById('lblFactores').textContent = detalle.length;
            document.getElementById('lblSumaNivel').textContent = Number(j.suma_nivel ?? 0).toFixed(2);
            document.getElementById('lblPromNivel').textContent = Number(j.promedio_nivel ?? 0).toFixed(4);

            // (mantener KPIs superiores sincronizados con el recálculo)
            document.getElementById('lblPuntaje').textContent = Number(j.suma_nivel ?? 0).toFixed(4);
            document.getElementById('lblNivelPLD').textContent = Number(j.promedio_nivel ?? 0).toFixed(2);
            document.getElementById('lblMetodoPLD').textContent = (j.metodologia || '').toString();
            document.getElementById('lblFechaPLD').textContent = new Date().toLocaleString();

            const promedio = Number(j.promedio_nivel ?? 0);

            const tb = document.querySelector('#tblPLDDetalle tbody');
            tb.innerHTML = '';

            detalle.forEach(f => {
                // Corrección puntual: en recálculo a veces el backend manda "origen" en vez de "origen_tipo".
                const origenVal = (f.origen_tipo !== undefined && f.origen_tipo !== null && ('' + f.origen_tipo).trim() !== '')
                    ? f.origen_tipo
                    : f.origen;

                tb.insertAdjacentHTML('beforeend', `
                <tr>
                  <td>${origenVal ?? ''}</td>
                  <td>${f.referencia_id ?? ''}</td>
                  <td>${f.impacto ?? ''}</td>
                  <td>${f.probabilidad ?? ''}</td>
                  <td>${Number(f.nivel || 0).toFixed(2)}</td>
                  <td>${Number(f.contribucion ?? (promedio > 0 ? (Number(f.nivel || 0) / promedio) : 0)).toFixed(4)}</td>
                </tr>
            `);
            });

            swal.fire('PLD', 'Cálculo aplicado correctamente.', 'success');

        } catch (e) {
            swal.fire('PLD', 'Error: ' + e, 'error');
        }
    }



    async function cargarPLDDetalle() {
        if (!SOL_ID) return;
        try {
            const r1 = await fetch(H_PLD + `?action=listar&solicitud_id=${SOL_ID}&page=1&pageSize=50`);
            const j1 = await r1.json();
            const tb = document.querySelector('#tblPLDDetalle tbody'); tb.innerHTML = '';
            if (j1.ok) {
                (j1.data || []).forEach(x => {
                    const origen = x.origen_tipo || '';
                    const ref = x.referencia_id || '';
                    tb.insertAdjacentHTML('beforeend', `<tr>
                    <td>${origen}</td>
                    <td>${ref}</td>
                    <td>${x.impacto}</td>
                    <td>${x.probabilidad}</td>
                    <td>${Number(x.nivel).toFixed(2)}</td>
                    <td>${Number(x.contribucion || 0).toFixed(4)}</td>
                </tr>`);
                });
            }
            const r2 = await fetch(H_PLD + `?action=resumen&solicitud_id=${SOL_ID}`);
            const j2 = await r2.json();
            if (j2.ok) {
                document.getElementById('lblFactores').textContent = j2.factores ?? 0;
                document.getElementById('lblSumaNivel').textContent = Number(j2.suma_nivel || 0).toFixed(2);
                document.getElementById('lblPromNivel').textContent = Number(j2.promedio_nivel || 0).toFixed(4);
            }
        } catch (e) { console.warn(e); }
    }

    function limpiarAmortizacionUI() {
        AMORT_DATA = null;
        document.getElementById('lblComisionAperturaDia0').textContent = '$0.00';
        document.getElementById('lblIvaComisionAperturaDia0').textContent = '$0.00';
        document.getElementById('lblTotalCargosDia0').textContent = '$0.00';
        document.getElementById('thPagoPeriodo').textContent = 'Pago Mensual';
        document.querySelector('#tblAmortizacionCondusef tbody').innerHTML = '';
    }

    function formatCsvCell(value) {
        const v = (value == null ? '' : value.toString());
        if (v.includes(',') || v.includes('"') || v.includes('\n')) {
            return `"${v.replace(/"/g, '""')}"`;
        }
        return v;
    }

    function toFixed2(value) {
        return Number(value ?? 0).toFixed(2);
    }

    async function cargarAmortizacionCondusef(showErrors = true) {
        if (productoSeleccionadoEsRevolvente() || SOL_DATA?.es_revolvente) {
            limpiarAmortizacionUI();
            if (showErrors) {
                swal.fire('Amortización', 'La tabla de amortización fija CONDUSEF no aplica a créditos revolventes.', 'info');
            }
            return;
        }

        if (!SOL_ID) {
            limpiarAmortizacionUI();
            return;
        }

        try {
            const r = await fetch(`${H_SOL}?action=amortizacion_condusef&solicitud_id=${encodeURIComponent(SOL_ID)}`);
            const j = await r.json();
            if (!j.ok) {
                limpiarAmortizacionUI();
                if (showErrors) swal.fire('Amortización', j.message || 'No fue posible generar la tabla.', 'warning');
                return;
            }

            AMORT_DATA = j.data || null;
            const ci = AMORT_DATA?.cargos_iniciales || {};
            document.getElementById('lblComisionAperturaDia0').textContent = money(ci.comision_apertura_monto || 0);
            document.getElementById('lblIvaComisionAperturaDia0').textContent = money(ci.iva_comision_apertura || 0);
            document.getElementById('lblTotalCargosDia0').textContent = money(ci.total_cargos_iniciales || 0);
            document.getElementById('thPagoPeriodo').textContent = (AMORT_DATA?.pago_header || 'Pago Mensual').toString();

            const tb = document.querySelector('#tblAmortizacionCondusef tbody');
            tb.innerHTML = '';
            const rows = AMORT_DATA?.rows || [];
            rows.forEach(x => {
                tb.insertAdjacentHTML('beforeend', `<tr>
                    <td>${x.numero_pago ?? ''}</td>
                    <td>${resolveFechaAmortizacion(x)}</td>
                    <td class="text-end">${toFixed2(x.saldo_inicial)}</td>
                    <td class="text-end">${toFixed2(x.capital_interes)}</td>
                    <td class="text-end">${toFixed2(x.capital)}</td>
                    <td class="text-end">${toFixed2(x.interes)}</td>
                    <td class="text-end">${toFixed2(x.iva_interes)}</td>
                    <td class="text-end">${toFixed2(x.pago_periodico)}</td>
                    <td class="text-end">${toFixed2(x.saldo_pendiente)}</td>
                </tr>`);
            });
        } catch (e) {
            limpiarAmortizacionUI();
            if (showErrors) swal.fire('Amortización', 'Error: ' + e, 'error');
        }
    }

    function pickPrimaryItem(items, principalKey = 'es_principal') {
        if (!Array.isArray(items) || !items.length) return null;
        return items.find(x => !!x?.[principalKey]) || items.find(x => !!x?.activo) || items[0];
    }

    function buildClienteNombrePdf() {
        const partes = [
            document.getElementById('cli_primer_nombre')?.value || CLIENTE_DATA?.primer_nombre,
            document.getElementById('cli_segundo_nombre')?.value || CLIENTE_DATA?.segundo_nombre,
            document.getElementById('cli_ap_paterno')?.value || CLIENTE_DATA?.apellido_paterno || CLIENTE_DATA?.ap_paterno,
            document.getElementById('cli_ap_materno')?.value || CLIENTE_DATA?.apellido_materno || CLIENTE_DATA?.ap_materno
        ].map(x => cleanText(x, '')).filter(Boolean);

        if (partes.length) return partes.join(' ');
        return cleanText(CLIENTE_DATA?.razon_social, 'Cliente no especificado');
    }

    function buildDomicilioPrincipalPdf() {
        const d = pickPrimaryItem(DOMICILIOS_DATA);
        if (!d) return 'Sin domicilio registrado';

        const calle = [cleanText(d.calle, ''), cleanText(d.num_ext, ''), cleanText(d.num_int, '')].filter(Boolean).join(' ');
        const ubicacion = [cleanText(d.colonia, ''), cleanText(d.municipio, ''), cleanText(d.estado, ''), cleanText(d.cp, '')].filter(Boolean).join(', ');
        return cleanText([calle, ubicacion].filter(Boolean).join(', '), 'Sin domicilio registrado');
    }

    function getResumenAmortizacionPdf() {
        const rows = (AMORT_DATA?.rows || []).filter(x => int(x.numero_pago, 0) > 0);
        const totals = rows.reduce((acc, row) => {
            acc.capital += num(row.capital, 0);
            acc.interes += num(row.interes, 0);
            acc.ivaInteres += num(row.iva_interes, 0);
            acc.pagos += num(row.pago_periodico, 0);
            return acc;
        }, { capital: 0, interes: 0, ivaInteres: 0, pagos: 0 });

        return {
            rows,
            pagoPeriodo: rows.length ? num(rows[0].pago_periodico, 0) : 0,
            totalCapital: totals.capital,
            totalInteres: totals.interes,
            totalIvaInteres: totals.ivaInteres,
            totalPagos: totals.pagos
        };
    }

    function buildPdfMeta(summary) {
        const telefono = pickPrimaryItem(TELEFONOS_DATA);
        const email = pickPrimaryItem(EMAILS_DATA);
        const montoSolicitado = num(document.getElementById('monto_solicitado')?.value || SOL_DATA?.monto_solicitado, 0);
        const tasaEntrada = num(document.getElementById('tasa_entrada')?.value || AMORT_DATA?.tasa_anual_pct, 0);

        return {
            empresa: 'CRENOR',
            solicitudId: SOL_ID,
            estatus: cleanText(document.getElementById('lblEstatus')?.textContent, 'BORRADOR'),
            fechaEmision: fmtDateLong(SOL_DATA?.fecha_creacion || new Date()),
            cliente: buildClienteNombrePdf(),
            producto: cleanText(selectedText('producto_financiero_id')),
            periodicidad: cleanText(selectedText('producto_financiero_periodo_id') || AMORT_DATA?.tipo_periodo),
            canalPago: cleanText(selectedText('canal_pago_id')),
            destino: cleanText(selectedText('destino_recursos_id')),
            origen: cleanText(selectedText('origen_recursos_id')),
            plazo: int(document.getElementById('plazo')?.value || AMORT_DATA?.plazo, 0),
            tasa: `${tasaEntrada.toFixed(2)}% anual`,
            montoSolicitado: moneyPlain(montoSolicitado),
            pagoHeader: cleanText(AMORT_DATA?.pago_header, 'Pago Mensual'),
            pagoPeriodo: moneyPlain(summary.pagoPeriodo),
            telefono: cleanText(telefono ? [telefono.numero, telefono.extension ? `Ext. ${telefono.extension}` : ''].filter(Boolean).join(' ') : ''),
            email: cleanText(email?.email),
            domicilio: buildDomicilioPrincipalPdf(),
            observaciones: cleanText(document.getElementById('observaciones')?.value, '')
        };
    }

    function loadImageAsDataUrl(src) {
        return new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = () => {
                try {
                    const canvas = document.createElement('canvas');
                    canvas.width = img.naturalWidth;
                    canvas.height = img.naturalHeight;
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(img, 0, 0);
                    resolve(canvas.toDataURL('image/png'));
                } catch (_) {
                    resolve(null);
                }
            };
            img.onerror = () => resolve(null);
            img.src = src;
        });
    }

    function drawPdfHeader(doc, meta, logoDataUrl, isFirstPage) {
        const pageWidth = doc.internal.pageSize.getWidth();
        const colors = PDF_THEME;

        if (isFirstPage) {
            doc.setFillColor(...colors.brandDark);
            doc.rect(0, 0, pageWidth, 128, 'F');
            doc.setFillColor(...colors.brand);
            doc.rect(0, 0, pageWidth, 10, 'F');

            if (logoDataUrl) {
                doc.addImage(logoDataUrl, 'PNG', pageWidth - 150, 24, 94, 76);
            }

            doc.setTextColor(255, 255, 255);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(14);
            doc.text(meta.empresa, 36, 36);
            doc.setFontSize(25);
            doc.text('Tabla de amortización CONDUSEF', 36, 66);
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(11);
            doc.text(`Solicitud #${meta.solicitudId}  |  ${meta.estatus}  |  ${meta.fechaEmision}`, 36, 90);
            doc.setFontSize(10);
            doc.text('Propuesta de crédito con identidad CRENOR', 36, 108);

            doc.setFillColor(255, 255, 255);
            doc.roundedRect(36, 142, 250, 72, 12, 12, 'F');
            doc.setTextColor(...colors.muted);
            doc.setFontSize(10);
            doc.text('Monto solicitado', 52, 166);
            doc.setTextColor(...colors.text);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(24);
            doc.text(meta.montoSolicitado, 52, 198);

            doc.setFillColor(...colors.brandSoft);
            doc.setDrawColor(...colors.border);
            doc.roundedRect(pageWidth - 286, 142, 250, 72, 12, 12, 'FD');
            doc.setTextColor(...colors.muted);
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(10);
            doc.text(meta.pagoHeader, pageWidth - 268, 166);
            doc.setTextColor(...colors.text);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(22);
            doc.text(meta.pagoPeriodo, pageWidth - 268, 198);
            doc.setFont('helvetica', 'normal');
            doc.setFontSize(10);
            doc.setTextColor(...colors.muted);
            doc.text(`${meta.periodicidad}  |  Plazo ${meta.plazo}`, pageWidth - 268, 211);
            return;
        }

        doc.setFillColor(...colors.brandDark);
        doc.rect(0, 0, pageWidth, 58, 'F');
        doc.setFillColor(...colors.brand);
        doc.rect(0, 0, pageWidth, 8, 'F');
        if (logoDataUrl) {
            doc.addImage(logoDataUrl, 'PNG', pageWidth - 102, 13, 48, 38);
        }
        doc.setTextColor(255, 255, 255);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(15);
        doc.text(`CRENOR · Solicitud #${meta.solicitudId}`, 36, 28);
        doc.setFont('helvetica', 'normal');
        doc.setFontSize(10);
        doc.text('Tabla de amortización CONDUSEF', 36, 44);
        doc.text(meta.cliente, pageWidth - 116, 34, { align: 'right' });
    }

    async function descargarTablaAmortizacionPDF() {
        if (!SOL_ID) {
            return swal.fire('Amortización', 'Primero guarda la operación.', 'warning');
        }
        if (!AMORT_DATA) {
            await cargarAmortizacionCondusef(false);
        }
        if (!AMORT_DATA) {
            return swal.fire('Amortización', 'No hay datos para descargar.', 'warning');
        }
        if (!window.jspdf?.jsPDF) {
            return swal.fire('Amortización', 'No fue posible inicializar la librería PDF.', 'error');
        }

        const btn = document.getElementById('btnDescargarAmortizacion');
        btn.disabled = true;

        try {
            swal.fire({
                title: 'Generando PDF',
                text: 'Estamos preparando la propuesta de crédito...',
                allowOutsideClick: false,
                didOpen: () => {
                    if (window.Swal?.showLoading) {
                        window.Swal.showLoading();
                    } else if (window.swal?.showLoading) {
                        window.swal.showLoading();
                    }
                }
            });

            const { jsPDF } = window.jspdf;
            const doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4', compress: true });
            if (typeof doc.autoTable !== 'function') {
                throw new Error('La extensión de tablas PDF no está disponible.');
            }

            const logoDataUrl = await loadImageAsDataUrl(PDF_LOGO_URL);
            const summary = getResumenAmortizacionPdf();
            const meta = buildPdfMeta(summary);
            const ci = AMORT_DATA?.cargos_iniciales || {};
            const marginX = 36;
            const pageWidth = doc.internal.pageSize.getWidth();
            const pageHeight = doc.internal.pageSize.getHeight();
            const tableWidth = pageWidth - (marginX * 2);
            const labelCell = { fillColor: PDF_THEME.brandSoft, textColor: PDF_THEME.text, fontStyle: 'bold' };

            let y = 232;

            doc.autoTable({
                startY: y,
                margin: { top: 84, right: marginX, bottom: 38, left: marginX },
                tableWidth,
                theme: 'grid',
                body: [
                    [{ content: 'Cliente', styles: labelCell }, meta.cliente, { content: 'Fecha de emisión', styles: labelCell }, meta.fechaEmision],
                    [{ content: 'Producto', styles: labelCell }, meta.producto, { content: 'Periodicidad', styles: labelCell }, meta.periodicidad],
                    [{ content: 'Canal de pago', styles: labelCell }, meta.canalPago, { content: 'Destino', styles: labelCell }, meta.destino],
                    [{ content: 'Origen de recursos', styles: labelCell }, meta.origen, { content: 'Tasa / Plazo', styles: labelCell }, `${meta.tasa} · ${meta.plazo} pagos`],
                    [{ content: 'Teléfono', styles: labelCell }, meta.telefono, { content: 'Correo', styles: labelCell }, meta.email],
                    [{ content: 'Domicilio', styles: labelCell }, meta.domicilio, { content: 'Estatus', styles: labelCell }, meta.estatus]
                ],
                styles: { font: 'helvetica', fontSize: 9.5, cellPadding: 7, textColor: PDF_THEME.text, lineColor: PDF_THEME.border, lineWidth: 1 },
                columnStyles: {
                    0: { cellWidth: 118 },
                    1: { cellWidth: 268 },
                    2: { cellWidth: 118 },
                    3: { cellWidth: tableWidth - 504 }
                }
            });

            y = doc.lastAutoTable.finalY + 18;
            doc.setTextColor(...PDF_THEME.brandDark);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(12);
            doc.text('Indicadores principales', marginX, y);

            doc.autoTable({
                startY: y + 8,
                margin: { top: 84, right: marginX, bottom: 38, left: marginX },
                tableWidth,
                head: [['Monto solicitado', meta.pagoHeader, 'Total intereses', 'Total a pagar']],
                body: [[
                    meta.montoSolicitado,
                    meta.pagoPeriodo,
                    moneyPlain(summary.totalInteres),
                    moneyPlain(summary.totalPagos + num(ci.total_cargos_iniciales, 0))
                ]],
                theme: 'grid',
                headStyles: { fillColor: PDF_THEME.brand, textColor: [255, 255, 255], fontStyle: 'bold', halign: 'center' },
                bodyStyles: { textColor: PDF_THEME.text, fontStyle: 'bold', halign: 'center', fontSize: 11, minCellHeight: 28 },
                styles: { font: 'helvetica', lineColor: PDF_THEME.border, lineWidth: 1, cellPadding: 8 }
            });

            y = doc.lastAutoTable.finalY + 18;
            doc.setTextColor(...PDF_THEME.brandDark);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(12);
            doc.text('Cargos iniciales (Día 0)', marginX, y);

            doc.autoTable({
                startY: y + 8,
                margin: { top: 84, right: marginX, bottom: 38, left: marginX },
                tableWidth,
                head: [['Comisión por apertura', 'IVA comisión por apertura', 'Total cargos iniciales']],
                body: [[
                    moneyPlain(ci.comision_apertura_monto || 0),
                    moneyPlain(ci.iva_comision_apertura || 0),
                    moneyPlain(ci.total_cargos_iniciales || 0)
                ]],
                theme: 'grid',
                headStyles: { fillColor: PDF_THEME.brandDark, textColor: [255, 255, 255], fontStyle: 'bold', halign: 'center' },
                bodyStyles: { textColor: PDF_THEME.text, halign: 'center', fontSize: 10.5, minCellHeight: 24 },
                styles: { font: 'helvetica', lineColor: PDF_THEME.border, lineWidth: 1, cellPadding: 8 }
            });

            if (meta.observaciones) {
                y = doc.lastAutoTable.finalY + 18;
                doc.setTextColor(...PDF_THEME.brandDark);
                doc.setFont('helvetica', 'bold');
                doc.setFontSize(12);
                doc.text('Observaciones', marginX, y);

                doc.autoTable({
                    startY: y + 8,
                    margin: { top: 84, right: marginX, bottom: 38, left: marginX },
                    tableWidth,
                    theme: 'grid',
                    body: [[meta.observaciones]],
                    styles: { font: 'helvetica', fontSize: 9.5, cellPadding: 8, textColor: PDF_THEME.text, lineColor: PDF_THEME.border, lineWidth: 1 }
                });
            }

            y = doc.lastAutoTable.finalY + 20;
            doc.setTextColor(...PDF_THEME.brandDark);
            doc.setFont('helvetica', 'bold');
            doc.setFontSize(12);
            doc.text('Tabla de amortización', marginX, y);

            doc.autoTable({
                startY: y + 8,
                margin: { top: 84, right: marginX, bottom: 38, left: marginX },
                tableWidth,
                head: [[
                    'Número de pagos',
                    'Fecha',
                    'Saldo inicial',
                    'Capital + Interés',
                    'Capital',
                    'Interés',
                    'IVA de los Intereses',
                    meta.pagoHeader,
                    'Saldo pendiente'
                ]],
                body: (AMORT_DATA.rows || []).map(row => ([
                    row.numero_pago ?? '',
                    resolveFechaAmortizacion(row),
                    moneyPlain(row.saldo_inicial || 0),
                    moneyPlain(row.capital_interes || 0),
                    moneyPlain(row.capital || 0),
                    moneyPlain(row.interes || 0),
                    moneyPlain(row.iva_interes || 0),
                    moneyPlain(row.pago_periodico || 0),
                    moneyPlain(row.saldo_pendiente || 0)
                ])),
                theme: 'grid',
                headStyles: { fillColor: PDF_THEME.brand, textColor: [255, 255, 255], fontSize: 8.5, fontStyle: 'bold', halign: 'center', valign: 'middle' },
                styles: { font: 'helvetica', fontSize: 8.3, cellPadding: 5, textColor: PDF_THEME.text, lineColor: PDF_THEME.border, lineWidth: 1, valign: 'middle' },
                alternateRowStyles: { fillColor: PDF_THEME.highlight },
                columnStyles: {
                    0: { cellWidth: 54, halign: 'center' },
                    1: { cellWidth: 76, halign: 'center' },
                    2: { cellWidth: 90, halign: 'right' },
                    3: { cellWidth: 92, halign: 'right' },
                    4: { cellWidth: 82, halign: 'right' },
                    5: { cellWidth: 74, halign: 'right' },
                    6: { cellWidth: 95, halign: 'right' },
                    7: { cellWidth: 86, halign: 'right' },
                    8: { cellWidth: tableWidth - 649, halign: 'right' }
                },
                didParseCell: (data) => {
                    if (data.section === 'body' && data.row.index === 0) {
                        data.cell.styles.fillColor = PDF_THEME.accent;
                        data.cell.styles.fontStyle = 'bold';
                    }
                }
            });

            const totalPages = doc.getNumberOfPages();
            for (let page = 1; page <= totalPages; page++) {
                doc.setPage(page);
                drawPdfHeader(doc, meta, logoDataUrl, page === 1);
                doc.setDrawColor(...PDF_THEME.border);
                doc.line(marginX, pageHeight - 28, pageWidth - marginX, pageHeight - 28);
                doc.setTextColor(...PDF_THEME.muted);
                doc.setFont('helvetica', 'normal');
                doc.setFontSize(8.5);
                doc.text(`Documento CRENOR generado el ${new Date().toLocaleString('es-MX')}`, marginX, pageHeight - 12);
                doc.text(`Página ${page} de ${totalPages}`, pageWidth - marginX, pageHeight - 12, { align: 'right' });
            }

            swal.close();
            doc.save(`propuesta_credito_solicitud_${SOL_ID}.pdf`);
        } catch (e) {
            swal.close();
            swal.fire('Amortización', 'No fue posible generar el PDF: ' + (e?.message || e), 'error');
        } finally {
            btn.disabled = false;
        }
    }

    async function finalizarSolicitud() {
        if (!SOL_ID) {
            return swal.fire('Finalizar', 'Primero guarda la operación.', 'warning');
        }

        const ok = await swal.fire({
            title: 'Finalizar solicitud',
            text: '¿Deseas finalizar la solicitud?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, finalizar'
        });

        if (!ok.isConfirmed) return;

        try {
            const params = new URLSearchParams();
            params.append('solicitud_id', SOL_ID);
            params.append('estatus', 'FINALIZADA');

            const r = await fetch(H_SOL + '?action=finalizar', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                body: params.toString()
            });

            const j = await r.json();

            if (!j.ok) {
                return swal.fire('Finalizar', j.message || 'No fue posible finalizar.', 'error');
            }

            const paramsAlertas = new URLSearchParams();
            paramsAlertas.append('action', 'generar_desde_solicitud');
            paramsAlertas.append('solicitud_id', SOL_ID);

            const rAlertas = await fetch(H_ALERTAS, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                body: paramsAlertas.toString()
            });

            const jAlertas = await rAlertas.json();

            await cargarSolicitud();

            if (jAlertas && jAlertas.ok) {
                return swal.fire(
                    'Finalizar',
                    'Solicitud finalizada. Alertas generadas: ' + (jAlertas.generadas || 0) + '. Duplicadas omitidas: ' + (jAlertas.omitidas_duplicado || 0) + '.',
                    'success'
                );
            }

            return swal.fire(
                'Finalizar',
                'Solicitud finalizada, pero no se pudieron evaluar alertas: ' + (jAlertas.message || jAlertas.mensaje || 'sin detalle'),
                'warning'
            );

        } catch (e) {
            swal.fire('Finalizar', 'Error: ' + e, 'error');
        }
    }
            </script>
</asp:Content>
