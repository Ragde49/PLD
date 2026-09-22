Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_paises
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim serializer As New JavaScriptSerializer()
        Dim respuesta As New Dictionary(Of String, Object)
        Dim op As String = context.Request("op")
        Dim yaRespondido As Boolean = False

        Try
            Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                conn.Open()

                ' --------- COMBO PARA DROPDOWN ---------
                If op = "combo" Then
                    Dim lista As New List(Of Dictionary(Of String, Object))()
                    Dim sql As String = "SELECT pais FROM catalogo_paises WHERE activo = 1 ORDER BY pais ASC"
                    Using cmd As New SqlCommand(sql, conn)
                        Using reader As SqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                lista.Add(New Dictionary(Of String, Object) From {
                                    {"pais", reader("pais").ToString()}
                                })
                            End While
                        End Using
                    End Using
                    context.Response.Write(serializer.Serialize(lista))
                    yaRespondido = True

                    ' --------- LISTA PARA DATATABLE ---------
                ElseIf op = "lista" Then
                    Dim lista As New List(Of Dictionary(Of String, Object))()
                    Dim sql As String = "SELECT id, pais, iso, activo, fecha_creacion FROM catalogo_paises ORDER BY pais ASC"
                    Using cmd As New SqlCommand(sql, conn)
                        Using reader As SqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                lista.Add(New Dictionary(Of String, Object) From {
                                    {"id", reader("id")},
                                    {"pais", reader("pais").ToString()},
                                    {"iso", If(IsDBNull(reader("iso")), "", reader("iso").ToString())},
                                    {"activo", Convert.ToBoolean(reader("activo"))},
                                    {"fecha_creacion", Convert.ToDateTime(reader("fecha_creacion")).ToString("yyyy-MM-dd HH:mm:ss")}
                                })
                            End While
                        End Using
                    End Using
                    respuesta("ok") = True
                    respuesta("data") = lista

                    ' --------- INSERTAR ---------
                ElseIf op = "insertar" Then
                    Dim pais As String = context.Request("pais")
                    Dim iso As String = context.Request("iso")
                    Dim activo As Boolean = context.Request("activo") = "1"

                    If String.IsNullOrEmpty(pais) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El campo 'pais' es obligatorio."
                    Else
                        Dim sql As String = "INSERT INTO catalogo_paises (pais, iso, activo, fecha_creacion) VALUES (@pais, @iso, @activo, GETDATE())"
                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@pais", pais)
                            cmd.Parameters.AddWithValue("@iso", If(String.IsNullOrEmpty(iso), DBNull.Value, iso))
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("ok") = True
                        respuesta("mensaje") = "País insertado correctamente."
                    End If

                    ' --------- EDITAR ---------
                ElseIf op = "editar" Then
                    Dim id As Integer = Convert.ToInt32(context.Request("id"))
                    Dim pais As String = context.Request("pais")
                    Dim iso As String = context.Request("iso")
                    Dim activo As Boolean = context.Request("activo") = "1"

                    If id <= 0 Or String.IsNullOrEmpty(pais) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Datos incompletos para editar."
                    Else
                        Dim sql As String = "UPDATE catalogo_paises SET pais = @pais, iso = @iso, activo = @activo WHERE id = @id"
                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@pais", pais)
                            cmd.Parameters.AddWithValue("@iso", If(String.IsNullOrEmpty(iso), DBNull.Value, iso))
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("ok") = True
                        respuesta("mensaje") = "País actualizado correctamente."
                    End If

                    ' --------- ELIMINAR ---------
                ElseIf op = "eliminar" Then
                    Dim id As Integer = Convert.ToInt32(context.Request("id"))
                    If id <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "ID inválido para eliminar."
                    Else
                        Dim sql As String = "DELETE FROM catalogo_paises WHERE id = @id"
                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("ok") = True
                        respuesta("mensaje") = "País eliminado correctamente."
                    End If

                    ' --------- OPERACIÓN NO VÁLIDA ---------
                Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Operación no válida: " & op
                End If
            End Using

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error interno: " & ex.Message
        End Try

        If Not yaRespondido Then
            context.Response.Write(serializer.Serialize(respuesta))
        End If
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
