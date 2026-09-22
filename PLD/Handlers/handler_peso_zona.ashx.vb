Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_zona
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim op As String = context.Request("op")

        Try
            Select Case op
                Case "consultar"
                    Dim datos As New List(Of Dictionary(Of String, Object))()
                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("
                        SELECT zona_geografica,
                            SUM(CASE WHEN nivel = 'ALTO' THEN valor ELSE 0 END) AS puntaje_alto,
                            SUM(CASE WHEN nivel = 'MEDIO' THEN valor ELSE 0 END) AS puntaje_medio,
                            SUM(CASE WHEN nivel = 'BAJO' THEN valor ELSE 0 END) AS puntaje_bajo
                        FROM config_peso_zona
                        GROUP BY zona_geografica
                        ORDER BY zona_geografica", conn)

                        Dim reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            datos.Add(New Dictionary(Of String, Object) From {
                                {"categoria", reader("zona_geografica").ToString()},
                                {"puntaje_alto", Convert.ToDecimal(reader("puntaje_alto"))},
                                {"puntaje_medio", Convert.ToDecimal(reader("puntaje_medio"))},
                                {"puntaje_bajo", Convert.ToDecimal(reader("puntaje_bajo"))}
                            })
                        End While

                    End Using
                    context.Response.Write(js.Serialize(New With {.success = True, .data = datos}))

                Case "actualizar"
                    Try
                        Dim zona As String = context.Request("zona")
                        Dim nivel As String = context.Request("nivel")
                        Dim valor As Decimal = Convert.ToDecimal(context.Request("valor"))

                        Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                            conn.Open()

                            Dim updateCmd As New SqlCommand("UPDATE config_peso_zona SET valor = @valor, fecha_actualizacion = GETDATE() WHERE zona_geografica = @zona AND nivel = @nivel", conn)
                            updateCmd.Parameters.AddWithValue("@zona", zona)
                            updateCmd.Parameters.AddWithValue("@nivel", nivel)
                            updateCmd.Parameters.AddWithValue("@valor", valor)
                            updateCmd.ExecuteNonQuery()
                        End Using

                        context.Response.Write(js.Serialize(New With {.success = True, .mensaje = "Actualizado correctamente"}))
                    Catch ex As Exception
                        context.Response.Write(js.Serialize(New With {.success = False, .mensaje = "Error: " & ex.Message}))
                    End Try


                Case Else
                    context.Response.Write(js.Serialize(New With {.success = False, .mensaje = "Operación inválida"}))
            End Select
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
