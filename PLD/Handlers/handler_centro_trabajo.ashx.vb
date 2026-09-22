Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_centro_trabajo : Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim serializer As New JavaScriptSerializer()
        Dim accion As String = ""
        Dim data As Dictionary(Of String, Object) = Nothing

        Try
            If context.Request.HttpMethod = "POST" Then
                Dim raw As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                data = serializer.Deserialize(Of Dictionary(Of String, Object))(raw)
                If data.ContainsKey("accion") Then
                    accion = data("accion").ToString()
                End If
            Else
                accion = context.Request("accion")
            End If

            Select Case accion

                Case "lista"
                    Dim resultado As New List(Of Object)
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("SELECT id, descripcion, activo FROM catalogo_centro_trabajo ORDER BY descripcion", conn)
                        Dim rdr As SqlDataReader = cmd.ExecuteReader()
                        While rdr.Read()
                            resultado.Add(New With {
                                .id = rdr("id"),
                                .descripcion = rdr("descripcion"),
                                .activo = If(Not IsDBNull(rdr("activo")) AndAlso Convert.ToBoolean(rdr("activo")), "Activo", "Inactivo")
                            })
                        End While
                    End Using
                    context.Response.Write(serializer.Serialize(resultado))

                Case "guardar"
                    Dim id = Convert.ToInt32(data("id"))
                    Dim descripcion = data("descripcion").ToString().Trim()
                    Dim activo = Convert.ToBoolean(data("activo"))

                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As SqlCommand

                        If id = 0 Then
                            cmd = New SqlCommand("INSERT INTO catalogo_centro_trabajo (descripcion, activo, fecha_creacion) VALUES (@descripcion, @activo, GETDATE())", conn)
                        Else
                            cmd = New SqlCommand("UPDATE catalogo_centro_trabajo SET descripcion=@descripcion, activo=@activo WHERE id=@id", conn)
                            cmd.Parameters.AddWithValue("@id", id)
                        End If

                        cmd.Parameters.AddWithValue("@descripcion", descripcion)
                        cmd.Parameters.AddWithValue("@activo", activo)
                        cmd.ExecuteNonQuery()
                    End Using

                    context.Response.Write("{""ok"":true, ""mensaje"":""Guardado correctamente""}")

                Case "eliminar"
                    Dim id = Convert.ToInt32(data("id"))
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("DELETE FROM catalogo_centro_trabajo WHERE id=@id", conn)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                    End Using
                    context.Response.Write("{""ok"":true, ""mensaje"":""Registro eliminado correctamente""}")

                Case Else
                    context.Response.Write("{""ok"":false, ""mensaje"":""Acción no válida""}")
            End Select

        Catch ex As Exception
            context.Response.Write("{""ok"":false, ""mensaje"":""" & ex.Message.Replace("""", "'") & """}")
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
