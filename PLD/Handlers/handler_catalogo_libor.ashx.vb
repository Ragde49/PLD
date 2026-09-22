Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Globalization

Public Class handler_catalogo_libor
    Implements System.Web.IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"

        Dim op As String = If(context.Request("op"), String.Empty).ToString().ToLowerInvariant().Trim()

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
                    Toggle(context)
                Case Else
                    RespuestaError(context, "Operación no reconocida. Parámetro 'op' esperado: consultar|obtener|guardar|actualizar|toggle")
            End Select
        Catch ex As Exception
            ' Aquí podrías registrar el error en tu log/auditoría si tienes uno
            RespuestaError(context, "Error interno del servidor: " & ex.Message)
        End Try
    End Sub

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

    Private Sub Consultar(ByVal context As HttpContext)
        Dim lista As New List(Of Object)
        Dim sql As String = "SELECT id, tasa_interes, puntos_adicionales, valor_tasa_interes, fecha, estatus, fecha_creacion FROM dbo.catalogo_libor ORDER BY fecha DESC, id DESC"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        lista.Add(New With {
                            Key .id = Convert.ToInt32(rdr("id")),
                            Key .tasa_interes = If(IsDBNull(rdr("tasa_interes")), 0D, Convert.ToDecimal(rdr("tasa_interes"))),
                            Key .puntos_adicionales = If(IsDBNull(rdr("puntos_adicionales")), 0D, Convert.ToDecimal(rdr("puntos_adicionales"))),
                            Key .valor_tasa_interes = If(IsDBNull(rdr("valor_tasa_interes")), 0D, Convert.ToDecimal(rdr("valor_tasa_interes"))),
                            Key .fecha = If(IsDBNull(rdr("fecha")), Nothing, Convert.ToDateTime(rdr("fecha")).ToString("yyyy-MM-dd")),
                            Key .estatus = If(IsDBNull(rdr("estatus")), False, Convert.ToBoolean(rdr("estatus"))),
                            Key .fecha_creacion = If(IsDBNull(rdr("fecha_creacion")), Nothing, Convert.ToDateTime(rdr("fecha_creacion")).ToString("yyyy-MM-dd HH:mm:ss"))
                        })
                    End While
                End Using
            End Using
        End Using

        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(New With {Key .success = True, Key .data = lista}))
    End Sub

    Private Sub Obtener(ByVal context As HttpContext)
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            RespuestaError(context, "ID inválido para 'obtener'.")
            Return
        End If

        Dim sql As String = "SELECT id, tasa_interes, puntos_adicionales, valor_tasa_interes, fecha, estatus, fecha_creacion FROM dbo.catalogo_libor WHERE id = @id"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim item = New With {
                            Key .id = Convert.ToInt32(rdr("id")),
                            Key .tasa_interes = If(IsDBNull(rdr("tasa_interes")), 0D, Convert.ToDecimal(rdr("tasa_interes"))),
                            Key .puntos_adicionales = If(IsDBNull(rdr("puntos_adicionales")), 0D, Convert.ToDecimal(rdr("puntos_adicionales"))),
                            Key .valor_tasa_interes = If(IsDBNull(rdr("valor_tasa_interes")), 0D, Convert.ToDecimal(rdr("valor_tasa_interes"))),
                            Key .fecha = If(IsDBNull(rdr("fecha")), Nothing, Convert.ToDateTime(rdr("fecha")).ToString("yyyy-MM-dd")),
                            Key .estatus = If(IsDBNull(rdr("estatus")), False, Convert.ToBoolean(rdr("estatus"))),
                            Key .fecha_creacion = If(IsDBNull(rdr("fecha_creacion")), Nothing, Convert.ToDateTime(rdr("fecha_creacion")).ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                        Dim js As New JavaScriptSerializer()
                        context.Response.Write(js.Serialize(New With {Key .success = True, Key .data = item}))
                    Else
                        RespuestaError(context, "Registro no encontrado.")
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(ByVal context As HttpContext)
        ' Parámetros esperados (POST): tasa_interes, puntos_adicionales, valor_tasa_interes, fecha (yyyy-MM-dd), estatus (0/1)
        Dim tasa As Decimal = ParseDecimal(context.Request("tasa_interes"))
        Dim puntos As Decimal = ParseDecimal(context.Request("puntos_adicionales"))
        Dim valor As Decimal = ParseDecimal(context.Request("valor_tasa_interes"))
        Dim fechaStr As String = context.Request("fecha")
        Dim estatusVal As Integer = If(String.IsNullOrEmpty(context.Request("estatus")), 1, If(context.Request("estatus") = "1" Or context.Request("estatus").ToLower() = "true", 1, 0))

        If String.IsNullOrEmpty(fechaStr) Then
            RespuestaError(context, "La fecha es requerida.")
            Return
        End If

        Dim fecha As DateTime
        If Not DateTime.TryParseExact(fechaStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, fecha) Then
            ' Intentar parseo flexible
            If Not DateTime.TryParse(fechaStr, fecha) Then
                RespuestaError(context, "Fecha inválida. Formato esperado: yyyy-MM-dd")
                Return
            End If
        End If

        Dim sql As String = "INSERT INTO dbo.catalogo_libor (tasa_interes, puntos_adicionales, valor_tasa_interes, fecha, estatus, fecha_creacion) " &
                            "VALUES (@tasa, @puntos, @valor, @fecha, @estatus, GETDATE()); SELECT SCOPE_IDENTITY();"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@tasa", tasa)
                cmd.Parameters.AddWithValue("@puntos", puntos)
                cmd.Parameters.AddWithValue("@valor", valor)
                cmd.Parameters.AddWithValue("@fecha", fecha)
                cmd.Parameters.AddWithValue("@estatus", estatusVal)
                cn.Open()
                Dim newIdObj = cmd.ExecuteScalar()
                Dim newId As Integer = If(newIdObj IsNot Nothing, Convert.ToInt32(newIdObj), 0)
                Dim js As New JavaScriptSerializer()
                context.Response.Write(js.Serialize(New With {Key .success = True, Key .id = newId, Key .message = "Registro guardado correctamente."}))
            End Using
        End Using
    End Sub

    Private Sub Actualizar(ByVal context As HttpContext)
        ' Parámetros esperados (POST): id, tasa_interes, puntos_adicionales, valor_tasa_interes, fecha (yyyy-MM-dd), estatus (0/1)
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            RespuestaError(context, "ID inválido para 'actualizar'.")
            Return
        End If

        Dim tasa As Decimal = ParseDecimal(context.Request("tasa_interes"))
        Dim puntos As Decimal = ParseDecimal(context.Request("puntos_adicionales"))
        Dim valor As Decimal = ParseDecimal(context.Request("valor_tasa_interes"))
        Dim fechaStr As String = context.Request("fecha")
        Dim estatusVal As Integer = If(String.IsNullOrEmpty(context.Request("estatus")), 1, If(context.Request("estatus") = "1" Or context.Request("estatus").ToLower() = "true", 1, 0))

        If String.IsNullOrEmpty(fechaStr) Then
            RespuestaError(context, "La fecha es requerida.")
            Return
        End If

        Dim fecha As DateTime
        If Not DateTime.TryParseExact(fechaStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, fecha) Then
            If Not DateTime.TryParse(fechaStr, fecha) Then
                RespuestaError(context, "Fecha inválida. Formato esperado: yyyy-MM-dd")
                Return
            End If
        End If

        Dim sql As String = "UPDATE dbo.catalogo_libor SET tasa_interes=@tasa, puntos_adicionales=@puntos, valor_tasa_interes=@valor, fecha=@fecha, estatus=@estatus WHERE id=@id"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@tasa", tasa)
                cmd.Parameters.AddWithValue("@puntos", puntos)
                cmd.Parameters.AddWithValue("@valor", valor)
                cmd.Parameters.AddWithValue("@fecha", fecha)
                cmd.Parameters.AddWithValue("@estatus", estatusVal)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows > 0 Then
                    Dim js As New JavaScriptSerializer()
                    context.Response.Write(js.Serialize(New With {Key .success = True, Key .message = "Registro actualizado."}))
                Else
                    RespuestaError(context, "No se actualizó el registro. Verifica el ID.")
                End If
            End Using
        End Using
    End Sub

    Private Sub Toggle(ByVal context As HttpContext)
        ' Cambia estatus 1->0 o 0->1
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            RespuestaError(context, "ID inválido para 'toggle'.")
            Return
        End If

        Dim sql As String = "UPDATE dbo.catalogo_libor SET estatus = CASE WHEN ISNULL(estatus,1)=1 THEN 0 ELSE 1 END WHERE id=@id"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows > 0 Then
                    Dim js As New JavaScriptSerializer()
                    context.Response.Write(js.Serialize(New With {Key .success = True, Key .message = "Estatus actualizado."}))
                Else
                    RespuestaError(context, "No se encontró el registro para cambiar estatus.")
                End If
            End Using
        End Using
    End Sub

    ' ----------------- Helpers -----------------
    Private Sub RespuestaError(ByVal context As HttpContext, ByVal mensaje As String)
        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(New With {Key .success = False, Key .message = mensaje}))
    End Sub

    Private Function ParseDecimal(ByVal input As String) As Decimal
        If String.IsNullOrWhiteSpace(input) Then Return 0D
        Dim s As String = input.Trim()
        ' Normalizar separadores decimales (coma -> punto)
        s = s.Replace(" ", String.Empty)
        s = s.Replace(",", ".")
        Dim result As Decimal = 0D
        Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, result)
        Return result
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
