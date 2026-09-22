<%@ Page Title="Catálogo de Producto Financiero" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CatalogoProductoFinanciero.aspx.vb" Inherits="PLD.CatalogoProductoFinanciero" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
  /* Oculta el h4 para que no duplique el encabezado visual */
  .container-fluid.mt-4 > h4.mb-3 { display: none; }

  /* Barra azul de cabecera */
  .pld-headbar{
    background: #0d6efd;
    color: #fff;
    padding: .5rem .75rem;
    border-radius: .375rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
  .pld-headbar-title{
    font-weight: 600;
  }

  /* Layout de DataTables como el screenshot:
     length a la izq, search a la der; info izq, paginación der */
  #tblCatalogo_wrapper .row:first-child > div:first-child { text-align: left; }
  #tblCatalogo_wrapper .row:first-child > div:last-child  { text-align: right; }
  #tblCatalogo_wrapper .dataTables_filter { text-align: right; }
  #tblCatalogo_wrapper .dataTables_filter label { margin-bottom: 0; }
  #tblCatalogo_wrapper .dataTables_info { padding-top: .5rem; }
  #tblCatalogo_wrapper .dataTables_paginate { text-align: right !important; }
</style>


    <div class="container-fluid mt-4">
        <h4 class="mb-3">Catálogo de Producto Financiero</h4>

        <!-- Toolbar -->
        <!-- Headbar azul como en el diseño -->
        <div class="pld-headbar mb-2">
            <div class="pld-headbar-title">Créditos</div>
            <div>
                <button type="button" id="btnCapturar" class="btn btn-success btn-sm">
                    <i class="fas fa-plus"></i> Capturar
                </button>
            </div>
        </div>



        <!-- Tabla -->
        <div class="table-responsive">
            <table id="tblCatalogo" class="table table-sm table-striped table-hover align-middle w-100">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Descripción</th>
                        <th class="text-end">Impacto</th>
                        <th class="text-end">Probabilidad</th>
                        <th class="text-end">Nivel riesgo PLD</th>
                        <th>Estatus</th>
                        <th>Mitigantes</th>
                        <th style="width:120px;">Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>

        <!-- MODAL -->
        <div class="modal fade" id="modalCatalogo" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalTitle">Nuevo producto</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                    </div>

                    <div class="modal-body">
                        <div id="formProducto" autocomplete="off">
                            <input type="hidden" id="modal_producto_id" />

                            <!-- Tabs -->
                            <ul class="nav nav-tabs mb-3" role="tablist">
                                <li class="nav-item" role="presentation">
                                    <button type="button" class="nav-link active" data-bs-toggle="tab" data-bs-target="#tab-identificacion" role="tab">Datos de identificación</button>
                                </li>
                                <li class="nav-item" role="presentation">
                                    <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-detalles" role="tab">Detalles del producto</button>
                                </li>
                                <li class="nav-item" role="presentation">
                                    <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-prelacion" role="tab">Prelación</button>
                                </li>
                                <li class="nav-item" role="presentation">
                                    <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-ajustes" role="tab">Otros ajustes</button>
                                </li>
                                <li class="nav-item" role="presentation">
                                    <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#tab-periodos" role="tab">Periodos</button>
                                </li>
                            </ul>

                            <div class="tab-content">
                                <!-- TAB 1: Identificación -->
                                <div class="tab-pane fade show active" id="tab-identificacion" role="tabpanel">
                                    <div class="border rounded p-3 mb-3">
                                        <h6 class="text-success mb-3">Datos generales</h6>
                                        <div class="row g-3">
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Crédito*</label>
                                                <select id="fld_credito" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Régimen fiscal</label>
                                                <select id="fld_regimen" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Producto* (texto)</label>
                                                <input type="text" id="fld_producto" class="form-control form-control-sm" placeholder="">
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Moneda*</label>
                                                <select id="fld_moneda" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Impacto* (solo números)</label>
                                                <input type="text" id="fld_impacto" class="form-control form-control-sm" inputmode="decimal" placeholder="">
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Probabilidad* (solo números)</label>
                                                <input type="text" id="fld_probabilidad" class="form-control form-control-sm" inputmode="decimal" placeholder="">
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Nivel de riesgo P.L.D*</label>
                                                <input type="text" id="fld_nivel_riesgo" class="form-control form-control-sm" inputmode="decimal" placeholder="">
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- TAB 2: Detalles del producto (32 campos) -->
                                <div class="tab-pane fade" id="tab-detalles" role="tabpanel">
                                    <div class="border rounded p-3 mb-3">
                                        <h6 class="text-success mb-3">Datos generales del producto</h6>
                                        <div class="row g-2 align-items-center">
                                            <!-- 1 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" id="fld_tasa_anual_prefijo" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Tasa interés anual*</label>
                                                <input type="text" id="fld_tasa_anual" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 2 -->
                                            <div class="col-12 col-md-8">
                                                <label class="form-label">Tasa interés anual detallada*</label>
                                                <textarea id="fld_tasa_anual_det" class="form-control form-control-sm" rows="2"></textarea>
                                            </div>
                                            <!-- 3 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Tasa interés mensual*</label>
                                                <input type="text" id="fld_tasa_mensual" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 4 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">IVA*</label>
                                                <input type="text" id="fld_iva" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 5 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">IVA Comisión*</label>
                                                <input type="text" id="fld_iva_comision" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 6 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">$ </label>
                                                <input type="text" class="form-control form-control-sm" value="$" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Capital mínimo*</label>
                                                <input type="text" id="fld_capital_min" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 7 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">$ </label>
                                                <input type="text" class="form-control form-control-sm" value="$" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Capital máximo*</label>
                                                <input type="text" id="fld_capital_max" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 8 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">#</label>
                                                <input type="text" class="form-control form-control-sm" value="#" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Plazo mínimo*</label>
                                                <input type="text" id="fld_plazo_min" class="form-control form-control-sm" inputmode="numeric" value="">
                                            </div>
                                            <!-- 9 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">#</label>
                                                <input type="text" class="form-control form-control-sm" value="#" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Plazo máximo*</label>
                                                <input type="text" id="fld_plazo_max" class="form-control form-control-sm" inputmode="numeric" value="">
                                            </div>

                                            <!-- 10 Vencimiento -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Vencimiento*</label>
                                                <select id="fld_vencimiento" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="MENSUAL">Mensual</option>
                                                    <option value="QUINCENAL">Quincenal</option>
                                                    <option value="CATORCENAL">Catorcenal</option>
                                                    <option value="SEMANAL">Semanal</option>
                                                    <option value="DIARIO">Diario</option>
                                                </select>
                                            </div>

                                            <!-- 11 Cálculo -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Cálculo*</label>
                                                <select id="fld_calculo" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SALDOS_INSOLUTOS">Saldos insolutos</option>
                                                    <option value="SALDOS_SOLUTOS">Saldos solutos</option>
                                                    <option value="SALDOS_INICIALES">Saldos iniciales</option>
                                                    <option value="ADELANTO_NOMINA">Adelanto de nómina</option>
                                                </select>
                                            </div>

                                            <!-- 12 Factor moratorio -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Factor moratorio*</label>
                                                <select id="fld_factor_moratorio" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 13 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Tasa moratoria*</label>
                                                <input type="text" id="fld_tasa_moratoria" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>

                                            <!-- 14 -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">IVA moratoria*</label>
                                                <input type="text" id="fld_iva_moratoria" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>

                                            <!-- 15 -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Días de gracia</label>
                                                <input type="text" id="fld_dias_gracia" class="form-control form-control-sm" inputmode="numeric" value="">
                                            </div>

                        <!-- 16 Comisión por apertura -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Comisión por apertura*</label>
                                                <input type="text" id="fld_comision_apertura" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <!-- 17 -->
                                            <div class="col-12 col-md-8">
                                                <label class="form-label">Comisión por apertura detallada*</label>
                                                <textarea id="fld_comision_apertura_det" class="form-control form-control-sm" rows="2"></textarea>
                                            </div>

                                            <!-- 18 Amortizar comisión por apertura -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Amortizar comisión por apertura</label>
                                                <select id="fld_amortiza_comision_apertura" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 19 Comisión por gestión -->
                                            <div class="col-12 col-md-1">
                                                <label class="form-label">% </label>
                                                <input type="text" class="form-control form-control-sm" value="%" disabled>
                                            </div>
                                            <div class="col-12 col-md-3">
                                                <label class="form-label">Comisión por gestión</label>
                                                <input type="text" id="fld_comision_gestion" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>

                                            <!-- 20 Amortizar comisión por gestión -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Amortizar comisión por gestión</label>
                                                <select id="fld_amortiza_comision_gestion" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 21 Monto de seguro -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Monto de seguro</label>
                                                <select id="fld_monto_seguro" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 22 ¿Usar redondeo de centavos? -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">¿Usar redondeo de centavos?</label>
                                                <select id="fld_redondeo_centavos" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>

                                            <!-- 23 Base cálculo -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Base cálculo*</label>
                                                <select id="fld_base_calculo" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="360">360</option>
                                                    <option value="365">365</option>
                                                </select>
                                            </div>

                                            <!-- 24 Tipo periodo (DETALLE: CK_detalles_tipo_periodo = COMPLETO/FIJO) -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Tipo periodo (detalle)*</label>
                                                <select id="fld_tipo_periodo" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="COMPLETO">COMPLETO</option>
                                                    <option value="FIJO">FIJO</option>
                                                </select>
                                            </div>

                                            <!-- 25 Cálculo fecha exigible -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Cálculo fecha exigible*</label>
                                                <select id="fld_calc_fecha_exigible" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="IDEM">IDEM</option>
                                                    <option value="SIGUIENTE_DIA_HABIL">Siguiente día hábil</option>
                                                    <option value="ANTERIOR_DIA_HABIL">Anterior día hábil</option>
                                                </select>
                                            </div>

                                            <!-- 26 Centavos para redondeo -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Centavos para redondeo*</label>
                                                <input type="text" id="fld_centavos_redondeo" class="form-control form-control-sm" inputmode="numeric" value="">
                                            </div>

                                            <!-- 27 Redondear pago fijo -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Redondear pago fijo?</label>
                                                <select id="fld_redondear_pago_fijo" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 28 Aplicación de tasa -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Aplicación de tasa*</label>
                                                <select id="fld_aplicacion_tasa" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SIN_EXTRAS">Sin extras</option>
                                                    <option value="CON_TASA_LIBOR">Con tasa LIBOR</option>
                                                    <option value="CON_TASA_TIEE">Con tasa TIIE</option>
                                                </select>
                                            </div>

                                            <!-- 29 IVA sobre el total -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">IVA sobre el total</label>
                                                <select id="fld_iva_sobre_total" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 30 Modificador monto crédito -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Modificador monto crédito*</label>
                                                <select id="fld_modificador_monto_credito" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                    <option value="SI">Sí</option>
                                                    <option value="NO">No</option>
                                                </select>
                                            </div>

                                            <!-- 31/32: placeholders de compatibilidad (si tu back ya los espera) -->
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Tipo periodo (compatibilidad)</label>
                                                <input type="text" id="fld_tipo_periodo_txt" class="form-control form-control-sm" placeholder="(opcional)">
                                            </div>
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Notas adicionales</label>
                                                <input type="text" id="fld_notas_adicionales" class="form-control form-control-sm" placeholder="(opcional)">
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- TAB 3: Prelación -->
                                <div class="tab-pane fade" id="tab-prelacion" role="tabpanel">
                                    <div class="border rounded p-3 mb-3">
                                        <h6 class="text-success mb-3">Prelación</h6>
                                        <div class="row g-2">
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #1</label>
                                                <select id="fld_prelacion_1" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #2</label>
                                                <select id="fld_prelacion_2" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #3</label>
                                                <select id="fld_prelacion_3" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #4</label>
                                                <select id="fld_prelacion_4" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #5</label>
                                                <select id="fld_prelacion_5" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #6</label>
                                                <select id="fld_prelacion_6" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">Prelación #7</label>
                                                <select id="fld_prelacion_7" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                        </div>
                                    </div>
                                </div>


                                <!-- TAB 4: Otros ajustes -->
                                <div class="tab-pane fade" id="tab-ajustes" role="tabpanel">
                                    <div class="border rounded p-3">
                                        <h6 class="text-success mb-3">Ajustes adicionales</h6>
                                        <div class="row g-2">
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">¿Aplica cargos administrativos?</label>
                                                <select id="fld_aplica_cargos" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">¿Amortizar cargos administrativos?</label>
                                                <select id="fld_amortiza_cargos" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>

                                            <div class="col-12 col-md-6">
                                                <label class="form-label">¿Aplica cargo de multa por cobranza?</label>
                                                <select id="fld_cargo_multa" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>

                                            <!-- Comisión admin -->
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">¿Aplica comisión por administración?</label>
                                                <select id="fld_aplica_comision_admin" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">% IVA de comisión por administración</label>
                                                <input type="text" id="fld_iva_com_admin" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <div class="col-12">
                                                <label class="form-label">Comisión por administración detallada</label>
                                                <textarea id="fld_com_admin_det" class="form-control form-control-sm" rows="2"></textarea>
                                            </div>

                                            <!-- Comisión investigación -->
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">¿Aplica comisión por investigación?</label>
                                                <select id="fld_aplica_invest" class="form-select form-select-sm">
                                                    <option value="">No</option>
                                                    <option value="SI">Sí</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-6">
                                                <label class="form-label">% IVA de comisión por investigación</label>
                                                <input type="text" id="fld_iva_com_inv" class="form-control form-control-sm" inputmode="decimal" value="0">
                                            </div>
                                            <div class="col-12">
                                                <label class="form-label">Comisión por investigación detallada</label>
                                                <textarea id="fld_com_inv_det" class="form-control form-control-sm" rows="2"></textarea>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!-- TAB 5: Periodos / periodicidades -->
                                <div class="tab-pane fade" id="tab-periodos" role="tabpanel">
                                    <div class="border rounded p-3 mb-3">
                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                            <h6 class="text-success mb-0">Periodicidades del producto</h6>
                                            <div class="small text-muted">Captura al menos 1 periodicidad para poder guardar el producto.</div>
                                        </div>

                                        <input type="hidden" id="mp_periodos_id" />

                                        <div class="row g-2 align-items-end">
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Tipo periodo*</label>
                                                <select id="mp_periodos_tipo_periodo" class="form-select form-select-sm">
                                                    <option value="">--------</option>
                                                </select>
                                            </div>
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Plazo* (entero)</label>
                                                <input type="text" id="mp_periodos_plazo" class="form-control form-control-sm" inputmode="numeric" placeholder="Ej. 12">
                                            </div>
                                            <div class="col-12 col-md-4">
                                                <label class="form-label">Tasa interés* (decimal)</label>
                                                <input type="text" id="mp_periodos_tasa_interes" class="form-control form-control-sm" inputmode="decimal" placeholder="Ej. 0.21">
                                            </div>

                                            <div class="col-12 d-flex gap-2 mt-2">
                                                <button type="button" id="btnMpGuardarPeriodo" class="btn btn-success btn-sm">
                                                    <i class="fas fa-plus"></i> Agregar / actualizar
                                                </button>
                                                <button type="button" id="btnMpCancelarPeriodo" class="btn btn-outline-secondary btn-sm">
                                                    <i class="fas fa-undo"></i> Cancelar
                                                </button>
                                            </div>
                                        </div>

                                        <div class="table-responsive mt-3">
                                            <table id="mp_tblPeriodos" class="table table-sm table-striped table-hover align-middle w-100">
                                                <thead class="table-light">
                                                    <tr>
                                                        <th>Tipo periodo</th>
                                                        <th class="text-end">Plazo</th>
                                                        <th class="text-end">Tasa interés</th>
                                                        <th style="width:140px;">Acciones</th>
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

                    <div class="modal-footer">
                        <button type="button" id="btnGuardarModal" class="btn btn-success">
                            <i class="fas fa-save"></i> Guardar información
                        </button>
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times"></i> Cancelar
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /MODAL -->
    </div>


        <!-- MODAL PERIODICIDADES (DETALLE) -->
        <div class="modal fade" id="modalPeriodos" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            Periodicidades: <span id="periodos_producto_titulo" class="text-primary"></span>
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" id="periodos_producto_id" />
                        <input type="hidden" id="periodos_id" />

                        <div class="border rounded p-3 mb-3">
                            <div class="row g-2 align-items-end">
                                <div class="col-12 col-md-4">
                                    <label class="form-label">Tipo periodo*</label>
                                    <select id="periodos_tipo_periodo" class="form-select form-select-sm">
                                        <option value="">--------</option>
                                    </select>
                                </div>
                                <div class="col-12 col-md-4">
                                    <label class="form-label">Plazo* (entero)</label>
                                    <input type="text" id="periodos_plazo" class="form-control form-control-sm" inputmode="numeric" placeholder="Ej. 12">
                                </div>
                                <div class="col-12 col-md-4">
                                    <label class="form-label">Tasa interés* (decimal)</label>
                                    <input type="text" id="periodos_tasa_interes" class="form-control form-control-sm" inputmode="decimal" placeholder="Ej. 0.21">
                                </div>

                                <div class="col-12 d-flex gap-2 mt-2">
                                    <button type="button" id="btnGuardarPeriodo" class="btn btn-success btn-sm">
                                        <i class="fas fa-save"></i> Guardar periodicidad
                                    </button>
                                    <button type="button" id="btnCancelarEdicionPeriodo" class="btn btn-outline-secondary btn-sm">
                                        <i class="fas fa-undo"></i> Cancelar edición
                                    </button>
                                </div>
                            </div>
                            <div class="small text-muted mt-2">
                                Nota: La periodicidad es única por producto (p. ej. Semanal/Quincenal/Mensual) y se almacena en la tabla de periodicidades del producto.
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table id="tblPeriodos" class="table table-sm table-striped table-hover align-middle w-100">
                                <thead class="table-light">
                                    <tr>
                                        <th>Tipo periodo</th>
                                        <th class="text-end">Plazo</th>
                                        <th class="text-end">Tasa interés</th>
                                        <th>Estatus</th>
                                        <th style="width:160px;">Acciones</th>
                                    </tr>
                                </thead>
                                <tbody></tbody>
                            </table>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times"></i> Cerrar
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /MODAL PERIODICIDADES -->


    <!-- SCRIPT -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // =======================
            // CONFIGURACIÓN DE RUTAS
            // =======================
            const HANDLER_URL = "/handlers/producto_financiero_handler.ashx";      // list / get / save / update / delete
            const ALLOWED_CALCULO_UI = ["SALDOS_INSOLUTOS", "SALDOS_SOLUTOS", "SALDOS_INICIALES", "ADELANTO_NOMINA"];
            // const ALLOWED_CALCULO_DB eliminado: se mapea UI->DB (espacios) para cumplir CK_detalles_calculo.
            const HANDLER_CREDITOS = "/handlers/catalogo_creditos_pld.ashx";            // ?op=consultar
            const HANDLER_MONEDAS = "/handlers/handler_catalogo_moneda_divisa.ashx";   // ?op=consultar
            const HANDLER_PERIODOS = "/handlers/producto_financiero_handler.ashx"; // periodos_* (consolidado en el handler principal)


            // =======================
            // HELPERS (fetch + alerts)
            // =======================
            function swalFire(title, text, icon) {
                try {
                    if (window.Swal && typeof Swal.fire === 'function') { Swal.fire(title || '', text || '', icon || 'info'); return; }
                } catch (e) { /* ignore */ }
                alert((title ? (title + "\n") : "") + (text || ""));
            }

            function swalConfirm(title, text, icon, onOk) {
                try {
                    if (window.Swal && typeof Swal.fire === 'function') {
                        Swal.fire({
                            title: title || '',
                            text: text || '',
                            icon: icon || 'question',
                            showCancelButton: true,
                            confirmButtonText: 'Sí',
                            cancelButtonText: 'No'
                        }).then(function (r) { if (r && r.isConfirmed && typeof onOk === 'function') onOk(); });
                        return;
                    }
                } catch (e) { /* ignore */ }
                if (confirm((title ? (title + "\n") : "") + (text || ""))) { if (typeof onOk === 'function') onOk(); }
            }

            async function fetchJsonSafe(url, opts) {
                const options = opts || {};
                if (!options.credentials) options.credentials = 'same-origin';
                const res = await fetch(url, options);
                const raw = await res.text();
                let json = null;
                try { json = raw ? JSON.parse(raw) : {}; } catch (e) {
                    // Si el backend devolvió HTML (por ejemplo login), mostramos un mensaje útil
                    throw new Error('Respuesta no es JSON (HTTP ' + res.status + '). Posible sesión expirada o error del servidor.');
                }
                if (!res.ok) {
                    const msg = (json && (json.message || json.error)) ? (json.message || json.error) : ('HTTP ' + res.status);
                    throw new Error(msg);
                }
                return json;
            }

            async function getJson(action, params) {
                const qp = new URLSearchParams(params || {});
                if (action) qp.append('action', action);
                return await fetchJsonSafe(HANDLER_URL + '?' + qp.toString(), { method: 'GET' });
            }

            async function postJson(action, params, bodyObj) {
                const qp = new URLSearchParams(params || {});
                if (action) qp.append('action', action);
                return await fetchJsonSafe(HANDLER_URL + '?' + qp.toString(), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify(bodyObj || {})
                });
            }

            async function getJsonPeriodos(action, params) {
                const qp = new URLSearchParams(params || {});
                if (action) qp.append('action', action);
                return await fetchJsonSafe(HANDLER_PERIODOS + '?' + qp.toString(), { method: 'GET' });
            }

            async function postFormPeriodos(formData) {
                return await fetchJsonSafe(HANDLER_PERIODOS, { method: 'POST', body: formData });
            }

            function setBtnBusy(btn, busy, textBusy) {
                if (!btn) return;
                if (busy) {
                    btn.disabled = true;
                    if (!btn.getAttribute('data-orig-text')) btn.setAttribute('data-orig-text', btn.innerHTML);
                    btn.innerHTML = (textBusy || 'Procesando...');
                } else {
                    btn.disabled = false;
                    const orig = btn.getAttribute('data-orig-text');
                    if (orig) btn.innerHTML = orig;
                }
            }


            // Catálogo GLOBAL de periodicidades (tipo_periodo) — data-driven desde BD
            // IMPORTANTE: Este catálogo es para la tabla catalogo_producto_financiero_periodos (Semanal/Quincenal/Mensual, etc.).
            let _periodicidadCatalog = []; // [{tipo_periodo}]
            function getPeriodicidadValues() {
                return Array.isArray(_periodicidadCatalog)
                    ? _periodicidadCatalog.map(x => String((x && x.tipo_periodo) ? x.tipo_periodo : '').trim()).filter(x => x)
                    : [];
            }
            function fillPeriodicidadSelect(selectEl, selectedValue) {
                if (!selectEl) return;
                const prev = (selectedValue !== undefined) ? String(selectedValue || '').trim() : String(selectEl.value || '').trim();
                const values = getPeriodicidadValues();
                selectEl.innerHTML = '<option value="">--------</option>';
                values.forEach(v => selectEl.appendChild(new Option(v, v)));
                if (prev) selectEl.value = prev;
            }
            async function loadPeriodicidadCatalogOrFail() {
                try {
                    // El catálogo de periodicidades está controlado por el backend (IsTipoPeriodoValido).
                    // Para evitar desalineaciones y dependencias innecesarias, aquí listamos EXACTAMENTE los valores permitidos.
                    _periodicidadCatalog = [
                        { tipo_periodo: 'Mensual' },
                        { tipo_periodo: 'Quincenal' },
                        { tipo_periodo: 'Semanal' }
                    ];

                    const values = getPeriodicidadValues();
                    fillPeriodicidadSelect(document.getElementById('periodos_tipo_periodo'));
                    fillPeriodicidadSelect(document.getElementById('mp_periodos_tipo_periodo'));

                    if (!values.length) {
                        const btnGuardarPeriodo = document.getElementById('btnGuardarPeriodo');
                        if (btnGuardarPeriodo) btnGuardarPeriodo.setAttribute('disabled', 'disabled');
                        Swal.fire('Error', 'No hay catálogo de periodicidades disponible.', 'error');
                        throw new Error('Catalogo de periodicidades vacío');
                    } else {
                        const btnGuardarPeriodo = document.getElementById('btnGuardarPeriodo');
                        if (btnGuardarPeriodo) btnGuardarPeriodo.removeAttribute('disabled');
                    }
                } catch (e) {
                    err('loadPeriodicidadCatalogOrFail', e);
                    _periodicidadCatalog = [];
                }

                const values = getPeriodicidadValues();
                fillPeriodicidadSelect(document.getElementById('periodos_tipo_periodo'));
                fillPeriodicidadSelect(document.getElementById('mp_periodos_tipo_periodo'));

                if (!values.length) {
                    if (btnGuardarPeriodo) btnGuardarPeriodo.setAttribute('disabled', 'disabled');
                    Swal.fire('Error', 'No hay catálogo de periodicidades disponible. Debes tener valores en BD para poder capturar (ej. Semanal/Quincenal/Mensual).', 'error');
                    throw new Error('Catalogo de periodicidades vacío');
                } else {
                    if (btnGuardarPeriodo) btnGuardarPeriodo.removeAttribute('disabled');
                }
            }

            // Control de flujo de alta: maestro + al menos 1 periodicidad
            let _altaPendiente = false;
            let _altaProductoId = null;
            let _altaResuelta = false;


            // =========
            // HELPERS
            // =========
            function $id(id) { try { return document.getElementById(id) || null; } catch (e) { return null; } }
            function findField(id) {
                if (!id) return null;
                const modal = $id('modalCatalogo');
                let el = null;
                try { if (modal) el = modal.querySelector('#' + id); } catch (e) { }
                if (!el) el = $id(id) || null;
                return el;
            }
            function parseNumberStrict(valor) {
                if (valor === null || valor === undefined) return NaN;
                let s = String(valor).trim().replace(/\s/g, '').replace(/,/g, '.');
                return (/^[+-]?\d+(\.\d+)?$/).test(s) ? parseFloat(s) : NaN;
            }

            function escapeHtml(str) {
                const s = (str === undefined || str === null) ? '' : String(str);
                return s
                    .replace(/&/g, '&amp;')
                    .replace(/</g, '&lt;')
                    .replace(/>/g, '&gt;')
                    .replace(/"/g, '&quot;')
                    .replace(/'/g, '&#39;');
            }

            function mapTipoPeriodoDetalleToDb(raw) {
                const v = String(raw || '').trim().toUpperCase();
                if (v === 'SEMANAL') return 'Semanal';
                if (v === 'QUINCENAL') return 'Quincenal';
                if (v === 'MENSUAL') return 'Mensual';

                // Compatibilidad con valores legacy del UI
                if (v === 'COMPLETO' || v === 'FIJO') return 'Mensual';

                return '';
            }


            function log() { try { console.log.apply(console, ['[PLD]'].concat(Array.from(arguments))); } catch (e) { } }
            function err() { try { console.error.apply(console, ['[PLD]'].concat(Array.from(arguments))); } catch (e) { } }

            // =================
            // DATATABLE (LISTA)
            // =================
            if (!$.fn.DataTable.isDataTable('#tblCatalogo')) {
                window.catalogoTable = $('#tblCatalogo').DataTable({
                    processing: true,
                    serverSide: true,

                    // ⬇️ FORMATO EXACTO: "Show entries" a la izquierda, "Search" a la derecha,
                    // tabla, y abajo "Showing ... entries" a la izq. y paginación a la der.
                    dom: "<'row align-items-center'<'col-sm-6'l><'col-sm-6'f>>" +
                        "t" +
                        "<'row align-items-center mt-2'<'col-sm-6'i><'col-sm-6'p>>",

                    // (TEXTO igual al que muestra DataTables por defecto en inglés del screenshot)
                    language: {
                        lengthMenu: "Show _MENU_ entries",
                        search: "Search:",
                        info: "Showing _START_ to _END_ of _TOTAL_ entries",
                        infoEmpty: "Showing 0 to 0 of 0 entries",
                        zeroRecords: "No matching records found",
                        paginate: { previous: "Previous", next: "Next" }
                    },

                    // ⬇️ Tu ajax intacto
                    ajax: function (data, callback) {
                        const params = new URLSearchParams();
                        params.append('action', 'list');
                        params.append('start', data.start);
                        params.append('length', data.length);
                        const searchValue = (data.search && typeof data.search.value === 'string') ? data.search.value : '';
                        params.append('q', searchValue);

                        (async function () {
                            try {
                                const json = await fetchJsonSafe(HANDLER_URL + '?' + params.toString(), { method: 'GET' });

                                if (json && json.success) {
                                    callback({ data: json.data || [], recordsTotal: json.recordsTotal || 0, recordsFiltered: json.recordsFiltered || 0 });
                                } else if (Array.isArray(json)) {
                                    callback({ data: json, recordsTotal: json.length, recordsFiltered: json.length });
                                } else {
                                    callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                                }
                            } catch (e) {
                                try { err('DataTable ajax error', e); } catch (x) { console.error(e); }
                                swalFire('Error', 'No se pudieron cargar los productos. Revisa consola.', 'error');
                                callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                            }
                        })();
                    },

                    // ⬇️ Tus columnas intactas
                    columns: [
                        { data: "producto_id" },
                        { data: "descripcion_larga" },
                        { data: "impacto", className: "text-end" },
                        { data: "probabilidad", className: "text-end" },
                        { data: "nivel_riesgo_pld", className: "text-end" },
                        { data: "activo", render: d => (d == 1 || d === true) ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>' },
                        { data: null, render: (d, t, r) => '<span class="badge bg-info">' + (r.mitigantes_count || 0) + '</span>' },
                        {
                            data: null, orderable: false, render: (d, t, r) =>
                                '<div class="btn-group" role="group" aria-label="Acciones">' +
                                '<button type="button" class="btn btn-sm btn-outline-secondary btn-consultar me-1" data-id="' + r.producto_id + '"><i class="fas fa-eye"></i></button>' +
                                '<button type="button" class="btn btn-sm btn-outline-warning btn-editar me-1" data-id="' + r.producto_id + '"><i class="fas fa-pen"></i></button>' +
                                '<button type="button" class="btn btn-sm btn-outline-primary btn-periodicidades me-1" data-id="' + r.producto_id + '" data-desc="' + encodeURIComponent(r.descripcion_larga || '') + '"><i class="fas fa-calendar-alt"></i></button>' +
                                '<button type="button" class="btn btn-sm btn-outline-danger btn-borrar" data-id="' + r.producto_id + '"><i class="fas fa-trash-alt"></i></button>' +
                                '</div>'
                        }
                    ]
                });
            }


            // ===========
            // MODAL BASE
            // ===========
            const modalRoot = $id('modalCatalogo');
            const bsModal = modalRoot ? new bootstrap.Modal(modalRoot, { backdrop: 'static', keyboard: false }) : null;
            const modalPeriodosRoot = $id('modalPeriodos');
            const bsModalPeriodos = modalPeriodosRoot ? new bootstrap.Modal(modalPeriodosRoot, { backdrop: 'static', keyboard: false }) : null;

            // Bloquear cierre de modales si la alta está pendiente (evita productos sin detalle)
            if (modalRoot) {
                modalRoot.addEventListener('hide.bs.modal', function (ev) {
                    if (_altaPendiente && !_altaResuelta) {
                        ev.preventDefault();
                        Swal.fire('Atención', 'Debes capturar al menos una periodicidad para finalizar el alta.', 'warning');
                    }
                });
            }
            if (modalPeriodosRoot) {
                modalPeriodosRoot.addEventListener('hide.bs.modal', function (ev) {
                    if (_altaPendiente && !_altaResuelta) {
                        ev.preventDefault();
                        Swal.fire('Atención', 'Debes capturar al menos una periodicidad para finalizar el alta.', 'warning');
                    }
                });
            }

            async function evaluarAltaResuelta(productoId) {
                try {
                    const params = new URLSearchParams();
                    params.append('action', 'periodos_combo');
                    params.append('producto_financiero_id', productoId);
                    const j = await fetchJsonSafe(HANDLER_PERIODOS + '?' + params.toString(), { method: 'GET' });
                    const arr = (j && j.success && Array.isArray(j.data)) ? j.data : [];
                    _altaResuelta = arr.length > 0;
                    return _altaResuelta;
                } catch (e) {
                    err('evaluarAltaResuelta', e);
                    _altaResuelta = false;
                    return false;
                }
            }


            function $p(id) { try { return document.getElementById(id) || null; } catch (e) { return null; } }
            function parseIntStrict(v) {
                if (v === null || v === undefined) return NaN;
                const s = String(v).trim().replace(/\s/g, '');
                return (/^[+-]?\d+$/).test(s) ? parseInt(s, 10) : NaN;
            }
            function resetPeriodoForm() {
                const hid = $p('periodos_id'); if (hid) hid.value = '';
                const t = $p('periodos_tipo_periodo'); if (t) t.value = '';
                const pz = $p('periodos_plazo'); if (pz) pz.value = '';
                const tasa = $p('periodos_tasa_interes'); if (tasa) tasa.value = '';
            }

            async function openPeriodosModalSafe(productoId, desc) {
                /* catálogo de periodicidades se carga al abrir el modal de Periodicidades */
                try { await openPeriodosModal(productoId, desc); }
                catch (e) { err('openPeriodosModalSafe', e); Swal.fire('Error', 'No se pudo abrir Periodicidades. Revisa consola.', 'error'); }
            }

            async function openPeriodosModal(productoId, desc) {
                if (!bsModalPeriodos) { Swal.fire('Error', 'Modal de Periodicidades no disponible.', 'error'); return; }
                const hidProd = $p('periodos_producto_id'); if (hidProd) hidProd.value = productoId || '';
                const ttl = $p('periodos_producto_titulo'); if (ttl) ttl.textContent = (desc && desc.trim()) ? desc : ('Producto #' + productoId);
                resetPeriodoForm();
                await cargarPeriodos(productoId);

                // Si estamos en flujo de ALTA, cerrar hasta tener al menos 1 periodicidad
                if (_altaPendiente && _altaProductoId && String(_altaProductoId) === String(productoId)) {
                    const ok = await evaluarAltaResuelta(String(productoId));
                    if (ok) {
                        _altaResuelta = true;
                        _altaPendiente = false;

                        const btnG = findField('btnGuardarModal');
                        if (btnG) btnG.removeAttribute('disabled');

                        if (bsModalPeriodos) bsModalPeriodos.hide();
                        if (bsModal) bsModal.hide();
                        Swal.fire('Correcto', 'Alta finalizada (maestro + periodicidad).', 'success');
                        if (window.catalogoTable) window.catalogoTable.draw(false);
                    }
                }
                bsModalPeriodos.show();
            }


            // =======================
            // PERIODOS EN MODAL PRODUCTO (MISMA MODAL)
            // =======================
            let mpPeriodos = [];          // { id, tipo_periodo, plazo, tasa_interes }
            let mpDeletedIds = [];        // ids eliminados (solo para update)

            function mpResetPeriodoForm() {
                const hid = document.getElementById('mp_periodos_id'); if (hid) hid.value = '';
                const tp = document.getElementById('mp_periodos_tipo_periodo'); if (tp) tp.value = '';
                const pl = document.getElementById('mp_periodos_plazo'); if (pl) pl.value = '';
                const ti = document.getElementById('mp_periodos_tasa_interes'); if (ti) ti.value = '';
            }
            function mpParseInt(v) { const n = parseInt(String(v || '').trim(), 10); return isNaN(n) ? null : n; }
            function mpParseDec(v) { const n = parseNumberStrict(String(v || '').trim()); return isNaN(n) ? null : n; }

            function mpRenderPeriodos() {
                const tbody = document.querySelector('#mp_tblPeriodos tbody');
                if (!tbody) return;
                if (!mpPeriodos.length) { tbody.innerHTML = '<tr><td colspan="4">(Sin periodicidades)</td></tr>'; return; }

                tbody.innerHTML = mpPeriodos.map(function (r, idx) {
                    const tp = escapeHtml(r.tipo_periodo || '');
                    const pl = (r.plazo != null) ? String(r.plazo) : '';
                    const ti = (r.tasa_interes != null) ? String(r.tasa_interes) : '';
                    const idAttr = (r.id != null && r.id !== '') ? String(r.id) : '';
                    return (
                        '<tr data-idx="' + idx + '" data-id="' + escapeHtml(idAttr) + '">' +
                        '<td>' + tp + '</td>' +
                        '<td class="text-end">' + escapeHtml(pl) + '</td>' +
                        '<td class="text-end">' + escapeHtml(ti) + '</td>' +
                        '<td class="text-end">' +
                        '<button type="button" class="btn btn-outline-primary btn-sm mp-edit-periodo" title="Editar"><i class="fas fa-edit"></i></button> ' +
                        '<button type="button" class="btn btn-outline-danger btn-sm mp-del-periodo" title="Eliminar"><i class="fas fa-trash"></i></button>' +
                        '</td>' +
                        '</tr>'
                    );
                }).join('');
            }

            async function mpLoadPeriodosFromServer(productoId) {
                mpPeriodos = [];
                mpDeletedIds = [];
                mpRenderPeriodos();

                if (!productoId) return;

                const params = new URLSearchParams();
                params.append('action', 'periodos_listar');
                params.append('producto_financiero_id', productoId);

                const j = await fetchJsonSafe(HANDLER_PERIODOS + '?' + params.toString(), { method: 'GET' });
                if (!j || !j.success) {
                    Swal.fire('Error', (j && j.message) ? j.message : 'No se pudieron cargar las periodicidades', 'error');
                    return;
                }
                const rows = (Array.isArray(j.data) ? j.data : []).filter(function (x) {
                    if (!x) return false;
                    if (x.activo === undefined || x.activo === null) return true;
                    if (x.activo === true || x.activo === 1) return true;
                    const activoStr = String(x.activo).trim().toLowerCase();
                    return activoStr === 'true' || activoStr === '1';
                });
                mpPeriodos = rows.map(function (x) {
                    return {
                        id: x.id || x.periodo_id || x.producto_financiero_periodo_id || x.ID || '',
                        tipo_periodo: x.tipo_periodo || x.TipoPeriodo || '',
                        plazo: (x.plazo != null) ? parseInt(x.plazo, 10) : null,
                        tasa_interes: (x.tasa_interes != null) ? parseNumberStrict(String(x.tasa_interes)) : null
                    };
                });
                mpRenderPeriodos();
            }

            function mpGetPeriodosPayload() {
                const clean = mpPeriodos.map(function (r) {
                    return {
                        id: (r.id != null) ? String(r.id).trim() : '',
                        tipo_periodo: String(r.tipo_periodo || '').trim(),
                        plazo: (r.plazo != null) ? parseInt(r.plazo, 10) : 0,
                        tasa_interes: (r.tasa_interes != null) ? parseNumberStrict(String(r.tasa_interes)) : 0
                    };
                }).filter(function (x) { return x.tipo_periodo && x.plazo > 0; });

                return clean;
            }

            async function cargarPeriodos(productoId) {
                const tbody = document.querySelector('#tblPeriodos tbody');
                if (tbody) tbody.innerHTML = '<tr><td colspan="5">Cargando...</td></tr>';

                const params = new URLSearchParams();
                params.append('action', 'periodos_listar');
                params.append('producto_financiero_id', productoId);

                const j = await fetchJsonSafe(HANDLER_PERIODOS + '?' + params.toString(), { method: 'GET' });

                if (!j || !j.success) {
                    if (tbody) tbody.innerHTML = '<tr><td colspan="5">(Sin datos)</td></tr>';
                    Swal.fire('Error', (j && j.message) ? j.message : 'No se pudieron cargar las periodicidades', 'error');
                    return;
                }

                const rows = Array.isArray(j.data) ? j.data : [];
                if (!rows.length) { if (tbody) tbody.innerHTML = '<tr><td colspan="5">(Sin periodicidades)</td></tr>'; return; }

                const html = rows.map(function (x) {
                    const est = (x.activo == 1 || x.activo === true) ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-secondary">Inactivo</span>';
                    const btnToggleText = (x.activo == 1 || x.activo === true) ? 'Desactivar' : 'Activar';
                    const btnToggleClass = (x.activo == 1 || x.activo === true) ? 'btn-outline-secondary' : 'btn-outline-success';
                    const tasaFmt = (x.tasa_interes !== null && x.tasa_interes !== undefined) ? Number(x.tasa_interes).toFixed(6) : '0.000000';
                    return '<tr data-id="' + x.id + '">' +
                        '<td>' + (x.tipo_periodo || '') + '</td>' +
                        '<td class="text-end">' + (x.plazo ?? '') + '</td>' +
                        '<td class="text-end">' + tasaFmt + '</td>' +
                        '<td>' + est + '</td>' +
                        '<td>' +
                        '<div class="btn-group" role="group">' +
                        '<button type="button" class="btn btn-sm btn-outline-warning btn-periodo-editar" data-id="' + x.id + '"><i class="fas fa-pen"></i></button>' +
                        '<button type="button" class="btn btn-sm ' + btnToggleClass + ' btn-periodo-toggle" data-id="' + x.id + '">' + btnToggleText + '</button>' +
                        '<button type="button" class="btn btn-sm btn-outline-danger btn-periodo-eliminar" data-id="' + x.id + '"><i class="fas fa-trash-alt"></i></button>' +
                        '</div>' +
                        '</td>' +
                        '</tr>';
                }).join('');

                if (tbody) tbody.innerHTML = html;
            }

            // Acciones dentro de tabla periodos
            document.addEventListener('click', function (ev) {
                const t = ev.target;
                const btnEdit = t.closest ? t.closest('.btn-periodo-editar') : null;
                const btnToggle = t.closest ? t.closest('.btn-periodo-toggle') : null;
                const btnDel = t.closest ? t.closest('.btn-periodo-eliminar') : null;

                if (btnEdit) {
                    const id = btnEdit.getAttribute('data-id');
                    const tr = btnEdit.closest('tr');
                    if (!tr) return;
                    const cols = tr.querySelectorAll('td');
                    const tipo = cols[0] ? cols[0].textContent.trim() : '';
                    const plazo = cols[1] ? cols[1].textContent.trim() : '';
                    const tasa = cols[2] ? cols[2].textContent.trim() : '';
                    const hid = $p('periodos_id'); if (hid) hid.value = id || '';
                    const tp = $p('periodos_tipo_periodo'); if (tp) tp.value = tipo;
                    const pz = $p('periodos_plazo'); if (pz) pz.value = plazo;
                    const ta = $p('periodos_tasa_interes'); if (ta) ta.value = tasa;
                    return;
                }

                if (btnToggle) {
                    const id = btnToggle.getAttribute('data-id');
                    const productoId = $p('periodos_producto_id') ? $p('periodos_producto_id').value : '';
                    Swal.fire({
                        title: 'Confirmar',
                        text: '¿Cambiar estatus de la periodicidad?',
                        icon: 'question',
                        showCancelButton: true,
                        confirmButtonText: 'Sí',
                        cancelButtonText: 'Cancelar'
                    }).then(function (res) {
                        if (!res.isConfirmed) return;
                        const form = new URLSearchParams();
                        form.append('action', 'periodos_toggle');
                        form.append('id', id);
                        fetchJsonSafe(HANDLER_PERIODOS, { method: 'POST', body: form })
                            .then(r => r.json())
                            .then(j => {
                                if (j && j.success) cargarPeriodos(productoId);
                                else Swal.fire('Error', (j && j.message) ? j.message : 'No se pudo cambiar estatus', 'error');
                            })
                            .catch(e => { err('periodos_toggle', e); Swal.fire('Error', 'Error en servidor', 'error'); });
                    });
                    return;
                }

                if (btnDel) {
                    const id = btnDel.getAttribute('data-id');
                    const productoId = $p('periodos_producto_id') ? $p('periodos_producto_id').value : '';
                    Swal.fire({
                        title: 'Confirmar',
                        text: '¿Eliminar la periodicidad?',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: 'Sí, eliminar',
                        cancelButtonText: 'Cancelar'
                    }).then(function (res) {
                        if (!res.isConfirmed) return;
                        const form = new URLSearchParams();
                        form.append('action', 'periodos_eliminar');
                        form.append('id', id);
                        fetchJsonSafe(HANDLER_PERIODOS, { method: 'POST', body: form })
                            .then(r => r.json())
                            .then(j => {
                                if (j && j.success) { resetPeriodoForm(); cargarPeriodos(productoId); }
                                else Swal.fire('Error', (j && j.message) ? j.message : 'No se pudo eliminar', 'error');
                            })
                            .catch(e => { err('periodos_eliminar', e); Swal.fire('Error', 'Error en servidor', 'error'); });
                    });
                    return;
                }
            });

            // Guardar periodo (insert/update)
            const btnGuardarPeriodo = $p('btnGuardarPeriodo');
            if (btnGuardarPeriodo) btnGuardarPeriodo.addEventListener('click', function () {
                (async function () {
                    try {
                        const productoId = $p('periodos_producto_id') ? $p('periodos_producto_id').value : '';
                        const id = $p('periodos_id') ? $p('periodos_id').value : '';
                        const tipo = $p('periodos_tipo_periodo') ? $p('periodos_tipo_periodo').value : '';
                        const plazoStr = $p('periodos_plazo') ? $p('periodos_plazo').value : '';
                        const tasaStr = $p('periodos_tasa_interes') ? $p('periodos_tasa_interes').value : '';

                        const allowed = getPeriodicidadValues();
                        if (!tipo) { Swal.fire('Atención', 'Selecciona un tipo de periodo.', 'warning'); return; }
                        if (allowed.length > 0 && allowed.indexOf(tipo) === -1) { Swal.fire('Atención', 'Tipo periodo inválido.', 'warning'); return; }

                        const plazo = parseIntStrict(plazoStr);
                        if (isNaN(plazo) || plazo <= 0) { Swal.fire('Atención', 'Plazo debe ser entero > 0.', 'warning'); return; }

                        const tasa = parseNumberStrict(tasaStr);
                        if (isNaN(tasa) || tasa < 0) { Swal.fire('Atención', 'Tasa interés debe ser decimal >= 0.', 'warning'); return; }

                        const form = new URLSearchParams();
                        form.append('action', id ? 'periodos_actualizar' : 'periodos_guardar');
                        form.append('id', id);
                        form.append('producto_financiero_id', productoId);
                        form.append('tipo_periodo', tipo);
                        form.append('plazo', plazo);
                        form.append('tasa_interes', tasa);

                        const r = await fetchJsonSafe(HANDLER_PERIODOS, { method: 'POST', body: form });
                        const j = await r.json();

                        if (j && j.success) {
                            Swal.fire('Correcto', j.message || 'Guardado', 'success');
                            resetPeriodoForm();
                            await cargarPeriodos(productoId);
                        } else {
                            Swal.fire('Error', (j && j.message) ? j.message : 'No se pudo guardar', 'error');
                        }
                    } catch (e) {
                        err('periodos_guardar', e);
                        Swal.fire('Error', 'Error al guardar periodicidad. Revisa consola.', 'error');
                    }
                })();
            });

            const btnCancelarEdicionPeriodo = $p('btnCancelarEdicionPeriodo');
            if (btnCancelarEdicionPeriodo) btnCancelarEdicionPeriodo.addEventListener('click', function () { resetPeriodoForm(); });


            function clearModalFields() {
                try {
                    // Nota: En WebForms ya existe un &lt;form runat="server"&gt;. Aquí NO usamos form anidado.
                    // Por eso evitamos form.reset() y limpiamos inputs manualmente dentro del modal.
                    const root = (modalRoot || document);
                    const scope = root.querySelector('#modalProducto') || root;

                    // Limpia inputs/selects/textareas dentro del modal (sin tocar otros elementos de la página)
                    const fields = scope.querySelectorAll('input, select, textarea');
                    for (let i = 0; i < fields.length; i++) {
                        const el = fields[i];
                        const tag = (el.tagName || '').toUpperCase();
                        const type = (el.getAttribute('type') || '').toLowerCase();

                        // No borrar valores "constantes" si existieran (por ejemplo, disabled readonly)
                        if (el.disabled) continue;

                        if (type === 'checkbox' || type === 'radio') {
                            el.checked = false;
                            continue;
                        }

                        if (tag === 'SELECT') {
                            // Si tiene placeholder, lo deja; si no, selecciona el primer option
                            el.selectedIndex = 0;
                            continue;
                        }

                        // inputs/textareas
                        el.value = '';
                    }

                    // Limpia ID oculto del maestro
                    const mid = findField('modal_producto_id');
                    if (mid) mid.value = '';

                    // Defaults para selects SI/NO
                    const ms = findField('fld_monto_seguro'); if (ms) ms.value = 'NO';
                    const ac = findField('fld_aplica_cargos'); if (ac) ac.value = '';
                    const amc = findField('fld_amortiza_cargos'); if (amc) amc.value = '';
                    const cm = findField('fld_cargo_multa'); if (cm) cm.value = '';

                } catch (e) { err('clearModalFields', e); }
            }
            function setModalReadonly(readonly) {
                const form = (modalRoot && modalRoot.querySelector('form')) || $id('formProducto');
                if (!form) return;
                Array.from(form.querySelectorAll('input,select,textarea,button')).forEach(el => {
                    if (!el) return;
                    if (el.id === 'btnGuardarModal' || el.id === 'btnCancelarModal') return;
                    // Mantener navegación entre pestañas aun en modo consulta.
                    const isTabButton = (el.tagName.toLowerCase() === 'button')
                        && (((el.getAttribute('data-bs-toggle') || '').toLowerCase() === 'tab')
                            || el.classList.contains('nav-link'));
                    if (isTabButton) {
                        el.disabled = false;
                        el.removeAttribute('disabled');
                        return;
                    }
                    if (readonly) {
                        if (el.tagName.toLowerCase() === 'button') el.disabled = true; else el.setAttribute('disabled', 'disabled');
                    } else { el.removeAttribute('disabled'); }
                });
            }

            // Asegurar que "Producto" es input (no select)
            (function ensureProductoIsTextbox() {
                const el = findField('fld_producto');
                if (!el) return;
                if (el.tagName.toLowerCase() === 'select') {
                    const inp = document.createElement('input');
                    inp.type = 'text'; inp.id = el.id; inp.name = el.name || el.id; inp.className = (el.className || '') + ' form-control form-control-sm';
                    el.parentNode.replaceChild(inp, el);
                }
            })();

            // ==========================
            // LOOKUPS DE CATÁLOGOS
            // ==========================
            function fillRegimenFiscal() {
                const sel = findField('fld_regimen'); if (!sel) return;
                sel.innerHTML = '';
                sel.appendChild(new Option('--------', ''));
                // DB: catalogo_producto_financiero.regimen_fiscal_id es INT (sin FK). Usamos mapeo simple:
                // 1=Persona Física, 2=Persona Física con actividad empresarial, 3=Persona Moral
                sel.appendChild(new Option('Persona Física', '1'));
                sel.appendChild(new Option('Persona Física con actividad empresarial', '2'));
                sel.appendChild(new Option('Persona Moral', '3'));
            }
            async function fillCreditos() {
                const sel = findField('fld_credito'); if (!sel) return false;
                sel.innerHTML = '<option value="">Cargando...</option>';
                try {
                    const r = await fetch(HANDLER_CREDITOS + '?op=consultar', { credentials: 'same-origin' });
                    if (!r.ok) throw new Error('HTTP ' + r.status);
                    const j = await r.json();
                    let items = Array.isArray(j) ? j
                        : Array.isArray(j.data) ? j.data
                            : Array.isArray(j.rows) ? j.rows
                                : Array.isArray(j.list) ? j.list
                                    : Array.isArray(j.datos) ? j.datos
                                        : [];
                    sel.innerHTML = '<option value="">--------</option>';
                    items.forEach(it => {
                        const value = it.credito ?? it.credito_id ?? it.id ?? it.clave ?? '';
                        const text = it.descripcion ?? it.nombre ?? it.text ?? String(value);
                        sel.appendChild(new Option(text, String(value)));
                    });
                    return sel.options.length > 1;
                } catch (e) { err('fillCreditos', e); sel.innerHTML = '<option value="">(No disponible)</option>'; return false; }
            }
            async function fillMonedas() {
                const sel = findField('fld_moneda'); if (!sel) return false;
                sel.innerHTML = '<option value="">Cargando...</option>';
                try {
                    const r = await fetch(HANDLER_MONEDAS + '?op=consultar', { credentials: 'same-origin' });
                    if (!r.ok) throw new Error('HTTP ' + r.status);
                    const j = await r.json();
                    const items = Array.isArray(j.datos) ? j.datos : [];
                    sel.innerHTML = '<option value="">--------</option>';
                    items.forEach(it => {
                        const value = (it.id != null ? String(it.id) : ''); // valor que se envía (ID int moneda_id)
                        const label = (it.clave ? it.clave + ' - ' : '') + (it.moneda ?? '');
                        const opt = new Option(label, value);
                        opt.dataset.id = it.id ?? '';
                        opt.dataset.clave = it.clave ?? '';
                        sel.appendChild(opt);
                    });
                    return sel.options.length > 1;
                } catch (e) { err('fillMonedas', e); sel.innerHTML = '<option value="">(No disponible)</option>'; return false; }
            }

            // ==========================
            // PRELACIÓN: MISMO CATÁLOGO
            // ==========================
            const CATALOGO_PRELACION = [
                "Capital",
                "Intereses nominales",
                "Iva intereses nominales",
                "Intereses moratorios",
                "Iva moratorios",
                "Comisión por apertura",
                "Comisión por gastos de cobranza",
                "IVA comisión por gastos de cobranza",
                "Seguro",
                "Renta de servicios",
                "Cargos administrativos"
            ];
            function fillPrelacionSelect(selectEl) {
                if (!selectEl) return;
                const prev = selectEl.value;
                // Evita volver a llenar si ya tiene catálogo cargado
                if (selectEl.options && selectEl.options.length > 1) return;
                selectEl.innerHTML = '<option value="">--------</option>';
                CATALOGO_PRELACION.forEach(txt => selectEl.appendChild(new Option(txt, txt)));
                if (prev) selectEl.value = prev;
            }
            function fillAllPrelacion() {
                for (let i = 1; i <= 7; i++) fillPrelacionSelect(findField('fld_prelacion_' + i));
            }

            // Carga de lookups con reintento
            let _lookupsLoaded = false, _attempts = 0, _maxAttempts = 2;
            async function cargarLookupsSafe() {
                if (_lookupsLoaded) return;
                _attempts++;
                fillRegimenFiscal();
                const okCred = await fillCreditos();
                const okMon = await fillMonedas();
                await loadPeriodicidadCatalogOrFail();
                fillAllPrelacion();

                if (okCred && okMon) { _lookupsLoaded = true; return; }
                if (_attempts < _maxAttempts) {
                    if (!okCred) await fillCreditos();
                    if (!okMon) await fillMonedas();
                    const c = findField('fld_credito'), m = findField('fld_moneda');
                    if (c && c.options.length > 1 && m && m.options.length > 1) _lookupsLoaded = true;
                }
            }

            // ======================
            // EVENTOS DE LA TABLA
            // ======================
            $('#tblCatalogo tbody')
                .off('click', '.btn-consultar').on('click', '.btn-consultar', function () { openModalSafe(this.getAttribute('data-id'), true); })
                .off('click', '.btn-editar').on('click', '.btn-editar', function () { openModalSafe(this.getAttribute('data-id'), false); })
                .off('click', '.btn-periodicidades').on('click', '.btn-periodicidades', function () {
                    const id = this.getAttribute('data-id');
                    const desc = decodeURIComponent(this.getAttribute('data-desc') || '');
                    openPeriodosModalSafe(id, desc);
                })
                .off('click', '.btn-borrar').on('click', '.btn-borrar', function () {
                    const id = this.getAttribute('data-id');
                    Swal.fire({
                        title: 'Confirmar',
                        text: '¿Eliminar el registro #' + id + '?',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: 'Sí, eliminar',
                        cancelButtonText: 'Cancelar'
                    }).then(function (res) {
                        if (!res.isConfirmed) return;
                        (async function () {
                            try {
                                const json = await fetchJsonSafe(HANDLER_URL + '?action=delete&id=' + encodeURIComponent(id), { method: 'POST' });

                                if (json && json.success) {
                                    Swal.fire('Listo', 'Registro eliminado', 'success');
                                    if (window.catalogoTable) window.catalogoTable.draw(false);
                                } else {
                                    Swal.fire('Error', (json && json.message) ? json.message : 'No se pudo eliminar', 'error');
                                }
                            } catch (e) {
                                err('delete', e);
                                swalFire('Error', 'Error en servidor', 'error');
                            }
                        })();
                    });
                });

            // =====================
            // ABRIR MODAL / CAPTURA
            // =====================
            const btnCapturar = $id('btnCapturar');
            if (btnCapturar) { btnCapturar.addEventListener('click', () => openModalSafe('', false)); }

            async function openModalSafe(id, readonly, initialTab) {
                try { await openModal(id, readonly, initialTab); }
                catch (e) { err('openModalSafe', e); Swal.fire('Error', 'Ocurrió un error al abrir el modal. Revisa consola.', 'error'); }
            }
            async function openModal(id, readonly, initialTab) {
                clearModalFields();
                // Inicializar periodos en modal producto
                mpPeriodos = [];
                mpDeletedIds = [];
                mpResetPeriodoForm();
                mpRenderPeriodos();
                if (!modalRoot) { Swal.fire('Error', 'Modal no disponible (modalCatalogo).', 'error'); return; }

                const titleEl = findField('modalTitle');
                if (titleEl) titleEl.innerText = readonly ? 'Consultar producto' : (id ? 'Editar producto' : 'Nuevo producto');
                const btnGuardar = findField('btnGuardarModal');
                if (btnGuardar) btnGuardar.style.display = readonly ? 'none' : 'inline-block';

                // Cargar catálogos (incluye Prelación)
                await cargarLookupsSafe();
                // Forzar catálogo de prelación por si se limpió al abrir
                fillAllPrelacion();

                if (id) {
                    const res = await fetch(HANDLER_URL + '?action=get&id=' + encodeURIComponent(id), { method: 'GET', credentials: 'same-origin' });
                    if (!res.ok) { Swal.fire('Error', 'Respuesta inválida al obtener registro', 'error'); return; }
                    const json = await res.json();
                    if (!json || !json.success) { Swal.fire('Error', 'No se encontró el registro', 'error'); return; }
                    const master = (json.producto && typeof json.producto === 'object') ? json.producto : (json.data || {});
                    const det = (json.detalle && typeof json.detalle === 'object') ? json.detalle : {};
                    const planeacionRows = Array.isArray(json.planeacion) ? json.planeacion : [];

                    // ==========================
                    // BIND UI (EDIT) - mapeo explícito maestro/detalle
                    // ==========================
                    function _toStr(v) { return (v === undefined || v === null) ? '' : String(v); }
                    function _isTrue(v) {
                        return (v === true || v === 1 || v === '1' || String(v).toLowerCase() === 'true' || String(v).toUpperCase() === 'SI');
                    }
                    function setField(id, v) {
                        const el = findField(id);
                        if (!el) return;
                        if (el.type === 'checkbox') el.checked = _isTrue(v);
                        else el.value = _toStr(v);
                    }
                    function setSiNo(id, v) {
                        const el = findField(id);
                        if (!el) return;
                        if (_isTrue(v)) { el.value = 'SI'; return; }
                        const hasNo = Array.from(el.options || []).some(o => String(o.value || '').toUpperCase() === 'NO');
                        el.value = hasNo ? 'NO' : '';
                    }
                    function mapCalculoDbToUi(dbVal) {
                        const raw = _toStr(dbVal).trim().toUpperCase();
                        if (!raw) return '';
                        // DB: "SALDOS INICIALES ADELANTO NOMINA" -> UI: "ADELANTO_NOMINA"
                        if (raw === 'SALDOS INICIALES ADELANTO NOMINA') return 'ADELANTO_NOMINA';
                        const ui = raw.replace(/\s+/g, '_');
                        return (ALLOWED_CALCULO_UI.indexOf(ui) !== -1) ? ui : '';
                    }

                    // ---- Maestro ----
                    setField('fld_producto', master.descripcion_larga ?? master.descripcion ?? '');
                    setField('fld_credito', master.tipo_credito_id ?? master.credito_id ?? '');
                    setField('fld_regimen', master.regimen_fiscal_id ?? '');
                    setField('fld_moneda', master.moneda_id ?? '');
                    setField('fld_impacto', master.impacto ?? '');
                    setField('fld_probabilidad', master.probabilidad ?? '');
                    setField('fld_nivel_riesgo', master.nivel_riesgo_pld ?? '');
                    // compatibilidad: mostrar tipo_periodo maestro (VARIAS)
                    setField('fld_tipo_periodo_txt', master.tipo_periodo ?? '');

                    // ---- Detalle (detalles_del_producto) ----
                    setField('fld_tasa_anual', det.tasa_interes_anual);
                    setField('fld_tasa_anual_det', det.tasa_interes_anual_detallada);
                    setField('fld_tasa_mensual', det.tasa_interes_mensual);

                    setField('fld_iva', det.iva);
                    setField('fld_iva_comision', det.iva_comision);
                    setField('fld_iva_moratoria', det.iva_moratoria);
                    setSiNo('fld_iva_sobre_total', det.iva_sobre_total);

                    setField('fld_capital_min', det.capital_minimo);
                    setField('fld_capital_max', det.capital_maximo);

                    setField('fld_plazo_min', det.plazo_minimo);
                    setField('fld_plazo_max', det.plazo_maximo);

                    setField('fld_vencimiento', det.vencimiento);
                    setField('fld_calculo', mapCalculoDbToUi(det.calculo));

                    setSiNo('fld_factor_moratorio', det.factor_moratorio);
                    setField('fld_tasa_moratoria', det.tasa_moratoria);
                    setField('fld_dias_gracia', det.dias_gracia);

                    setField('fld_comision_apertura', det.comision_apertura);
                    setField('fld_comision_apertura_det', det.comision_apertura_detallada);
                    setField('fld_comision_gestion', det.comision_gestion);

                    setSiNo('fld_amortiza_comision_apertura', det.amortizar_comision_apertura);
                    setSiNo('fld_amortiza_comision_gestion', det.amortizar_comision_gestion);

                    setSiNo('fld_redondeo_centavos', det.usar_redondeo_centavos);
                    setField('fld_centavos_redondeo', det.centavos_para_redondeo);
                    setSiNo('fld_redondear_pago_fijo', det.redondear_pago_fijo);

                    setField('fld_aplicacion_tasa', det.aplicacion_de_tasa);
                    setField('fld_base_calculo', det.base_calculo);
                    setField('fld_calc_fecha_exigible', det.calculo_fecha_exigible);

                    setSiNo('fld_modificador_monto_credito', det.modificador_monto_credito);

                    setSiNo('fld_aplica_cargos', det.aplica_cargos_administrativos);
                    setSiNo('fld_amortiza_cargos', det.amortizar_cargos_administrativos);
                    setSiNo('fld_cargo_multa', det.aplica_cargo_multa_cobranza);

                    setSiNo('fld_aplica_comision_admin', det.aplica_comision_administracion);
                    setField('fld_iva_com_admin', det.iva_comision_administracion);
                    setField('fld_com_admin_det', det.comision_administracion_detallada);

                    setSiNo('fld_aplica_invest', det.aplica_comision_investigacion);
                    setField('fld_iva_com_inv', det.iva_comision_investigacion);
                    setField('fld_com_inv_det', det.comision_investigacion_detallada);

                    setSiNo('fld_monto_seguro', det.aplica_monto_seguro);

                    // Notas / Observaciones
                    setField('fld_notas_adicionales', det.notas ?? master.observaciones_generales ?? '');

                    // Tipo periodo (detalle) - CK_detalles_tipo_periodo = COMPLETO/FIJO
                    setField('fld_tipo_periodo', _toStr(det.tipo_periodo).toUpperCase());

                    // ---- Prelación (planeación) ----
                    for (let i = 1; i <= 7; i++) setField('fld_prelacion_' + i, '');
                    try {
                        planeacionRows.forEach(function (row) {
                            const ord = parseInt(row.orden ?? row.Orden ?? 0, 10);
                            const tipo = row.tipo ?? row.Tipo ?? '';
                            if (ord >= 1 && ord <= 7) setField('fld_prelacion_' + ord, tipo);
                        });
                    } catch (e) { }

                    const mid = findField('modal_producto_id'); if (mid) mid.value = _toStr(id);
                } else {
                    const mid = findField('modal_producto_id'); if (mid) mid.value = '';
                }

                // Cargar periodos existentes al editar
                try { if (id) await mpLoadPeriodosFromServer(String(id)); } catch (e) { /* ya se notifica */ }

                // Activar tab inicial si se pidió
                try {
                    if (initialTab) {
                        const btn = document.querySelector('button[data-bs-target="#' + initialTab + '"]');
                        if (btn) { const t = new bootstrap.Tab(btn); t.show(); }
                    }
                } catch (e) { }

                setModalReadonly(!!readonly);
                if (bsModal) bsModal.show(); else Swal.fire('Error', 'No se pudo instanciar el modal bootstrap', 'error');
            }


            // =======================
            // EVENTOS: PERIODOS EN MODAL PRODUCTO
            // =======================
            const btnMpGuardarPeriodo = document.getElementById('btnMpGuardarPeriodo');
            if (btnMpGuardarPeriodo) btnMpGuardarPeriodo.addEventListener('click', function () {
                try {
                    const id = (document.getElementById('mp_periodos_id') ? String(document.getElementById('mp_periodos_id').value || '').trim() : '');
                    const tipo = (document.getElementById('mp_periodos_tipo_periodo') ? String(document.getElementById('mp_periodos_tipo_periodo').value || '').trim() : '');
                    const plazo = mpParseInt(document.getElementById('mp_periodos_plazo') ? document.getElementById('mp_periodos_plazo').value : '');
                    const tasa = mpParseDec(document.getElementById('mp_periodos_tasa_interes') ? document.getElementById('mp_periodos_tasa_interes').value : '');

                    if (!tipo) { Swal.fire('Atención', 'Selecciona "Tipo periodo".', 'warning'); return; }
                    if (plazo == null || plazo <= 0) { Swal.fire('Atención', 'Captura "Plazo" (entero > 0).', 'warning'); return; }
                    if (tasa == null || tasa < 0) { Swal.fire('Atención', 'Captura "Tasa interés" (decimal >= 0).', 'warning'); return; }

                    // Evitar duplicado por tipo_periodo (mismo producto)
                    const dup = mpPeriodos.some(function (r) {
                        return String(r.tipo_periodo || '').toUpperCase() === tipo.toUpperCase() && String(r.id || '') !== id;
                    });
                    if (dup) { Swal.fire('Atención', 'Ya existe esa periodicidad para este producto.', 'warning'); return; }

                    if (id) {
                        const idx = mpPeriodos.findIndex(function (r) { return String(r.id || '') === id; });
                        if (idx >= 0) {
                            mpPeriodos[idx].tipo_periodo = tipo;
                            mpPeriodos[idx].plazo = plazo;
                            mpPeriodos[idx].tasa_interes = tasa;
                        }
                    } else {
                        // id temporal para edición antes de guardar
                        const tmpId = 'TMP_' + Date.now() + '_' + Math.floor(Math.random() * 1000);
                        mpPeriodos.push({ id: tmpId, tipo_periodo: tipo, plazo: plazo, tasa_interes: tasa });
                    }

                    mpResetPeriodoForm();
                    mpRenderPeriodos();
                } catch (e) {
                    err('btnMpGuardarPeriodo', e);
                    Swal.fire('Error', 'No se pudo agregar/actualizar el periodo. Revisa consola.', 'error');
                }
            });

            const btnMpCancelarPeriodo = document.getElementById('btnMpCancelarPeriodo');
            if (btnMpCancelarPeriodo) btnMpCancelarPeriodo.addEventListener('click', function () { mpResetPeriodoForm(); });

            const mpTbl = document.getElementById('mp_tblPeriodos');
            if (mpTbl) mpTbl.addEventListener('click', function (ev) {
                try {
                    const t = ev.target;
                    const btnEdit = (t && (t.closest ? t.closest('.mp-edit-periodo') : null));
                    const btnDel = (t && (t.closest ? t.closest('.mp-del-periodo') : null));
                    if (!btnEdit && !btnDel) return;

                    const tr = (t.closest ? t.closest('tr') : null);
                    if (!tr) return;
                    const idx = parseInt(tr.getAttribute('data-idx'), 10);
                    if (isNaN(idx) || idx < 0 || idx >= mpPeriodos.length) return;

                    const row = mpPeriodos[idx];

                    if (btnEdit) {
                        const hid = document.getElementById('mp_periodos_id'); if (hid) hid.value = String(row.id || '');
                        const tp = document.getElementById('mp_periodos_tipo_periodo'); if (tp) tp.value = String(row.tipo_periodo || '');
                        const pl = document.getElementById('mp_periodos_plazo'); if (pl) pl.value = (row.plazo != null) ? String(row.plazo) : '';
                        const ti = document.getElementById('mp_periodos_tasa_interes'); if (ti) ti.value = (row.tasa_interes != null) ? String(row.tasa_interes) : '';
                        return;
                    }

                    if (btnDel) {
                        Swal.fire({
                            title: 'Eliminar periodicidad',
                            text: '¿Deseas eliminar esta periodicidad?',
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonText: 'Sí',
                            cancelButtonText: 'No'
                        }).then(function (r) {
                            if (!r || !r.isConfirmed) return;
                            const removed = mpPeriodos.splice(idx, 1)[0];
                            // si era real (numérico), marcar para delete en update
                            if (removed && removed.id && String(removed.id).indexOf('TMP_') !== 0) {
                                mpDeletedIds.push(String(removed.id));
                            }
                            mpRenderPeriodos();
                        });
                    }
                } catch (e) {
                    err('mp_tblPeriodos', e);
                }
            });


            // =========
            // GUARDAR
            // =========
            const btnGuardar = findField('btnGuardarModal');
            if (btnGuardar) btnGuardar.addEventListener('click', onGuardarClick);

            function onGuardarClick() {
                (async function () {
                    try {
                        const idStr = (findField('modal_producto_id') && findField('modal_producto_id').value) ? String(findField('modal_producto_id').value).trim() : '';
                        const isEdit = idStr !== '';
                        const id = parseInt(idStr, 10);

                        const impEl = findField('fld_impacto'), probEl = findField('fld_probabilidad'), nivelEl = findField('fld_nivel_riesgo');
                        if (!impEl || !probEl || !nivelEl) { Swal.fire('Error', 'Campos numéricos no encontrados', 'error'); return; }

                        const imp = parseNumberStrict(impEl.value);
                        const prob = parseNumberStrict(probEl.value);
                        const niv = parseNumberStrict(nivelEl.value);
                        if (isNaN(imp) || isNaN(prob) || isNaN(niv)) {
                            Swal.fire('Atención', 'Impacto/Probabilidad/Nivel deben ser numéricos (no ALTO/MEDIO/BAJO).', 'warning'); return;
                        }

                        // =========
                        // VALIDACIÓN / NORMALIZACIÓN: detalles_del_producto.calculo (CK_detalles_calculo)
                        // NOTA: En producción el CHECK suele validar valores con ESPACIOS (no guiones bajos),
                        // por eso guardamos en DB como "SALDOS INSOLUTOS" aunque el <select> use "SALDOS_INSOLUTOS".
                        // =========
                        const calculoSelUi = (findField('fld_calculo') ? String(findField('fld_calculo').value || '').trim() : '');
                        if (ALLOWED_CALCULO_UI.indexOf(calculoSelUi) === -1) {
                            Swal.fire('Atención', 'Selecciona un valor válido en "Cálculo".', 'warning');
                            const cEl = findField('fld_calculo'); if (cEl) cEl.focus();
                            return;
                        }
                        let calculoSelDb = calculoSelUi.toUpperCase().trim().replace(/_/g, ' ');
                        // Ajuste puntual para cumplir CK_detalles_calculo:
                        // UI: ADELANTO_NOMINA -> DB: SALDOS INICIALES ADELANTO NOMINA
                        if (calculoSelUi === 'ADELANTO_NOMINA') calculoSelDb = 'SALDOS INICIALES ADELANTO NOMINA';
                        // Nota: En SQL el CHECK suele estar definido con valores con espacios (ej. "SALDOS INSOLUTOS"),
                        // por eso convertimos UI (con _) -> DB (con espacios) antes de enviar.
                        if (!calculoSelDb) {
                            Swal.fire('Atención', 'Selecciona un valor válido en "Cálculo".', 'warning');
                            const cEl = findField('fld_calculo'); if (cEl) cEl.focus();
                            return;
                        }




                        // =========
                        // VALIDACIÓN: detalles_del_producto.tipo_periodo (CK_detalles_tipo_periodo = COMPLETO/FIJO)
                        // =========
                        const tipoPeriodoDetalle = (findField('fld_tipo_periodo') ? String(findField('fld_tipo_periodo').value || '').trim() : '');
                        const tipoPeriodoDetalleDb = (tipoPeriodoDetalle || '').toUpperCase().trim();
                        if (!tipoPeriodoDetalleDb) {
                            Swal.fire('Atención', 'Selecciona un valor válido en "Tipo periodo (detalle)" (COMPLETO o FIJO).', 'warning');
                            return;
                        }
                        if (!tipoPeriodoDetalle || ['COMPLETO', 'FIJO'].indexOf(tipoPeriodoDetalle.toUpperCase()) === -1) {
                            Swal.fire('Atención', 'Selecciona un valor válido en "Tipo periodo (detalle)" (COMPLETO o FIJO).', 'warning');
                            const tp = findField('fld_tipo_periodo'); if (tp) tp.focus();
                            return;
                        }
                        // =========
                        // VALIDACIÓN: periodos (obligatorio en alta y requerido para cumplir handler action=create)
                        // =========
                        const periodosPayload = mpGetPeriodosPayload();
                        if (!Array.isArray(periodosPayload) || !periodosPayload.length) {
                            Swal.fire('Atención', 'Debes capturar al menos una periodicidad en la pestaña "Periodos".', 'warning');
                            try {
                                const btn = document.querySelector('button[data-bs-target="#tab-periodos"]');
                                if (btn) { const t = new bootstrap.Tab(btn); t.show(); }
                            } catch (e) { }
                            return;
                        }

                        // =========
                        // PAYLOAD JSON (conforme a producto_financiero_handler.ashx.vb)
                        // =========
                        function val(id) { const el = findField(id); return el ? (el.value == null ? '' : String(el.value).trim()) : ''; }
                        function num(id) { const v = parseNumberStrict(val(id)); return isNaN(v) ? 0 : v; }
                        // VB handler acepta null vía JSON
                        function intNullable(id) { const v = parseInt(val(id), 10); return isNaN(v) ? null : v; }

                        function boolSI(id) {
                            const v = (val(id) || '').toUpperCase();
                            if (v === 'SI' || v === 'TRUE' || v === '1') return true;
                            if (v === 'NO' || v === 'FALSE' || v === '0') return false;
                            // vacío u otro valor: false por defecto (bit NOT NULL en BD)
                            return false;
                        }
                        function int0(id) {
                            const v = parseIntStrict(val(id));
                            return isNaN(v) ? 0 : v;
                        }

                        const payloadObj = {
                            descripcion_larga: val('fld_producto'),
                            descripcion: val('fld_producto'),
                            tipo_credito_id: intNullable('fld_credito'),
                            producto_id: null, // si tu UI no maneja catálogo numérico, se queda null
                            regimen_fiscal_id: intNullable('fld_regimen'),
                            moneda_id: intNullable('fld_moneda'),

                            // Maestro: tipo_periodo se marca como 'VARIAS' (la periodicidad real vive en la tabla de periodicidades)
                            tipo_periodo: 'VARIAS',
                            plazo: null,
                            tasa_interes: null,

                            // Riesgo PLD (numérico)
                            impacto: imp,
                            probabilidad: prob,
                            nivel_riesgo_pld: niv,

                            // Observaciones generales (si existe)
                            observaciones_generales: (val('fld_notas_adicionales') || '')
                        };

                        // ====== DETALLE (dict) ======
                        payloadObj.detalle = {
                            tasa_interes_anual: num('fld_tasa_anual'),
                            tasa_interes_anual_detallada: val('fld_tasa_anual_det'),
                            tasa_interes_mensual: num('fld_tasa_mensual'),

                            iva: num('fld_iva'),
                            iva_comision: num('fld_iva_comision'),
                            iva_moratoria: num('fld_iva_moratoria'),
                            iva_sobre_total: boolSI('fld_iva_sobre_total'),

                            capital_minimo: num('fld_capital_min'),
                            capital_maximo: num('fld_capital_max'),

                            plazo_minimo: parseInt(val('fld_plazo_min') || '0', 10) || 0,
                            plazo_maximo: parseInt(val('fld_plazo_max') || '0', 10) || 0,

                            vencimiento: val('fld_vencimiento'),
                            calculo: calculoSelDb,

                            factor_moratorio: boolSI('fld_factor_moratorio'),
                            tasa_moratoria: num('fld_tasa_moratoria'),

                            dias_gracia: parseInt(val('fld_dias_gracia') || '0', 10) || 0,

                            comision_apertura: num('fld_comision_apertura'),
                            comision_apertura_detallada: val('fld_comision_apertura_det'),
                            comision_gestion: num('fld_comision_gestion'),

                            amortizar_comision_apertura: boolSI('fld_amortiza_comision_apertura'),
                            amortizar_comision_gestion: boolSI('fld_amortiza_comision_gestion'),

                            usar_redondeo_centavos: boolSI('fld_redondeo_centavos'),
                            centavos_para_redondeo: intNullable('fld_centavos_redondeo'),
                            redondear_pago_fijo: boolSI('fld_redondear_pago_fijo'),

                            aplicacion_de_tasa: val('fld_aplicacion_tasa'),
                            base_calculo: intNullable('fld_base_calculo'),
                            calculo_fecha_exigible: val('fld_calc_fecha_exigible'),

                            modificador_monto_credito: boolSI('fld_modificador_monto_credito'),

                            // Campos adicionales (bit NOT NULL) en detalles_del_producto
                            aplica_cargos_administrativos: boolSI('fld_aplica_cargos'),
                            amortizar_cargos_administrativos: boolSI('fld_amortiza_cargos'),
                            aplica_cargo_multa_cobranza: boolSI('fld_cargo_multa'),

                            aplica_comision_administracion: boolSI('fld_aplica_comision_admin'),
                            iva_comision_administracion: num('fld_iva_com_admin'),
                            comision_administracion_valor: null,
                            comision_administracion_detallada: val('fld_com_admin_det'),

                            aplica_comision_investigacion: boolSI('fld_aplica_invest'),
                            iva_comision_investigacion: num('fld_iva_com_inv'),

                            // Monto de seguro (UI select SI/NO) -> DB bit aplica_monto_seguro
                            aplica_monto_seguro: boolSI('fld_monto_seguro'),
                            comision_investigacion_detallada: val('fld_com_inv_det'),

                            notas: val('fld_notas_adicionales'),
                            tipo_periodo: tipoPeriodoDetalleDb
                        };

                        // ====== PLANEACIÓN (array) desde Prelación 1..7 (si aplica) ======
                        const planeacion = [];
                        for (let i = 1; i <= 7; i++) {
                            const v = val('fld_prelacion_' + i);
                            if (v !== '') {
                                planeacion.push({ orden: i, tipo: v, referencia_id: null, descripcion: null });
                            }
                        }
                        payloadObj.planeacion = planeacion;

                        // ====== PERIODOS (array) requerido por handler en action=create ======
                        payloadObj.periodos = periodosPayload;

                        // =========
                        // CALL
                        // =========
                        const url = isEdit
                            ? (HANDLER_URL + '?action=update&id=' + encodeURIComponent(idStr))
                            : (HANDLER_URL + '?action=create');

                        const json = await fetchJsonSafe(url, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json; charset=utf-8' },
                            body: JSON.stringify(payloadObj)
                        });
                        if (json && json.success) {
                            Swal.fire('Correcto', json.message || (isEdit ? 'Actualizado' : 'Guardado'), 'success');
                            if (bsModal) bsModal.hide();
                            if (window.catalogoTable) window.catalogoTable.draw(false);
                        } else {
                            Swal.fire('Error', (json && json.message) ? json.message : 'No se pudo guardar', 'error');
                        }
                    } catch (e) {
                        err('Guardar', e);
                        Swal.fire('Error', 'Error en guardar. Revisa consola', 'error');
                    }
                })();
            }// ==========================
            // INPUTS NUMÉRICOS & CÁLCULO
            // ==========================
            [
                'fld_impacto', 'fld_probabilidad', 'fld_nivel_riesgo', 'fld_tasa_anual', 'fld_tasa_mensual', 'fld_iva', 'fld_iva_comision',
                'fld_capital_min', 'fld_capital_max', 'fld_tasa_moratoria', 'fld_iva_moratoria', 'fld_comision_apertura',
                'fld_comision_gestion', 'fld_iva_com_admin', 'fld_iva_com_inv'
            ].forEach(id => {
                const el = findField(id); if (!el) return;
                el.addEventListener('input', function () {
                    let v = this.value; v = v.replace(/,/g, '.').replace(/[^0-9.\-]/g, '');
                    const parts = v.split('.'); if (parts.length > 2) v = parts[0] + '.' + parts.slice(1).join('');
                    this.value = v;
                });
            });
            const impEl = findField('fld_impacto'), probEl = findField('fld_probabilidad');
            if (impEl) impEl.addEventListener('change', calcNivelAutomatico);
            if (probEl) probEl.addEventListener('change', calcNivelAutomatico);
            function calcNivelAutomatico() {
                const imp = parseNumberStrict(findField('fld_impacto') ? findField('fld_impacto').value : '');
                const prob = parseNumberStrict(findField('fld_probabilidad') ? findField('fld_probabilidad').value : '');
                if (!isNaN(imp) && !isNaN(prob)) {
                    const nivel = (imp * prob) / 100;
                    const nEl = findField('fld_nivel_riesgo'); if (nEl) nEl.value = nivel.toFixed(2);
                }
            }

            // Exponer utilidades por si se necesitan
            window.PLD_Lookups = { cargarLookupsSafe, fillMonedas, fillCreditos };
            window.PLD_Prelacion = { fillAllPrelacion };

        });
    </script>


</asp:Content>
