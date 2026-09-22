Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization
Imports System.Globalization
Imports System.Collections.Generic

Public Class handler_catalogo_propietario_real
    Implements IHttpHandler

    Private ReadOnly Property ConnectionString As String
        Get
            Dim csName As String = "PLDConnection" ' <-- usa PLDConnection como acordamos
            If ConfigurationManager.ConnectionStrings(csName) IsNot Nothing Then
                Return ConfigurationManager.ConnectionStrings(csName).ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings.Count > 0 Then
                Return ConfigurationManager.ConnectionStrings(0).ConnectionString
            End If
            Return String.Empty
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue

        Try
            Dim op As String = If(context.Request("op"), String.Empty).ToLowerInvariant().Trim()

            Select Case op
                Case "consultar"
                    WriteResponse(context, js, Consultar())
                Case "obtener"
                    WriteResponse(context, js, Obtener(context))
                Case "guardar"
                    WriteResponse(context, js, Guardar(context))
                Case "actualizar"
                    WriteResponse(context, js, Actualizar(context))
                Case "eliminar"
                    WriteResponse(context, js, Eliminar(context))
                Case "recalcular"
                    WriteResponse(context, js, RecalcularNiveles())
                Case Else
                    WriteResponse(context, js, New With {.success = False, .message = "Operación no especificada o inválida.", .data = Nothing})
            End Select

        Catch ex As Exception
            Dim resp = New With {
                .success = False,
                .message = "Error en el servidor: " & ex.Message,
                .stack = ex.StackTrace
            }
            context.Response.Write(New JavaScriptSerializer().Serialize(resp))
        End Try
    End Sub

    Private Sub WriteResponse(context As HttpContext, js As JavaScriptSerializer, obj As Object)
        context.Response.Write(js.Serialize(obj))
    End Sub

    ' ----------------- OPERACIONES -----------------

    Private Function Consultar() As Object
        Dim list As New List(Of Object)()
        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "SELECT id, descripcion, ISNULL(impacto,0) AS impacto, ISNULL(probabilidad,0) AS probabilidad, ISNULL(nivel_riesgo_pld,0.00) AS nivel_riesgo_pld, ISNULL(activo,1) AS activo, fecha_creacion FROM dbo.catalogo_propietario_real ORDER BY id"
            Using cmd As New SqlCommand(sql, cn)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        list.Add(New With {
                            .id = Convert.ToInt32(rdr("id")),
                            .descripcion = rdr("descripcion").ToString(),
                            .impacto = Convert.ToInt32(rdr("impacto")),
                            .probabilidad = Convert.ToInt32(rdr("probabilidad")),
                            .nivel_riesgo_pld = Convert.ToDecimal(rdr("nivel_riesgo_pld")),
                            .activo = If(Convert.ToInt32(rdr("activo")) = 1, True, False),
                            .fecha_creacion = If(rdr.IsDBNull(rdr.GetOrdinal("fecha_creacion")), Nothing, rdr("fecha_creacion"))
                        })
                    End While
                End Using
            End Using
        End Using

        Return New With {.success = True, .message = "OK", .data = list}
    End Function

    Private Function Obtener(context As HttpContext) As Object
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            Return New With {.success = False, .message = "ID inválido.", .data = Nothing}
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "SELECT id, descripcion, ISNULL(impacto,0) AS impacto, ISNULL(probabilidad,0) AS probabilidad, ISNULL(nivel_riesgo_pld,0.00) AS nivel_riesgo_pld, ISNULL(activo,1) AS activo, fecha_creacion FROM dbo.catalogo_propietario_real WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim row = New With {
                            .id = Convert.ToInt32(rdr("id")),
                            .descripcion = rdr("descripcion").ToString(),
                            .impacto = Convert.ToInt32(rdr("impacto")),
                            .probabilidad = Convert.ToInt32(rdr("probabilidad")),
                            .nivel_riesgo_pld = Convert.ToDecimal(rdr("nivel_riesgo_pld")),
                            .activo = If(Convert.ToInt32(rdr("activo")) = 1, True, False),
                            .fecha_creacion = If(rdr.IsDBNull(rdr.GetOrdinal("fecha_creacion")), Nothing, rdr("fecha_creacion"))
                        }
                        Return New With {.success = True, .message = "OK", .data = row}
                    Else
                        Return New With {.success = False, .message = "No se encontró registro con id=" & id.ToString(), .data = Nothing}
                    End If
                End Using
            End Using
        End Using
    End Function

    Private Function Guardar(context As HttpContext) As Object
        Dim form = ReadForm(context)
        Dim descripcion As String = If(form("descripcion"), String.Empty).Trim()
        Dim impactoRaw As String = If(form("impacto"), "0").Trim()
        Dim probRaw As String = If(form("probabilidad"), "0").Trim()
        Dim nivelRaw As String = If(form("nivel_riesgo_pld"), String.Empty).Trim()
        Dim activoRaw As String = If(form("activo"), "1").Trim()

        If String.IsNullOrWhiteSpace(descripcion) Then
            Return New With {.success = False, .message = "Descripción es requerida.", .data = Nothing}
        End If

        Dim impacto As Integer = RiesgoTextoANumero_Int(impactoRaw)
        Dim probabilidad As Integer = RiesgoTextoANumero_Int(probRaw)
        Dim nivel As Decimal

        If String.IsNullOrWhiteSpace(nivelRaw) Then
            nivel = CalcularNivel(impacto, probabilidad)
        Else
            nivel = RiesgoTextoANumero_Decimal(nivelRaw)
        End If

        Dim activo As Integer = If(activoRaw = "1" Or String.Compare(activoRaw, "true", True) = 0, 1, 0)

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "INSERT INTO dbo.catalogo_propietario_real (descripcion, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) VALUES (@descripcion, @impacto, @probabilidad, @nivel, @activo, GETDATE()); SELECT SCOPE_IDENTITY();"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", Decimal.Round(nivel, 2))
                cmd.Parameters.AddWithValue("@activo", activo)

                Dim newIdObj = cmd.ExecuteScalar()
                Dim newId As Integer = 0
                If newIdObj IsNot Nothing Then Integer.TryParse(newIdObj.ToString(), newId)

                Return New With {.success = True, .message = "Registro guardado.", .data = New With {.id = newId}}
            End Using
        End Using
    End Function

    Private Function Actualizar(context As HttpContext) As Object
        Dim form = ReadForm(context)
        Dim idStr As String = If(form("id"), String.Empty).Trim()
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) OrElse id <= 0 Then
            Return New With {.success = False, .message = "ID inválido para actualizar.", .data = Nothing}
        End If

        Dim descripcion As String = If(form("descripcion"), String.Empty).Trim()
        If String.IsNullOrWhiteSpace(descripcion) Then
            Return New With {.success = False, .message = "Descripción es requerida.", .data = Nothing}
        End If

        Dim impactoRaw As String = If(form("impacto"), "0").Trim()
        Dim probRaw As String = If(form("probabilidad"), "0").Trim()
        Dim nivelRaw As String = If(form("nivel_riesgo_pld"), String.Empty).Trim()
        Dim activoRaw As String = If(form("activo"), "1").Trim()

        Dim impacto As Integer = RiesgoTextoANumero_Int(impactoRaw)
        Dim probabilidad As Integer = RiesgoTextoANumero_Int(probRaw)
        Dim nivel As Decimal
        If String.IsNullOrWhiteSpace(nivelRaw) Then
            nivel = CalcularNivel(impacto, probabilidad)
        Else
            nivel = RiesgoTextoANumero_Decimal(nivelRaw)
        End If

        Dim activo As Integer = If(activoRaw = "1" Or String.Compare(activoRaw, "true", True) = 0, 1, 0)

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "UPDATE dbo.catalogo_propietario_real SET descripcion = @descripcion, impacto = @impacto, probabilidad = @probabilidad, nivel_riesgo_pld = @nivel, activo = @activo WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", Decimal.Round(nivel, 2))
                cmd.Parameters.AddWithValue("@activo", activo)
                cmd.Parameters.AddWithValue("@id", id)

                Dim affected As Integer = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    Return New With {.success = True, .message = "Registro actualizado.", .data = New With {.id = id}}
                Else
                    Return New With {.success = False, .message = "No se encontró registro para actualizar.", .data = Nothing}
                End If
            End Using
        End Using
    End Function

    Private Function Eliminar(context As HttpContext) As Object
        Dim form = ReadForm(context)
        Dim idStr As String = If(form("id"), String.Empty).Trim()
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) OrElse id <= 0 Then
            Return New With {.success = False, .message = "ID inválido para eliminar.", .data = Nothing}
        End If

        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "UPDATE dbo.catalogo_propietario_real SET activo = 0 WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Dim affected As Integer = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    Return New With {.success = True, .message = "Registro desactivado.", .data = New With {.id = id}}
                Else
                    Return New With {.success = False, .message = "No se encontró registro para eliminar.", .data = Nothing}
                End If
            End Using
        End Using
    End Function

    Private Function RecalcularNiveles() As Object
        Using cn As New SqlConnection(ConnectionString)
            cn.Open()
            Dim sql As String = "UPDATE dbo.catalogo_propietario_real SET nivel_riesgo_pld = ROUND(CAST(ISNULL(impacto,0) AS DECIMAL(10,4)) * (CAST(ISNULL(probabilidad,0) AS DECIMAL(10,4)) / 100.0), 2)"
            Using cmd As New SqlCommand(sql, cn)
                Dim affected As Integer = cmd.ExecuteNonQuery()
                Return New With {.success = True, .message = "Niveles recalculados.", .rows_affected = affected}
            End Using
        End Using
    End Function

    ' ---------------- HELPERS --------------------

    Private Function ReadForm(context As HttpContext) As IDictionary(Of String, String)
        Dim dict As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Dim contentType As String = context.Request.ContentType
        If Not String.IsNullOrWhiteSpace(contentType) AndAlso contentType.ToLowerInvariant().Contains("application/json") Then
            Try
                context.Request.InputStream.Position = 0
                Using sr As New IO.StreamReader(context.Request.InputStream)
                    Dim jsonBody As String = sr.ReadToEnd()
                    If Not String.IsNullOrWhiteSpace(jsonBody) Then
                        Dim js As New JavaScriptSerializer()
                        Dim obj As Object = js.DeserializeObject(jsonBody)
                        If TypeOf obj Is IDictionary(Of String, Object) Then
                            For Each kvp As KeyValuePair(Of String, Object) In CType(obj, IDictionary(Of String, Object))
                                dict(kvp.Key) = If(kvp.Value, String.Empty).ToString()
                            Next
                        End If
                    End If
                End Using
            Catch
            End Try
        End If

        For Each key As String In context.Request.Form.Keys
            If Not dict.ContainsKey(key) Then dict(key) = context.Request.Form(key)
        Next
        For Each key As String In context.Request.QueryString.Keys
            If Not String.IsNullOrEmpty(key) AndAlso Not dict.ContainsKey(key) Then dict(key) = context.Request.QueryString(key)
        Next

        Return dict
    End Function

    Private Function RiesgoTextoANumero_Int(raw As String) As Integer
        Dim d As Decimal = RiesgoTextoANumero_Decimal(raw)
        Try
            Return Convert.ToInt32(Math.Floor(Math.Abs(d)))
        Catch
            Return 0
        End Try
    End Function

    Private Function RiesgoTextoANumero_Decimal(raw As String) As Decimal
        If raw Is Nothing Then Return 0D
        Dim v As String = raw.Trim()
        If String.IsNullOrWhiteSpace(v) Then Return 0D

        Select Case v.ToUpperInvariant()
            Case "BAJO" : Return 1D
            Case "MEDIO" : Return 2D
            Case "ALTO" : Return 3D
        End Select

        v = v.Replace(",", ".").Trim()
        Dim result As Decimal
        If Decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, result) Then
            Return result
        End If

        If Decimal.TryParse(v, result) Then Return result
        Return 0D
    End Function

    Private Function CalcularNivel(impacto As Integer, probabilidad As Integer) As Decimal
        Try
            Dim nivel As Decimal = Convert.ToDecimal(impacto) * (Convert.ToDecimal(probabilidad) / 100D)
            Return Decimal.Round(nivel, 2)
        Catch
            Return 0D
        End Try
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
