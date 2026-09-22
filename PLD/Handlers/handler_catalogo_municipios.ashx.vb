Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization
Imports System.Globalization
Imports System.Text
Imports System.Collections.Generic
Imports System.Linq

Public Class handler_catalogo_municipios
    Implements IHttpHandler

    Private ReadOnly Property ConnectionString As String
        Get
            If ConfigurationManager.ConnectionStrings("PLDConnection") IsNot Nothing Then
                Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings.Count > 0 Then
                Return ConfigurationManager.ConnectionStrings(0).ConnectionString
            Else
                Return String.Empty
            End If
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"

        Try
            ' Obtener operación (segura, sin usar ??)
            Dim op As String = String.Empty
            If context.Request("op") IsNot Nothing Then
                op = context.Request("op").ToString().Trim().ToLowerInvariant()
            End If

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
                    ToggleEstatus(context)
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    WriteJson(context, New With {.success = False, .message = "Operación no especificada. Use op=consultar|obtener|guardar|actualizar|toggle|eliminar"})
            End Select

        Catch ex As Exception
            WriteJson(context, New With {.success = False, .message = "Error del servidor: " & ex.Message})
        End Try
    End Sub

    ' =========================
    ' Consultar - devuelve lista
    ' =========================
    Private Sub Consultar(context As HttpContext)
        Dim search As String = String.Empty
        Dim claveEstado As String = String.Empty
        If context.Request("search") IsNot Nothing Then search = context.Request("search").ToString().Trim()
        If context.Request("clave_estado") IsNot Nothing Then claveEstado = context.Request("clave_estado").ToString().Trim()

        Dim sql As String = "SELECT id, descripcion, clave_municipio, clave_estado, estado, impacto, probabilidad, nivel_riesgo_pld, estatus, mitigantes, fecha_creacion FROM dbo.catalogo_municipios WHERE 1=1"
        Dim paramList As New List(Of SqlParameter)()

        If Not String.IsNullOrEmpty(claveEstado) Then
            sql &= " AND clave_estado = @clave_estado"
            paramList.Add(New SqlParameter("@clave_estado", claveEstado))
        End If

        If Not String.IsNullOrEmpty(search) Then
            sql &= " AND (descripcion LIKE @search OR clave_municipio LIKE @search OR estado LIKE @search)"
            paramList.Add(New SqlParameter("@search", "%" & search & "%"))
        End If

        sql &= " ORDER BY clave_estado, clave_municipio"

        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                If paramList.Count > 0 Then cmd.Parameters.AddRange(paramList.ToArray())
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        ' Convertir filas a lista simple de objetos (evitamos sintaxis LINQ potencialmente problemática)
        Dim list As New List(Of Object)()
        For Each r As DataRow In dt.Rows
            Dim obj = New With {
                .id = Convert.ToInt32(r("id")),
                .descripcion = If(r.IsNull("descripcion"), String.Empty, r("descripcion").ToString()),
                .clave_municipio = If(r.IsNull("clave_municipio"), String.Empty, r("clave_municipio").ToString()),
                .clave_estado = If(r.IsNull("clave_estado"), String.Empty, r("clave_estado").ToString()),
                .estado = If(r.IsNull("estado"), String.Empty, r("estado").ToString()),
                .impacto = If(r.IsNull("impacto"), 0, Convert.ToInt32(r("impacto"))),
                .probabilidad = If(r.IsNull("probabilidad"), 0, Convert.ToInt32(r("probabilidad"))),
                .nivel_riesgo_pld = If(r.IsNull("nivel_riesgo_pld"), 0D, Convert.ToDecimal(r("nivel_riesgo_pld"))),
                .estatus = If(r.IsNull("estatus"), False, Convert.ToBoolean(r("estatus"))),
                .mitigantes = If(r.IsNull("mitigantes"), 0, Convert.ToInt32(r("mitigantes"))),
                .fecha_creacion = If(r.IsNull("fecha_creacion"), Nothing, r("fecha_creacion"))
            }
            list.Add(obj)
        Next

        WriteJson(context, New With {.success = True, .data = list})
    End Sub

    ' =========================
    ' Obtener
    ' =========================
    Private Sub Obtener(context As HttpContext)
        Dim id As Integer = 0
        If context.Request("id") IsNot Nothing Then Integer.TryParse(context.Request("id").ToString(), id)
        If id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "Id inválido"})
            Return
        End If

        Dim sql As String = "SELECT id, descripcion, clave_municipio, clave_estado, estado, impacto, probabilidad, nivel_riesgo_pld, estatus, mitigantes, fecha_creacion FROM dbo.catalogo_municipios WHERE id = @id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim row = New With {
                            .id = rdr.GetInt32(rdr.GetOrdinal("id")),
                            .descripcion = If(rdr.IsDBNull(rdr.GetOrdinal("descripcion")), String.Empty, rdr("descripcion").ToString()),
                            .clave_municipio = If(rdr.IsDBNull(rdr.GetOrdinal("clave_municipio")), String.Empty, rdr("clave_municipio").ToString()),
                            .clave_estado = If(rdr.IsDBNull(rdr.GetOrdinal("clave_estado")), String.Empty, rdr("clave_estado").ToString()),
                            .estado = If(rdr.IsDBNull(rdr.GetOrdinal("estado")), String.Empty, rdr("estado").ToString()),
                            .impacto = If(rdr.IsDBNull(rdr.GetOrdinal("impacto")), 0, Convert.ToInt32(rdr("impacto"))),
                            .probabilidad = If(rdr.IsDBNull(rdr.GetOrdinal("probabilidad")), 0, Convert.ToInt32(rdr("probabilidad"))),
                            .nivel_riesgo_pld = If(rdr.IsDBNull(rdr.GetOrdinal("nivel_riesgo_pld")), 0D, Convert.ToDecimal(rdr("nivel_riesgo_pld"))),
                            .estatus = If(rdr.IsDBNull(rdr.GetOrdinal("estatus")), False, Convert.ToBoolean(rdr("estatus"))),
                            .mitigantes = If(rdr.IsDBNull(rdr.GetOrdinal("mitigantes")), 0, Convert.ToInt32(rdr("mitigantes"))),
                            .fecha_creacion = If(rdr.IsDBNull(rdr.GetOrdinal("fecha_creacion")), Nothing, rdr("fecha_creacion"))
                        }
                        WriteJson(context, New With {.success = True, .data = row})
                        Return
                    Else
                        WriteJson(context, New With {.success = False, .message = "Registro no encontrado"})
                        Return
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' =========================
    ' Guardar (Alta)
    ' =========================
    Private Sub Guardar(context As HttpContext)
        Dim payload = ReadRequestBody(context)
        Dim descripcion As String = payloadString(payload, "descripcion")
        Dim clave_municipio As String = payloadString(payload, "clave_municipio")
        Dim clave_estado As String = payloadString(payload, "clave_estado")
        Dim estado As String = payloadString(payload, "estado")
        Dim impacto As Integer = 0
        Dim probabilidad As Integer = 0
        Dim nivel As Decimal = 0D
        Dim estatus As Boolean = True
        Dim mitigantes As Integer = 0

        If String.IsNullOrWhiteSpace(descripcion) Then
            WriteJson(context, New With {.success = False, .message = "descripcion requerida"})
            Return
        End If

        Integer.TryParse(payloadString(payload, "impacto"), impacto)
        Integer.TryParse(payloadString(payload, "probabilidad"), probabilidad)
        Dim nivelStr As String = payloadString(payload, "nivel_riesgo_pld")
        If Not String.IsNullOrWhiteSpace(nivelStr) Then
            Decimal.TryParse(nivelStr.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, nivel)
        End If

        Dim estStr As String = payloadString(payload, "estatus")
        If Not String.IsNullOrWhiteSpace(estStr) Then
            estatus = (estStr = "1" OrElse estStr.ToLowerInvariant() = "true")
        End If

        Integer.TryParse(payloadString(payload, "mitigantes"), mitigantes)

        Dim sql As String = "INSERT INTO dbo.catalogo_municipios (descripcion, clave_municipio, clave_estado, estado, impacto, probabilidad, nivel_riesgo_pld, estatus, mitigantes, fecha_creacion) " &
                            "VALUES (@descripcion, @clave_municipio, @clave_estado, @estado, @impacto, @probabilidad, @nivel, @estatus, @mitigantes, GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS INT) AS newId;"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                If String.IsNullOrWhiteSpace(clave_municipio) Then
                    cmd.Parameters.AddWithValue("@clave_municipio", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_municipio", clave_municipio)
                End If
                If String.IsNullOrWhiteSpace(clave_estado) Then
                    cmd.Parameters.AddWithValue("@clave_estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_estado", clave_estado)
                End If
                If String.IsNullOrWhiteSpace(estado) Then
                    cmd.Parameters.AddWithValue("@estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@estado", estado)
                End If
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@estatus", If(estatus, 1, 0))
                cmd.Parameters.AddWithValue("@mitigantes", mitigantes)
                cn.Open()
                Dim newIdObj = cmd.ExecuteScalar()
                Dim newId As Integer = If(newIdObj IsNot Nothing, Convert.ToInt32(newIdObj), 0)
                WriteJson(context, New With {.success = True, .message = "Registro creado", .id = newId})
            End Using
        End Using
    End Sub

    ' =========================
    ' Actualizar
    ' =========================
    Private Sub Actualizar(context As HttpContext)
        Dim payload = ReadRequestBody(context)
        Dim id As Integer = 0
        Integer.TryParse(payloadString(payload, "id"), id)
        If id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "Id inválido para actualizar"})
            Return
        End If

        Dim descripcion As String = payloadString(payload, "descripcion")
        Dim clave_municipio As String = payloadString(payload, "clave_municipio")
        Dim clave_estado As String = payloadString(payload, "clave_estado")
        Dim estado As String = payloadString(payload, "estado")
        Dim impacto As Integer = 0
        Dim probabilidad As Integer = 0
        Dim nivel As Decimal = 0D
        Dim estatus As Boolean = True
        Dim mitigantes As Integer = 0

        Integer.TryParse(payloadString(payload, "impacto"), impacto)
        Integer.TryParse(payloadString(payload, "probabilidad"), probabilidad)
        Dim nivelStr As String = payloadString(payload, "nivel_riesgo_pld")
        If Not String.IsNullOrWhiteSpace(nivelStr) Then
            Decimal.TryParse(nivelStr.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, nivel)
        End If

        Dim estStr As String = payloadString(payload, "estatus")
        If Not String.IsNullOrWhiteSpace(estStr) Then
            estatus = (estStr = "1" OrElse estStr.ToLowerInvariant() = "true")
        End If

        Integer.TryParse(payloadString(payload, "mitigantes"), mitigantes)

        Dim sql As String = "UPDATE dbo.catalogo_municipios SET descripcion=@descripcion, clave_municipio=@clave_municipio, clave_estado=@clave_estado, estado=@estado, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel, estatus=@estatus, mitigantes=@mitigantes WHERE id=@id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                If String.IsNullOrWhiteSpace(clave_municipio) Then
                    cmd.Parameters.AddWithValue("@clave_municipio", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_municipio", clave_municipio)
                End If
                If String.IsNullOrWhiteSpace(clave_estado) Then
                    cmd.Parameters.AddWithValue("@clave_estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_estado", clave_estado)
                End If
                If String.IsNullOrWhiteSpace(estado) Then
                    cmd.Parameters.AddWithValue("@estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@estado", estado)
                End If
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@estatus", If(estatus, 1, 0))
                cmd.Parameters.AddWithValue("@mitigantes", mitigantes)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim affected = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Registro actualizado"})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró registro para actualizar"})
                End If
            End Using
        End Using
    End Sub

    ' =========================
    ' Toggle estatus
    ' =========================
    Private Sub ToggleEstatus(context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(If(context.Request("id") IsNot Nothing, context.Request("id").ToString(), String.Empty), id)
        If id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "Id inválido"})
            Return
        End If

        Dim sql As String = "UPDATE dbo.catalogo_municipios SET estatus = CASE WHEN ISNULL(estatus,0)=1 THEN 0 ELSE 1 END WHERE id = @id; SELECT estatus FROM dbo.catalogo_municipios WHERE id = @id;"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim newValObj = cmd.ExecuteScalar()
                Dim newVal As Boolean = False
                If newValObj IsNot Nothing AndAlso Not Convert.IsDBNull(newValObj) Then
                    newVal = Convert.ToBoolean(newValObj)
                End If
                WriteJson(context, New With {.success = True, .message = "Estatus cambiado", .estatus = newVal})
            End Using
        End Using
    End Sub

    ' =========================
    ' Eliminar
    ' =========================
    Private Sub Eliminar(context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(If(context.Request("id") IsNot Nothing, context.Request("id").ToString(), String.Empty), id)
        If id <= 0 Then
            WriteJson(context, New With {.success = False, .message = "Id inválido"})
            Return
        End If

        Dim sql As String = "DELETE FROM dbo.catalogo_municipios WHERE id = @id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim affected = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Registro eliminado"})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró registro para eliminar"})
                End If
            End Using
        End Using
    End Sub

    ' =========================
    ' Helpers
    ' =========================
    Private Sub WriteJson(context As HttpContext, obj As Object)
        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(obj))
    End Sub

    Private Function ReadRequestBody(context As HttpContext) As Dictionary(Of String, Object)
        Dim dict As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
        Try
            If context.Request.HttpMethod = "POST" OrElse context.Request.HttpMethod = "PUT" Then
                If context.Request.Form IsNot Nothing AndAlso context.Request.Form.Count > 0 Then
                    For Each k As String In context.Request.Form.Keys
                        dict(k) = context.Request.Form(k)
                    Next
                    Return dict
                End If

                context.Request.InputStream.Position = 0
                Using sr As New IO.StreamReader(context.Request.InputStream, Encoding.UTF8)
                    Dim raw As String = sr.ReadToEnd().Trim()
                    If Not String.IsNullOrEmpty(raw) Then
                        Try
                            Dim js As New JavaScriptSerializer()
                            Dim obj = js.DeserializeObject(raw)
                            If TypeOf obj Is Dictionary(Of String, Object) Then
                                Return CType(obj, Dictionary(Of String, Object))
                            End If
                        Catch ex As Exception
                            ' ignorar, retornará dict vacío
                        End Try
                    End If
                End Using
            End If
        Catch ex As Exception
            ' ignorar error de lectura, retornará dict vacío
        End Try
        Return dict
    End Function

    Private Function payloadString(payload As Dictionary(Of String, Object), key As String) As String
        If payload Is Nothing Then Return String.Empty
        If payload.ContainsKey(key) Then
            Dim o = payload(key)
            If o Is Nothing Then Return String.Empty
            Return o.ToString().Trim()
        Else
            Return String.Empty
        End If
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
