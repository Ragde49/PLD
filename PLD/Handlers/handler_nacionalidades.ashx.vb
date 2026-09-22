Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_nacionalidades
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim respuesta As New Dictionary(Of String, Object)
        Dim op As String = context.Request("op")

        Try
            Select Case op
                Case "select"
                    Dim lista As New List(Of Dictionary(Of String, Object))
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("SELECT * FROM catalogo_nacionalidades WHERE activo = 1 ORDER BY pais", conn)
                        Dim reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim item As New Dictionary(Of String, Object)
                            For i As Integer = 0 To reader.FieldCount - 1
                                item.Add(reader.GetName(i), reader(i))
                            Next
                            lista.Add(item)
                        End While
                        respuesta("data") = lista
                        respuesta("success") = True
                    End Using

                Case "insert"
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("INSERT INTO catalogo_nacionalidades (pais, nacionalidad, clave, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) VALUES (@pais, @nacionalidad, @clave, @impacto, @probabilidad, @nivel, 1, GETDATE())", conn)
                        cmd.Parameters.AddWithValue("@pais", context.Request("pais"))
                        cmd.Parameters.AddWithValue("@nacionalidad", context.Request("nacionalidad"))
                        cmd.Parameters.AddWithValue("@clave", context.Request("clave"))
                        cmd.Parameters.AddWithValue("@impacto", Integer.Parse(context.Request("impacto")))
                        cmd.Parameters.AddWithValue("@probabilidad", Integer.Parse(context.Request("probabilidad")))
                        cmd.Parameters.AddWithValue("@nivel", Decimal.Parse(context.Request("nivel_riesgo_pld")))
                        cmd.ExecuteNonQuery()
                        respuesta("success") = True
                    End Using

                Case "update"
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("UPDATE catalogo_nacionalidades SET pais=@pais, nacionalidad=@nacionalidad, clave=@clave, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel WHERE id=@id", conn)
                        cmd.Parameters.AddWithValue("@pais", context.Request("pais"))
                        cmd.Parameters.AddWithValue("@nacionalidad", context.Request("nacionalidad"))
                        cmd.Parameters.AddWithValue("@clave", context.Request("clave"))
                        cmd.Parameters.AddWithValue("@impacto", Integer.Parse(context.Request("impacto")))
                        cmd.Parameters.AddWithValue("@probabilidad", Integer.Parse(context.Request("probabilidad")))
                        cmd.Parameters.AddWithValue("@nivel", Decimal.Parse(context.Request("nivel_riesgo_pld")))
                        cmd.Parameters.AddWithValue("@id", Integer.Parse(context.Request("id")))
                        cmd.ExecuteNonQuery()
                        respuesta("success") = True
                    End Using

                Case "delete"
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("UPDATE catalogo_nacionalidades SET activo = 0 WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@id", Integer.Parse(context.Request("id")))
                        cmd.ExecuteNonQuery()
                        respuesta("success") = True
                    End Using

                Case Else
                    respuesta("success") = False
                    respuesta("error") = "Operación no válida"
            End Select

        Catch ex As Exception
            respuesta("success") = False
            respuesta("error") = ex.Message
        End Try

        context.Response.Write(js.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
