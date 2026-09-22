Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_general : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim respuesta As New Dictionary(Of String, Object)
        Dim op As String = context.Request("op")

        Try
            Select Case op
                Case "consultar"
                    Dim datos As New List(Of Dictionary(Of String, Object))()

                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim cmd As New SqlCommand("
                            SELECT seccion AS categoria, 
                                   puntaje_alto, 
                                   puntaje_medio, 
                                   puntaje_bajo 
                            FROM config_peso_general 
                            ORDER BY seccion", conn)

                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                datos.Add(New Dictionary(Of String, Object) From {
                                    {"categoria", reader("categoria")},
                                    {"puntaje_alto", reader("puntaje_alto")},
                                    {"puntaje_medio", reader("puntaje_medio")},
                                    {"puntaje_bajo", reader("puntaje_bajo")}
                                })
                            End While
                        End Using
                    End Using

                    respuesta("success") = True
                    respuesta("data") = datos

                Case "actualizar"
                    Dim categoria = context.Request("categoria")
                    Dim campo = context.Request("campo")
                    Dim valor = Convert.ToDecimal(context.Request("valor"))

                    Dim columna As String = ""
                    Select Case campo
                        Case "puntaje_alto" : columna = "puntaje_alto"
                        Case "puntaje_medio" : columna = "puntaje_medio"
                        Case "puntaje_bajo" : columna = "puntaje_bajo"
                        Case Else
                            respuesta("success") = False
                            respuesta("message") = "Campo no válido"
                            context.Response.Write(js.Serialize(respuesta))
                            Return
                    End Select

                    Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                        conn.Open()
                        Dim sql As String = $"UPDATE config_peso_general SET {columna} = @valor, fecha_actualizacion = GETDATE() WHERE seccion = @categoria"
                        Using cmd As New SqlCommand(sql, conn)
                            cmd.Parameters.AddWithValue("@valor", valor)
                            cmd.Parameters.AddWithValue("@categoria", categoria)
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    respuesta("success") = True

                Case Else
                    respuesta("success") = False
                    respuesta("message") = "Operación no válida"
            End Select

        Catch ex As Exception
            respuesta("success") = False
            respuesta("message") = "Error: " & ex.Message
        End Try

        context.Response.Write(js.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
