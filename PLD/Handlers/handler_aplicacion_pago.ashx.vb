Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_aplicacion_pago
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js = New JavaScriptSerializer()
        Dim respuesta = New Dictionary(Of String, Object)()

        Try
            Dim accion As String = context.Request("accion")

            Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                conn.Open()

                Select Case accion
                    Case "consultar"
                        Dim lista = New List(Of Dictionary(Of String, Object))()
                        Dim query = "SELECT * FROM catalogo_aplicacion_pago ORDER BY descripcion"
                        Using cmd As New SqlCommand(query, conn)
                            Using reader As SqlDataReader = cmd.ExecuteReader()
                                While reader.Read()
                                    Dim item = New Dictionary(Of String, Object) From {
                                        {"id", reader("id")},
                                        {"descripcion", reader("descripcion")},
                                        {"subtipo", reader("subtipo")},
                                        {"tipo_credito", reader("tipo_credito")},
                                        {"tipo_regimen", reader("tipo_regimen")},
                                        {"impacto", reader("impacto")},
                                        {"probabilidad", reader("probabilidad")},
                                        {"nivel_riesgo_pld", reader("nivel_riesgo_pld")},
                                        {"activo", reader("activo")},
                                        {"fecha_creacion", reader("fecha_creacion")}
                                    }
                                    lista.Add(item)
                                End While
                            End Using
                        End Using
                        respuesta("data") = lista
                        respuesta("success") = True

                    Case "guardar"
                        Dim input = ObtenerInput(context)
                        Dim insert = "INSERT INTO catalogo_aplicacion_pago (descripcion, subtipo, tipo_credito, tipo_regimen, impacto, probabilidad, nivel_riesgo_pld, activo)
                                      VALUES (@descripcion, @subtipo, @tipo_credito, @tipo_regimen, @impacto, @probabilidad, @nivel_riesgo_pld, @activo)"
                        Using cmd As New SqlCommand(insert, conn)
                            cmd.Parameters.AddWithValue("@descripcion", input("descripcion"))
                            cmd.Parameters.AddWithValue("@subtipo", input("subtipo"))
                            cmd.Parameters.AddWithValue("@tipo_credito", input("tipo_credito"))
                            cmd.Parameters.AddWithValue("@tipo_regimen", input("tipo_regimen"))
                            cmd.Parameters.AddWithValue("@impacto", input("impacto"))
                            cmd.Parameters.AddWithValue("@probabilidad", input("probabilidad"))
                            cmd.Parameters.AddWithValue("@nivel_riesgo_pld", input("nivel_riesgo_pld"))
                            cmd.Parameters.AddWithValue("@activo", input("activo"))
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("success") = True

                    Case "editar"
                        Dim input = ObtenerInput(context)
                        Dim update = "UPDATE catalogo_aplicacion_pago SET descripcion=@descripcion, subtipo=@subtipo, tipo_credito=@tipo_credito, tipo_regimen=@tipo_regimen,
                                      impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel_riesgo_pld, activo=@activo WHERE id=@id"
                        Using cmd As New SqlCommand(update, conn)
                            cmd.Parameters.AddWithValue("@id", input("id"))
                            cmd.Parameters.AddWithValue("@descripcion", input("descripcion"))
                            cmd.Parameters.AddWithValue("@subtipo", input("subtipo"))
                            cmd.Parameters.AddWithValue("@tipo_credito", input("tipo_credito"))
                            cmd.Parameters.AddWithValue("@tipo_regimen", input("tipo_regimen"))
                            cmd.Parameters.AddWithValue("@impacto", input("impacto"))
                            cmd.Parameters.AddWithValue("@probabilidad", input("probabilidad"))
                            cmd.Parameters.AddWithValue("@nivel_riesgo_pld", input("nivel_riesgo_pld"))
                            cmd.Parameters.AddWithValue("@activo", input("activo"))
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("success") = True

                    Case "eliminar"
                        Dim id As Integer = Integer.Parse(context.Request("id"))
                        Dim delete = "DELETE FROM catalogo_aplicacion_pago WHERE id=@id"
                        Using cmd As New SqlCommand(delete, conn)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        respuesta("success") = True

                    Case Else
                        respuesta("success") = False
                        respuesta("mensaje") = "Acción no válida"
                End Select
            End Using
        Catch ex As Exception
            respuesta("success") = False
            respuesta("mensaje") = "Error: " & ex.Message
        End Try

        context.Response.Write(js.Serialize(respuesta))
    End Sub

    Private Function ObtenerInput(context As HttpContext) As Dictionary(Of String, Object)
        Dim js As New JavaScriptSerializer()
        Dim body As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
        Return js.Deserialize(Of Dictionary(Of String, Object))(body)
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
