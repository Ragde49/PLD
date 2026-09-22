Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Web
Imports System.Web.Script.Serialization

Public Class handler_config_umbrales_pld
    Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"

        Dim serializer As New JavaScriptSerializer()
        Dim action As String = (If(context.Request("action"), "")).Trim().ToLower()

        Try
            If String.IsNullOrEmpty(action) Then action = "obtener"

            Select Case action
                Case "obtener"
                    Dim data = ObtenerConfig(context)
                    context.Response.Write(serializer.Serialize(New With {.ok = True, .data = data}))

                Case "guardar"
                    GuardarConfig(context)
                    context.Response.Write(serializer.Serialize(New With {.ok = True, .msg = "Umbrales PLD guardados correctamente."}))

                Case Else
                    context.Response.StatusCode = 400
                    context.Response.Write(serializer.Serialize(New With {.ok = False, .msg = "Acción no válida."}))
            End Select

        Catch ex As Exception
            context.Response.StatusCode = 500
            context.Response.Write(serializer.Serialize(New With {.ok = False, .msg = "Error en el servidor.", .detail = ex.Message}))
        End Try
    End Sub

    Private Function ObtenerConfig(context As HttpContext) As Dictionary(Of String, Object)
        Dim result As New Dictionary(Of String, Object)()

        Using cn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            cn.Open()

            ' Asegura que exista el registro id=1 (por si en algún ambiente no corrió el seed)
            Using cmdEnsure As New SqlCommand("
IF NOT EXISTS(SELECT 1 FROM dbo.config_umbrales_pld WHERE id=1)
BEGIN
    INSERT INTO dbo.config_umbrales_pld
    (
        id, organo_supervisor, clave_sujeto_obligado,
        id_oficial, correo_oficial, oficial_cumplimiento,
        pcnt_minimo_inusual_rebasa_pagos, pcnt_minimo_inusual_aumento_ingresos, pagos_acumulacion,
        persona_fisica_usd, persona_moral_usd,
        minimo_usd, minimo_mxnd,
        boes_personas_fisicas, boes_personas_morales, boes_personas_fisicas_usd, boes_personas_morales_usd,
        max_clasificar_microcredito_fisicas_udis, max_clasificar_microcredito_pfae_udis, max_clasificar_microcredito_morales_udis,
        periodicidad_dias_operaciones_inusuales, periodicidad_dias_internas_preocupantes,
        modificado_por
    )
    VALUES
    (
        1, '01002', '695583',
        NULL, NULL, NULL,
        0, 0, 0,
        0, 0,
        0, 0,
        0, 0, 0, 0,
        0, 0, 0,
        0, 0,
        NULL
    )
END
", cn)
                cmdEnsure.ExecuteNonQuery()
            End Using

            Using cmd As New SqlCommand("
SELECT TOP 1
    id,
    organo_supervisor,
    clave_sujeto_obligado,
    id_oficial,
    correo_oficial,
    oficial_cumplimiento,
    pcnt_minimo_inusual_rebasa_pagos,
    pcnt_minimo_inusual_aumento_ingresos,
    pagos_acumulacion,
    persona_fisica_usd,
    persona_moral_usd,
    minimo_usd,
    minimo_mxnd,
    boes_personas_fisicas,
    boes_personas_morales,
    boes_personas_fisicas_usd,
    boes_personas_morales_usd,
    max_clasificar_microcredito_fisicas_udis,
    max_clasificar_microcredito_pfae_udis,
    max_clasificar_microcredito_morales_udis,
    periodicidad_dias_operaciones_inusuales,
    periodicidad_dias_internas_preocupantes,
    fecha_modificacion,
    modificado_por
FROM dbo.config_umbrales_pld
WHERE id = 1
", cn)
                Using dr As SqlDataReader = cmd.ExecuteReader()
                    If dr.Read() Then
                        For i As Integer = 0 To dr.FieldCount - 1
                            Dim key As String = dr.GetName(i)
                            Dim val As Object = If(dr.IsDBNull(i), Nothing, dr.GetValue(i))
                            result(key) = val
                        Next
                    End If
                End Using
            End Using
        End Using

        Return result
    End Function

    Private Sub GuardarConfig(context As HttpContext)
        Dim organo_supervisor As String = SafeText(context.Request("organo_supervisor"), 6)
        Dim clave_sujeto_obligado As String = SafeText(context.Request("clave_sujeto_obligado"), 7)

        Dim id_oficial As Integer? = SafeIntNullable(context.Request("id_oficial"))
        Dim correo_oficial As String = SafeTextNullable(context.Request("correo_oficial"), 150)
        Dim oficial_cumplimiento As String = SafeTextNullable(context.Request("oficial_cumplimiento"), 150)

        Dim pcnt_minimo_inusual_rebasa_pagos As Decimal = SafeDec(context.Request("pcnt_minimo_inusual_rebasa_pagos"))
        Dim pcnt_minimo_inusual_aumento_ingresos As Decimal = SafeDec(context.Request("pcnt_minimo_inusual_aumento_ingresos"))
        Dim pagos_acumulacion As Decimal = SafeDec(context.Request("pagos_acumulacion"))

        Dim persona_fisica_usd As Decimal = SafeDec(context.Request("persona_fisica_usd"))
        Dim persona_moral_usd As Decimal = SafeDec(context.Request("persona_moral_usd"))

        Dim minimo_usd As Decimal = SafeDec(context.Request("minimo_usd"))
        Dim minimo_mxnd As Decimal = SafeDec(context.Request("minimo_mxnd"))

        Dim boes_personas_fisicas As Decimal = SafeDec(context.Request("boes_personas_fisicas"))
        Dim boes_personas_morales As Decimal = SafeDec(context.Request("boes_personas_morales"))
        Dim boes_personas_fisicas_usd As Decimal = SafeDec(context.Request("boes_personas_fisicas_usd"))
        Dim boes_personas_morales_usd As Decimal = SafeDec(context.Request("boes_personas_morales_usd"))

        Dim max_clasificar_microcredito_fisicas_udis As Integer = SafeInt(context.Request("max_clasificar_microcredito_fisicas_udis"))
        Dim max_clasificar_microcredito_pfae_udis As Integer = SafeInt(context.Request("max_clasificar_microcredito_pfae_udis"))
        Dim max_clasificar_microcredito_morales_udis As Integer = SafeInt(context.Request("max_clasificar_microcredito_morales_udis"))

        Dim periodicidad_dias_operaciones_inusuales As Integer = SafeInt(context.Request("periodicidad_dias_operaciones_inusuales"))
        Dim periodicidad_dias_internas_preocupantes As Integer = SafeInt(context.Request("periodicidad_dias_internas_preocupantes"))

        Dim modificado_por As String = Nothing
        Try
            If context.Session IsNot Nothing AndAlso context.Session("usuario") IsNot Nothing Then
                modificado_por = Convert.ToString(context.Session("usuario"))
            ElseIf context.User IsNot Nothing AndAlso context.User.Identity IsNot Nothing AndAlso Not String.IsNullOrEmpty(context.User.Identity.Name) Then
                modificado_por = context.User.Identity.Name
            End If
        Catch
            modificado_por = Nothing
        End Try

        Using cn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            cn.Open()

            Using cmd As New SqlCommand("
UPDATE dbo.config_umbrales_pld
SET
    organo_supervisor = @organo_supervisor,
    clave_sujeto_obligado = @clave_sujeto_obligado,
    id_oficial = @id_oficial,
    correo_oficial = @correo_oficial,
    oficial_cumplimiento = @oficial_cumplimiento,
    pcnt_minimo_inusual_rebasa_pagos = @pcnt_minimo_inusual_rebasa_pagos,
    pcnt_minimo_inusual_aumento_ingresos = @pcnt_minimo_inusual_aumento_ingresos,
    pagos_acumulacion = @pagos_acumulacion,
    persona_fisica_usd = @persona_fisica_usd,
    persona_moral_usd = @persona_moral_usd,
    minimo_usd = @minimo_usd,
    minimo_mxnd = @minimo_mxnd,
    boes_personas_fisicas = @boes_personas_fisicas,
    boes_personas_morales = @boes_personas_morales,
    boes_personas_fisicas_usd = @boes_personas_fisicas_usd,
    boes_personas_morales_usd = @boes_personas_morales_usd,
    max_clasificar_microcredito_fisicas_udis = @max_clasificar_microcredito_fisicas_udis,
    max_clasificar_microcredito_pfae_udis = @max_clasificar_microcredito_pfae_udis,
    max_clasificar_microcredito_morales_udis = @max_clasificar_microcredito_morales_udis,
    periodicidad_dias_operaciones_inusuales = @periodicidad_dias_operaciones_inusuales,
    periodicidad_dias_internas_preocupantes = @periodicidad_dias_internas_preocupantes,
    fecha_modificacion = GETDATE(),
    modificado_por = @modificado_por
WHERE id = 1
", cn)

                cmd.Parameters.AddWithValue("@organo_supervisor", organo_supervisor)
                cmd.Parameters.AddWithValue("@clave_sujeto_obligado", clave_sujeto_obligado)

                If id_oficial.HasValue Then
                    cmd.Parameters.AddWithValue("@id_oficial", id_oficial.Value)
                Else
                    cmd.Parameters.Add("@id_oficial", SqlDbType.Int).Value = DBNull.Value
                End If

                If String.IsNullOrEmpty(correo_oficial) Then
                    cmd.Parameters.Add("@correo_oficial", SqlDbType.VarChar, 150).Value = DBNull.Value
                Else
                    cmd.Parameters.AddWithValue("@correo_oficial", correo_oficial)
                End If

                If String.IsNullOrEmpty(oficial_cumplimiento) Then
                    cmd.Parameters.Add("@oficial_cumplimiento", SqlDbType.VarChar, 150).Value = DBNull.Value
                Else
                    cmd.Parameters.AddWithValue("@oficial_cumplimiento", oficial_cumplimiento)
                End If

                cmd.Parameters.AddWithValue("@pcnt_minimo_inusual_rebasa_pagos", pcnt_minimo_inusual_rebasa_pagos)
                cmd.Parameters.AddWithValue("@pcnt_minimo_inusual_aumento_ingresos", pcnt_minimo_inusual_aumento_ingresos)
                cmd.Parameters.AddWithValue("@pagos_acumulacion", pagos_acumulacion)

                cmd.Parameters.AddWithValue("@persona_fisica_usd", persona_fisica_usd)
                cmd.Parameters.AddWithValue("@persona_moral_usd", persona_moral_usd)

                cmd.Parameters.AddWithValue("@minimo_usd", minimo_usd)
                cmd.Parameters.AddWithValue("@minimo_mxnd", minimo_mxnd)

                cmd.Parameters.AddWithValue("@boes_personas_fisicas", boes_personas_fisicas)
                cmd.Parameters.AddWithValue("@boes_personas_morales", boes_personas_morales)
                cmd.Parameters.AddWithValue("@boes_personas_fisicas_usd", boes_personas_fisicas_usd)
                cmd.Parameters.AddWithValue("@boes_personas_morales_usd", boes_personas_morales_usd)

                cmd.Parameters.AddWithValue("@max_clasificar_microcredito_fisicas_udis", max_clasificar_microcredito_fisicas_udis)
                cmd.Parameters.AddWithValue("@max_clasificar_microcredito_pfae_udis", max_clasificar_microcredito_pfae_udis)
                cmd.Parameters.AddWithValue("@max_clasificar_microcredito_morales_udis", max_clasificar_microcredito_morales_udis)

                cmd.Parameters.AddWithValue("@periodicidad_dias_operaciones_inusuales", periodicidad_dias_operaciones_inusuales)
                cmd.Parameters.AddWithValue("@periodicidad_dias_internas_preocupantes", periodicidad_dias_internas_preocupantes)

                If String.IsNullOrEmpty(modificado_por) Then
                    cmd.Parameters.Add("@modificado_por", SqlDbType.VarChar, 100).Value = DBNull.Value
                Else
                    cmd.Parameters.AddWithValue("@modificado_por", modificado_por)
                End If

                Dim rows As Integer = cmd.ExecuteNonQuery()
                If rows <= 0 Then
                    Throw New Exception("No se pudo actualizar el registro de configuración (id=1).")
                End If
            End Using
        End Using
    End Sub

    ' ---------------- Helpers ----------------

    Private Function SafeText(value As String, maxLen As Integer) As String
        Dim v As String = If(value, "").Trim()
        If v.Length > maxLen Then v = v.Substring(0, maxLen)
        Return v
    End Function

    Private Function SafeTextNullable(value As String, maxLen As Integer) As String
        Dim v As String = If(value, "").Trim()
        If v = "" Then Return Nothing
        If v.Length > maxLen Then v = v.Substring(0, maxLen)
        Return v
    End Function

    Private Function SafeInt(value As String) As Integer
        Dim n As Integer = 0
        Integer.TryParse((If(value, "")).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, n)
        If n < 0 Then n = 0
        Return n
    End Function

    Private Function SafeIntNullable(value As String) As Integer?
        Dim raw As String = (If(value, "")).Trim()
        If raw = "" Then Return Nothing
        Dim n As Integer = 0
        If Integer.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            If n < 0 Then n = 0
            Return n
        End If
        Return Nothing
    End Function

    Private Function SafeDec(value As String) As Decimal
        Dim raw As String = (If(value, "")).Trim()
        If raw = "" Then Return 0D

        ' Normaliza: quita separadores de miles y fuerza punto decimal
        raw = raw.Replace(" ", "")
        raw = raw.Replace(",", "") ' asume formato 1,000.00 o 100,000
        Dim d As Decimal = 0D
        Decimal.TryParse(raw, NumberStyles.Number Or NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, d)
        If d < 0D Then d = 0D
        Return d
    End Function

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
