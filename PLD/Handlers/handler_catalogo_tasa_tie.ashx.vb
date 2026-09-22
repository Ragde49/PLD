Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Globalization

Public Class handler_catalogo_tasa_tie
    Implements System.Web.IHttpHandler

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

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = System.Text.Encoding.UTF8

        Dim op As String = If(context.Request("op"), String.Empty).ToLowerInvariant()

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
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    WriteJson(context, New With {.success = False, .message = "Operación no reconocida."})
            End Select
        Catch ex As Exception
            ' Loguear si tienen bitacora (a futuro). Por ahora devolver error.
            context.Response.StatusCode = 500
            WriteJson(context, New With {.success = False, .message = ex.Message})
        End Try
    End Sub

    Private Sub Consultar(ByVal context As HttpContext)
        Dim lista As New List(Of Object)

        Using cn As New SqlConnection(ConnectionString)
            Dim sql As String = "SELECT id, tasa, fecha, estatus FROM dbo.catalogo_tasa_tie ORDER BY fecha DESC, id DESC"
            Using cmd As New SqlCommand(sql, cn)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        lista.Add(New With {
                            .id = Convert.ToInt32(rdr("id")),
                            .tasa = If(IsDBNull(rdr("tasa")), Nothing, Convert.ToDecimal(rdr("tasa"))),
                            .fecha = If(IsDBNull(rdr("fecha")), Nothing, Convert.ToDateTime(rdr("fecha")).ToString("yyyy-MM-dd")),
                            .estatus = If(IsDBNull(rdr("estatus")), False, Convert.ToBoolean(rdr("estatus")))
                        })
                    End While
                End Using
            End Using
        End Using

        WriteJson(context, New With {.success = True, .data = lista})
    End Sub

    Private Sub Obtener(ByVal context As HttpContext)
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            WriteJson(context, New With {.success = False, .message = "Id inválido."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            Dim sql As String = "SELECT id, tasa, fecha, estatus, fecha_creacion FROM dbo.catalogo_tasa_tie WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim item = New With {
                            .id = Convert.ToInt32(rdr("id")),
                            .tasa = If(IsDBNull(rdr("tasa")), Nothing, Convert.ToDecimal(rdr("tasa"))),
                            .fecha = If(IsDBNull(rdr("fecha")), Nothing, Convert.ToDateTime(rdr("fecha")).ToString("yyyy-MM-dd")),
                            .estatus = If(IsDBNull(rdr("estatus")), False, Convert.ToBoolean(rdr("estatus"))),
                            .fecha_creacion = If(IsDBNull(rdr("fecha_creacion")), Nothing, Convert.ToDateTime(rdr("fecha_creacion")).ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                        WriteJson(context, New With {.success = True, .data = item})
                        Return
                    Else
                        WriteJson(context, New With {.success = False, .message = "Registro no encontrado."})
                        Return
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(ByVal context As HttpContext)
        ' Espera: tasa, fecha, estatus (opcional)
        Dim tasaStr As String = context.Request("tasa")
        Dim fechaStr As String = context.Request("fecha")
        Dim estatusStr As String = context.Request("estatus")

        Dim tasa As Decimal
        Dim fecha As DateTime
        Dim estatus As Boolean = True

        If String.IsNullOrWhiteSpace(tasaStr) OrElse Not Decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, tasa) Then
            WriteJson(context, New With {.success = False, .message = "Tasa inválida. Asegúrese de enviar un número (ej. 5.00)."})
            Return
        End If

        If String.IsNullOrWhiteSpace(fechaStr) OrElse Not DateTime.TryParse(fechaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha) Then
            WriteJson(context, New With {.success = False, .message = "Fecha inválida. Formato esperado: yyyy-MM-dd."})
            Return
        End If

        If Not String.IsNullOrWhiteSpace(estatusStr) Then
            Boolean.TryParse(estatusStr, estatus)
        End If

        Using cn As New SqlConnection(ConnectionString)
            Dim sql As String = "INSERT INTO dbo.catalogo_tasa_tie (tasa, fecha, estatus) VALUES (@tasa, @fecha, @estatus); SELECT SCOPE_IDENTITY() AS newId;"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                cmd.Parameters("@tasa").Precision = 5
                cmd.Parameters("@tasa").Scale = 2
                cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = fecha.Date
                cmd.Parameters.Add("@estatus", SqlDbType.Bit).Value = If(estatus, 1, 0)

                cn.Open()
                Dim newIdObj = cmd.ExecuteScalar()
                Dim newId As Integer = If(newIdObj Is Nothing OrElse IsDBNull(newIdObj), 0, Convert.ToInt32(newIdObj))
                WriteJson(context, New With {.success = True, .message = "Registro guardado.", .id = newId})
            End Using
        End Using
    End Sub

    Private Sub Actualizar(ByVal context As HttpContext)
        ' Espera: id, tasa, fecha, estatus
        Dim idStr As String = context.Request("id")
        Dim tasaStr As String = context.Request("tasa")
        Dim fechaStr As String = context.Request("fecha")
        Dim estatusStr As String = context.Request("estatus")

        Dim id As Integer
        Dim tasa As Decimal
        Dim fecha As DateTime
        Dim estatus As Boolean = True

        If Not Integer.TryParse(idStr, id) Then
            WriteJson(context, New With {.success = False, .message = "Id inválido."})
            Return
        End If

        If String.IsNullOrWhiteSpace(tasaStr) OrElse Not Decimal.TryParse(tasaStr, NumberStyles.Any, CultureInfo.InvariantCulture, tasa) Then
            WriteJson(context, New With {.success = False, .message = "Tasa inválida."})
            Return
        End If

        If String.IsNullOrWhiteSpace(fechaStr) OrElse Not DateTime.TryParse(fechaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, fecha) Then
            WriteJson(context, New With {.success = False, .message = "Fecha inválida."})
            Return
        End If

        If Not String.IsNullOrWhiteSpace(estatusStr) Then
            Boolean.TryParse(estatusStr, estatus)
        End If

        Using cn As New SqlConnection(ConnectionString)
            Dim sql As String = "UPDATE dbo.catalogo_tasa_tie SET tasa = @tasa, fecha = @fecha, estatus = @estatus WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = tasa
                cmd.Parameters("@tasa").Precision = 5
                cmd.Parameters("@tasa").Scale = 2
                cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = fecha.Date
                cmd.Parameters.Add("@estatus", SqlDbType.Bit).Value = If(estatus, 1, 0)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id

                cn.Open()
                Dim affected As Integer = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Registro actualizado."})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró registro para actualizar."})
                End If
            End Using
        End Using
    End Sub

    Private Sub Eliminar(ByVal context As HttpContext)
        ' Se eliminará físicamente. Si prefieres soft-delete, lo cambiamos a UPDATE estatus = 0.
        Dim idStr As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idStr, id) Then
            WriteJson(context, New With {.success = False, .message = "Id inválido."})
            Return
        End If

        Using cn As New SqlConnection(ConnectionString)
            Dim sql As String = "DELETE FROM dbo.catalogo_tasa_tie WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cn.Open()
                Dim affected As Integer = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    WriteJson(context, New With {.success = True, .message = "Registro eliminado."})
                Else
                    WriteJson(context, New With {.success = False, .message = "No se encontró registro para eliminar."})
                End If
            End Using
        End Using
    End Sub

    Private Sub WriteJson(ByVal context As HttpContext, ByVal obj As Object)
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Int32.MaxValue
        Dim json As String = js.Serialize(obj)
        context.Response.Write(json)
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
