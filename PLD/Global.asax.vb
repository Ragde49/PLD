Imports System.Web.Optimization

Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(sender As Object, e As EventArgs)

    End Sub

    Sub Application_AuthenticateRequest(sender As Object, e As EventArgs)
        SeguridadAuth.RestaurarPrincipalDesdeCookie(HttpContext.Current)
    End Sub

    Sub Application_AuthorizeRequest(sender As Object, e As EventArgs)
        Dim ctx = HttpContext.Current
        If ctx Is Nothing OrElse Not SeguridadAutorizacion.RequiereControl(ctx) Then Return

        Dim ruta = SeguridadAutorizacion.NormalizarRuta(ctx)
        If SeguridadAutorizacion.EsRutaPublica(ruta) Then Return

        Dim sesion = SeguridadAuth.ObtenerUsuarioActual(ctx)
        If sesion Is Nothing Then
            SeguridadAutorizacion.ResponderNoAutenticado(ctx)
            Return
        End If

        If SeguridadAutorizacion.PermiteCambioPassword(ruta) Then Return

        If sesion.DebeCambiarPassword AndAlso Not SeguridadAutorizacion.PermiteCambioPassword(ruta) Then
            If SeguridadAutorizacion.EsPeticionHandler(ctx) Then
                SeguridadAutorizacion.EscribirJson(ctx, 403, "Debes cambiar tu contrasena antes de continuar.")
            Else
                ctx.Response.Redirect("~/CambiarPassword.aspx", False)
                ctx.ApplicationInstance.CompleteRequest()
            End If
            Return
        End If

        If Not SeguridadAutorizacion.UsuarioTieneAcceso(ctx, ruta) Then
            SeguridadAutorizacion.ResponderNoAutorizado(ctx)
        End If
    End Sub

    Sub Application_EndRequest(sender As Object, e As EventArgs)
        Dim ctx As HttpContext = HttpContext.Current

        If ctx Is Nothing Then Return
        If Not SeguridadAutorizacion.EsPeticionHandler(ctx) Then Return

        If ctx.Response Is Nothing Then Return

        If ctx.Response.StatusCode = 302 Then

            Dim location As String = String.Empty

            If ctx.Response.Headers IsNot Nothing Then
                location = Convert.ToString(ctx.Response.Headers("Location"))
            End If

            If Not String.IsNullOrWhiteSpace(location) AndAlso
           location.IndexOf("Login.aspx", StringComparison.OrdinalIgnoreCase) >= 0 Then

                ctx.Response.Clear()
                ctx.Response.StatusCode = 401
                ctx.Response.ContentType = "application/json; charset=utf-8"
                ctx.Response.Write("{""ok"":false,""mensaje"":""Sesion requerida.""}")
                ctx.Response.End()
            End If

        End If
    End Sub
End Class
