Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net
Imports System.Text

Public Class handler_pagos_credito
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = ObtenerParametro(context, "action").Trim().ToLowerInvariant()

            If String.IsNullOrWhiteSpace(action) Then
                action = ObtenerParametro(context, "accion").Trim().ToLowerInvariant()
            End If

            Select Case action
                Case "consultar"
                    respuesta = ConsultarPagos(context)

                Case "obtener"
                    respuesta = ObtenerPago(context)

                Case "catalogos"
                    respuesta = ObtenerCatalogos()

                Case "buscar_referencias"
                    respuesta = BuscarReferencias(context)

                Case "guardar_aplicar"
                    respuesta = GuardarAplicarPago(context)

                Case "cancelar"
                    respuesta = CancelarPago(context)

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_pagos_credito."
            respuesta("detalle") = ex.Message
        End Try

        context.Response.Write(serializer.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Function CadenaConexion() As String
        Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
    End Function

    Private Function ObtenerParametro(ByVal context As HttpContext, ByVal nombre As String) As String
        If context.Request(nombre) IsNot Nothing Then
            Return Convert.ToString(context.Request(nombre))
        End If

        Return ""
    End Function

    Private Function ObtenerUsuario(ByVal context As HttpContext) As String
        Dim usuario As String = ""

        If context.Session IsNot Nothing AndAlso context.Session("usuario") IsNot Nothing Then
            usuario = Convert.ToString(context.Session("usuario"))
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = ObtenerParametro(context, "usuario")
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = "SISTEMA"
        End If

        Return usuario.Trim()
    End Function

    Private Function ToInt(ByVal valor As Object, Optional ByVal defaultValue As Integer = 0) As Integer
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return defaultValue

        Dim resultado As Integer = defaultValue
        Integer.TryParse(Convert.ToString(valor), resultado)

        Return resultado
    End Function

    Private Function ToDecimal(ByVal valor As Object, Optional ByVal defaultValue As Decimal = 0D) As Decimal
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return defaultValue

        Dim texto As String = Convert.ToString(valor).Trim()

        If String.IsNullOrWhiteSpace(texto) Then Return defaultValue

        texto = texto.Replace(",", ".")

        Dim resultado As Decimal = defaultValue

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, resultado) Then
            Return resultado
        End If

        Return defaultValue
    End Function

    Private Function Money2(ByVal valor As Decimal) As Decimal
        Return Math.Round(valor, 2, MidpointRounding.AwayFromZero)
    End Function


    Private Function ToDateTimeNullable(ByVal valor As Object) As DateTime?
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return Nothing

        Dim texto As String = Convert.ToString(valor).Trim()

        If String.IsNullOrWhiteSpace(texto) Then Return Nothing

        Dim fecha As DateTime

        If DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha) Then
            Return fecha
        End If

        If DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.None, fecha) Then
            Return fecha
        End If

        Return Nothing
    End Function

    Private Function ToBool(ByVal valor As Object, Optional ByVal defaultValue As Boolean = False) As Boolean
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return defaultValue

        Dim texto As String = Convert.ToString(valor).Trim().ToLowerInvariant()

        If texto = "1" OrElse texto = "true" OrElse texto = "si" OrElse texto = "sí" Then
            Return True
        End If

        If texto = "0" OrElse texto = "false" OrElse texto = "no" Then
            Return False
        End If

        Return defaultValue
    End Function

    Private Function DbValue(ByVal valor As Object) As Object
        If valor Is Nothing Then Return DBNull.Value

        If TypeOf valor Is String AndAlso String.IsNullOrWhiteSpace(Convert.ToString(valor)) Then
            Return DBNull.Value
        End If

        Return valor
    End Function

    Private Function TablaALista(ByVal dt As DataTable) As List(Of Dictionary(Of String, Object))
        Dim lista As New List(Of Dictionary(Of String, Object))()

        For Each fila As DataRow In dt.Rows
            Dim item As New Dictionary(Of String, Object)()

            For Each col As DataColumn In dt.Columns
                If fila(col) Is DBNull.Value Then
                    item(col.ColumnName) = Nothing
                Else
                    item(col.ColumnName) = fila(col)
                End If
            Next

            lista.Add(item)
        Next

        Return lista
    End Function

    Private Function EjecutarTabla(ByVal sql As String, ByVal parametros As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()

        Using cn As New SqlConnection(CadenaConexion())
            Using cmd As New SqlCommand(sql, cn)
                cmd.CommandType = CommandType.Text

                If parametros IsNot Nothing Then
                    cmd.Parameters.AddRange(parametros.ToArray())
                End If

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        Return dt
    End Function

    Private Function EjecutarTablaTransaccion(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal sql As String, ByVal parametros As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.CommandType = CommandType.Text

            If parametros IsNot Nothing Then
                cmd.Parameters.AddRange(parametros.ToArray())
            End If

            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using

        Return dt
    End Function

    Private Function ConsultarPagos(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_credito_id"), 0)
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim estatus As String = ObtenerParametro(context, "estatus").Trim()
        Dim texto As String = ObtenerParametro(context, "q").Trim()

        Dim parametros As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE pc.activo = 1 "

        If solicitudId > 0 Then
            whereSql &= "AND pc.solicitud_credito_id = @solicitud_credito_id "
            parametros.Add(New SqlParameter("@solicitud_credito_id", solicitudId))
        End If

        If clienteId > 0 Then
            whereSql &= "AND ISNULL(pc.cliente_id, 0) = @cliente_id "
            parametros.Add(New SqlParameter("@cliente_id", clienteId))
        End If

        If Not String.IsNullOrWhiteSpace(estatus) Then
            whereSql &= "AND pc.estatus = @estatus "
            parametros.Add(New SqlParameter("@estatus", estatus))
        End If

        If Not String.IsNullOrWhiteSpace(texto) Then
            whereSql &= "AND (pc.folio_pago LIKE @q OR pc.referencia_pago LIKE @q OR sc.estatus LIKE @q) "
            parametros.Add(New SqlParameter("@q", "%" & texto & "%"))
        End If

        Dim sql As String =
            "SELECT TOP 500 " &
            "pc.id, pc.solicitud_credito_id, pc.cliente_id, pc.folio_pago, pc.referencia_pago, " &
            "pc.fecha_pago, pc.fecha_registro, pc.monto_pago, pc.tipo_cambio, pc.monto_equivalente_mxn, pc.monto_equivalente_usd, " &
            "pc.saldo_antes_pago, pc.saldo_despues_pago, pc.pago_fijo_contractual, pc.monto_credito_original, " &
            "pc.es_efectivo, pc.es_moneda_extranjera, pc.es_pago_excedente, pc.es_liquidacion, pc.es_liquidacion_anticipada, " &
            "pc.requiere_devolucion, pc.estatus, pc.activo, " &
            "tp.descripcion AS tipo_pago, cp.canal AS canal_pago, md.moneda, md.clave AS moneda_clave, " &
            "sc.monto_solicitado, sc.plazo, sc.estatus AS estatus_solicitud " &
            "FROM dbo.pagos_credito pc " &
            "INNER JOIN dbo.solicitud_credito sc ON sc.id = pc.solicitud_credito_id " &
            "LEFT JOIN dbo.catalogo_tipo_pago tp ON tp.id = pc.tipo_pago_id " &
            "LEFT JOIN dbo.catalogo_canal_pago cp ON cp.id = pc.canal_pago_id " &
            "LEFT JOIN dbo.catalogo_moneda_divisa md ON md.id = pc.moneda_id " &
            whereSql &
            "ORDER BY pc.fecha_pago DESC, pc.id DESC;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function ObtenerPago(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim id As Integer = ToInt(ObtenerParametro(context, "id"), 0)

        If id <= 0 Then
            id = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        End If

        If id <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de pago inválido."
            Return respuesta
        End If

        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_pagos_credito_pld " &
            "WHERE pago_credito_id = @id;"

        Dim dt As DataTable = EjecutarTabla(sql, New List(Of SqlParameter) From {
            New SqlParameter("@id", id)
        })

        If dt.Rows.Count = 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "No se encontró el pago."
            Return respuesta
        End If

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)(0)

        Return respuesta
    End Function

    Private Function ObtenerCatalogos() As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim data As New Dictionary(Of String, Object)()

        data("tipo_pago") = TablaALista(EjecutarTabla(
            "SELECT id, descripcion, estatus FROM dbo.catalogo_tipo_pago WHERE estatus = 1 ORDER BY descripcion;",
            Nothing
        ))

        data("canal_pago") = TablaALista(EjecutarTabla(
            "SELECT id, canal, activo FROM dbo.catalogo_canal_pago WHERE activo = 1 ORDER BY canal;",
            Nothing
        ))

        data("monedas") = TablaALista(EjecutarTabla(
            "SELECT id, moneda, clave, activo, estatus FROM dbo.catalogo_moneda_divisa WHERE ISNULL(estatus, ISNULL(activo, 1)) = 1 ORDER BY id;",
            Nothing
        ))

        data("aplicacion_pago") = TablaALista(EjecutarTabla(
            "SELECT id, descripcion, subtipo, tipo_credito, tipo_regimen, activo FROM dbo.catalogo_aplicacion_pago WHERE activo = 1 ORDER BY descripcion;",
            Nothing
        ))

        respuesta("ok") = True
        respuesta("data") = data

        Return respuesta
    End Function

    Private Function BuscarReferencias(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim texto As String = ObtenerParametro(context, "q").Trim()
        Dim modo As String = ObtenerParametro(context, "modo").Trim().ToLowerInvariant()
        Dim limite As Integer = ToInt(ObtenerParametro(context, "limite"), 20)

        If texto.Length < 2 Then
            respuesta("ok") = True
            respuesta("data") = New List(Of Dictionary(Of String, Object))()
            Return respuesta
        End If
        If limite <= 0 Then limite = 20
        If limite > 20 Then limite = 20

        Dim incluirClientes As Boolean = (modo = "filtro")
        Dim sql As String =
            ";WITH referencias AS (" &
            " SELECT CAST(N'credito' AS nvarchar(10)) AS tipo, sc.id AS solicitud_credito_id, sc.cliente_id, " &
            " LTRIM(RTRIM(REPLACE(REPLACE(CONCAT(ISNULL(c.primer_nombre,''),' ',ISNULL(c.segundo_nombre,''),' ',ISNULL(c.apellido_paterno,''),' ',ISNULL(c.apellido_materno,'')),'  ',' '),'  ',' '))) AS cliente_nombre, " &
            " c.rfc, c.curp, sc.monto_solicitado, " &
            " CAST(CASE WHEN ISNULL(cc.es_revolvente,0)=1 THEN ISNULL(vr.saldo_utilizado,0) ELSE COALESCE(ultimo.saldo_despues_pago,sc.monto_solicitado,0) END AS decimal(18,2)) AS saldo_vigente, " &
            " sc.estatus AS estatus_solicitud, sc.plazo, sc.moneda_id, md.moneda, md.clave AS moneda_clave, sc.canal_pago_id, cp.canal AS canal_pago, " &
            " ISNULL(secuencia.ultimo_numero,0)+1 AS siguiente_numero_pago, cc.nombre_credito AS tipo_credito, ISNULL(cc.es_revolvente,0) AS es_revolvente, " &
            " sc.monto_autorizado, vr.disponible, " &
            " CASE WHEN TRY_CONVERT(int,@qExacta)=sc.id THEN 0 WHEN TRY_CONVERT(int,@qExacta)=sc.cliente_id THEN 1 " &
            "      WHEN UPPER(ISNULL(c.rfc,''))=@qExactaUpper OR UPPER(ISNULL(c.curp,''))=@qExactaUpper THEN 2 ELSE 3 END AS orden " &
            " FROM dbo.solicitud_credito sc " &
            " LEFT JOIN dbo.cliente_persona_fisica c ON c.id_cliente=sc.cliente_id " &
            " LEFT JOIN dbo.catalogo_producto_financiero pf ON pf.id=sc.producto_financiero_id " &
            " LEFT JOIN dbo.catalogo_creditos cc ON cc.id=pf.tipo_credito_id " &
            " LEFT JOIN dbo.vw_credito_revolvente_saldo vr ON vr.solicitud_credito_id=sc.id " &
            " LEFT JOIN dbo.catalogo_moneda_divisa md ON md.id=sc.moneda_id " &
            " LEFT JOIN dbo.catalogo_canal_pago cp ON cp.id=sc.canal_pago_id " &
            " OUTER APPLY (SELECT TOP 1 pc.saldo_despues_pago FROM dbo.pagos_credito pc WHERE pc.solicitud_credito_id=sc.id AND pc.activo=1 AND pc.estatus=N'APLICADO' ORDER BY pc.fecha_pago DESC,pc.id DESC) ultimo " &
            " OUTER APPLY (SELECT MAX(pc.numero_pago_en_credito) AS ultimo_numero FROM dbo.pagos_credito pc WHERE pc.solicitud_credito_id=sc.id AND pc.activo=1 AND pc.estatus=N'APLICADO') secuencia " &
            " WHERE sc.activo=1 AND (CONVERT(varchar(20),sc.id) LIKE @qLike OR CONVERT(varchar(20),ISNULL(sc.cliente_id,0)) LIKE @qLike " &
            " OR UPPER(ISNULL(c.rfc,'')) LIKE @qLikeUpper OR UPPER(ISNULL(c.curp,'')) LIKE @qLikeUpper " &
            " OR UPPER(LTRIM(RTRIM(REPLACE(REPLACE(CONCAT(ISNULL(c.primer_nombre,''),' ',ISNULL(c.segundo_nombre,''),' ',ISNULL(c.apellido_paterno,''),' ',ISNULL(c.apellido_materno,'')),'  ',' '),'  ',' ')))) LIKE @qLikeUpper) " &
            " UNION ALL " &
            " SELECT CAST(N'cliente' AS nvarchar(10)), CAST(NULL AS int), c.id_cliente, " &
            " LTRIM(RTRIM(REPLACE(REPLACE(CONCAT(ISNULL(c.primer_nombre,''),' ',ISNULL(c.segundo_nombre,''),' ',ISNULL(c.apellido_paterno,''),' ',ISNULL(c.apellido_materno,'')),'  ',' '),'  ',' '))), " &
            " c.rfc,c.curp,CAST(NULL AS decimal(18,2)),CAST(NULL AS decimal(18,2)),CAST(NULL AS nvarchar(30)),CAST(NULL AS int),CAST(NULL AS int),CAST(NULL AS nvarchar(200)),CAST(NULL AS nvarchar(50)),CAST(NULL AS int),CAST(NULL AS nvarchar(200)),CAST(NULL AS int), " &
            " CAST(NULL AS nvarchar(150)),CAST(0 AS bit),CAST(NULL AS decimal(18,2)),CAST(NULL AS decimal(18,2)), " &
            " CASE WHEN TRY_CONVERT(int,@qExacta)=c.id_cliente THEN 0 WHEN UPPER(ISNULL(c.rfc,''))=@qExactaUpper OR UPPER(ISNULL(c.curp,''))=@qExactaUpper THEN 1 ELSE 4 END " &
            " FROM dbo.cliente_persona_fisica c WHERE @incluirClientes=1 " &
            " AND EXISTS (SELECT 1 FROM dbo.solicitud_credito scx WHERE scx.cliente_id=c.id_cliente AND scx.activo=1) " &
            " AND (CONVERT(varchar(20),c.id_cliente) LIKE @qLike OR UPPER(ISNULL(c.rfc,'')) LIKE @qLikeUpper OR UPPER(ISNULL(c.curp,'')) LIKE @qLikeUpper " &
            " OR UPPER(LTRIM(RTRIM(REPLACE(REPLACE(CONCAT(ISNULL(c.primer_nombre,''),' ',ISNULL(c.segundo_nombre,''),' ',ISNULL(c.apellido_paterno,''),' ',ISNULL(c.apellido_materno,'')),'  ',' '),'  ',' ')))) LIKE @qLikeUpper)" &
            ") SELECT TOP (@limite) tipo,solicitud_credito_id,cliente_id,cliente_nombre,rfc,curp,monto_solicitado,saldo_vigente,estatus_solicitud,plazo,moneda_id,moneda,moneda_clave,canal_pago_id,canal_pago,siguiente_numero_pago,tipo_credito,es_revolvente,monto_autorizado,disponible " &
            "FROM referencias ORDER BY orden,CASE WHEN tipo=N'cliente' THEN 0 ELSE 1 END,cliente_nombre,solicitud_credito_id DESC;"

        Dim parametros As New List(Of SqlParameter) From {
            New SqlParameter("@qExacta", SqlDbType.NVarChar, 100) With {.Value = texto},
            New SqlParameter("@qExactaUpper", SqlDbType.NVarChar, 100) With {.Value = texto.ToUpperInvariant()},
            New SqlParameter("@qLike", SqlDbType.NVarChar, 110) With {.Value = "%" & texto & "%"},
            New SqlParameter("@qLikeUpper", SqlDbType.NVarChar, 110) With {.Value = "%" & texto.ToUpperInvariant() & "%"},
            New SqlParameter("@incluirClientes", SqlDbType.Bit) With {.Value = incluirClientes},
            New SqlParameter("@limite", SqlDbType.Int) With {.Value = limite}
        }
        respuesta("ok") = True
        respuesta("data") = TablaALista(EjecutarTabla(sql, parametros))
        Return respuesta
    End Function

    Private Function GuardarAplicarPago(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim usuario As String = ObtenerUsuario(context)

        Dim nuevoPagoId As Integer = 0

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_credito_id"), 0)

                    If solicitudId <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "La solicitud es obligatoria."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim solicitud As DataRow = ObtenerSolicitud(cn, tr, solicitudId)

                    If solicitud Is Nothing Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró la solicitud activa."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim tipoPagoId As Integer = ToInt(ObtenerParametro(context, "tipo_pago_id"), 0)
                    Dim canalPagoId As Integer = ToInt(ObtenerParametro(context, "canal_pago_id"), ToInt(solicitud("canal_pago_id"), 0))
                    Dim monedaId As Integer = ToInt(ObtenerParametro(context, "moneda_id"), ToInt(solicitud("moneda_id"), 0))
                    Dim aplicacionPagoId As Integer = ToInt(ObtenerParametro(context, "aplicacion_pago_id"), 0)

                    Dim montoPago As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_pago"), 0D))
                    Dim tipoCambio As Decimal = ToDecimal(ObtenerParametro(context, "tipo_cambio"), 1D)

                    If tipoPagoId <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El tipo de pago es obligatorio."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If canalPagoId <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El canal de pago es obligatorio."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If monedaId <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "La moneda es obligatoria."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If montoPago <= 0D Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El monto del pago debe ser mayor a cero."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If tipoCambio <= 0D Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El tipo de cambio debe ser mayor a cero."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim clienteSolicitudId As Integer = ToInt(solicitud("cliente_id"), 0)
                    Dim clienteSolicitadoId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), clienteSolicitudId)

                    If clienteSolicitadoId > 0 AndAlso clienteSolicitudId > 0 AndAlso clienteSolicitadoId <> clienteSolicitudId Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El cliente seleccionado no corresponde a la solicitud de crédito."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim clienteId As Integer = clienteSolicitudId
                    Dim fechaPago As DateTime = DateTime.Now

                    Dim fechaCapturada As DateTime? = ToDateTimeNullable(ObtenerParametro(context, "fecha_pago"))

                    If fechaCapturada.HasValue Then
                        fechaPago = fechaCapturada.Value
                    End If

                    Dim esRevolvente As Boolean = ToBool(solicitud("es_revolvente"), False)
                    Dim montoCreditoOriginal As Decimal = ToDecimal(ObtenerParametro(context, "monto_credito_original"), ToDecimal(solicitud("monto_solicitado"), 0D))
                    Dim saldoAntes As Decimal = ToDecimal(ObtenerParametro(context, "saldo_antes_pago"), montoCreditoOriginal)
                    Dim montoCapital As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_capital"), montoPago))
                    Dim saldoDespues As Decimal = 0D

                    If esRevolvente Then
                        Dim estatusSolicitud As String = Convert.ToString(solicitud("estatus"), CultureInfo.InvariantCulture).Trim().ToUpperInvariant()
                        If estatusSolicitud <> "FINALIZADA" Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "La solicitud revolvente debe estar FINALIZADA antes de registrar pagos."
                            tr.Rollback()
                            Return respuesta
                        End If

                        If solicitud.IsNull("monto_autorizado") Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "La línea revolvente no tiene límite autorizado configurado."
                            tr.Rollback()
                            Return respuesta
                        End If
                        montoCreditoOriginal = Money2(Convert.ToDecimal(solicitud("monto_autorizado"), CultureInfo.InvariantCulture))
                        saldoAntes = Money2(ObtenerSaldoUtilizadoRevolvente(cn, tr, solicitudId))

                        If saldoAntes < 0D Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "La línea presenta capital amortizado mayor al capital dispuesto. Debe corregirse antes de registrar más pagos."
                            tr.Rollback()
                            Return respuesta
                        End If

                        If montoCapital < 0D OrElse montoCapital > montoPago Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "El capital aplicado debe estar entre cero y el monto total del pago."
                            tr.Rollback()
                            Return respuesta
                        End If
                        If montoCapital > saldoAntes Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "El capital aplicado no puede exceder el capital utilizado actual de la línea."
                            tr.Rollback()
                            Return respuesta
                        End If

                        Dim monedaCreditoId As Integer = ToInt(solicitud("moneda_id"), 0)
                        If montoCapital > 0D AndAlso monedaCreditoId > 0 AndAlso monedaId <> monedaCreditoId Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "No se puede aplicar capital a una línea revolvente con una moneda distinta a la moneda del crédito sin una regla de conversión aprobada."
                            tr.Rollback()
                            Return respuesta
                        End If

                        Dim saldoEnFechaPago As Decimal = Money2(ObtenerSaldoUtilizadoRevolventeEnFecha(cn, tr, solicitudId, fechaPago))
                        If saldoEnFechaPago < 0D Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "La secuencia histórica de movimientos ya presenta un saldo negativo en la fecha indicada."
                            tr.Rollback()
                            Return respuesta
                        End If
                        If montoCapital > saldoEnFechaPago Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "El capital aplicado excede el saldo utilizado que existía en la fecha del pago."
                            tr.Rollback()
                            Return respuesta
                        End If

                        saldoDespues = Money2(saldoAntes - montoCapital)
                    Else
                        saldoDespues = ToDecimal(ObtenerParametro(context, "saldo_despues_pago"), saldoAntes - montoCapital)
                        If saldoDespues < 0D Then saldoDespues = 0D
                    End If

                    Dim montoInteres As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_interes"), 0D))
                    Dim montoIva As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_iva"), 0D))
                    Dim montoMoratorio As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_moratorio"), 0D))
                    Dim montoComisiones As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_comisiones"), 0D))
                    Dim montoOtros As Decimal = Money2(ToDecimal(ObtenerParametro(context, "monto_otros"), 0D))
                    Dim pagoFijoContractual As Decimal = Money2(ToDecimal(ObtenerParametro(context, "pago_fijo_contractual"), 0D))

                    If montoInteres < 0D OrElse montoIva < 0D OrElse montoMoratorio < 0D OrElse montoComisiones < 0D OrElse montoOtros < 0D Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Los componentes del pago no pueden ser negativos."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim montoEquivalenteMxn As Decimal = ToDecimal(ObtenerParametro(context, "monto_equivalente_mxn"), montoPago * tipoCambio)
                    Dim montoEquivalenteUsd As Decimal = ToDecimal(ObtenerParametro(context, "monto_equivalente_usd"), 0D)

                    Dim tipoPagoTexto As String = ObtenerDescripcionTipoPago(cn, tr, tipoPagoId)
                    Dim monedaClave As String = ObtenerClaveMoneda(cn, tr, monedaId)

                    Dim defaultEsEfectivo As Boolean = False
                    If Not String.IsNullOrWhiteSpace(tipoPagoTexto) Then
                        defaultEsEfectivo = tipoPagoTexto.ToUpperInvariant().IndexOf("EFECTIVO") >= 0
                    End If

                    Dim defaultEsMonedaExtranjera As Boolean = Me.EsMonedaExtranjera(monedaClave)
                    Dim defaultEsPagoExcedente As Boolean = False
                    Dim defaultEsLiquidacion As Boolean = False

                    If esRevolvente Then
                        defaultEsPagoExcedente = montoCapital > saldoAntes
                    ElseIf montoPago > saldoAntes Then
                        defaultEsPagoExcedente = True
                    End If

                    If Not esRevolvente AndAlso saldoDespues <= 0D Then
                        defaultEsLiquidacion = True
                    End If

                    Dim esEfectivo As Boolean = ToBool(ObtenerParametro(context, "es_efectivo"), defaultEsEfectivo)
                    Dim esMonedaExtranjera As Boolean = ToBool(ObtenerParametro(context, "es_moneda_extranjera"), defaultEsMonedaExtranjera)
                    Dim esPagoExcedente As Boolean = ToBool(ObtenerParametro(context, "es_pago_excedente"), defaultEsPagoExcedente)
                    Dim esLiquidacion As Boolean = ToBool(ObtenerParametro(context, "es_liquidacion"), defaultEsLiquidacion)
                    If esRevolvente Then esLiquidacion = False

                    Dim fechaCreacionSolicitud As DateTime = Convert.ToDateTime(solicitud("fecha_creacion"))
                    Dim diasDesdeOtorgamiento As Integer = DateDiff(DateInterval.Day, fechaCreacionSolicitud, fechaPago)

                    Dim plazoTotal As Integer = ToInt(ObtenerParametro(context, "plazo_total_credito"), ToInt(solicitud("plazo"), 0))
                    Dim numeroPago As Integer = ToInt(ObtenerParametro(context, "numero_pago_en_credito"), ObtenerSiguienteNumeroPago(cn, tr, solicitudId))

                    Dim porcentajePlazo As Decimal = ToDecimal(ObtenerParametro(context, "porcentaje_plazo_transcurrido"), 0D)
                    If porcentajePlazo <= 0D AndAlso plazoTotal > 0 Then
                        porcentajePlazo = CalcularPorcentajePlazo(diasDesdeOtorgamiento, plazoTotal)
                    End If

                    Dim porcentajePagado As Decimal = ToDecimal(ObtenerParametro(context, "porcentaje_pagado_credito"), 0D)
                    If esRevolvente Then
                        porcentajePagado = 0D
                    ElseIf porcentajePagado <= 0D AndAlso montoCreditoOriginal > 0D Then
                        porcentajePagado = (montoPago / montoCreditoOriginal) * 100D
                    End If

                    Dim esLiquidacionAnticipada As Boolean = ToBool(ObtenerParametro(context, "es_liquidacion_anticipada"), esLiquidacion AndAlso porcentajePlazo < 100D)

                    Dim requiereDevolucion As Boolean = ToBool(ObtenerParametro(context, "requiere_devolucion"), esPagoExcedente)
                    Dim motivoDevolucion As String = ObtenerParametro(context, "motivo_devolucion").Trim()
                    Dim referenciaPago As String = ObtenerParametro(context, "referencia_pago").Trim()
                    Dim observaciones As String = ObtenerParametro(context, "observaciones").Trim()

                    Dim folioPago As String = ObtenerParametro(context, "folio_pago").Trim()

                    If String.IsNullOrWhiteSpace(folioPago) Then
                        folioPago = GenerarFolioPago(cn, tr)
                    End If

                    nuevoPagoId = InsertarPago(
                        cn,
                        tr,
                        solicitudId,
                        clienteId,
                        tipoPagoId,
                        canalPagoId,
                        monedaId,
                        aplicacionPagoId,
                        folioPago,
                        referenciaPago,
                        fechaPago,
                        montoPago,
                        tipoCambio,
                        montoEquivalenteMxn,
                        montoEquivalenteUsd,
                        montoCapital,
                        montoInteres,
                        montoIva,
                        montoMoratorio,
                        montoComisiones,
                        montoOtros,
                        saldoAntes,
                        saldoDespues,
                        pagoFijoContractual,
                        montoCreditoOriginal,
                        esEfectivo,
                        esMonedaExtranjera,
                        esPagoExcedente,
                        esLiquidacion,
                        esLiquidacionAnticipada,
                        diasDesdeOtorgamiento,
                        numeroPago,
                        plazoTotal,
                        porcentajePlazo,
                        porcentajePagado,
                        requiereDevolucion,
                        motivoDevolucion,
                        observaciones,
                        usuario
                    )

                    tr.Commit()

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Dim alertaOk As Boolean = False
        Dim alertaMensaje As String = ""
        Dim alertaDetalle As String = ""

        Try
            Dim resultadoAlerta As Dictionary(Of String, Object) = LlamarMotorAlertasPago(context, nuevoPagoId, usuario)
            alertaOk = Convert.ToBoolean(resultadoAlerta("ok"))

            If resultadoAlerta.ContainsKey("mensaje") Then
                alertaMensaje = Convert.ToString(resultadoAlerta("mensaje"))
            End If

            alertaDetalle = serializer.Serialize(resultadoAlerta)

        Catch exAlerta As Exception
            alertaOk = False
            alertaMensaje = "El pago se aplicó, pero no se pudo ejecutar el motor de alertas PLD."
            alertaDetalle = exAlerta.Message
        End Try

        respuesta("ok") = True
        respuesta("mensaje") = "Pago aplicado correctamente."
        respuesta("pago_credito_id") = nuevoPagoId
        respuesta("alertas_ok") = alertaOk
        respuesta("alertas_mensaje") = alertaMensaje
        respuesta("alertas_detalle") = alertaDetalle

        Return respuesta
    End Function

    Private Function CancelarPago(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim usuario As String = ObtenerUsuario(context)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "id"), 0)
        Dim motivo As String = ObtenerParametro(context, "motivo").Trim()
        If pagoId <= 0 Then pagoId = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        If pagoId <= 0 Then
            respuesta("ok") = False : respuesta("mensaje") = "ID de pago inválido." : Return respuesta
        End If
        If String.IsNullOrWhiteSpace(motivo) Then
            respuesta("ok") = False : respuesta("mensaje") = "El motivo de cancelación es obligatorio." : Return respuesta
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr,
                        "SELECT TOP 1 id,solicitud_credito_id,ISNULL(monto_capital,0) AS monto_capital FROM dbo.pagos_credito WITH (UPDLOCK,HOLDLOCK) WHERE id=@id AND activo=1 AND estatus=N'APLICADO';",
                        New List(Of SqlParameter) From {New SqlParameter("@id", pagoId)})
                    If dt.Rows.Count=0 Then
                        respuesta("ok")=False : respuesta("mensaje")="No se encontró el pago aplicado." : tr.Rollback() : Return respuesta
                    End If

                    Dim pago As DataRow=dt.Rows(0)
                    Dim solicitudId As Integer=ToInt(pago("solicitud_credito_id"),0)
                    Dim solicitud As DataRow=ObtenerSolicitud(cn,tr,solicitudId)

                    If solicitud IsNot Nothing AndAlso ToBool(solicitud("es_revolvente"),False) Then
                        If solicitud.IsNull("monto_autorizado") Then
                            respuesta("ok")=False : respuesta("mensaje")="No se puede cancelar: la línea revolvente no tiene límite autorizado." : tr.Rollback() : Return respuesta
                        End If
                        Dim saldoActual As Decimal=Money2(ObtenerSaldoUtilizadoRevolvente(cn,tr,solicitudId))
                        If saldoActual < 0D Then
                            respuesta("ok")=False : respuesta("mensaje")="No se puede cancelar mientras la línea tenga saldo utilizado negativo." : tr.Rollback() : Return respuesta
                        End If
                        Dim saldoPosterior As Decimal=Money2(saldoActual+ToDecimal(pago("monto_capital"),0D))
                        Dim limiteLinea As Decimal=Money2(Convert.ToDecimal(solicitud("monto_autorizado"),CultureInfo.InvariantCulture))
                        If saldoPosterior>limiteLinea Then
                            respuesta("ok")=False : respuesta("mensaje")="No se puede cancelar este pago porque disposiciones posteriores utilizaron el disponible recuperado." : tr.Rollback() : Return respuesta
                        End If
                    End If

                    Using cmd As New SqlCommand("UPDATE dbo.pagos_credito SET estatus=N'CANCELADO',activo=0,observaciones=CASE WHEN @motivo='' THEN observaciones ELSE ISNULL(observaciones,'')+CHAR(13)+CHAR(10)+N'Cancelación: '+@motivo END,modificado_por=@usuario,fecha_modificacion=SYSDATETIME() WHERE id=@id;",cn,tr)
                        cmd.Parameters.Add("@id",SqlDbType.Int).Value=pagoId
                        cmd.Parameters.Add("@motivo",SqlDbType.NVarChar,500).Value=motivo
                        cmd.Parameters.Add("@usuario",SqlDbType.NVarChar,100).Value=usuario
                        cmd.ExecuteNonQuery()
                    End Using
                    Dim saldoResultado As Object = Nothing
                    Dim disponibleResultado As Object = Nothing
                    If solicitud IsNot Nothing AndAlso ToBool(solicitud("es_revolvente"),False) Then
                        Dim saldoPost As Decimal = Money2(ObtenerSaldoUtilizadoRevolvente(cn,tr,solicitudId))
                        saldoResultado = saldoPost
                        disponibleResultado = Money2(Convert.ToDecimal(solicitud("monto_autorizado"),CultureInfo.InvariantCulture) - saldoPost)
                    End If

                    tr.Commit()
                    respuesta("ok")=True : respuesta("mensaje")="Pago cancelado correctamente."
                    respuesta("saldo_utilizado_despues") = saldoResultado
                    respuesta("disponible_despues") = disponibleResultado
                Catch
                    tr.Rollback() : Throw
                End Try
            End Using
        End Using
        Return respuesta
    End Function

    Private Function ObtenerSolicitud(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 sc.id,sc.cliente_id,sc.monto_solicitado,sc.monto_autorizado,sc.plazo,sc.moneda_id,sc.canal_pago_id,sc.estatus,sc.activo,sc.fecha_creacion,sc.fecha_vigencia_inicio,sc.fecha_vigencia_fin,ISNULL(cc.es_revolvente,0) AS es_revolvente,cc.nombre_credito AS tipo_credito " &
            "FROM dbo.solicitud_credito sc WITH (UPDLOCK,HOLDLOCK) LEFT JOIN dbo.catalogo_producto_financiero pf ON pf.id=sc.producto_financiero_id LEFT JOIN dbo.catalogo_creditos cc ON cc.id=pf.tipo_credito_id WHERE sc.id=@id AND sc.activo=1;"
        Dim dt As DataTable = EjecutarTablaTransaccion(cn,tr,sql,New List(Of SqlParameter) From {New SqlParameter("@id",solicitudId)})
        If dt.Rows.Count=0 Then Return Nothing
        Return dt.Rows(0)
    End Function

    Private Function ObtenerSaldoUtilizadoRevolvente(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As Decimal
        Dim dispuesto As Decimal=0D, amortizado As Decimal=0D
        Using cmd As New SqlCommand("SELECT ISNULL(SUM(monto),0) FROM dbo.credito_disposiciones WITH (UPDLOCK,HOLDLOCK) WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADA';",cn,tr)
            cmd.Parameters.Add("@id",SqlDbType.Int).Value=solicitudId
            dispuesto=Convert.ToDecimal(cmd.ExecuteScalar(),CultureInfo.InvariantCulture)
        End Using
        Using cmd As New SqlCommand("SELECT ISNULL(SUM(ISNULL(monto_capital,0)),0) FROM dbo.pagos_credito WITH (UPDLOCK,HOLDLOCK) WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADO';",cn,tr)
            cmd.Parameters.Add("@id",SqlDbType.Int).Value=solicitudId
            amortizado=Convert.ToDecimal(cmd.ExecuteScalar(),CultureInfo.InvariantCulture)
        End Using
        Return dispuesto-amortizado
    End Function

    Private Function ObtenerSaldoUtilizadoRevolventeEnFecha(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer, ByVal fechaCorte As DateTime) As Decimal
        Dim dispuesto As Decimal = 0D
        Dim amortizado As Decimal = 0D

        Using cmd As New SqlCommand("SELECT ISNULL(SUM(monto),0) FROM dbo.credito_disposiciones WITH (UPDLOCK,HOLDLOCK) WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADA' AND fecha_disposicion<=@fecha;", cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
            cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = fechaCorte
            dispuesto = Convert.ToDecimal(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using

        Using cmd As New SqlCommand("SELECT ISNULL(SUM(ISNULL(monto_capital,0)),0) FROM dbo.pagos_credito WITH (UPDLOCK,HOLDLOCK) WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADO' AND fecha_pago<=@fecha;", cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
            cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = fechaCorte
            amortizado = Convert.ToDecimal(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using

        Return Money2(dispuesto - amortizado)
    End Function

    Private Function ObtenerDescripcionTipoPago(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal tipoPagoId As Integer) As String
        Dim sql As String =
            "SELECT TOP 1 descripcion " &
            "FROM dbo.catalogo_tipo_pago " &
            "WHERE id = @id AND estatus = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@id", tipoPagoId)

            Dim valor As Object = cmd.ExecuteScalar()

            If valor Is Nothing OrElse valor Is DBNull.Value Then
                Return ""
            End If

            Return Convert.ToString(valor)
        End Using
    End Function

    Private Function ObtenerClaveMoneda(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal monedaId As Integer) As String
        Dim sql As String =
            "SELECT TOP 1 ISNULL(clave, '') AS clave " &
            "FROM dbo.catalogo_moneda_divisa " &
            "WHERE id = @id " &
            "AND ISNULL(estatus, ISNULL(activo, 1)) = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@id", monedaId)

            Dim valor As Object = cmd.ExecuteScalar()

            If valor Is Nothing OrElse valor Is DBNull.Value Then
                Return ""
            End If

            Return Convert.ToString(valor).Trim()
        End Using
    End Function

    Private Function EsMonedaExtranjera(ByVal clave As String) As Boolean
        Dim c As String = ""

        If clave IsNot Nothing Then
            c = clave.Trim().ToUpperInvariant()
        End If

        If String.IsNullOrWhiteSpace(c) Then Return False
        If c = "MXN" OrElse c = "MXP" OrElse c = "MXV" Then Return False

        Return True
    End Function

    Private Function ObtenerSiguienteNumeroPago(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As Integer
        Dim sql As String =
            "SELECT ISNULL(MAX(numero_pago_en_credito), 0) + 1 " &
            "FROM dbo.pagos_credito WITH (UPDLOCK, HOLDLOCK) " &
            "WHERE solicitud_credito_id = @solicitud_credito_id " &
            "AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@solicitud_credito_id", solicitudId)
            Return Convert.ToInt32(cmd.ExecuteScalar())
        End Using
    End Function

    Private Function CalcularPorcentajePlazo(ByVal diasDesdeOtorgamiento As Integer, ByVal plazoTotal As Integer) As Decimal
        If plazoTotal <= 0 Then Return 0D

        Dim diasEstimados As Decimal = Convert.ToDecimal(plazoTotal) * 30D

        If diasEstimados <= 0D Then Return 0D

        Dim porcentaje As Decimal = (Convert.ToDecimal(diasDesdeOtorgamiento) / diasEstimados) * 100D

        If porcentaje < 0D Then porcentaje = 0D
        If porcentaje > 100D Then porcentaje = 100D

        Return porcentaje
    End Function

    Private Function GenerarFolioPago(ByVal cn As SqlConnection, ByVal tr As SqlTransaction) As String
        Dim prefijo As String = "PAG-" & DateTime.Now.ToString("yyyyMMdd") & "-"
        Dim consecutivo As Integer = 1

        Dim sql As String =
            "SELECT ISNULL(MAX(CAST(RIGHT(folio_pago, 5) AS INT)), 0) + 1 " &
            "FROM dbo.pagos_credito WITH (UPDLOCK, HOLDLOCK) " &
            "WHERE folio_pago LIKE @prefijo + '%';"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@prefijo", prefijo)
            consecutivo = Convert.ToInt32(cmd.ExecuteScalar())
        End Using

        Return prefijo & consecutivo.ToString("00000")
    End Function

    Private Function InsertarPago(
        ByVal cn As SqlConnection,
        ByVal tr As SqlTransaction,
        ByVal solicitudId As Integer,
        ByVal clienteId As Integer,
        ByVal tipoPagoId As Integer,
        ByVal canalPagoId As Integer,
        ByVal monedaId As Integer,
        ByVal aplicacionPagoId As Integer,
        ByVal folioPago As String,
        ByVal referenciaPago As String,
        ByVal fechaPago As DateTime,
        ByVal montoPago As Decimal,
        ByVal tipoCambio As Decimal,
        ByVal montoEquivalenteMxn As Decimal,
        ByVal montoEquivalenteUsd As Decimal,
        ByVal montoCapital As Decimal,
        ByVal montoInteres As Decimal,
        ByVal montoIva As Decimal,
        ByVal montoMoratorio As Decimal,
        ByVal montoComisiones As Decimal,
        ByVal montoOtros As Decimal,
        ByVal saldoAntes As Decimal,
        ByVal saldoDespues As Decimal,
        ByVal pagoFijoContractual As Decimal,
        ByVal montoCreditoOriginal As Decimal,
        ByVal esEfectivo As Boolean,
        ByVal esMonedaExtranjera As Boolean,
        ByVal esPagoExcedente As Boolean,
        ByVal esLiquidacion As Boolean,
        ByVal esLiquidacionAnticipada As Boolean,
        ByVal diasDesdeOtorgamiento As Integer,
        ByVal numeroPago As Integer,
        ByVal plazoTotal As Integer,
        ByVal porcentajePlazo As Decimal,
        ByVal porcentajePagado As Decimal,
        ByVal requiereDevolucion As Boolean,
        ByVal motivoDevolucion As String,
        ByVal observaciones As String,
        ByVal usuario As String
    ) As Integer

        Dim sql As String =
            "INSERT INTO dbo.pagos_credito " &
            "(solicitud_credito_id, cliente_id, tipo_pago_id, canal_pago_id, moneda_id, aplicacion_pago_id, " &
            "folio_pago, referencia_pago, fecha_pago, fecha_registro, monto_pago, tipo_cambio, monto_equivalente_mxn, monto_equivalente_usd, " &
            "monto_capital, monto_interes, monto_iva, monto_moratorio, monto_comisiones, monto_otros, " &
            "saldo_antes_pago, saldo_despues_pago, pago_fijo_contractual, monto_credito_original, " &
            "es_efectivo, es_moneda_extranjera, es_pago_excedente, es_liquidacion, es_liquidacion_anticipada, " &
            "dias_desde_otorgamiento, numero_pago_en_credito, plazo_total_credito, porcentaje_plazo_transcurrido, porcentaje_pagado_credito, " &
            "requiere_devolucion, motivo_devolucion, estatus, activo, observaciones, creado_por, fecha_creacion) " &
            "VALUES " &
            "(@solicitud_credito_id, @cliente_id, @tipo_pago_id, @canal_pago_id, @moneda_id, @aplicacion_pago_id, " &
            "@folio_pago, @referencia_pago, @fecha_pago, SYSDATETIME(), @monto_pago, @tipo_cambio, @monto_equivalente_mxn, @monto_equivalente_usd, " &
            "@monto_capital, @monto_interes, @monto_iva, @monto_moratorio, @monto_comisiones, @monto_otros, " &
            "@saldo_antes_pago, @saldo_despues_pago, @pago_fijo_contractual, @monto_credito_original, " &
            "@es_efectivo, @es_moneda_extranjera, @es_pago_excedente, @es_liquidacion, @es_liquidacion_anticipada, " &
            "@dias_desde_otorgamiento, @numero_pago_en_credito, @plazo_total_credito, @porcentaje_plazo_transcurrido, @porcentaje_pagado_credito, " &
            "@requiere_devolucion, @motivo_devolucion, N'APLICADO', 1, @observaciones, @creado_por, SYSDATETIME()); " &
            "SELECT CAST(SCOPE_IDENTITY() AS INT);"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@solicitud_credito_id", solicitudId)

            If clienteId > 0 Then
                cmd.Parameters.AddWithValue("@cliente_id", clienteId)
            Else
                cmd.Parameters.AddWithValue("@cliente_id", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@tipo_pago_id", tipoPagoId)
            cmd.Parameters.AddWithValue("@canal_pago_id", canalPagoId)
            cmd.Parameters.AddWithValue("@moneda_id", monedaId)

            If aplicacionPagoId > 0 Then
                cmd.Parameters.AddWithValue("@aplicacion_pago_id", aplicacionPagoId)
            Else
                cmd.Parameters.AddWithValue("@aplicacion_pago_id", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@folio_pago", folioPago)
            cmd.Parameters.AddWithValue("@referencia_pago", DbValue(referenciaPago))
            cmd.Parameters.AddWithValue("@fecha_pago", fechaPago)
            cmd.Parameters.AddWithValue("@monto_pago", montoPago)
            cmd.Parameters.AddWithValue("@tipo_cambio", tipoCambio)
            cmd.Parameters.AddWithValue("@monto_equivalente_mxn", montoEquivalenteMxn)
            cmd.Parameters.AddWithValue("@monto_equivalente_usd", montoEquivalenteUsd)
            cmd.Parameters.AddWithValue("@monto_capital", montoCapital)
            cmd.Parameters.AddWithValue("@monto_interes", montoInteres)
            cmd.Parameters.AddWithValue("@monto_iva", montoIva)
            cmd.Parameters.AddWithValue("@monto_moratorio", montoMoratorio)
            cmd.Parameters.AddWithValue("@monto_comisiones", montoComisiones)
            cmd.Parameters.AddWithValue("@monto_otros", montoOtros)
            cmd.Parameters.AddWithValue("@saldo_antes_pago", saldoAntes)
            cmd.Parameters.AddWithValue("@saldo_despues_pago", saldoDespues)

            If pagoFijoContractual > 0D Then
                cmd.Parameters.AddWithValue("@pago_fijo_contractual", pagoFijoContractual)
            Else
                cmd.Parameters.AddWithValue("@pago_fijo_contractual", DBNull.Value)
            End If

            If montoCreditoOriginal > 0D Then
                cmd.Parameters.AddWithValue("@monto_credito_original", montoCreditoOriginal)
            Else
                cmd.Parameters.AddWithValue("@monto_credito_original", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@es_efectivo", esEfectivo)
            cmd.Parameters.AddWithValue("@es_moneda_extranjera", esMonedaExtranjera)
            cmd.Parameters.AddWithValue("@es_pago_excedente", esPagoExcedente)
            cmd.Parameters.AddWithValue("@es_liquidacion", esLiquidacion)
            cmd.Parameters.AddWithValue("@es_liquidacion_anticipada", esLiquidacionAnticipada)
            cmd.Parameters.AddWithValue("@dias_desde_otorgamiento", diasDesdeOtorgamiento)
            cmd.Parameters.AddWithValue("@numero_pago_en_credito", numeroPago)
            cmd.Parameters.AddWithValue("@plazo_total_credito", plazoTotal)
            cmd.Parameters.AddWithValue("@porcentaje_plazo_transcurrido", porcentajePlazo)
            cmd.Parameters.AddWithValue("@porcentaje_pagado_credito", porcentajePagado)
            cmd.Parameters.AddWithValue("@requiere_devolucion", requiereDevolucion)
            cmd.Parameters.AddWithValue("@motivo_devolucion", DbValue(motivoDevolucion))
            cmd.Parameters.AddWithValue("@observaciones", DbValue(observaciones))
            cmd.Parameters.AddWithValue("@creado_por", usuario)

            Return Convert.ToInt32(cmd.ExecuteScalar())
        End Using
    End Function

    Private Function LlamarMotorAlertasPago(ByVal context As HttpContext, ByVal pagoId As Integer, ByVal usuario As String) As Dictionary(Of String, Object)
        Dim baseUrl As String = context.Request.Url.GetLeftPart(UriPartial.Authority)
        Dim ruta As String = VirtualPathUtility.ToAbsolute("~/handlers/handler_alertas_pld.ashx")

        Dim url As String =
            baseUrl &
            ruta &
            "?action=generar_desde_pago_completo" &
            "&pago_credito_id=" & HttpUtility.UrlEncode(pagoId.ToString()) &
            "&usuario=" & HttpUtility.UrlEncode(usuario)

        Using wc As New WebClient()
            wc.Encoding = Encoding.UTF8
            Dim json As String = wc.DownloadString(url)

            If String.IsNullOrWhiteSpace(json) Then
                Dim vacio As New Dictionary(Of String, Object)()
                vacio("ok") = False
                vacio("mensaje") = "El motor de alertas no devolvió respuesta."
                Return vacio
            End If

            Return serializer.Deserialize(Of Dictionary(Of String, Object))(json)
        End Using
    End Function

End Class

