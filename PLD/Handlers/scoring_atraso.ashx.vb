Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class scoring_atraso : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim jss As New JavaScriptSerializer()
        Dim op As String = ""

        If context.Request.HttpMethod = "GET" Then
            op = context.Request("op")
        ElseIf context.Request.HttpMethod = "POST" Then
            Dim body As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
            Dim data = jss.Deserialize(Of Dictionary(Of String, String))(body)
            op = data("op")

            If op = "guardar" Then
                Dim id As Integer = CInt(data("id"))
                Dim campo As String = data("campo")
                Dim valor As String = data("valor")

                If campo <> "valor" Then
                    context.Response.Write(jss.Serialize(New With {.ok = False, .msg = "Campo inválido"}))
                    Return
                End If

                Try
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("UPDATE scoring_nivel_atraso SET valor = @valor, fecha_actualizacion = GETDATE() WHERE id = @id", conn)
                        cmd.Parameters.AddWithValue("@valor", valor)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                    End Using
                    context.Response.Write(jss.Serialize(New With {.ok = True}))
                Catch ex As Exception
                    context.Response.Write(jss.Serialize(New With {.ok = False, .msg = ex.Message}))
                End Try

                Return
            End If
        End If

        If op = "consulta" Then
            Dim lista As New List(Of Object)
            Try
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand("SELECT id, etiqueta, valor FROM scoring_nivel_atraso ORDER BY id", conn)
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
            Catch ex As Exception
                context.Response.Write(jss.Serialize(New With {.ok = False, .msg = ex.Message}))
            End Try
        End If
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
