Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.IO
Imports System.Web.Script.Serialization

Public Class handler_origen_recursos : Implements IHttpHandler

    Private ReadOnly Serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Try
            Dim op As String = (context.Request("op") & "").ToLower().Trim()

            Select Case op
                Case "listar"
                    Listar(context)
                Case "getbyid"
                    GetById(context)
                Case "guardar"
                    Guardar(context)
                Case "editar"
                    Editar(context)
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    WriteResult(context, New With {.ok = False, .mensaje = "Operación no válida"})
            End Select
        Catch ex As Exception
            WriteResult(context, New With {.ok = False, .mensaje = "Error interno: " & ex.Message})
        End Try
    End Sub

    ' =========================
    ' LISTAR
    ' =========================
    Private Sub Listar(context As HttpContext)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Dim lista As New List(Of Dictionary(Of String, Object))()

        Using cn As New SqlConnection(connStr)
            Dim sql As String = "
                SELECT id, origen, ISNULL(impacto,0) AS impacto, ISNULL(probabilidad,0) AS probabilidad,
                       ISNULL(nivel_riesgo_pld,0.00) AS nivel_riesgo_pld,
                       ISNULL(activo,1) AS estatus
                FROM dbo.catalogo_origen_recursos
                ORDER BY id
            "
            Using cmd As New SqlCommand(sql, cn)
                cn.Open()
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    While dr.Read()
                        Dim row As New Dictionary(Of String, Object)()
                        row("id") = Convert.ToInt32(dr("id"))
                        row("origen") = Convert.ToString(dr("origen"))
                        row("impacto") = Convert.ToInt32(dr("impacto"))
                        row("probabilidad") = Convert.ToInt32(dr("probabilidad"))
                        row("nivel_riesgo_pld") = Convert.ToDecimal(dr("nivel_riesgo_pld"))
                        row("estatus") = If(Convert.IsDBNull(dr("estatus")), 1, Convert.ToInt32(dr("estatus")))
                        lista.Add(row)
                    End While
                End Using
            End Using
        End Using

        WriteResult(context, lista) ' devuelve array JSON
    End Sub

    ' =========================
    ' GET BY ID
    ' =========================
    Private Sub GetById(context As HttpContext)
        Dim idRaw As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idRaw, id) Then
            WriteResult(context, New With {.ok = False, .mensaje = "Id inválido"})
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Using cn As New SqlConnection(connStr)
            Dim sql As String = "
                SELECT id, origen, ISNULL(impacto,0) AS impacto, ISNULL(probabilidad,0) AS probabilidad,
                       ISNULL(nivel_riesgo_pld,0.00) AS nivel_riesgo_pld,
                       ISNULL(activo,1) AS estatus
                FROM dbo.catalogo_origen_recursos
                WHERE id = @id
            "
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    If dr.Read() Then
                        Dim row As New Dictionary(Of String, Object)()
                        row("id") = Convert.ToInt32(dr("id"))
                        row("origen") = Convert.ToString(dr("origen"))
                        row("impacto") = Convert.ToInt32(dr("impacto"))
                        row("probabilidad") = Convert.ToInt32(dr("probabilidad"))
                        row("nivel_riesgo_pld") = Convert.ToDecimal(dr("nivel_riesgo_pld"))
                        row("estatus") = If(Convert.IsDBNull(dr("estatus")), 1, Convert.ToInt32(dr("estatus")))
                        WriteResult(context, row)
                        Return
                    Else
                        WriteResult(context, New With {.ok = False, .mensaje = "Registro no encontrado"})
                        Return
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' =========================
    ' GUARDAR (INSERT)
    ' =========================
    Private Sub Guardar(context As HttpContext)
        Dim body As String = ReadRequestBody(context)
        Dim dict As Dictionary(Of String, Object) = Nothing
        Try
            dict = If(String.IsNullOrWhiteSpace(body), New Dictionary(Of String, Object)(), Serializer.Deserialize(Of Dictionary(Of String, Object))(body))
        Catch ex As Exception
            WriteResult(context, New With {.ok = False, .mensaje = "JSON inválido"})
            Return
        End Try

        Dim origen As String = GetString(dict, "origen").Trim()
        If String.IsNullOrEmpty(origen) Then
            WriteResult(context, New With {.ok = False, .mensaje = "Origen es requerido"})
            Return
        End If

        Dim impacto As Integer = ClampInt(GetInt(dict, "impacto", 0), 0, Integer.MaxValue)
        Dim probabilidad As Integer = ClampInt(GetInt(dict, "probabilidad", 0), 0, 100)
        Dim nivel As Decimal = GetDecimalOrCalc(dict, "nivel_riesgo_pld", impacto, probabilidad)
        Dim activo As Integer = If(GetInt(dict, "estatus", -1) >= 0, GetInt(dict, "estatus", 1), If(GetInt(dict, "activo", -1) >= 0, GetInt(dict, "activo", 1), 1))

        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Using cn As New SqlConnection(connStr)
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim sql As String = "
                        INSERT INTO dbo.catalogo_origen_recursos (origen, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion)
                        VALUES (@origen, @impacto, @probabilidad, @nivel_riesgo_pld, @activo, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS INT);
                    "
                    Using cmd As New SqlCommand(sql, cn, tx)
                        cmd.Parameters.AddWithValue("@origen", origen)
                        cmd.Parameters.AddWithValue("@impacto", impacto)
                        cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                        cmd.Parameters.AddWithValue("@nivel_riesgo_pld", Decimal.Round(nivel, 2))
                        cmd.Parameters.AddWithValue("@activo", activo)
                        Dim newIdObj = cmd.ExecuteScalar()
                        Dim newId As Integer = If(newIdObj Is Nothing OrElse Convert.IsDBNull(newIdObj), 0, Convert.ToInt32(newIdObj))
                        tx.Commit()
                        WriteResult(context, New With {.ok = True, .mensaje = "Guardado correctamente", .id = newId})
                        Return
                    End Using
                Catch ex As Exception
                    Try
                        tx.Rollback()
                    Catch ex2 As Exception
                    End Try
                    WriteResult(context, New With {.ok = False, .mensaje = "Error al guardar: " & ex.Message})
                    Return
                End Try
            End Using
        End Using
    End Sub

    ' =========================
    ' EDITAR (UPDATE)
    ' =========================
    Private Sub Editar(context As HttpContext)
        Dim body As String = ReadRequestBody(context)
        Dim dict As Dictionary(Of String, Object) = Nothing
        Try
            dict = If(String.IsNullOrWhiteSpace(body), New Dictionary(Of String, Object)(), Serializer.Deserialize(Of Dictionary(Of String, Object))(body))
        Catch ex As Exception
            WriteResult(context, New With {.ok = False, .mensaje = "JSON inválido"})
            Return
        End Try

        Dim id As Integer = GetInt(dict, "id", 0)
        If id <= 0 Then
            WriteResult(context, New With {.ok = False, .mensaje = "Id inválido para editar"})
            Return
        End If

        Dim origen As String = GetString(dict, "origen").Trim()
        If String.IsNullOrEmpty(origen) Then
            WriteResult(context, New With {.ok = False, .mensaje = "Origen es requerido"})
            Return
        End If

        Dim impacto As Integer = ClampInt(GetInt(dict, "impacto", 0), 0, Integer.MaxValue)
        Dim probabilidad As Integer = ClampInt(GetInt(dict, "probabilidad", 0), 0, 100)
        Dim nivel As Decimal = GetDecimalOrCalc(dict, "nivel_riesgo_pld", impacto, probabilidad)
        Dim activo As Integer = If(GetInt(dict, "estatus", -1) >= 0, GetInt(dict, "estatus", 1), If(GetInt(dict, "activo", -1) >= 0, GetInt(dict, "activo", 1), 1))

        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Using cn As New SqlConnection(connStr)
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim sql As String = "
                        UPDATE dbo.catalogo_origen_recursos
                        SET origen = @origen,
                            impacto = @impacto,
                            probabilidad = @probabilidad,
                            nivel_riesgo_pld = @nivel_riesgo_pld,
                            activo = @activo
                        WHERE id = @id
                    "
                    Using cmd As New SqlCommand(sql, cn, tx)
                        cmd.Parameters.AddWithValue("@origen", origen)
                        cmd.Parameters.AddWithValue("@impacto", impacto)
                        cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                        cmd.Parameters.AddWithValue("@nivel_riesgo_pld", Decimal.Round(nivel, 2))
                        cmd.Parameters.AddWithValue("@activo", activo)
                        cmd.Parameters.AddWithValue("@id", id)
                        Dim affected = cmd.ExecuteNonQuery()
                        If affected = 0 Then
                            tx.Rollback()
                            WriteResult(context, New With {.ok = False, .mensaje = "Registro no encontrado para actualizar"})
                            Return
                        End If
                        tx.Commit()
                        WriteResult(context, New With {.ok = True, .mensaje = "Actualizado correctamente"})
                        Return
                    End Using
                Catch ex As Exception
                    Try
                        tx.Rollback()
                    Catch ex2 As Exception
                    End Try
                    WriteResult(context, New With {.ok = False, .mensaje = "Error al actualizar: " & ex.Message})
                    Return
                End Try
            End Using
        End Using
    End Sub

    ' =========================
    ' ELIMINAR (LOGICO -> activo = 0)
    ' =========================
    Private Sub Eliminar(context As HttpContext)
        Dim idRaw As String = context.Request("id")
        Dim id As Integer
        If Not Integer.TryParse(idRaw, id) Then
            ' Intentar leer JSON body para callers que envíen POST JSON { id: 5 }
            Dim body As String = ReadRequestBody(context)
            If Not String.IsNullOrWhiteSpace(body) Then
                Try
                    Dim d = Serializer.Deserialize(Of Dictionary(Of String, Object))(body)
                    id = GetInt(d, "id", 0)
                Catch
                End Try
            End If
        End If

        If id <= 0 Then
            WriteResult(context, New With {.ok = False, .mensaje = "Id inválido para eliminar"})
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Using cn As New SqlConnection(connStr)
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim sql As String = "UPDATE dbo.catalogo_origen_recursos SET activo = 0 WHERE id = @id"
                    Using cmd As New SqlCommand(sql, cn, tx)
                        cmd.Parameters.AddWithValue("@id", id)
                        Dim affected = cmd.ExecuteNonQuery()
                        If affected = 0 Then
                            tx.Rollback()
                            WriteResult(context, New With {.ok = False, .mensaje = "Registro no encontrado para eliminar"})
                            Return
                        End If
                        tx.Commit()
                        WriteResult(context, New With {.ok = True, .mensaje = "Eliminado correctamente"})
                        Return
                    End Using
                Catch ex As Exception
                    Try
                        tx.Rollback()
                    Catch ex2 As Exception
                    End Try
                    WriteResult(context, New With {.ok = False, .mensaje = "Error al eliminar: " & ex.Message})
                    Return
                End Try
            End Using
        End Using
    End Sub

    ' -------------------------
    ' Helpers
    ' -------------------------
    Private Function ReadRequestBody(context As HttpContext) As String
        Try
            context.Request.InputStream.Position = 0
            Using sr As New StreamReader(context.Request.InputStream)
                Return sr.ReadToEnd()
            End Using
        Catch
            Return ""
        End Try
    End Function

    Private Sub WriteResult(context As HttpContext, obj As Object)
        Try
            context.Response.Write(Serializer.Serialize(obj))
        Catch ex As Exception
            ' fallback mínimo
            context.Response.Write("{""ok"":false,""mensaje"":""Error al serializar respuesta""}")
        End Try
    End Sub

    Private Function GetString(dict As Dictionary(Of String, Object), key As String) As String
        If dict Is Nothing Then Return ""
        If dict.ContainsKey(key) AndAlso dict(key) IsNot Nothing Then
            Return Convert.ToString(dict(key))
        End If
        Return ""
    End Function

    Private Function GetInt(dict As Dictionary(Of String, Object), key As String, Optional defaultValue As Integer = 0) As Integer
        If dict Is Nothing Then Return defaultValue
        If dict.ContainsKey(key) AndAlso dict(key) IsNot Nothing Then
            Try
                Return Convert.ToInt32(Math.Floor(Convert.ToDouble(dict(key))))
            Catch
            End Try
        End If
        Return defaultValue
    End Function

    Private Function GetDecimalOrCalc(dict As Dictionary(Of String, Object), key As String, impacto As Integer, probabilidad As Integer) As Decimal
        ' Si viene nivel_riesgo_pld lo parseamos, si no lo calculamos como impacto * (probabilidad / 100)
        If dict Is Nothing Then
            Return Decimal.Round(CDec(impacto) * (CDec(probabilidad) / 100D), 2)
        End If
        If dict.ContainsKey(key) AndAlso dict(key) IsNot Nothing Then
            Try
                Dim raw = dict(key)
                Dim d As Decimal = Convert.ToDecimal(raw)
                Return Decimal.Round(d, 2)
            Catch
            End Try
        End If
        Return Decimal.Round(CDec(impacto) * (CDec(probabilidad) / 100D), 2)
    End Function

    Private Function ClampInt(value As Integer, min As Integer, max As Integer) As Integer
        If value < min Then Return min
        If value > max Then Return max
        Return value
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
