Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Collections.Generic
Imports System.Text

Public Class handler_prueba_correo_pld
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = ""

            If context.Request("action") IsNot Nothing Then
                action = Convert.ToString(context.Request("action")).Trim().ToLowerInvariant()
            End If

            Select Case action
                Case "probar"
                    respuesta = ProbarCorreo(context)

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida. Usa action=probar."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_prueba_correo_pld."
            respuesta("detalle") = ex.Message
        End Try

        context.Response.Write(serializer.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Function ProbarCorreo(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim destinatario As String = ""

        If context.Request("email") IsNot Nothing Then
            destinatario = Convert.ToString(context.Request("email")).Trim()
        End If

        If String.IsNullOrWhiteSpace(destinatario) Then
            respuesta("ok") = False
            respuesta("mensaje") = "Debes enviar el parámetro email."
            Return respuesta
        End If

        Dim resultado As PLD.PldEmailService.EmailResult = PLD.PldEmailService.EnviarCorreoPrueba(destinatario)

        respuesta("ok") = resultado.Ok
        respuesta("smtp_enabled") = resultado.Enabled
        respuesta("mensaje") = resultado.Mensaje
        respuesta("detalle") = resultado.Detalle

        Return respuesta
    End Function

End Class