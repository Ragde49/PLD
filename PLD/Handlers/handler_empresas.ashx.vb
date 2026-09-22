Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_empresas : Implements IHttpHandler

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
                        Dim query As String = "SELECT * FROM catalogo_empresas ORDER BY empresa"
                        Using cmd As New SqlCommand(query, conn)
                            Dim rdr As SqlDataReader = cmd.ExecuteReader()
                            Dim lista As New List(Of Dictionary(Of String, Object))()

                            While rdr.Read()
                                Dim item As New Dictionary(Of String, Object) From {
                                    {"id", rdr("id")},
                                    {"empresa", rdr("empresa")},
                                    {"codigo_postal", rdr("codigo_postal")},
                                    {"estado", rdr("estado")},
                                    {"municipio", rdr("municipio")},
                                    {"ciudad", rdr("ciudad")},
                                    {"colonia", rdr("colonia")},
                                    {"calle", rdr("calle")},
                                    {"numero", rdr("numero")},
                                    {"pais_domicilio_empresa", rdr("pais_domicilio_empresa")},
                                    {"telefono", rdr("telefono")},
                                    {"extension", rdr("extension")},
                                    {"impacto", rdr("impacto")},
                                    {"probabilidad", rdr("probabilidad")},
                                    {"nivel_riesgo_pld", rdr("nivel_riesgo_pld")},
                                    {"estatus", rdr("estatus")}
                                }
                                lista.Add(item)
                            End While
                            response("data") = lista
                        End Using

                    Case "guardar"
                        Dim empresa As String = context.Request("empresa")
                        Dim codigo_postal As String = context.Request("codigo_postal")
                        Dim estado As String = context.Request("estado")
                        Dim municipio As String = context.Request("municipio")
                        Dim ciudad As String = context.Request("ciudad")
                        Dim colonia As String = context.Request("colonia")
                        Dim calle As String = context.Request("calle")
                        Dim numero As String = context.Request("numero")
                        Dim pais As String = context.Request("pais_domicilio_empresa")
                        Dim telefono As String = context.Request("telefono")
                        Dim extension As String = context.Request("extension")
                        Dim impacto As Integer = Convert.ToInt32(context.Request("impacto"))
                        Dim probabilidad As Integer = Convert.ToInt32(context.Request("probabilidad"))
                        Dim nivelRiesgo As Decimal = Convert.ToDecimal(context.Request("nivel_riesgo_pld"))
                        Dim estatus As Boolean = Convert.ToBoolean(context.Request("estatus"))

                        Dim query As String = "INSERT INTO catalogo_empresas (empresa, codigo_postal, estado, municipio, ciudad, colonia, calle, numero, pais_domicilio_empresa, telefono, extension, impacto, probabilidad, nivel_riesgo_pld, estatus, fecha_creacion) VALUES (@empresa, @codigo_postal, @estado, @municipio, @ciudad, @colonia, @calle, @numero, @pais, @telefono, @extension, @impacto, @probabilidad, @nivelRiesgo, @estatus, GETDATE())"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@empresa", empresa)
                            cmd.Parameters.AddWithValue("@codigo_postal", codigo_postal)
                            cmd.Parameters.AddWithValue("@estado", estado)
                            cmd.Parameters.AddWithValue("@municipio", municipio)
                            cmd.Parameters.AddWithValue("@ciudad", ciudad)
                            cmd.Parameters.AddWithValue("@colonia", colonia)
                            cmd.Parameters.AddWithValue("@calle", calle)
                            cmd.Parameters.AddWithValue("@numero", numero)
                            cmd.Parameters.AddWithValue("@pais", pais)
                            cmd.Parameters.AddWithValue("@telefono", telefono)
                            cmd.Parameters.AddWithValue("@extension", extension)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@nivelRiesgo", nivelRiesgo)
                            cmd.Parameters.AddWithValue("@estatus", estatus)
                            cmd.ExecuteNonQuery()
                        End Using
                        response("success") = True

                    Case "editar"
                        Dim id As Integer = Convert.ToInt32(context.Request("id"))
                        Dim empresa As String = context.Request("empresa")
                        Dim codigo_postal As String = context.Request("codigo_postal")
                        Dim estado As String = context.Request("estado")
                        Dim municipio As String = context.Request("municipio")
                        Dim ciudad As String = context.Request("ciudad")
                        Dim colonia As String = context.Request("colonia")
                        Dim calle As String = context.Request("calle")
                        Dim numero As String = context.Request("numero")
                        Dim pais As String = context.Request("pais_domicilio_empresa")
                        Dim telefono As String = context.Request("telefono")
                        Dim extension As String = context.Request("extension")
                        Dim impacto As Integer = Convert.ToInt32(context.Request("impacto"))
                        Dim probabilidad As Integer = Convert.ToInt32(context.Request("probabilidad"))
                        Dim nivelRiesgo As Decimal = Convert.ToDecimal(context.Request("nivel_riesgo_pld"))
                        Dim estatus As Boolean = Convert.ToBoolean(context.Request("estatus"))

                        Dim query As String = "UPDATE catalogo_empresas SET empresa=@empresa, codigo_postal=@codigo_postal, estado=@estado, municipio=@municipio, ciudad=@ciudad, colonia=@colonia, calle=@calle, numero=@numero, pais_domicilio_empresa=@pais, telefono=@telefono, extension=@extension, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivelRiesgo, estatus=@estatus WHERE id=@id"
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@empresa", empresa)
                            cmd.Parameters.AddWithValue("@codigo_postal", codigo_postal)
                            cmd.Parameters.AddWithValue("@estado", estado)
                            cmd.Parameters.AddWithValue("@municipio", municipio)
                            cmd.Parameters.AddWithValue("@ciudad", ciudad)
                            cmd.Parameters.AddWithValue("@colonia", colonia)
                            cmd.Parameters.AddWithValue("@calle", calle)
                            cmd.Parameters.AddWithValue("@numero", numero)
                            cmd.Parameters.AddWithValue("@pais", pais)
                            cmd.Parameters.AddWithValue("@telefono", telefono)
                            cmd.Parameters.AddWithValue("@extension", extension)
                            cmd.Parameters.AddWithValue("@impacto", impacto)
                            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                            cmd.Parameters.AddWithValue("@nivelRiesgo", nivelRiesgo)
                            cmd.Parameters.AddWithValue("@estatus", estatus)
                            cmd.Parameters.AddWithValue("@id", id)
                            cmd.ExecuteNonQuery()
                        End Using
                        response("success") = True

                    Case "eliminar"
                        Dim id As Integer = Convert.ToInt32(context.Request("id"))
                        Dim query As String = "DELETE FROM catalogo_empresas WHERE id = @id"
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
