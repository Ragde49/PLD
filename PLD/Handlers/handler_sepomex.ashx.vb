Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_sepomex : Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim jss = New JavaScriptSerializer()
        Dim respuesta As New List(Of Dictionary(Of String, Object))()

        Try
            Dim accion = context.Request("accion")

            If accion = "lista" Then
                Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    con.Open()
                    Dim sql As String = "
                        SELECT TOP 500
                            cp, asentamiento, tipo_asentamiento,
                            municipio, estado, ciudad,
                            zona, clave_estado, clave_municipio
                        FROM catalogo_sepomex
                        ORDER BY cp
                    "
                    Dim cmd As New SqlCommand(sql, con)
                    Dim rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim item As New Dictionary(Of String, Object) From {
                            {"cp", rdr("cp")},
                            {"asentamiento", rdr("asentamiento")},
                            {"tipo_asentamiento", rdr("tipo_asentamiento")},
                            {"municipio", rdr("municipio")},
                            {"estado", rdr("estado")},
                            {"ciudad", rdr("ciudad")},
                            {"zona", rdr("zona")},
                            {"clave_estado", rdr("clave_estado")},
                            {"clave_municipio", rdr("clave_municipio")}
                        }
                        respuesta.Add(item)
                    End While
                End Using

            ElseIf accion = "buscarcp" Then
                Dim cp As String = context.Request("cp")

                Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    con.Open()
                    Dim sql As String = "
                        SELECT asentamiento, municipio, estado, ciudad
                        FROM catalogo_sepomex
                        WHERE cp = @cp
                        ORDER BY asentamiento
                    "
                    Dim cmd As New SqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@cp", cp)
                    Dim rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim item As New Dictionary(Of String, Object) From {
                            {"colonia", rdr("asentamiento")},
                            {"municipio", rdr("municipio")},
                            {"estado", rdr("estado")},
                            {"ciudad", rdr("ciudad")}
                        }
                        respuesta.Add(item)
                    End While
                End Using

            Else
                context.Response.Write("{""ok"":false,""mensaje"":""Acción no válida""}")
                Return
            End If

        Catch ex As Exception
            context.Response.Write("{""ok"":false,""mensaje"":""" & ex.Message.Replace("""", "'") & """}")
            Return
        End Try

        context.Response.Write(jss.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class