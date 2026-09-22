Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Web.Script.Serialization
Imports System.IO

Public Class solicitud_credito_handler : Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        Try
            Dim action As String = (If(context.Request("action"), "")).Trim().ToLowerInvariant()

            Select Case action
                Case "crear" : HandleCrear(context)
                Case "obtener" : HandleObtener(context)
                Case "actualizar_operacion" : HandleActualizarOperacion(context)
                Case "actualizar_relaciones" : HandleActualizarRelaciones(context)
                Case "listar" : HandleListar(context)
                Case "finalizar" : HandleFinalizar(context)
                Case "amortizacion_condusef" : HandleAmortizacionCondusef(context)

                ' NUEVO: periodos por producto (source of truth para el front)
                Case "periodos_producto" : HandlePeriodosProducto(context)

                Case "ping" : WriteJson(context, New With {.ok = True, .message = "pong"})
                Case Else
                    WriteError(context, "Acción no soportada. Usa: crear | obtener | actualizar_operacion | actualizar_relaciones | listar | finalizar | amortizacion_condusef | periodos_producto | ping")
            End Select

        Catch ex As Exception
            WriteError(context, "Excepción no controlada: " & ex.Message)
        End Try
    End Sub

    '====================================================
    ' ACCIÓN: AMORTIZACION_CONDUSEF
    '====================================================
    Private Sub HandleAmortizacionCondusef(ctx As HttpContext)
        Dim sid = ToInt(ctx.Request("solicitud_id"))
        If Not sid.HasValue OrElse sid.Value <= 0 Then
            WriteError(ctx, "solicitud_id es requerido.")
            Return
        End If

        Using con = GetConn()
            con.Open()

            Dim s As New SolicitudAmortData()
            Dim p As New PeriodoAmortData()
            Dim d As New DetalleProductoAmortData()

            Dim sql As String = "
SELECT TOP 1 id, producto_financiero_id, producto_financiero_periodo_id, monto_solicitado, plazo, tasa_entrada, fecha_creacion
FROM dbo.solicitud_credito WITH (NOLOCK)
WHERE id = @id;"
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = sid.Value
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        WriteError(ctx, "Solicitud no encontrada.")
                        Return
                    End If
                    s.SolicitudId = Convert.ToInt32(rd("id"))
                    s.ProductoFinancieroId = If(rd.IsDBNull(rd.GetOrdinal("producto_financiero_id")), 0, Convert.ToInt32(rd("producto_financiero_id")))
                    s.ProductoFinancieroPeriodoId = If(rd.IsDBNull(rd.GetOrdinal("producto_financiero_periodo_id")), 0, Convert.ToInt32(rd("producto_financiero_periodo_id")))
                    s.MontoSolicitado = ToDecObj(rd("monto_solicitado"))
                    s.Plazo = If(rd.IsDBNull(rd.GetOrdinal("plazo")), 0, Convert.ToInt32(rd("plazo")))
                    s.TasaEntradaPct = ToDecObj(rd("tasa_entrada"))
                    If Not rd.IsDBNull(rd.GetOrdinal("fecha_creacion")) Then
                        s.FechaCreacion = Convert.ToDateTime(rd("fecha_creacion"), CultureInfo.InvariantCulture)
                    End If
                End Using
            End Using

            If s.ProductoFinancieroId <= 0 Then
                WriteError(ctx, "La solicitud no tiene producto_financiero_id.")
                Return
            End If
            If s.ProductoFinancieroPeriodoId <= 0 Then
                WriteError(ctx, "La solicitud no tiene producto_financiero_periodo_id.")
                Return
            End If
            If s.MontoSolicitado <= 0D Then
                WriteError(ctx, "La solicitud no tiene monto_solicitado válido.")
                Return
            End If

            Dim sqlPeriodo As String = "
SELECT TOP 1 id, tipo_periodo, plazo, tasa_interes
FROM dbo.catalogo_producto_financiero_periodos WITH (NOLOCK)
WHERE id = @pid AND producto_financiero_id = @pf AND activo = 1;"
            Using cmd As New SqlCommand(sqlPeriodo, con)
                cmd.Parameters.Add("@pid", SqlDbType.Int).Value = s.ProductoFinancieroPeriodoId
                cmd.Parameters.Add("@pf", SqlDbType.Int).Value = s.ProductoFinancieroId
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        WriteError(ctx, "No se encontró la periodicidad seleccionada para el producto.")
                        Return
                    End If
                    p.TipoPeriodo = If(rd.IsDBNull(rd.GetOrdinal("tipo_periodo")), "", Convert.ToString(rd("tipo_periodo"), CultureInfo.InvariantCulture))
                    p.Plazo = If(rd.IsDBNull(rd.GetOrdinal("plazo")), 0, Convert.ToInt32(rd("plazo")))
                    p.TasaInteresFrac = ToDecObj(rd("tasa_interes"))
                End Using
            End Using

            Dim sqlDet As String = "
SELECT TOP 1
    iva, iva_comision, base_calculo, redondear_pago_fijo, centavos_para_redondeo,
    comision_apertura, amortizar_comision_apertura
FROM dbo.detalles_del_producto WITH (NOLOCK)
WHERE producto_id = @pf
ORDER BY id DESC;"
            Using cmd As New SqlCommand(sqlDet, con)
                cmd.Parameters.Add("@pf", SqlDbType.Int).Value = s.ProductoFinancieroId
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        d.IvaPct = ToDecObj(rd("iva"))
                        d.IvaComisionPct = ToDecObj(rd("iva_comision"))
                        d.BaseCalculo = If(rd.IsDBNull(rd.GetOrdinal("base_calculo")), 0D, ToDecObj(rd("base_calculo")))
                        d.RedondearPagoFijo = (Not rd.IsDBNull(rd.GetOrdinal("redondear_pago_fijo")) AndAlso Convert.ToBoolean(rd("redondear_pago_fijo")))
                        d.CentavosParaRedondeo = ToDecObj(rd("centavos_para_redondeo"))
                        d.ComisionAperturaPct = ToDecObj(rd("comision_apertura"))
                        d.AmortizarComisionApertura = (Not rd.IsDBNull(rd.GetOrdinal("amortizar_comision_apertura")) AndAlso Convert.ToBoolean(rd("amortizar_comision_apertura")))
                    End If
                End Using
            End Using

            Dim plazo As Integer = If(s.Plazo > 0, s.Plazo, p.Plazo)
            If plazo <= 0 Then
                WriteError(ctx, "No se pudo resolver el plazo.")
                Return
            End If

            Dim tasaAnualPct As Decimal = If(s.TasaEntradaPct > 0D, s.TasaEntradaPct, (p.TasaInteresFrac * 100D))
            Dim tasaPeriodo As Decimal = GetTasaPeriodo(tasaAnualPct, p.TipoPeriodo)

            Dim pagoFijoCapitalInteres As Decimal = CalcularPagoFijo(s.MontoSolicitado, tasaPeriodo, plazo)
            If d.RedondearPagoFijo Then
                Dim stepRedondeo As Decimal = ResolveStepRedondeo(d.CentavosParaRedondeo)
                pagoFijoCapitalInteres = RedondearAlPaso(pagoFijoCapitalInteres, stepRedondeo)
            End If

            Dim comisionAperturaMonto As Decimal = Math.Round(s.MontoSolicitado * (d.ComisionAperturaPct / 100D), 2, MidpointRounding.AwayFromZero)
            Dim ivaComisionAperturaMonto As Decimal = Math.Round(comisionAperturaMonto * (d.IvaComisionPct / 100D), 2, MidpointRounding.AwayFromZero)
            Dim totalCargosIniciales As Decimal = Math.Round(comisionAperturaMonto + ivaComisionAperturaMonto, 2, MidpointRounding.AwayFromZero)

            Dim rows As New List(Of Dictionary(Of String, Object))()

            Dim row0 As New Dictionary(Of String, Object)
            row0("numero_pago") = 0
            row0("fecha") = ""
            row0("saldo_inicial") = 0D
            row0("capital_interes") = 0D
            row0("capital") = 0D
            row0("interes") = 0D
            row0("iva_interes") = 0D
            row0("pago_periodico") = 0D
            row0("saldo_pendiente") = Math.Round(s.MontoSolicitado, 2, MidpointRounding.AwayFromZero)
            rows.Add(row0)

            Dim saldo As Decimal = s.MontoSolicitado
            For i As Integer = 1 To plazo
                Dim saldoInicial As Decimal = saldo
                Dim interes As Decimal = Math.Round(saldoInicial * tasaPeriodo, 6, MidpointRounding.AwayFromZero)
                Dim capital As Decimal = pagoFijoCapitalInteres - interes

                If i = plazo OrElse capital > saldoInicial Then
                    capital = saldoInicial
                End If
                If capital < 0D Then capital = 0D

                Dim capitalInteres As Decimal = Math.Round(capital + interes, 2, MidpointRounding.AwayFromZero)
                Dim ivaInteres As Decimal = Math.Round(interes * (d.IvaPct / 100D), 2, MidpointRounding.AwayFromZero)
                Dim pagoPeriodo As Decimal = Math.Round(capitalInteres + ivaInteres, 2, MidpointRounding.AwayFromZero)
                saldo = Math.Round(saldoInicial - capital, 2, MidpointRounding.AwayFromZero)
                If Math.Abs(Convert.ToDouble(saldo, CultureInfo.InvariantCulture)) < 0.01 Then saldo = 0D

                Dim row As New Dictionary(Of String, Object)
                row("numero_pago") = i
                row("fecha") = FormatFechaAmortizacion(s.FechaCreacion, p.TipoPeriodo, i)
                row("saldo_inicial") = Math.Round(saldoInicial, 2, MidpointRounding.AwayFromZero)
                row("capital_interes") = capitalInteres
                row("capital") = Math.Round(capital, 2, MidpointRounding.AwayFromZero)
                row("interes") = Math.Round(interes, 2, MidpointRounding.AwayFromZero)
                row("iva_interes") = ivaInteres
                row("pago_periodico") = pagoPeriodo
                row("saldo_pendiente") = saldo
                rows.Add(row)
            Next

            Dim tipoPeriodoNorm As String = NormalizePeriodo(p.TipoPeriodo)
            Dim pagoHeader As String = If(tipoPeriodoNorm = "SEMANAL", "Pago Semanal", "Pago Mensual")

            WriteJson(ctx, New With {
                .ok = True,
                .data = New With {
                    .solicitud_id = s.SolicitudId,
                    .producto_financiero_id = s.ProductoFinancieroId,
                    .producto_financiero_periodo_id = s.ProductoFinancieroPeriodoId,
                    .tipo_periodo = p.TipoPeriodo,
                    .plazo = plazo,
                    .tasa_anual_pct = Math.Round(tasaAnualPct, 6, MidpointRounding.AwayFromZero),
                    .tasa_periodo = Math.Round(tasaPeriodo, 10, MidpointRounding.AwayFromZero),
                    .pago_header = pagoHeader,
                    .pago_fijo_capital_interes = Math.Round(pagoFijoCapitalInteres, 2, MidpointRounding.AwayFromZero),
                    .cargos_iniciales = New With {
                        .comision_apertura_monto = comisionAperturaMonto,
                        .iva_comision_apertura = ivaComisionAperturaMonto,
                        .total_cargos_iniciales = totalCargosIniciales
                    },
                    .rows = rows
                }
            })
        End Using
    End Sub

    Private Function NormalizePeriodo(tipoPeriodo As String) As String
        Dim t As String = If(tipoPeriodo, "").Trim().ToUpperInvariant()
        If t.Contains("QUINC") Then Return "QUINCENAL"
        If t.Contains("SEMAN") Then Return "SEMANAL"
        Return "MENSUAL"
    End Function

    Private Function GetTasaPeriodo(tasaAnualPct As Decimal, tipoPeriodo As String) As Decimal
        Dim anual As Decimal = tasaAnualPct / 100D
        Select Case NormalizePeriodo(tipoPeriodo)
            Case "SEMANAL" : Return anual / 52D
            Case "QUINCENAL" : Return anual / 24D
            Case Else : Return anual / 12D
        End Select
    End Function

    Private Function CalcularPagoFijo(capital As Decimal, tasaPeriodo As Decimal, plazo As Integer) As Decimal
        If plazo <= 0 Then Return 0D
        If tasaPeriodo <= 0D Then
            Return Math.Round(capital / plazo, 6, MidpointRounding.AwayFromZero)
        End If

        Dim r As Double = Convert.ToDouble(tasaPeriodo, CultureInfo.InvariantCulture)
        Dim pv As Double = Convert.ToDouble(capital, CultureInfo.InvariantCulture)
        Dim n As Double = Convert.ToDouble(plazo, CultureInfo.InvariantCulture)
        Dim pago As Double = pv * r / (1D - Math.Pow(1D + r, -n))
        Return Convert.ToDecimal(pago, CultureInfo.InvariantCulture)
    End Function

    Private Function ResolveStepRedondeo(v As Decimal) As Decimal
        If v <= 0D Then Return 0D
        If v >= 1D Then
            Return v / 100D
        End If
        Return v
    End Function

    Private Function RedondearAlPaso(valor As Decimal, paso As Decimal) As Decimal
        If paso <= 0D Then Return valor
        Dim unidades As Decimal = valor / paso
        Dim unidadesRed As Decimal = Math.Round(unidades, 0, MidpointRounding.AwayFromZero)
        Return unidadesRed * paso
    End Function

    Private Function FormatFechaAmortizacion(fechaBase As DateTime?, tipoPeriodo As String, numeroPago As Integer) As String
        If numeroPago <= 0 Then Return ""

        Dim baseDate As DateTime = If(fechaBase.HasValue, fechaBase.Value, DateTime.UtcNow)
        Dim fechaPago As DateTime

        Select Case NormalizePeriodo(tipoPeriodo)
            Case "SEMANAL"
                fechaPago = baseDate.AddDays(7 * numeroPago)
            Case "QUINCENAL"
                fechaPago = baseDate.AddDays(15 * numeroPago)
            Case Else
                fechaPago = baseDate.AddMonths(numeroPago)
        End Select

        Return fechaPago.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
    End Function

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '====================================================
    ' Helpers generales
    '====================================================
    Private Function GetConn() As SqlConnection
        Return New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
    End Function

    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String)
        ctx.Response.StatusCode = 200 ' manejo uniforme en front
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    Private Sub WriteErrorWithStatus(ctx As HttpContext, statusCode As Integer, msg As String)
        ctx.Response.StatusCode = statusCode
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    '====================================================
    ' Validaciones con HTTP status (para fetch + Swal.fire)
    '====================================================
    Private Class ValidationHttpException
        Inherits Exception
        Public ReadOnly Property StatusCode As Integer
        Public Sub New(statusCode As Integer, message As String)
            MyBase.New(message)
            Me.StatusCode = statusCode
        End Sub
    End Class

    Private Structure ResolvedPeriodo
        Public PeriodoId As Integer
        Public Plazo As Integer
        Public TasaEntrada As Decimal ' porcentaje (21.000000)
    End Structure

    '====================================================
    ' Resolver periodicidad del producto (id + plazo + tasa)
    ' - La tasa en catalogo_producto_financiero_periodos.tasa_interes viene como fracción (0.21 = 21%)
    ' - La solicitud guarda tasa_entrada en porcentaje
    '====================================================
    Private Function ResolvePeriodo(con As SqlConnection,
                                   tx As SqlTransaction,
                                   productoFinancieroId As Integer,
                                   periodoId As Integer?,
                                   plazoFromFront As Integer?,
                                   tasaEntradaFromFront As Decimal?) As ResolvedPeriodo

        ' 1) Si viene el id del periodo, ese manda (pero validamos pertenencia)
        If periodoId.HasValue AndAlso periodoId.Value > 0 Then
            Dim sql As String = "
                SELECT TOP 1 id, plazo, tasa_interes
                FROM dbo.catalogo_producto_financiero_periodos WITH (NOLOCK)
                WHERE id = @pid AND producto_financiero_id = @pf AND activo = 1;"
            Using cmd As New SqlCommand(sql, con, tx)
                cmd.Parameters.Add("@pid", SqlDbType.Int).Value = periodoId.Value
                cmd.Parameters.Add("@pf", SqlDbType.Int).Value = productoFinancieroId
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        Throw New ValidationHttpException(400, "El periodo no pertenece al producto seleccionado.")
                    End If

                    Dim pid As Integer = Convert.ToInt32(rd("id"))
                    Dim plazoDb As Integer = Convert.ToInt32(rd("plazo"))
                    Dim tasaFrac As Decimal = 0D
                    If Not Convert.IsDBNull(rd("tasa_interes")) Then
                        tasaFrac = Convert.ToDecimal(rd("tasa_interes"), CultureInfo.InvariantCulture)
                    End If
                    Dim tasaPct As Decimal = Math.Round(tasaFrac * 100D, 6)

                    Return New ResolvedPeriodo With {
                        .PeriodoId = pid,
                        .Plazo = plazoDb,
                        .TasaEntrada = tasaPct
                    }
                End Using
            End Using
        End If

        ' 2) Compatibilidad: si no viene periodo_id, intentamos inferirlo por (plazo + tasa_entrada) del front
        If plazoFromFront.HasValue AndAlso plazoFromFront.Value > 0 AndAlso tasaEntradaFromFront.HasValue AndAlso tasaEntradaFromFront.Value > 0D Then
            Dim sql2 As String = "
                SELECT TOP 1 id, plazo, tasa_interes
                FROM dbo.catalogo_producto_financiero_periodos WITH (NOLOCK)
                WHERE producto_financiero_id = @pf
                  AND activo = 1
                  AND plazo = @plazo
                  AND ABS((tasa_interes * 100.0) - @tasa_pct) < 0.000001
                ORDER BY id;"
            Using cmd As New SqlCommand(sql2, con, tx)
                cmd.Parameters.Add("@pf", SqlDbType.Int).Value = productoFinancieroId
                cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = plazoFromFront.Value
                cmd.Parameters.Add("@tasa_pct", SqlDbType.Decimal).Value = New System.Data.SqlTypes.SqlDecimal(tasaEntradaFromFront.Value)
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        Dim pid As Integer = Convert.ToInt32(rd("id"))
                        Dim plazoDb As Integer = Convert.ToInt32(rd("plazo"))
                        Dim tasaFrac As Decimal = 0D
                        If Not Convert.IsDBNull(rd("tasa_interes")) Then
                            tasaFrac = Convert.ToDecimal(rd("tasa_interes"), CultureInfo.InvariantCulture)
                        End If
                        Dim tasaPct As Decimal = Math.Round(tasaFrac * 100D, 6)

                        Return New ResolvedPeriodo With {
                            .PeriodoId = pid,
                            .Plazo = plazoDb,
                            .TasaEntrada = tasaPct
                        }
                    End If
                End Using
            End Using
        End If

        Throw New ValidationHttpException(400, "Debe seleccionar la periodicidad del producto.")
    End Function

    Private Function ToInt(value As String) As Integer?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim n As Integer
        If Integer.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            Return n
        End If
        Return Nothing
    End Function

    Private Function ToDec(value As String) As Decimal?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim clean = value.Trim().Replace(",", ".")
        Dim d As Decimal
        If Decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then
            Return d
        End If
        Return Nothing
    End Function

    Private Function ToStr(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Return value.Trim()
    End Function

    Private Function SqlDecOrNull(d As Decimal?) As Object
        If d.HasValue Then
            Return New System.Data.SqlTypes.SqlDecimal(d.Value)
        Else
            Return DBNull.Value
        End If
    End Function

    Private Function NowUser() As String
        Dim u = HttpContext.Current.User
        If u IsNot Nothing AndAlso u.Identity IsNot Nothing AndAlso u.Identity.IsAuthenticated Then
            Return u.Identity.Name
        End If
        Return "system"
    End Function

    Private Function ToDecObj(value As Object) As Decimal
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return 0D
        Dim d As Decimal
        If Decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, d) Then
            Return d
        End If
        Return 0D
    End Function

    Private Function NivelDesdeCampos(impacto As Object, prob As Object, nivelDirecto As Object) As Decimal
        Dim nivel As Decimal = ToDecObj(nivelDirecto)
        If nivel > 0D Then
            Return Math.Round(nivel, 2)
        End If
        Dim imp As Decimal = ToDecObj(impacto)
        Dim pr As Decimal = ToDecObj(prob)
        If imp <= 0 OrElse pr <= 0 Then
            Return 0D
        End If
        Dim calc As Decimal = (imp * pr) / 100D
        Return Math.Round(calc, 2)
    End Function

    ' -------- Helpers para JSON (front usa fetch con body JSON) --------
    Private Function ReadJsonBody(ctx As HttpContext) As Dictionary(Of String, Object)
        If Not String.Equals(ctx.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) Then
            Return Nothing
        End If
        Dim ct As String = If(ctx.Request.ContentType, "")
        If Not ct.ToLowerInvariant().Contains("application/json") Then
            Return Nothing
        End If

        ctx.Request.InputStream.Position = 0
        Using sr As New StreamReader(ctx.Request.InputStream)
            Dim raw As String = sr.ReadToEnd()
            If String.IsNullOrWhiteSpace(raw) Then Return Nothing
            Try
                Return serializer.Deserialize(Of Dictionary(Of String, Object))(raw)
            Catch
                Return Nothing
            End Try
        End Using
    End Function

    Private Function GetParam(ctx As HttpContext, json As Dictionary(Of String, Object), key As String) As String
        If json IsNot Nothing AndAlso json.ContainsKey(key) AndAlso json(key) IsNot Nothing Then
            Return Convert.ToString(json(key), CultureInfo.InvariantCulture)
        End If
        Return ctx.Request(key)
    End Function

    '====================================================
    ' NUEVO: PERIODOS_PRODUCTO
    ' GET: /handlers/solicitud_credito_handler.ashx?action=periodos_producto&producto_financiero_id=9
    ' Respuesta: { ok:true, moneda_id:1, data:[{id,tipo_periodo,plazo,tasa_interes}] }
    '====================================================
    Private Sub HandlePeriodosProducto(ctx As HttpContext)
        Dim pf = ToInt(ctx.Request("producto_financiero_id"))
        If Not pf.HasValue OrElse pf.Value <= 0 Then
            WriteErrorWithStatus(ctx, 400, "producto_financiero_id es requerido.")
            Return
        End If

        Using con = GetConn()
            con.Open()

            ' moneda_id del maestro (para autollenado del front)
            Dim monedaId As Integer? = Nothing
            Using cmdMon As New SqlCommand("
                SELECT TOP 1 moneda_id
                FROM dbo.catalogo_producto_financiero WITH (NOLOCK)
                WHERE id = @id;", con)
                cmdMon.Parameters.Add("@id", SqlDbType.Int).Value = pf.Value
                Dim o = cmdMon.ExecuteScalar()
                If o IsNot Nothing AndAlso Not Convert.IsDBNull(o) Then
                    monedaId = Convert.ToInt32(o)
                End If
            End Using

            ' periodos activos del producto
            Dim sql As String = "
                SELECT id, tipo_periodo, plazo, tasa_interes
                FROM dbo.catalogo_producto_financiero_periodos WITH (NOLOCK)
                WHERE producto_financiero_id = @pf
                  AND activo = 1
                ORDER BY tipo_periodo, plazo, id;"
            Dim rows As New List(Of Object)
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@pf", SqlDbType.Int).Value = pf.Value
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        Dim row As New Dictionary(Of String, Object)
                        row("id") = If(rd.IsDBNull(0), Nothing, rd.GetValue(0))
                        row("tipo_periodo") = If(rd.IsDBNull(1), Nothing, rd.GetValue(1))
                        row("plazo") = If(rd.IsDBNull(2), Nothing, rd.GetValue(2))
                        row("tasa_interes") = If(rd.IsDBNull(3), Nothing, rd.GetValue(3)) ' fracción (0.21)
                        rows.Add(row)
                    End While
                End Using
            End Using

            WriteJson(ctx, New With {.ok = True, .moneda_id = monedaId, .data = rows})
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: CREAR
    '====================================================
    Private Sub HandleCrear(ctx As HttpContext)
        Dim json = ReadJsonBody(ctx)

        Dim pf = ToInt(GetParam(ctx, json, "producto_financiero_id"))
        If Not pf.HasValue Then
            WriteError(ctx, "producto_financiero_id es requerido.")
            Return
        End If

        Dim clienteId = ToInt(GetParam(ctx, json, "cliente_id"))
        Dim canalId = ToInt(GetParam(ctx, json, "canal_pago_id"))
        Dim destId = ToInt(GetParam(ctx, json, "destino_recursos_id"))
        Dim origenId = ToInt(GetParam(ctx, json, "origen_recursos_id"))
        Dim medioId = ToInt(GetParam(ctx, json, "medio_contacto_id"))
        Dim monedaId = ToInt(GetParam(ctx, json, "moneda_id"))
        Dim periodoId = ToInt(GetParam(ctx, json, "producto_financiero_periodo_id"))

        Dim monto = ToDec(GetParam(ctx, json, "monto_solicitado"))
        Dim plazo = ToInt(GetParam(ctx, json, "plazo"))
        Dim tasa = ToDec(GetParam(ctx, json, "tasa_entrada"))
        Dim obs = ToStr(GetParam(ctx, json, "observaciones"))
        Dim permitirPagosAnticipados As Boolean = ToBoolBit(GetParam(ctx, json, "permitir_pagos_anticipados"), True)

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    If Not ExistsById(con, tx, "dbo.catalogo_producto_financiero", pf.Value) Then
                        WriteError(ctx, "producto_financiero_id no existe o está inactivo.")
                        tx.Rollback() : Return
                    End If

                    If origenId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_origen_recursos", origenId.Value) Then
                        WriteError(ctx, "origen_recursos_id inválido.")
                        tx.Rollback() : Return
                    End If

                    Dim rp As ResolvedPeriodo = ResolvePeriodo(con, tx, pf.Value, periodoId, plazo, tasa)
                    periodoId = rp.PeriodoId
                    plazo = rp.Plazo
                    tasa = rp.TasaEntrada

                    Dim sql As String = "
                    INSERT INTO dbo.solicitud_credito
                      (cliente_id, producto_financiero_id, canal_pago_id, destino_recursos_id, origen_recursos_id, medio_contacto_id, moneda_id, producto_financiero_periodo_id,
                       monto_solicitado, plazo, tasa_entrada, observaciones, permitir_pagos_anticipados,
                       pld_puntaje_total, pld_nivel_riesgo_pld, pld_metodologia_version, pld_fecha_calculo,
                       estatus, activo, creado_por)
                    VALUES
                      (@cliente_id, @pf_id, @canal_id, @dest_id, @origen_id, @medio_id, @moneda_id, @periodo_id,
                       @monto, @plazo, @tasa, @obs, @permitir_pagos_anticipados,
                       @pld_puntaje_total, @pld_nivel, @pld_version, SYSUTCDATETIME(),
                       @estatus, 1, @creado_por);
                    SELECT SCOPE_IDENTITY();"

                    Dim solicitudId As Integer
                    Using cmd As New SqlCommand(sql, con, tx)
                        cmd.Parameters.Add("@cliente_id", SqlDbType.Int).Value = If(clienteId.HasValue, CType(clienteId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@pf_id", SqlDbType.Int).Value = pf.Value
                        cmd.Parameters.Add("@canal_id", SqlDbType.Int).Value = If(canalId.HasValue, CType(canalId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@dest_id", SqlDbType.Int).Value = If(destId.HasValue, CType(destId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@origen_id", SqlDbType.Int).Value = If(origenId.HasValue, CType(origenId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@medio_id", SqlDbType.Int).Value = If(medioId.HasValue, CType(medioId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@moneda_id", SqlDbType.Int).Value = If(monedaId.HasValue, CType(monedaId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@periodo_id", SqlDbType.Int).Value = If(periodoId.HasValue, CType(periodoId.Value, Object), DBNull.Value)

                        cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = SqlDecOrNull(monto)
                        cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = If(plazo.HasValue, CType(plazo.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = SqlDecOrNull(tasa)
                        cmd.Parameters.Add("@obs", SqlDbType.NVarChar, 1000).Value = If(obs Is Nothing, CType(DBNull.Value, Object), obs)
                        cmd.Parameters.Add("@permitir_pagos_anticipados", SqlDbType.Bit).Value = permitirPagosAnticipados

                        cmd.Parameters.Add("@pld_puntaje_total", SqlDbType.Decimal).Value = New System.Data.SqlTypes.SqlDecimal(0D)
                        cmd.Parameters.Add("@pld_nivel", SqlDbType.Decimal).Value = New System.Data.SqlTypes.SqlDecimal(0D)
                        cmd.Parameters.Add("@pld_version", SqlDbType.NVarChar, 50).Value = DBNull.Value

                        cmd.Parameters.Add("@estatus", SqlDbType.NVarChar, 30).Value = "BORRADOR"
                        cmd.Parameters.Add("@creado_por", SqlDbType.NVarChar, 100).Value = NowUser()

                        solicitudId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Solicitud creada.", .solicitud_id = solicitudId})
                Catch ex As Exception
                    tx.Rollback()
                    If TypeOf ex Is ValidationHttpException Then
                        Dim vex = CType(ex, ValidationHttpException)
                        WriteErrorWithStatus(ctx, vex.StatusCode, vex.Message)
                    Else
                        WriteError(ctx, "No se pudo crear la solicitud: " & ex.Message)
                    End If
                End Try
            End Using
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: OBTENER
    '====================================================
    Private Sub HandleObtener(ctx As HttpContext)
        Dim json = ReadJsonBody(ctx)
        Dim sid = ToInt(GetParam(ctx, json, "id"))
        If Not sid.HasValue Then sid = ToInt(GetParam(ctx, json, "solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "solicitud_id es requerido.")
            Return
        End If

        Using con = GetConn()
            con.Open()

            Dim sqlSol As String = "
            SELECT TOP 1 *
            FROM dbo.solicitud_credito
            WHERE id = @id;
            "

            Dim solRow As New Dictionary(Of String, Object)
            Using cmd As New SqlCommand(sqlSol, con)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = sid.Value

                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        WriteError(ctx, "Solicitud no encontrada.")
                        Return
                    End If

                    For i = 0 To rd.FieldCount - 1
                        solRow(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                    Next
                End Using
            End Using

            Dim clienteObj As Object = Nothing
            If solRow.ContainsKey("cliente_id") AndAlso solRow("cliente_id") IsNot Nothing Then
                Dim cliId As Integer = Convert.ToInt32(solRow("cliente_id"))

                Dim sqlCli As String = "
                SELECT TOP 1 *
                FROM dbo.cliente_persona_fisica
                WHERE id_cliente = @id;"

                Using cmd As New SqlCommand(sqlCli, con)
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = cliId

                    Using rd = cmd.ExecuteReader()
                        If rd.Read() Then
                            Dim cli As New Dictionary(Of String, Object)
                            For i = 0 To rd.FieldCount - 1
                                cli(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                            Next
                            clienteObj = cli
                        End If
                    End Using
                End Using
            End If

            Dim contactoObj As Object = Nothing
            Dim telefonos As New List(Of Object)
            Dim emails As New List(Of Object)
            Dim domicilios As New List(Of Object)

            Dim sqlCnt As String = "
            SELECT TOP 1 *
            FROM dbo.contacto_solicitud
            WHERE solicitud_id = @sid;
            "

            Dim contactoId As Integer? = Nothing

            Using cmd As New SqlCommand(sqlCnt, con)
                cmd.Parameters.Add("@sid", SqlDbType.Int).Value = sid.Value

                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        Dim cnt As New Dictionary(Of String, Object)
                        For i = 0 To rd.FieldCount - 1
                            cnt(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                        Next
                        contactoObj = cnt

                        If Not rd.IsDBNull(rd.GetOrdinal("id")) Then
                            contactoId = Convert.ToInt32(rd("id"))
                        End If
                    End If
                End Using
            End Using

            If contactoId.HasValue Then
                Dim sqlTel As String = "
                SELECT *
                FROM dbo.contacto_solicitud_telefono
                WHERE contacto_id = @cid
                ORDER BY id;"
                Using cmd As New SqlCommand(sqlTel, con)
                    cmd.Parameters.Add("@cid", SqlDbType.Int).Value = contactoId.Value
                    Using rd = cmd.ExecuteReader()
                        While rd.Read()
                            Dim t As New Dictionary(Of String, Object)
                            For i = 0 To rd.FieldCount - 1
                                t(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                            Next
                            telefonos.Add(t)
                        End While
                    End Using
                End Using
            End If

            If contactoId.HasValue Then
                Dim sqlMail As String = "
                SELECT *
                FROM dbo.contacto_solicitud_email
                WHERE contacto_id = @cid
                ORDER BY id;"
                Using cmd As New SqlCommand(sqlMail, con)
                    cmd.Parameters.Add("@cid", SqlDbType.Int).Value = contactoId.Value
                    Using rd = cmd.ExecuteReader()
                        While rd.Read()
                            Dim m As New Dictionary(Of String, Object)
                            For i = 0 To rd.FieldCount - 1
                                m(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                            Next
                            emails.Add(m)
                        End While
                    End Using
                End Using
            End If

            If contactoId.HasValue Then
                Dim sqlDom As String = "
                SELECT *
                FROM dbo.contacto_solicitud_domicilio
                WHERE contacto_id = @cid
                ORDER BY es_principal DESC, id DESC;"
                Using cmd As New SqlCommand(sqlDom, con)
                    cmd.Parameters.Add("@cid", SqlDbType.Int).Value = contactoId.Value
                    Using rd = cmd.ExecuteReader()
                        While rd.Read()
                            Dim d As New Dictionary(Of String, Object)
                            For i = 0 To rd.FieldCount - 1
                                d(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                            Next
                            domicilios.Add(d)
                        End While
                    End Using
                End Using
            End If

            WriteJson(ctx, New With {
                .ok = True,
                .data = New With {
                    .solicitud = solRow,
                    .cliente = clienteObj,
                    .contacto = contactoObj,
                    .telefonos = telefonos,
                    .emails = emails,
                    .domicilios = domicilios
                }
            })
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: ACTUALIZAR_OPERACION
    '====================================================
    Private Sub HandleActualizarOperacion(ctx As HttpContext)
        Dim json = ReadJsonBody(ctx)
        Dim sid = ToInt(GetParam(ctx, json, "id"))
        If Not sid.HasValue Then sid = ToInt(GetParam(ctx, json, "solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "id (solicitud_id) es requerido.")
            Return
        End If

        Dim pf = ToInt(GetParam(ctx, json, "producto_financiero_id"))
        If Not pf.HasValue Then
            WriteError(ctx, "producto_financiero_id es requerido.")
            Return
        End If

        Dim canalId = ToInt(GetParam(ctx, json, "canal_pago_id"))
        Dim destId = ToInt(GetParam(ctx, json, "destino_recursos_id"))
        Dim origenId = ToInt(GetParam(ctx, json, "origen_recursos_id"))
        Dim monedaId = ToInt(GetParam(ctx, json, "moneda_id"))
        Dim periodoId = ToInt(GetParam(ctx, json, "producto_financiero_periodo_id"))

        Dim monto = ToDec(GetParam(ctx, json, "monto_solicitado"))
        Dim plazo = ToInt(GetParam(ctx, json, "plazo"))
        Dim tasa = ToDec(GetParam(ctx, json, "tasa_entrada"))
        Dim obs = ToStr(GetParam(ctx, json, "observaciones"))
        Dim permitirPagosAnticipados As Boolean = ToBoolBit(GetParam(ctx, json, "permitir_pagos_anticipados"), True)

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    If Not ExistsById(con, tx, "dbo.solicitud_credito", sid.Value) Then
                        WriteError(ctx, "Solicitud no encontrada.")
                        tx.Rollback() : Return
                    End If

                    If Not ExistsById(con, tx, "dbo.catalogo_producto_financiero", pf.Value) Then Throw New Exception("producto_financiero_id inválido.")
                    If canalId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_canal_pago", canalId.Value) Then Throw New Exception("canal_pago_id inválido.")
                    If destId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_destino_recursos", destId.Value) Then Throw New Exception("destino_recursos_id inválido.")
                    If origenId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_origen_recursos", origenId.Value) Then Throw New Exception("origen_recursos_id inválido.")
                    If monedaId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_moneda_divisa", monedaId.Value) Then Throw New Exception("moneda_id inválido.")

                    Dim rp As ResolvedPeriodo = ResolvePeriodo(con, tx, pf.Value, periodoId, plazo, tasa)
                    periodoId = rp.PeriodoId
                    plazo = rp.Plazo
                    tasa = rp.TasaEntrada

                    Dim sql As String = "
                    UPDATE dbo.solicitud_credito
                    SET producto_financiero_id = @pf,
                        producto_financiero_periodo_id = @periodo,
                        canal_pago_id           = @canal,
                        destino_recursos_id     = @dest,
                        origen_recursos_id      = @origen,
                        moneda_id               = @moneda,
                        monto_solicitado        = @monto,
                        plazo                   = @plazo,
                        tasa_entrada            = @tasa,
                        observaciones           = @obs,
                        permitir_pagos_anticipados = @permitir_pagos_anticipados,
                        modificado_por          = @user,
                        fecha_modificacion      = SYSUTCDATETIME()
                    WHERE id = @id;"

                    Using cmd As New SqlCommand(sql, con, tx)
                        cmd.Parameters.Add("@pf", SqlDbType.Int).Value = pf.Value
                        cmd.Parameters.Add("@periodo", SqlDbType.Int).Value = If(periodoId.HasValue, CType(periodoId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@canal", SqlDbType.Int).Value = If(canalId.HasValue, CType(canalId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@dest", SqlDbType.Int).Value = If(destId.HasValue, CType(destId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@origen", SqlDbType.Int).Value = If(origenId.HasValue, CType(origenId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@moneda", SqlDbType.Int).Value = If(monedaId.HasValue, CType(monedaId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = SqlDecOrNull(monto)
                        cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = If(plazo.HasValue, CType(plazo.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = SqlDecOrNull(tasa)
                        cmd.Parameters.Add("@obs", SqlDbType.NVarChar, 1000).Value = If(obs Is Nothing, CType(DBNull.Value, Object), obs)
                        cmd.Parameters.Add("@permitir_pagos_anticipados", SqlDbType.Bit).Value = permitirPagosAnticipados
                        cmd.Parameters.Add("@user", SqlDbType.NVarChar, 100).Value = NowUser()
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = sid.Value

                        Dim n = cmd.ExecuteNonQuery()
                        If n = 0 Then Throw New Exception("No se actualizó la solicitud.")
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Operación actualizada.", .solicitud_id = sid.Value})
                Catch ex As Exception
                    tx.Rollback()
                    If TypeOf ex Is ValidationHttpException Then
                        Dim vex = CType(ex, ValidationHttpException)
                        WriteErrorWithStatus(ctx, vex.StatusCode, vex.Message)
                    Else
                        WriteError(ctx, "No se pudo actualizar operación: " & ex.Message)
                    End If
                End Try
            End Using
        End Using
    End Sub

    Private Function ToBoolBit(value As String, Optional defaultValue As Boolean = False) As Boolean
        If String.IsNullOrWhiteSpace(value) Then Return defaultValue

        Dim v As String = value.Trim().ToLowerInvariant()

        If v = "1" OrElse v = "true" OrElse v = "si" OrElse v = "sí" OrElse v = "on" Then
            Return True
        End If

        If v = "0" OrElse v = "false" OrElse v = "no" OrElse v = "off" Then
            Return False
        End If

        Return defaultValue
    End Function

    '====================================================
    ' ACCIÓN: ACTUALIZAR_RELACIONES
    '====================================================
    Private Sub HandleActualizarRelaciones(ctx As HttpContext)
        Dim json = ReadJsonBody(ctx)
        Dim sid = ToInt(GetParam(ctx, json, "id"))
        If Not sid.HasValue Then sid = ToInt(GetParam(ctx, json, "solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "id (solicitud_id) es requerido.")
            Return
        End If

        Dim pf = ToInt(GetParam(ctx, json, "producto_financiero_id"))
        Dim canalId = ToInt(GetParam(ctx, json, "canal_pago_id"))
        Dim destId = ToInt(GetParam(ctx, json, "destino_recursos_id"))
        Dim origenId = ToInt(GetParam(ctx, json, "origen_recursos_id"))
        Dim medioId = ToInt(GetParam(ctx, json, "medio_contacto_id"))
        Dim monedaId = ToInt(GetParam(ctx, json, "moneda_id"))
        Dim clienteId = ToInt(GetParam(ctx, json, "cliente_id"))

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    If Not ExistsById(con, tx, "dbo.solicitud_credito", sid.Value) Then
                        WriteError(ctx, "Solicitud no encontrada.")
                        tx.Rollback() : Return
                    End If

                    If pf.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_producto_financiero", pf.Value) Then Throw New Exception("producto_financiero_id inválido.")
                    If canalId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_canal_pago", canalId.Value) Then Throw New Exception("canal_pago_id inválido.")
                    If destId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_destino_recursos", destId.Value) Then Throw New Exception("destino_recursos_id inválido.")
                    If origenId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_origen_recursos", origenId.Value) Then Throw New Exception("origen_recursos_id inválido.")
                    If medioId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_medios_contacto", medioId.Value) Then Throw New Exception("medio_contacto_id inválido.")
                    If monedaId.HasValue AndAlso Not ExistsById(con, tx, "dbo.catalogo_moneda_divisa", monedaId.Value) Then Throw New Exception("moneda_id inválido.")

                    Dim sql As String = "
                    UPDATE dbo.solicitud_credito
                    SET producto_financiero_id = COALESCE(@pf, producto_financiero_id),
                        canal_pago_id           = COALESCE(@canal, canal_pago_id),
                        destino_recursos_id     = COALESCE(@dest, destino_recursos_id),
                        origen_recursos_id      = COALESCE(@origen, origen_recursos_id),
                        medio_contacto_id       = COALESCE(@medio, medio_contacto_id),
                        moneda_id               = COALESCE(@moneda, moneda_id),
                        cliente_id              = COALESCE(@cliente, cliente_id),
                        modificado_por          = @user,
                        fecha_modificacion      = SYSUTCDATETIME()
                    WHERE id = @id;"

                    Using cmd As New SqlCommand(sql, con, tx)
                        cmd.Parameters.Add("@pf", SqlDbType.Int).Value = If(pf.HasValue, CType(pf.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@canal", SqlDbType.Int).Value = If(canalId.HasValue, CType(canalId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@dest", SqlDbType.Int).Value = If(destId.HasValue, CType(destId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@origen", SqlDbType.Int).Value = If(origenId.HasValue, CType(origenId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@medio", SqlDbType.Int).Value = If(medioId.HasValue, CType(medioId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@moneda", SqlDbType.Int).Value = If(monedaId.HasValue, CType(monedaId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@cliente", SqlDbType.Int).Value = If(clienteId.HasValue, CType(clienteId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@user", SqlDbType.NVarChar, 100).Value = NowUser()
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = sid.Value
                        Dim n = cmd.ExecuteNonQuery()
                        If n = 0 Then Throw New Exception("No se actualizaron relaciones.")
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Relaciones actualizadas.", .solicitud_id = sid.Value})
                Catch ex As Exception
                    tx.Rollback()
                    WriteError(ctx, "No se pudieron actualizar relaciones: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: LISTAR
    '====================================================
    Private Sub HandleListar(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim clienteId = ToInt(ctx.Request("cliente_id"))
        Dim estatus = ToStr(ctx.Request("estatus"))

        Dim page As Integer = 1, pageSize As Integer = 20
        Integer.TryParse(If(ctx.Request("page"), "1"), page)
        Integer.TryParse(If(ctx.Request("pageSize"), "20"), pageSize)
        If page < 1 Then page = 1
        If pageSize < 1 Then pageSize = 20
        Dim offset As Integer = (page - 1) * pageSize

        Dim whereSql As String = " WHERE 1=1 "
        If Not String.IsNullOrEmpty(q) Then whereSql &= " AND ISNULL(observaciones,'') LIKE @q "
        If clienteId.HasValue Then whereSql &= " AND ISNULL(cliente_id,0) = @cliente "
        If Not String.IsNullOrEmpty(estatus) Then whereSql &= " AND ISNULL(estatus,'') = @estatus "

        Using con = GetConn()
            con.Open()

            Dim total As Integer = 0
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.solicitud_credito " & whereSql, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 1024).Value = "%" & q & "%"
                If clienteId.HasValue Then cCount.Parameters.Add("@cliente", SqlDbType.Int).Value = clienteId.Value
                If Not String.IsNullOrEmpty(estatus) Then cCount.Parameters.Add("@estatus", SqlDbType.NVarChar, 30).Value = estatus
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            Dim sql As String = "
                SELECT id,
                       cliente_id,
                       producto_financiero_id,
                       producto_financiero_periodo_id,
                       canal_pago_id,
                       destino_recursos_id,
                       origen_recursos_id,
                       medio_contacto_id,
                       moneda_id,
                       monto_solicitado,
                       plazo,
                       tasa_entrada,
                       observaciones,
                       permitir_pagos_anticipados,
                       pld_puntaje_total,
                       pld_nivel_riesgo_pld,
                       pld_metodologia_version,
                       pld_fecha_calculo,
                       estatus,
                       activo,
                       creado_por,
                       fecha_creacion,
                       modificado_por,
                       fecha_modificacion
                FROM dbo.solicitud_credito " & whereSql & "
                ORDER BY id DESC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"

            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 1024).Value = "%" & q & "%"
                If clienteId.HasValue Then cmd.Parameters.Add("@cliente", SqlDbType.Int).Value = clienteId.Value
                If Not String.IsNullOrEmpty(estatus) Then cmd.Parameters.Add("@estatus", SqlDbType.NVarChar, 30).Value = estatus
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize

                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        Dim row As New Dictionary(Of String, Object)
                        For i = 0 To rd.FieldCount - 1
                            row(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                        Next
                        rows.Add(row)
                    End While

                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: FINALIZAR
    '====================================================
    Private Sub HandleFinalizar(ctx As HttpContext)
        Dim json = ReadJsonBody(ctx)
        Dim sid = ToInt(GetParam(ctx, json, "id"))
        If Not sid.HasValue Then sid = ToInt(GetParam(ctx, json, "solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "id (solicitud_id) es requerido.")
            Return
        End If

        Dim estatus As String = ToStr(GetParam(ctx, json, "estatus"))
        If String.IsNullOrEmpty(estatus) Then estatus = "FINALIZADA"

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    If Not ExistsById(con, tx, "dbo.solicitud_credito", sid.Value) Then
                        WriteError(ctx, "Solicitud no encontrada.")
                        tx.Rollback() : Return
                    End If

                    Dim sql As String = "
                        UPDATE dbo.solicitud_credito
                        SET estatus = @estatus,
                            modificado_por = @user,
                            fecha_modificacion = SYSUTCDATETIME()
                        WHERE id = @id;"

                    Using cmd As New SqlCommand(sql, con, tx)
                        cmd.Parameters.Add("@estatus", SqlDbType.NVarChar, 30).Value = estatus
                        cmd.Parameters.Add("@user", SqlDbType.NVarChar, 100).Value = NowUser()
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = sid.Value
                        Dim n = cmd.ExecuteNonQuery()
                        If n = 0 Then Throw New Exception("No se actualizó estatus.")
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Estatus actualizado.", .solicitud_id = sid.Value, .estatus = estatus})
                Catch ex As Exception
                    tx.Rollback()
                    WriteError(ctx, "No se pudo actualizar estatus: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    '====================================================
    ' ACCIÓN: APLICAR_PLD
    ' (Se mantiene intacto; está comentado en este handler)
    '====================================================

    Private Function ExistsById(con As SqlConnection, tx As SqlTransaction, fullTableName As String, id As Integer) As Boolean
        Dim sql As String = "SELECT 1 FROM " & fullTableName & " WITH (NOLOCK) WHERE id = @id;"
        Using cmd As New SqlCommand(sql, con, tx)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
            Dim o = cmd.ExecuteScalar()
            Return (o IsNot Nothing)
        End Using
    End Function

    Private Class FactorDetalle
        Public Property origen_tipo As String
        Public Property referencia_id As Integer?
        Public Property impacto As Decimal
        Public Property probabilidad As Decimal
        Public Property nivel As Decimal
    End Class

    Private Class SolicitudAmortData
        Public Property SolicitudId As Integer
        Public Property ProductoFinancieroId As Integer
        Public Property ProductoFinancieroPeriodoId As Integer
        Public Property MontoSolicitado As Decimal
        Public Property Plazo As Integer
        Public Property TasaEntradaPct As Decimal
        Public Property FechaCreacion As DateTime?
    End Class

    Private Class PeriodoAmortData
        Public Property TipoPeriodo As String
        Public Property Plazo As Integer
        Public Property TasaInteresFrac As Decimal
    End Class

    Private Class DetalleProductoAmortData
        Public Property IvaPct As Decimal
        Public Property IvaComisionPct As Decimal
        Public Property BaseCalculo As Decimal
        Public Property RedondearPagoFijo As Boolean
        Public Property CentavosParaRedondeo As Decimal
        Public Property ComisionAperturaPct As Decimal
        Public Property AmortizarComisionApertura As Boolean
    End Class

End Class
