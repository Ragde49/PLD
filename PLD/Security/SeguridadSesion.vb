Imports System
Imports System.Web.Security

Public Class SeguridadSesion
    Public Property Id As Integer
    Public Property Usuario As String
    Public Property Nombre As String
    Public Property RolId As Integer
    Public Property Rol As String
    Public Property DebeCambiarPassword As Boolean

    Public Function ToUserData() As String
        Return Id.ToString() & "|" &
            Limpiar(Usuario) & "|" &
            Limpiar(Nombre) & "|" &
            RolId.ToString() & "|" &
            Limpiar(Rol) & "|" &
            If(DebeCambiarPassword, "1", "0")
    End Function

    Public Shared Function FromTicket(ByVal ticket As FormsAuthenticationTicket) As SeguridadSesion
        If ticket Is Nothing OrElse String.IsNullOrEmpty(ticket.UserData) Then Return Nothing

        Dim partes = ticket.UserData.Split("|"c)
        If partes.Length < 6 Then Return Nothing

        Dim id As Integer
        Dim rolId As Integer
        If Not Integer.TryParse(partes(0), id) Then Return Nothing
        If Not Integer.TryParse(partes(3), rolId) Then Return Nothing

        Return New SeguridadSesion With {
            .Id = id,
            .Usuario = partes(1),
            .Nombre = partes(2),
            .RolId = rolId,
            .Rol = partes(4),
            .DebeCambiarPassword = (partes(5) = "1")
        }
    End Function

    Private Shared Function Limpiar(ByVal valor As String) As String
        If valor Is Nothing Then Return ""
        Return valor.Replace("|", " ").Trim()
    End Function
End Class
