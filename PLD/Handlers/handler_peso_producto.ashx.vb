Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_producto
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Dim resultado As New List(Of Dictionary(Of String, Object))()
        Dim op As String = context.Request("op")

        If op = "consultar" Then
            Using conn As New SqlConnection(connStr)
                conn.Open()
                Dim sql As String = "
                    SELECT producto,
                        SUM(CASE WHEN nivel = 'ALTO' THEN valor ELSE 0 END) AS puntaje_alto,
                        SUM(CASE WHEN nivel = 'MEDIO' THEN valor ELSE 0 END) AS puntaje_medio,
                        SUM(CASE WHEN nivel = 'BAJO' THEN valor ELSE 0 END) AS puntaje_bajo
                    FROM config_peso_producto
                    GROUP BY producto
                    ORDER BY producto"
                Using cmd As New SqlCommand(sql, conn)
                    Dim reader = cmd.ExecuteReader()
                    While reader.Read()
                        resultado.Add(New Dictionary(Of String, Object) From {
                            {"categoria", reader("producto")},
                            {"puntaje_alto", reader("puntaje_alto")},
                            {"puntaje_medio", reader("puntaje_medio")},
                            {"puntaje_bajo", reader("puntaje_bajo")}
                        })
                    End While
                End Using
            End Using
            context.Response.Write(js.Serialize(New With {.success = True, .data = resultado}))
            Return
        End If

        If op = "actualizar" Then
            Dim producto As String = context.Request("categoria")
            Dim campo As String = context.Request("campo")
            Dim valor As String = context.Request("valor")

            Dim nivel As String = ""
            If campo = "puntaje_alto" Then nivel = "ALTO"
            If campo = "puntaje_medio" Then nivel = "MEDIO"
            If campo = "puntaje_bajo" Then nivel = "BAJO"

            If nivel = "" Then
                context.Response.Write(js.Serialize(New With {.success = False, .error = "Nivel inválido"}))
                Return
            End If

            Using conn As New SqlConnection(connStr)
                conn.Open()
                Dim sql As String = "
                    UPDATE config_peso_producto
                    SET valor = @valor, fecha_actualizacion = GETDATE()
                    WHERE producto = @producto AND nivel = @nivel"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@valor", valor)
                    cmd.Parameters.AddWithValue("@producto", producto)
                    cmd.Parameters.AddWithValue("@nivel", nivel)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            context.Response.Write(js.Serialize(New With {.success = True}))
            Return
        End If

        context.Response.Write(js.Serialize(New With {.success = False, .error = "Operación no válida."}))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
