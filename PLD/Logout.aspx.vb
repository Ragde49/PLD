Imports System
Imports System.Web.UI

Public Class Logout
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        SeguridadAuth.CerrarSesion(Context)
        Response.Redirect("~/Login.aspx", False)
    End Sub
End Class
