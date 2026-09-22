Imports System.Web
Imports System.Web.Services
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class handler_peso_transacciones
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim js As New JavaScriptSerializer()

        Try
            Dim op As String = context.Request("op")

            If op = "consultar" Then
                ' PIVOTE MANUAL para armar columnas ALTO, MEDIO, BAJO por tipo_transaccion
                Dim lista As New List(Of Dictionary(Of String, Object))()

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()
                    Dim query As String = "
                        SELECT tipo_transaccion,
                            MAX(CASE WHEN nivel = 'ALTO' THEN valor ELSE 0 END) AS puntaje_alto,
                            MAX(CASE WHEN nivel = 'MEDIO' THEN valor ELSE 0 END) AS puntaje_medio,
                            MAX(CASE WHEN nivel = 'BAJO' THEN valor ELSE 0 END) AS puntaje_bajo
                        FROM config_peso_transacciones
                        GROUP BY tipo_transaccion
                        ORDER BY tipo_transaccion
                    "
                    Dim cmd As New SqlCommand(query, conn)
                    Dim rdr As SqlDataReader = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim item As New Dictionary(Of String, Object) From {
                            {"categoria", rdr("tipo_transaccion")},
                            {"puntaje_alto", rdr("puntaje_alto")},
                            {"puntaje_medio", rdr("puntaje_medio")},
                            {"puntaje_bajo", rdr("puntaje_bajo")}
                        }
                        lista.Add(item)
                    End While
                End Using

                context.Response.Write(js.Serialize(New With {.success = True, .data = lista}))
                Return

            ElseIf op = "actualizar" Then
                Dim categoria As String = context.Request("categoria")
                Dim campo As String = context.Request("campo")
                Dim valor As Decimal = Convert.ToDecimal(context.Request("valor"))

                Dim nivel As String = ""
                If campo = "puntaje_alto" Then
                    nivel = "ALTO"
                ElseIf campo = "puntaje_medio" Then
                    nivel = "MEDIO"
                ElseIf campo = "puntaje_bajo" Then
                    nivel = "BAJO"
                Else
                    Throw New Exception("Campo no válido.")
                End If

                Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    conn.Open()

                    ' Verificar si ya existe ese registro
                    Dim existeCmd As New SqlCommand("SELECT COUNT(*) FROM config_peso_transacciones WHERE tipo_transaccion = @categoria AND nivel = @nivel", conn)
                    existeCmd.Parameters.AddWithValue("@categoria", categoria)
                    existeCmd.Parameters.AddWithValue("@nivel", nivel)
                    Dim existe As Integer = Convert.ToInt32(existeCmd.ExecuteScalar())

                    If existe = 0 Then
                        ' Insertar si no existe
                        Dim insertCmd As New SqlCommand("
                            INSERT INTO config_peso_transacciones (tipo_transaccion, nivel, valor, fecha_actualizacion)
                            VALUES (@categoria, @nivel, @valor, GETDATE())", conn)
                        insertCmd.Parameters.AddWithValue("@categoria", categoria)
                        insertCmd.Parameters.AddWithValue("@nivel", nivel)
                        insertCmd.Parameters.AddWithValue("@valor", valor)
                        insertCmd.ExecuteNonQuery()
                    Else
                        ' Actualizar si ya existe
                        Dim updateCmd As New SqlCommand("
                            UPDATE config_peso_transacciones
                            SET valor = @valor, fecha_actualizacion = GETDATE()
                            WHERE tipo_transaccion = @categoria AND nivel = @nivel", conn)
                        updateCmd.Parameters.AddWithValue("@categoria", categoria)
                        updateCmd.Parameters.AddWithValue("@nivel", nivel)
                        updateCmd.Parameters.AddWithValue("@valor", valor)
                        updateCmd.ExecuteNonQuery()
                    End If
                End Using

                context.Response.Write(js.Serialize(New With {.success = True}))
                Return

            Else
                context.Response.Write(js.Serialize(New With {.error = "Operación no válida."}))
            End If

        Catch ex As Exception
            context.Response.Write(js.Serialize(New With {.error = ex.Message}))
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
