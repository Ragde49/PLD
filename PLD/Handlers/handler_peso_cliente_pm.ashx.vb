Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_cliente_pm
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim op As String = context.Request("op")

        Select Case op

            Case "consultar"
                Dim resultado = New List(Of Dictionary(Of String, Object))()

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim sql As String = "
                        SELECT categoria,
                               MAX(CASE WHEN nivel = 'ALTO' THEN valor ELSE NULL END) AS puntaje_alto,
                               MAX(CASE WHEN nivel = 'MEDIO' THEN valor ELSE NULL END) AS puntaje_medio,
                               MAX(CASE WHEN nivel = 'BAJO' THEN valor ELSE NULL END) AS puntaje_bajo
                        FROM config_peso_cliente_pm
                        GROUP BY categoria
                        ORDER BY categoria
                    "

                    Using cmd As New SqlCommand(sql, conn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                resultado.Add(New Dictionary(Of String, Object) From {
                                    {"categoria", reader("categoria")},
                                    {"puntaje_alto", reader("puntaje_alto")},
                                    {"puntaje_medio", reader("puntaje_medio")},
                                    {"puntaje_bajo", reader("puntaje_bajo")}
                                })
                            End While
                        End Using
                    End Using
                End Using

                context.Response.Write(js.Serialize(New With {.success = True, .data = resultado}))

            Case "actualizar"
                Dim categoria = context.Request("categoria")
                Dim campo = context.Request("campo")
                Dim valor = context.Request("valor")

                Dim nivel As String = ""
                Select Case campo
                    Case "puntaje_alto" : nivel = "ALTO"
                    Case "puntaje_medio" : nivel = "MEDIO"
                    Case "puntaje_bajo" : nivel = "BAJO"
                    Case Else
                        context.Response.Write(js.Serialize(New With {.success = False, .message = "Nivel no válido"}))
                        Return
                End Select

                If String.IsNullOrEmpty(categoria) OrElse String.IsNullOrEmpty(nivel) Then
                    context.Response.Write(js.Serialize(New With {.success = False, .message = "Datos incompletos"}))
                    Return
                End If

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim sql As String = "UPDATE config_peso_cliente_pm SET valor = @valor, fecha_actualizacion = GETDATE() WHERE categoria = @categoria AND nivel = @nivel"
                    Using cmd As New SqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@valor", Convert.ToDecimal(valor))
                        cmd.Parameters.AddWithValue("@categoria", categoria)
                        cmd.Parameters.AddWithValue("@nivel", nivel)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                context.Response.Write(js.Serialize(New With {.success = True}))
        End Select
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
