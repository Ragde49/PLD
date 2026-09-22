Imports System
Imports System.Web
Imports System.Web.UI

Public Class CambiarPassword
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If SeguridadAuth.ObtenerUsuarioActual(Context) Is Nothing Then
            Response.Redirect("~/Login.aspx?ReturnUrl=" & HttpUtility.UrlEncode(Request.RawUrl), False)
        End If
    End Sub

    Protected Sub btnGuardar_Click(ByVal sender As Object, ByVal e As EventArgs)
        If txtNueva.Text <> txtConfirmar.Text Then
            Mostrar("La confirmacion no coincide.", "danger")
            Return
        End If

        Dim mensaje As String = ""
        If SeguridadAuth.CambiarPassword(Context, txtActual.Text, txtNueva.Text, mensaje) Then
            Response.Redirect("~/Default.aspx", False)
            Return
        End If

        Mostrar(mensaje, "danger")
    End Sub

    Private Sub Mostrar(ByVal mensaje As String, ByVal tipo As String)
        litMensaje.Text = "<div class=""alert alert-" & tipo & """ role=""alert"">" & HttpUtility.HtmlEncode(mensaje) & "</div>"
    End Sub
End Class
