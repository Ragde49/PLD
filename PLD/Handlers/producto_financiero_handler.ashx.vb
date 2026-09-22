Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text
Imports System.IO
Imports System.Collections
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Globalization

Public Class ProductoFinancieroHandler
    Implements IHttpHandler
    Implements System.Web.SessionState.IRequiresSessionState

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"

        Try
            Dim action As String = (context.Request("action") & "").Trim().ToLowerInvariant()
            If String.IsNullOrWhiteSpace(action) Then
                If (context.Request.HttpMethod & "").ToUpperInvariant() = "GET" Then
                    action = "list"
                End If
            End If

            Select Case action
                Case "list"
                    HandleList(context)
                Case "get"
                    HandleGet(context)
                Case "create"
                    HandleCreate(context) ' Alta MAESTRO + DETALLE + PERIODOS (periodos obligatorio)
                Case "update"
                    HandleUpdate(context)
                Case "delete"
                    HandleDelete(context)

                Case "periodos_listar"
                    PeriodosListar(context)
                Case "periodos_guardar"
                    PeriodosGuardar(context)
                Case "periodos_actualizar"
                    PeriodosActualizar(context)
                Case "periodos_toggle"
                    PeriodosToggle(context)
                Case "periodos_eliminar"
                    PeriodosEliminar(context)
                Case "periodos_combo"
                    PeriodosCombo(context)
                Case Else
                    WriteJson(context, New With {.success = False, .message = "Acción no soportada: " & action})
            End Select

        Catch ex As Exception
            WriteJson(context, New With {.success = False, .message = "Error en handler: " & ex.Message})
        End Try
    End Sub

    ' ==========================
    ' CONEXIÓN (sin fallback)
    ' ==========================
    Private Function GetConnString() As String
        Dim cs = ConfigurationManager.ConnectionStrings("PLDConnection")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New Exception("No existe connectionString 'PLDConnection' en web.config.")
        End If
        Return cs.ConnectionString
    End Function

    ' ==========================
    ' USUARIO (sin fallback)
    ' ==========================
    Private Function GetUsuarioRequired(ctx As HttpContext) As String
        ' NOTA (modo sin usuarios): Para este módulo, si no existe sesión/usuario,
        ' devolvemos un valor por defecto para no bloquear el CRUD.
        ' En cuanto activen autenticación, pueden volver a forzar el requerido.
        Try
            If ctx Is Nothing Then Return "SYSTEM"

            Dim u As Object = Nothing
            If ctx.Session IsNot Nothing Then
                u = ctx.Session("usuario")
                If u IsNot Nothing AndAlso (u.ToString().Trim() <> "") Then Return u.ToString().Trim()

                u = ctx.Session("Usuario")
                If u IsNot Nothing AndAlso (u.ToString().Trim() <> "") Then Return u.ToString().Trim()
            End If
        Catch
            ' Ignorar y usar fallback
        End Try

        Return "SYSTEM"
    End Function

    ' ==========================
    ' JSON helpers
    ' ==========================
    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.ContentType = "application/json; charset=utf-8"
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue
        ctx.Response.Write(js.Serialize(obj))
    End Sub

    Private Function ReadRequestBody(ctx As HttpContext) As String
        ctx.Request.InputStream.Position = 0
        Using sr As New StreamReader(ctx.Request.InputStream, Encoding.UTF8)
            Return sr.ReadToEnd()
        End Using
    End Function

    ' ==========================
    ' Conversiones (estrictas)
    ' ==========================
    Private Function GetReqString(d As IDictionary, key As String) As String
        If d Is Nothing OrElse Not d.Contains(key) OrElse d(key) Is Nothing Then Throw New Exception("Falta campo requerido: " & key)
        Dim s As String = d(key).ToString().Trim()
        If s = "" Then Throw New Exception("Campo requerido vacío: " & key)
        Return s
    End Function

    Private Function GetOptString(d As IDictionary, key As String) As String
        If d Is Nothing OrElse Not d.Contains(key) OrElse d(key) Is Nothing Then Return Nothing
        Dim s As String = d(key).ToString().Trim()
        If s = "" Then Return Nothing
        Return s
    End Function

    Private Function GetReqInt(d As IDictionary, key As String) As Integer
        Dim s As String = GetReqString(d, key)
        Dim v As Integer
        If Not Integer.TryParse(s, v) Then Throw New Exception("Campo inválido (int): " & key)
        Return v
    End Function

    Private Function GetOptInt(d As IDictionary, key As String) As Integer?
        If d Is Nothing OrElse Not d.Contains(key) OrElse d(key) Is Nothing Then Return Nothing
        Dim s As String = d(key).ToString().Trim()
        If s = "" Then Return Nothing
        Dim v As Integer
        If Not Integer.TryParse(s, v) Then Throw New Exception("Campo inválido (int): " & key)
        Return v
    End Function

    Private Function GetReqDec(d As IDictionary, key As String) As Decimal
        Dim s As String = GetReqString(d, key).Replace(",", ".")
        Dim v As Decimal
        If Not Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, v) Then Throw New Exception("Campo inválido (decimal): " & key)
        Return v
    End Function

    Private Function GetOptDec(d As IDictionary, key As String) As Decimal?
        If d Is Nothing OrElse Not d.Contains(key) OrElse d(key) Is Nothing Then Return Nothing
        Dim s As String = d(key).ToString().Trim()
        If s = "" Then Return Nothing
        s = s.Replace(",", ".")
        Dim v As Decimal
        If Not Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, v) Then Throw New Exception("Campo inválido (decimal): " & key)
        Return v
    End Function

    Private Function GetReqBool(d As IDictionary, key As String) As Boolean
        If d Is Nothing OrElse Not d.Contains(key) OrElse d(key) Is Nothing Then Throw New Exception("Falta campo requerido (bool): " & key)
        Dim s As String = d(key).ToString().Trim().ToLowerInvariant()
        If s = "1" OrElse s = "true" OrElse s = "si" OrElse s = "yes" Then Return True
        If s = "0" OrElse s = "false" OrElse s = "no" Then Return False
        Throw New Exception("Campo inválido (bool): " & key)
    End Function

    Private Function IsTipoPeriodoValido(v As String) As Boolean
        If String.IsNullOrWhiteSpace(v) Then Return False
        Dim x = v.Trim()
        Return (String.Compare(x, "Semanal", StringComparison.OrdinalIgnoreCase) = 0) _
            OrElse (String.Compare(x, "Quincenal", StringComparison.OrdinalIgnoreCase) = 0) _
            OrElse (String.Compare(x, "Mensual", StringComparison.OrdinalIgnoreCase) = 0)
    End Function

    ' ==========================
    ' LIST
    ' ==========================
    Private Sub HandleList(ctx As HttpContext)
        Dim q As String = (ctx.Request("q") & "").Trim()
        Dim start As Integer = 0
        Dim length As Integer = 10
        Integer.TryParse((ctx.Request("start") & "").Trim(), start)
        Integer.TryParse((ctx.Request("length") & "").Trim(), length)

        Using cn As New SqlConnection(GetConnString())
            cn.Open()

            Dim total As Integer
            Using cmdCount As New SqlCommand("SELECT COUNT(1) FROM dbo.vw_producto_financiero_completo", cn)
                total = Convert.ToInt32(cmdCount.ExecuteScalar())
            End Using

            Dim whereClause As String = ""
            Dim parms As New List(Of SqlParameter)

            If q <> "" Then
                whereClause = " WHERE (UPPER(ISNULL(descripcion_larga,'')) LIKE @q OR UPPER(ISNULL(nombre_credito,'')) LIKE @q)"
                parms.Add(New SqlParameter("@q", SqlDbType.NVarChar) With {.Value = "%" & q.ToUpperInvariant() & "%"})
            End If

            Dim filtered As Integer = total
            If whereClause <> "" Then
                Using cmdF As New SqlCommand("SELECT COUNT(1) FROM dbo.vw_producto_financiero_completo " & whereClause, cn)
                    cmdF.Parameters.AddRange(parms.ToArray())
                    filtered = Convert.ToInt32(cmdF.ExecuteScalar())
                End Using
            End If

            Dim sql As New StringBuilder()
            sql.Append("SELECT * FROM dbo.vw_producto_financiero_completo ")
            If whereClause <> "" Then sql.Append(whereClause)
            sql.Append(" ORDER BY producto_id OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;")

            Using cmd As New SqlCommand(sql.ToString(), cn)
                cmd.Parameters.AddRange(parms.ToArray())
                cmd.Parameters.Add("@start", SqlDbType.Int).Value = start
                cmd.Parameters.Add("@length", SqlDbType.Int).Value = length

                Dim dt As New DataTable()
                Using rdr = cmd.ExecuteReader()
                    dt.Load(rdr)
                End Using

                Dim rows As New List(Of Dictionary(Of String, Object))()
                For Each dr As DataRow In dt.Rows
                    Dim dict As New Dictionary(Of String, Object)()
                    For Each c As DataColumn In dt.Columns
                        dict(c.ColumnName) = If(dr.IsNull(c), Nothing, dr(c))
                    Next
                    rows.Add(dict)
                Next

                WriteJson(ctx, New With {.success = True, .recordsTotal = total, .recordsFiltered = filtered, .data = rows})
            End Using
        End Using
    End Sub

    ' ==========================
    ' GET
    ' ==========================
    Private Sub HandleGet(ctx As HttpContext)
        Dim id As Integer
        If Not Integer.TryParse((ctx.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(ctx, New With {.success = False, .message = "Parámetro id inválido."})
            Return
        End If

        Using cn As New SqlConnection(GetConnString())
            cn.Open()

            Dim producto As Object = Nothing
            Using cmd As New SqlCommand("SELECT TOP 1 * FROM dbo.vw_producto_financiero_completo WHERE producto_id = @id", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim dt As New DataTable()
                Using rdr = cmd.ExecuteReader()
                    dt.Load(rdr)
                End Using
                If dt.Rows.Count > 0 Then
                    Dim dict As New Dictionary(Of String, Object)()
                    For Each c As DataColumn In dt.Columns
                        dict(c.ColumnName) = If(dt.Rows(0).IsNull(c), Nothing, dt.Rows(0)(c))
                    Next
                    producto = dict
                End If
            End Using

            Dim detalle As Object = Nothing
            Using cmdD As New SqlCommand("SELECT TOP 1 * FROM dbo.detalles_del_producto WHERE producto_id = @id", cn)
                cmdD.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim dtD As New DataTable()
                Using rdr = cmdD.ExecuteReader()
                    dtD.Load(rdr)
                End Using
                If dtD.Rows.Count > 0 Then
                    Dim d As New Dictionary(Of String, Object)()
                    For Each c As DataColumn In dtD.Columns
                        d(c.ColumnName) = If(dtD.Rows(0).IsNull(c), Nothing, dtD.Rows(0)(c))
                    Next
                    detalle = d
                End If
            End Using

            Dim planeacion As New List(Of Dictionary(Of String, Object))()
            Using cmdP As New SqlCommand("SELECT id, orden, tipo, referencia_id, descripcion, activo FROM dbo.producto_planeacion WHERE producto_id = @id ORDER BY orden", cn)
                cmdP.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim dtP As New DataTable()
                Using rdr = cmdP.ExecuteReader()
                    dtP.Load(rdr)
                End Using
                For Each r As DataRow In dtP.Rows
                    Dim row As New Dictionary(Of String, Object)()
                    For Each c As DataColumn In dtP.Columns
                        row(c.ColumnName) = If(r.IsNull(c), Nothing, r(c))
                    Next
                    planeacion.Add(row)
                Next
            End Using

            WriteJson(ctx, New With {.success = True, .producto = producto, .detalle = detalle, .planeacion = planeacion})
        End Using
    End Sub

    ' ==========================
    ' CREATE (MAESTRO + DETALLE + PERIODOS)
    ' periodos es OBLIGATORIO
    ' ==========================
    Private Sub HandleCreate(ctx As HttpContext)
        Dim usuario As String = GetUsuarioRequired(ctx)

        Dim body = ReadRequestBody(ctx)
        If String.IsNullOrWhiteSpace(body) Then
            WriteJson(ctx, New With {.success = False, .message = "Body vacío."})
            Return
        End If

        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue

        Dim data As Dictionary(Of String, Object)
        Try
            data = js.Deserialize(Of Dictionary(Of String, Object))(body)
        Catch ex As Exception
            WriteJson(ctx, New With {.success = False, .message = "JSON inválido: " & ex.Message})
            Return
        End Try

        Try
            ' ---------- MAESTRO ----------
            Dim master As IDictionary = data

            Dim descripcion_larga As String = GetReqString(master, "descripcion_larga")
            Dim tipo_credito_id As Integer? = GetOptInt(master, "tipo_credito_id")
            Dim producto_id As Integer? = GetOptInt(master, "producto_id")
            Dim regimen_fiscal_id As Integer? = GetOptInt(master, "regimen_fiscal_id")
            Dim moneda_id As Integer? = GetOptInt(master, "moneda_id")

            Dim monto_minimo As Decimal? = GetOptDec(master, "monto_minimo")
            Dim monto_maximo As Decimal? = GetOptDec(master, "monto_maximo")
            Dim monto_apertura As Decimal? = GetOptDec(master, "monto_apertura")
            Dim porcentaje_seguro As Decimal? = GetOptDec(master, "porcentaje_seguro")
            Dim costo_gestion As Decimal? = GetOptDec(master, "costo_gestion")
            Dim observaciones_generales As String = GetOptString(master, "observaciones_generales")

            Dim impacto As Integer = GetReqInt(master, "impacto")
            Dim probabilidad As Integer = GetReqInt(master, "probabilidad")
            Dim nivel_riesgo_pld As Decimal = GetReqDec(master, "nivel_riesgo_pld")

            ' Para consistencia con “varias”
            Dim tipo_periodo_maestro As String = GetReqString(master, "tipo_periodo") ' aquí debe venir "Varias"
            If tipo_periodo_maestro.Trim().ToLowerInvariant() <> "varias" Then
                Throw New Exception("En alta maestro-detalle, tipo_periodo debe ser 'Varias'.")
            End If

            ' ---------- DETALLE (obligatorio) ----------
            If Not data.ContainsKey("detalle") OrElse data("detalle") Is Nothing Then
                Throw New Exception("Falta objeto requerido: detalle")
            End If
            Dim detalle As IDictionary = TryCast(data("detalle"), IDictionary)
            If detalle Is Nothing Then Throw New Exception("detalle inválido (no es objeto).")

            ' Campos NOT NULL en detalles_del_producto (obligatorios)
            Dim tasa_interes_anual As Decimal = GetReqDec(detalle, "tasa_interes_anual")
            Dim tasa_interes_mensual As Decimal = GetReqDec(detalle, "tasa_interes_mensual")
            Dim iva As Decimal = GetReqDec(detalle, "iva")
            Dim factor_moratorio As Boolean = GetReqBool(detalle, "factor_moratorio")
            Dim amortizar_comision_apertura As Boolean = GetReqBool(detalle, "amortizar_comision_apertura")
            Dim amortizar_comision_gestion As Boolean = GetReqBool(detalle, "amortizar_comision_gestion")
            Dim aplica_monto_seguro As Boolean = GetReqBool(detalle, "aplica_monto_seguro")
            Dim usar_redondeo_centavos As Boolean = GetReqBool(detalle, "usar_redondeo_centavos")
            Dim redondear_pago_fijo As Boolean = GetReqBool(detalle, "redondear_pago_fijo")
            Dim iva_sobre_total As Boolean = GetReqBool(detalle, "iva_sobre_total")
            Dim modificador_monto_credito As Boolean = GetReqBool(detalle, "modificador_monto_credito")



            ' tipo_periodo (detalle) debe cumplir CK_detalles_tipo_periodo: COMPLETO/FIJO/NULL
            Dim tipo_periodo_detalle_norm As String = Nothing
            Dim tipo_periodo_detalle_raw As String = GetOptString(detalle, "tipo_periodo")
            If Not String.IsNullOrWhiteSpace(tipo_periodo_detalle_raw) Then
                Dim t As String = tipo_periodo_detalle_raw.Trim().ToUpperInvariant()
                If t <> "COMPLETO" AndAlso t <> "FIJO" Then
                    Throw New Exception("tipo_periodo (detalle) inválido. Valores permitidos: COMPLETO, FIJO.")
                End If
                tipo_periodo_detalle_norm = t
            End If

            ' Nuevos NOT NULL (obligatorios)
            Dim aplica_cargos_administrativos As Boolean = GetReqBool(detalle, "aplica_cargos_administrativos")
            Dim amortizar_cargos_administrativos As Boolean = GetReqBool(detalle, "amortizar_cargos_administrativos")
            Dim aplica_cargo_multa_cobranza As Boolean = GetReqBool(detalle, "aplica_cargo_multa_cobranza")
            Dim aplica_comision_administracion As Boolean = GetReqBool(detalle, "aplica_comision_administracion")
            Dim aplica_comision_investigacion As Boolean = GetReqBool(detalle, "aplica_comision_investigacion")

            ' ---------- PERIODOS (obligatorio) ----------
            If Not data.ContainsKey("periodos") OrElse data("periodos") Is Nothing Then
                Throw New Exception("Falta arreglo requerido: periodos")
            End If
            Dim periodos As ArrayList = TryCast(data("periodos"), ArrayList)
            If periodos Is Nothing OrElse periodos.Count = 0 Then
                Throw New Exception("periodos debe contener al menos 1 registro.")
            End If

            ' ---------- PLANEACION (opcional) ----------
            Dim planeacionList As ArrayList = Nothing
            If data.ContainsKey("planeacion") Then
                planeacionList = TryCast(data("planeacion"), ArrayList)
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Using tx = cn.BeginTransaction()
                    Try
                        ' 1) Insert maestro (catalogo_producto_financiero)
                        Dim insertProdSql As String =
"INSERT INTO dbo.catalogo_producto_financiero
 (descripcion_larga, tipo_credito_id, producto_id, regimen_fiscal_id, moneda_id,
  plazo, tipo_periodo, tasa_interes, monto_minimo, monto_maximo, monto_apertura, porcentaje_seguro, costo_gestion,
  impacto, probabilidad, nivel_riesgo_pld, observaciones_generales,
  activo, creado_por, fecha_creacion)
VALUES
 (@descripcion_larga, @tipo_credito_id, @producto_id, @regimen_fiscal_id, @moneda_id,
  NULL, @tipo_periodo, NULL, @monto_minimo, @monto_maximo, @monto_apertura, @porcentaje_seguro, @costo_gestion,
  @impacto, @probabilidad, @nivel_riesgo_pld, @observaciones_generales,
  1, @usr, SYSUTCDATETIME());
SELECT SCOPE_IDENTITY();"

                        Dim newId As Integer
                        Using cmd As New SqlCommand(insertProdSql, cn, tx)
                            cmd.Parameters.Add("@descripcion_larga", SqlDbType.NVarChar).Value = descripcion_larga
                            cmd.Parameters.Add("@tipo_credito_id", SqlDbType.Int).Value = If(tipo_credito_id.HasValue, CType(tipo_credito_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@producto_id", SqlDbType.Int).Value = If(producto_id.HasValue, CType(producto_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@regimen_fiscal_id", SqlDbType.Int).Value = If(regimen_fiscal_id.HasValue, CType(regimen_fiscal_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@moneda_id", SqlDbType.Int).Value = If(moneda_id.HasValue, CType(moneda_id.Value, Object), DBNull.Value)

                            cmd.Parameters.Add("@tipo_periodo", SqlDbType.NVarChar, 50).Value = tipo_periodo_maestro

                            cmd.Parameters.Add("@monto_minimo", SqlDbType.Decimal).Value = If(monto_minimo.HasValue, CType(monto_minimo.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_minimo").Precision = 18 : cmd.Parameters("@monto_minimo").Scale = 2

                            cmd.Parameters.Add("@monto_maximo", SqlDbType.Decimal).Value = If(monto_maximo.HasValue, CType(monto_maximo.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_maximo").Precision = 18 : cmd.Parameters("@monto_maximo").Scale = 2

                            cmd.Parameters.Add("@monto_apertura", SqlDbType.Decimal).Value = If(monto_apertura.HasValue, CType(monto_apertura.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_apertura").Precision = 18 : cmd.Parameters("@monto_apertura").Scale = 2

                            cmd.Parameters.Add("@porcentaje_seguro", SqlDbType.Decimal).Value = If(porcentaje_seguro.HasValue, CType(porcentaje_seguro.Value, Object), DBNull.Value)
                            cmd.Parameters("@porcentaje_seguro").Precision = 8 : cmd.Parameters("@porcentaje_seguro").Scale = 4

                            cmd.Parameters.Add("@costo_gestion", SqlDbType.Decimal).Value = If(costo_gestion.HasValue, CType(costo_gestion.Value, Object), DBNull.Value)
                            cmd.Parameters("@costo_gestion").Precision = 18 : cmd.Parameters("@costo_gestion").Scale = 2

                            cmd.Parameters.Add("@impacto", SqlDbType.Int).Value = impacto
                            cmd.Parameters.Add("@probabilidad", SqlDbType.Int).Value = probabilidad
                            cmd.Parameters.Add("@nivel_riesgo_pld", SqlDbType.Decimal).Value = nivel_riesgo_pld
                            cmd.Parameters("@nivel_riesgo_pld").Precision = 5 : cmd.Parameters("@nivel_riesgo_pld").Scale = 2

                            cmd.Parameters.Add("@observaciones_generales", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(observaciones_generales), CType(DBNull.Value, Object), observaciones_generales)
                            cmd.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario

                            newId = Convert.ToInt32(Convert.ToDecimal(cmd.ExecuteScalar()))
                        End Using

                        ' 2) Insert detalle (detalles_del_producto) — incluyendo nuevos NOT NULL
                        Dim insertDetSql As String =
"INSERT INTO dbo.detalles_del_producto
 (producto_id,
  tasa_interes_anual, tasa_interes_anual_detallada, tasa_interes_mensual,
  iva, iva_comision,
  capital_minimo, capital_maximo, plazo_minimo, plazo_maximo,
  vencimiento, calculo,
  factor_moratorio, tasa_moratoria, iva_moratoria, dias_gracia,
  comision_apertura, comision_apertura_detallada, amortizar_comision_apertura,
  comision_gestion, amortizar_comision_gestion,
  aplica_monto_seguro, comision_cancelacion, otros_gastos,
  usar_redondeo_centavos, base_calculo, tipo_periodo, calculo_fecha_exigible,
  centavos_para_redondeo, redondear_pago_fijo, aplicacion_de_tasa,
  iva_sobre_total, modificador_monto_credito, notas,
  activo, creado_por, fecha_creacion,
  aplica_cargos_administrativos, amortizar_cargos_administrativos, aplica_cargo_multa_cobranza,
  aplica_comision_administracion, iva_comision_administracion, comision_administracion_valor, comision_administracion_detallada,
  aplica_comision_investigacion, iva_comision_investigacion, comision_investigacion_detallada)
VALUES
 (@producto_id,
  @tasa_interes_anual, @tasa_interes_anual_detallada, @tasa_interes_mensual,
  @iva, @iva_comision,
  @capital_minimo, @capital_maximo, @plazo_minimo, @plazo_maximo,
  @vencimiento, @calculo,
  @factor_moratorio, @tasa_moratoria, @iva_moratoria, @dias_gracia,
  @comision_apertura, @comision_apertura_detallada, @amortizar_comision_apertura,
  @comision_gestion, @amortizar_comision_gestion,
  @aplica_monto_seguro, @comision_cancelacion, @otros_gastos,
  @usar_redondeo_centavos, @base_calculo, @tipo_periodo_det, @calculo_fecha_exigible,
  @centavos_para_redondeo, @redondear_pago_fijo, @aplicacion_de_tasa,
  @iva_sobre_total, @modificador_monto_credito, @notas,
  1, @usr, SYSUTCDATETIME(),
  @aplica_cargos_administrativos, @amortizar_cargos_administrativos, @aplica_cargo_multa_cobranza,
  @aplica_comision_administracion, @iva_comision_administracion, @comision_administracion_valor, @comision_administracion_detallada,
  @aplica_comision_investigacion, @iva_comision_investigacion, @comision_investigacion_detallada);"

                        Using cmdD As New SqlCommand(insertDetSql, cn, tx)
                            cmdD.Parameters.Add("@producto_id", SqlDbType.Int).Value = newId

                            cmdD.Parameters.Add("@tasa_interes_anual", SqlDbType.Decimal).Value = tasa_interes_anual
                            cmdD.Parameters("@tasa_interes_anual").Precision = 8 : cmdD.Parameters("@tasa_interes_anual").Scale = 4

                            cmdD.Parameters.Add("@tasa_interes_anual_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "tasa_interes_anual_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "tasa_interes_anual_detallada"))

                            cmdD.Parameters.Add("@tasa_interes_mensual", SqlDbType.Decimal).Value = tasa_interes_mensual
                            cmdD.Parameters("@tasa_interes_mensual").Precision = 8 : cmdD.Parameters("@tasa_interes_mensual").Scale = 4

                            cmdD.Parameters.Add("@iva", SqlDbType.Decimal).Value = iva
                            cmdD.Parameters("@iva").Precision = 5 : cmdD.Parameters("@iva").Scale = 2

                            Dim iva_comision As Decimal? = GetOptDec(detalle, "iva_comision")
                            cmdD.Parameters.Add("@iva_comision", SqlDbType.Decimal).Value = If(iva_comision.HasValue, CType(iva_comision.Value, Object), DBNull.Value)
                            cmdD.Parameters("@iva_comision").Precision = 5 : cmdD.Parameters("@iva_comision").Scale = 2

                            Dim capital_minimo As Decimal? = GetOptDec(detalle, "capital_minimo")
                            Dim capital_maximo As Decimal? = GetOptDec(detalle, "capital_maximo")
                            cmdD.Parameters.Add("@capital_minimo", SqlDbType.Decimal).Value = If(capital_minimo.HasValue, CType(capital_minimo.Value, Object), DBNull.Value)
                            cmdD.Parameters("@capital_minimo").Precision = 18 : cmdD.Parameters("@capital_minimo").Scale = 2
                            cmdD.Parameters.Add("@capital_maximo", SqlDbType.Decimal).Value = If(capital_maximo.HasValue, CType(capital_maximo.Value, Object), DBNull.Value)
                            cmdD.Parameters("@capital_maximo").Precision = 18 : cmdD.Parameters("@capital_maximo").Scale = 2

                            cmdD.Parameters.Add("@plazo_minimo", SqlDbType.Int).Value = If(GetOptInt(detalle, "plazo_minimo").HasValue, CType(GetOptInt(detalle, "plazo_minimo").Value, Object), DBNull.Value)
                            cmdD.Parameters.Add("@plazo_maximo", SqlDbType.Int).Value = If(GetOptInt(detalle, "plazo_maximo").HasValue, CType(GetOptInt(detalle, "plazo_maximo").Value, Object), DBNull.Value)

                            cmdD.Parameters.Add("@vencimiento", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "vencimiento")), CType(DBNull.Value, Object), GetOptString(detalle, "vencimiento"))
                            cmdD.Parameters.Add("@calculo", SqlDbType.NVarChar, 100).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "calculo")), CType(DBNull.Value, Object), GetOptString(detalle, "calculo"))

                            cmdD.Parameters.Add("@factor_moratorio", SqlDbType.Bit).Value = factor_moratorio

                            Dim tasa_moratoria As Decimal? = GetOptDec(detalle, "tasa_moratoria")
                            cmdD.Parameters.Add("@tasa_moratoria", SqlDbType.Decimal).Value = If(tasa_moratoria.HasValue, CType(tasa_moratoria.Value, Object), DBNull.Value)
                            cmdD.Parameters("@tasa_moratoria").Precision = 8 : cmdD.Parameters("@tasa_moratoria").Scale = 4

                            Dim iva_moratoria As Decimal? = GetOptDec(detalle, "iva_moratoria")
                            cmdD.Parameters.Add("@iva_moratoria", SqlDbType.Decimal).Value = If(iva_moratoria.HasValue, CType(iva_moratoria.Value, Object), DBNull.Value)
                            cmdD.Parameters("@iva_moratoria").Precision = 5 : cmdD.Parameters("@iva_moratoria").Scale = 2

                            cmdD.Parameters.Add("@dias_gracia", SqlDbType.Int).Value = If(GetOptInt(detalle, "dias_gracia").HasValue, CType(GetOptInt(detalle, "dias_gracia").Value, Object), DBNull.Value)

                            Dim comision_apertura As Decimal? = GetOptDec(detalle, "comision_apertura")
                            cmdD.Parameters.Add("@comision_apertura", SqlDbType.Decimal).Value = If(comision_apertura.HasValue, CType(comision_apertura.Value, Object), DBNull.Value)
                            cmdD.Parameters("@comision_apertura").Precision = 18 : cmdD.Parameters("@comision_apertura").Scale = 6

                            cmdD.Parameters.Add("@comision_apertura_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_apertura_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_apertura_detallada"))
                            cmdD.Parameters.Add("@amortizar_comision_apertura", SqlDbType.Bit).Value = amortizar_comision_apertura

                            Dim comision_gestion As Decimal? = GetOptDec(detalle, "comision_gestion")
                            cmdD.Parameters.Add("@comision_gestion", SqlDbType.Decimal).Value = If(comision_gestion.HasValue, CType(comision_gestion.Value, Object), DBNull.Value)
                            cmdD.Parameters("@comision_gestion").Precision = 18 : cmdD.Parameters("@comision_gestion").Scale = 6
                            cmdD.Parameters.Add("@amortizar_comision_gestion", SqlDbType.Bit).Value = amortizar_comision_gestion

                            cmdD.Parameters.Add("@aplica_monto_seguro", SqlDbType.Bit).Value = aplica_monto_seguro
                            cmdD.Parameters.Add("@comision_cancelacion", SqlDbType.NVarChar, 200).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_cancelacion")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_cancelacion"))

                            Dim otros_gastos As Decimal? = GetOptDec(detalle, "otros_gastos")
                            cmdD.Parameters.Add("@otros_gastos", SqlDbType.Decimal).Value = If(otros_gastos.HasValue, CType(otros_gastos.Value, Object), DBNull.Value)
                            cmdD.Parameters("@otros_gastos").Precision = 18 : cmdD.Parameters("@otros_gastos").Scale = 6

                            cmdD.Parameters.Add("@usar_redondeo_centavos", SqlDbType.Bit).Value = usar_redondeo_centavos
                            cmdD.Parameters.Add("@base_calculo", SqlDbType.Int).Value = If(GetOptInt(detalle, "base_calculo").HasValue, CType(GetOptInt(detalle, "base_calculo").Value, Object), DBNull.Value)
                            cmdD.Parameters.Add("@tipo_periodo_det", SqlDbType.NVarChar, 20).Value = If(tipo_periodo_detalle_norm Is Nothing, CType(DBNull.Value, Object), tipo_periodo_detalle_norm)
                            cmdD.Parameters.Add("@calculo_fecha_exigible", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "calculo_fecha_exigible")), CType(DBNull.Value, Object), GetOptString(detalle, "calculo_fecha_exigible"))
                            cmdD.Parameters.Add("@centavos_para_redondeo", SqlDbType.Int).Value = If(GetOptInt(detalle, "centavos_para_redondeo").HasValue, CType(GetOptInt(detalle, "centavos_para_redondeo").Value, Object), DBNull.Value)

                            cmdD.Parameters.Add("@redondear_pago_fijo", SqlDbType.Bit).Value = redondear_pago_fijo
                            cmdD.Parameters.Add("@aplicacion_de_tasa", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "aplicacion_de_tasa")), CType(DBNull.Value, Object), GetOptString(detalle, "aplicacion_de_tasa"))
                            cmdD.Parameters.Add("@iva_sobre_total", SqlDbType.Bit).Value = iva_sobre_total
                            cmdD.Parameters.Add("@modificador_monto_credito", SqlDbType.Bit).Value = modificador_monto_credito
                            cmdD.Parameters.Add("@notas", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "notas")), CType(DBNull.Value, Object), GetOptString(detalle, "notas"))

                            cmdD.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario

                            cmdD.Parameters.Add("@aplica_cargos_administrativos", SqlDbType.Bit).Value = aplica_cargos_administrativos
                            cmdD.Parameters.Add("@amortizar_cargos_administrativos", SqlDbType.Bit).Value = amortizar_cargos_administrativos
                            cmdD.Parameters.Add("@aplica_cargo_multa_cobranza", SqlDbType.Bit).Value = aplica_cargo_multa_cobranza

                            cmdD.Parameters.Add("@aplica_comision_administracion", SqlDbType.Bit).Value = aplica_comision_administracion

                            Dim iva_com_admin As Decimal? = GetOptDec(detalle, "iva_comision_administracion")
                            cmdD.Parameters.Add("@iva_comision_administracion", SqlDbType.Decimal).Value = If(iva_com_admin.HasValue, CType(iva_com_admin.Value, Object), DBNull.Value)
                            cmdD.Parameters("@iva_comision_administracion").Precision = 5 : cmdD.Parameters("@iva_comision_administracion").Scale = 2

                            Dim com_admin_val As Decimal? = GetOptDec(detalle, "comision_administracion_valor")
                            cmdD.Parameters.Add("@comision_administracion_valor", SqlDbType.Decimal).Value = If(com_admin_val.HasValue, CType(com_admin_val.Value, Object), DBNull.Value)
                            cmdD.Parameters("@comision_administracion_valor").Precision = 18 : cmdD.Parameters("@comision_administracion_valor").Scale = 6

                            cmdD.Parameters.Add("@comision_administracion_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_administracion_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_administracion_detallada"))

                            cmdD.Parameters.Add("@aplica_comision_investigacion", SqlDbType.Bit).Value = aplica_comision_investigacion

                            Dim iva_com_inv As Decimal? = GetOptDec(detalle, "iva_comision_investigacion")
                            cmdD.Parameters.Add("@iva_comision_investigacion", SqlDbType.Decimal).Value = If(iva_com_inv.HasValue, CType(iva_com_inv.Value, Object), DBNull.Value)
                            cmdD.Parameters("@iva_comision_investigacion").Precision = 5 : cmdD.Parameters("@iva_comision_investigacion").Scale = 2

                            cmdD.Parameters.Add("@comision_investigacion_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_investigacion_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_investigacion_detallada"))

                            cmdD.ExecuteNonQuery()
                        End Using

                        ' 3) Insert periodos (catalogo_producto_financiero_periodos)
                        Dim sqlPer As String =
"INSERT INTO dbo.catalogo_producto_financiero_periodos
 (producto_financiero_id, tipo_periodo, plazo, tasa_interes, activo, creado_por, fecha_creacion, modificado_por, fecha_modificacion)
VALUES
 (@producto, @tipo, @plazo, @tasa, 1, @usr, SYSUTCDATETIME(), @usr, SYSUTCDATETIME());"

                        For Each obj As Object In periodos
                            Dim p As IDictionary = TryCast(obj, IDictionary)
                            If p Is Nothing Then Throw New Exception("Elemento de periodos inválido.")

                            Dim tp As String = GetReqString(p, "tipo_periodo")
                            If Not IsTipoPeriodoValido(tp) Then Throw New Exception("tipo_periodo inválido en periodos: " & tp)

                            Dim pl As Integer = GetReqInt(p, "plazo")
                            If pl <= 0 Then Throw New Exception("plazo inválido en periodos.")

                            Dim tasa As Decimal = GetReqDec(p, "tasa_interes")
                            If tasa < 0D Then Throw New Exception("tasa_interes inválida en periodos.")

                            Using cmdP As New SqlCommand(sqlPer, cn, tx)
                                cmdP.Parameters.Add("@producto", SqlDbType.Int).Value = newId
                                cmdP.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = tp
                                cmdP.Parameters.Add("@plazo", SqlDbType.Int).Value = pl
                                cmdP.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                                cmdP.Parameters("@tasa").Precision = 18 : cmdP.Parameters("@tasa").Scale = 6
                                cmdP.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                                cmdP.ExecuteNonQuery()
                            End Using
                        Next

                        ' 4) Planeación (opcional)
                        If planeacionList IsNot Nothing AndAlso planeacionList.Count > 0 Then
                            Using cmdDel As New SqlCommand("DELETE FROM dbo.producto_planeacion WHERE producto_id = @p", cn, tx)
                                cmdDel.Parameters.Add("@p", SqlDbType.Int).Value = newId
                                cmdDel.ExecuteNonQuery()
                            End Using

                            For Each objPlane As Object In planeacionList
                                Dim pDict = TryCast(objPlane, IDictionary)
                                If pDict Is Nothing Then Continue For

                                Dim ordenVal As Integer = 0
                                Integer.TryParse((If(pDict.Contains("orden"), pDict("orden"), "0") & "").ToString(), ordenVal)

                                Dim tipoVal As String = If(pDict.Contains("tipo"), (pDict("tipo") & "").ToString().Trim(), Nothing)
                                Dim referenciaVal As Integer? = Nothing
                                If pDict.Contains("referencia_id") AndAlso pDict("referencia_id") IsNot Nothing Then
                                    Dim tmp As Integer
                                    If Integer.TryParse((pDict("referencia_id") & "").ToString(), tmp) Then referenciaVal = tmp
                                End If
                                Dim descripcionVal As String = If(pDict.Contains("descripcion"), (pDict("descripcion") & "").ToString().Trim(), Nothing)

                                Using cmdIns As New SqlCommand("INSERT INTO dbo.producto_planeacion (producto_id, orden, tipo, referencia_id, descripcion, activo, creado_por, fecha_creacion) VALUES (@producto_id, @orden, @tipo, @referencia_id, @descripcion, 1, @usr, SYSUTCDATETIME())", cn, tx)
                                    cmdIns.Parameters.Add("@producto_id", SqlDbType.Int).Value = newId
                                    cmdIns.Parameters.Add("@orden", SqlDbType.Int).Value = ordenVal
                                    cmdIns.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(tipoVal), CType(DBNull.Value, Object), tipoVal)
                                    cmdIns.Parameters.Add("@referencia_id", SqlDbType.Int).Value = If(referenciaVal.HasValue, CType(referenciaVal.Value, Object), DBNull.Value)
                                    cmdIns.Parameters.Add("@descripcion", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(descripcionVal), CType(DBNull.Value, Object), descripcionVal)
                                    cmdIns.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                                    cmdIns.ExecuteNonQuery()
                                End Using
                            Next
                        End If

                        tx.Commit()
                        WriteJson(ctx, New With {.success = True, .message = "Producto creado (maestro-detalle) con periodos.", .id = newId})
                    Catch ex As Exception
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As Exception
            WriteJson(ctx, New With {.success = False, .message = ex.Message})
        End Try
    End Sub

    ' ==========================
    ' UPDATE (maestro + detalle)
    ' ==========================
    Private Sub HandleUpdate(ctx As HttpContext)
        Dim usuario As String = GetUsuarioRequired(ctx)

        Dim id As Integer
        If Not Integer.TryParse((ctx.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(ctx, New With {.success = False, .message = "Parámetro id inválido."})
            Return
        End If

        Dim body = ReadRequestBody(ctx)
        If String.IsNullOrWhiteSpace(body) Then
            WriteJson(ctx, New With {.success = False, .message = "Body vacío."})
            Return
        End If

        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue

        Dim data As Dictionary(Of String, Object) = Nothing

        Try
            data = js.Deserialize(Of Dictionary(Of String, Object))(body)
        Catch ex As Exception
            WriteJson(ctx, New With {.success = False, .message = "JSON inválido: " & ex.Message})
            Return
        End Try

        If data Is Nothing Then
            WriteJson(ctx, New With {.success = False, .message = "Body vacío o JSON nulo."})
            Return
        End If

        ' PERIODOS opcional en update: si viene, se reemplaza el set de periodicidades del producto
        Dim periodosOpt As Object = Nothing
        Dim hasPeriodos As Boolean = False
        If data.ContainsKey("periodos") AndAlso data("periodos") IsNot Nothing Then
            periodosOpt = data("periodos")
            hasPeriodos = True
        End If


        Try
            Dim master As IDictionary = data

            Dim descripcion_larga As String = GetReqString(master, "descripcion_larga")
            Dim tipo_credito_id As Integer? = GetOptInt(master, "tipo_credito_id")
            Dim producto_id As Integer? = GetOptInt(master, "producto_id")
            Dim regimen_fiscal_id As Integer? = GetOptInt(master, "regimen_fiscal_id")
            Dim moneda_id As Integer? = GetOptInt(master, "moneda_id")

            Dim monto_minimo As Decimal? = GetOptDec(master, "monto_minimo")
            Dim monto_maximo As Decimal? = GetOptDec(master, "monto_maximo")
            Dim monto_apertura As Decimal? = GetOptDec(master, "monto_apertura")
            Dim porcentaje_seguro As Decimal? = GetOptDec(master, "porcentaje_seguro")
            Dim costo_gestion As Decimal? = GetOptDec(master, "costo_gestion")
            Dim observaciones_generales As String = GetOptString(master, "observaciones_generales")

            Dim impacto As Integer = GetReqInt(master, "impacto")
            Dim probabilidad As Integer = GetReqInt(master, "probabilidad")
            Dim nivel_riesgo_pld As Decimal = GetReqDec(master, "nivel_riesgo_pld")

            Dim tipo_periodo_maestro As String = GetReqString(master, "tipo_periodo")
            If tipo_periodo_maestro.Trim().ToLowerInvariant() <> "varias" Then
                Throw New Exception("En maestro-detalle, tipo_periodo debe ser 'Varias'.")
            End If

            Dim detalle As IDictionary = Nothing
            If data.ContainsKey("detalle") AndAlso data("detalle") IsNot Nothing Then
                detalle = TryCast(data("detalle"), IDictionary)
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Using tx = cn.BeginTransaction()
                    Try
                        Dim sqlU As String =
"UPDATE dbo.catalogo_producto_financiero
 SET descripcion_larga=@descripcion_larga,
     tipo_credito_id=@tipo_credito_id,
     producto_id=@producto_id,
     regimen_fiscal_id=@regimen_fiscal_id,
     moneda_id=@moneda_id,
     tipo_periodo=@tipo_periodo,
     monto_minimo=@monto_minimo,
     monto_maximo=@monto_maximo,
     monto_apertura=@monto_apertura,
     porcentaje_seguro=@porcentaje_seguro,
     costo_gestion=@costo_gestion,
     impacto=@impacto,
     probabilidad=@probabilidad,
     nivel_riesgo_pld=@nivel_riesgo_pld,
     observaciones_generales=@observaciones_generales,
     modificado_por=@usr,
     fecha_modificacion=SYSUTCDATETIME()
 WHERE id=@id;"

                        Using cmd As New SqlCommand(sqlU, cn, tx)
                            cmd.Parameters.Add("@descripcion_larga", SqlDbType.NVarChar).Value = descripcion_larga
                            cmd.Parameters.Add("@tipo_credito_id", SqlDbType.Int).Value = If(tipo_credito_id.HasValue, CType(tipo_credito_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@producto_id", SqlDbType.Int).Value = If(producto_id.HasValue, CType(producto_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@regimen_fiscal_id", SqlDbType.Int).Value = If(regimen_fiscal_id.HasValue, CType(regimen_fiscal_id.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@moneda_id", SqlDbType.Int).Value = If(moneda_id.HasValue, CType(moneda_id.Value, Object), DBNull.Value)

                            cmd.Parameters.Add("@tipo_periodo", SqlDbType.NVarChar, 50).Value = tipo_periodo_maestro

                            cmd.Parameters.Add("@monto_minimo", SqlDbType.Decimal).Value = If(monto_minimo.HasValue, CType(monto_minimo.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_minimo").Precision = 18 : cmd.Parameters("@monto_minimo").Scale = 2

                            cmd.Parameters.Add("@monto_maximo", SqlDbType.Decimal).Value = If(monto_maximo.HasValue, CType(monto_maximo.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_maximo").Precision = 18 : cmd.Parameters("@monto_maximo").Scale = 2

                            cmd.Parameters.Add("@monto_apertura", SqlDbType.Decimal).Value = If(monto_apertura.HasValue, CType(monto_apertura.Value, Object), DBNull.Value)
                            cmd.Parameters("@monto_apertura").Precision = 18 : cmd.Parameters("@monto_apertura").Scale = 2

                            cmd.Parameters.Add("@porcentaje_seguro", SqlDbType.Decimal).Value = If(porcentaje_seguro.HasValue, CType(porcentaje_seguro.Value, Object), DBNull.Value)
                            cmd.Parameters("@porcentaje_seguro").Precision = 8 : cmd.Parameters("@porcentaje_seguro").Scale = 4

                            cmd.Parameters.Add("@costo_gestion", SqlDbType.Decimal).Value = If(costo_gestion.HasValue, CType(costo_gestion.Value, Object), DBNull.Value)
                            cmd.Parameters("@costo_gestion").Precision = 18 : cmd.Parameters("@costo_gestion").Scale = 2

                            cmd.Parameters.Add("@impacto", SqlDbType.Int).Value = impacto
                            cmd.Parameters.Add("@probabilidad", SqlDbType.Int).Value = probabilidad
                            cmd.Parameters.Add("@nivel_riesgo_pld", SqlDbType.Decimal).Value = nivel_riesgo_pld
                            cmd.Parameters("@nivel_riesgo_pld").Precision = 5 : cmd.Parameters("@nivel_riesgo_pld").Scale = 2

                            cmd.Parameters.Add("@observaciones_generales", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(observaciones_generales), CType(DBNull.Value, Object), observaciones_generales)
                            cmd.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id

                            cmd.ExecuteNonQuery()
                        End Using

                        ' Detalle (si lo mandas, se actualiza)
                        If detalle IsNot Nothing Then
                            ' obligatorios NOT NULL (misma regla que create)
                            Dim tasa_interes_anual As Decimal = GetReqDec(detalle, "tasa_interes_anual")
                            Dim tasa_interes_mensual As Decimal = GetReqDec(detalle, "tasa_interes_mensual")
                            Dim iva As Decimal = GetReqDec(detalle, "iva")
                            Dim factor_moratorio As Boolean = GetReqBool(detalle, "factor_moratorio")
                            Dim amortizar_comision_apertura As Boolean = GetReqBool(detalle, "amortizar_comision_apertura")
                            Dim amortizar_comision_gestion As Boolean = GetReqBool(detalle, "amortizar_comision_gestion")
                            Dim aplica_monto_seguro As Boolean = GetReqBool(detalle, "aplica_monto_seguro")
                            Dim usar_redondeo_centavos As Boolean = GetReqBool(detalle, "usar_redondeo_centavos")
                            Dim redondear_pago_fijo As Boolean = GetReqBool(detalle, "redondear_pago_fijo")
                            Dim iva_sobre_total As Boolean = GetReqBool(detalle, "iva_sobre_total")
                            Dim modificador_monto_credito As Boolean = GetReqBool(detalle, "modificador_monto_credito")



                            ' tipo_periodo (detalle) debe cumplir CK_detalles_tipo_periodo: COMPLETO/FIJO/NULL
                            Dim tipo_periodo_detalle_norm As String = Nothing
                            Dim tipo_periodo_detalle_raw As String = GetOptString(detalle, "tipo_periodo")
                            If Not String.IsNullOrWhiteSpace(tipo_periodo_detalle_raw) Then
                                Dim t As String = tipo_periodo_detalle_raw.Trim().ToUpperInvariant()
                                If t <> "COMPLETO" AndAlso t <> "FIJO" Then
                                    Throw New Exception("tipo_periodo (detalle) inválido. Valores permitidos: COMPLETO, FIJO.")
                                End If
                                tipo_periodo_detalle_norm = t
                            End If

                            Dim aplica_cargos_administrativos As Boolean = GetReqBool(detalle, "aplica_cargos_administrativos")
                            Dim amortizar_cargos_administrativos As Boolean = GetReqBool(detalle, "amortizar_cargos_administrativos")
                            Dim aplica_cargo_multa_cobranza As Boolean = GetReqBool(detalle, "aplica_cargo_multa_cobranza")
                            Dim aplica_comision_administracion As Boolean = GetReqBool(detalle, "aplica_comision_administracion")
                            Dim aplica_comision_investigacion As Boolean = GetReqBool(detalle, "aplica_comision_investigacion")

                            Dim exists As Integer = 0
                            Using cmdE As New SqlCommand("SELECT COUNT(1) FROM dbo.detalles_del_producto WHERE producto_id=@id", cn, tx)
                                cmdE.Parameters.Add("@id", SqlDbType.Int).Value = id
                                exists = Convert.ToInt32(cmdE.ExecuteScalar())
                            End Using

                            If exists = 0 Then
                                Throw New Exception("No existe detalle_del_producto para este producto. (En este flujo, el detalle debe existir).")
                            End If

                            Dim sqlDetU As String =
"UPDATE dbo.detalles_del_producto
 SET tasa_interes_anual=@tasa_interes_anual,
     tasa_interes_anual_detallada=@tasa_interes_anual_detallada,
     tasa_interes_mensual=@tasa_interes_mensual,
     iva=@iva,
     iva_comision=@iva_comision,
     capital_minimo=@capital_minimo,
     capital_maximo=@capital_maximo,
     plazo_minimo=@plazo_minimo,
     plazo_maximo=@plazo_maximo,
     vencimiento=@vencimiento,
     calculo=@calculo,
     factor_moratorio=@factor_moratorio,
     tasa_moratoria=@tasa_moratoria,
     iva_moratoria=@iva_moratoria,
     dias_gracia=@dias_gracia,
     comision_apertura=@comision_apertura,
     comision_apertura_detallada=@comision_apertura_detallada,
     amortizar_comision_apertura=@amortizar_comision_apertura,
     comision_gestion=@comision_gestion,
     amortizar_comision_gestion=@amortizar_comision_gestion,
     aplica_monto_seguro=@aplica_monto_seguro,
     comision_cancelacion=@comision_cancelacion,
     otros_gastos=@otros_gastos,
     usar_redondeo_centavos=@usar_redondeo_centavos,
     base_calculo=@base_calculo,
     tipo_periodo=@tipo_periodo,
     calculo_fecha_exigible=@calculo_fecha_exigible,
     centavos_para_redondeo=@centavos_para_redondeo,
     redondear_pago_fijo=@redondear_pago_fijo,
     aplicacion_de_tasa=@aplicacion_de_tasa,
     iva_sobre_total=@iva_sobre_total,
     modificador_monto_credito=@modificador_monto_credito,
     notas=@notas,
     modificado_por=@usr,
     fecha_modificacion=SYSUTCDATETIME(),
     aplica_cargos_administrativos=@aplica_cargos_administrativos,
     amortizar_cargos_administrativos=@amortizar_cargos_administrativos,
     aplica_cargo_multa_cobranza=@aplica_cargo_multa_cobranza,
     aplica_comision_administracion=@aplica_comision_administracion,
     iva_comision_administracion=@iva_comision_administracion,
     comision_administracion_valor=@comision_administracion_valor,
     comision_administracion_detallada=@comision_administracion_detallada,
     aplica_comision_investigacion=@aplica_comision_investigacion,
     iva_comision_investigacion=@iva_comision_investigacion,
     comision_investigacion_detallada=@comision_investigacion_detallada
 WHERE producto_id=@producto_id;"

                            Using cmdD As New SqlCommand(sqlDetU, cn, tx)
                                cmdD.Parameters.Add("@producto_id", SqlDbType.Int).Value = id

                                cmdD.Parameters.Add("@tasa_interes_anual", SqlDbType.Decimal).Value = tasa_interes_anual
                                cmdD.Parameters("@tasa_interes_anual").Precision = 8 : cmdD.Parameters("@tasa_interes_anual").Scale = 4
                                cmdD.Parameters.Add("@tasa_interes_anual_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "tasa_interes_anual_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "tasa_interes_anual_detallada"))

                                cmdD.Parameters.Add("@tasa_interes_mensual", SqlDbType.Decimal).Value = tasa_interes_mensual
                                cmdD.Parameters("@tasa_interes_mensual").Precision = 8 : cmdD.Parameters("@tasa_interes_mensual").Scale = 4

                                cmdD.Parameters.Add("@iva", SqlDbType.Decimal).Value = iva
                                cmdD.Parameters("@iva").Precision = 5 : cmdD.Parameters("@iva").Scale = 2

                                Dim iva_comision As Decimal? = GetOptDec(detalle, "iva_comision")
                                cmdD.Parameters.Add("@iva_comision", SqlDbType.Decimal).Value = If(iva_comision.HasValue, CType(iva_comision.Value, Object), DBNull.Value)
                                cmdD.Parameters("@iva_comision").Precision = 5 : cmdD.Parameters("@iva_comision").Scale = 2

                                Dim capital_minimo As Decimal? = GetOptDec(detalle, "capital_minimo")
                                Dim capital_maximo As Decimal? = GetOptDec(detalle, "capital_maximo")
                                cmdD.Parameters.Add("@capital_minimo", SqlDbType.Decimal).Value = If(capital_minimo.HasValue, CType(capital_minimo.Value, Object), DBNull.Value)
                                cmdD.Parameters("@capital_minimo").Precision = 18 : cmdD.Parameters("@capital_minimo").Scale = 2
                                cmdD.Parameters.Add("@capital_maximo", SqlDbType.Decimal).Value = If(capital_maximo.HasValue, CType(capital_maximo.Value, Object), DBNull.Value)
                                cmdD.Parameters("@capital_maximo").Precision = 18 : cmdD.Parameters("@capital_maximo").Scale = 2

                                cmdD.Parameters.Add("@plazo_minimo", SqlDbType.Int).Value = If(GetOptInt(detalle, "plazo_minimo").HasValue, CType(GetOptInt(detalle, "plazo_minimo").Value, Object), DBNull.Value)
                                cmdD.Parameters.Add("@plazo_maximo", SqlDbType.Int).Value = If(GetOptInt(detalle, "plazo_maximo").HasValue, CType(GetOptInt(detalle, "plazo_maximo").Value, Object), DBNull.Value)

                                cmdD.Parameters.Add("@vencimiento", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "vencimiento")), CType(DBNull.Value, Object), GetOptString(detalle, "vencimiento"))
                                cmdD.Parameters.Add("@calculo", SqlDbType.NVarChar, 100).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "calculo")), CType(DBNull.Value, Object), GetOptString(detalle, "calculo"))

                                cmdD.Parameters.Add("@factor_moratorio", SqlDbType.Bit).Value = factor_moratorio

                                Dim tasa_moratoria As Decimal? = GetOptDec(detalle, "tasa_moratoria")
                                cmdD.Parameters.Add("@tasa_moratoria", SqlDbType.Decimal).Value = If(tasa_moratoria.HasValue, CType(tasa_moratoria.Value, Object), DBNull.Value)
                                cmdD.Parameters("@tasa_moratoria").Precision = 8 : cmdD.Parameters("@tasa_moratoria").Scale = 4

                                Dim iva_moratoria As Decimal? = GetOptDec(detalle, "iva_moratoria")
                                cmdD.Parameters.Add("@iva_moratoria", SqlDbType.Decimal).Value = If(iva_moratoria.HasValue, CType(iva_moratoria.Value, Object), DBNull.Value)
                                cmdD.Parameters("@iva_moratoria").Precision = 5 : cmdD.Parameters("@iva_moratoria").Scale = 2

                                cmdD.Parameters.Add("@dias_gracia", SqlDbType.Int).Value = If(GetOptInt(detalle, "dias_gracia").HasValue, CType(GetOptInt(detalle, "dias_gracia").Value, Object), DBNull.Value)

                                Dim comision_apertura As Decimal? = GetOptDec(detalle, "comision_apertura")
                                cmdD.Parameters.Add("@comision_apertura", SqlDbType.Decimal).Value = If(comision_apertura.HasValue, CType(comision_apertura.Value, Object), DBNull.Value)
                                cmdD.Parameters("@comision_apertura").Precision = 18 : cmdD.Parameters("@comision_apertura").Scale = 6

                                cmdD.Parameters.Add("@comision_apertura_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_apertura_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_apertura_detallada"))
                                cmdD.Parameters.Add("@amortizar_comision_apertura", SqlDbType.Bit).Value = amortizar_comision_apertura

                                Dim comision_gestion As Decimal? = GetOptDec(detalle, "comision_gestion")
                                cmdD.Parameters.Add("@comision_gestion", SqlDbType.Decimal).Value = If(comision_gestion.HasValue, CType(comision_gestion.Value, Object), DBNull.Value)
                                cmdD.Parameters("@comision_gestion").Precision = 18 : cmdD.Parameters("@comision_gestion").Scale = 6
                                cmdD.Parameters.Add("@amortizar_comision_gestion", SqlDbType.Bit).Value = amortizar_comision_gestion

                                cmdD.Parameters.Add("@aplica_monto_seguro", SqlDbType.Bit).Value = aplica_monto_seguro
                                cmdD.Parameters.Add("@comision_cancelacion", SqlDbType.NVarChar, 200).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_cancelacion")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_cancelacion"))

                                Dim otros_gastos As Decimal? = GetOptDec(detalle, "otros_gastos")
                                cmdD.Parameters.Add("@otros_gastos", SqlDbType.Decimal).Value = If(otros_gastos.HasValue, CType(otros_gastos.Value, Object), DBNull.Value)
                                cmdD.Parameters("@otros_gastos").Precision = 18 : cmdD.Parameters("@otros_gastos").Scale = 6

                                cmdD.Parameters.Add("@usar_redondeo_centavos", SqlDbType.Bit).Value = usar_redondeo_centavos
                                cmdD.Parameters.Add("@base_calculo", SqlDbType.Int).Value = If(GetOptInt(detalle, "base_calculo").HasValue, CType(GetOptInt(detalle, "base_calculo").Value, Object), DBNull.Value)
                                cmdD.Parameters.Add("@tipo_periodo", SqlDbType.NVarChar, 20).Value = If(tipo_periodo_detalle_norm Is Nothing, CType(DBNull.Value, Object), tipo_periodo_detalle_norm)
                                cmdD.Parameters.Add("@calculo_fecha_exigible", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "calculo_fecha_exigible")), CType(DBNull.Value, Object), GetOptString(detalle, "calculo_fecha_exigible"))
                                cmdD.Parameters.Add("@centavos_para_redondeo", SqlDbType.Int).Value = If(GetOptInt(detalle, "centavos_para_redondeo").HasValue, CType(GetOptInt(detalle, "centavos_para_redondeo").Value, Object), DBNull.Value)

                                cmdD.Parameters.Add("@redondear_pago_fijo", SqlDbType.Bit).Value = redondear_pago_fijo
                                cmdD.Parameters.Add("@aplicacion_de_tasa", SqlDbType.NVarChar, 50).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "aplicacion_de_tasa")), CType(DBNull.Value, Object), GetOptString(detalle, "aplicacion_de_tasa"))
                                cmdD.Parameters.Add("@iva_sobre_total", SqlDbType.Bit).Value = iva_sobre_total
                                cmdD.Parameters.Add("@modificador_monto_credito", SqlDbType.Bit).Value = modificador_monto_credito
                                cmdD.Parameters.Add("@notas", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "notas")), CType(DBNull.Value, Object), GetOptString(detalle, "notas"))

                                cmdD.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario

                                cmdD.Parameters.Add("@aplica_cargos_administrativos", SqlDbType.Bit).Value = aplica_cargos_administrativos
                                cmdD.Parameters.Add("@amortizar_cargos_administrativos", SqlDbType.Bit).Value = amortizar_cargos_administrativos
                                cmdD.Parameters.Add("@aplica_cargo_multa_cobranza", SqlDbType.Bit).Value = aplica_cargo_multa_cobranza

                                cmdD.Parameters.Add("@aplica_comision_administracion", SqlDbType.Bit).Value = aplica_comision_administracion

                                Dim iva_com_admin As Decimal? = GetOptDec(detalle, "iva_comision_administracion")
                                cmdD.Parameters.Add("@iva_comision_administracion", SqlDbType.Decimal).Value = If(iva_com_admin.HasValue, CType(iva_com_admin.Value, Object), DBNull.Value)
                                cmdD.Parameters("@iva_comision_administracion").Precision = 5 : cmdD.Parameters("@iva_comision_administracion").Scale = 2

                                Dim com_admin_val As Decimal? = GetOptDec(detalle, "comision_administracion_valor")
                                cmdD.Parameters.Add("@comision_administracion_valor", SqlDbType.Decimal).Value = If(com_admin_val.HasValue, CType(com_admin_val.Value, Object), DBNull.Value)
                                cmdD.Parameters("@comision_administracion_valor").Precision = 18 : cmdD.Parameters("@comision_administracion_valor").Scale = 6

                                cmdD.Parameters.Add("@comision_administracion_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_administracion_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_administracion_detallada"))

                                cmdD.Parameters.Add("@aplica_comision_investigacion", SqlDbType.Bit).Value = aplica_comision_investigacion

                                Dim iva_com_inv As Decimal? = GetOptDec(detalle, "iva_comision_investigacion")
                                cmdD.Parameters.Add("@iva_comision_investigacion", SqlDbType.Decimal).Value = If(iva_com_inv.HasValue, CType(iva_com_inv.Value, Object), DBNull.Value)
                                cmdD.Parameters("@iva_comision_investigacion").Precision = 5 : cmdD.Parameters("@iva_comision_investigacion").Scale = 2

                                cmdD.Parameters.Add("@comision_investigacion_detallada", SqlDbType.NVarChar).Value = If(String.IsNullOrWhiteSpace(GetOptString(detalle, "comision_investigacion_detallada")), CType(DBNull.Value, Object), GetOptString(detalle, "comision_investigacion_detallada"))

                                cmdD.ExecuteNonQuery()
                            End Using
                        End If


                        ' ---------- PERIODOS (si vienen en el JSON, sincronizamos sin borrar físicamente) ----------
                        If hasPeriodos Then
                            Dim periodosArr As ArrayList = TryCast(periodosOpt, ArrayList)
                            If periodosArr Is Nothing OrElse periodosArr.Count = 0 Then
                                Throw New Exception("Debe capturar al menos una periodicidad (periodos).")
                            End If

                            Dim existingPeriodos As New List(Of KeyValuePair(Of Integer, String))()
                            Using cmdExisting As New SqlCommand("SELECT id, tipo_periodo FROM dbo.catalogo_producto_financiero_periodos WHERE producto_financiero_id=@id ORDER BY activo DESC, id DESC;", cn, tx)
                                cmdExisting.Parameters.Add("@id", SqlDbType.Int).Value = id
                                Using rdr As SqlDataReader = cmdExisting.ExecuteReader()
                                    While rdr.Read()
                                        existingPeriodos.Add(New KeyValuePair(Of Integer, String)(Convert.ToInt32(rdr("id")), Convert.ToString(rdr("tipo_periodo"))))
                                    End While
                                End Using
                            End Using

                            Dim matchedExistingIds As New HashSet(Of Integer)()
                            Dim payloadTipos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                            Dim sqlPer As String =
"INSERT INTO dbo.catalogo_producto_financiero_periodos
 (producto_financiero_id, tipo_periodo, plazo, tasa_interes, activo, creado_por, fecha_creacion, modificado_por, fecha_modificacion)
VALUES
 (@producto, @tipo, @plazo, @tasa, 1, @usr, SYSUTCDATETIME(), @usr, SYSUTCDATETIME());"

                            Dim sqlPerUpd As String =
"UPDATE dbo.catalogo_producto_financiero_periodos
 SET tipo_periodo=@tipo,
     plazo=@plazo,
     tasa_interes=@tasa,
     activo=1,
     modificado_por=@usr,
     fecha_modificacion=SYSUTCDATETIME()
 WHERE id=@periodo_id AND producto_financiero_id=@producto;"

                            For Each obj As Object In periodosArr
                                Dim p As IDictionary = TryCast(obj, IDictionary)
                                If p Is Nothing Then Throw New Exception("Elemento de periodos inválido.")

                                Dim tp As String = GetReqString(p, "tipo_periodo")
                                If Not IsTipoPeriodoValido(tp) Then Throw New Exception("tipo_periodo inválido en periodos: " & tp)
                                If payloadTipos.Contains(tp) Then Throw New Exception("No puede haber periodicidades repetidas para el mismo tipo: " & tp)
                                payloadTipos.Add(tp)

                                Dim pl As Integer = GetReqInt(p, "plazo")
                                If pl <= 0 Then Throw New Exception("plazo inválido en periodos.")

                                Dim tasa As Decimal = GetReqDec(p, "tasa_interes")
                                If tasa < 0D Then Throw New Exception("tasa_interes inválida en periodos.")

                                Dim incomingId As Integer? = Nothing
                                Dim rawId As String = Nothing
                                If p.Contains("id") AndAlso p("id") IsNot Nothing Then
                                    rawId = p("id").ToString().Trim()
                                    Dim parsedId As Integer
                                    If Integer.TryParse(rawId, parsedId) AndAlso parsedId > 0 Then incomingId = parsedId
                                End If

                                Dim targetId As Integer? = Nothing
                                If incomingId.HasValue Then
                                    For Each kv As KeyValuePair(Of Integer, String) In existingPeriodos
                                        If kv.Key = incomingId.Value Then
                                            targetId = incomingId.Value
                                            Exit For
                                        End If
                                    Next
                                End If

                                If (Not targetId.HasValue) Then
                                    For Each kv As KeyValuePair(Of Integer, String) In existingPeriodos
                                        If matchedExistingIds.Contains(kv.Key) Then Continue For
                                        If String.Equals(kv.Value, tp, StringComparison.OrdinalIgnoreCase) Then
                                            targetId = kv.Key
                                            Exit For
                                        End If
                                    Next
                                End If

                                If targetId.HasValue Then
                                    Using cmdP As New SqlCommand(sqlPerUpd, cn, tx)
                                        cmdP.Parameters.Add("@producto", SqlDbType.Int).Value = id
                                        cmdP.Parameters.Add("@periodo_id", SqlDbType.Int).Value = targetId.Value
                                        cmdP.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = tp
                                        cmdP.Parameters.Add("@plazo", SqlDbType.Int).Value = pl
                                        cmdP.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                                        cmdP.Parameters("@tasa").Precision = 18 : cmdP.Parameters("@tasa").Scale = 6
                                        cmdP.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario

                                        If cmdP.ExecuteNonQuery() <= 0 Then
                                            Throw New Exception("No se pudo actualizar una periodicidad del producto.")
                                        End If
                                    End Using
                                    matchedExistingIds.Add(targetId.Value)
                                Else
                                    Using cmdP As New SqlCommand(sqlPer, cn, tx)
                                        cmdP.Parameters.Add("@producto", SqlDbType.Int).Value = id
                                        cmdP.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = tp
                                        cmdP.Parameters.Add("@plazo", SqlDbType.Int).Value = pl
                                        cmdP.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                                        cmdP.Parameters("@tasa").Precision = 18 : cmdP.Parameters("@tasa").Scale = 6
                                        cmdP.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                                        cmdP.ExecuteNonQuery()
                                    End Using
                                End If
                            Next

                            For Each kv As KeyValuePair(Of Integer, String) In existingPeriodos
                                If matchedExistingIds.Contains(kv.Key) Then Continue For

                                Using cmdDeactivate As New SqlCommand("UPDATE dbo.catalogo_producto_financiero_periodos SET activo=0, modificado_por=@usr, fecha_modificacion=SYSUTCDATETIME() WHERE id=@periodo_id AND producto_financiero_id=@producto;", cn, tx)
                                    cmdDeactivate.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                                    cmdDeactivate.Parameters.Add("@periodo_id", SqlDbType.Int).Value = kv.Key
                                    cmdDeactivate.Parameters.Add("@producto", SqlDbType.Int).Value = id
                                    cmdDeactivate.ExecuteNonQuery()
                                End Using
                            Next
                        End If

                        tx.Commit()
                        WriteJson(ctx, New With {.success = True, .message = "Producto actualizado.", .id = id})
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

        Catch ex As Exception
            WriteJson(ctx, New With {.success = False, .message = ex.Message})
        End Try
    End Sub

    ' ==========================
    ' DELETE (baja lógica + cascada lógica)
    ' ==========================
    Private Sub HandleDelete(ctx As HttpContext)
        Dim usuario As String = GetUsuarioRequired(ctx)

        Dim id As Integer
        If Not Integer.TryParse((ctx.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(ctx, New With {.success = False, .message = "Parámetro id inválido."})
            Return
        End If

        Using cn As New SqlConnection(GetConnString())
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    Dim affectedProducto As Integer = 0

                    Using cmd As New SqlCommand("UPDATE dbo.catalogo_producto_financiero SET activo=0, modificado_por=@usr, fecha_modificacion=SYSUTCDATETIME() WHERE id=@id AND ISNULL(activo, 0) = 1;", cn, tx)
                        cmd.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                        affectedProducto = cmd.ExecuteNonQuery()
                    End Using

                    If affectedProducto = 0 Then
                        Throw New Exception("No se encontró un producto activo con el id indicado.")
                    End If

                    Using cmd2 As New SqlCommand("UPDATE dbo.detalles_del_producto SET activo=0, modificado_por=@usr, fecha_modificacion=SYSUTCDATETIME() WHERE producto_id=@id;", cn, tx)
                        cmd2.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                        cmd2.Parameters.Add("@id", SqlDbType.Int).Value = id
                        cmd2.ExecuteNonQuery()
                    End Using

                    Using cmd3 As New SqlCommand("UPDATE dbo.catalogo_producto_financiero_periodos SET activo=0, modificado_por=@usr, fecha_modificacion=SYSUTCDATETIME() WHERE producto_financiero_id=@id;", cn, tx)
                        cmd3.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = usuario
                        cmd3.Parameters.Add("@id", SqlDbType.Int).Value = id
                        cmd3.ExecuteNonQuery()
                    End Using

                    Using cmd4 As New SqlCommand("UPDATE dbo.producto_planeacion SET activo=0 WHERE producto_id=@id;", cn, tx)
                        cmd4.Parameters.Add("@id", SqlDbType.Int).Value = id
                        cmd4.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.success = True, .message = "Producto dado de baja (lógica).", .id = id})
                Catch ex As Exception
                    tx.Rollback()
                    WriteJson(ctx, New With {.success = False, .message = "Error al eliminar: " & ex.Message})
                End Try
            End Using
        End Using
    End Sub


    ' ==========================
    ' PERIODOS (CONSOLIDADO)
    ' Nota: Se movió aquí para evitar duplicidad de lógica.
    ' ==========================

    Private Sub PeriodosListar(ByVal context As HttpContext)
        Dim productoId As Integer
        If Not Integer.TryParse((context.Request("producto_financiero_id") & "").Trim(), productoId) OrElse productoId <= 0 Then
            WriteJson(context, New With {.success = False, .message = "producto_financiero_id inválido."})
            Return
        End If

        Dim data As New List(Of Object)()

        Using cn As New SqlConnection(GetConnString())
            cn.Open()
            Dim sql As String =
"SELECT id, producto_financiero_id, tipo_periodo, plazo, tasa_interes, activo
 FROM dbo.catalogo_producto_financiero_periodos
 WHERE producto_financiero_id = @producto
 ORDER BY CASE tipo_periodo WHEN 'Mensual' THEN 1 WHEN 'Quincenal' THEN 2 WHEN 'Semanal' THEN 3 ELSE 99 END, id;"

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@producto", SqlDbType.Int).Value = productoId
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    While dr.Read()
                        data.Add(New With {
                            .id = Convert.ToInt32(dr("id")),
                            .producto_financiero_id = Convert.ToInt32(dr("producto_financiero_id")),
                            .tipo_periodo = Convert.ToString(dr("tipo_periodo")),
                            .plazo = Convert.ToInt32(dr("plazo")),
                            .tasa_interes = Convert.ToDecimal(dr("tasa_interes")),
                            .activo = Convert.ToBoolean(dr("activo"))
                        })
                    End While
                End Using
            End Using
        End Using

        WriteJson(context, New With {.success = True, .data = data})
    End Sub

    Private Sub PeriodosGuardar(ByVal context As HttpContext)
        Dim usuario As String = GetUsuarioRequired(context)

        Dim productoId As Integer
        Dim tipoPeriodo As String = (context.Request("tipo_periodo") & "").Trim()
        Dim plazo As Integer
        Dim tasa As Decimal

        If Not Integer.TryParse((context.Request("producto_financiero_id") & "").Trim(), productoId) OrElse productoId <= 0 Then
            WriteJson(context, New With {.success = False, .message = "producto_financiero_id inválido."})
            Return
        End If

        If Not IsTipoPeriodoValido(tipoPeriodo) Then
            WriteJson(context, New With {.success = False, .message = "tipo_periodo inválido (Semanal/Quincenal/Mensual)."})
            Return
        End If

        If Not Integer.TryParse((context.Request("plazo") & "").Trim(), plazo) OrElse plazo <= 0 Then
            WriteJson(context, New With {.success = False, .message = "plazo inválido (entero > 0)."})
            Return
        End If

        If Not Decimal.TryParse(NormalizaDecimal(context.Request("tasa_interes")), tasa) OrElse tasa < 0D Then
            WriteJson(context, New With {.success = False, .message = "tasa_interes inválida (decimal >= 0)."})
            Return
        End If

        Try
            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String =
"INSERT INTO dbo.catalogo_producto_financiero_periodos
 (producto_financiero_id, tipo_periodo, plazo, tasa_interes, activo, creado_por, fecha_creacion, modificado_por, fecha_modificacion)
 VALUES
 (@producto, @tipo, @plazo, @tasa, 1, @usr, SYSUTCDATETIME(), @usr, SYSUTCDATETIME());
 SELECT SCOPE_IDENTITY();"

                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.Add("@producto", SqlDbType.Int).Value = productoId
                    cmd.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = tipoPeriodo
                    cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = plazo
                    cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                    cmd.Parameters("@tasa").Precision = 18
                    cmd.Parameters("@tasa").Scale = 6
                    cmd.Parameters.Add("@usr", SqlDbType.VarChar, 100).Value = usuario

                    Dim newIdObj As Object = cmd.ExecuteScalar()
                    Dim newId As Integer = Convert.ToInt32(Convert.ToDecimal(newIdObj))

                    WriteJson(context, New With {.success = True, .message = "Periodicidad guardada.", .id = newId})
                End Using
            End Using

        Catch ex As SqlException
            If ex.Number = 2601 OrElse ex.Number = 2627 Then
                WriteJson(context, New With {.success = False, .message = "Ya existe esa periodicidad para este producto."})
            Else
                WriteJson(context, New With {.success = False, .message = "Error SQL: " & ex.Message})
            End If
        End Try
    End Sub

    Private Sub PeriodosActualizar(ByVal context As HttpContext)
        Dim usuario As String = GetUsuarioRequired(context)

        Dim id As Integer
        Dim productoId As Integer
        Dim tipoPeriodo As String = (context.Request("tipo_periodo") & "").Trim()
        Dim plazo As Integer
        Dim tasa As Decimal

        If Not Integer.TryParse((context.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "id inválido."})
            Return
        End If

        If Not Integer.TryParse((context.Request("producto_financiero_id") & "").Trim(), productoId) OrElse productoId <= 0 Then
            WriteJson(context, New With {.success = False, .message = "producto_financiero_id inválido."})
            Return
        End If

        If Not IsTipoPeriodoValido(tipoPeriodo) Then
            WriteJson(context, New With {.success = False, .message = "tipo_periodo inválido (Semanal/Quincenal/Mensual)."})
            Return
        End If

        If Not Integer.TryParse((context.Request("plazo") & "").Trim(), plazo) OrElse plazo <= 0 Then
            WriteJson(context, New With {.success = False, .message = "plazo inválido (entero > 0)."})
            Return
        End If

        If Not Decimal.TryParse(NormalizaDecimal(context.Request("tasa_interes")), tasa) OrElse tasa < 0D Then
            WriteJson(context, New With {.success = False, .message = "tasa_interes inválida (decimal >= 0)."})
            Return
        End If

        Try
            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String =
"UPDATE dbo.catalogo_producto_financiero_periodos
 SET tipo_periodo = @tipo,
     plazo = @plazo,
     tasa_interes = @tasa,
     modificado_por = @usr,
     fecha_modificacion = SYSUTCDATETIME()
 WHERE id = @id AND producto_financiero_id = @producto;"

                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = tipoPeriodo
                    cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = plazo
                    cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                    cmd.Parameters("@tasa").Precision = 18
                    cmd.Parameters("@tasa").Scale = 6
                    cmd.Parameters.Add("@usr", SqlDbType.VarChar, 100).Value = usuario
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                    cmd.Parameters.Add("@producto", SqlDbType.Int).Value = productoId

                    Dim aff As Integer = cmd.ExecuteNonQuery()
                    If aff > 0 Then
                        WriteJson(context, New With {.success = True, .message = "Periodicidad actualizada."})
                    Else
                        WriteJson(context, New With {.success = False, .message = "No se encontró la periodicidad a actualizar."})
                    End If
                End Using
            End Using

        Catch ex As SqlException
            If ex.Number = 2601 OrElse ex.Number = 2627 Then
                WriteJson(context, New With {.success = False, .message = "Ya existe esa periodicidad para este producto."})
            Else
                WriteJson(context, New With {.success = False, .message = "Error SQL: " & ex.Message})
            End If
        End Try
    End Sub

    Private Sub PeriodosToggle(ByVal context As HttpContext)
        Dim usuario As String = GetUsuarioRequired(context)

        Dim id As Integer
        If Not Integer.TryParse((context.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "id inválido."})
            Return
        End If

        Using cn As New SqlConnection(GetConnString())
            cn.Open()
            Dim sql As String =
"UPDATE dbo.catalogo_producto_financiero_periodos
 SET activo = CASE WHEN activo = 1 THEN 0 ELSE 1 END,
     modificado_por = @usr,
     fecha_modificacion = SYSUTCDATETIME()
 WHERE id = @id;"

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@usr", SqlDbType.VarChar, 100).Value = usuario
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim aff As Integer = cmd.ExecuteNonQuery()
                If aff > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Estatus actualizado."})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró la periodicidad."})
                End If
            End Using
        End Using
    End Sub

    Private Sub PeriodosEliminar(ByVal context As HttpContext)
        Dim usuario As String = GetUsuarioRequired(context) ' requerido por regla, aunque aquí no se use

        Dim id As Integer
        If Not Integer.TryParse((context.Request("id") & "").Trim(), id) OrElse id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "id inválido."})
            Return
        End If

        Using cn As New SqlConnection(GetConnString())
            cn.Open()
            Dim sql As String = "DELETE FROM dbo.catalogo_producto_financiero_periodos WHERE id = @id;"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim aff As Integer = cmd.ExecuteNonQuery()
                If aff > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Periodicidad eliminada."})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró la periodicidad."})
                End If
            End Using
        End Using
    End Sub

    Private Sub PeriodosCombo(ByVal context As HttpContext)
        Dim productoId As Integer
        If Not Integer.TryParse((context.Request("producto_financiero_id") & "").Trim(), productoId) OrElse productoId <= 0 Then
            WriteJson(context, New With {.success = False, .message = "producto_financiero_id inválido."})
            Return
        End If

        Dim data As New List(Of Object)()

        Using cn As New SqlConnection(GetConnString())
            cn.Open()
            Dim sql As String =
"SELECT id, tipo_periodo, plazo, tasa_interes
 FROM dbo.catalogo_producto_financiero_periodos
 WHERE producto_financiero_id = @producto AND activo = 1
 ORDER BY CASE tipo_periodo WHEN 'Mensual' THEN 1 WHEN 'Quincenal' THEN 2 WHEN 'Semanal' THEN 3 ELSE 99 END, id;"

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@producto", SqlDbType.Int).Value = productoId
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    While dr.Read()
                        data.Add(New With {
                            .id = Convert.ToInt32(dr("id")),
                            .tipo_periodo = Convert.ToString(dr("tipo_periodo")),
                            .plazo = Convert.ToInt32(dr("plazo")),
                            .tasa_interes = Convert.ToDecimal(dr("tasa_interes"))
                        })
                    End While
                End Using
            End Using
        End Using

        WriteJson(context, New With {.success = True, .data = data})
    End Sub

    Private Function NormalizaDecimal(ByVal v As Object) As String
        If v Is Nothing Then Return ""
        Return (v.ToString().Trim().Replace(",", "."))
    End Function

End Class
