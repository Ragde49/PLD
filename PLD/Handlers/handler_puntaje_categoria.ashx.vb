Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_puntaje_categoria
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js = New JavaScriptSerializer()
        Dim accion As String = context.Request("accion")

        Select Case accion
            Case "consultar"
                Dim data = New List(Of Dictionary(Of String, Object))()
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim query As String = "SELECT * FROM config_puntaje_categoria WHERE activo = 1 ORDER BY tipo_persona, nivel_riesgo"
                    Using cmd As New SqlCommand(query, conn)
                        Dim reader = cmd.ExecuteReader()
                        While reader.Read()
                            data.Add(New Dictionary(Of String, Object) From {
                                {"id", reader("id")},
                                {"tipo_persona", reader("tipo_persona")},
                                {"nivel_riesgo", reader("nivel_riesgo")},
                                {"valor_minimo", reader("valor_minimo")},
                                {"valor_maximo", reader("valor_maximo")},
                                {"fecha_creacion", reader("fecha_creacion")},
                                {"fecha_actualizacion", reader("fecha_actualizacion")}
                            })
                        End While
                    End Using
                End Using
                context.Response.Write(js.Serialize(data))

            Case "guardar"
                Dim tipo_persona = context.Request("tipo_persona")
                Dim nivel_riesgo = context.Request("nivel_riesgo")
                Dim valor_minimo = Convert.ToDecimal(context.Request("valor_minimo"))
                Dim valor_maximo = Convert.ToDecimal(context.Request("valor_maximo"))

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim query As String = "INSERT INTO config_puntaje_categoria (tipo_persona, nivel_riesgo, valor_minimo, valor_maximo) VALUES (@tipo_persona, @nivel_riesgo, @valor_minimo, @valor_maximo)"
                    Using cmd As New SqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@tipo_persona", tipo_persona)
                        cmd.Parameters.AddWithValue("@nivel_riesgo", nivel_riesgo)
                        cmd.Parameters.AddWithValue("@valor_minimo", valor_minimo)
                        cmd.Parameters.AddWithValue("@valor_maximo", valor_maximo)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                context.Response.Write("{""status"":""ok""}")

            Case "actualizar"
                Dim id = Convert.ToInt32(context.Request("id"))
                Dim valor_minimo = Convert.ToDecimal(context.Request("valor_minimo"))
                Dim valor_maximo = Convert.ToDecimal(context.Request("valor_maximo"))

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim query As String = "UPDATE config_puntaje_categoria SET valor_minimo = @valor_minimo, valor_maximo = @valor_maximo WHERE id = @id"
                    Using cmd As New SqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@valor_minimo", valor_minimo)
                        cmd.Parameters.AddWithValue("@valor_maximo", valor_maximo)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                context.Response.Write("{""status"":""ok""}")

            Case "eliminar"
                Dim id = Convert.ToInt32(context.Request("id"))
                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim query As String = "UPDATE config_puntaje_categoria SET activo = 0 WHERE id = @id"
                    Using cmd As New SqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@id", id)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                context.Response.Write("{""status"":""ok""}")

            Case Else
                context.Response.Write("{""error"":""Acción no válida""}")
        End Select
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
