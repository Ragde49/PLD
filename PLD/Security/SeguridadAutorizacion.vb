Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web
Imports System.Web.Security

Public NotInheritable Class SeguridadAutorizacion
    Private Sub New()
    End Sub

    Public Shared Function RequiereControl(ByVal ctx As HttpContext) As Boolean
        If ctx Is Nothing OrElse ctx.Request Is Nothing Then Return False
        Dim ext = Path.GetExtension(ctx.Request.Url.AbsolutePath).ToLowerInvariant()
        Return ext = ".aspx" OrElse ext = ".ashx"
    End Function

    Public Shared Function NormalizarRuta(ByVal ctx As HttpContext) As String
        If ctx Is Nothing OrElse ctx.Request Is Nothing Then Return "/"
        Return NormalizarRuta(ctx.Request.Url.AbsolutePath, ctx.Request.ApplicationPath)
    End Function

    Public Shared Function NormalizarRuta(ByVal ruta As String, Optional ByVal appPath As String = "/") As String
        Dim p = If(ruta, "").Replace("\"c, "/"c).Trim()
        If p = "" Then Return "/"

        If Not p.StartsWith("/") Then p = "/" & p

        If Not String.IsNullOrEmpty(appPath) AndAlso appPath <> "/" Then
            If p.StartsWith(appPath, StringComparison.OrdinalIgnoreCase) Then
                p = p.Substring(appPath.Length)
                If Not p.StartsWith("/") Then p = "/" & p
            End If
        End If

        Return p.ToLowerInvariant()
    End Function

    Public Shared Function EsRutaPublica(ByVal ruta As String) As Boolean
        ruta = NormalizarRuta(ruta)
        Return ruta = "/login.aspx" OrElse
            ruta = "/noautorizado.aspx" OrElse
            ruta = "/favicon.ico" OrElse
            ruta = "/handlers/seguridad_handler.ashx"
    End Function

    Public Shared Function PermiteCambioPassword(ByVal ruta As String) As Boolean
        ruta = NormalizarRuta(ruta)
        Return ruta = "/cambiarpassword.aspx" OrElse
            ruta = "/logout.aspx" OrElse
            ruta = "/handlers/seguridad_handler.ashx"
    End Function

    Public Shared Function UsuarioTieneAcceso(ByVal ctx As HttpContext, ByVal ruta As String) As Boolean
        Dim sesion = SeguridadAuth.ObtenerUsuarioActual(ctx)
        If sesion Is Nothing Then Return False
        If sesion.Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) Then Return True

        ruta = NormalizarRuta(ruta, If(ctx Is Nothing OrElse ctx.Request Is Nothing, "/", ctx.Request.ApplicationPath))

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()

            If ruta.EndsWith(".ashx", StringComparison.OrdinalIgnoreCase) Then
                Using cmd As New SqlCommand("
                    SELECT TOP 1 1
                    FROM dbo.seguridad_paginas h
                    INNER JOIN dbo.seguridad_pagina_handler ph
                        ON ph.handler_id = h.id
                       AND ISNULL(ph.activo, 0) = 1
                    INNER JOIN dbo.seguridad_paginas p
                        ON p.id = ph.pagina_id
                       AND ISNULL(p.activo, 0) = 1
                       AND ISNULL(p.es_handler, 0) = 0
                    INNER JOIN dbo.seguridad_rol_pagina rp
                        ON rp.pagina_id = p.id
                    WHERE h.ruta = @ruta
                      AND ISNULL(h.activo, 0) = 1
                      AND ISNULL(h.es_handler, 0) = 1
                      AND rp.rol_id = @rol_id
                      AND ISNULL(rp.puede_ver, 0) = 1;", cn)
                    cmd.Parameters.Add("@ruta", SqlDbType.VarChar, 260).Value = ruta
                    cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = sesion.RolId
                    Return cmd.ExecuteScalar() IsNot Nothing
                End Using
            End If

            Using cmd As New SqlCommand("
                SELECT TOP 1 1
                FROM dbo.seguridad_paginas p
                INNER JOIN dbo.seguridad_rol_pagina rp ON rp.pagina_id = p.id
                WHERE p.ruta = @ruta
                  AND ISNULL(p.activo, 0) = 1
                  AND ISNULL(p.es_handler, 0) = 0
                  AND rp.rol_id = @rol_id
                  AND ISNULL(rp.puede_ver, 0) = 1;", cn)
                cmd.Parameters.Add("@ruta", SqlDbType.VarChar, 260).Value = ruta
                cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = sesion.RolId
                Return cmd.ExecuteScalar() IsNot Nothing
            End Using
        End Using
    End Function

    Public Shared Function EsPeticionHandler(ByVal ctx As HttpContext) As Boolean
        If ctx Is Nothing OrElse ctx.Request Is Nothing Then Return False
        Return Path.GetExtension(ctx.Request.Url.AbsolutePath).Equals(".ashx", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Sub ResponderNoAutenticado(ByVal ctx As HttpContext)
        If EsPeticionHandler(ctx) Then
            EscribirJson(ctx, 401, "Sesion requerida.")
            Return
        End If

        Dim destino = FormsAuthentication.LoginUrl
        Dim actual = ctx.Request.RawUrl
        If Not String.IsNullOrEmpty(actual) Then
            destino &= "?ReturnUrl=" & HttpUtility.UrlEncode(actual)
        End If

        ctx.Response.Redirect(destino, False)
        ctx.ApplicationInstance.CompleteRequest()
    End Sub

    Public Shared Sub ResponderNoAutorizado(ByVal ctx As HttpContext)
        If EsPeticionHandler(ctx) Then
            EscribirJson(ctx, 403, "No tienes permiso para acceder a este recurso.")
            Return
        End If

        ctx.Response.Redirect("~/NoAutorizado.aspx", False)
        ctx.ApplicationInstance.CompleteRequest()
    End Sub

    Public Shared Sub EscribirJson(ByVal ctx As HttpContext, ByVal statusCode As Integer, ByVal mensaje As String)
        ctx.Response.Clear()
        ctx.Response.StatusCode = statusCode
        ctx.Response.ContentType = "application/json; charset=utf-8"
        ctx.Response.Write("{""ok"":false,""mensaje"":""" & HttpUtility.JavaScriptStringEncode(mensaje) & """}")
        ctx.ApplicationInstance.CompleteRequest()
    End Sub

    Public Shared Function ExigirPermisoSeguridadAjax(ByVal ctx As HttpContext) As Boolean
        If SeguridadAuth.ObtenerUsuarioActual(ctx) Is Nothing Then
            EscribirJson(ctx, 401, "Sesion requerida.")
            Return False
        End If

        If Not UsuarioTieneAcceso(ctx, "/secure/seguridad.aspx") Then
            EscribirJson(ctx, 403, "No tienes permiso para administrar seguridad.")
            Return False
        End If

        Return True
    End Function
End Class
