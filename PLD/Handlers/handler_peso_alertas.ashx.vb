Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_alertas
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim op As String = context.Request("op")

        Select Case op
            Case "consultar"
                Dim datos As New List(Of Dictionary(Of String, Object))()
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim cmd As New SqlCommand("SELECT * FROM config_peso_alertas ORDER BY alerta, nivel", conn)
                    Dim rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim row As New Dictionary(Of String, Object)
                        row("id") = rdr("id")
                        row("alerta") = rdr("alerta").ToString()
                        row("nivel") = rdr("nivel").ToString()
                        row("valor") = Convert.ToDecimal(rdr("valor"))
                        datos.Add(row)
                    End While
                End Using
                context.Response.Write(js.Serialize(New With {.success = True, .data = datos}))

            Case "guardar"
                Try
                    Dim id As String = context.Request("id")
                    Dim alerta As String = context.Request("alerta")
                    Dim nivel As String = context.Request("nivel")
                    Dim valor As Decimal = Convert.ToDecimal(context.Request("valor"))

                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()

                        If String.IsNullOrEmpty(id) OrElse id = "0" Then
                            Dim checkCmd As New SqlCommand("SELECT COUNT(*) FROM config_peso_alertas WHERE alerta = @alerta AND nivel = @nivel", conn)
                            checkCmd.Parameters.AddWithValue("@alerta", alerta)
                            checkCmd.Parameters.AddWithValue("@nivel", nivel)
                            Dim existe As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                            If existe > 0 Then
                                context.Response.Write(js.Serialize(New With {.success = False, .mensaje = "Ya existe esa combinación de alerta y nivel."}))
                                Return
                            End If

                            Dim insertCmd As New SqlCommand("INSERT INTO config_peso_alertas (alerta, nivel, valor) VALUES (@alerta, @nivel, @valor)", conn)
                            insertCmd.Parameters.AddWithValue("@alerta", alerta)
                            insertCmd.Parameters.AddWithValue("@nivel", nivel)
                            insertCmd.Parameters.AddWithValue("@valor", valor)
                            insertCmd.ExecuteNonQuery()
                        Else
                            Dim updateCmd As New SqlCommand("UPDATE config_peso_alertas SET valor = @valor WHERE id = @id", conn)
                            updateCmd.Parameters.AddWithValue("@id", id)
                            updateCmd.Parameters.AddWithValue("@valor", valor)
                            updateCmd.ExecuteNonQuery()
                        End If
                    End Using

                    context.Response.Write(js.Serialize(New With {.success = True, .mensaje = "Guardado correctamente"}))
                Catch ex As Exception
                    context.Response.Write(js.Serialize(New With {.success = False, .mensaje = "Error: " & ex.Message}))
                End Try

            Case Else
                context.Response.Write(js.Serialize(New With {.success = False, .mensaje = "Operación inválida"}))
        End Select
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
