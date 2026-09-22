Imports System
Imports System.Web
Imports System.Web.UI

Public Class Login
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not Page.IsPostBack AndAlso SeguridadAuth.ObtenerUsuarioActual(Context) IsNot Nothing Then
            Response.Redirect(ResolverDestinoAutenticado(), False)
        End If
    End Sub

    Protected Sub btnIngresar_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim sesion As SeguridadSesion = Nothing
        Dim mensaje As String = ""

        Try
            If SeguridadAuth.ValidarCredenciales(txtUsuario.Text, txtPassword.Text, sesion, mensaje) Then
                SeguridadAuth.IniciarSesion(Context, sesion, chkRecordar.Checked)
                Response.Redirect(If(sesion.DebeCambiarPassword, "~/CambiarPassword.aspx", ResolverReturnUrl()), False)
                Return
            End If
        Catch ex As Exception
            mensaje = "No se pudo validar el acceso. Verifica que el script de seguridad ya se ejecuto."
        End Try

        litMensaje.Text = "<div class=""alert alert-danger"" role=""alert"">" & HttpUtility.HtmlEncode(mensaje) & "</div>"
    End Sub

    Private Function ResolverDestinoAutenticado() As String
        Dim sesion = SeguridadAuth.ObtenerUsuarioActual(Context)
        If sesion IsNot Nothing AndAlso sesion.DebeCambiarPassword Then Return "~/CambiarPassword.aspx"
        Return ResolverReturnUrl()
    End Function

    Private Function ResolverReturnUrl() As String
        Dim returnUrl = Convert.ToString(Request("ReturnUrl"))
        If String.IsNullOrWhiteSpace(returnUrl) Then Return "~/Default.aspx"
        If returnUrl.StartsWith("/") AndAlso Not returnUrl.StartsWith("//") AndAlso Not returnUrl.StartsWith("/\") Then Return returnUrl
        Return "~/Default.aspx"
    End Function
End Class
