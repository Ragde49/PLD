Imports System.Web
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Text
Imports System.Web.Script.Serialization

Public Class importar_sepomex : Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim respuesta As New Dictionary(Of String, Object)
        Dim jss As New JavaScriptSerializer()
        Dim hojasProcesadas As Integer = 0
        Dim registrosInsertados As Integer = 0

        Try
            If context.Request.Files.Count = 0 Then
                respuesta("ok") = False
                respuesta("mensaje") = "No se recibió archivo."
                context.Response.Write(jss.Serialize(respuesta))
                Return
            End If

            Dim archivo = context.Request.Files(0)
            Dim rutaTemporal = context.Server.MapPath("~/App_Data/" & archivo.FileName)
            archivo.SaveAs(rutaTemporal)

            Dim conexion = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & rutaTemporal & ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'"
            Using oledbConn As New OleDbConnection(conexion)
                oledbConn.Open()

                Dim hojaTabla As DataTable = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing)
                If hojaTabla Is Nothing OrElse hojaTabla.Rows.Count = 0 Then
                    Throw New Exception("No se encontraron hojas en el archivo.")
                End If

                Dim todos = System.Configuration.ConfigurationManager.ConnectionStrings
                Dim pld = todos("PLDConnection")
                If pld Is Nothing Then
                    Throw New Exception("🚨 La cadena 'PLDConnection' NO existe en este contexto.")
                Else
                    Dim cad = pld.ConnectionString
                    If String.IsNullOrEmpty(cad) Then
                        Throw New Exception("🚨 La cadena 'PLDConnection' está vacía.")
                    End If
                End If

                Using con As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                    con.Open()
                    Dim tran As SqlTransaction = con.BeginTransaction()

                    For Each hoja As DataRow In hojaTabla.Rows
                        Dim hojaNombre As String = hoja("TABLE_NAME").ToString()

                        Dim dt As New DataTable()
                        Dim cmd As New OleDbCommand("SELECT * FROM [" & hojaNombre & "]", oledbConn)
                        Dim adapter As New OleDbDataAdapter(cmd)

                        Try
                            adapter.Fill(dt)
                        Catch ex As Exception
                            Continue For ' hoja inválida, se salta
                        End Try

                        ' Validar si hay filas
                        If dt.Rows.Count = 0 Then Continue For

                        ' Validar si contiene columna clave
                        If Not dt.Columns.Contains("d_codigo") Then Continue For

                        Dim valuesList As New List(Of String)

                        ' 🔄 Llenamos la lista
                        For Each row As DataRow In dt.Rows
                            Try
                                Dim cp = If(row.Table.Columns.Contains("d_codigo"), row("d_codigo").ToString(), "").Replace("'", "''").Trim()
                                Dim asentamiento = If(row.Table.Columns.Contains("d_asenta"), row("d_asenta").ToString(), "").Replace("'", "''").Trim()
                                Dim tipo = If(row.Table.Columns.Contains("d_tipo_asenta"), row("d_tipo_asenta").ToString(), "").Replace("'", "''").Trim()
                                Dim muni = If(row.Table.Columns.Contains("D_mnpio"), row("D_mnpio").ToString(), "").Replace("'", "''").Trim()
                                Dim edo = If(row.Table.Columns.Contains("d_estado"), row("d_estado").ToString(), "").Replace("'", "''").Trim()
                                Dim ciudad = If(row.Table.Columns.Contains("d_ciudad"), row("d_ciudad").ToString(), "").Replace("'", "''").Trim()
                                Dim zona = If(row.Table.Columns.Contains("d_zona"), row("d_zona").ToString(), "").Replace("'", "''").Trim()
                                Dim claveEdo = If(row.Table.Columns.Contains("c_estado"), row("c_estado").ToString(), "").Replace("'", "''").Trim()
                                Dim claveMuni = If(row.Table.Columns.Contains("c_mnpio"), row("c_mnpio").ToString(), "").Replace("'", "''").Trim()

                                valuesList.Add($"('{cp}', '{asentamiento}', '{tipo}', '{muni}', '{edo}', '{ciudad}', '{zona}', '{claveEdo}', '{claveMuni}')")
                            Catch ex As Exception
                                Continue For ' si una fila falla, se ignora
                            End Try
                        Next

                        hojasProcesadas += 1
                        registrosInsertados += valuesList.Count

                        ' ✅ Ejecutar insert en lotes de 1000
                        If valuesList.Count > 0 Then
                            Dim batchSize As Integer = 1000
                            For i As Integer = 0 To valuesList.Count - 1 Step batchSize
                                Dim batch = valuesList.Skip(i).Take(batchSize).ToList()
                                Dim insertBatch As New StringBuilder()
                                insertBatch.Append("INSERT INTO catalogo_sepomex (cp, asentamiento, tipo_asentamiento, municipio, estado, ciudad, zona, clave_estado, clave_municipio) VALUES ")
                                insertBatch.Append(String.Join(",", batch))

                                Dim batchCmd As New SqlCommand(insertBatch.ToString(), con, tran)
                                Try
                                    batchCmd.ExecuteNonQuery()
                                Catch sqlEx As SqlException
                                    If sqlEx.Number = 2601 Or sqlEx.Number = 2627 Then
                                        ' Registro duplicado: se ignora
                                    Else
                                        Throw
                                    End If
                                End Try
                            Next
                        End If
                    Next

                    tran.Commit()
                End Using
            End Using

            respuesta("ok") = True
            respuesta("mensaje") = $"Importación finalizada. Hojas procesadas: {hojasProcesadas}, registros cargados: {registrosInsertados}."

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error al procesar el archivo: " & ex.Message
        End Try

        context.Response.Write(jss.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
