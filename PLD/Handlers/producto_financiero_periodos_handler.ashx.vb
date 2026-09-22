Imports System
Imports System.Web

' Alias/compatibilidad: mantiene el endpoint de periodos, pero toda la lógica vive en ProductoFinancieroHandler
Public Class producto_financiero_periodos_handler
    Implements IHttpHandler
    Implements System.Web.SessionState.IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        ' Delegamos 100% al handler consolidado para evitar duplicidad
        Dim h As New ProductoFinancieroHandler()
        h.ProcessRequest(context)
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
