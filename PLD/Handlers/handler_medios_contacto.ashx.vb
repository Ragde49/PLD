Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_medios_contacto : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"

        Dim action = context.Request("action")
        Dim connStr As String = System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Dim json = New JavaScriptSerializer()

        Try
            Using conn As New SqlConnection(connStr)
                conn.Open()

                Select Case action
                    Case "listar"
                        Dim lista As New List(Of Dictionary(Of String, Object))
                        Dim cmd As New SqlCommand("SELECT * FROM catalogo_medios_contacto ORDER BY id ASC", conn)
                        Using rdr As SqlDataReader = cmd.ExecuteReader()
                            While rdr.Read()
                                Dim item As New Dictionary(Of String, Object) From {
                                    {"id", rdr("id")},
                                    {"forma_contacto", rdr("forma_contacto")},
                                    {"impacto", rdr("impacto")},
                                    {"probabilidad", rdr("probabilidad")},
                                    {"nivel_riesgo_pld", rdr("nivel_riesgo_pld")},
                                    {"activo", rdr("activo")}
                                }
                                lista.Add(item)
                            End While
                        End Using
                        context.Response.Write(json.Serialize(New With {.success = True, .data = lista}))

                    Case "guardar"
                        Dim id As String = context.Request("id")
                        Dim forma As String = context.Request("forma_contacto")
                        Dim impacto As String = context.Request("impacto")
                        Dim probabilidad As String = context.Request("probabilidad")
                        Dim riesgo As String = context.Request("nivel_riesgo_pld")
                        Dim activo As Boolean = context.Request("activo") = "true"

                        If id = "" Then
                            ' INSERT
                            Dim cmd As New SqlCommand("INSERT INTO catalogo_medios_contacto (forma_contacto, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) VALUES (@forma, @impacto, @probabilidad, @riesgo, @activo, GETDATE())", conn)
                            cmd.Parameters.AddWithValue("@forma", forma)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@riesgo", riesgo)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.ExecuteNonQuery()
                        Else
                            ' UPDATE
                            Dim cmd As New SqlCommand("UPDATE catalogo_medios_contacto SET forma_contacto = @forma, impacto = @impacto, probabilidad = @probabilidad, nivel_riesgo_pld = @riesgo, activo = @activo WHERE id = @id", conn)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.Parameters.AddWithValue("@forma", forma)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@riesgo", riesgo)
                            cmd.Parameters.AddWithValue("@activo", activo)
                            cmd.ExecuteNonQuery()
                        End If
                        context.Response.Write(json.Serialize(New With {.success = True}))

                    Case "eliminar"
                        Dim id As String = context.Request("id")
                        Dim cmd As New SqlCommand("UPDATE catalogo_medios_contacto SET activo = 0 WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                        context.Response.Write(json.Serialize(New With {.success = True}))

                    Case Else
                        context.Response.Write(json.Serialize(New With {.success = False, .error = "Acción no válida"}))
                End Select

                conn.Close()
            End Using
        Catch ex As Exception
            context.Response.Write(json.Serialize(New With {.success = False, .error = ex.Message}))
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
