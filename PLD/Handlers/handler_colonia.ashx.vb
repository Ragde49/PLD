Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_colonia : Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.Clear()
        context.Response.ContentType = "application/json"
        Dim jss As New JavaScriptSerializer()
        Dim respuesta As New Dictionary(Of String, Object)

        Try
            Dim json As String = New IO.StreamReader(context.Request.InputStream).ReadToEnd()
            Dim data = jss.Deserialize(Of Dictionary(Of String, String))(json)
            Dim accion = data("accion").Trim()

            Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                con.Open()

                If accion = "guardar" Then
                    Dim cp = data("cp").Trim()
                    Dim asentamiento = data("asentamiento").Trim()
                    Dim municipio = data("municipio").Trim()
                    Dim estado = data("estado").Trim()
                    Dim ciudad = data("ciudad").Trim()

                    ' Insertamos la colonia si no existe
                    Dim validar As New SqlCommand("
                        SELECT COUNT(*) FROM catalogo_sepomex 
                        WHERE cp = @cp AND asentamiento = @asentamiento 
                          AND municipio = @municipio AND estado = @estado", con)

                    validar.Parameters.AddWithValue("@cp", cp)
                    validar.Parameters.AddWithValue("@asentamiento", asentamiento)
                    validar.Parameters.AddWithValue("@municipio", municipio)
                    validar.Parameters.AddWithValue("@estado", estado)

                    If Convert.ToInt32(validar.ExecuteScalar()) > 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "La colonia ya existe para ese código postal."
                    Else
                        Dim cmd As New SqlCommand("
                            INSERT INTO catalogo_sepomex 
                            (cp, asentamiento, tipo_asentamiento, municipio, estado, ciudad, zona, clave_estado, clave_municipio)
                            VALUES (@cp, @asentamiento, '', @municipio, @estado, @ciudad, '', '', '')", con)

                        cmd.Parameters.AddWithValue("@cp", cp)
                        cmd.Parameters.AddWithValue("@asentamiento", asentamiento)
                        cmd.Parameters.AddWithValue("@municipio", municipio)
                        cmd.Parameters.AddWithValue("@estado", estado)
                        cmd.Parameters.AddWithValue("@ciudad", ciudad)

                        cmd.ExecuteNonQuery()

                        respuesta("ok") = True
                        respuesta("mensaje") = "Colonia registrada correctamente."
                    End If

                ElseIf accion = "buscar_cp" Then
                    Dim cp = data("cp").Trim()

                    Dim cmd As New SqlCommand("
                        SELECT TOP 1 estado, municipio, ciudad 
                        FROM catalogo_sepomex WHERE cp = @cp", con)
                    cmd.Parameters.AddWithValue("@cp", cp)

                    Dim reader = cmd.ExecuteReader()

                    If reader.Read() Then
                        Dim estado = reader("estado").ToString()
                        Dim municipio = reader("municipio").ToString()
                        Dim ciudad = reader("ciudad").ToString()
                        reader.Close()

                        Dim colonias As New List(Of String)
                        Dim cmd2 As New SqlCommand("SELECT asentamiento FROM catalogo_sepomex WHERE cp = @cp ORDER BY asentamiento", con)
                        cmd2.Parameters.AddWithValue("@cp", cp)
                        Dim rdr2 = cmd2.ExecuteReader()
                        While rdr2.Read()
                            colonias.Add(rdr2("asentamiento").ToString())
                        End While
                        rdr2.Close()

                        respuesta("ok") = True
                        respuesta("mensaje") = "Datos obtenidos correctamente."
                        Dim datosPlanos As New Dictionary(Of String, Object) From {
                            {"estado", estado},
                            {"municipio", municipio},
                            {"ciudad", ciudad},
                            {"colonias", colonias}
                        }
                        respuesta("data") = datosPlanos
                    Else
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró información para ese código postal."
                    End If

                Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
                End If
            End Using

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error: " & ex.Message
        End Try

        ' SERIALIZAR SOLO DATOS PLANOS
        Dim jsonFinal As String = jss.Serialize(New Dictionary(Of String, Object) From {
            {"ok", If(respuesta.ContainsKey("ok"), respuesta("ok"), False)},
            {"mensaje", If(respuesta.ContainsKey("mensaje"), respuesta("mensaje"), "")},
            {"data", If(respuesta.ContainsKey("data"), respuesta("data"), Nothing)}
        })

        context.Response.Write(jsonFinal)
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
