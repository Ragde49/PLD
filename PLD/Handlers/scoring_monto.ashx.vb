Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class scoring_monto : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim jss As New JavaScriptSerializer()
        Dim op As String = context.Request("op")

        Select Case op
            Case "consulta"
                Dim lista As New List(Of Object)
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand("SELECT id, etiqueta, valor FROM scoring_porcentaje_monto ORDER BY id", conn)
                    Dim rd = cmd.ExecuteReader()
                    While rd.Read()
                        lista.Add(New With {
                          .id = rd("id"),
                          .etiqueta = rd("etiqueta"),
                          .valor = rd("valor")
                        })
                    End While
                End Using
                context.Response.Write(jss.Serialize(lista))

            Case "guardar"
                Dim body As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
                Dim data = jss.Deserialize(Of Dictionary(Of String, String))(body)
                Dim id = CInt(data("id"))
                Dim campo = data("campo")
                Dim valor = data("valor")

                If campo <> "valor" Then
                    context.Response.Write(jss.Serialize(New With {.ok = False, .msg = "Campo inválido"}))
                    Return
                End If

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand("UPDATE scoring_porcentaje_monto SET valor = @valor, fecha_actualizacion = GETDATE() WHERE id = @id", conn)
                    cmd.Parameters.AddWithValue("@valor", valor)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.ExecuteNonQuery()
                End Using

                context.Response.Write(jss.Serialize(New With {.ok = True}))
        End Select
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
