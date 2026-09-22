Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization

Public Class clientes_handler
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ctx As HttpContext) Implements IHttpHandler.ProcessRequest
        ctx.Response.ContentType = "application/json; charset=utf-8"
        Try
            Dim action As String = (If(ctx.Request("action"), "")).Trim().ToLowerInvariant()
            Select Case action
                Case "crear"
                    HandleCrear(ctx)
                Case "actualizar"
                    HandleActualizar(ctx)
                Case "buscar", "listar"
                    HandleBuscar(ctx)
                Case "obtener"
                    HandleObtener(ctx)
                Case "ping"
                    WriteJson(ctx, New With {.ok = True, .message = "pong"})
                Case Else
                    WriteError(ctx, "Acción no soportada. Usa: crear | actualizar | buscar | listar | obtener | ping")
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

    '=================== Helpers ===================

    Private Function GetConn() As SqlConnection
        Return New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
    End Function

    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String)
        ctx.Response.StatusCode = 200
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    Private Function NowUser() As String
        Dim u = HttpContext.Current.User
        If u IsNot Nothing AndAlso u.Identity IsNot Nothing AndAlso u.Identity.IsAuthenticated Then
            Return u.Identity.Name
        End If
        Return "system"
    End Function

    Private Function ToStr(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Return value.Trim()
    End Function

    Private Function ToDate(value As String) As DateTime?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim dt As DateTime
        If DateTime.TryParse(value.Trim(), New CultureInfo("es-MX"), DateTimeStyles.AssumeLocal, dt) _
           OrElse DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, dt) Then
            Return dt
        End If
        Return Nothing
    End Function

    Private Function ToDec(value As String) As Decimal?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim clean = Regex.Replace(value.Trim(), "[^\d\.,\-]", "")
        Dim posComma = clean.LastIndexOf(","c)
        Dim posDot = clean.LastIndexOf("."c)
        Dim commaIsDecimal = (posComma > posDot)
        Dim normalized As String = If(commaIsDecimal, clean.Replace(".", "").Replace(",", "."), clean.Replace(",", ""))
        Dim d As Decimal
        If Decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then Return d
        Return Nothing
    End Function

    ' ---- leer JSON del body si viene application/json ----
    Private Function GetJsonBody(ctx As HttpContext) As Dictionary(Of String, Object)
        Try
            If ctx Is Nothing OrElse ctx.Request Is Nothing OrElse ctx.Request.InputStream Is Nothing Then
                Return Nothing
            End If
            If ctx.Request.InputStream.CanSeek Then
                ctx.Request.InputStream.Position = 0
            End If
            Using reader As New System.IO.StreamReader(ctx.Request.InputStream, Text.Encoding.UTF8)
                Dim raw = reader.ReadToEnd()
                If String.IsNullOrWhiteSpace(raw) Then Return Nothing
                Return serializer.Deserialize(Of Dictionary(Of String, Object))(raw)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    ' helper para leer campo desde form o JSON
    Private Function GetField(ctx As HttpContext, json As Dictionary(Of String, Object), key As String) As String
        Dim v As String = ToStr(ctx.Request(key))
        If String.IsNullOrEmpty(v) AndAlso json IsNot Nothing AndAlso json.ContainsKey(key) AndAlso json(key) IsNot Nothing Then
            v = ToStr(Convert.ToString(json(key), CultureInfo.InvariantCulture))
        End If
        Return v
    End Function

    ' ===================== RFC CORREGIDO ===================== 
    Private Function ValidRFC(rfc As String) As Boolean
        If String.IsNullOrWhiteSpace(rfc) Then Return False

        ' Normalizar: quitar espacios y guiones
        Dim limpio As String = Regex.Replace(rfc.Trim().ToUpperInvariant(), "[\s\-]", "")

        ' Longitud SAT válida (PF: 13, PM: 12)
        If limpio.Length <> 12 AndAlso limpio.Length <> 13 Then
            Return False
        End If

        ' Regla general SAT PF/PM
        Dim re As New Regex("^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$")
        Return re.IsMatch(limpio)
    End Function
    ' =========================================================

    Private Function ValidCURP(curp As String) As Boolean
        If String.IsNullOrWhiteSpace(curp) Then Return True ' opcional en la práctica
        Dim re As New Regex("^[A-Z][AEIOUX][A-Z]{2}\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])[HM](AS|BC|BS|CC|CL|CM|CS|CH|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)[B-DF-HJ-NP-TV-Z]{3}[A-Z\d]\d$", RegexOptions.IgnoreCase)
        Return re.IsMatch(curp.Trim().ToUpperInvariant())
    End Function

    Private Sub ReadPaging(ctx As HttpContext, ByRef page As Integer, ByRef pageSize As Integer, Optional defaultPage As Integer = 1, Optional defaultSize As Integer = 20)
        page = defaultPage : pageSize = defaultSize
        Integer.TryParse(If(ctx.Request("page"), defaultPage.ToString()), page)
        Integer.TryParse(If(ctx.Request("pageSize"), defaultSize.ToString()), pageSize)
        If page < 1 Then page = 1
        If pageSize < 1 Then pageSize = defaultSize
    End Sub

    '=================== ACCIONES ===================

    Private Function ToInt(value As String) As Integer?
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Dim n As Integer
        If Integer.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            Return n
        End If
        Return Nothing
    End Function


    ' ==================== CREAR (cliente_persona_fisica) ====================
    Private Sub HandleCrear(ctx As HttpContext)

        ' Leer JSON si viene application/json
        Dim json As Dictionary(Of String, Object) = Nothing
        Dim ct As String = If(ctx.Request.ContentType, "").ToLowerInvariant()
        If ct.Contains("application/json") Then
            json = GetJsonBody(ctx)
        End If

        ' ===== RFC obligatorio =====
        Dim rfcEntrada As String = GetField(ctx, json, "rfc")
        Dim rfc As String = Nothing
        If Not String.IsNullOrWhiteSpace(rfcEntrada) Then
            rfc = Regex.Replace(rfcEntrada.Trim().ToUpperInvariant(), "[\s\-]", "")
        End If
        If String.IsNullOrEmpty(rfc) OrElse Not ValidRFC(rfc) Then
            WriteError(ctx, "RFC inválido o vacío.") : Exit Sub
        End If

        ' ===== Datos PF =====
        Dim primerNombre = GetField(ctx, json, "primer_nombre")
        Dim segundoNombre = GetField(ctx, json, "segundo_nombre")
        Dim apPaterno = GetField(ctx, json, "ap_paterno")
        Dim apMaterno = GetField(ctx, json, "ap_materno")
        Dim fechaNac = ToDate(GetField(ctx, json, "fecha_nacimiento"))
        Dim sexo = GetField(ctx, json, "sexo")
        Dim curp = GetField(ctx, json, "curp")
        Dim pep As Boolean = False
        Dim pepRaw As String = GetField(ctx, json, "pep")
        If Not String.IsNullOrWhiteSpace(pepRaw) Then
            pep = (pepRaw = "1" OrElse pepRaw.Equals("true", StringComparison.OrdinalIgnoreCase))
        End If
        Dim estadoCivil = GetField(ctx, json, "estado_civil")

        ' Regimen / dependientes / escolaridad / antigüedad laboral
        Dim regimenMatrimonial = GetField(ctx, json, "regimen_matrimonial")
        Dim dependientesVal = GetField(ctx, json, "dependientes")
        Dim dependientes As Integer? = ToInt(dependientesVal)
        Dim escolaridad = GetField(ctx, json, "escolaridad")
        Dim antigMesesVal = GetField(ctx, json, "antiguedad_meses")
        Dim antiguedadMeses As Integer? = ToInt(antigMesesVal)

        ' ===== Nacionalidad y lugar de nacimiento =====
        Dim nacionalidadTxt = GetField(ctx, json, "nacionalidad")
        Dim nacId = ToInt(GetField(ctx, json, "nacionalidad_id"))

        Dim paisNacTxt = GetField(ctx, json, "pais_nacimiento")
        Dim paisNacId = ToInt(GetField(ctx, json, "pais_nacimiento_id"))

        Dim edoNacTxt = GetField(ctx, json, "entidad_nacimiento")
        Dim edoNacId = ToInt(GetField(ctx, json, "estado_nacimiento_id"))

        ' ===== Domicilio =====
        Dim calle = GetField(ctx, json, "calle")
        Dim numExt = GetField(ctx, json, "numero_exterior")
        Dim numInt = GetField(ctx, json, "numero_interior")
        Dim colonia = GetField(ctx, json, "colonia")
        Dim cp = GetField(ctx, json, "codigo_postal")
        Dim municipio = GetField(ctx, json, "municipio")
        Dim estado = GetField(ctx, json, "estado")
        Dim ciudad = GetField(ctx, json, "ciudad")
        Dim referencias = GetField(ctx, json, "referencias")
        Dim pais = GetField(ctx, json, "pais")

        Dim paisDomId = ToInt(GetField(ctx, json, "pais_domicilio_id"))
        Dim edoDomId = ToInt(GetField(ctx, json, "estado_domicilio_id"))
        Dim munDomId = ToInt(GetField(ctx, json, "municipio_domicilio_id"))

        ' ===== Contacto =====
        Dim telCasa = GetField(ctx, json, "telefono_casa")
        Dim celular = GetField(ctx, json, "celular")
        Dim email = GetField(ctx, json, "email")

        ' ===== Laboral / actividad =====
        Dim empresa = GetField(ctx, json, "empresa")
        Dim puesto = GetField(ctx, json, "puesto")
        Dim antigTexto = GetField(ctx, json, "antiguedad_empleo") ' si algún día se usa texto, aquí va
        Dim telEmpresa = GetField(ctx, json, "telefono_empresa")
        Dim emailTrabajo = GetField(ctx, json, "email_trabajo")

        Dim actEcoTxt = GetField(ctx, json, "actividad_economica")
        Dim actEcoId = ToInt(GetField(ctx, json, "actividad_economica_id"))
        Dim ocupacionId = ToInt(GetField(ctx, json, "ocupacion_id"))

        ' ===== Ingresos =====
        Dim ingBrutos = ToDec(GetField(ctx, json, "ingresos_brutos"))
        Dim ingNetos = ToDec(GetField(ctx, json, "ingresos_netos"))
        Dim otrosIng = ToDec(GetField(ctx, json, "otros_ingresos"))
        Dim egresos = ToDec(GetField(ctx, json, "egresos"))
        Dim ingresoMensual = ToDec(GetField(ctx, json, "ingreso_mensual"))
        Dim perfilPagosMensuales = ToInt(GetField(ctx, json, "perfil_pagos_mensuales_esperados"))
        Dim perfilMontoMensual = ToDec(GetField(ctx, json, "perfil_monto_mensual_esperado"))

        If perfilPagosMensuales.HasValue AndAlso perfilPagosMensuales.Value < 0 Then
            WriteError(ctx, "El número esperado de pagos por mes no puede ser negativo.") : Exit Sub
        End If
        If perfilMontoMensual.HasValue AndAlso perfilMontoMensual.Value < 0D Then
            WriteError(ctx, "El monto mensual esperado no puede ser negativo.") : Exit Sub
        End If

        ' ===== Nuevos campos PLD / consentimiento / identificación =====
        Dim origenOtros = GetField(ctx, json, "origen_otros_ingresos")

        Dim aceptaAviso As Boolean = False
        Dim aceptaRaw = GetField(ctx, json, "acepta_aviso_privacidad")
        If String.IsNullOrEmpty(aceptaRaw) Then
            ' el front manda "acepta_avisos"
            aceptaRaw = GetField(ctx, json, "acepta_avisos")
        End If
        If Not String.IsNullOrEmpty(aceptaRaw) Then
            aceptaAviso = (aceptaRaw = "1" OrElse aceptaRaw.Equals("true", StringComparison.OrdinalIgnoreCase))
        End If

        Dim tipoIdent = GetField(ctx, json, "tipo_identificacion")
        If String.IsNullOrEmpty(tipoIdent) Then
            tipoIdent = GetField(ctx, json, "ident_tipo")
        End If

        Dim numIdent = GetField(ctx, json, "numero_identificacion")
        If String.IsNullOrEmpty(numIdent) Then
            numIdent = GetField(ctx, json, "ident_numero")
        End If

        Dim vigStr = GetField(ctx, json, "vigencia_identificacion")
        If String.IsNullOrEmpty(vigStr) Then
            vigStr = GetField(ctx, json, "ident_vigencia")
        End If
        Dim vigIdent = ToDate(vigStr)

        ' ===== Validaciones mínimas =====
        If String.IsNullOrEmpty(primerNombre) OrElse String.IsNullOrEmpty(apPaterno) Then
            WriteError(ctx, "Primer nombre y apellido paterno son obligatorios.") : Exit Sub
        End If
        If String.IsNullOrWhiteSpace(curp) Then
            WriteError(ctx, "La CURP es obligatoria.") : Exit Sub
        End If
        If String.IsNullOrWhiteSpace(curp) Then
            WriteError(ctx, "La CURP es obligatoria.") : Exit Sub
        End If
        If Not fechaNac.HasValue Then
            WriteError(ctx, "La fecha de nacimiento es obligatoria.") : Exit Sub
        End If
        If String.IsNullOrEmpty(sexo) Then
            WriteError(ctx, "El sexo es obligatorio.") : Exit Sub
        End If
        If String.IsNullOrEmpty(estadoCivil) Then
            WriteError(ctx, "El estado civil es obligatorio.") : Exit Sub
        End If

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    ' Validar duplicado por RFC
                    Using v As New SqlCommand("SELECT 1 FROM cliente_persona_fisica WHERE rfc=@rfc", con, tx)
                        v.Parameters.Add("@rfc", SqlDbType.VarChar, 13).Value = rfc
                        If v.ExecuteScalar() IsNot Nothing Then
                            WriteError(ctx, "Ya existe un cliente con ese RFC.") : tx.Rollback() : Exit Sub
                        End If
                    End Using

                    Dim sql As String = "
                    INSERT INTO cliente_persona_fisica
                    (primer_nombre, segundo_nombre, apellido_paterno, apellido_materno, sexo, fecha_nacimiento, estado_civil,
                     curp, rfc, nacionalidad, puesto_politico, calle, numero_exterior, numero_interior, colonia, codigo_postal, municipio, estado,
                     ciudad, referencias, pais, telefono_casa, celular, email, empresa, puesto, antiguedad_empleo,
                     ingresos_brutos, ingresos_netos, otros_ingresos, egresos, telefono_empresa, email_trabajo,
                     origen_otros_ingresos, acepta_aviso_privacidad, tipo_identificacion, numero_identificacion, vigencia_identificacion,
                     nacionalidad_id, pais_nacimiento_id, estado_nacimiento_id, ocupacion_id, actividad_economica_id,
                     pais_domicilio_id, estado_domicilio_id, municipio_domicilio_id, ingreso_mensual,
                     perfil_pagos_mensuales_esperados, perfil_monto_mensual_esperado,
                     perfil_transaccional_modificado_por, perfil_transaccional_fecha_modificacion,
                     regimen_matrimonial, dependientes, escolaridad, antiguedad_meses, actividad_economica,
                     fecha_captura, usuario_captura)
                    VALUES
                    (@pnom,@snom,@apat,@amat,@sexo,@fnac,@ecivil,
                     @curp,@rfc,@nacionalidad,@pep,@calle,@numext,@numint,@col,@cp,@mun,@edo,
                     @ciudad,@refs,@pais,@tcasa,@cel,@mail,@emp,@puesto,@antig,
                     @ingb,@ingn,@otros,@egre,@telEmp,@mailEmp,
                     @origen,@aviso,@tident,@nident,@vigident,
                     @nid,@pid,@eid,@oid,@aeid,
                     @pdom,@edom,@mdom,@ingm,
                     @perfilPagos,@perfilMonto,@user,SYSDATETIME(),
                     @regimen,@dep,@esc,@antMeses,@actEcoTxt,
                     GETDATE(),@user);
                    SELECT SCOPE_IDENTITY();"

                    Dim newId As Integer

                    Using cmd As New SqlCommand(sql, con, tx)
                        ' Datos personales
                        cmd.Parameters.Add("@pnom", SqlDbType.VarChar, 100).Value = primerNombre
                        cmd.Parameters.Add("@snom", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(segundoNombre), CType(DBNull.Value, Object), segundoNombre)
                        cmd.Parameters.Add("@apat", SqlDbType.VarChar, 100).Value = apPaterno
                        cmd.Parameters.Add("@amat", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(apMaterno), CType(DBNull.Value, Object), apMaterno)
                        cmd.Parameters.Add("@sexo", SqlDbType.VarChar, 10).Value = sexo
                        cmd.Parameters.Add("@fnac", SqlDbType.Date).Value = fechaNac.Value
                        cmd.Parameters.Add("@ecivil", SqlDbType.VarChar, 20).Value = estadoCivil

                        cmd.Parameters.Add("@curp", SqlDbType.VarChar, 18).Value = If(String.IsNullOrEmpty(curp), CType(DBNull.Value, Object), curp)
                        cmd.Parameters.Add("@rfc", SqlDbType.VarChar, 13).Value = rfc
                        cmd.Parameters.Add("@nacionalidad", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(nacionalidadTxt), CType(DBNull.Value, Object), nacionalidadTxt)
                        cmd.Parameters.Add("@pep", SqlDbType.Bit).Value = If(pep, 1, 0)

                        ' Domicilio
                        cmd.Parameters.Add("@calle", SqlDbType.VarChar, 150).Value = If(String.IsNullOrEmpty(calle), CType(DBNull.Value, Object), calle)
                        cmd.Parameters.Add("@numext", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(numExt), CType(DBNull.Value, Object), numExt)
                        cmd.Parameters.Add("@numint", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(numInt), CType(DBNull.Value, Object), numInt)
                        cmd.Parameters.Add("@col", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(colonia), CType(DBNull.Value, Object), colonia)
                        cmd.Parameters.Add("@cp", SqlDbType.VarChar, 10).Value = If(String.IsNullOrEmpty(cp), CType(DBNull.Value, Object), cp)
                        cmd.Parameters.Add("@mun", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(municipio), CType(DBNull.Value, Object), municipio)
                        cmd.Parameters.Add("@edo", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(estado), CType(DBNull.Value, Object), estado)
                        cmd.Parameters.Add("@ciudad", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(ciudad), CType(DBNull.Value, Object), ciudad)
                        cmd.Parameters.Add("@refs", SqlDbType.VarChar, 250).Value = If(String.IsNullOrEmpty(referencias), CType(DBNull.Value, Object), referencias)
                        cmd.Parameters.Add("@pais", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(pais), CType(DBNull.Value, Object), pais)

                        ' Contacto
                        cmd.Parameters.Add("@tcasa", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(telCasa), CType(DBNull.Value, Object), telCasa)
                        cmd.Parameters.Add("@cel", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(celular), CType(DBNull.Value, Object), celular)
                        cmd.Parameters.Add("@mail", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(email), CType(DBNull.Value, Object), email)

                        ' Laboral
                        cmd.Parameters.Add("@emp", SqlDbType.VarChar, 150).Value = If(String.IsNullOrEmpty(empresa), CType(DBNull.Value, Object), empresa)
                        cmd.Parameters.Add("@puesto", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(puesto), CType(DBNull.Value, Object), puesto)
                        cmd.Parameters.Add("@antig", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(antigTexto), CType(DBNull.Value, Object), antigTexto)

                        ' Ingresos
                        cmd.Parameters.Add("@ingb", SqlDbType.Decimal).Value = If(ingBrutos.HasValue, CType(ingBrutos.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@ingn", SqlDbType.Decimal).Value = If(ingNetos.HasValue, CType(ingNetos.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@otros", SqlDbType.Decimal).Value = If(otrosIng.HasValue, CType(otrosIng.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@egre", SqlDbType.Decimal).Value = If(egresos.HasValue, CType(egresos.Value, Object), DBNull.Value)

                        cmd.Parameters.Add("@telEmp", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(telEmpresa), CType(DBNull.Value, Object), telEmpresa)
                        cmd.Parameters.Add("@mailEmp", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(emailTrabajo), CType(DBNull.Value, Object), emailTrabajo)

                        ' Nuevos campos / PLD
                        cmd.Parameters.Add("@origen", SqlDbType.VarChar, 200).Value = If(String.IsNullOrEmpty(origenOtros), CType(DBNull.Value, Object), origenOtros)
                        cmd.Parameters.Add("@aviso", SqlDbType.Bit).Value = If(aceptaAviso, 1, 0)
                        cmd.Parameters.Add("@tident", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(tipoIdent), CType(DBNull.Value, Object), tipoIdent)
                        cmd.Parameters.Add("@nident", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(numIdent), CType(DBNull.Value, Object), numIdent)
                        cmd.Parameters.Add("@vigident", SqlDbType.Date).Value = If(vigIdent.HasValue, CType(vigIdent.Value, Object), DBNull.Value)

                        ' IDs PLD
                        cmd.Parameters.Add("@nid", SqlDbType.Int).Value = If(nacId.HasValue, CType(nacId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@pid", SqlDbType.Int).Value = If(paisNacId.HasValue, CType(paisNacId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@eid", SqlDbType.Int).Value = If(edoNacId.HasValue, CType(edoNacId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@oid", SqlDbType.Int).Value = If(ocupacionId.HasValue, CType(ocupacionId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@aeid", SqlDbType.Int).Value = If(actEcoId.HasValue, CType(actEcoId.Value, Object), DBNull.Value)

                        cmd.Parameters.Add("@pdom", SqlDbType.Int).Value = If(paisDomId.HasValue, CType(paisDomId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@edom", SqlDbType.Int).Value = If(edoDomId.HasValue, CType(edoDomId.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@mdom", SqlDbType.Int).Value = If(munDomId.HasValue, CType(munDomId.Value, Object), DBNull.Value)

                        cmd.Parameters.Add("@ingm", SqlDbType.Decimal).Value = If(ingresoMensual.HasValue, CType(ingresoMensual.Value, Object), DBNull.Value)

                        cmd.Parameters.Add("@perfilPagos", SqlDbType.Int).Value = If(perfilPagosMensuales.HasValue, CType(perfilPagosMensuales.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@perfilMonto", SqlDbType.Decimal).Value = If(perfilMontoMensual.HasValue, CType(perfilMontoMensual.Value, Object), DBNull.Value)
                        cmd.Parameters("@perfilMonto").Precision = 18
                        cmd.Parameters("@perfilMonto").Scale = 2

                        ' Regimen / dependientes / escolaridad / antigüedad (meses) / actividad texto
                        cmd.Parameters.Add("@regimen", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(regimenMatrimonial), CType(DBNull.Value, Object), regimenMatrimonial)
                        cmd.Parameters.Add("@dep", SqlDbType.Int).Value = If(dependientes.HasValue, CType(dependientes.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@esc", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(escolaridad), CType(DBNull.Value, Object), escolaridad)
                        cmd.Parameters.Add("@antMeses", SqlDbType.Int).Value = If(antiguedadMeses.HasValue, CType(antiguedadMeses.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@actEcoTxt", SqlDbType.VarChar, 200).Value = If(String.IsNullOrEmpty(actEcoTxt), CType(DBNull.Value, Object), actEcoTxt)

                        ' Auditoría
                        cmd.Parameters.Add("@user", SqlDbType.VarChar, 100).Value = NowUser()

                        newId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Cliente creado.", .cliente_id = newId})

                Catch ex As Exception
                    tx.Rollback()
                    WriteError(ctx, "Error al crear cliente: " & ex.Message)
                End Try
            End Using
        End Using

    End Sub
    ' ================= FIN CREAR ===================

    ' ==================== ACTUALIZAR (cliente_persona_fisica) ====================
    Private Sub HandleActualizar(ctx As HttpContext)

        ' Leer JSON si viene application/json
        Dim json As Dictionary(Of String, Object) = Nothing
        Dim ct As String = If(ctx.Request.ContentType, "").ToLowerInvariant()
        If ct.Contains("application/json") Then
            json = GetJsonBody(ctx)
        End If

        ' ===== ID obligatorio =====
        Dim idStr As String = GetField(ctx, json, "id")
        If String.IsNullOrEmpty(idStr) Then
            idStr = GetField(ctx, json, "cliente_id")
        End If

        Dim cid As Integer
        If Not Integer.TryParse(idStr, cid) OrElse cid <= 0 Then
            WriteError(ctx, "id de cliente inválido.") : Exit Sub
        End If

        ' ===== RFC (normalizado) =====
        Dim rfcEntrada As String = GetField(ctx, json, "rfc")
        Dim rfc As String = Nothing
        If Not String.IsNullOrWhiteSpace(rfcEntrada) Then
            rfc = Regex.Replace(rfcEntrada.Trim().ToUpperInvariant(), "[\s\-]", "")
        End If
        If String.IsNullOrEmpty(rfc) OrElse Not ValidRFC(rfc) Then
            WriteError(ctx, "RFC inválido o vacío.") : Exit Sub
        End If

        ' ===== Datos personales =====
        Dim primerNombre = GetField(ctx, json, "primer_nombre")
        Dim segundoNombre = GetField(ctx, json, "segundo_nombre")
        Dim apPaterno = GetField(ctx, json, "ap_paterno")
        Dim apMaterno = GetField(ctx, json, "ap_materno")
        Dim fechaNac = ToDate(GetField(ctx, json, "fecha_nacimiento"))
        Dim sexo = GetField(ctx, json, "sexo")
        Dim curp = GetField(ctx, json, "curp")
        Dim pep As Boolean = False
        Dim pepRaw As String = GetField(ctx, json, "pep")
        If Not String.IsNullOrWhiteSpace(pepRaw) Then
            pep = (pepRaw = "1" OrElse pepRaw.Equals("true", StringComparison.OrdinalIgnoreCase))
        End If
        Dim estadoCivil = GetField(ctx, json, "estado_civil")

        ' Regimen / dependientes / escolaridad
        Dim regimenMatrimonial = GetField(ctx, json, "regimen_matrimonial")
        Dim dependientes As Integer? = ToInt(GetField(ctx, json, "dependientes"))
        Dim escolaridad = GetField(ctx, json, "escolaridad")
        Dim antiguedadMeses As Integer? = ToInt(GetField(ctx, json, "antiguedad_meses"))

        ' ===== Nacionalidad y lugar de nacimiento =====
        Dim nacionalidadTxt = GetField(ctx, json, "nacionalidad")
        Dim nacId = ToInt(GetField(ctx, json, "nacionalidad_id"))

        Dim paisNacTxt = GetField(ctx, json, "pais_nacimiento")
        Dim paisNacId = ToInt(GetField(ctx, json, "pais_nacimiento_id"))

        Dim edoNacTxt = GetField(ctx, json, "entidad_nacimiento")
        Dim edoNacId = ToInt(GetField(ctx, json, "estado_nacimiento_id"))

        ' ===== Domicilio =====
        Dim calle = GetField(ctx, json, "calle")
        Dim numExt = GetField(ctx, json, "numero_exterior")
        Dim numInt = GetField(ctx, json, "numero_interior")
        Dim colonia = GetField(ctx, json, "colonia")
        Dim cp = GetField(ctx, json, "codigo_postal")
        Dim municipio = GetField(ctx, json, "municipio")
        Dim estado = GetField(ctx, json, "estado")
        Dim ciudad = GetField(ctx, json, "ciudad")
        Dim referencias = GetField(ctx, json, "referencias")
        Dim pais = GetField(ctx, json, "pais")

        Dim paisDomId = ToInt(GetField(ctx, json, "pais_domicilio_id"))
        Dim edoDomId = ToInt(GetField(ctx, json, "estado_domicilio_id"))
        Dim munDomId = ToInt(GetField(ctx, json, "municipio_domicilio_id"))

        ' ===== Contacto =====
        Dim telCasa = GetField(ctx, json, "telefono_casa")
        Dim celular = GetField(ctx, json, "celular")
        Dim email = GetField(ctx, json, "email")

        ' ===== Laboral =====
        Dim empresa = GetField(ctx, json, "empresa")
        Dim puesto = GetField(ctx, json, "puesto")
        Dim antigTexto = GetField(ctx, json, "antiguedad_empleo")
        Dim telEmpresa = GetField(ctx, json, "telefono_empresa")
        Dim emailTrabajo = GetField(ctx, json, "email_trabajo")

        Dim actEcoTxt = GetField(ctx, json, "actividad_economica")
        Dim actEcoId = ToInt(GetField(ctx, json, "actividad_economica_id"))
        Dim ocupacionId = ToInt(GetField(ctx, json, "ocupacion_id"))

        ' ===== Ingresos =====
        Dim ingBrutos = ToDec(GetField(ctx, json, "ingresos_brutos"))
        Dim ingNetos = ToDec(GetField(ctx, json, "ingresos_netos"))
        Dim otrosIng = ToDec(GetField(ctx, json, "otros_ingresos"))
        Dim egresos = ToDec(GetField(ctx, json, "egresos"))
        Dim ingresoMensual = ToDec(GetField(ctx, json, "ingreso_mensual"))
        Dim perfilPagosMensuales = ToInt(GetField(ctx, json, "perfil_pagos_mensuales_esperados"))
        Dim perfilMontoMensual = ToDec(GetField(ctx, json, "perfil_monto_mensual_esperado"))

        If perfilPagosMensuales.HasValue AndAlso perfilPagosMensuales.Value < 0 Then
            WriteError(ctx, "El número esperado de pagos por mes no puede ser negativo.") : Exit Sub
        End If
        If perfilMontoMensual.HasValue AndAlso perfilMontoMensual.Value < 0D Then
            WriteError(ctx, "El monto mensual esperado no puede ser negativo.") : Exit Sub
        End If

        ' ===== PLD: Origen ingresos / consentimiento / identificación =====
        Dim origenOtros = GetField(ctx, json, "origen_otros_ingresos")

        Dim aceptaAviso As Boolean = False
        Dim aceptaRaw = GetField(ctx, json, "acepta_aviso_privacidad")
        If String.IsNullOrEmpty(aceptaRaw) Then
            aceptaRaw = GetField(ctx, json, "acepta_avisos")
        End If
        If Not String.IsNullOrEmpty(aceptaRaw) Then
            aceptaAviso = (aceptaRaw = "1" OrElse aceptaRaw.Equals("true", StringComparison.OrdinalIgnoreCase))
        End If

        Dim tipoIdent = GetField(ctx, json, "tipo_identificacion")
        If String.IsNullOrEmpty(tipoIdent) Then tipoIdent = GetField(ctx, json, "ident_tipo")

        Dim numIdent = GetField(ctx, json, "numero_identificacion")
        If String.IsNullOrEmpty(numIdent) Then numIdent = GetField(ctx, json, "ident_numero")

        Dim vigStr = GetField(ctx, json, "vigencia_identificacion")
        If String.IsNullOrEmpty(vigStr) Then vigStr = GetField(ctx, json, "ident_vigencia")
        Dim vigIdent = ToDate(vigStr)

        ' ===== Validaciones mínimas =====
        If String.IsNullOrEmpty(primerNombre) OrElse String.IsNullOrEmpty(apPaterno) Then
            WriteError(ctx, "Primer nombre y apellido paterno son obligatorios.") : Exit Sub
        End If
        If Not fechaNac.HasValue Then
            WriteError(ctx, "La fecha de nacimiento es obligatoria.") : Exit Sub
        End If
        If String.IsNullOrEmpty(sexo) Then
            WriteError(ctx, "El sexo es obligatorio.") : Exit Sub
        End If
        If String.IsNullOrEmpty(estadoCivil) Then
            WriteError(ctx, "El estado civil es obligatorio.") : Exit Sub
        End If

        Using con = GetConn()
            con.Open()
            Using tx = con.BeginTransaction()
                Try
                    Dim sql As String = "
                    UPDATE cliente_persona_fisica
                    SET primer_nombre=@pnom,
                        segundo_nombre=@snom,
                        apellido_paterno=@apat,
                        apellido_materno=@amat,
                        sexo=@sexo,
                        fecha_nacimiento=@fnac,
                        estado_civil=@ecivil,
                        curp=@curp,
                        rfc=@rfc,
                        nacionalidad=@nacionalidad,
                        puesto_politico=@pep,
                        calle=@calle,
                        numero_exterior=@numext,
                        numero_interior=@numint,
                        colonia=@col,
                        codigo_postal=@cp,
                        municipio=@mun,
                        estado=@edo,
                        ciudad=@ciudad,
                        referencias=@refs,
                        pais=@pais,
                        telefono_casa=@tcasa,
                        celular=@cel,
                        email=@mail,
                        empresa=@emp,
                        puesto=@puesto,
                        antiguedad_empleo=@antig,
                        ingresos_brutos=@ingb,
                        ingresos_netos=@ingn,
                        otros_ingresos=@otros,
                        egresos=@egre,
                        telefono_empresa=@telEmp,
                        email_trabajo=@mailEmp,
                        origen_otros_ingresos=@origen,
                        acepta_aviso_privacidad=@aviso,
                        tipo_identificacion=@tident,
                        numero_identificacion=@nident,
                        vigencia_identificacion=@vigident,
                        nacionalidad_id=@nid,
                        pais_nacimiento_id=@pid,
                        estado_nacimiento_id=@eid,
                        ocupacion_id=@oid,
                        actividad_economica_id=@aeid,
                        pais_domicilio_id=@pdom,
                        estado_domicilio_id=@edom,
                        municipio_domicilio_id=@mdom,
                        ingreso_mensual=@ingm,
                        perfil_pagos_mensuales_esperados=@perfilPagos,
                        perfil_monto_mensual_esperado=@perfilMonto,
                        perfil_transaccional_modificado_por=@user,
                        perfil_transaccional_fecha_modificacion=SYSDATETIME(),
                        regimen_matrimonial=@regimen,
                        dependientes=@dep,
                        escolaridad=@esc,
                        antiguedad_meses=@antMeses,
                        actividad_economica=@actEcoTxt
                    WHERE id_cliente=@id;"

                    Using cmd As New SqlCommand(sql, con, tx)

                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = cid

                        ' Datos personales
                        cmd.Parameters.Add("@pnom", SqlDbType.VarChar, 100).Value = primerNombre
                        cmd.Parameters.Add("@snom", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(segundoNombre), DBNull.Value, segundoNombre)
                        cmd.Parameters.Add("@apat", SqlDbType.VarChar, 100).Value = apPaterno
                        cmd.Parameters.Add("@amat", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(apMaterno), DBNull.Value, apMaterno)
                        cmd.Parameters.Add("@sexo", SqlDbType.VarChar, 10).Value = sexo
                        cmd.Parameters.Add("@fnac", SqlDbType.Date).Value = fechaNac.Value
                        cmd.Parameters.Add("@ecivil", SqlDbType.VarChar, 20).Value = estadoCivil

                        cmd.Parameters.Add("@curp", SqlDbType.VarChar, 18).Value = If(String.IsNullOrEmpty(curp), DBNull.Value, curp)
                        cmd.Parameters.Add("@rfc", SqlDbType.VarChar, 13).Value = rfc
                        cmd.Parameters.Add("@nacionalidad", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(nacionalidadTxt), DBNull.Value, nacionalidadTxt)
                        cmd.Parameters.Add("@pep", SqlDbType.Bit).Value = If(pep, 1, 0)

                        ' Domicilio
                        cmd.Parameters.Add("@calle", SqlDbType.VarChar, 150).Value = If(String.IsNullOrEmpty(calle), DBNull.Value, calle)
                        cmd.Parameters.Add("@numext", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(numExt), DBNull.Value, numExt)
                        cmd.Parameters.Add("@numint", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(numInt), DBNull.Value, numInt)
                        cmd.Parameters.Add("@col", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(colonia), DBNull.Value, colonia)
                        cmd.Parameters.Add("@cp", SqlDbType.VarChar, 10).Value = If(String.IsNullOrEmpty(cp), DBNull.Value, cp)
                        cmd.Parameters.Add("@mun", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(municipio), DBNull.Value, municipio)
                        cmd.Parameters.Add("@edo", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(estado), DBNull.Value, estado)
                        cmd.Parameters.Add("@ciudad", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(ciudad), DBNull.Value, ciudad)
                        cmd.Parameters.Add("@refs", SqlDbType.VarChar, 250).Value = If(String.IsNullOrEmpty(referencias), DBNull.Value, referencias)
                        cmd.Parameters.Add("@pais", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(pais), DBNull.Value, pais)

                        ' Contacto
                        cmd.Parameters.Add("@tcasa", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(telCasa), DBNull.Value, telCasa)
                        cmd.Parameters.Add("@cel", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(celular), DBNull.Value, celular)
                        cmd.Parameters.Add("@mail", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(email), DBNull.Value, email)

                        ' Laboral
                        cmd.Parameters.Add("@emp", SqlDbType.VarChar, 150).Value = If(String.IsNullOrEmpty(empresa), DBNull.Value, empresa)
                        cmd.Parameters.Add("@puesto", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(puesto), DBNull.Value, puesto)
                        cmd.Parameters.Add("@antig", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(antigTexto), DBNull.Value, antigTexto)

                        ' Ingresos
                        cmd.Parameters.Add("@ingb", SqlDbType.Decimal).Value = If(ingBrutos.HasValue, ingBrutos.Value, DBNull.Value)
                        cmd.Parameters.Add("@ingn", SqlDbType.Decimal).Value = If(ingNetos.HasValue, ingNetos.Value, DBNull.Value)
                        cmd.Parameters.Add("@otros", SqlDbType.Decimal).Value = If(otrosIng.HasValue, otrosIng.Value, DBNull.Value)
                        cmd.Parameters.Add("@egre", SqlDbType.Decimal).Value = If(egresos.HasValue, egresos.Value, DBNull.Value)

                        cmd.Parameters.Add("@telEmp", SqlDbType.VarChar, 20).Value = If(String.IsNullOrEmpty(telEmpresa), DBNull.Value, telEmpresa)
                        cmd.Parameters.Add("@mailEmp", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(emailTrabajo), DBNull.Value, emailTrabajo)

                        ' PLD / Identificación
                        cmd.Parameters.Add("@origen", SqlDbType.VarChar, 200).Value = If(String.IsNullOrEmpty(origenOtros), DBNull.Value, origenOtros)
                        cmd.Parameters.Add("@aviso", SqlDbType.Bit).Value = If(aceptaAviso, 1, 0)
                        cmd.Parameters.Add("@tident", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(tipoIdent), DBNull.Value, tipoIdent)
                        cmd.Parameters.Add("@nident", SqlDbType.VarChar, 100).Value = If(String.IsNullOrEmpty(numIdent), DBNull.Value, numIdent)
                        cmd.Parameters.Add("@vigident", SqlDbType.Date).Value = If(vigIdent.HasValue, vigIdent.Value, DBNull.Value)

                        ' IDs PLD
                        cmd.Parameters.Add("@nid", SqlDbType.Int).Value = If(nacId.HasValue, nacId.Value, DBNull.Value)
                        cmd.Parameters.Add("@pid", SqlDbType.Int).Value = If(paisNacId.HasValue, paisNacId.Value, DBNull.Value)
                        cmd.Parameters.Add("@eid", SqlDbType.Int).Value = If(edoNacId.HasValue, edoNacId.Value, DBNull.Value)
                        cmd.Parameters.Add("@oid", SqlDbType.Int).Value = If(ocupacionId.HasValue, ocupacionId.Value, DBNull.Value)
                        cmd.Parameters.Add("@aeid", SqlDbType.Int).Value = If(actEcoId.HasValue, actEcoId.Value, DBNull.Value)

                        cmd.Parameters.Add("@pdom", SqlDbType.Int).Value = If(paisDomId.HasValue, paisDomId.Value, DBNull.Value)
                        cmd.Parameters.Add("@edom", SqlDbType.Int).Value = If(edoDomId.HasValue, edoDomId.Value, DBNull.Value)
                        cmd.Parameters.Add("@mdom", SqlDbType.Int).Value = If(munDomId.HasValue, munDomId.Value, DBNull.Value)

                        cmd.Parameters.Add("@ingm", SqlDbType.Decimal).Value = If(ingresoMensual.HasValue, ingresoMensual.Value, DBNull.Value)
                        cmd.Parameters.Add("@perfilPagos", SqlDbType.Int).Value = If(perfilPagosMensuales.HasValue, CType(perfilPagosMensuales.Value, Object), DBNull.Value)
                        cmd.Parameters.Add("@perfilMonto", SqlDbType.Decimal).Value = If(perfilMontoMensual.HasValue, CType(perfilMontoMensual.Value, Object), DBNull.Value)
                        cmd.Parameters("@perfilMonto").Precision = 18
                        cmd.Parameters("@perfilMonto").Scale = 2
                        cmd.Parameters.Add("@user", SqlDbType.VarChar, 100).Value = NowUser()

                        ' Regimen / dependientes / escolaridad / antigüedad (meses) / actividad texto
                        cmd.Parameters.Add("@regimen", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(regimenMatrimonial), DBNull.Value, regimenMatrimonial)
                        cmd.Parameters.Add("@dep", SqlDbType.Int).Value = If(dependientes.HasValue, dependientes.Value, DBNull.Value)
                        cmd.Parameters.Add("@esc", SqlDbType.VarChar, 50).Value = If(String.IsNullOrEmpty(escolaridad), DBNull.Value, escolaridad)
                        cmd.Parameters.Add("@antMeses", SqlDbType.Int).Value = If(antiguedadMeses.HasValue, antiguedadMeses.Value, DBNull.Value)
                        cmd.Parameters.Add("@actEcoTxt", SqlDbType.VarChar, 200).Value = If(String.IsNullOrEmpty(actEcoTxt), DBNull.Value, actEcoTxt)

                        Dim rows = cmd.ExecuteNonQuery()
                        If rows <= 0 Then
                            tx.Rollback()
                            WriteError(ctx, "No se actualizó ningún registro.") : Exit Sub
                        End If

                    End Using

                    tx.Commit()
                    WriteJson(ctx, New With {.ok = True, .message = "Cliente actualizado.", .cliente_id = cid})

                Catch ex As Exception
                    tx.Rollback()
                    WriteError(ctx, "Error al actualizar cliente: " & ex.Message)
                End Try
            End Using
        End Using

    End Sub
    ' ================= FIN ACTUALIZAR ======================

    ' ============================= BUSCAR / LISTAR =============================
    ' Soporta:
    '   ?action=listar&search=texto&page=1&pageSize=20
    ' Busca por RFC, CURP, nombre, apellidos.
    Private Sub HandleBuscar(ctx As HttpContext)

        Dim search As String = (If(ctx.Request("search"), "")).Trim()
        Dim page As Integer = 1
        Dim pageSize As Integer = 20

        Integer.TryParse(ctx.Request("page"), page)
        Integer.TryParse(ctx.Request("pageSize"), pageSize)

        If page <= 0 Then page = 1
        If pageSize <= 0 Then pageSize = 20

        Dim off As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()

            Dim sql As String = "
            SELECT 
                id_cliente AS id,
                primer_nombre,
                segundo_nombre,
                apellido_paterno,
                apellido_materno,
                rfc,
                curp,
                fecha_nacimiento,
                sexo,
                estado_civil
            FROM cliente_persona_fisica
            WHERE 1=1
            "

            Dim cmd As New SqlCommand()
            cmd.Connection = con

            If Not String.IsNullOrEmpty(search) Then
                sql &= "
                  AND (
                        REPLACE(REPLACE(UPPER(rfc),'-',''),' ','') LIKE @s
                     OR REPLACE(REPLACE(UPPER(curp),'-',''),' ','') LIKE @s
                     OR UPPER(primer_nombre) LIKE @s
                     OR UPPER(apellido_paterno) LIKE @s
                     OR UPPER(apellido_materno) LIKE @s
                     )"
                cmd.Parameters.Add("@s", SqlDbType.VarChar, 100).Value = "%" & search.ToUpper() & "%"
            End If

            sql &= "
                ORDER BY apellido_paterno, apellido_materno, primer_nombre
                OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY;
                "

            cmd.CommandText = sql
            cmd.Parameters.Add("@off", SqlDbType.Int).Value = off
            cmd.Parameters.Add("@ps", SqlDbType.Int).Value = pageSize

            Dim lista As New List(Of Dictionary(Of String, Object))()

            Using rd = cmd.ExecuteReader()
                While rd.Read()
                    Dim row As New Dictionary(Of String, Object)
                    For i = 0 To rd.FieldCount - 1
                        row(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                    Next
                    lista.Add(row)
                End While
            End Using

            WriteJson(ctx, New With {
            .ok = True,
            .data = lista,
            .page = page,
            .pageSize = pageSize
        })
        End Using

    End Sub
    ' ========================== FIN HANDLE BUSCAR ===============================

    ' ============================== OBTENER CLIENTE ==============================
    ' Soporta:
    '   ?action=obtener&id=123
    '   ?action=obtener&cliente_id=123
    '   ?action=obtener&rfc=CAME7701108SA
    '   ?action=obtener&curp=CAME770110HCHNRD04

    Private Sub HandleObtener(ctx As HttpContext)
        ' Soporta cliente_id o id, y como fallback rfc=
        Dim idStr As String = (If(ctx.Request("cliente_id"), "")).Trim()
        If String.IsNullOrEmpty(idStr) Then
            idStr = (If(ctx.Request("id"), "")).Trim()
        End If

        Dim rfcParam As String = (If(ctx.Request("rfc"), "")).Trim()
        Dim cid As Integer = 0
        Dim buscarPorRfc As Boolean = False

        If Not String.IsNullOrEmpty(idStr) AndAlso Integer.TryParse(idStr, cid) AndAlso cid > 0 Then
            buscarPorRfc = False
        ElseIf Not String.IsNullOrEmpty(rfcParam) Then
            buscarPorRfc = True
        Else
            WriteError(ctx, "cliente_id o rfc es requerido.") : Exit Sub
        End If

        Using con = GetConn()
            con.Open()
            Dim sql As String
            If buscarPorRfc Then
                sql = "
                SELECT TOP 1 *
                FROM dbo.cliente_persona_fisica
                WHERE REPLACE(REPLACE(UPPER(rfc),'-',''),' ','') = REPLACE(REPLACE(UPPER(@rfc),'-',''),' ','');"
            Else
                sql = "
                SELECT TOP 1 *
                FROM dbo.cliente_persona_fisica
                WHERE id_cliente = @id;"
            End If

            Using cmd As New SqlCommand(sql, con)
                If buscarPorRfc Then
                    cmd.Parameters.Add("@rfc", SqlDbType.VarChar, 13).Value = rfcParam
                Else
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = cid
                End If

                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        WriteError(ctx, "Cliente no encontrado.") : Exit Sub
                    End If

                    ' =============================
                    ' Construir diccionario base
                    ' =============================
                    Dim row As New Dictionary(Of String, Object)
                    For i = 0 To rd.FieldCount - 1
                        row(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                    Next

                    ' ===============================================
                    ' MAPEO PARA FRONT-END — nombres esperados por JS
                    ' ===============================================

                    ' --- Apellidos ---
                    row("ap_paterno") = row("apellido_paterno")
                    row("ap_materno") = row("apellido_materno")

                    ' --- Nacionalidad (texto) ---
                    row("nacionalidad") = row("nacionalidad")

                    ' --- IDs relacionados (ya vienen del SQL) ---
                    row("nacionalidad_id") = row("nacionalidad_id")
                    row("pais_nacimiento_id") = row("pais_nacimiento_id")
                    row("estado_nacimiento_id") = row("estado_nacimiento_id")
                    row("ocupacion_id") = row("ocupacion_id")
                    row("actividad_economica_id") = row("actividad_economica_id")
                    row("pais_domicilio_id") = row("pais_domicilio_id")
                    row("estado_domicilio_id") = row("estado_domicilio_id")
                    row("municipio_domicilio_id") = row("municipio_domicilio_id")

                    ' --- Antigüedad laboral (front usa años) ---
                    Dim meses As Integer = If(row("antiguedad_meses") Is Nothing, 0, CInt(row("antiguedad_meses")))
                    row("antiguedad_anios") = Math.Floor(meses / 12)

                    ' --- Ingresos ---
                    row("ingreso_mensual") = row("ingreso_mensual")
                    row("otros_ingresos") = row("otros_ingresos")
                    row("origen_otros_ingresos") = row("origen_otros_ingresos")

                    ' --- Acepta avisos ---
                    row("acepta_avisos") = If(row("acepta_aviso_privacidad"), True, False)

                    ' --- Identificación ---
                    row("ident_tipo") = row("tipo_identificacion")
                    row("ident_numero") = row("numero_identificacion")
                    row("ident_vigencia") = row("vigencia_identificacion")

                    ' --- Estado civil / régimen / dependientes / escolaridad ---
                    row("estado_civil") = row("estado_civil")
                    row("regimen_matrimonial") = row("regimen_matrimonial")
                    row("dependientes") = row("dependientes")
                    row("escolaridad") = row("escolaridad")

                    ' --- Actividad económica (cadena) ---
                    row("actividad_economica") = row("actividad_economica")

                    ' =============================
                    ' RESPUESTA FINAL
                    ' =============================
                    WriteJson(ctx, New With {.ok = True, .data = row})

                End Using
            End Using
        End Using
    End Sub

    ' =========================== FIN HANDLEOBTENER ===============================


End Class
