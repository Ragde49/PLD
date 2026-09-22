Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_catalogo_tipo_documentos
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim modo As String = context.Request("modo")
        Dim connStr As String = System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Dim serializer As New JavaScriptSerializer()

        Select Case modo

            Case "consultar"
                Dim lista As New List(Of Object)

                Try
                    Using conn As New SqlConnection(connStr)
                        conn.Open()
                        Dim query As String = "SELECT id, descripcion, activo FROM catalogo_tipo_documentos ORDER BY descripcion"
                        Using cmd As New SqlCommand(query, conn)
                            Using reader As SqlDataReader = cmd.ExecuteReader()
                                While reader.Read()
                                    lista.Add(New With {
                                        .id = reader("id"),
                                        .descripcion = reader("descripcion").ToString(),
                                        .activo = Convert.ToBoolean(reader("activo"))
                                    })
                                End While
                            End Using
                        End Using
                    End Using

                    context.Response.Write(serializer.Serialize(lista))
                Catch ex As Exception
                    context.Response.StatusCode = 500
                    context.Response.Write("{""ok"":false,""mensaje"":""Error al consultar los documentos""}")
                End Try

            Case "insertar"
                Dim descripcion As String = context.Request("descripcion")
                Dim activo As Boolean = (context.Request("activo") = "1")

                If String.IsNullOrEmpty(descripcion) Then
                    context.Response.Write("{""ok"":false,""mensaje"":""La descripción no puede estar vacía""}")
                    Return
                End If

                Try
                    Using conn As New SqlConnection(connStr)
                        conn.Open()
                        Dim query As String = "INSERT INTO catalogo_tipo_documentos (descripcion, activo, fecha_creacion) VALUES (@descripcion, @activo, GETDATE())"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    context.Response.Write("{""ok"":true}")
                Catch ex As Exception
                    context.Response.Write("{""ok"":false,""mensaje"":""Error al insertar el documento""}")
                End Try

            Case "actualizar"
                Dim id As String = context.Request("id")
                Dim descripcion As String = context.Request("descripcion")
                Dim activo As Boolean = (context.Request("activo") = "1")

                If String.IsNullOrEmpty(id) OrElse String.IsNullOrEmpty(descripcion) Then
                    context.Response.Write("{""ok"":false,""mensaje"":""Faltan datos para actualizar""}")
                    Return
                End If

                Try
                    Using conn As New SqlConnection(connStr)
                        conn.Open()
                        Dim query As String = "UPDATE catalogo_tipo_documentos SET descripcion = @descripcion, activo = @activo WHERE id = @id"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    context.Response.Write("{""ok"":true}")
                Catch ex As Exception
                    context.Response.Write("{""ok"":false,""mensaje"":""Error al actualizar el documento""}")
                End Try

            Case "eliminar"
                Dim id As String = context.Request("id")

                If String.IsNullOrEmpty(id) Then
                    context.Response.Write("{""ok"":false,""mensaje"":""ID no proporcionado""}")
                    Return
                End If

                Try
                    Using conn As New SqlConnection(connStr)
                        conn.Open()
                        Dim query As String = "DELETE FROM catalogo_tipo_documentos WHERE id = @id"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    context.Response.Write("{""ok"":true}")
                Catch ex As Exception
                    context.Response.Write("{""ok"":false,""mensaje"":""Error al eliminar el documento""}")
                End Try

            Case Else
                context.Response.Write("{""ok"":false,""mensaje"":""Modo inválido""}")
        End Select
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
