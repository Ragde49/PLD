<%@ WebHandler Language="VB" Class="PLD.handler_catalogo_destino_recursos" %>

Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Globalization

Namespace PLD

    Public Class handler_catalogo_destino_recursos
        Implements IHttpHandler

        Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            context.Response.ContentType = "application/json"
            Dim op As String = (context.Request("op") & "").ToLower().Trim()

            Try
                Select Case op
                    Case "consultar" : Consultar(context)
                    Case "getbyid" : GetById(context)
                    Case "guardar" : Guardar(context)
                    Case "eliminar" : Eliminar(context)
                    Case Else
                        WriteObj(context, New With {.ok = False, .mensaje = "Operación no válida: " & op})
                End Select
            Catch ex As Exception
                WriteObj(context, New With {.ok = False, .mensaje = "Excepción: " & ex.Message})
            End Try
        End Sub

        ' =======================
        ' CONSULTAR (LISTADO)
        ' =======================
        Private Sub Consultar(context As HttpContext)
            Dim lista As New List(Of Dictionary(Of String, Object))()
            Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString

            Dim q As String = (context.Request("q") & "").Trim()
            Dim mostrarActivo As String = (context.Request("activo") & "").Trim() ' "1" o "0" o vacío

            Using cn As New SqlConnection(connStr)
                cn.Open()
                Dim sql As String = "SELECT id, descripcion, valor, impacto, ocurrencia, nivel_riesgo_pld, ISNULL(estatus,'') AS estatus, ISNULL(activo,1) AS activo, ISNULL(mitigantes,0) AS mitigantes, fecha_creacion " &
                                    "FROM dbo.catalogo_destino_recursos WHERE 1=1 "
                If Not String.IsNullOrEmpty(q) Then
                    sql &= " AND (descripcion LIKE @q OR valor LIKE @q) "
                End If
                If mostrarActivo = "1" Or mostrarActivo = "0" Then
                    sql &= " AND ISNULL(activo,1) = @activo "
                End If
                sql &= " ORDER BY descripcion"

                Using cmd As New SqlCommand(sql, cn)
                    If Not String.IsNullOrEmpty(q) Then
                        cmd.Parameters.AddWithValue("@q", "%" & q & "%")
                    End If
                    If mostrarActivo = "1" Or mostrarActivo = "0" Then
                        cmd.Parameters.AddWithValue("@activo", Convert.ToInt32(mostrarActivo))
                    End If

                    Using r As SqlDataReader = cmd.ExecuteReader()
                        While r.Read()
                            Dim row As New Dictionary(Of String, Object)()
                            row("id") = r("id")
                            row("descripcion") = If(IsDBNull(r("descripcion")), "", r("descripcion"))
                            row("valor") = If(IsDBNull(r("valor")), "", r("valor"))
                            row("impacto") = If(IsDBNull(r("impacto")), 0, Convert.ToInt32(r("impacto")))
                            row("ocurrencia") = If(IsDBNull(r("ocurrencia")), "", r("ocurrencia"))
                            row("nivel_riesgo_pld") = If(IsDBNull(r("nivel_riesgo_pld")), 0D, Convert.ToDecimal(r("nivel_riesgo_pld")))
                            row("estatus") = If(IsDBNull(r("estatus")), "", r("estatus"))
                            row("activo") = If(IsDBNull(r("activo")), True, Convert.ToBoolean(r("activo")))
                            row("mitigantes") = If(IsDBNull(r("mitigantes")), 0, Convert.ToInt32(r("mitigantes")))
                            row("fecha_creacion") = If(IsDBNull(r("fecha_creacion")), Nothing, r("fecha_creacion"))
                            lista.Add(row)
                        End While
                    End Using
                End Using
            End Using

            WriteObj(context, New With {.ok = True, .data = lista})
        End Sub

        ' =======================
        ' GET BY ID
        ' =======================
        Private Sub GetById(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {.ok = False, .mensaje = "Id inválido"})
                Return
            End If

            Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            Using cn As New SqlConnection(connStr)
                cn.Open()
                Dim sql As String = "SELECT id, descripcion, valor, impacto, ocurrencia, nivel_riesgo_pld, ISNULL(estatus,'') AS estatus, ISNULL(activo,1) AS activo, ISNULL(mitigantes,0) AS mitigantes, fecha_creacion " &
                                    "FROM dbo.catalogo_destino_recursos WHERE id = @id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@id", id)
                    Using r As SqlDataReader = cmd.ExecuteReader()
                        If r.Read() Then
                            Dim row As New Dictionary(Of String, Object)()
                            row("id") = r("id")
                            row("descripcion") = If(IsDBNull(r("descripcion")), "", r("descripcion"))
                            row("valor") = If(IsDBNull(r("valor")), "", r("valor"))
                            row("impacto") = If(IsDBNull(r("impacto")), 0, Convert.ToInt32(r("impacto")))
                            row("ocurrencia") = If(IsDBNull(r("ocurrencia")), "", r("ocurrencia"))
                            row("nivel_riesgo_pld") = If(IsDBNull(r("nivel_riesgo_pld")), 0D, Convert.ToDecimal(r("nivel_riesgo_pld")))
                            row("estatus") = If(IsDBNull(r("estatus")), "", r("estatus"))
                            row("activo") = If(IsDBNull(r("activo")), True, Convert.ToBoolean(r("activo")))
                            row("mitigantes") = If(IsDBNull(r("mitigantes")), 0, Convert.ToInt32(r("mitigantes")))
                            row("fecha_creacion") = If(IsDBNull(r("fecha_creacion")), Nothing, r("fecha_creacion"))
                            WriteObj(context, New With {.ok = True, .data = row})
                            Return
                        Else
                            WriteObj(context, New With {.ok = False, .mensaje = "No se encontró registro con id = " & id})
                            Return
                        End If
                    End Using
                End Using
            End Using
        End Sub

        ' =======================
        ' GUARDAR (INSERT / UPDATE)
        ' =======================
        Private Sub Guardar(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            Dim descripcion As String = (context.Request("descripcion") & "").Trim()
            Dim valor As String = (context.Request("valor") & "").Trim()
            Dim impacto As Integer = 0
            Integer.TryParse(context.Request("impacto"), impacto)
            Dim ocurrencia As String = (context.Request("ocurrencia") & "").Trim()
            Dim nivelStr As String = (context.Request("nivel_riesgo_pld") & "").Trim()
            Dim nivel As Decimal = 0D
            Decimal.TryParse(nivelStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, nivel)
            Dim estatus As String = (context.Request("estatus") & "").Trim()
            Dim activo As Integer = 1
            If Not String.IsNullOrEmpty(context.Request("activo")) Then Integer.TryParse(context.Request("activo"), activo)
            Dim mitigantes As Integer = 0
            Integer.TryParse(context.Request("mitigantes"), mitigantes)

            If String.IsNullOrEmpty(descripcion) Then
                WriteObj(context, New With {.ok = False, .mensaje = "La descripción es requerida."})
                Return
            End If

            Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            Using cn As New SqlConnection(connStr)
                cn.Open()
                If id > 0 Then
                    ' UPDATE
                    Dim sqlUpd As String = "UPDATE dbo.catalogo_destino_recursos SET descripcion = @descripcion, valor = @valor, impacto = @impacto, ocurrencia = @ocurrencia, nivel_riesgo_pld = @nivel, estatus = @estatus, activo = @activo, mitigantes = @mitigantes WHERE id = @id"
                    Using cmd As New SqlCommand(sqlUpd, cn)
                        cmd.Parameters.AddWithValue("@descripcion", descripcion)
                        cmd.Parameters.AddWithValue("@valor", If(String.IsNullOrEmpty(valor), DBNull.Value, CType(valor, Object)))
                        cmd.Parameters.AddWithValue("@impacto", If(impacto = 0, DBNull.Value, CType(impacto, Object)))
                        cmd.Parameters.AddWithValue("@ocurrencia", If(String.IsNullOrEmpty(ocurrencia), DBNull.Value, CType(ocurrencia, Object)))
                        cmd.Parameters.AddWithValue("@nivel", If(nivel = 0D, DBNull.Value, CType(nivel, Object)))
                        cmd.Parameters.AddWithValue("@estatus", If(String.IsNullOrEmpty(estatus), DBNull.Value, CType(estatus, Object)))
                        cmd.Parameters.AddWithValue("@activo", activo)
                        cmd.Parameters.AddWithValue("@mitigantes", mitigantes)
                        cmd.Parameters.AddWithValue("@id", id)

                        Dim aff As Integer = cmd.ExecuteNonQuery()
                        If aff > 0 Then
                            WriteObj(context, New With {.ok = True, .mensaje = "Actualizado correctamente.", .id = id})
                        Else
                            WriteObj(context, New With {.ok = False, .mensaje = "No se actualizó (id no encontrado)."})
                        End If
                    End Using
                Else
                    ' INSERT
                    Dim sqlIns As String = "INSERT INTO dbo.catalogo_destino_recursos (descripcion, valor, impacto, ocurrencia, nivel_riesgo_pld, estatus, activo, mitigantes, fecha_creacion) " &
                                           "VALUES (@descripcion, @valor, @impacto, @ocurrencia, @nivel, @estatus, @activo, @mitigantes, GETDATE()); SELECT SCOPE_IDENTITY();"
                    Using cmd As New SqlCommand(sqlIns, cn)
                        cmd.Parameters.AddWithValue("@descripcion", descripcion)
                        cmd.Parameters.AddWithValue("@valor", If(String.IsNullOrEmpty(valor), DBNull.Value, CType(valor, Object)))
                        cmd.Parameters.AddWithValue("@impacto", If(impacto = 0, DBNull.Value, CType(impacto, Object)))
                        cmd.Parameters.AddWithValue("@ocurrencia", If(String.IsNullOrEmpty(ocurrencia), DBNull.Value, CType(ocurrencia, Object)))
                        cmd.Parameters.AddWithValue("@nivel", If(nivel = 0D, DBNull.Value, CType(nivel, Object)))
                        cmd.Parameters.AddWithValue("@estatus", If(String.IsNullOrEmpty(estatus), DBNull.Value, CType(estatus, Object)))
                        cmd.Parameters.AddWithValue("@activo", activo)
                        cmd.Parameters.AddWithValue("@mitigantes", mitigantes)

                        Dim newIdObj As Object = cmd.ExecuteScalar()
                        Dim newId As Integer = 0
                        If Not newIdObj Is Nothing Then Integer.TryParse(newIdObj.ToString(), newId)
                        WriteObj(context, New With {.ok = True, .mensaje = "Insertado correctamente.", .id = newId})
                    End Using
                End If
            End Using
        End Sub

        ' =======================
        ' ELIMINAR (BAJA LÓGICA)
        ' =======================
        Private Sub Eliminar(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {.ok = False, .mensaje = "Id inválido para eliminar."})
                Return
            End If

            Dim connStr As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            Using cn As New SqlConnection(connStr)
                cn.Open()
                Dim sql As String = "UPDATE dbo.catalogo_destino_recursos SET activo = 0 WHERE id = @id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@id", id)
                    Dim aff As Integer = cmd.ExecuteNonQuery()
                    If aff > 0 Then
                        WriteObj(context, New With {.ok = True, .mensaje = "Registro desactivado correctamente.", .id = id})
                    Else
                        WriteObj(context, New With {.ok = False, .mensaje = "No se encontró registro para desactivar."})
                    End If
                End Using
            End Using
        End Sub

        ' =======================
        ' Helper: escribir JSON
        ' =======================
        Private Sub WriteObj(context As HttpContext, obj As Object)
            Dim js As New JavaScriptSerializer()
            js.MaxJsonLength = Int32.MaxValue
            context.Response.Write(js.Serialize(obj))
        End Sub

        Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return False
            End Get
        End Property

    End Class

End Namespace
