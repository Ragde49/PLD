Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_ocupacion : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Dim response As New Dictionary(Of String, Object)
        Dim action As String = context.Request("action")

        Try
            Using conn As New SqlConnection(connStr)
                conn.Open()

                Select Case action
                    Case "listar"
                        Dim query As String = "SELECT * FROM catalogo_ocupacion ORDER BY descripcion"
                        Using cmd As New SqlCommand(query, conn)
                            Dim rdr As SqlDataReader = cmd.ExecuteReader()
                            Dim lista As New List(Of Dictionary(Of String, Object))()

                            While rdr.Read()
                                Dim item As New Dictionary(Of String, Object) From {
                                    {"id", rdr("id")},
                                    {"descripcion", rdr("descripcion")},
                                    {"clasificacion_riesgo", rdr("clasificacion_riesgo")},
                                    {"impacto", rdr("impacto")},
                                    {"probabilidad", rdr("probabilidad")},
                                    {"nivel_riesgo_pld", rdr("nivel_riesgo_pld")},
                                    {"activo", rdr("activo")}
                                }
                                lista.Add(item)
                            End While
                            response("data") = lista
                        End Using

                    Case "guardar"
                        Dim descripcion As String = context.Request("descripcion")
                        Dim clasificacion As String = context.Request("clasificacion_riesgo")
                        Dim impacto As Integer = Convert.ToInt32(context.Request("impacto"))
                        Dim probabilidad As Integer = Convert.ToInt32(context.Request("probabilidad"))
                        Dim nivelRiesgo As Decimal = Convert.ToDecimal(context.Request("nivel_riesgo_pld"))
                        Dim activo As Boolean = Convert.ToBoolean(context.Request("activo"))

                        Dim query As String = "INSERT INTO catalogo_ocupacion (descripcion, clasificacion_riesgo, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) VALUES (@descripcion, @clasificacion, @impacto, @probabilidad, @nivelRiesgo, @activo, GETDATE())"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@clasificacion", clasificacion)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@nivelRiesgo", nivelRiesgo)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.ExecuteNonQuery()
                        End Using
                        response("success") = True

                    Case "editar"
                        Dim id As Integer = Convert.ToInt32(context.Request("id"))
                        Dim descripcion As String = context.Request("descripcion")
                        Dim clasificacion As String = context.Request("clasificacion_riesgo")
                        Dim impacto As Integer = Convert.ToInt32(context.Request("impacto"))
                        Dim probabilidad As Integer = Convert.ToInt32(context.Request("probabilidad"))
                        Dim nivelRiesgo As Decimal = Convert.ToDecimal(context.Request("nivel_riesgo_pld"))
                        Dim activo As Boolean = Convert.ToBoolean(context.Request("activo"))

                        Dim query As String = "UPDATE catalogo_ocupacion SET descripcion=@descripcion, clasificacion_riesgo=@clasificacion, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivelRiesgo, activo=@activo WHERE id=@id"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@descripcion", descripcion)
                            cmd.Parameters.AddWithValue("@clasificacion", clasificacion)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@nivelRiesgo", nivelRiesgo)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        response("success") = True

                    Case "eliminar"
                        Dim id As Integer = Convert.ToInt32(context.Request("id"))
                        Dim query As String = "DELETE FROM catalogo_ocupacion WHERE id = @id"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        response("success") = True

                    Case Else
                        response("error") = "Acción no reconocida"
                End Select
            End Using

        Catch ex As Exception
            response("error") = ex.Message
        End Try

        Dim json As String = New JavaScriptSerializer().Serialize(response)
        context.Response.Write(json)
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
