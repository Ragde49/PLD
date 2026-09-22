Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration

Public Class handler_catalogo_canal_pago
    Implements IHttpHandler

    Private ReadOnly Property ConnString As String
        Get
            Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.Charset = "utf-8"

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
                Case Else
                    WriteObj(context, New With {.ok = False, .mensaje = "Operación no válida."})
            End Select
        Catch ex As Exception
            WriteObj(context, New With {.ok = False, .mensaje = "Excepción: " & ex.Message})
        End Try
    End Sub

    ' ========================
    ' LISTAR
    ' ========================
    Private Sub Consultar(context As HttpContext)
        Dim lista As New List(Of Dictionary(Of String, Object))()

        Using cn As New SqlConnection(ConnString)
            cn.Open()
            Dim sql As String = "SELECT id, canal, impacto, probabilidad, nivel_riesgo_pld, activo FROM catalogo_canal_pago ORDER BY id"
            Using cmd As New SqlCommand(sql, cn)
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    While dr.Read()
                        Dim item As New Dictionary(Of String, Object)()
                        item("id") = If(IsDBNull(dr("id")), 0, Convert.ToInt32(dr("id")))
                        item("canal") = If(IsDBNull(dr("canal")), "", dr("canal").ToString())
                        item("impacto") = If(IsDBNull(dr("impacto")), Nothing, dr("impacto"))
                        item("probabilidad") = If(IsDBNull(dr("probabilidad")), Nothing, dr("probabilidad"))
                        item("nivel_riesgo_pld") = If(IsDBNull(dr("nivel_riesgo_pld")), Nothing, dr("nivel_riesgo_pld"))
                        item("activo") = If(IsDBNull(dr("activo")), 0, Convert.ToInt32(dr("activo")))
                        lista.Add(item)
                    End While
                End Using
            End Using
        End Using

        WriteObj(context, lista) 'retorna array JSON
    End Sub

    ' ========================
    ' OBTENER POR ID
    ' ========================
    Private Sub GetById(context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(context.Request("id"), id)
        If id <= 0 Then
            WriteObj(context, New With {.ok = False, .mensaje = "ID inválido"})
            Return
        End If

        Using cn As New SqlConnection(ConnString)
            cn.Open()
            Dim sql As String = "SELECT id, canal, impacto, probabilidad, nivel_riesgo_pld, activo FROM catalogo_canal_pago WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    If dr.Read() Then
                        Dim item As New Dictionary(Of String, Object)()
                        item("id") = If(IsDBNull(dr("id")), 0, Convert.ToInt32(dr("id")))
                        item("canal") = If(IsDBNull(dr("canal")), "", dr("canal").ToString())
                        item("impacto") = If(IsDBNull(dr("impacto")), Nothing, dr("impacto"))
                        item("probabilidad") = If(IsDBNull(dr("probabilidad")), Nothing, dr("probabilidad"))
                        item("nivel_riesgo_pld") = If(IsDBNull(dr("nivel_riesgo_pld")), Nothing, dr("nivel_riesgo_pld"))
                        item("activo") = If(IsDBNull(dr("activo")), 0, Convert.ToInt32(dr("activo")))
                        WriteObj(context, New With {.ok = True, .data = item})
                        Return
                    Else
                        WriteObj(context, New With {.ok = False, .mensaje = "Registro no encontrado"})
                        Return
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' ========================
    ' GUARDAR (INSERT)
    ' ========================
    Private Sub Guardar(context As HttpContext)
        Dim canal As String = (context.Request.Form("canal") & "").Trim()
        If String.IsNullOrEmpty(canal) Then
            WriteObj(context, New With {.ok = False, .mensaje = "La descripción (canal) es requerida."})
            Return
        End If

        Dim impacto As Integer = 0
        Integer.TryParse((context.Request.Form("impacto") & "").Replace(",", "."), impacto)

        Dim probabilidadDecimal As Decimal = 0
        Decimal.TryParse((context.Request.Form("probabilidad") & "").Replace(",", "."), probabilidadDecimal)

        ' Calcula el nivel en servidor (seguridad)
        Dim nivel As Decimal = Math.Round(CDec(impacto) * (probabilidadDecimal / 100D), 2)

        Dim activo As Integer = 0
        Integer.TryParse(context.Request.Form("activo"), activo)

        Using cn As New SqlConnection(ConnString)
            cn.Open()
            Dim sql As String = "INSERT INTO catalogo_canal_pago (canal, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) " &
                                "VALUES (@canal, @impacto, @probabilidad, @nivel, @activo, GETDATE()); SELECT SCOPE_IDENTITY();"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@canal", canal)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidadDecimal)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@activo", activo)
                Dim newIdObj = cmd.ExecuteScalar()
                Dim newId As Integer = 0
                If newIdObj IsNot Nothing Then Integer.TryParse(newIdObj.ToString(), newId)
                WriteObj(context, New With {.ok = True, .mensaje = "Registro guardado.", .id = newId})
            End Using
        End Using
    End Sub

    ' ========================
    ' EDITAR (UPDATE)
    ' ========================
    Private Sub Editar(context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(context.Request.Form("id"), id)
        If id <= 0 Then
            WriteObj(context, New With {.ok = False, .mensaje = "ID inválido para editar."})
            Return
        End If

        Dim canal As String = (context.Request.Form("canal") & "").Trim()
        If String.IsNullOrEmpty(canal) Then
            WriteObj(context, New With {.ok = False, .mensaje = "La descripción (canal) es requerida."})
            Return
        End If

        Dim impacto As Integer = 0
        Integer.TryParse((context.Request.Form("impacto") & "").Replace(",", "."), impacto)

        Dim probabilidadDecimal As Decimal = 0
        Decimal.TryParse((context.Request.Form("probabilidad") & "").Replace(",", "."), probabilidadDecimal)

        ' Recalcular nivel
        Dim nivel As Decimal = Math.Round(CDec(impacto) * (probabilidadDecimal / 100D), 2)

        Dim activo As Integer = 0
        Integer.TryParse(context.Request.Form("activo"), activo)

        Using cn As New SqlConnection(ConnString)
            cn.Open()
            Dim sql As String = "UPDATE catalogo_canal_pago SET canal=@canal, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel, activo=@activo, fecha_modificacion=GETDATE() WHERE id=@id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@canal", canal)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidadDecimal)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@activo", activo)
                cmd.Parameters.AddWithValue("@id", id)
                Dim filas = cmd.ExecuteNonQuery()
                If filas > 0 Then
                    WriteObj(context, New With {.ok = True, .mensaje = "Registro actualizado."})
                Else
                    WriteObj(context, New With {.ok = False, .mensaje = "No se encontró el registro para actualizar."})
                End If
            End Using
        End Using
    End Sub

    ' ========================
    ' DESACTIVAR (baja lógica)
    ' ========================
    Private Sub Desactivar(context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(context.Request.Form("id"), id)
        If id <= 0 Then
            WriteObj(context, New With {.ok = False, .mensaje = "ID inválido para desactivar."})
            Return
        End If

        Using cn As New SqlConnection(ConnString)
            cn.Open()
            Dim sql As String = "UPDATE catalogo_canal_pago SET activo = 0, fecha_modificacion = GETDATE() WHERE id = @id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Dim filas = cmd.ExecuteNonQuery()
                If filas > 0 Then
                    WriteObj(context, New With {.ok = True, .mensaje = "Registro dado de baja."})
                Else
                    WriteObj(context, New With {.ok = False, .mensaje = "No se encontró el registro para dar de baja."})
                End If
            End Using
        End Using
    End Sub

    ' ========================
    ' Helper: enviar JSON
    ' ========================
    Private Sub WriteObj(context As HttpContext, obj As Object)
        Dim js As New JavaScriptSerializer()
        js.MaxJsonLength = Integer.MaxValue
        context.Response.Write(js.Serialize(obj))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
