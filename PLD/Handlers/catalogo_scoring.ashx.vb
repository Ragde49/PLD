Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_scoring : Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim jss = New JavaScriptSerializer()
        Dim respuesta As New Dictionary(Of String, Object)

        Try
            Dim accion = context.Request("accion")
            If String.IsNullOrEmpty(accion) Then
                Dim json As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                Dim data = jss.Deserialize(Of Dictionary(Of String, String))(json)
                If data.ContainsKey("accion") Then accion = data("accion")
            End If

            Select Case accion
                Case "lista"
                    Dim datos As New List(Of Dictionary(Of String, Object))()
                    Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("conn").ConnectionString)
                        con.Open()
                        Dim cmd As New SqlCommand("SELECT id, descripcion, puntaje_minimo, puntaje_maximo FROM catalogo_scoring WHERE activo = 1 ORDER BY descripcion", con)
                        Dim rdr = cmd.ExecuteReader()
                        While rdr.Read()
                            datos.Add(New Dictionary(Of String, Object) From {
                                {"id", rdr("id")},
                                {"descripcion", rdr("descripcion").ToString()},
                                {"puntaje_minimo", If(IsDBNull(rdr("puntaje_minimo")), Nothing, rdr("puntaje_minimo"))},
                                {"puntaje_maximo", If(IsDBNull(rdr("puntaje_maximo")), Nothing, rdr("puntaje_maximo"))}
                            })
                        End While
                    End Using
                    context.Response.Write(jss.Serialize(datos))
                    Return

                Case "guardar"
                    Dim json As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                    Dim data = jss.Deserialize(Of Dictionary(Of String, String))(json)
                    Dim id = If(data.ContainsKey("id"), data("id"), "").Trim()
                    Dim descripcion = data("descripcion").Trim()
                    Dim puntaje_minimo As Object = If(data.ContainsKey("puntaje_minimo") AndAlso IsNumeric(data("puntaje_minimo")), data("puntaje_minimo"), DBNull.Value)
                    Dim puntaje_maximo As Object = If(data.ContainsKey("puntaje_maximo") AndAlso IsNumeric(data("puntaje_maximo")), data("puntaje_maximo"), DBNull.Value)

                    Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("conn").ConnectionString)
                        con.Open()
                        If String.IsNullOrEmpty(id) Then
                            Dim cmd As New SqlCommand("INSERT INTO catalogo_scoring (descripcion, puntaje_minimo, puntaje_maximo, activo, fecha_creacion) VALUES (@descripcion, @min, @max, 1, GETDATE())", con)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@min", puntaje_minimo)
                            cmd.Parameters.AddWithValue("@max", puntaje_maximo)
                            cmd.ExecuteNonQuery()
                            respuesta("ok") = True
                            respuesta("mensaje") = "Registro agregado correctamente."
                        Else
                            Dim cmd As New SqlCommand("UPDATE catalogo_scoring SET descripcion = @descripcion, puntaje_minimo = @min, puntaje_maximo = @max WHERE id = @id", con)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@min", puntaje_minimo)
                            cmd.Parameters.AddWithValue("@max", puntaje_maximo)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                            respuesta("ok") = True
                            respuesta("mensaje") = "Registro actualizado correctamente."
                        End If
                    End Using

                Case "eliminar"
                    Dim json As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                    Dim data = jss.Deserialize(Of Dictionary(Of String, String))(json)
                    Dim id = data("id")

                    Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("conn").ConnectionString)
                        con.Open()
                        Dim cmd As New SqlCommand("UPDATE catalogo_scoring SET activo = 0 WHERE id = @id", con)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                        respuesta("ok") = True
                        respuesta("mensaje") = "Registro eliminado correctamente."
                    End Using

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = ex.Message
        End Try

        context.Response.Write(jss.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
