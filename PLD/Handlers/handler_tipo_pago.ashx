<%@ WebHandler Language="VB" Class="PLD.handler_tipo_pago" %>

Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration

Namespace PLD

    Public Class handler_tipo_pago
        Implements IHttpHandler

        Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            context.Response.ContentType = "application/json"
            Dim op As String = (context.Request("op") & "").ToLower().Trim()

            Try
                Select Case op
                    Case "consultar"
                        Consultar(context)
                    Case "getbyid"
                        GetById(context)
                    Case "guardar"
                        Guardar(context)
                    Case "editar"
                        Editar(context)
                    Case "desactivar"
                        Desactivar(context)
                    Case "toggle"
                        ToggleEstatus(context)
                    Case Else
                        WriteObj(context, New With {Key .ok = False, Key .mensaje = "Operación no válida: " & op})
                End Select
            Catch ex As Exception
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Error general: " & ex.Message})
            End Try
        End Sub

        ' ============================
        ' UTILIDADES
        ' ============================
        Private Function GetConnString() As String
            Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        End Function

        Private Sub WriteObj(context As HttpContext, obj As Object)
            Dim js As New JavaScriptSerializer()
            context.Response.Write(js.Serialize(obj))
        End Sub

        ' ============================
        ' CONSULTAR (lista)
        ' ============================
        Private Sub Consultar(context As HttpContext)
            Dim lista As New List(Of Dictionary(Of String, Object))()
            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "SELECT id, descripcion, impacto, probabilidad, nivel_riesgo_pld, estatus FROM catalogo_tipo_pago ORDER BY id ASC"
                Using cmd As New SqlCommand(sql, cn)
                    Using r = cmd.ExecuteReader()
                        While r.Read()
                            Dim row As New Dictionary(Of String, Object)()
                            row("id") = If(IsDBNull(r("id")), 0, Convert.ToInt32(r("id")))
                            row("descripcion") = If(IsDBNull(r("descripcion")), "", r("descripcion").ToString())
                            row("impacto") = If(IsDBNull(r("impacto")), 0, Convert.ToInt32(r("impacto")))
                            row("probabilidad") = If(IsDBNull(r("probabilidad")), 0, Convert.ToInt32(r("probabilidad")))
                            row("nivel_riesgo_pld") = If(IsDBNull(r("nivel_riesgo_pld")), 0D, Convert.ToDecimal(r("nivel_riesgo_pld")))
                            row("estatus") = If(IsDBNull(r("estatus")), False, Convert.ToBoolean(r("estatus")))
                            lista.Add(row)
                        End While
                    End Using
                End Using
            End Using

            WriteObj(context, New With {Key .ok = True, Key .data = lista})
        End Sub

        ' ============================
        ' GET BY ID
        ' ============================
        Private Sub GetById(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Id inválido."})
                Return
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "SELECT id, descripcion, impacto, probabilidad, nivel_riesgo_pld, estatus FROM catalogo_tipo_pago WHERE id=@id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@id", id)
                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            Dim obj As New Dictionary(Of String, Object)()
                            obj("id") = If(IsDBNull(r("id")), 0, Convert.ToInt32(r("id")))
                            obj("descripcion") = If(IsDBNull(r("descripcion")), "", r("descripcion").ToString())
                            obj("impacto") = If(IsDBNull(r("impacto")), 0, Convert.ToInt32(r("impacto")))
                            obj("probabilidad") = If(IsDBNull(r("probabilidad")), 0, Convert.ToInt32(r("probabilidad")))
                            obj("nivel_riesgo_pld") = If(IsDBNull(r("nivel_riesgo_pld")), 0D, Convert.ToDecimal(r("nivel_riesgo_pld")))
                            obj("estatus") = If(IsDBNull(r("estatus")), False, Convert.ToBoolean(r("estatus")))
                            WriteObj(context, New With {Key .ok = True, Key .data = obj})
                            Return
                        End If
                    End Using
                End Using
            End Using

            WriteObj(context, New With {Key .ok = False, Key .mensaje = "Registro no encontrado."})
        End Sub

        ' ============================
        ' GUARDAR (alta)
        ' ============================
        Private Sub Guardar(context As HttpContext)
            Dim descripcion As String = (context.Request("descripcion") & "").Trim()
            Dim impacto As Integer = 0
            Integer.TryParse(context.Request("impacto"), impacto)
            Dim probabilidad As Integer = 0
            Integer.TryParse(context.Request("probabilidad"), probabilidad)
            Dim nivel_riesgo_pld As Decimal = 0
            Decimal.TryParse((context.Request("nivel_riesgo_pld") & "").Replace(",", "."), nivel_riesgo_pld)

            If descripcion = "" Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Descripción requerida."})
                Return
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "INSERT INTO catalogo_tipo_pago (descripcion, impacto, probabilidad, nivel_riesgo_pld, estatus, creado_por, creado_en) " &
                                    "VALUES (@descripcion, @impacto, @probabilidad, @nivel_riesgo_pld, @estatus, @usuario, GETDATE()); SELECT SCOPE_IDENTITY();"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@descripcion", descripcion)
                    cmd.Parameters.AddWithValue("@impacto", impacto)
                    cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                    cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                    cmd.Parameters.AddWithValue("@estatus", If((context.Request("estatus") = "1" Or context.Request("estatus") = "true"), 1, 0))
                    cmd.Parameters.AddWithValue("@usuario", (context.Request("usuario") & "")) ' opcional
                    Dim newIdObj = cmd.ExecuteScalar()
                    Dim newId As Integer = 0
                    Integer.TryParse(newIdObj.ToString(), newId)
                    WriteObj(context, New With {Key .ok = True, Key .mensaje = "Registro creado.", Key .id = newId})
                    Return
                End Using
            End Using
        End Sub

        ' ============================
        ' EDITAR
        ' ============================
        Private Sub Editar(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Id inválido."})
                Return
            End If

            Dim descripcion As String = (context.Request("descripcion") & "").Trim()
            Dim impacto As Integer = 0
            Integer.TryParse(context.Request("impacto"), impacto)
            Dim probabilidad As Integer = 0
            Integer.TryParse(context.Request("probabilidad"), probabilidad)
            Dim nivel_riesgo_pld As Decimal = 0
            Decimal.TryParse((context.Request("nivel_riesgo_pld") & "").Replace(",", "."), nivel_riesgo_pld)

            If descripcion = "" Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Descripción requerida."})
                Return
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "UPDATE catalogo_tipo_pago SET descripcion=@descripcion, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel_riesgo_pld, modificado_por=@usuario, modificado_en=GETDATE() WHERE id=@id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@descripcion", descripcion)
                    cmd.Parameters.AddWithValue("@impacto", impacto)
                    cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                    cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                    cmd.Parameters.AddWithValue("@usuario", (context.Request("usuario") & ""))
                    cmd.Parameters.AddWithValue("@id", id)
                    Dim afectadas = cmd.ExecuteNonQuery()
                    If afectadas > 0 Then
                        WriteObj(context, New With {Key .ok = True, Key .mensaje = "Registro actualizado."})
                    Else
                        WriteObj(context, New With {Key .ok = False, Key .mensaje = "No se actualizó (id no encontrado)."})
                    End If
                End Using
            End Using
        End Sub

        ' ============================
        ' DESACTIVAR (baja lógica)
        ' ============================
        Private Sub Desactivar(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Id inválido."})
                Return
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "UPDATE catalogo_tipo_pago SET estatus = 0, modificado_por=@usuario, modificado_en=GETDATE() WHERE id=@id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@usuario", (context.Request("usuario") & ""))
                    cmd.Parameters.AddWithValue("@id", id)
                    Dim filas = cmd.ExecuteNonQuery()
                    If filas > 0 Then
                        WriteObj(context, New With {Key .ok = True, Key .mensaje = "Registro desactivado."})
                    Else
                        WriteObj(context, New With {Key .ok = False, Key .mensaje = "No se desactivó (id no encontrado)."})
                    End If
                End Using
            End Using
        End Sub

        ' ============================
        ' TOGGLE ESTATUS
        ' ============================
        Private Sub ToggleEstatus(context As HttpContext)
            Dim id As Integer = 0
            Integer.TryParse(context.Request("id"), id)
            If id <= 0 Then
                WriteObj(context, New With {Key .ok = False, Key .mensaje = "Id inválido."})
                Return
            End If

            Using cn As New SqlConnection(GetConnString())
                cn.Open()
                Dim sql As String = "UPDATE catalogo_tipo_pago SET estatus = CASE WHEN estatus = 1 THEN 0 ELSE 1 END, modificado_por=@usuario, modificado_en=GETDATE(); SELECT estatus FROM catalogo_tipo_pago WHERE id=@id"
                Using cmd As New SqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@usuario", (context.Request("usuario") & ""))
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.ExecuteNonQuery()
                    ' Obtener nuevo estatus
                    Using cmd2 As New SqlCommand("SELECT estatus FROM catalogo_tipo_pago WHERE id=@id", cn)
                        cmd2.Parameters.AddWithValue("@id", id)
                        Dim nt = cmd2.ExecuteScalar()
                        Dim est = False
                        If nt IsNot Nothing Then
                            est = Convert.ToBoolean(nt)
                        End If
                        WriteObj(context, New With {Key .ok = True, Key .mensaje = "Estatus actualizado.", Key .estatus = est})
                    End Using
                End Using
            End Using
        End Sub

        Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return False
            End Get
        End Property

    End Class

End Namespace
