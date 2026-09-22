<%@ Page Title="Catálogos" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid py-4 mt-4">

    <ul class="nav nav-tabs" id="catalogoTabs" role="tablist">
        <li class="nav-item">
            <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#tabCatalogos" type="button">Catálogos</button>
        </li>
        <li class="nav-item">
            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tabMatriz" type="button">Conf. Matriz</button>
        </li>
        <li class="nav-item">
            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tabRiesgo" type="button">Clasificación de Riesgo</button>
        </li>
        <li class="nav-item">
            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tabUmbrales" type="button">Umbrales PLD</button>
        </li>
        <li class="nav-item">
            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tabAlertas" type="button">Motivos de Alerta</button>
        </li>
    </ul>

    <div class="card border-secondary mt-3">
        <div class="card-body tab-content">
            <!-- Tab: Catálogos -->
            <div class="tab-pane fade show active" id="tabCatalogos">
        
                <!-- 🔴 Cliente -->
                <div class="card border-danger mb-4 mt-2">
                    <div class="card-header bg-danger text-white">Catálogos de CLIENTE</div>
                    <div class="card-body">
                        <div class="row g-2">
                            <div class="col-md-1"><a href="catalogo_actividad_economica.aspx" class="btn btn-outline-primary btn-sm w-100">Actividad Económica</a></div>
                            <div class="col-md-1"><a href="catalogo_sepomex.aspx" class="btn btn-outline-primary btn-sm w-100">Códigos postales</a></div>
                            <div class="col-md-1"><a href="catalogo_colonia.aspx" class="btn btn-outline-primary btn-sm w-100">Alta de colonias C.P.</a></div>
                            <div class="col-md-1"><a href="catalogo_fondeador.aspx" class="btn btn-outline-primary btn-sm w-100">Fuente de Fondeo</a></div>
                            <div class="col-md-1"><a href="catalogo_centro_trabajo.aspx" class="btn btn-outline-primary btn-sm w-100">Centro de trabajo</a></div>
                            <div class="col-md-1"><a href="catalogo_scoring.aspx" class="btn btn-outline-primary btn-sm w-100">Scoring</a></div>
                            <div class="col-md-1"><a href="catalogo_tipo_documentos.aspx" class="btn btn-outline-primary btn-sm w-100">Tipo de documentos</a></div>
                            <div class="col-md-1"><a href="catalogo_empresas.aspx" class="btn btn-outline-primary btn-sm w-100">Empresas</a></div>
                            <div class="col-md-1"><a href="catalogo_giro_negocio.aspx" class="btn btn-outline-primary btn-sm w-100">Giro del negocio</a></div>
                            <div class="col-md-1"><a href="catalogo_ocupacion.aspx" class="btn btn-outline-primary btn-sm w-100">Ocupación</a></div>
                            <div class="col-md-1"><a href="catalogo_medio_contacto.aspx" class="btn btn-outline-primary btn-sm w-100">Medio de contacto</a></div>
                            <div class="col-md-1"><a href="catalogo_promotores.aspx" class="btn btn-outline-primary btn-sm w-100">Promotores</a></div>
                        </div>
                    </div>
                </div>

                <!-- 🔵 PLD -->
                <div class="card border-primary mb-4">
                    <div class="card-header bg-primary text-white">Catálogos de PLD</div>
                    <div class="card-body">
                        <div class="row g-2">
                            <div class="col-md-1"><a href="catalogo_aplicacion_pago.aspx" class="btn btn-outline-primary btn-sm w-100">Aplicacion de Pago</a></div>
                            <div class="col-md-1"><a href="catalogo_canal_pago.aspx" class="btn btn-outline-primary btn-sm w-100">Canal de pagos</a></div>
                            <div class="col-md-1"><a href="catalogo_creditos.aspx" class="btn btn-outline-primary btn-sm w-100">Creditos</a></div>
                            <div class="col-md-1"><a href="catalogo_destino_recursos.aspx" class="btn btn-outline-primary btn-sm w-100">Destino de los Recursos</a></div>
                            <div class="col-md-1"><a href="catalogo_estados.aspx" class="btn btn-outline-primary btn-sm w-100">Estados</a></div>
                            <div class="col-md-1"><a href="catalogo_instrumento_monetario.aspx" class="btn btn-outline-primary btn-sm w-100">Instrumento Monetario</a></div>
                            <div class="col-md-1"><a href="catalogo_moneda_divisa.aspx" class="btn btn-outline-primary btn-sm w-100">Moneda Divisa</a></div>
                            <div class="col-md-1"><a href="catalogo_municipios.aspx" class="btn btn-outline-primary btn-sm w-100">Municipios</a></div>
                            <div class="col-md-1"><a href="catalogo_nacionalidades.aspx" class="btn btn-outline-primary btn-sm w-100">Nacionalidad</a></div>
                            <div class="col-md-1"><a href="CatalogoNichoMercado.aspx" class="btn btn-outline-primary btn-sm w-100">Nicho de Mercado</a></div>
                            <div class="col-md-1"><a href="catalogo_origen_recursos.aspx" class="btn btn-outline-primary btn-sm w-100">Origen de los recursos </a></div>
                            <div class="col-md-1"><a href="catalogo_paises.aspx" class="btn btn-outline-primary btn-sm w-100">Paises</a></div>
                            <div class="col-md-1"><a href="catalogoproductofinanciero.aspx" class="btn btn-outline-primary btn-sm w-100">Producto Financerio</a></div>
                            <div class="col-md-1"><a href="catalogo_propietario_real.aspx" class="btn btn-outline-primary btn-sm w-100">Propietario Real</a></div>
                            <div class="col-md-1"><a href="catalogo_libor.aspx" class="btn btn-outline-primary btn-sm w-100">Tasa Libor</a></div>
                            <div class="col-md-1"><a href="catalogo_tasa_tie.aspx" class="btn btn-outline-primary btn-sm w-100">Tasa TIE</a></div>
                            <div class="col-md-1"><a href="catalogo_tipo_pago.aspx" class="btn btn-outline-primary btn-sm w-100">Tipo de Pago</a></div>
                        </div>
                    </div>
                </div>

                <!-- 🟡 Listas Negras -->
                <div class="card border-warning mb-4">
                    <div class="card-header bg-warning text-dark">Catálogos de LISTAS NEGRAS</div>
                    <div class="card-body">
                        <div class="row g-2">
                            <a>En construccion. Aqui es donde se va a ligar con Quien es quien?. </a>
                        </div>
                    </div>
                </div>
            </div>

           <!-- Tab: Conf. Matriz -->
            <div class="tab-pane fade" id="tabMatriz">
                <div class="row">
                    <div class="col-12">
                        <iframe src="config_puntaje_categoria.aspx" width="100%" height="800" frameborder="0" style="border: 1px solid #ccc;"></iframe>
                    </div>
                </div>
            </div>

           <!-- Tab: Clasificación de Riesgo -->
            <div class="tab-pane fade" id="tabRiesgo">
                <div class="card border-dark mt-3">
                    <div class="card-header bg-dark text-white">
                        Clasificación de Riesgo PLD
                    </div>
                    <div class="card-body p-0" style="height: 850px;">
                        <iframe src="clasificacion_riesgo.aspx?raw=1" style="width:100%; height:100%; border:none;" loading="lazy"></iframe>
                    </div>
                </div>
            </div>

            <!-- Tab: Umbrales PLD -->
            <div class="tab-pane fade" id="tabUmbrales">

                <div class="card border-info mt-3">
                    <div class="card-header bg-info text-white d-flex align-items-center justify-content-between">
                        <div class="fw-bold">Ponderaciones <span class="badge bg-primary ms-2"><strong>P.L.D</strong></span></div>

                        <div class="d-flex gap-2 align-items-center">
                            <small class="opacity-75" id="umbrales_info_modificacion"></small>
                            <button type="button" class="btn btn-sm btn-light" id="btnGuardarUmbrales">
                                Guardar cambios
                            </button>
                        </div>
                    </div>

                    <div class="card-body">

                        <div class="row g-4">
                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Claves requeridas por CNBV para emisión de reportes
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Clave de Organo Supervisor</td>
                                                <td>
                                                    <input type="text" id="organo_supervisor" class="form-control form-control-sm UPDATE" value="01002" maxlength="6" style="text-align:right;">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Clave del Organo Sujeto Obligado</td>
                                                <td>
                                                    <input type="text" id="clave_sujeto_obligado" class="form-control form-control-sm UPDATE" value="695583" maxlength="7" style="text-align:right;">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>

                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Oficial de cumplimiento designado ante CNBV
                                </label>

                                <input type="hidden" id="id_oficial" value="16">
                                <input type="hidden" id="correo_oficial" value="ramon.dorado@c211.mx">

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 25%;"><i class="fas fa-user me-1"></i>Usuario</td>
                                                <td>
                                                    <span class="fw-bold" id="nombre_oficial_texto">MANUEL RAMON DORADO GARCIA</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;"><i class="fas fa-envelope me-1"></i>Correo</td>
                                                <td>
                                                    <input type="text" id="oficial_cumplimiento" class="form-control form-control-sm UPDATE" value="ramon.dorado@c211.mx" style="max-width: 420px;">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>
                        </div>

                        <hr class="my-4">

                        <div class="row g-4">
                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Operaciones Inusuales
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Porcentaje de pago excedente respecto a su cuota</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="pcnt_minimo_inusual_rebasa_pagos" class="form-control form-control-sm UPDATE numbersOnly" value="1500" maxlength="10" style="text-align:right; max-width: 180px;">
                                                        <span class="fw-bold">%</span>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Porcentaje de incremento en sus ingresos</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="pcnt_minimo_inusual_aumento_ingresos" class="form-control form-control-sm UPDATE numbersOnly" value="100" maxlength="10" style="text-align:right; max-width: 180px;">
                                                        <span class="fw-bold">%</span>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Seguimiento y acumulación (USD)</td>
                                                <td>
                                                    <input type="text" id="pagos_acumulacion" class="form-control form-control-sm UPDATE numbersOnly" value="500" maxlength="7" style="text-align:right; max-width: 180px;">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>

                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Operaciones Relevantes
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Personas Físicas (USD)</td>
                                                <td>
                                                    <input type="text" id="persona_fisica_usd" class="form-control form-control-sm UPDATE numbersOnly" value="7,500.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Personas Morales (USD)</td>
                                                <td>
                                                    <input type="text" id="persona_moral_usd" class="form-control form-control-sm UPDATE numbersOnly" value="7,500.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>
                        </div>

                        <hr class="my-4">

                        <div class="row g-4">
                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Bitácora de operaciones de vigilancia estricta
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td colspan="2">
                                                    <span class="badge bg-warning text-dark">
                                                        <i class="fas fa-flag me-1"></i> Acumulados en un mes calendario
                                                    </span>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Pagos superiores o iguales P.F y P.M (USD)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="minimo_usd" class="form-control form-control-sm UPDATE numbersOnly" value="100,000.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="a_pesos" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Pagos superiores o iguales P.F y P.M (MXN)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="minimo_mxnd" class="form-control form-control-sm UPDATE numbersOnly" value="1,000,000.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="a_mxn" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td colspan="2">
                                                    <span class="badge bg-warning text-dark">
                                                        <i class="fas fa-flag me-1"></i> Por operación
                                                    </span>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Personas Físicas (MXN)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="boes_personas_fisicas" class="form-control form-control-sm UPDATE numbersOnly" value="300,000.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="boes_pesos" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Personas Morales (MXN)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="boes_personas_morales" class="form-control form-control-sm UPDATE numbersOnly" value="500,000.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="boes_pm_pesos" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Personas Físicas (USD)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="boes_personas_fisicas_usd" class="form-control form-control-sm UPDATE numbersOnly" value="500.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="boes_pf_usd" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Personas Morales (USD)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="boes_personas_morales_usd" class="form-control form-control-sm UPDATE numbersOnly" value="50,000.00" maxlength="15" style="text-align:right; max-width: 180px;">
                                                        <label id="boes_pm_usd" class="fst-italic small mb-0"></label>
                                                    </div>
                                                </td>
                                            </tr>

                                        </tbody>
                                    </table>
                                </div>

                            </div>

                            <div class="col-lg-6">
                                <!-- Reservado -->
                            </div>
                        </div>

                        <hr class="my-4">

                        <div class="row g-4">
                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Monto del crédito en Unidades de Inversión (UDIs).
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Crédito máximo personas físicas (UDIS)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="max_clasificar_microcredito_fisicas_udis" class="form-control form-control-sm UPDATE numbersOnly" value="3000" maxlength="10" style="text-align:right; max-width: 180px;">
                                                        <span class="small fw-bold">UDIS</span>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Crédito máximo personas físicas con actividad empresarial (UDIS)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="max_clasificar_microcredito_pfae_udis" class="form-control form-control-sm UPDATE numbersOnly" value="10000" maxlength="10" style="text-align:right; max-width: 180px;">
                                                        <span class="small fw-bold">UDIS</span>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Crédito máximo personas morales (UDIS)</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="max_clasificar_microcredito_morales_udis" class="form-control form-control-sm UPDATE numbersOnly" value="10000" maxlength="10" style="text-align:right; max-width: 180px;">
                                                        <span class="small fw-bold">UDIS</span>
                                                    </div>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>

                            <div class="col-lg-6">

                                <label class="fw-bold" style="font-size: 16px;">
                                    <i class="fas fa-bell me-1" style="color: darkorange;"></i>
                                    Periodicidad con la que deberán emitirse los reportes
                                </label>

                                <div class="table-responsive mt-3">
                                    <table class="table table-sm align-middle">
                                        <tbody>
                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B; width: 55%;">Operaciones inusuales</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="periodicidad_dias_operaciones_inusuales" class="form-control form-control-sm UPDATE numbersOnly" value="60" maxlength="5" style="text-align:right; max-width: 180px;">
                                                        <span class="small fw-bold">Días naturales</span>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Internas preocupantes</td>
                                                <td>
                                                    <div class="d-flex align-items-center gap-2">
                                                        <input type="text" id="periodicidad_dias_internas_preocupantes" class="form-control form-control-sm UPDATE numbersOnly" value="60" maxlength="5" style="text-align:right; max-width: 180px;">
                                                        <span class="small fw-bold">Días naturales</span>
                                                    </div>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="fw-bold" style="color:#3F729B;">Operaciones relevantes</td>
                                                <td>
                                                    <span class="fw-bold">Últimos 10 días hábiles de enero, abril, julio y octubre.</span>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>

                            </div>
                        </div>

                    </div>
                </div>

            </div>

            <!-- Tab: Motivos de Alerta -->
            <div class="tab-pane fade" id="tabAlertas">
                <div class="alert alert-info">En construcción...</div>
            </div>
        </div>
    </div>
</div>

<script>
    document.addEventListener("DOMContentLoaded", function () {

        var HANDLER_URL = "/handlers/handler_config_umbrales_pld.ashx";

        function fireAlert(title, text, icon) {
            try {
                if (window.Swal && Swal.fire) {
                    Swal.fire({ title: title, text: text, icon: icon || "info" });
                } else {
                    alert(title + "\n\n" + text);
                }
            } catch (e) {
                alert(title + "\n\n" + text);
            }
        }

        // Convierte /Date(1768926456830)/ o ISO a texto legible
        function formatServerDate(val) {
            if (val === null || typeof val === "undefined") return "";

            if (typeof val === "string") {
                var m = /\/Date\((\d+)\)\//.exec(val);
                if (m && m[1]) {
                    var ms = parseInt(m[1], 10);
                    if (!isNaN(ms)) {
                        var d1 = new Date(ms);
                        return d1.toLocaleString("es-MX", {
                            year: "numeric", month: "2-digit", day: "2-digit",
                            hour: "2-digit", minute: "2-digit"
                        });
                    }
                }
                var d2 = new Date(val);
                if (!isNaN(d2.getTime())) {
                    return d2.toLocaleString("es-MX", {
                        year: "numeric", month: "2-digit", day: "2-digit",
                        hour: "2-digit", minute: "2-digit"
                    });
                }
                return val;
            }

            try {
                var d3 = new Date(val);
                if (!isNaN(d3.getTime())) {
                    return d3.toLocaleString("es-MX", {
                        year: "numeric", month: "2-digit", day: "2-digit",
                        hour: "2-digit", minute: "2-digit"
                    });
                }
            } catch (e) { }

            return "";
        }

        // Si en el sistema ya existen, NO las pisamos.
        if (typeof window.validateInput !== "function") {
            window.validateInput = function (el) {
                if (!el) return;
                var v = (el.value || "").toString();
                if (el.id === "oficial_cumplimiento" || el.id === "organo_supervisor" || el.id === "clave_sujeto_obligado") return;
                el.value = v.replace(/[^\d.,-]/g, "");
            };
        }
        if (typeof window.formatInput !== "function") {
            window.formatInput = function (el) {
                if (!el) return;
                var n = parseDecimal(el.value);
                el.value = formatDecimal(n);
            };
        }
        if (typeof window.formatIntegerInput !== "function") {
            window.formatIntegerInput = function (el) {
                if (!el) return;
                var n = parseIntSafe(el.value);
                el.value = (isNaN(n) ? 0 : n).toString();
            };
        }

        function parseDecimal(val) {
            var raw = (val || "").toString().trim();
            if (raw === "") return 0;

            raw = raw.replace(/\s/g, "").replace(/%/g, "");
            var hasDot = raw.indexOf(".") >= 0;
            var hasComma = raw.indexOf(",") >= 0;

            if (hasDot && hasComma) {
                raw = raw.replace(/,/g, "");
            } else if (!hasDot && hasComma) {
                raw = raw.replace(/,/g, ".");
            }

            var num = parseFloat(raw);
            if (isNaN(num) || !isFinite(num)) return 0;
            if (num < 0) num = 0;
            return num;
        }

        function formatDecimal(num) {
            try {
                var n = parseFloat(num);
                if (isNaN(n) || !isFinite(n)) n = 0;
                if (n < 0) n = 0;
                return n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            } catch (e) {
                return "0.00";
            }
        }

        function parseIntSafe(val) {
            var raw = (val || "").toString().trim();
            if (raw === "") return 0;
            raw = raw.replace(/\s/g, "").replace(/,/g, "").replace(/%/g, "");
            var n = parseInt(raw, 10);
            if (isNaN(n) || !isFinite(n)) return 0;
            if (n < 0) n = 0;
            return n;
        }

        function getEl(id) { return document.getElementById(id); }

        function setVal(id, value) {
            var el = getEl(id);
            if (!el) return;
            if (value === null || typeof value === "undefined") return;
            el.value = value;
        }

        function collectPayload() {
            var payload = new URLSearchParams();
            payload.append("action", "guardar");

            payload.append("organo_supervisor", (getEl("organo_supervisor").value || "").trim());
            payload.append("clave_sujeto_obligado", (getEl("clave_sujeto_obligado").value || "").trim());

            payload.append("id_oficial", (getEl("id_oficial").value || "").trim());
            payload.append("correo_oficial", (getEl("correo_oficial").value || "").trim());
            payload.append("oficial_cumplimiento", (getEl("oficial_cumplimiento").value || "").trim());

            payload.append("pcnt_minimo_inusual_rebasa_pagos", parseDecimal(getEl("pcnt_minimo_inusual_rebasa_pagos").value).toString());
            payload.append("pcnt_minimo_inusual_aumento_ingresos", parseDecimal(getEl("pcnt_minimo_inusual_aumento_ingresos").value).toString());
            payload.append("pagos_acumulacion", parseDecimal(getEl("pagos_acumulacion").value).toString());

            payload.append("persona_fisica_usd", parseDecimal(getEl("persona_fisica_usd").value).toString());
            payload.append("persona_moral_usd", parseDecimal(getEl("persona_moral_usd").value).toString());

            payload.append("minimo_usd", parseDecimal(getEl("minimo_usd").value).toString());
            payload.append("minimo_mxnd", parseDecimal(getEl("minimo_mxnd").value).toString());

            payload.append("boes_personas_fisicas", parseDecimal(getEl("boes_personas_fisicas").value).toString());
            payload.append("boes_personas_morales", parseDecimal(getEl("boes_personas_morales").value).toString());
            payload.append("boes_personas_fisicas_usd", parseDecimal(getEl("boes_personas_fisicas_usd").value).toString());
            payload.append("boes_personas_morales_usd", parseDecimal(getEl("boes_personas_morales_usd").value).toString());

            payload.append("max_clasificar_microcredito_fisicas_udis", parseIntSafe(getEl("max_clasificar_microcredito_fisicas_udis").value).toString());
            payload.append("max_clasificar_microcredito_pfae_udis", parseIntSafe(getEl("max_clasificar_microcredito_pfae_udis").value).toString());
            payload.append("max_clasificar_microcredito_morales_udis", parseIntSafe(getEl("max_clasificar_microcredito_morales_udis").value).toString());

            payload.append("periodicidad_dias_operaciones_inusuales", parseIntSafe(getEl("periodicidad_dias_operaciones_inusuales").value).toString());
            payload.append("periodicidad_dias_internas_preocupantes", parseIntSafe(getEl("periodicidad_dias_internas_preocupantes").value).toString());

            return payload;
        }

        function applyLoaded(data) {
            if (!data) return;

            setVal("organo_supervisor", data.organo_supervisor);
            setVal("clave_sujeto_obligado", data.clave_sujeto_obligado);

            if (data.id_oficial !== null && typeof data.id_oficial !== "undefined") setVal("id_oficial", data.id_oficial);
            if (data.correo_oficial !== null && typeof data.correo_oficial !== "undefined") setVal("correo_oficial", data.correo_oficial);
            if (data.oficial_cumplimiento !== null && typeof data.oficial_cumplimiento !== "undefined") setVal("oficial_cumplimiento", data.oficial_cumplimiento);

            setVal("pcnt_minimo_inusual_rebasa_pagos", (data.pcnt_minimo_inusual_rebasa_pagos != null ? data.pcnt_minimo_inusual_rebasa_pagos : ""));
            setVal("pcnt_minimo_inusual_aumento_ingresos", (data.pcnt_minimo_inusual_aumento_ingresos != null ? data.pcnt_minimo_inusual_aumento_ingresos : ""));
            setVal("pagos_acumulacion", (data.pagos_acumulacion != null ? data.pagos_acumulacion : ""));

            setVal("persona_fisica_usd", (data.persona_fisica_usd != null ? data.persona_fisica_usd : ""));
            setVal("persona_moral_usd", (data.persona_moral_usd != null ? data.persona_moral_usd : ""));

            setVal("minimo_usd", (data.minimo_usd != null ? data.minimo_usd : ""));
            setVal("minimo_mxnd", (data.minimo_mxnd != null ? data.minimo_mxnd : ""));

            setVal("boes_personas_fisicas", (data.boes_personas_fisicas != null ? data.boes_personas_fisicas : ""));
            setVal("boes_personas_morales", (data.boes_personas_morales != null ? data.boes_personas_morales : ""));
            setVal("boes_personas_fisicas_usd", (data.boes_personas_fisicas_usd != null ? data.boes_personas_fisicas_usd : ""));
            setVal("boes_personas_morales_usd", (data.boes_personas_morales_usd != null ? data.boes_personas_morales_usd : ""));

            setVal("max_clasificar_microcredito_fisicas_udis", (data.max_clasificar_microcredito_fisicas_udis != null ? data.max_clasificar_microcredito_fisicas_udis : ""));
            setVal("max_clasificar_microcredito_pfae_udis", (data.max_clasificar_microcredito_pfae_udis != null ? data.max_clasificar_microcredito_pfae_udis : ""));
            setVal("max_clasificar_microcredito_morales_udis", (data.max_clasificar_microcredito_morales_udis != null ? data.max_clasificar_microcredito_morales_udis : ""));

            setVal("periodicidad_dias_operaciones_inusuales", (data.periodicidad_dias_operaciones_inusuales != null ? data.periodicidad_dias_operaciones_inusuales : ""));
            setVal("periodicidad_dias_internas_preocupantes", (data.periodicidad_dias_internas_preocupantes != null ? data.periodicidad_dias_internas_preocupantes : ""));

            // Formateo visual decimales
            [
                "pagos_acumulacion", "persona_fisica_usd", "persona_moral_usd",
                "minimo_usd", "minimo_mxnd",
                "boes_personas_fisicas", "boes_personas_morales", "boes_personas_fisicas_usd", "boes_personas_morales_usd"
            ].forEach(function (id) {
                var el = getEl(id);
                if (!el) return;
                el.value = formatDecimal(parseDecimal(el.value));
            });

            // Info modificación (formato humano)
            var info = getEl("umbrales_info_modificacion");
            if (info) {
                var f = formatServerDate(data.fecha_modificacion);
                info.textContent = f
                    ? ("Última modificación: " + f + (data.modificado_por ? (" (" + data.modificado_por + ")") : ""))
                    : "";
            }
        }

        function loadUmbrales() {
            fetch(HANDLER_URL + "?action=obtener", { method: "GET", credentials: "same-origin" })
                .then(function (r) { return r.json(); })
                .then(function (resp) {
                    if (!resp || resp.ok !== true) {
                        fireAlert("Umbrales PLD", (resp && resp.msg ? resp.msg : "No se pudo cargar la configuración."), "error");
                        return;
                    }
                    applyLoaded(resp.data);
                })
                .catch(function () {
                    fireAlert("Umbrales PLD", "Error de red al cargar la configuración.", "error");
                });
        }

        function saveUmbrales() {
            var btn = getEl("btnGuardarUmbrales");
            if (btn) btn.disabled = true;

            var payload = collectPayload();

            fetch(HANDLER_URL, {
                method: "POST",
                credentials: "same-origin",
                headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                body: payload.toString()
            })
                .then(function (r) { return r.json(); })
                .then(function (resp) {
                    if (!resp || resp.ok !== true) {
                        fireAlert("Umbrales PLD", (resp && resp.msg ? resp.msg : "No se pudo guardar."), "error");
                        return;
                    }
                    fireAlert("Umbrales PLD", (resp.msg || "Guardado correcto."), "success");
                    loadUmbrales();
                })
                .catch(function () {
                    fireAlert("Umbrales PLD", "Error de red al guardar.", "error");
                })
                .finally(function () {
                    if (btn) btn.disabled = false;
                });
        }

        // Eventos numéricos
        document.querySelectorAll("#tabUmbrales .numbersOnly").forEach(function (el) {
            el.addEventListener("keyup", function () { validateInput(el); });
            el.addEventListener("blur", function () {
                var isInt = (
                    el.id.indexOf("_udis") >= 0 ||
                    el.id.indexOf("periodicidad_") >= 0 ||
                    el.id.indexOf("pcnt_minimo_") >= 0
                );
                if (isInt) formatIntegerInput(el); else formatInput(el);
            });
        });

        var btnSave = getEl("btnGuardarUmbrales");
        if (btnSave) btnSave.addEventListener("click", saveUmbrales);

        loadUmbrales();
    });
</script>

</asp:Content>
