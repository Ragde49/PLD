Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_sucursales
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()

        Try
            Dim lista As New List(Of Object)()

            Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                conn.Open()
                Dim cmd As New SqlCommand("SELECT id, nombre FROM catalogo_sucursales WHERE activo = 1 ORDER BY nombre", conn)
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    lista.Add(New With {
                        .id = reader("id"),
                        .nombre = reader("nombre").ToString()
                    })
                End While
            End Using

            context.Response.Write(js.Serialize(lista))

        Catch ex As Exception
            context.Response.StatusCode = 500
            context.Response.Write(js.Serialize(New With {
                .ok = False,
                .mensaje = "Error: " & ex.Message
            }))
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
