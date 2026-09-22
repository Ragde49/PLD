Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization
Imports System.Globalization
Imports System.Text

Public Class handler_catalogo_instrumento_monetario
    Implements IHttpHandler

    ' === CONEXIÓN OFICIAL ===
    Private ReadOnly Property ConnectionString As String
        Get
            Dim cs As String = String.Empty
            If ConfigurationManager.ConnectionStrings("PLDConnection") IsNot Nothing Then
                cs = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings.Count > 0 Then
                cs = ConfigurationManager.ConnectionStrings(0).ConnectionString
            End If
            Return cs
        End Get
    End Property

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache)

        Dim op As String = (context.Request("op") & "").ToLowerInvariant()

        Try
            Select Case op
                Case "consultar"
                    Consultar(context)
                Case "obtener"
                    Obtener(context)
                Case "guardar"
                    Guardar(context)
                Case "actualizar"
                    Actualizar(context)
                Case "toggle"
                    ToggleActivo(context)
                Case Else
                    WriteJson(context, New With {.ok = False, .message = "Operación no válida.", .op = op})
            End Select
        Catch ex As Exception
            WriteJson(context, New With {.ok = False, .message = "Error inesperado: " & Sanitize(ex.Message)})
        End Try
    End Sub

    ' ---------------------------
    '  CONSULTAR (DataTables ready)
    ' ---------------------------
    Private Sub Consultar(ByVal context As HttpContext)
        Dim draw As Integer = SafeInt(context.Request("draw"))
        Dim start As Integer = SafeInt(context.Request("start"), 0)
        Dim length As Integer = SafeInt(context.Request("length"), 10)
        If length <= 0 Then length = 10
        Dim searchValue As String = (context.Request("search[value]") & "").Trim()

        Dim totalRecords As Integer = 0
        Dim totalFiltered As Integer = 0

        Dim data As New List(Of Object)()

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()

            ' Total sin filtro
            Using cmdCount As New SqlCommand("SELECT COUNT(*) FROM dbo.catalogo_instrumento_monetario", cn)
                totalRecords = Convert.ToInt32(cmdCount.ExecuteScalar())
            End Using

            ' Query base
            Dim sb As New StringBuilder()
            sb.AppendLine("WITH base AS (")
            sb.AppendLine("  SELECT")
            sb.AppendLine("    im.id,")
            sb.AppendLine("    im.instrumento AS descripcion,")
            sb.AppendLine("    ISNULL(im.clave,'') AS clave,")
            sb.AppendLine("    ISNULL(im.impacto,0) AS impacto,")
            sb.AppendLine("    ISNULL(im.probabilidad,0) AS ocurrencia,")
            sb.AppendLine("    ISNULL(im.nivel_riesgo_pld,0.00) AS nivel_riesgo_pld,")
            sb.AppendLine("    ISNULL(im.activo,0) AS estatus,")
            sb.AppendLine("    im.fecha_creacion")
            sb.AppendLine("  FROM dbo.catalogo_instrumento_monetario im")
            sb.AppendLine(")")
            sb.AppendLine("SELECT * FROM base WHERE 1=1")

            Dim hasSearch As Boolean = Not String.IsNullOrEmpty(searchValue)
            If hasSearch Then
                sb.AppendLine("  AND (descripcion LIKE @q OR clave LIKE @q)")
            End If

            ' Conteo filtrado
            Using cmdFiltered As New SqlCommand(sb.ToString().Replace("SELECT * FROM base", "SELECT COUNT(*) FROM base"), cn)
                If hasSearch Then
                    cmdFiltered.Parameters.AddWithValue("@q", "%" & searchValue & "%")
                End If
                totalFiltered = Convert.ToInt32(cmdFiltered.ExecuteScalar())
            End Using

            ' Paginado
            sb.AppendLine("ORDER BY id")
            sb.AppendLine("OFFSET @start ROWS FETCH NEXT @length ROWS ONLY;")

            Using cmd As New SqlCommand(sb.ToString(), cn)
                If hasSearch Then
                    cmd.Parameters.AddWithValue("@q", "%" & searchValue & "%")
                End If
                cmd.Parameters.AddWithValue("@start", start)
                cmd.Parameters.AddWithValue("@length", length)

                Using rd As SqlDataReader = cmd.ExecuteReader()
                    While rd.Read()
                        data.Add(New With {
                            .id = rd("id"),
                            .descripcion = rd("descripcion"),
                            .clave = rd("clave"),
                            .impacto = rd("impacto"),
                            .ocurrencia = rd("ocurrencia"),
                            .nivel_riesgo_pld = Convert.ToDecimal(rd("nivel_riesgo_pld")).ToString("0.00", CultureInfo.InvariantCulture),
                            .estatus = Convert.ToInt32(rd("estatus")),
                            .fecha_creacion = Convert.ToDateTime(rd("fecha_creacion")).ToString("yyyy-MM-dd HH:mm:ss")
                        })
                    End While
                End Using
            End Using
        End Using

        Dim payload = New With {
            .ok = True,
            .draw = draw,
            .recordsTotal = totalRecords,
            .recordsFiltered = If(String.IsNullOrEmpty(searchValue), totalRecords, totalFiltered),
            .data = data
        }
        WriteJson(context, payload)
    End Sub

    ' ---------------------------
    '  OBTENER (por id)
    ' ---------------------------
    Private Sub Obtener(ByVal context As HttpContext)
        Dim id As Integer = SafeInt(context.Request("id"))
        If id <= 0 Then
            WriteJson(context, New With {.ok = False, .message = "Id inválido."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "
                    SELECT id, instrumento AS descripcion, clave, 
                           ISNULL(impacto,0) AS impacto,
                           ISNULL(probabilidad,0) AS probabilidad,
                           ISNULL(nivel_riesgo_pld,0.00) AS nivel_riesgo_pld,
                           ISNULL(activo,0) AS activo
                    FROM dbo.catalogo_instrumento_monetario
                    WHERE id = @id;
                "
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rd As SqlDataReader = cmd.ExecuteReader()
                    If rd.Read() Then
                        Dim item = New With {
                            .id = rd("id"),
                            .descripcion = rd("descripcion"),
                            .clave = If(IsDBNull(rd("clave")), "", rd("clave").ToString()),
                            .impacto = Convert.ToInt32(rd("impacto")),
                            .probabilidad = Convert.ToInt32(rd("probabilidad")),
                            .nivel_riesgo_pld = Convert.ToDecimal(rd("nivel_riesgo_pld")).ToString("0.00", CultureInfo.InvariantCulture),
                            .activo = Convert.ToInt32(rd("activo"))
                        }
                        WriteJson(context, New With {.ok = True, .data = item})
                        Return
                    End If
                End Using
            End Using
        End Using

        WriteJson(context, New With {.ok = False, .message = "Registro no encontrado."})
    End Sub

    ' ---------------------------
    '  GUARDAR (INSERT)
    ' ---------------------------
    Private Sub Guardar(ByVal context As HttpContext)
        Dim descripcion As String = (context.Request("descripcion") & "").Trim()
        Dim clave As String = (context.Request("clave") & "").Trim()

        Dim impacto As Integer = SafeInt(context.Request("impacto"))
        Dim probabilidad As Integer = SafeInt(context.Request("probabilidad"))
        Dim nivelRiesgo As Decimal = SafeDecimal(context.Request("nivel_riesgo_pld"))

        Dim activo As Integer = If((context.Request("activo") & "").Trim() = "1", 1, 0)

        If String.IsNullOrEmpty(descripcion) Then
            WriteJson(context, New With {.ok = False, .message = "El campo Instrumento (descripción) es obligatorio."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()

            ' Evitar duplicado por nombre
            Using cmdChk As New SqlCommand("SELECT COUNT(*) FROM dbo.catalogo_instrumento_monetario WHERE instrumento = @desc", cn)
                cmdChk.Parameters.AddWithValue("@desc", descripcion)
                Dim exists As Integer = Convert.ToInt32(cmdChk.ExecuteScalar())
                If exists > 0 Then
                    WriteJson(context, New With {.ok = False, .message = "Ya existe un instrumento con ese nombre."})
                    Return
                End If
            End Using

            Dim sql As String = "
                    INSERT INTO dbo.catalogo_instrumento_monetario
                    (instrumento, activo, fecha_creacion, clave, impacto, probabilidad, nivel_riesgo_pld)
                    VALUES
                    (@instrumento, @activo, GETDATE(), @clave, @impacto, @probabilidad, @nivel_riesgo_pld);
                    SELECT SCOPE_IDENTITY();
                "

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@instrumento", descripcion)
                cmd.Parameters.AddWithValue("@activo", activo)
                cmd.Parameters.AddWithValue("@clave", If(String.IsNullOrEmpty(clave), CType(DBNull.Value, Object), clave))
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.Add("@nivel_riesgo_pld", SqlDbType.Decimal).Value = nivelRiesgo
                Dim newId As Integer = Convert.ToInt32(Math.Truncate(Convert.ToDecimal(cmd.ExecuteScalar())))
                WriteJson(context, New With {.ok = True, .message = "Registro guardado correctamente.", .id = newId})
            End Using
        End Using
    End Sub

    ' ---------------------------
    '  ACTUALIZAR (UPDATE)
    ' ---------------------------
    Private Sub Actualizar(ByVal context As HttpContext)
        Dim id As Integer = SafeInt(context.Request("id"))
        If id <= 0 Then
            WriteJson(context, New With {.ok = False, .message = "Id inválido."})
            Return
        End If

        Dim descripcion As String = (context.Request("descripcion") & "").Trim()
        Dim clave As String = (context.Request("clave") & "").Trim()
        Dim impacto As Integer = SafeInt(context.Request("impacto"))
        Dim probabilidad As Integer = SafeInt(context.Request("probabilidad"))
        Dim nivelRiesgo As Decimal = SafeDecimal(context.Request("nivel_riesgo_pld"))
        Dim activo As Integer = If((context.Request("activo") & "").Trim() = "1", 1, 0)

        If String.IsNullOrEmpty(descripcion) Then
            WriteJson(context, New With {.ok = False, .message = "El campo Instrumento (descripción) es obligatorio."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()

            ' Evitar duplicado (otro id)
            Using cmdChk As New SqlCommand("
                    SELECT COUNT(*) 
                    FROM dbo.catalogo_instrumento_monetario 
                    WHERE instrumento = @desc AND id <> @id;
                ", cn)
                cmdChk.Parameters.AddWithValue("@desc", descripcion)
                cmdChk.Parameters.AddWithValue("@id", id)
                Dim exists As Integer = Convert.ToInt32(cmdChk.ExecuteScalar())
                If exists > 0 Then
                    WriteJson(context, New With {.ok = False, .message = "Ya existe otro instrumento con ese nombre."})
                    Return
                End If
            End Using

            Dim sql As String = "
                    UPDATE dbo.catalogo_instrumento_monetario
                    SET instrumento = @instrumento,
                        clave = @clave,
                        impacto = @impacto,
                        probabilidad = @probabilidad,
                        nivel_riesgo_pld = @nivel_riesgo_pld,
                        activo = @activo
                    WHERE id = @id;
                "

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cmd.Parameters.AddWithValue("@instrumento", descripcion)
                cmd.Parameters.AddWithValue("@clave", If(String.IsNullOrEmpty(clave), CType(DBNull.Value, Object), clave))
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.Add("@nivel_riesgo_pld", SqlDbType.Decimal).Value = nivelRiesgo
                cmd.Parameters.AddWithValue("@activo", activo)

                Dim rows As Integer = cmd.ExecuteNonQuery()
                WriteJson(context, New With {.ok = (rows > 0), .message = If(rows > 0, "Registro actualizado correctamente.", "No se actualizó ningún registro.")})
            End Using
        End Using
    End Sub

    ' ---------------------------
    '  TOGGLE (activo)
    ' ---------------------------
    Private Sub ToggleActivo(ByVal context As HttpContext)
        Dim id As Integer = SafeInt(context.Request("id"))
        If id <= 0 Then
            WriteJson(context, New With {.ok = False, .message = "Id inválido."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()

            Dim sql As String = "
                    UPDATE dbo.catalogo_instrumento_monetario
                    SET activo = CASE WHEN ISNULL(activo,0) = 1 THEN 0 ELSE 1 END
                    WHERE id = @id;

                    SELECT ISNULL(activo,0) AS activo 
                    FROM dbo.catalogo_instrumento_monetario WHERE id = @id;
                "

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Dim nuevoEstatus As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                WriteJson(context, New With {.ok = True, .message = "Estatus actualizado.", .activo = nuevoEstatus})
            End Using
        End Using
    End Sub

    ' =========================
    ' Helpers
    ' =========================
    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue
        Dim json As String = js.Serialize(obj)
        ctx.Response.Write(json)
    End Sub

    Private Function SafeInt(value As String, Optional def As Integer = 0) As Integer
        Dim n As Integer
        If Integer.TryParse((value & "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            Return n
        End If
        Return def
    End Function

    Private Function SafeDecimal(value As String, Optional def As Decimal = 0D) As Decimal
        Dim s As String = (value & "").Trim().Replace(",", ".")
        Dim d As Decimal
        If Decimal.TryParse(s, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, d) Then
            Return Math.Round(d, 2, MidpointRounding.AwayFromZero)
        End If
        Return def
    End Function

    Private Function Sanitize(msg As String) As String
        If String.IsNullOrEmpty(msg) Then Return ""
        Return msg.Replace(vbCr, " ").Replace(vbLf, " ").Replace("""", "'")
    End Function

End Class

