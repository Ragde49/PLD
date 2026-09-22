Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Public Class handler_credito_revolvente
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer() With {.MaxJsonLength = Integer.MaxValue}

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = Param(context, "action").Trim().ToLowerInvariant()

            Select Case action
                Case "resumen"
                    respuesta = Resumen(context)

                Case "configurar_linea"
                    respuesta = ConfigurarLinea(context)

                Case "listar_disposiciones"
                    respuesta = ListarDisposiciones(context)

                Case "crear_disposicion"
                    respuesta = CrearDisposicion(context)

                Case "reversar_disposicion"
                    respuesta = ReversarDisposicion(context)

                Case "historial"
                    respuesta = Historial(context)

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_credito_revolvente."
            respuesta("detalle") = ex.Message
        End Try

        context.Response.Write(serializer.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Function CnStr() As String
        Dim cs = ConfigurationManager.ConnectionStrings("PLDConnection")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New Exception("Falta PLDConnection en Web.config.")
        End If
        Return cs.ConnectionString
    End Function

    Private Function Param(ByVal context As HttpContext, ByVal nombre As String) As String
        If context.Request(nombre) Is Nothing Then Return ""
        Return Convert.ToString(context.Request(nombre), CultureInfo.InvariantCulture)
    End Function

    Private Function Usuario(ByVal context As HttpContext) As String
        Dim u As String = ""
        If context.User IsNot Nothing AndAlso context.User.Identity IsNot Nothing AndAlso context.User.Identity.IsAuthenticated Then
            u = context.User.Identity.Name
        End If
        If String.IsNullOrWhiteSpace(u) AndAlso context.Session IsNot Nothing AndAlso context.Session("usuario") IsNot Nothing Then
            u = Convert.ToString(context.Session("usuario"), CultureInfo.InvariantCulture)
        End If
        If String.IsNullOrWhiteSpace(u) Then u = "SISTEMA"
        Return u.Trim()
    End Function

    Private Function ToInt(ByVal valor As Object, Optional ByVal def As Integer = 0) As Integer
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return def
        Dim n As Integer
        If Integer.TryParse(Convert.ToString(valor, CultureInfo.InvariantCulture), n) Then Return n
        Return def
    End Function

    Private Function ToDec(ByVal valor As Object, Optional ByVal def As Decimal = 0D) As Decimal
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return def
        Dim s As String = Convert.ToString(valor, CultureInfo.InvariantCulture).Trim().Replace(",", ".")
        Dim n As Decimal
        If Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, n) Then Return n
        Return def
    End Function

    Private Function ToDateNullable(ByVal valor As String) As DateTime?
        If String.IsNullOrWhiteSpace(valor) Then Return Nothing
        Dim d As DateTime
        If DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, d) Then Return d
        If DateTime.TryParse(valor, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, d) Then Return d
        Return Nothing
    End Function

    Private Function DbValue(ByVal valor As String) As Object
        If String.IsNullOrWhiteSpace(valor) Then Return DBNull.Value
        Return valor.Trim()
    End Function

    Private Function TableToList(ByVal dt As DataTable) As List(Of Dictionary(Of String, Object))
        Dim lista As New List(Of Dictionary(Of String, Object))()
        For Each row As DataRow In dt.Rows
            Dim item As New Dictionary(Of String, Object)()
            For Each col As DataColumn In dt.Columns
                item(col.ColumnName) = If(row.IsNull(col), Nothing, row(col))
            Next
            lista.Add(item)
        Next
        Return lista
    End Function

    Private Function QueryTable(ByVal cn As SqlConnection,
                                ByVal tr As SqlTransaction,
                                ByVal sql As String,
                                ByVal parametros As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()
        Using cmd As New SqlCommand(sql, cn)
            If tr IsNot Nothing Then cmd.Transaction = tr
            If parametros IsNot Nothing Then
                For Each p As SqlParameter In parametros
                    cmd.Parameters.Add(p)
                Next
            End If
            Using rd As SqlDataReader = cmd.ExecuteReader()
                dt.Load(rd)
            End Using
        End Using
        Return dt
    End Function

    Private Function ObtenerCabecera(ByVal cn As SqlConnection,
                                     ByVal tr As SqlTransaction,
                                     ByVal solicitudId As Integer,
                                     ByVal bloquear As Boolean) As DataRow
        Dim hint As String = If(bloquear, " WITH (UPDLOCK, HOLDLOCK)", "")
        Dim sql As String =
            "SELECT TOP 1 " &
            " sc.id, sc.cliente_id, sc.producto_financiero_id, sc.moneda_id, " &
            " sc.monto_solicitado, sc.monto_autorizado, sc.fecha_vigencia_inicio, sc.fecha_vigencia_fin, " &
            " sc.estatus, sc.activo, " &
            " pf.descripcion_larga AS producto, pf.tipo_credito_id, " &
            " cc.nombre_credito AS tipo_credito, ISNULL(cc.es_revolvente,0) AS es_revolvente, " &
            " LTRIM(RTRIM(REPLACE(REPLACE(CONCAT(ISNULL(c.primer_nombre,''),' ',ISNULL(c.segundo_nombre,''),' ',ISNULL(c.apellido_paterno,''),' ',ISNULL(c.apellido_materno,'')),'  ',' '),'  ',' '))) AS cliente_nombre, " &
            " c.rfc, c.curp, md.moneda, md.clave AS moneda_clave " &
            "FROM dbo.solicitud_credito sc" & hint & " " &
            "INNER JOIN dbo.catalogo_producto_financiero pf ON pf.id = sc.producto_financiero_id " &
            "INNER JOIN dbo.catalogo_creditos cc ON cc.id = pf.tipo_credito_id " &
            "LEFT JOIN dbo.cliente_persona_fisica c ON c.id_cliente = sc.cliente_id " &
            "LEFT JOIN dbo.catalogo_moneda_divisa md ON md.id = sc.moneda_id " &
            "WHERE sc.id = @id;"

        Dim dt As DataTable = QueryTable(
            cn, tr, sql,
            New List(Of SqlParameter) From {
                New SqlParameter("@id", SqlDbType.Int) With {.Value = solicitudId}
            }
        )

        If dt.Rows.Count = 0 Then Return Nothing
        Return dt.Rows(0)
    End Function

    Private Function ObtenerSaldos(ByVal cn As SqlConnection,
                                   ByVal tr As SqlTransaction,
                                   ByVal solicitudId As Integer,
                                   ByVal bloquearMovimientos As Boolean) As Dictionary(Of String, Decimal)
        Dim dHint As String = If(bloquearMovimientos, " WITH (UPDLOCK, HOLDLOCK)", "")
        Dim pHint As String = If(bloquearMovimientos, " WITH (UPDLOCK, HOLDLOCK)", "")

        Dim capitalDispuesto As Decimal = 0D
        Dim capitalAmortizado As Decimal = 0D

        Using cmd As New SqlCommand(
            "SELECT ISNULL(SUM(monto),0) " &
            "FROM dbo.credito_disposiciones" & dHint & " " &
            "WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADA';", cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
            capitalDispuesto = Convert.ToDecimal(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using

        Using cmd As New SqlCommand(
            "SELECT ISNULL(SUM(ISNULL(monto_capital,0)),0) " &
            "FROM dbo.pagos_credito" & pHint & " " &
            "WHERE solicitud_credito_id=@id AND activo=1 AND estatus=N'APLICADO';", cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
            capitalAmortizado = Convert.ToDecimal(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using

        Dim r As New Dictionary(Of String, Decimal)()
        r("capital_dispuesto") = capitalDispuesto
        r("capital_amortizado") = capitalAmortizado
        r("saldo_utilizado") = capitalDispuesto - capitalAmortizado
        Return r
    End Function

    Private Function ValidarRevolvente(ByVal cab As DataRow) As String
        If cab Is Nothing Then Return "No se encontró la solicitud."
        If ToInt(cab("activo"), 0) <> 1 Then Return "La solicitud está inactiva."
        If ToInt(cab("es_revolvente"), 0) <> 1 Then Return "El producto seleccionado no está configurado como crédito revolvente."
        Return ""
    End Function

    Private Function Resumen(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(Param(context, "solicitud_id"), 0)

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "solicitud_id inválido."
            Return respuesta
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim cab As DataRow = ObtenerCabecera(cn, Nothing, solicitudId, False)
            Dim err As String = ValidarRevolvente(cab)
            If err <> "" Then
                respuesta("ok") = False
                respuesta("mensaje") = err
                Return respuesta
            End If

            Dim saldos = ObtenerSaldos(cn, Nothing, solicitudId, False)
            Dim limite As Decimal? = Nothing
            If Not cab.IsNull("monto_autorizado") Then limite = Convert.ToDecimal(cab("monto_autorizado"))

            Dim disponible As Decimal? = Nothing
            If limite.HasValue Then disponible = limite.Value - saldos("saldo_utilizado")

            respuesta("ok") = True
            respuesta("data") = New Dictionary(Of String, Object) From {
                {"solicitud_id", cab("id")},
                {"cliente_id", If(cab.IsNull("cliente_id"), Nothing, cab("cliente_id"))},
                {"cliente", If(cab.IsNull("cliente_nombre"), Nothing, cab("cliente_nombre"))},
                {"rfc", If(cab.IsNull("rfc"), Nothing, cab("rfc"))},
                {"curp", If(cab.IsNull("curp"), Nothing, cab("curp"))},
                {"producto", If(cab.IsNull("producto"), Nothing, cab("producto"))},
                {"tipo_credito", If(cab.IsNull("tipo_credito"), Nothing, cab("tipo_credito"))},
                {"moneda_id", If(cab.IsNull("moneda_id"), Nothing, cab("moneda_id"))},
                {"moneda", If(cab.IsNull("moneda"), Nothing, cab("moneda"))},
                {"moneda_clave", If(cab.IsNull("moneda_clave"), Nothing, cab("moneda_clave"))},
                {"monto_solicitado", If(cab.IsNull("monto_solicitado"), Nothing, cab("monto_solicitado"))},
                {"monto_autorizado", If(limite.HasValue, CType(limite.Value, Object), Nothing)},
                {"fecha_vigencia_inicio", If(cab.IsNull("fecha_vigencia_inicio"), Nothing, cab("fecha_vigencia_inicio"))},
                {"fecha_vigencia_fin", If(cab.IsNull("fecha_vigencia_fin"), Nothing, cab("fecha_vigencia_fin"))},
                {"estatus", If(cab.IsNull("estatus"), Nothing, cab("estatus"))},
                {"capital_dispuesto", saldos("capital_dispuesto")},
                {"capital_amortizado", saldos("capital_amortizado")},
                {"saldo_utilizado", saldos("saldo_utilizado")},
                {"disponible", If(disponible.HasValue, CType(disponible.Value, Object), Nothing)}
            }
        End Using

        Return respuesta
    End Function

    Private Function ConfigurarLinea(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(Param(context, "solicitud_id"), 0)
        Dim montoAutorizado As Decimal = ToDec(Param(context, "monto_autorizado"), 0D)
        Dim fechaInicio = ToDateNullable(Param(context, "fecha_vigencia_inicio"))
        Dim fechaFin = ToDateNullable(Param(context, "fecha_vigencia_fin"))

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "solicitud_id inválido."
            Return respuesta
        End If
        If montoAutorizado <= 0D Then
            respuesta("ok") = False
            respuesta("mensaje") = "El límite autorizado debe ser mayor a cero."
            Return respuesta
        End If
        If Not fechaInicio.HasValue OrElse Not fechaFin.HasValue Then
            respuesta("ok") = False
            respuesta("mensaje") = "La vigencia inicial y final son obligatorias."
            Return respuesta
        End If
        If fechaFin.Value.Date < fechaInicio.Value.Date Then
            respuesta("ok") = False
            respuesta("mensaje") = "La fecha fin no puede ser anterior a la fecha inicio."
            Return respuesta
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim cab As DataRow = ObtenerCabecera(cn, tr, solicitudId, True)
                    Dim err As String = ValidarRevolvente(cab)
                    If err <> "" Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = err
                        Return respuesta
                    End If

                    Dim estatus As String = If(cab.IsNull("estatus"), "", Convert.ToString(cab("estatus"), CultureInfo.InvariantCulture)).Trim().ToUpperInvariant()
                    If estatus <> "FINALIZADA" Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La solicitud debe estar FINALIZADA antes de configurar la línea revolvente."
                        Return respuesta
                    End If

                    Dim saldos = ObtenerSaldos(cn, tr, solicitudId, True)
                    If montoAutorizado < saldos("saldo_utilizado") Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "El límite autorizado no puede ser menor al capital actualmente utilizado."
                        Return respuesta
                    End If

                    Using cmd As New SqlCommand(
                        "UPDATE dbo.solicitud_credito " &
                        "SET monto_autorizado=@monto, fecha_vigencia_inicio=@inicio, fecha_vigencia_fin=@fin, " &
                        "    modificado_por=@usuario, fecha_modificacion=SYSDATETIME() " &
                        "WHERE id=@id;", cn, tr)
                        cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = montoAutorizado
                        cmd.Parameters("@monto").Precision = 18
                        cmd.Parameters("@monto").Scale = 2
                        cmd.Parameters.Add("@inicio", SqlDbType.Date).Value = fechaInicio.Value.Date
                        cmd.Parameters.Add("@fin", SqlDbType.Date).Value = fechaFin.Value.Date
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(context)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
                        cmd.ExecuteNonQuery()
                    End Using

                    tr.Commit()
                    respuesta("ok") = True
                    respuesta("mensaje") = "Línea revolvente configurada correctamente."
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function ListarDisposiciones(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(Param(context, "solicitud_id"), 0)

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "solicitud_id inválido."
            Return respuesta
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim sql As String =
                "SELECT id, solicitud_credito_id, fecha_disposicion, monto, moneda_id, referencia, " &
                "       estatus, activo, observaciones, motivo_reversa, creado_por, fecha_creacion, " &
                "       modificado_por, fecha_modificacion " &
                "FROM dbo.credito_disposiciones " &
                "WHERE solicitud_credito_id=@id " &
                "ORDER BY fecha_disposicion DESC, id DESC;"

            Dim dt = QueryTable(
                cn, Nothing, sql,
                New List(Of SqlParameter) From {
                    New SqlParameter("@id", SqlDbType.Int) With {.Value = solicitudId}
                }
            )

            respuesta("ok") = True
            respuesta("data") = TableToList(dt)
        End Using

        Return respuesta
    End Function

    Private Function CrearDisposicion(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(Param(context, "solicitud_id"), 0)
        Dim monto As Decimal = ToDec(Param(context, "monto"), 0D)
        Dim fecha = ToDateNullable(Param(context, "fecha_disposicion"))
        Dim referencia As String = Param(context, "referencia").Trim()
        Dim observaciones As String = Param(context, "observaciones").Trim()

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "solicitud_id inválido."
            Return respuesta
        End If
        If monto <= 0D Then
            respuesta("ok") = False
            respuesta("mensaje") = "El monto de la disposición debe ser mayor a cero."
            Return respuesta
        End If
        If Not fecha.HasValue Then fecha = DateTime.Now

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim cab As DataRow = ObtenerCabecera(cn, tr, solicitudId, True)
                    Dim err As String = ValidarRevolvente(cab)
                    If err <> "" Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = err
                        Return respuesta
                    End If

                    Dim estatus As String = If(cab.IsNull("estatus"), "", Convert.ToString(cab("estatus"), CultureInfo.InvariantCulture)).Trim().ToUpperInvariant()
                    If estatus <> "FINALIZADA" Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La solicitud debe estar FINALIZADA antes de registrar disposiciones."
                        Return respuesta
                    End If

                    If cab.IsNull("monto_autorizado") OrElse cab.IsNull("fecha_vigencia_inicio") OrElse cab.IsNull("fecha_vigencia_fin") Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "Primero configura el límite autorizado y la vigencia de la línea."
                        Return respuesta
                    End If

                    Dim inicio As DateTime = Convert.ToDateTime(cab("fecha_vigencia_inicio"), CultureInfo.InvariantCulture).Date
                    Dim fin As DateTime = Convert.ToDateTime(cab("fecha_vigencia_fin"), CultureInfo.InvariantCulture).Date
                    If fecha.Value.Date < inicio OrElse fecha.Value.Date > fin Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La fecha de disposición está fuera de la vigencia de la línea."
                        Return respuesta
                    End If

                    Dim limite As Decimal = Convert.ToDecimal(cab("monto_autorizado"), CultureInfo.InvariantCulture)
                    Dim saldos = ObtenerSaldos(cn, tr, solicitudId, True)
                    Dim disponible As Decimal = limite - saldos("saldo_utilizado")

                    If disponible < 0D Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La línea presenta un saldo utilizado mayor al límite autorizado. Debe revisarse antes de disponer."
                        Return respuesta
                    End If

                    If monto > disponible Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La disposición excede el disponible de la línea."
                        respuesta("disponible") = disponible
                        Return respuesta
                    End If

                    Dim nuevoId As Integer
                    Using cmd As New SqlCommand(
                        "INSERT INTO dbo.credito_disposiciones " &
                        "(solicitud_credito_id, fecha_disposicion, monto, moneda_id, referencia, estatus, activo, observaciones, creado_por, fecha_creacion) " &
                        "VALUES " &
                        "(@solicitud, @fecha, @monto, @moneda, @referencia, N'APLICADA', 1, @observaciones, @usuario, SYSDATETIME()); " &
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tr)
                        cmd.Parameters.Add("@solicitud", SqlDbType.Int).Value = solicitudId
                        cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = fecha.Value
                        cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = monto
                        cmd.Parameters("@monto").Precision = 18
                        cmd.Parameters("@monto").Scale = 2
                        If cab.IsNull("moneda_id") Then
                            cmd.Parameters.Add("@moneda", SqlDbType.Int).Value = DBNull.Value
                        Else
                            cmd.Parameters.Add("@moneda", SqlDbType.Int).Value = Convert.ToInt32(cab("moneda_id"))
                        End If
                        cmd.Parameters.Add("@referencia", SqlDbType.NVarChar, 100).Value = DbValue(referencia)
                        cmd.Parameters.Add("@observaciones", SqlDbType.NVarChar, 1000).Value = DbValue(observaciones)
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(context)
                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    tr.Commit()
                    respuesta("ok") = True
                    respuesta("mensaje") = "Disposición aplicada correctamente."
                    respuesta("disposicion_id") = nuevoId
                    respuesta("disponible_despues") = disponible - monto
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function ReversarDisposicion(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim disposicionId As Integer = ToInt(Param(context, "disposicion_id"), 0)
        Dim motivo As String = Param(context, "motivo").Trim()

        If disposicionId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "disposicion_id inválido."
            Return respuesta
        End If
        If String.IsNullOrWhiteSpace(motivo) Then
            respuesta("ok") = False
            respuesta("mensaje") = "El motivo de reversa es obligatorio."
            Return respuesta
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim dt = QueryTable(
                        cn, tr,
                        "SELECT TOP 1 id, solicitud_credito_id, fecha_disposicion, estatus, activo " &
                        "FROM dbo.credito_disposiciones WITH (UPDLOCK, HOLDLOCK) WHERE id=@id;",
                        New List(Of SqlParameter) From {
                            New SqlParameter("@id", SqlDbType.Int) With {.Value = disposicionId}
                        }
                    )

                    If dt.Rows.Count = 0 Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró la disposición."
                        Return respuesta
                    End If

                    Dim row As DataRow = dt.Rows(0)
                    If ToInt(row("activo"), 0) <> 1 OrElse Convert.ToString(row("estatus"), CultureInfo.InvariantCulture).ToUpperInvariant() <> "APLICADA" Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "La disposición ya no está aplicada."
                        Return respuesta
                    End If

                    Dim solicitudId As Integer = Convert.ToInt32(row("solicitud_credito_id"))
                    Dim fechaDisp As DateTime = Convert.ToDateTime(row("fecha_disposicion"), CultureInfo.InvariantCulture)

                    Dim posteriores As Integer = 0
                    Using cmd As New SqlCommand(
                        "SELECT " &
                        " (SELECT COUNT(1) FROM dbo.credito_disposiciones WITH (UPDLOCK, HOLDLOCK) " &
                        "  WHERE solicitud_credito_id=@sid AND activo=1 AND estatus=N'APLICADA' " &
                        "    AND id<>@did AND fecha_disposicion>=@fecha) " &
                        " + " &
                        " (SELECT COUNT(1) FROM dbo.pagos_credito WITH (UPDLOCK, HOLDLOCK) " &
                        "  WHERE solicitud_credito_id=@sid AND activo=1 AND estatus=N'APLICADO' " &
                        "    AND fecha_pago>=@fecha);", cn, tr)
                        cmd.Parameters.Add("@sid", SqlDbType.Int).Value = solicitudId
                        cmd.Parameters.Add("@did", SqlDbType.Int).Value = disposicionId
                        cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = fechaDisp
                        posteriores = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    If posteriores > 0 Then
                        tr.Rollback()
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se puede reversar porque existen movimientos posteriores o simultáneos. Debe revisarse manualmente."
                        Return respuesta
                    End If

                    Using cmd As New SqlCommand(
                        "UPDATE dbo.credito_disposiciones " &
                        "SET estatus=N'REVERSADA', activo=0, motivo_reversa=@motivo, " &
                        "    modificado_por=@usuario, fecha_modificacion=SYSDATETIME() " &
                        "WHERE id=@id;", cn, tr)
                        cmd.Parameters.Add("@motivo", SqlDbType.NVarChar, 500).Value = motivo
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(context)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = disposicionId
                        cmd.ExecuteNonQuery()
                    End Using

                    tr.Commit()
                    respuesta("ok") = True
                    respuesta("mensaje") = "Disposición reversada correctamente."
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function Historial(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(Param(context, "solicitud_id"), 0)

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "solicitud_id inválido."
            Return respuesta
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()

            Dim sql As String =
                ";WITH mov AS (" &
                " SELECT cd.fecha_disposicion AS fecha, CAST(N'DISPOSICION' AS nvarchar(20)) AS tipo, " &
                "        cd.id AS movimiento_id, cd.monto AS cargo, CAST(0 AS decimal(18,2)) AS abono, " &
                "        CAST(0 AS decimal(18,2)) AS capital_abonado, cd.monto AS delta_capital, " &
                "        cd.referencia AS referencia, cd.estatus, cd.observaciones " &
                " FROM dbo.credito_disposiciones cd " &
                " WHERE cd.solicitud_credito_id=@id AND cd.activo=1 AND cd.estatus=N'APLICADA' " &
                " UNION ALL " &
                " SELECT pc.fecha_pago AS fecha, CAST(N'PAGO' AS nvarchar(20)) AS tipo, " &
                "        pc.id AS movimiento_id, CAST(0 AS decimal(18,2)) AS cargo, pc.monto_pago AS abono, " &
                "        ISNULL(pc.monto_capital,0) AS capital_abonado, -ISNULL(pc.monto_capital,0) AS delta_capital, " &
                "        pc.referencia_pago AS referencia, pc.estatus, pc.observaciones " &
                " FROM dbo.pagos_credito pc " &
                " WHERE pc.solicitud_credito_id=@id AND pc.activo=1 AND pc.estatus=N'APLICADO' " &
                ") " &
                "SELECT fecha, tipo, movimiento_id, cargo, abono, capital_abonado, referencia, estatus, observaciones, " &
                "       CAST(SUM(delta_capital) OVER (ORDER BY fecha, CASE WHEN tipo=N'DISPOSICION' THEN 0 ELSE 1 END, movimiento_id ROWS UNBOUNDED PRECEDING) AS decimal(18,2)) AS saldo_principal " &
                "FROM mov " &
                "ORDER BY fecha DESC, movimiento_id DESC;"

            Dim dt = QueryTable(
                cn, Nothing, sql,
                New List(Of SqlParameter) From {
                    New SqlParameter("@id", SqlDbType.Int) With {.Value = solicitudId}
                }
            )

            respuesta("ok") = True
            respuesta("data") = TableToList(dt)
        End Using

        Return respuesta
    End Function

End Class
