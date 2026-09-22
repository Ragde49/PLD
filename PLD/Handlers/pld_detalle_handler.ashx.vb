Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Web.Script.Serialization

Public Class pld_detalle_handler : Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ctx As HttpContext) Implements IHttpHandler.ProcessRequest
        ctx.Response.ContentType = "application/json; charset=utf-8"
        Try
            Dim action As String = (If(ctx.Request("action"), "")).Trim().ToLowerInvariant()
            Select Case action
                Case "listar" : HandleListar(ctx)
                Case "resumen" : HandleResumen(ctx)
                Case "ping" : WriteJson(ctx, New With {.ok = True, .message = "pong"})
                Case Else
                    WriteError(ctx, "Acción no soportada. Usa: listar | resumen | ping")
            End Select
        Catch ex As Exception
            WriteError(ctx, "Excepción no controlada: " & ex.Message)
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '=================== Helpers ===================
    Private Function GetConn() As SqlConnection
        Return New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
    End Function

    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String)
        ctx.Response.StatusCode = 200
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    Private Function ToInt(value As String) As Integer?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim n As Integer
        If Integer.TryParse(value.Trim(), Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            Return n
        End If
        Return Nothing
    End Function

    Private Sub ReadPaging(ctx As HttpContext, ByRef page As Integer, ByRef pageSize As Integer, Optional defaultPage As Integer = 1, Optional defaultSize As Integer = 50)
        page = defaultPage : pageSize = defaultSize
        Integer.TryParse(If(ctx.Request("page"), defaultPage.ToString()), page)
        Integer.TryParse(If(ctx.Request("pageSize"), defaultSize.ToString()), pageSize)
        If page < 1 Then page = 1
        If pageSize < 1 Then pageSize = defaultSize
    End Sub

    '=================== Acciones ===================

    ' LISTAR: solicitud_id (req), page/pageSize (opt), sort (opt: id|origen|impacto|prob|nivel), dir (asc|desc)
    Private Sub HandleListar(ctx As HttpContext)
        Dim sid = ToInt(ctx.Request("solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "solicitud_id es requerido.") : Exit Sub
        End If

        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim offset As Integer = (page - 1) * pageSize

        Dim sort As String = (If(ctx.Request("sort"), "id")).Trim().ToLowerInvariant()
        Dim dir As String = (If(ctx.Request("dir"), "asc")).Trim().ToLowerInvariant()
        Dim orderBy As String = "id"
        Select Case sort
            Case "id" : orderBy = "id"
            Case "origen" : orderBy = "origen_tipo"
            Case "impacto" : orderBy = "impacto"
            Case "prob" : orderBy = "probabilidad"
            Case "nivel" : orderBy = "nivel_riesgo_pld"
        End Select
        Dim orderDir As String = If(dir = "desc", "DESC", "ASC")

        Using con = GetConn()
            con.Open()

            Dim total As Integer
            Using c As New SqlCommand("SELECT COUNT(1) FROM dbo.solicitud_pld_detalle WHERE solicitud_id = @sid;", con)
                c.Parameters.Add("@sid", SqlDbType.Int).Value = sid.Value
                total = Convert.ToInt32(c.ExecuteScalar())
            End Using

            Dim sql As String = "
                SELECT id, solicitud_id, origen_tipo, referencia_id, factor_codigo,
                       impacto, probabilidad, nivel_riesgo_pld, peso, contribucion,
                       creado_por, fecha_creacion
                FROM dbo.solicitud_pld_detalle
                WHERE solicitud_id = @sid
                ORDER BY " & orderBy & " " & orderDir & "
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"

            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@sid", SqlDbType.Int).Value = sid.Value
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .solicitud_id = Convert.ToInt32(rd("solicitud_id")),
                            .origen_tipo = Convert.ToString(rd("origen_tipo")),
                            .referencia_id = If(IsDBNull(rd("referencia_id")), CType(Nothing, Integer?), Convert.ToInt32(rd("referencia_id"))),
                            .factor_codigo = If(IsDBNull(rd("factor_codigo")), Nothing, Convert.ToString(rd("factor_codigo"))),
                            .impacto = If(IsDBNull(rd("impacto")), CType(Nothing, Integer?), Convert.ToInt32(rd("impacto"))),
                            .probabilidad = If(IsDBNull(rd("probabilidad")), CType(Nothing, Integer?), Convert.ToInt32(rd("probabilidad"))),
                            .nivel = If(IsDBNull(rd("nivel_riesgo_pld")), CType(Nothing, Decimal?), Convert.ToDecimal(rd("nivel_riesgo_pld"), CultureInfo.InvariantCulture)),
                            .peso = If(IsDBNull(rd("peso")), CType(Nothing, Decimal?), Convert.ToDecimal(rd("peso"), CultureInfo.InvariantCulture)),
                            .contribucion = If(IsDBNull(rd("contribucion")), CType(Nothing, Decimal?), Convert.ToDecimal(rd("contribucion"), CultureInfo.InvariantCulture)),
                            .creado_por = If(IsDBNull(rd("creado_por")), Nothing, Convert.ToString(rd("creado_por"))),
                            .fecha_creacion = rd("fecha_creacion")
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' RESUMEN: solicitud_id (req)
    Private Sub HandleResumen(ctx As HttpContext)
        Dim sid = ToInt(ctx.Request("solicitud_id"))
        If Not sid.HasValue Then
            WriteError(ctx, "solicitud_id es requerido.") : Exit Sub
        End If

        Using con = GetConn()
            con.Open()
            Dim sql As String = "
                SELECT
                    COUNT(1)                              AS factores,
                    ISNULL(SUM(nivel_riesgo_pld),0)       AS suma_nivel,
                    CASE WHEN COUNT(1)>0 THEN CAST(AVG(CAST(nivel_riesgo_pld AS DECIMAL(18,6))) AS DECIMAL(18,6)) ELSE 0 END AS promedio_nivel,
                    MAX(fecha_creacion)                   AS ultima_captura
                FROM dbo.solicitud_pld_detalle
                WHERE solicitud_id = @sid;"
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@sid", SqlDbType.Int).Value = sid.Value
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        WriteJson(ctx, New With {
                            .ok = True,
                            .solicitud_id = sid.Value,
                            .factores = Convert.ToInt32(rd("factores")),
                            .suma_nivel = Convert.ToDecimal(rd("suma_nivel"), CultureInfo.InvariantCulture),
                            .promedio_nivel = Convert.ToDecimal(rd("promedio_nivel"), CultureInfo.InvariantCulture),
                            .ultima_captura = If(rd.IsDBNull(rd.GetOrdinal("ultima_captura")), Nothing, rd("ultima_captura"))
                        })
                    Else
                        WriteJson(ctx, New With {.ok = True, .solicitud_id = sid.Value, .factores = 0, .suma_nivel = 0, .promedio_nivel = 0, .ultima_captura = Nothing})
                    End If
                End Using
            End Using
        End Using
    End Sub

End Class
