Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class scoring_circulo : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim jss = New JavaScriptSerializer()
        Dim op = context.Request("op")

        Select Case op
            Case "consulta"
                Dim lista = New List(Of Object)()
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand("SELECT * FROM scoring_circulo_credito", conn)
                    Dim rd = cmd.ExecuteReader()
                    While rd.Read()
                        lista.Add(New With {
                          .id = rd("id"),
                          .valor_inicial = rd("valor_inicial"),
                          .valor_final = rd("valor_final"),
                          .valor_cc = rd("valor_cc"),
                          .score_max = rd("score_max")
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

                Dim camposValidos = {"valor_inicial", "valor_final", "valor_cc", "score_max"}
                If Not camposValidos.Contains(campo) Then
                    context.Response.Write(jss.Serialize(New With {.ok = False, .msg = "Campo inválido"}))
                    Return
                End If

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand($"UPDATE scoring_circulo_credito SET {campo} = @valor, fecha_actualizacion = GETDATE() WHERE id = @id", conn)
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
