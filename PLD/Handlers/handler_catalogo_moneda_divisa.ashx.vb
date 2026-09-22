Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration

Public Class handler_catalogo_moneda_divisa
    Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.TrySkipIisCustomErrors = True

        Dim op As String = (context.Request("op") & "").ToLower().Trim()

        Try
            Select Case op
                Case "consultar"
                    Consultar(context)
                Case "guardar"
                    Guardar(context)
                Case "editar"
                    Editar(context)
                Case "activar"
                    ToggleEstatus(context, True)
                Case "desactivar"
                    ToggleEstatus(context, False)
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    ReturnError(context, "Operación no válida", 400)
            End Select
        Catch ex As Exception
            Dim js As New JavaScriptSerializer()
            context.Response.StatusCode = 500
            context.Response.Write(js.Serialize(New With {
                .ok = False,
                .mensaje = "Excepción en handler: " & ex.Message,
                .detalle = ex.ToString()
            }))
        End Try
    End Sub

    ' ----------------------
    ' CONSULTAR
    ' ----------------------
    Private Sub Consultar(context As HttpContext)
        Dim q As String = (context.Request("q") & "").Trim()
        Dim lista As New List(Of Dictionary(Of String, Object))()
        Dim sql As String = "SELECT id, moneda, clave, ISNULL(impacto,0) AS impacto, ISNULL(probabilidad,0) AS probabilidad, ISNULL(nivel_riesgo_pld,0) AS nivel_riesgo_pld, ISNULL(estatus,0) AS estatus FROM dbo.catalogo_moneda_divisa"

        If q <> "" Then
            sql &= " WHERE moneda LIKE @q OR clave LIKE @q"
        End If

        sql &= " ORDER BY moneda"

        Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                If q <> "" Then
                    cmd.Parameters.AddWithValue("@q", "%" & q & "%")
                End If
                cn.Open()
                Using r As SqlDataReader = cmd.ExecuteReader()
                    While r.Read()
                        Dim d As New Dictionary(Of String, Object)()
                        d("id") = r("id")
                        d("moneda") = If(IsDBNull(r("moneda")), "", r("moneda"))
                        d("clave") = If(IsDBNull(r("clave")), "", r("clave"))
                        d("impacto") = Convert.ToInt32(r("impacto"))
                        d("probabilidad") = Convert.ToInt32(r("probabilidad"))
                        d("nivel_riesgo_pld") = Convert.ToDecimal(r("nivel_riesgo_pld"))
                        d("estatus") = Convert.ToBoolean(r("estatus"))
                        lista.Add(d)
                    End While
                End Using
            End Using
        End Using

        WriteJson(context, New With {.ok = True, .datos = lista})
    End Sub

    ' ----------------------
    ' GUARDAR
    ' ----------------------
    Private Sub Guardar(context As HttpContext)
        Dim moneda As String = (context.Request("moneda") & "").Trim()
        Dim clave As String = (context.Request("clave") & "").Trim()
        Dim impacto As Integer = ParseInt(context.Request("impacto"))
        Dim probabilidad As Integer = ParseInt(context.Request("probabilidad"))
        Dim nivelStr As String = (context.Request("nivel_riesgo_pld") & "").Trim()
        Dim estatus As Boolean = ParseBool(context.Request("estatus"))

        Dim nivel As Decimal = 0D
        If nivelStr <> "" Then
            Decimal.TryParse(nivelStr.Replace(",", "."), nivel)
        Else
            nivel = Math.Round((impacto * probabilidad) / 100D, 2)
        End If

        Dim sql As String = "INSERT INTO dbo.catalogo_moneda_divisa (moneda, clave, impacto, probabilidad, nivel_riesgo_pld, estatus, fecha_creacion) " &
                            "VALUES (@moneda, @clave, @impacto, @probabilidad, @nivel, @estatus, GETDATE()); SELECT SCOPE_IDENTITY();"

        Dim newId As Integer = 0
        Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@moneda", moneda)
                cmd.Parameters.AddWithValue("@clave", clave)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@estatus", If(estatus, 1, 0))
                cn.Open()
                Dim obj = cmd.ExecuteScalar()
                If Not IsDBNull(obj) AndAlso obj IsNot Nothing Then
                    Integer.TryParse(obj.ToString(), newId)
                End If
            End Using
        End Using

        If newId > 0 Then
            WriteJson(context, New With {.ok = True, .mensaje = "Guardado correctamente", .id = newId})
        Else
            ReturnError(context, "No se pudo insertar", 500)
        End If
    End Sub

    ' ----------------------
    ' EDITAR
    ' ----------------------
    Private Sub Editar(context As HttpContext)
        Dim id As Integer = ParseInt(context.Request("id"))
        If id <= 0 Then
            ReturnError(context, "Id inválido", 400)
            Return
        End If

        Dim moneda As String = (context.Request("moneda") & "").Trim()
        Dim clave As String = (context.Request("clave") & "").Trim()
        Dim impacto As Integer = ParseInt(context.Request("impacto"))
        Dim probabilidad As Integer = ParseInt(context.Request("probabilidad"))
        Dim nivelStr As String = (context.Request("nivel_riesgo_pld") & "").Trim()
        Dim estatus As Boolean = ParseBool(context.Request("estatus"))

        Dim nivel As Decimal = 0D
        If nivelStr <> "" Then
            Decimal.TryParse(nivelStr.Replace(",", "."), nivel)
        Else
            nivel = Math.Round((impacto * probabilidad) / 100D, 2)
        End If

        Dim sql As String = "UPDATE dbo.catalogo_moneda_divisa SET moneda=@moneda, clave=@clave, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel, estatus=@estatus WHERE id=@id"

        Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@moneda", moneda)
                cmd.Parameters.AddWithValue("@clave", clave)
                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel", nivel)
                cmd.Parameters.AddWithValue("@estatus", If(estatus, 1, 0))
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows > 0 Then
                    WriteJson(context, New With {.ok = True, .mensaje = "Actualizado correctamente"})
                Else
                    ReturnError(context, "No se actualizó (id inválido?)", 404)
                End If
            End Using
        End Using
    End Sub

    ' ----------------------
    ' TOGGLE ESTATUS
    ' ----------------------
    Private Sub ToggleEstatus(context As HttpContext, ByVal enCI As Boolean)
        Dim id As Integer = ParseInt(context.Request("id"))
        If id <= 0 Then
            ReturnError(context, "Id inválido", 400)
            Return
        End If

        Dim sql As String = "UPDATE dbo.catalogo_moneda_divisa SET estatus=@estatus WHERE id=@id"
        Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@estatus", If(enCI, 1, 0))
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows > 0 Then
                    WriteJson(context, New With {.ok = True, .mensaje = If(enCI, "Activado", "Desactivado")})
                Else
                    ReturnError(context, "No se pudo actualizar el estatus", 404)
                End If
            End Using
        End Using
    End Sub

    ' ----------------------
    ' ELIMINAR
    ' ----------------------
    Private Sub Eliminar(context As HttpContext)
        Dim id As Integer = ParseInt(context.Request("id"))
        If id <= 0 Then
            ReturnError(context, "Id inválido", 400)
            Return
        End If

        Dim sql As String = "DELETE FROM dbo.catalogo_moneda_divisa WHERE id=@id"
        Using cn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows > 0 Then
                    WriteJson(context, New With {.ok = True, .mensaje = "Eliminado correctamente"})
                Else
                    ReturnError(context, "No se encontró el registro", 404)
                End If
            End Using
        End Using
    End Sub

    ' ----------------------
    ' UTILIDADES
    ' ----------------------
    Private Sub ReturnError(context As HttpContext, ByVal mensaje As String, ByVal status As Integer)
        context.Response.StatusCode = status
        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(New With {.ok = False, .mensaje = mensaje}))
    End Sub

    Private Sub WriteJson(context As HttpContext, ByVal obj As Object)
        Dim js As New JavaScriptSerializer()
        context.Response.Write(js.Serialize(obj))
    End Sub

    Private Function ParseInt(ByVal v As Object) As Integer
        Dim s As String = (v & "").Trim()
        Dim n As Integer = 0
        Integer.TryParse(s, n)
        Return n
    End Function

    Private Function ParseBool(ByVal v As Object) As Boolean
        Dim s As String = (v & "").Trim().ToLower()
        If s = "1" Or s = "true" Or s = "on" Or s = "si" Then
            Return True
        End If
        Return False
    End Function

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
