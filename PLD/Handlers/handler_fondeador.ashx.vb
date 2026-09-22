Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_fondeador
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim accion As String = ""
        Dim input As Dictionary(Of String, Object) = Nothing

        Try
            If context.Request.HttpMethod = "GET" Then
                accion = context.Request("accion")
            ElseIf context.Request.HttpMethod = "POST" Then
                Dim body As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                If Not String.IsNullOrEmpty(body) Then
                    input = js.Deserialize(Of Dictionary(Of String, Object))(body)
                    If input.ContainsKey("accion") Then
                        accion = input("accion").ToString()
                    End If
                End If
            End If

            Select Case accion
                Case "lista"
                    Dim datos As New List(Of Object)()
                    Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("SELECT id, descripcion, ISNULL(observaciones, '') AS observaciones, ISNULL(monto, 0) AS monto, estatus FROM catalogo_fondeador ORDER BY id DESC", conn)
                        Dim reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            datos.Add(New With {
                                .id = reader("id"),
                                .descripcion = reader("descripcion"),
                                .observaciones = reader("observaciones"),
                                .monto = Convert.ToDecimal(reader("monto")),
                                .estatus = reader("estatus").ToString()
                            })
                        End While
                    End Using
                    context.Response.Write(js.Serialize(datos))

                Case "guardar"
                    Dim id = If(input("id") Is Nothing OrElse input("id").ToString() = "", 0, Convert.ToInt32(input("id")))
                    Dim descripcion = input("descripcion").ToString().Trim()
                    Dim observaciones = If(input.ContainsKey("observaciones"), input("observaciones").ToString(), "")
                    Dim monto = If(input.ContainsKey("monto"), Convert.ToDecimal(input("monto")), 0)
                    Dim estatus = If(input.ContainsKey("estatus"), input("estatus").ToString(), "Activo")
                    Dim tipo_credito_id = If(input.ContainsKey("tipo_credito_id") AndAlso input("tipo_credito_id") <> "", Convert.ToInt32(input("tipo_credito_id")), DBNull.Value)

                    Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim sql As String
                        If id = 0 Then
                            sql = "INSERT INTO catalogo_fondeador (descripcion, observaciones, monto, estatus, tipo_credito_id, fecha_creacion)
                   VALUES (@descripcion, @observaciones, @monto, @estatus, @tipo_credito_id, GETDATE())"
                        Else
                            sql = "UPDATE catalogo_fondeador
                   SET descripcion = @descripcion, observaciones = @observaciones, monto = @monto, estatus = @estatus,
                       tipo_credito_id = @tipo_credito_id
                   WHERE id = @id"
                        End If

                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@observaciones", observaciones)
                            cmd.Parameters.AddWithValue("@monto", monto)
                            cmd.Parameters.AddWithValue("@estatus", estatus)
                            If tipo_credito_id Is DBNull.Value Then
                                cmd.Parameters.AddWithValue("@tipo_credito_id", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue("@tipo_credito_id", tipo_credito_id)
                            End If
                            If id <> 0 Then cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    context.Response.Write(js.Serialize(New With {.ok = True, .mensaje = "Guardado correctamente"}))


                Case "eliminar"
                    Dim id = Convert.ToInt32(input("id"))

                    Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("DELETE FROM catalogo_fondeador WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                    End Using

                    context.Response.Write(js.Serialize(New With {.ok = True, .mensaje = "Eliminado correctamente"}))

                Case "detalle"
                    Dim id = Convert.ToInt32(context.Request("id"))
                    Dim result As Object = Nothing

                    Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("SELECT id, descripcion, ISNULL(observaciones,'') AS observaciones, ISNULL(monto,0) AS monto,
                                          estatus, tipo_credito_id
                                   FROM catalogo_fondeador WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@id", id)
                        Dim reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            result = New With {
                                .id = reader("id"),
                                .descripcion = reader("descripcion"),
                                .observaciones = reader("observaciones"),
                                .monto = Convert.ToDecimal(reader("monto")),
                                .estatus = reader("estatus").ToString(),
                                .tipo_credito_id = If(IsDBNull(reader("tipo_credito_id")), Nothing, Convert.ToInt32(reader("tipo_credito_id")))
                            }
                        End If
                    End Using

                    context.Response.Write(js.Serialize(result))


                Case Else
                    context.Response.Write(js.Serialize(New With {.ok = False, .mensaje = "Acción no reconocida"}))
            End Select

        Catch ex As Exception
            context.Response.StatusCode = 500
            context.Response.Write(js.Serialize(New With {
                .ok = False,
                .mensaje = "Error interno en handler: " & ex.Message
            }))
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
