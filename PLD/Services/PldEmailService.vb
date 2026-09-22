Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Net
Imports System.Net.Mail
Imports System.Text

Namespace PLD

    Public Class PldEmailService

        Public Class EmailResult
            Public Property Ok As Boolean
            Public Property Enabled As Boolean
            Public Property Mensaje As String
            Public Property Detalle As String
        End Class

        Public Shared Function EnviarCorreo(
            ByVal destinatarios As List(Of String),
            ByVal asunto As String,
            ByVal cuerpoHtml As String
        ) As EmailResult

            Dim resultado As New EmailResult()
            resultado.Ok = False
            resultado.Enabled = EstaHabilitado()

            If Not resultado.Enabled Then
                resultado.Ok = True
                resultado.Mensaje = "Envío SMTP deshabilitado por configuración PLD_SMTP_ENABLED=false."
                resultado.Detalle = "No se intentó enviar correo."
                Return resultado
            End If

            If destinatarios Is Nothing OrElse destinatarios.Count = 0 Then
                resultado.Mensaje = "No hay destinatarios para enviar correo."
                Return resultado
            End If

            Dim host As String = AppSetting("PLD_SMTP_HOST")
            Dim puerto As Integer = AppSettingInt("PLD_SMTP_PORT", 587)
            Dim usarStartTls As Boolean = AppSettingBool("PLD_SMTP_STARTTLS", True)
            Dim usuario As String = AppSetting("PLD_SMTP_USER")
            Dim password As String = AppSetting("PLD_SMTP_PASSWORD")
            Dim fromEmail As String = AppSetting("PLD_SMTP_FROM")
            Dim fromName As String = AppSetting("PLD_SMTP_FROM_NAME")

            If String.IsNullOrWhiteSpace(host) Then
                resultado.Mensaje = "Falta configurar PLD_SMTP_HOST."
                Return resultado
            End If

            If String.IsNullOrWhiteSpace(fromEmail) Then
                resultado.Mensaje = "Falta configurar PLD_SMTP_FROM."
                Return resultado
            End If

            If String.IsNullOrWhiteSpace(fromName) Then
                fromName = "Sistema PLD"
            End If

            Try
                Using mensaje As New MailMessage()
                    mensaje.From = New MailAddress(fromEmail, fromName, Encoding.UTF8)
                    mensaje.Subject = If(asunto, "").Trim()
                    mensaje.SubjectEncoding = Encoding.UTF8
                    mensaje.Body = If(cuerpoHtml, "")
                    mensaje.BodyEncoding = Encoding.UTF8
                    mensaje.IsBodyHtml = True

                    For Each correo As String In destinatarios
                        Dim limpio As String = If(correo, "").Trim()

                        If Not String.IsNullOrWhiteSpace(limpio) Then
                            mensaje.To.Add(New MailAddress(limpio))
                        End If
                    Next

                    If mensaje.To.Count = 0 Then
                        resultado.Mensaje = "No hay destinatarios válidos para enviar correo."
                        Return resultado
                    End If

                    Using cliente As New SmtpClient(host, puerto)
                        cliente.EnableSsl = usarStartTls

                        If Not String.IsNullOrWhiteSpace(usuario) Then
                            cliente.Credentials = New NetworkCredential(usuario, password)
                        Else
                            cliente.UseDefaultCredentials = True
                        End If

                        cliente.Send(mensaje)
                    End Using
                End Using

                resultado.Ok = True
                resultado.Mensaje = "Correo enviado correctamente."
                resultado.Detalle = "Destinatarios: " & destinatarios.Count.ToString()

            Catch ex As Exception
                resultado.Ok = False
                resultado.Mensaje = "No se pudo enviar el correo."
                resultado.Detalle = ex.Message
            End Try

            Return resultado
        End Function

        Public Shared Function EnviarCorreoPrueba(ByVal destinatario As String) As EmailResult
            Dim lista As New List(Of String)()
            lista.Add(destinatario)

            Dim asunto As String = "Prueba de correo - Sistema PLD"

            Dim cuerpo As String =
                "<html>" &
                "<body style='font-family: Arial, sans-serif; font-size: 14px; color: #222;'>" &
                "<h3 style='color:#0b5ed7;'>Prueba de correo Sistema PLD</h3>" &
                "<p>Este es un correo de prueba para validar la configuración SMTP STARTTLS.</p>" &
                "<p><strong>Fecha:</strong> " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "</p>" &
                "<hr />" &
                "<p style='font-size:12px;color:#666;'>Sistema PLD</p>" &
                "</body>" &
                "</html>"

            Return EnviarCorreo(lista, asunto, cuerpo)
        End Function

        Public Shared Function EstaHabilitado() As Boolean
            Return AppSettingBool("PLD_SMTP_ENABLED", False)
        End Function

        Private Shared Function AppSetting(ByVal key As String) As String
            Dim valor As String = ConfigurationManager.AppSettings(key)

            If valor Is Nothing Then
                Return ""
            End If

            Return valor.Trim()
        End Function

        Private Shared Function AppSettingInt(ByVal key As String, ByVal defaultValue As Integer) As Integer
            Dim texto As String = AppSetting(key)
            Dim valor As Integer = defaultValue

            If Integer.TryParse(texto, valor) Then
                Return valor
            End If

            Return defaultValue
        End Function

        Private Shared Function AppSettingBool(ByVal key As String, ByVal defaultValue As Boolean) As Boolean
            Dim texto As String = AppSetting(key).ToLowerInvariant()

            If String.IsNullOrWhiteSpace(texto) Then
                Return defaultValue
            End If

            If texto = "1" OrElse texto = "true" OrElse texto = "si" OrElse texto = "sí" OrElse texto = "on" OrElse texto = "yes" Then
                Return True
            End If

            If texto = "0" OrElse texto = "false" OrElse texto = "no" OrElse texto = "off" Then
                Return False
            End If

            Return defaultValue
        End Function

    End Class

End Namespace