Imports System
Imports System.Web
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Web.Script.Serialization

Public Class utilidades : Implements IHttpHandler
    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ctx As HttpContext) Implements IHttpHandler.ProcessRequest
        ctx.Response.ContentType = "application/json; charset=utf-8"
        Try
            Dim action As String = (If(ctx.Request("action"), "")).Trim().ToLowerInvariant()
            Select Case action
                Case "ping" : WriteJson(ctx, New With {.ok = True, .message = "pong"})
                Case "validar_rfc" : HandleValidarRFC(ctx)
                Case "validar_curp" : HandleValidarCURP(ctx)
                Case "normalizar_numero" : HandleNormalizarNumero(ctx)  ' ← FIX
                Case "riesgo_texto_a_numero" : HandleRiesgoTextoANumero(ctx)
                Case "fecha_iso" : HandleFechaISO(ctx)
                Case Else
                    WriteError(ctx, "Acción no soportada. Usa: ping | validar_rfc | validar_curp | normalizar_numero | riesgo_texto_a_numero | fecha_iso")
            End Select
        Catch ex As Exception
            WriteError(ctx, "Excepción no controlada: " & ex.Message)
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '==================== Helpers ====================
    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String)
        ctx.Response.StatusCode = 200
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    Private Function RiesgoTextoANumero(valor As String) As Decimal
        If String.IsNullOrWhiteSpace(valor) Then Return 0D
        Dim v = valor.Trim().ToUpperInvariant()
        Select Case v
            Case "BAJO" : Return 1D
            Case "MEDIO" : Return 2D
            Case "ALTO" : Return 3D
        End Select
        Dim clean = v.Replace(",", ".")
        Dim d As Decimal
        If Decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return d
        Return 0D
    End Function

    '==================== Acciones ====================

    ' RFC (estructura)
    Private Sub HandleValidarRFC(ctx As HttpContext)
        Dim rfc As String = If(ctx.Request("rfc"), "")
        If String.IsNullOrWhiteSpace(rfc) Then
            WriteError(ctx, "rfc es requerido.") : Exit Sub
        End If
        Dim RFC_REGEX As New Regex("^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$", RegexOptions.IgnoreCase)
        Dim ok As Boolean = RFC_REGEX.IsMatch(rfc.Trim().ToUpperInvariant())
        WriteJson(ctx, New With {.ok = ok, .rfc = rfc.Trim(), .valido = ok})
    End Sub

    ' CURP (estructura)
    Private Sub HandleValidarCURP(ctx As HttpContext)
        Dim curp As String = If(ctx.Request("curp"), "")
        If String.IsNullOrWhiteSpace(curp) Then
            WriteError(ctx, "curp es requerido.") : Exit Sub
        End If
        Dim CURP_REGEX As New Regex("^[A-Z][AEIOUX][A-Z]{2}\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])[HM](AS|BC|BS|CC|CL|CM|CS|CH|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)[B-DF-HJ-NP-TV-Z]{3}[A-Z\d]\d$",
                                       RegexOptions.IgnoreCase)
        Dim ok As Boolean = CURP_REGEX.IsMatch(curp.Trim().ToUpperInvariant())
        WriteJson(ctx, New With {.ok = ok, .curp = curp.Trim(), .valido = ok})
    End Sub

    ' Normaliza texto numérico a decimal invariante.
    ' Acepta: "1,234.56", "1.234,56", "1234,56", "1234.56", " ALTO "→3, "MEDIO"→2, "BAJO"→1
    Private Sub HandleNormalizarNumero(ctx As HttpContext)
        Dim txt As String = If(ctx.Request("valor"), "")
        If String.IsNullOrWhiteSpace(txt) Then
            WriteError(ctx, "valor es requerido.") : Exit Sub
        End If

        ' 1) Intento directo por mapeo ALTO/MEDIO/BAJO
        Dim mapped As Decimal = RiesgoTextoANumero(txt)
        If mapped <> 0D OrElse txt.Trim().Equals("0", StringComparison.OrdinalIgnoreCase) Then
            WriteJson(ctx, New With {.ok = True, .valor = mapped}) : Exit Sub
        End If

        ' 2) Sanitiza: deja solo dígitos, coma, punto y signo menos
        Dim raw As String = Regex.Replace(txt, "[^\d\.,\-]", "").Trim()

        If String.IsNullOrEmpty(raw) Then
            WriteError(ctx, "No se pudo interpretar el número.") : Exit Sub
        End If

        ' 3) Heurística de separadores
        Dim guess As Decimal
        Dim success As Boolean = False

        ' Caso ES: "1.234,56" ó "1234,56" (coma como separador decimal)
        Dim posComma As Integer = raw.LastIndexOf(","c)
        Dim posDot As Integer = raw.LastIndexOf("."c)
        Dim commaIsDecimal As Boolean = (posComma > posDot)

        If commaIsDecimal AndAlso posComma >= 0 Then
            ' quita miles ".", convierte "," -> "."
            Dim normalized As String = raw.Replace(".", "").Replace(",", ".")
            success = Decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, guess)
        Else
            ' EN: "1,234.56" ó "1234.56" (punto como decimal)
            Dim normalized As String = raw.Replace(",", "")
            success = Decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, guess)
        End If

        If Not success Then
            WriteError(ctx, "No se pudo interpretar el número.") : Exit Sub
        End If

        WriteJson(ctx, New With {.ok = True, .valor = Math.Round(guess, 4)})
    End Sub

    ' BAJO/MEDIO/ALTO → 1/2/3, si no, intenta decimal
    Private Sub HandleRiesgoTextoANumero(ctx As HttpContext)
        Dim txt As String = If(ctx.Request("valor"), "")
        If String.IsNullOrWhiteSpace(txt) Then
            WriteError(ctx, "valor es requerido.") : Exit Sub
        End If
        Dim num As Decimal = RiesgoTextoANumero(txt)
        WriteJson(ctx, New With {.ok = True, .valor = num})
    End Sub

    ' Valida/convierte fecha a ISO (YYYY-MM-DD)
    Private Sub HandleFechaISO(ctx As HttpContext)
        Dim f As String = If(ctx.Request("fecha"), "")
        If String.IsNullOrWhiteSpace(f) Then
            WriteError(ctx, "fecha es requerida.") : Exit Sub
        End If

        Dim dt As DateTime
        Dim ok As Boolean = DateTime.TryParse(f, New CultureInfo("es-MX"), DateTimeStyles.AssumeLocal, dt) OrElse
                            DateTime.TryParse(f, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, dt)
        If Not ok Then
            WriteError(ctx, "Formato de fecha inválido.") : Exit Sub
        End If

        WriteJson(ctx, New With {.ok = True, .iso = dt.ToString("yyyy-MM-dd")})
    End Sub

End Class
