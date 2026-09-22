Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Text
Imports System.Globalization

Public Class handler_catalogo_nicho_mercado
    Implements System.Web.IHttpHandler

    Private ReadOnly Property ConnectionString As String
        Get
            Dim cs As String = String.Empty
            If ConfigurationManager.ConnectionStrings("PLDConnection") IsNot Nothing Then
                cs = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings("PLD") IsNot Nothing Then
                cs = ConfigurationManager.ConnectionStrings("PLD").ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings.Count > 0 Then
                cs = ConfigurationManager.ConnectionStrings(0).ConnectionString
            End If
            Return cs
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Try
            Dim op As String = (If(context.Request("op"), String.Empty)).ToLowerInvariant().Trim()

            Select Case op
                Case "consultar"
                    Consultar(context)
                Case "obtener"
                    Obtener(context)
                Case "guardar"
                    Guardar(context)
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    WriteJson(context, New With {Key .success = False, Key .message = "Operación no especificada o inválida."})
            End Select
        Catch ex As Exception
            ' No exponer stack completo en producción; aquí devolvemos mensaje
            WriteJson(context, New With {Key .success = False, Key .message = "Error: " & ex.Message})
        End Try
    End Sub

    Private Sub Consultar(ByVal context As HttpContext)
        ' Devuelve registros activos por defecto
        Dim sql As String = "SELECT id, descripcion, clasificacion, impacto, probabilidad, nivel_riesgo_pld, estatus AS activo, mitigantes FROM dbo.catalogo_nicho_mercado WHERE 1=1"
        Dim onlyActive As String = context.Request("onlyActive")
        If Not String.IsNullOrEmpty(onlyActive) AndAlso (onlyActive = "1" OrElse onlyActive.ToLower() = "true") Then
            sql &= " AND estatus = 1"
        Else
            ' por defecto devolvemos solo activos
            sql &= " AND estatus = 1"
        End If
        sql &= " ORDER BY descripcion"

        Dim list As New List(Of Dictionary(Of String, Object))()

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.CommandType = CommandType.Text
                cn.Open()
                Using r As SqlDataReader = cmd.ExecuteReader()
                    While r.Read()
                        Dim item As New Dictionary(Of String, Object)()
                        item("id") = If(r.IsDBNull(r.GetOrdinal("id")), 0, r.GetInt32(r.GetOrdinal("id")))
                        item("descripcion") = If(r.IsDBNull(r.GetOrdinal("descripcion")), String.Empty, r.GetString(r.GetOrdinal("descripcion")))
                        item("clasificacion") = If(r.IsDBNull(r.GetOrdinal("clasificacion")), String.Empty, r.GetString(r.GetOrdinal("clasificacion")))
                        item("impacto") = If(r.IsDBNull(r.GetOrdinal("impacto")), 0, r.GetInt32(r.GetOrdinal("impacto")))
                        item("probabilidad") = If(r.IsDBNull(r.GetOrdinal("probabilidad")), 0, r.GetInt32(r.GetOrdinal("probabilidad")))
                        item("nivel_riesgo_pld") = If(r.IsDBNull(r.GetOrdinal("nivel_riesgo_pld")), 0D, r.GetDecimal(r.GetOrdinal("nivel_riesgo_pld")))
                        item("estatus") = If(r.IsDBNull(r.GetOrdinal("activo")), 0, r.GetBoolean(r.GetOrdinal("activo")))
                        item("mitigantes") = If(r.IsDBNull(r.GetOrdinal("mitigantes")), 0, r.GetInt32(r.GetOrdinal("mitigantes")))
                        list.Add(item)
                    End While
                End Using
            End Using
        End Using

        WriteJson(context, list)
    End Sub

    Private Sub Obtener(ByVal context As HttpContext)
        Dim idRaw As String = context.Request("id")
        Dim id As Integer = 0
        If Not Integer.TryParse(idRaw, id) Then
            WriteJson(context, New With {Key .success = False, Key .message = "ID inválido"})
            Return
        End If

        Dim sql As String = "SELECT id, descripcion, clasificacion, impacto, probabilidad, nivel_riesgo_pld, estatus AS activo, mitigantes, created_at, created_by, updated_at, updated_by FROM dbo.catalogo_nicho_mercado WHERE id = @id"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using r As SqlDataReader = cmd.ExecuteReader()
                    If r.Read() Then
                        Dim item As New Dictionary(Of String, Object)()
                        item("id") = If(r.IsDBNull(r.GetOrdinal("id")), 0, r.GetInt32(r.GetOrdinal("id")))
                        item("descripcion") = If(r.IsDBNull(r.GetOrdinal("descripcion")), String.Empty, r.GetString(r.GetOrdinal("descripcion")))
                        item("clasificacion") = If(r.IsDBNull(r.GetOrdinal("clasificacion")), String.Empty, r.GetString(r.GetOrdinal("clasificacion")))
                        item("impacto") = If(r.IsDBNull(r.GetOrdinal("impacto")), 0, r.GetInt32(r.GetOrdinal("impacto")))
                        item("probabilidad") = If(r.IsDBNull(r.GetOrdinal("probabilidad")), 0, r.GetInt32(r.GetOrdinal("probabilidad")))
                        item("nivel_riesgo_pld") = If(r.IsDBNull(r.GetOrdinal("nivel_riesgo_pld")), 0D, r.GetDecimal(r.GetOrdinal("nivel_riesgo_pld")))
                        item("estatus") = If(r.IsDBNull(r.GetOrdinal("activo")), 0, r.GetBoolean(r.GetOrdinal("activo")))
                        item("mitigantes") = If(r.IsDBNull(r.GetOrdinal("mitigantes")), 0, r.GetInt32(r.GetOrdinal("mitigantes")))
                        item("created_at") = If(r.IsDBNull(r.GetOrdinal("created_at")), Nothing, r.GetDateTime(r.GetOrdinal("created_at")))
                        item("created_by") = If(r.IsDBNull(r.GetOrdinal("created_by")), String.Empty, r.GetString(r.GetOrdinal("created_by")))
                        item("updated_at") = If(r.IsDBNull(r.GetOrdinal("updated_at")), Nothing, If(r.IsDBNull(r.GetOrdinal("updated_at")), Nothing, r.GetDateTime(r.GetOrdinal("updated_at"))))
                        item("updated_by") = If(r.IsDBNull(r.GetOrdinal("updated_by")), String.Empty, r.GetString(r.GetOrdinal("updated_by")))
                        WriteJson(context, New With {Key .success = True, Key .data = item})
                        Return
                    End If
                End Using
            End Using
        End Using

        WriteJson(context, New With {Key .success = False, Key .message = "Registro no encontrado"})
    End Sub

    Private Sub Guardar(ByVal context As HttpContext)
        ' Lee el cuerpo JSON
        Dim json As String = String.Empty
        Using sr As New System.IO.StreamReader(context.Request.InputStream, Encoding.UTF8)
            sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin)
            json = sr.ReadToEnd()
        End Using

        If String.IsNullOrEmpty(json) Then
            WriteJson(context, New With {Key .success = False, Key .message = "Cuerpo vacío"})
            Return
        End If

        Dim js As New JavaScriptSerializer()
        Dim data As Dictionary(Of String, Object) = Nothing

        Try
            data = js.Deserialize(Of Dictionary(Of String, Object))(json)
        Catch ex As Exception
            WriteJson(context, New With {Key .success = False, Key .message = "JSON inválido: " & ex.Message})
            Return
        End Try

        ' Extraer campos y validar
        Dim id As Integer = 0
        If data.ContainsKey("id") Then Integer.TryParse(Convert.ToString(data("id")), id)

        Dim descripcion As String = If(data.ContainsKey("descripcion"), Convert.ToString(data("descripcion")).Trim(), String.Empty)
        Dim clasificacion As String = If(data.ContainsKey("clasificacion"), Convert.ToString(data("clasificacion")).Trim(), String.Empty)

        Dim impacto As Integer = 0
        If data.ContainsKey("impacto") Then Integer.TryParse(Convert.ToString(data("impacto")), impacto)

        Dim probabilidad As Integer = 0
        If data.ContainsKey("probabilidad") Then Integer.TryParse(Convert.ToString(data("probabilidad")), probabilidad)

        Dim nivel_riesgo_pld As Decimal = 0D
        If data.ContainsKey("nivel_riesgo_pld") Then
            Dim tmp As String = Convert.ToString(data("nivel_riesgo_pld")).Replace(",", ".")
            Decimal.TryParse(tmp, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, nivel_riesgo_pld)
        End If

        Dim estatus As Integer = 1
        If data.ContainsKey("estatus") Then Integer.TryParse(Convert.ToString(data("estatus")), estatus)

        Dim user As String = If(data.ContainsKey("user"), Convert.ToString(data("user")), "system")

        ' Validaciones básicas
        If String.IsNullOrEmpty(descripcion) Then
            WriteJson(context, New With {Key .success = False, Key .message = "La descripción es obligatoria."})
            Return
        End If
        If impacto < 0 Then
            WriteJson(context, New With {Key .success = False, Key .message = "Impacto inválido."})
            Return
        End If
        If probabilidad < 0 Then
            WriteJson(context, New With {Key .success = False, Key .message = "Probabilidad inválida."})
            Return
        End If
        If nivel_riesgo_pld < 0 Then
            WriteJson(context, New With {Key .success = False, Key .message = "Nivel de riesgo P.L.D inválido."})
            Return
        End If

        If id <= 0 Then
            ' Insert
            Dim sqlIns As String = "INSERT INTO dbo.catalogo_nicho_mercado (descripcion, clasificacion, impacto, probabilidad, nivel_riesgo_pld, estatus, mitigantes, created_by, created_at) " &
                                   "VALUES (@descripcion, @clasificacion, @impacto, @probabilidad, @nivel_riesgo_pld, @estatus, @mitigantes, @created_by, SYSUTCDATETIME()); SELECT SCOPE_IDENTITY();"

            Using cn As New SqlConnection(ConnectionString)
                Using cmd As New SqlCommand(sqlIns, cn)
                    cmd.Parameters.AddWithValue("@descripcion", descripcion)
                    cmd.Parameters.AddWithValue("@clasificacion", If(String.IsNullOrEmpty(clasificacion), DBNull.Value, CType(clasificacion, Object)))
                    cmd.Parameters.AddWithValue("@impacto", impacto)
                    cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                    cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                    cmd.Parameters.AddWithValue("@estatus", If(estatus = 1, 1, 0))
                    cmd.Parameters.AddWithValue("@mitigantes", 0)
                    cmd.Parameters.AddWithValue("@created_by", user)
                    cn.Open()
                    Dim newIdObj As Object = cmd.ExecuteScalar()
                    Dim newId As Integer = 0
                    If newIdObj IsNot Nothing AndAlso Integer.TryParse(Convert.ToString(newIdObj), newId) Then
                        WriteJson(context, New With {Key .success = True, Key .message = "Registro creado.", Key .id = newId})
                        Return
                    Else
                        WriteJson(context, New With {Key .success = False, Key .message = "No se obtuvo ID luego del INSERT."})
                        Return
                    End If
                End Using
            End Using
        Else
            ' Update
            Dim sqlUpd As String = "UPDATE dbo.catalogo_nicho_mercado SET descripcion=@descripcion, clasificacion=@clasificacion, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel_riesgo_pld, estatus=@estatus, updated_by=@updated_by, updated_at=SYSUTCDATETIME() WHERE id=@id"

            Using cn As New SqlConnection(ConnectionString)
                Using cmd As New SqlCommand(sqlUpd, cn)
                    cmd.Parameters.AddWithValue("@descripcion", descripcion)
                    cmd.Parameters.AddWithValue("@clasificacion", If(String.IsNullOrEmpty(clasificacion), DBNull.Value, CType(clasificacion, Object)))
                    cmd.Parameters.AddWithValue("@impacto", impacto)
                    cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                    cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                    cmd.Parameters.AddWithValue("@estatus", If(estatus = 1, 1, 0))
                    cmd.Parameters.AddWithValue("@updated_by", user)
                    cmd.Parameters.AddWithValue("@id", id)
                    cn.Open()
                    Dim affected As Integer = cmd.ExecuteNonQuery()
                    If affected > 0 Then
                        WriteJson(context, New With {Key .success = True, Key .message = "Registro actualizado.", Key .id = id})
                        Return
                    Else
                        WriteJson(context, New With {Key .success = False, Key .message = "No se actualizó ningún registro (ID no encontrado o sin cambios)."})
                        Return
                    End If
                End Using
            End Using
        End If
    End Sub

    Private Sub Eliminar(ByVal context As HttpContext)
        ' Soft-delete por defecto
        Dim idRaw As String = context.Request("id")
        Dim id As Integer = 0
        If Not Integer.TryParse(idRaw, id) Then
            WriteJson(context, New With {Key .success = False, Key .message = "ID inválido"})
            Return
        End If

        Dim user As String = If(context.Request("user"), "system")

        Dim sql As String = "UPDATE dbo.catalogo_nicho_mercado SET estatus = 0, updated_at = SYSUTCDATETIME(), updated_by = @updated_by WHERE id = @id"

        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@updated_by", user)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim affected As Integer = cmd.ExecuteNonQuery()
                If affected > 0 Then
                    WriteJson(context, New With {Key .success = True, Key .message = "Registro eliminado (soft)."})
                    Return
                Else
                    WriteJson(context, New With {Key .success = False, Key .message = "No se encontró el registro para eliminar."})
                    Return
                End If
            End Using
        End Using
    End Sub

    Private Sub WriteJson(ByVal context As HttpContext, ByVal obj As Object)
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue
        Dim json As String = js.Serialize(obj)
        context.Response.Write(json)
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
