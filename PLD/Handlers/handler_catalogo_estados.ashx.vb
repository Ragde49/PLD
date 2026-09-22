' handler_catalogo_estados.ashx.vb
Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Globalization

Public Class handler_catalogo_estados
    Implements IHttpHandler

    Private ReadOnly Property ConnectionString As String
        Get
            Dim cs As String = String.Empty
            If ConfigurationManager.ConnectionStrings("PLDConnection") IsNot Nothing Then
                cs = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
            ElseIf ConfigurationManager.ConnectionStrings.Count > 0 Then
                cs = ConfigurationManager.ConnectionStrings(0).ConnectionString
            End If
            Return cs
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        Dim op As String = Convert.ToString(context.Request("op")).ToLower().Trim()

        Try
            Select Case op
                Case "consultar"
                    Consultar(context)
                Case "obtener"
                    Obtener(context)
                Case "guardar"
                    Guardar(context)
                Case "actualizar"
                    Actualizar(context)
                Case "toggle"
                    ToggleEstatus(context)
                Case "eliminar"
                    Eliminar(context)
                Case Else
                    context.Response.StatusCode = 400
                    context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Operación no especificada."}))
            End Select
        Catch ex As Exception
            context.Response.StatusCode = 500
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = ex.Message}))
        End Try
    End Sub

    Private Sub Consultar(ByVal context As HttpContext)
        Dim sql As String = "SELECT id, descripcion, clave, clave_estado, impacto, probabilidad, nivel_riesgo_pld, estatus FROM dbo.catalogo_estados ORDER BY descripcion"
        Dim dt As New DataTable()
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        Dim serializer As New JavaScriptSerializer()
        Dim rows As New List(Of Object)()
        For Each r As DataRow In dt.Rows
            rows.Add(New With {
                .id = Convert.ToInt32(r("id")),
                .descripcion = Convert.ToString(r("descripcion")),
                .clave = If(IsDBNull(r("clave")), "", Convert.ToString(r("clave"))),
                .clave_estado = If(IsDBNull(r("clave_estado")), "", Convert.ToString(r("clave_estado"))),
                .impacto = If(IsDBNull(r("impacto")), 0, Convert.ToInt32(r("impacto"))),
                .probabilidad = If(IsDBNull(r("probabilidad")), 0, Convert.ToInt32(r("probabilidad"))),
                .nivel_riesgo_pld = If(IsDBNull(r("nivel_riesgo_pld")), 0D, Convert.ToDecimal(r("nivel_riesgo_pld"))),
                .estatus = If(IsDBNull(r("estatus")), False, Convert.ToBoolean(r("estatus")))
            })
        Next

        context.Response.Write(serializer.Serialize(New With {.success = True, .data = rows}))
    End Sub

    Private Sub Obtener(ByVal context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(Convert.ToString(context.Request("id")), id)
        If id <= 0 Then
            context.Response.StatusCode = 400
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Id inválido"}))
            Return
        End If

        Dim sql As String = "SELECT id, descripcion, clave, clave_estado, impacto, probabilidad, nivel_riesgo_pld, estatus FROM dbo.catalogo_estados WHERE id = @id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim obj = New With {
                            .id = Convert.ToInt32(rdr("id")),
                            .descripcion = Convert.ToString(rdr("descripcion")),
                            .clave = If(IsDBNull(rdr("clave")), "", Convert.ToString(rdr("clave"))),
                            .clave_estado = If(IsDBNull(rdr("clave_estado")), "", Convert.ToString(rdr("clave_estado"))),
                            .impacto = If(IsDBNull(rdr("impacto")), 0, Convert.ToInt32(rdr("impacto"))),
                            .probabilidad = If(IsDBNull(rdr("probabilidad")), 0, Convert.ToInt32(rdr("probabilidad"))),
                            .nivel_riesgo_pld = If(IsDBNull(rdr("nivel_riesgo_pld")), 0D, Convert.ToDecimal(rdr("nivel_riesgo_pld"))),
                            .estatus = If(IsDBNull(rdr("estatus")), False, Convert.ToBoolean(rdr("estatus")))
                        }
                        context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = True, .data = obj}))
                    Else
                        context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "No encontrado"}))
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(ByVal context As HttpContext)
        Dim descripcion As String = Convert.ToString(context.Request("descripcion")).Trim()
        Dim clave As String = Convert.ToString(context.Request("clave")).Trim()
        Dim clave_estado As String = Convert.ToString(context.Request("clave_estado")).Trim()

        Dim impacto As Integer = 0
        Dim probabilidad As Integer = 0
        Dim nivel_riesgo_pld As Decimal = 0D

        Integer.TryParse(Convert.ToString(context.Request("impacto")), impacto)
        Integer.TryParse(Convert.ToString(context.Request("probabilidad")), probabilidad)
        Decimal.TryParse(Convert.ToString(context.Request("nivel_riesgo_pld")), nivel_riesgo_pld)

        If String.IsNullOrEmpty(descripcion) Then
            context.Response.StatusCode = 400
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Descripción requerida"}))
            Return
        End If

        Dim sql As String = "INSERT INTO dbo.catalogo_estados (descripcion, clave, clave_estado, impacto, probabilidad, nivel_riesgo_pld, estatus, fecha_creacion) " &
                            "VALUES (@descripcion, @clave, @clave_estado, @impacto, @probabilidad, @nivel_riesgo_pld, @estatus, GETDATE()); SELECT SCOPE_IDENTITY();"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                If String.IsNullOrEmpty(clave) Then
                    cmd.Parameters.AddWithValue("@clave", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave", clave)
                End If

                If String.IsNullOrEmpty(clave_estado) Then
                    cmd.Parameters.AddWithValue("@clave_estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_estado", clave_estado)
                End If

                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                cmd.Parameters.AddWithValue("@estatus", True)
                cn.Open()
                Dim newId = cmd.ExecuteScalar()
                context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = True, .id = newId}))
            End Using
        End Using
    End Sub

    Private Sub Actualizar(ByVal context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(Convert.ToString(context.Request("id")), id)
        If id <= 0 Then
            context.Response.StatusCode = 400
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Id inválido"}))
            Return
        End If

        Dim descripcion As String = Convert.ToString(context.Request("descripcion")).Trim()
        Dim clave As String = Convert.ToString(context.Request("clave")).Trim()
        Dim clave_estado As String = Convert.ToString(context.Request("clave_estado")).Trim()

        Dim impacto As Integer = 0
        Dim probabilidad As Integer = 0
        Dim nivel_riesgo_pld As Decimal = 0D

        Integer.TryParse(Convert.ToString(context.Request("impacto")), impacto)
        Integer.TryParse(Convert.ToString(context.Request("probabilidad")), probabilidad)
        Decimal.TryParse(Convert.ToString(context.Request("nivel_riesgo_pld")), nivel_riesgo_pld)

        Dim sql As String = "UPDATE dbo.catalogo_estados SET descripcion=@descripcion, clave=@clave, clave_estado=@clave_estado, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel_riesgo_pld WHERE id=@id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
                If String.IsNullOrEmpty(clave) Then
                    cmd.Parameters.AddWithValue("@clave", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave", clave)
                End If

                If String.IsNullOrEmpty(clave_estado) Then
                    cmd.Parameters.AddWithValue("@clave_estado", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@clave_estado", clave_estado)
                End If

                cmd.Parameters.AddWithValue("@impacto", impacto)
                cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
                cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivel_riesgo_pld)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                cmd.ExecuteNonQuery()
                context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = True}))
            End Using
        End Using
    End Sub

    Private Sub ToggleEstatus(ByVal context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(Convert.ToString(context.Request("id")), id)
        Dim est As Boolean = False
        Boolean.TryParse(Convert.ToString(context.Request("estatus")), est)
        If id <= 0 Then
            context.Response.StatusCode = 400
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Id inválido"}))
            Return
        End If

        Dim sql As String = "UPDATE dbo.catalogo_estados SET estatus=@estatus WHERE id=@id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@estatus", est)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                cmd.ExecuteNonQuery()
                context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = True}))
            End Using
        End Using
    End Sub

    Private Sub Eliminar(ByVal context As HttpContext)
        Dim id As Integer = 0
        Integer.TryParse(Convert.ToString(context.Request("id")), id)
        If id <= 0 Then
            context.Response.StatusCode = 400
            context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = False, .message = "Id inválido"}))
            Return
        End If

        Dim sql As String = "DELETE FROM dbo.catalogo_estados WHERE id=@id"
        Using cn As New SqlConnection(ConnectionString)
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                cn.Open()
                cmd.ExecuteNonQuery()
                context.Response.Write(New JavaScriptSerializer().Serialize(New With {.success = True}))
            End Using
        End Using
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
