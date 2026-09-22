
Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization
Imports System.Text

Public Class contacto_solicitud_handler
    Implements IHttpHandler

    '========================
    ' Config
    '========================
    Private ReadOnly _cnStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
    Private ReadOnly _json As New JavaScriptSerializer() With {.MaxJsonLength = Integer.MaxValue}

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        context.Response.ContentEncoding = Encoding.UTF8

        Try
            Dim action As String = (context.Request("action") & "").Trim().ToLowerInvariant()

            Select Case action
                Case "ping" : WriteOk(context, New With {.message = "pong"})

                Case "crear_contacto" : Accion_CrearContacto(context)
                Case "obtener_contacto" : Accion_ObtenerContacto(context)
                Case "actualizar_contacto" : Accion_ActualizarContacto(context)

                Case "listar_telefonos" : Accion_ListarTelefonos(context)
                Case "upsert_telefono" : Accion_UpsertTelefono(context)
                Case "set_principal_telefono" : Accion_SetPrincipalTelefono(context)
                Case "toggle_telefono" : Accion_ToggleTelefono(context)

                Case "listar_emails" : Accion_ListarEmails(context)
                Case "upsert_email" : Accion_UpsertEmail(context)
                Case "set_principal_email" : Accion_SetPrincipalEmail(context)
                Case "toggle_email" : Accion_ToggleEmail(context)

                Case "listar_domicilios" : Accion_ListarDomicilios(context)
                Case "upsert_domicilio" : Accion_UpsertDomicilio(context)
                Case "set_principal_domicilio" : Accion_SetPrincipalDomicilio(context)
                Case "toggle_domicilio" : Accion_ToggleDomicilio(context)

                Case Else
                    WriteErr(context, "Acción no soportada. Usa: ping | crear_contacto | obtener_contacto | actualizar_contacto | listar_telefonos | upsert_telefono | set_principal_telefono | toggle_telefono | listar_emails | upsert_email | set_principal_email | toggle_email | listar_domicilios | upsert_domicilio | set_principal_domicilio | toggle_domicilio")
            End Select

        Catch ex As Exception
            WriteErr(context, "Error inesperado: " & ex.Message)
        End Try
    End Sub

    '========================
    ' Helpers JSON + Request
    '========================
    Private Sub WriteOk(ctx As HttpContext, data As Object)
        Dim payload = New Dictionary(Of String, Object) From {{"ok", True}}
        If data IsNot Nothing Then
            Dim t = data.GetType()
            If TypeOf data Is String Then
                payload("message") = CStr(data)
            Else
                For Each prop In t.GetProperties()
                    payload(prop.Name) = prop.GetValue(data, Nothing)
                Next
            End If
        End If
        ctx.Response.Write(_json.Serialize(payload))
    End Sub

    Private Sub WriteErr(ctx As HttpContext, message As String)
        Dim payload = New Dictionary(Of String, Object) From {
            {"ok", False},
            {"message", message}
        }
        ctx.Response.Write(_json.Serialize(payload))
    End Sub

    Private Function ReadBodyJson(ctx As HttpContext) As Dictionary(Of String, Object)
        ctx.Request.InputStream.Position = 0
        Using sr As New IO.StreamReader(ctx.Request.InputStream, Encoding.UTF8)
            Dim raw = sr.ReadToEnd()
            If String.IsNullOrWhiteSpace(raw) Then
                Return New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
            End If
            Return _json.Deserialize(Of Dictionary(Of String, Object))(raw)
        End Using
    End Function

    Private Function GetInt(dic As Dictionary(Of String, Object), key As String, Optional def As Integer = 0) As Integer
        If dic Is Nothing OrElse Not dic.ContainsKey(key) OrElse dic(key) Is Nothing Then Return def
        Dim s = dic(key).ToString().Trim()
        Dim n As Integer
        If Integer.TryParse(s, n) Then Return n
        Return def
    End Function

    Private Function GetBool(dic As Dictionary(Of String, Object), key As String, Optional def As Boolean = False) As Boolean
        If dic Is Nothing OrElse Not dic.ContainsKey(key) OrElse dic(key) Is Nothing Then Return def
        Dim s = dic(key).ToString().Trim().ToLowerInvariant()
        If s = "true" OrElse s = "1" OrElse s = "on" OrElse s = "si" OrElse s = "sí" Then Return True
        If s = "false" OrElse s = "0" OrElse s = "off" Then Return False
        Return def
    End Function

    Private Function GetStr(dic As Dictionary(Of String, Object), key As String, Optional def As String = "") As String
        If dic Is Nothing OrElse Not dic.ContainsKey(key) OrElse dic(key) Is Nothing Then Return def
        Return dic(key).ToString().Trim()
    End Function

    Private Function GetQueryInt(ctx As HttpContext, key As String, Optional def As Integer = 0) As Integer
        Dim s = (ctx.Request(key) & "").Trim()
        Dim n As Integer
        If Integer.TryParse(s, n) Then Return n
        Return def
    End Function

    Private Function GetQueryBool(ctx As HttpContext, key As String, Optional def As Boolean = False) As Boolean
        Dim s = (ctx.Request(key) & "").Trim().ToLowerInvariant()
        If s = "1" OrElse s = "true" OrElse s = "si" OrElse s = "sí" OrElse s = "on" Then Return True
        If s = "0" OrElse s = "false" OrElse s = "off" Then Return False
        Return def
    End Function

    Private Function GetQueryStr(ctx As HttpContext, key As String, Optional def As String = "") As String
        Return (ctx.Request(key) & "").Trim()
    End Function

    '========================
    ' DB helpers
    '========================
    Private Function NewConn() As SqlConnection
        Return New SqlConnection(_cnStr)
    End Function

    Private Function ExecScalar(cmd As SqlCommand) As Object
        Using cn = cmd.Connection
            If cn.State <> ConnectionState.Open Then cn.Open()
            Return cmd.ExecuteScalar()
        End Using
    End Function

    Private Function ExecNonQuery(cmd As SqlCommand, Optional tx As SqlTransaction = Nothing) As Integer
        If tx IsNot Nothing Then cmd.Transaction = tx
        Using cn = cmd.Connection
            If cn.State <> ConnectionState.Open Then cn.Open()
            Return cmd.ExecuteNonQuery()
        End Using
    End Function

    Private Function QueryTable(cmd As SqlCommand) As DataTable
        Using cn = cmd.Connection
            If cn.State <> ConnectionState.Open Then cn.Open()
            Using da As New SqlDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)
                Return dt
            End Using
        End Using
    End Function

    Private Function ToRows(dt As DataTable) As List(Of Dictionary(Of String, Object))
        Dim list As New List(Of Dictionary(Of String, Object))()
        For Each r As DataRow In dt.Rows
            Dim obj As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
            For Each c As DataColumn In dt.Columns
                obj(c.ColumnName) = If(IsDBNull(r(c)), Nothing, r(c))
            Next
            list.Add(obj)
        Next
        Return list
    End Function

    '========================
    ' A) CONTACTO (maestro)
    '========================
    Private Sub Accion_CrearContacto(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim solicitud_id = GetInt(body, "solicitud_id", 0)
        If solicitud_id <= 0 Then
            WriteErr(ctx, "solicitud_id requerido.")
            Exit Sub
        End If

        ' Validar solicitud
        Dim existe As Integer = 0
        Using cmd As New SqlCommand("SELECT COUNT(1) FROM dbo.solicitud_credito WHERE id=@id", NewConn())
            cmd.Parameters.AddWithValue("@id", solicitud_id)
            existe = Convert.ToInt32(ExecScalar(cmd))
        End Using
        If existe = 0 Then
            WriteErr(ctx, "Solicitud no encontrada.")
            Exit Sub
        End If

        ' Si ya existe contacto activo, devolverlo
        Dim contacto_id As Integer = 0
        Using cmd As New SqlCommand("SELECT TOP 1 id FROM dbo.contacto_solicitud WHERE solicitud_id=@s AND activo=1", NewConn())
            cmd.Parameters.AddWithValue("@s", solicitud_id)
            Dim o = ExecScalar(cmd)
            If o IsNot Nothing AndAlso o IsNot DBNull.Value Then contacto_id = Convert.ToInt32(o)
        End Using

        If contacto_id = 0 Then
            Using cn = NewConn()
                cn.Open()
                Using tx = cn.BeginTransaction()
                    Try
                        Dim medio_contacto_id = If(GetInt(body, "medio_contacto_id", 0) > 0, GetInt(body, "medio_contacto_id", 0), CType(Nothing, Object))
                        Dim consentimiento = GetBool(body, "consentimiento_comunicacion", False)

                        Using cmd As New SqlCommand("
INSERT INTO dbo.contacto_solicitud (solicitud_id, medio_contacto_id, consentimiento_comunicacion, activo, fecha_creacion)
VALUES (@s, @m, @c, 1, GETDATE());
SELECT SCOPE_IDENTITY();", cn, tx)
                            cmd.Parameters.AddWithValue("@s", solicitud_id)
                            If medio_contacto_id Is Nothing Then
                                cmd.Parameters.AddWithValue("@m", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@m", CInt(medio_contacto_id))
                            End If
                            cmd.Parameters.AddWithValue("@c", If(consentimiento, 1, 0))
                            Dim o = cmd.ExecuteScalar()
                            contacto_id = Convert.ToInt32(o)
                        End Using

                        tx.Commit()
                    Catch ex As Exception
                        tx.Rollback()
                        WriteErr(ctx, "No fue posible crear el contacto: " & ex.Message)
                        Exit Sub
                    End Try
                End Using
            End Using
        End If

        Using cmd As New SqlCommand("
SELECT id, solicitud_id, medio_contacto_id, consentimiento_comunicacion, activo, fecha_creacion, fecha_actualizacion
FROM dbo.contacto_solicitud WHERE id=@id", NewConn())
            cmd.Parameters.AddWithValue("@id", contacto_id)
            Dim dt = QueryTable(cmd)
            Dim data = If(dt.Rows.Count > 0, ToRows(dt)(0), Nothing)
            WriteOk(ctx, New With {.data = data})
        End Using
    End Sub

    Private Sub Accion_ObtenerContacto(ctx As HttpContext)
        Dim contacto_id = GetQueryInt(ctx, "contacto_id", 0)
        Dim solicitud_id = GetQueryInt(ctx, "solicitud_id", 0)

        If contacto_id = 0 AndAlso solicitud_id = 0 Then
            WriteErr(ctx, "Proporcione contacto_id o solicitud_id.")
            Exit Sub
        End If

        Dim sqlContacto As String = "SELECT TOP 1 id, solicitud_id, medio_contacto_id, consentimiento_comunicacion, activo, fecha_creacion, fecha_actualizacion
                                     FROM dbo.contacto_solicitud WHERE {0} ORDER BY id DESC"
        Dim filtro As String
        Dim cmd As SqlCommand

        If contacto_id > 0 Then
            filtro = "id=@id"
            cmd = New SqlCommand(String.Format(sqlContacto, filtro), NewConn())
            cmd.Parameters.AddWithValue("@id", contacto_id)
        Else
            filtro = "solicitud_id=@s"
            cmd = New SqlCommand(String.Format(sqlContacto, filtro), NewConn())
            cmd.Parameters.AddWithValue("@s", solicitud_id)
        End If

        Dim dtContacto = QueryTable(cmd)
        If dtContacto.Rows.Count = 0 Then
            WriteErr(ctx, "Contacto no encontrado.")
            Exit Sub
        End If

        Dim cRow = dtContacto.Rows(0)
        Dim cid = Convert.ToInt32(cRow("id"))

        ' Colecciones resumidas
        Dim dtTels As DataTable
        Using cm As New SqlCommand("
SELECT TOP 5 id, tipo_telefono, numero, extension, es_principal, activo, fecha_creacion
FROM dbo.contacto_solicitud_telefono
WHERE contacto_id=@c
ORDER BY es_principal DESC, id DESC", NewConn())
            cm.Parameters.AddWithValue("@c", cid)
            dtTels = QueryTable(cm)
        End Using

        Dim dtMails As DataTable
        Using cm As New SqlCommand("
SELECT TOP 5 id, email, es_principal, activo, fecha_creacion
FROM dbo.contacto_solicitud_email
WHERE contacto_id=@c
ORDER BY es_principal DESC, id DESC", NewConn())
            cm.Parameters.AddWithValue("@c", cid)
            dtMails = QueryTable(cm)
        End Using

        Dim dtDom As DataTable
        Using cm As New SqlCommand("
SELECT TOP 1 id, calle, num_ext, num_int, colonia, municipio, estado, pais, cp, es_principal, activo, fecha_creacion
FROM dbo.contacto_solicitud_domicilio
WHERE contacto_id=@c AND activo=1
ORDER BY es_principal DESC, id DESC", NewConn())
            cm.Parameters.AddWithValue("@c", cid)
            dtDom = QueryTable(cm)
        End Using

        Dim contacto = ToRows(dtContacto)(0)
        Dim telefonos = ToRows(dtTels)
        Dim emails = ToRows(dtMails)
        Dim domicilio_principal As Object = Nothing
        If dtDom.Rows.Count > 0 Then domicilio_principal = ToRows(dtDom)(0)

        Dim data As New Dictionary(Of String, Object) From {
            {"contacto", contacto},
            {"telefonos", telefonos},
            {"emails", emails},
            {"domicilio_principal", domicilio_principal}
        }

        WriteOk(ctx, New With {.data = data})
    End Sub

    Private Sub Accion_ActualizarContacto(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim contacto_id = GetInt(body, "contacto_id", 0)
        If contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido.")
            Exit Sub
        End If
        Dim medio_contacto_id As Object = Nothing
        Dim mc = GetInt(body, "medio_contacto_id", 0)
        If mc > 0 Then medio_contacto_id = mc

        Dim consentimiento = If(body.ContainsKey("consentimiento_comunicacion"), GetBool(body, "consentimiento_comunicacion", False), CType(Nothing, Boolean?))
        Dim activoFlag As Boolean? = Nothing
        If body.ContainsKey("activo") Then activoFlag = GetBool(body, "activo", True)

        Using cn = NewConn()
            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud
SET medio_contacto_id = @m,
    consentimiento_comunicacion = COALESCE(@c, consentimiento_comunicacion),
    activo = COALESCE(@a, activo),
    fecha_actualizacion = GETDATE()
WHERE id=@id", cn)
                cmd.Parameters.AddWithValue("@id", contacto_id)
                If medio_contacto_id Is Nothing Then
                    cmd.Parameters.AddWithValue("@m", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@m", CInt(medio_contacto_id))
                End If
                If consentimiento.HasValue Then
                    cmd.Parameters.AddWithValue("@c", If(consentimiento.Value, 1, 0))
                Else
                    cmd.Parameters.AddWithValue("@c", DBNull.Value)
                End If
                If activoFlag.HasValue Then
                    cmd.Parameters.AddWithValue("@a", If(activoFlag.Value, 1, 0))
                Else
                    cmd.Parameters.AddWithValue("@a", DBNull.Value)
                End If

                Dim n = ExecNonQuery(cmd)
                If n = 0 Then
                    WriteErr(ctx, "Contacto no encontrado.")
                    Exit Sub
                End If
            End Using
        End Using

        WriteOk(ctx, New With {.message = "Contacto actualizado."})
    End Sub

    '========================
    ' B) TELÉFONOS
    '========================
    Private Sub Accion_ListarTelefonos(ctx As HttpContext)
        Dim contacto_id = GetQueryInt(ctx, "contacto_id", 0)
        If contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido.")
            Exit Sub
        End If
        Dim search = GetQueryStr(ctx, "search", "")
        Dim page = Math.Max(1, GetQueryInt(ctx, "page", 1))
        Dim pageSize = Math.Max(1, Math.Min(100, GetQueryInt(ctx, "pageSize", 10)))
        Dim soloActivos = GetQueryBool(ctx, "solo_activos", False)

        Dim where As New Text.StringBuilder("WHERE contacto_id=@c ")
        If soloActivos Then where.Append("AND activo=1 ")
        If Not String.IsNullOrWhiteSpace(search) Then where.Append("AND (numero LIKE @q OR tipo_telefono LIKE @q) ")

        Dim total As Integer
        Using cmdTot As New SqlCommand("SELECT COUNT(1) FROM dbo.contacto_solicitud_telefono " & where.ToString(), NewConn())
            cmdTot.Parameters.AddWithValue("@c", contacto_id)
            If Not String.IsNullOrWhiteSpace(search) Then cmdTot.Parameters.AddWithValue("@q", "%" & search & "%")
            total = Convert.ToInt32(ExecScalar(cmdTot))
        End Using

        Dim offset = (page - 1) * pageSize
        Using cmd As New SqlCommand("
SELECT id, contacto_id, tipo_telefono, numero, extension, es_principal, activo, fecha_creacion, fecha_actualizacion
FROM dbo.contacto_solicitud_telefono
" & where.ToString() & "
ORDER BY es_principal DESC, id DESC
OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY;", NewConn())
            cmd.Parameters.AddWithValue("@c", contacto_id)
            If Not String.IsNullOrWhiteSpace(search) Then cmd.Parameters.AddWithValue("@q", "%" & search & "%")
            cmd.Parameters.AddWithValue("@off", offset)
            cmd.Parameters.AddWithValue("@ps", pageSize)

            Dim dt = QueryTable(cmd)
            WriteOk(ctx, New With {.data = ToRows(dt), .total = total, .page = page, .pageSize = pageSize})
        End Using
    End Sub

    Private Sub Accion_UpsertTelefono(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim contacto_id = GetInt(body, "contacto_id", 0)
        Dim tipo = GetStr(body, "tipo_telefono", "").ToUpperInvariant()
        Dim numero = GetStr(body, "numero", "")
        Dim extension = GetStr(body, "extension", Nothing)
        Dim es_principal = GetBool(body, "es_principal", False)
        Dim activo = If(body.ContainsKey("activo"), GetBool(body, "activo", True), True)

        If id = 0 AndAlso contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido para insertar.")
            Exit Sub
        End If
        If id = 0 Then
            If String.IsNullOrWhiteSpace(tipo) Then tipo = "MOVIL"
            If String.IsNullOrWhiteSpace(numero) Then
                WriteErr(ctx, "numero requerido.")
                Exit Sub
            End If
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    If id = 0 Then
                        Using cmd As New SqlCommand("
INSERT INTO dbo.contacto_solicitud_telefono (contacto_id, tipo_telefono, numero, extension, es_principal, activo, fecha_creacion)
VALUES (@c, @t, @n, @x, @p, @a, GETDATE());
SELECT SCOPE_IDENTITY();", cn, tx)
                            cmd.Parameters.AddWithValue("@c", contacto_id)
                            cmd.Parameters.AddWithValue("@t", tipo)
                            cmd.Parameters.AddWithValue("@n", numero)
                            If String.IsNullOrWhiteSpace(extension) Then
                                cmd.Parameters.AddWithValue("@x", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@x", extension)
                            End If
                            cmd.Parameters.AddWithValue("@p", If(es_principal, 1, 0))
                            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
                            Dim o = cmd.ExecuteScalar()
                            id = Convert.ToInt32(o)
                        End Using

                        If es_principal Then
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_telefono
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", contacto_id)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    Else
                        Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_telefono
SET tipo_telefono = COALESCE(@t, tipo_telefono),
    numero = COALESCE(@n, numero),
    extension = @x,
    es_principal = COALESCE(@p, es_principal),
    activo = COALESCE(@a, activo),
    fecha_actualizacion = GETDATE()
WHERE id=@id;", cn, tx)
                            cmd.Parameters.AddWithValue("@id", id)
                            If String.IsNullOrWhiteSpace(tipo) Then
                                cmd.Parameters.AddWithValue("@t", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@t", tipo)
                            End If
                            If String.IsNullOrWhiteSpace(numero) Then
                                cmd.Parameters.AddWithValue("@n", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@n", numero)
                            End If
                            If String.IsNullOrWhiteSpace(extension) Then
                                cmd.Parameters.AddWithValue("@x", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@x", extension)
                            End If
                            cmd.Parameters.AddWithValue("@p", If(body.ContainsKey("es_principal"), If(es_principal, 1, 0), CType(DBNull.Value, Object)))
                            cmd.Parameters.AddWithValue("@a", If(body.ContainsKey("activo"), If(activo, 1, 0), CType(DBNull.Value, Object)))
                            Dim n = cmd.ExecuteNonQuery()
                            If n = 0 Then
                                Throw New Exception("Teléfono no encontrado.")
                            End If
                        End Using

                        If body.ContainsKey("es_principal") AndAlso es_principal Then
                            Dim cId As Integer
                            Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_telefono WHERE id=@id", cn, tx)
                                ccmd.Parameters.AddWithValue("@id", id)
                                cId = Convert.ToInt32(ccmd.ExecuteScalar())
                            End Using
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_telefono
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", cId)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    End If

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Teléfono guardado.", .id = id})
                Catch ex As Exception
                    tx.Rollback()
                    WriteErr(ctx, "No fue posible guardar el teléfono: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_SetPrincipalTelefono(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    Dim contacto_id As Integer
                    Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_telefono WHERE id=@id", cn, tx)
                        ccmd.Parameters.AddWithValue("@id", id)
                        Dim o = ccmd.ExecuteScalar()
                        If o Is Nothing OrElse o Is DBNull.Value Then Throw New Exception("Teléfono no encontrado.")
                        contacto_id = Convert.ToInt32(o)
                    End Using

                    Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_telefono SET es_principal=1, fecha_actualizacion=GETDATE() WHERE id=@id;
UPDATE dbo.contacto_solicitud_telefono SET es_principal=0, fecha_actualizacion=GETDATE() WHERE contacto_id=@c AND id<>@id;", cn, tx)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.Parameters.AddWithValue("@c", contacto_id)
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Teléfono establecido como principal."})
                Catch ex As Exception
                    tx.Rollback()
                    WriteErr(ctx, "No fue posible establecer principal: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_ToggleTelefono(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim activo = GetBool(body, "activo", True)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cmd As New SqlCommand("UPDATE dbo.contacto_solicitud_telefono SET activo=@a, fecha_actualizacion=GETDATE() WHERE id=@id", NewConn())
            cmd.Parameters.AddWithValue("@id", id)
            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
            Dim n = ExecNonQuery(cmd)
            If n = 0 Then
                WriteErr(ctx, "Teléfono no encontrado.")
                Exit Sub
            End If
        End Using
        WriteOk(ctx, New With {.message = If(activo, "Teléfono activado.", "Teléfono desactivado.")})
    End Sub

    '========================
    ' C) EMAILS
    '========================
    Private Sub Accion_ListarEmails(ctx As HttpContext)
        Dim contacto_id = GetQueryInt(ctx, "contacto_id", 0)
        If contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido.")
            Exit Sub
        End If
        Dim search = GetQueryStr(ctx, "search", "")
        Dim page = Math.Max(1, GetQueryInt(ctx, "page", 1))
        Dim pageSize = Math.Max(1, Math.Min(100, GetQueryInt(ctx, "pageSize", 10)))
        Dim soloActivos = GetQueryBool(ctx, "solo_activos", False)

        Dim where As New Text.StringBuilder("WHERE contacto_id=@c ")
        If soloActivos Then where.Append("AND activo=1 ")
        If Not String.IsNullOrWhiteSpace(search) Then where.Append("AND (email LIKE @q) ")

        Dim total As Integer
        Using cmdTot As New SqlCommand("SELECT COUNT(1) FROM dbo.contacto_solicitud_email " & where.ToString(), NewConn())
            cmdTot.Parameters.AddWithValue("@c", contacto_id)
            If Not String.IsNullOrWhiteSpace(search) Then cmdTot.Parameters.AddWithValue("@q", "%" & search & "%")
            total = Convert.ToInt32(ExecScalar(cmdTot))
        End Using

        Dim offset = (page - 1) * pageSize
        Using cmd As New SqlCommand("
SELECT id, contacto_id, email, es_principal, activo, fecha_creacion, fecha_actualizacion
FROM dbo.contacto_solicitud_email
" & where.ToString() & "
ORDER BY es_principal DESC, id DESC
OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY;", NewConn())
            cmd.Parameters.AddWithValue("@c", contacto_id)
            If Not String.IsNullOrWhiteSpace(search) Then cmd.Parameters.AddWithValue("@q", "%" & search & "%")
            cmd.Parameters.AddWithValue("@off", offset)
            cmd.Parameters.AddWithValue("@ps", pageSize)

            Dim dt = QueryTable(cmd)
            WriteOk(ctx, New With {.data = ToRows(dt), .total = total, .page = page, .pageSize = pageSize})
        End Using
    End Sub

    Private Sub Accion_UpsertEmail(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim contacto_id = GetInt(body, "contacto_id", 0)
        Dim email = GetStr(body, "email", "")
        Dim es_principal = GetBool(body, "es_principal", False)
        Dim activo = If(body.ContainsKey("activo"), GetBool(body, "activo", True), True)

        If id = 0 AndAlso contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido para insertar.")
            Exit Sub
        End If
        If id = 0 AndAlso String.IsNullOrWhiteSpace(email) Then
            WriteErr(ctx, "email requerido.")
            Exit Sub
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    If id = 0 Then
                        Using cmd As New SqlCommand("
INSERT INTO dbo.contacto_solicitud_email (contacto_id, email, es_principal, activo, fecha_creacion)
VALUES (@c, @e, @p, @a, GETDATE());
SELECT SCOPE_IDENTITY();", cn, tx)
                            cmd.Parameters.AddWithValue("@c", contacto_id)
                            cmd.Parameters.AddWithValue("@e", email)
                            cmd.Parameters.AddWithValue("@p", If(es_principal, 1, 0))
                            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
                            Dim o = cmd.ExecuteScalar()
                            id = Convert.ToInt32(o)
                        End Using

                        If es_principal Then
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_email
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", contacto_id)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    Else
                        Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_email
SET email = COALESCE(@e, email),
    es_principal = COALESCE(@p, es_principal),
    activo = COALESCE(@a, activo),
    fecha_actualizacion = GETDATE()
WHERE id=@id;", cn, tx)
                            cmd.Parameters.AddWithValue("@id", id)
                            If String.IsNullOrWhiteSpace(email) Then
                                cmd.Parameters.AddWithValue("@e", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@e", email)
                            End If
                            cmd.Parameters.AddWithValue("@p", If(body.ContainsKey("es_principal"), If(es_principal, 1, 0), CType(DBNull.Value, Object)))
                            cmd.Parameters.AddWithValue("@a", If(body.ContainsKey("activo"), If(activo, 1, 0), CType(DBNull.Value, Object)))
                            Dim n = cmd.ExecuteNonQuery()
                            If n = 0 Then Throw New Exception("Email no encontrado.")
                        End Using

                        If body.ContainsKey("es_principal") AndAlso es_principal Then
                            Dim cId As Integer
                            Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_email WHERE id=@id", cn, tx)
                                ccmd.Parameters.AddWithValue("@id", id)
                                cId = Convert.ToInt32(ccmd.ExecuteScalar())
                            End Using
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_email
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", cId)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    End If

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Email guardado.", .id = id})
                Catch ex As Exception
                    tx.Rollback()
                    Dim msg = ex.Message
                    If msg.ToLowerInvariant().Contains("ux_contacto_mail__email_unico") Then
                        msg = "El email ya existe como activo para este contacto."
                    End If
                    WriteErr(ctx, "No fue posible guardar el email: " & msg)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_SetPrincipalEmail(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    Dim contacto_id As Integer
                    Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_email WHERE id=@id", cn, tx)
                        ccmd.Parameters.AddWithValue("@id", id)
                        Dim o = ccmd.ExecuteScalar()
                        If o Is Nothing OrElse o Is DBNull.Value Then Throw New Exception("Email no encontrado.")
                        contacto_id = Convert.ToInt32(o)
                    End Using

                    Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_email SET es_principal=1, fecha_actualizacion=GETDATE() WHERE id=@id;
UPDATE dbo.contacto_solicitud_email SET es_principal=0, fecha_actualizacion=GETDATE() WHERE contacto_id=@c AND id<>@id;", cn, tx)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.Parameters.AddWithValue("@c", contacto_id)
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Email establecido como principal."})
                Catch ex As Exception
                    tx.Rollback()
                    WriteErr(ctx, "No fue posible establecer principal: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_ToggleEmail(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim activo = GetBool(body, "activo", True)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cmd As New SqlCommand("UPDATE dbo.contacto_solicitud_email SET activo=@a, fecha_actualizacion=GETDATE() WHERE id=@id", NewConn())
            cmd.Parameters.AddWithValue("@id", id)
            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
            Dim n = ExecNonQuery(cmd)
            If n = 0 Then
                WriteErr(ctx, "Email no encontrado.")
                Exit Sub
            End If
        End Using
        WriteOk(ctx, New With {.message = If(activo, "Email activado.", "Email desactivado.")})
    End Sub

    '========================
    ' D) DOMICILIOS
    '========================
    Private Sub Accion_ListarDomicilios(ctx As HttpContext)
        Dim contacto_id = GetQueryInt(ctx, "contacto_id", 0)
        If contacto_id <= 0 Then
            WriteErr(ctx, "contacto_id requerido.")
            Exit Sub
        End If

        Dim page = Math.Max(1, GetQueryInt(ctx, "page", 1))
        Dim pageSize = Math.Max(1, Math.Min(100, GetQueryInt(ctx, "pageSize", 10)))
        Dim soloActivos = GetQueryBool(ctx, "solo_activos", False)

        Dim where = "WHERE contacto_id=@c " & If(soloActivos, "AND activo=1 ", "")

        Dim total As Integer
        Using cmdTot As New SqlCommand("SELECT COUNT(1) FROM dbo.contacto_solicitud_domicilio " & where, NewConn())
            cmdTot.Parameters.AddWithValue("@c", contacto_id)
            total = Convert.ToInt32(ExecScalar(cmdTot))
        End Using

        Dim offset = (page - 1) * pageSize
        Using cmd As New SqlCommand("
SELECT id, contacto_id, calle, num_ext, num_int, colonia, municipio, estado, pais, cp, es_principal, activo, fecha_creacion, fecha_actualizacion
FROM dbo.contacto_solicitud_domicilio
" & where & "
ORDER BY es_principal DESC, id DESC
OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY;", NewConn())
            cmd.Parameters.AddWithValue("@c", contacto_id)
            cmd.Parameters.AddWithValue("@off", offset)
            cmd.Parameters.AddWithValue("@ps", pageSize)

            Dim dt = QueryTable(cmd)
            WriteOk(ctx, New With {.data = ToRows(dt), .total = total, .page = page, .pageSize = pageSize})
        End Using
    End Sub

    Private Sub Accion_UpsertDomicilio(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim contacto_id = GetInt(body, "contacto_id", 0)
        Dim calle = GetStr(body, "calle", "")
        Dim num_ext = GetStr(body, "num_ext", "")
        Dim num_int = GetStr(body, "num_int", Nothing)
        Dim colonia = GetStr(body, "colonia", "")
        Dim municipio = GetStr(body, "municipio", "")
        Dim estado = GetStr(body, "estado", "")
        Dim pais = GetStr(body, "pais", "")
        Dim cp = GetStr(body, "cp", "")
        Dim es_principal = GetBool(body, "es_principal", False)
        Dim activo = If(body.ContainsKey("activo"), GetBool(body, "activo", True), True)

        If id = 0 Then
            If contacto_id <= 0 Then
                WriteErr(ctx, "contacto_id requerido.")
                Exit Sub
            End If
            If String.IsNullOrWhiteSpace(calle) OrElse String.IsNullOrWhiteSpace(num_ext) OrElse
               String.IsNullOrWhiteSpace(colonia) OrElse String.IsNullOrWhiteSpace(municipio) OrElse
               String.IsNullOrWhiteSpace(estado) OrElse String.IsNullOrWhiteSpace(pais) OrElse
               String.IsNullOrWhiteSpace(cp) Then
                WriteErr(ctx, "Campos obligatorios: calle, num_ext, colonia, municipio, estado, pais, cp.")
                Exit Sub
            End If
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    If id = 0 Then
                        Using cmd As New SqlCommand("
INSERT INTO dbo.contacto_solicitud_domicilio
(contacto_id, calle, num_ext, num_int, colonia, municipio, estado, pais, cp, es_principal, activo, fecha_creacion)
VALUES
(@c, @calle, @ext, @int, @col, @mun, @edo, @pais, @cp, @p, @a, GETDATE());
SELECT SCOPE_IDENTITY();", cn, tx)
                            cmd.Parameters.AddWithValue("@c", contacto_id)
                            cmd.Parameters.AddWithValue("@calle", calle)
                            cmd.Parameters.AddWithValue("@ext", num_ext)
                            If String.IsNullOrWhiteSpace(num_int) Then
                                cmd.Parameters.AddWithValue("@int", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@int", num_int)
                            End If
                            cmd.Parameters.AddWithValue("@col", colonia)
                            cmd.Parameters.AddWithValue("@mun", municipio)
                            cmd.Parameters.AddWithValue("@edo", estado)
                            cmd.Parameters.AddWithValue("@pais", pais)
                            cmd.Parameters.AddWithValue("@cp", cp)
                            cmd.Parameters.AddWithValue("@p", If(es_principal, 1, 0))
                            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
                            Dim o = cmd.ExecuteScalar()
                            id = Convert.ToInt32(o)
                        End Using

                        If es_principal Then
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_domicilio
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", contacto_id)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If

                    Else
                        Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_domicilio
SET calle = COALESCE(@calle, calle),
    num_ext = COALESCE(@ext, num_ext),
    num_int = @int,
    colonia = COALESCE(@col, colonia),
    municipio = COALESCE(@mun, municipio),
    estado = COALESCE(@edo, estado),
    pais = COALESCE(@pais, pais),
    cp = COALESCE(@cp, cp),
    es_principal = COALESCE(@p, es_principal),
    activo = COALESCE(@a, activo),
    fecha_actualizacion = GETDATE()
WHERE id=@id;", cn, tx)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.Parameters.AddWithValue("@calle", If(String.IsNullOrWhiteSpace(calle), CType(DBNull.Value, Object), calle))
                            cmd.Parameters.AddWithValue("@ext", If(String.IsNullOrWhiteSpace(num_ext), CType(DBNull.Value, Object), num_ext))
                            cmd.Parameters.AddWithValue("@int", If(String.IsNullOrWhiteSpace(num_int), CType(DBNull.Value, Object), num_int))
                            cmd.Parameters.AddWithValue("@col", If(String.IsNullOrWhiteSpace(colonia), CType(DBNull.Value, Object), colonia))
                            cmd.Parameters.AddWithValue("@mun", If(String.IsNullOrWhiteSpace(municipio), CType(DBNull.Value, Object), municipio))
                            cmd.Parameters.AddWithValue("@edo", If(String.IsNullOrWhiteSpace(estado), CType(DBNull.Value, Object), estado))
                            cmd.Parameters.AddWithValue("@pais", If(String.IsNullOrWhiteSpace(pais), CType(DBNull.Value, Object), pais))
                            cmd.Parameters.AddWithValue("@cp", If(String.IsNullOrWhiteSpace(cp), CType(DBNull.Value, Object), cp))
                            cmd.Parameters.AddWithValue("@p", If(body.ContainsKey("es_principal"), If(es_principal, 1, 0), CType(DBNull.Value, Object)))
                            cmd.Parameters.AddWithValue("@a", If(body.ContainsKey("activo"), If(activo, 1, 0), CType(DBNull.Value, Object)))
                            Dim n = cmd.ExecuteNonQuery()
                            If n = 0 Then Throw New Exception("Domicilio no encontrado.")
                        End Using

                        If body.ContainsKey("es_principal") AndAlso es_principal Then
                            Dim cId As Integer
                            Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_domicilio WHERE id=@id", cn, tx)
                                ccmd.Parameters.AddWithValue("@id", id)
                                cId = Convert.ToInt32(ccmd.ExecuteScalar())
                            End Using
                            Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_domicilio
SET es_principal=0, fecha_actualizacion=GETDATE()
WHERE contacto_id=@c AND id<>@id;", cn, tx)
                                cmd.Parameters.AddWithValue("@c", cId)
                                cmd.Parameters.AddWithValue("@id", id)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    End If

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Domicilio guardado.", .id = id})
                Catch ex As Exception
                    tx.Rollback()
                    WriteErr(ctx, "No fue posible guardar el domicilio: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_SetPrincipalDomicilio(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cn = NewConn()
            cn.Open()
            Using tx = cn.BeginTransaction()
                Try
                    Dim contacto_id As Integer
                    Using ccmd As New SqlCommand("SELECT contacto_id FROM dbo.contacto_solicitud_domicilio WHERE id=@id", cn, tx)
                        ccmd.Parameters.AddWithValue("@id", id)
                        Dim o = ccmd.ExecuteScalar()
                        If o Is Nothing OrElse o Is DBNull.Value Then Throw New Exception("Domicilio no encontrado.")
                        contacto_id = Convert.ToInt32(o)
                    End Using

                    Using cmd As New SqlCommand("
UPDATE dbo.contacto_solicitud_domicilio SET es_principal=1, fecha_actualizacion=GETDATE() WHERE id=@id;
UPDATE dbo.contacto_solicitud_domicilio SET es_principal=0, fecha_actualizacion=GETDATE() WHERE contacto_id=@c AND id<>@id;", cn, tx)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.Parameters.AddWithValue("@c", contacto_id)
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    WriteOk(ctx, New With {.message = "Domicilio establecido como principal."})
                Catch ex As Exception
                    tx.Rollback()
                    WriteErr(ctx, "No fue posible establecer principal: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub Accion_ToggleDomicilio(ctx As HttpContext)
        Dim body = ReadBodyJson(ctx)
        Dim id = GetInt(body, "id", 0)
        Dim activo = GetBool(body, "activo", True)
        If id <= 0 Then
            WriteErr(ctx, "id requerido.")
            Exit Sub
        End If

        Using cmd As New SqlCommand("UPDATE dbo.contacto_solicitud_domicilio SET activo=@a, fecha_actualizacion=GETDATE() WHERE id=@id", NewConn())
            cmd.Parameters.AddWithValue("@id", id)
            cmd.Parameters.AddWithValue("@a", If(activo, 1, 0))
            Dim n = ExecNonQuery(cmd)
            If n = 0 Then
                WriteErr(ctx, "Domicilio no encontrado.")
                Exit Sub
            End If
        End Using
        WriteOk(ctx, New With {.message = If(activo, "Domicilio activado.", "Domicilio desactivado.")})
    End Sub

End Class
