Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Globalization
Imports System.Data.SqlTypes

Public Class calcular_pld
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"

        Try
            Dim action As String = If(context.Request("action"), "").Trim().ToLowerInvariant()

            Select Case action
                Case "preload"
                    HandlePreload(context)

                Case "calcular"
                    Using conn = GetConn()
                        conn.Open()
                        HandleCalcular(context, conn)
                    End Using

                Case "guardar"
                    HandleGuardar(context)

                Case Else
                    WriteError(context, "Acción no soportada. Usa: preload | calcular | guardar.")
            End Select

        Catch ex As Exception
            WriteError(context, "Excepción no controlada: " & ex.Message)
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '==========================================================
    ' Helpers generales
    '==========================================================
    Private Function GetConn() As SqlConnection
        Return New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
    End Function

    Private Sub WriteJson(ctx As HttpContext, obj As Object)
        ctx.Response.StatusCode = 200
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String)
        WriteJson(ctx, New With {.ok = False, .message = msg})
    End Sub

    Private Function GetInt(val As Object) As Integer
        If val Is Nothing Then Return 0
        Dim s = val.ToString().Trim()
        If s = "" Then Return 0
        Dim n As Integer = 0
        Integer.TryParse(s, n)
        Return n
    End Function

    'Alias para compatibilidad (evita BC30451 cuando se usa ToInt en parsing de JSON)
    Private Function ToInt(val As Object) As Integer
        Return GetInt(val)
    End Function

    Private Function GetInt(ctx As HttpContext, key As String) As Integer
        Return GetInt(ctx.Request(key))
    End Function

    Private Function ToDec(value As Object) As Decimal
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return 0D
        Dim d As Decimal
        If Decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, d) Then
            Return d
        End If
        Return 0D
    End Function

    Private Function NivelDesdeCampos(impacto As Object, prob As Object, nivelDirecto As Object, nivelRiesgoPLD As Object) As Decimal
        ' Prioridad:
        ' 1) nivel (si tu vista lo expone)
        Dim n1 As Decimal = ToDec(nivelDirecto)
        If n1 > 0D Then Return Math.Round(n1, 2)

        ' 2) nivel_riesgo_pld (si está calculado en catálogo)
        Dim n2 As Decimal = ToDec(nivelRiesgoPLD)
        If n2 > 0D Then Return Math.Round(n2, 2)

        ' 3) cálculo (impacto*prob)/100
        Dim imp As Decimal = ToDec(impacto)
        Dim pr As Decimal = ToDec(prob)
        If imp <= 0D OrElse pr <= 0D Then Return 0D

        Return Math.Round((imp * pr) / 100D, 2)
    End Function

    Private Function puntodec(d As Decimal) As SqlDecimal
        Return New SqlDecimal(d)
    End Function

    Private Function ExisteSolicitud(con As SqlConnection, solicitudId As Integer) As Boolean
        If solicitudId <= 0 Then Return False
        Using cmd As New SqlCommand("SELECT COUNT(1) FROM dbo.solicitud_credito WHERE id = @id;", con)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
            Return (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
        End Using
    End Function

    Private Function EsMexicoPorPaisId(con As SqlConnection, paisId As Integer) As Boolean
        If paisId <= 0 Then Return False
        Using cmd As New SqlCommand("
                SELECT COUNT(1)
                FROM dbo.catalogo_paises
                WHERE id = @id
                  AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(pais,''),'Á','A'),'É','E'),'Í','I'),'Ó','O'),'Ú','U')
                      COLLATE Latin1_General_CI_AI = 'MEXICO';
            ", con)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = paisId
            Return (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
        End Using
    End Function

    '==========================================================
    ' NUEVO: Tipo de riesgo por puntaje total (PF)
    '==========================================================
    Private Function GetTipoRiesgoPF(con As SqlConnection, puntajeTotal As Decimal) As String
        Using cmd As New SqlCommand("
            SELECT TOP (1) nivel_riesgo
            FROM dbo.config_puntaje_categoria
            WHERE tipo_persona = 'PF'
              AND activo = 1
              AND @puntaje >= valor_minimo
              AND @puntaje <= valor_maximo
            ORDER BY valor_minimo DESC;
        ", con)

            cmd.Parameters.Add("@puntaje", SqlDbType.Decimal).Value = puntodec(Math.Round(puntajeTotal, 2))
            Dim o As Object = cmd.ExecuteScalar()
            If o Is Nothing OrElse Convert.IsDBNull(o) Then Return ""
            Return Convert.ToString(o)
        End Using
    End Function

    '==========================================================
    ' ACTION: preload
    '==========================================================
    Private Sub HandlePreload(ctx As HttpContext)
        Dim origenFiltro As String = If(ctx.Request("origen_tipo"), "").Trim()
        Dim q As String = If(ctx.Request("q"), "").Trim()
        Dim soloActivos As Boolean = (If(ctx.Request("solo_activos"), "1") = "1")

        Dim page As Integer = 1
        Dim pageSize As Integer = 50
        Integer.TryParse(If(ctx.Request("page"), "1"), page)
        Integer.TryParse(If(ctx.Request("pageSize"), "50"), pageSize)
        If page < 1 Then page = 1
        If pageSize < 1 Then pageSize = 50

        Dim offset As Integer = (page - 1) * pageSize

        Dim whereSql As String = " WHERE 1=1 "
        If Not String.IsNullOrEmpty(origenFiltro) Then
            whereSql &= " AND origen_tipo = @origen_tipo "
        End If
        If soloActivos Then
            whereSql &= " AND activo = 1 "
        End If
        If Not String.IsNullOrEmpty(q) Then
            whereSql &= " AND descripcion LIKE @q "
        End If

        Using con = GetConn()
            con.Open()

            Dim total As Integer = 0
            Using cmdCount As New SqlCommand("SELECT COUNT(1) FROM dbo.vw_pld_factores " & whereSql & ";", con)
                If Not String.IsNullOrEmpty(origenFiltro) Then
                    cmdCount.Parameters.Add("@origen_tipo", SqlDbType.NVarChar, 50).Value = origenFiltro
                End If
                If Not String.IsNullOrEmpty(q) Then
                    cmdCount.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                End If
                total = Convert.ToInt32(cmdCount.ExecuteScalar())
            End Using

            Dim sql As String =
                "SELECT origen_tipo, referencia_id, descripcion, impacto, probabilidad, nivel_riesgo_pld, nivel, activo " &
                "FROM dbo.vw_pld_factores " & whereSql &
                "ORDER BY origen_tipo, referencia_id " &
                "OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"

            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(origenFiltro) Then
                    cmd.Parameters.Add("@origen_tipo", SqlDbType.NVarChar, 50).Value = origenFiltro
                End If
                If Not String.IsNullOrEmpty(q) Then
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                End If
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize

                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)

                    While rd.Read()
                        rows.Add(New With {
                            .origen_tipo = Convert.ToString(rd("origen_tipo")),
                            .referencia_id = Convert.ToInt32(rd("referencia_id")),
                            .descripcion = Convert.ToString(rd("descripcion")),
                            .impacto = Convert.ToInt32(If(Convert.IsDBNull(rd("impacto")), 0, rd("impacto"))),
                            .probabilidad = Convert.ToInt32(If(Convert.IsDBNull(rd("probabilidad")), 0, rd("probabilidad"))),
                            .nivel_riesgo_pld = ToDec(rd("nivel_riesgo_pld")),
                            .nivel = ToDec(rd("nivel")),
                            .activo = If(Convert.IsDBNull(rd("activo")), False, Convert.ToBoolean(rd("activo")))
                        })
                    End While

                    WriteJson(ctx, New With {
                        .ok = True,
                        .data = rows,
                        .page = page,
                        .pageSize = pageSize,
                        .total = total
                    })
                End Using
            End Using
        End Using
    End Sub

    '==========================================================
    ' ACTION: calcular (maneja TODO lo que exista en vw_pld_factores)
    '==========================================================
    Private Sub HandleCalcular(context As HttpContext, conn As SqlConnection)
        Dim productoId As Integer = GetInt(context, "producto_financiero_id")
        If productoId = 0 Then
            WriteJson(context, New With {.ok = False, .message = "producto_financiero_id es obligatorio"})
            Return
        End If

        ' IDs (puedes mandar 0 si no aplica)
        Dim canalId As Integer = GetInt(context, "canal_pago_id")
        Dim destinoId As Integer = GetInt(context, "destino_recursos_id")
        Dim medioId As Integer = GetInt(context, "medio_contacto_id")
        Dim ocupacionId As Integer = GetInt(context, "ocupacion_id")
        Dim origenId As Integer = GetInt(context, "origen_recursos_id")

        ' Nacimiento (nacionalidad por separado del país de nacimiento)
        Dim nacionalidadId As Integer = GetInt(context, "nacionalidad_id")
        Dim paisNacimientoId As Integer = GetInt(context, "pais_id")
        Dim estadoNacimientoId As Integer = GetInt(context, "estado_nacimiento_id")

        ' Domicilio (independiente del país de nacimiento)
        Dim paisDomId As Integer = GetInt(context, "pais_domicilio_id")
        Dim estadoDomId As Integer = GetInt(context, "estado_domicilio_id")
        Dim municipioDomId As Integer = GetInt(context, "municipio_domicilio_id")

        ' Domicilios (lista completa de activos, opcional)
        Dim domiciliosJson As String = Convert.ToString(context.Request("domicilios"))
        Dim domiciliosList As List(Of Dictionary(Of String, Object)) = Nothing
        If Not String.IsNullOrWhiteSpace(domiciliosJson) Then
            Try
                domiciliosList = serializer.Deserialize(Of List(Of Dictionary(Of String, Object)))(domiciliosJson)
            Catch
                domiciliosList = Nothing
            End Try
        End If

        ' Otros factores del set “completo”
        ' NOTA: el front manda moneda_id (no moneda_divisa_id)
        Dim monedaId As Integer = GetInt(context, "moneda_id")
        Dim actividadEcoId As Integer = GetInt(context, "actividad_economica_id")
        Dim creditoPldId As Integer = GetInt(context, "credito_pld_id")

        Dim desglose As New List(Of Object)
        Dim niveles As New List(Of Decimal)

        Dim loadFactor As Action(Of String, Integer) =
            Sub(origen As String, refId As Integer)
                If refId <= 0 Then Exit Sub

                Using cmd As New SqlCommand("
                        SELECT TOP 1 descripcion, impacto, probabilidad, nivel_riesgo_pld, nivel
                        FROM dbo.vw_pld_factores
                        WHERE origen_tipo = @origen AND referencia_id = @id AND activo = 1
                        ORDER BY referencia_id DESC;
                    ", conn)

                    cmd.Parameters.Add("@origen", SqlDbType.NVarChar, 50).Value = origen
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = refId

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            Dim desc As String = Convert.ToString(r("descripcion"))
                            Dim imp As Decimal = ToDec(r("impacto"))
                            Dim prob As Decimal = ToDec(r("probabilidad"))
                            Dim nivel As Decimal = NivelDesdeCampos(r("impacto"), r("probabilidad"), r("nivel"), r("nivel_riesgo_pld"))

                            niveles.Add(nivel)

                            desglose.Add(New With {
                                .origen_tipo = origen,
                                .origen = origen,
                                .referencia_id = refId,
                                .descripcion = desc,
                                .impacto = imp,
                                .probabilidad = prob,
                                .nivel = nivel
                            })
                        End If
                    End Using
                End Using
            End Sub

        Dim NormalizaTexto As Func(Of String, String) =
            Function(s As String) As String
                If String.IsNullOrWhiteSpace(s) Then Return ""
                Dim t As String = s.Trim().ToUpperInvariant()
                t = t.Replace("Á"c, "A"c).Replace("É"c, "E"c).Replace("Í"c, "I"c).Replace("Ó"c, "O"c).Replace("Ú"c, "U"c).Replace("Ü"c, "U"c)
                t = System.Text.RegularExpressions.Regex.Replace(t, "\s+", " ")
                Return t
            End Function

        Dim loadFactorByDesc As Action(Of String, String) =
            Sub(origen As String, descInput As String)
                If String.IsNullOrWhiteSpace(descInput) Then Exit Sub

                Dim descNorm As String = NormalizaTexto(descInput)
                If descNorm = "" Then Exit Sub

                Using cmd As New SqlCommand("
                      SELECT TOP 1 descripcion, impacto, probabilidad, nivel_riesgo_pld, nivel
                        FROM dbo.vw_pld_factores
                        WHERE origen_tipo = @origen
                          AND activo = 1
                          AND descripcion COLLATE Latin1_General_CI_AI = @desc
                        ORDER BY referencia_id DESC;
                    ", conn)

                    cmd.Parameters.Add("@origen", SqlDbType.NVarChar, 50).Value = origen
                    cmd.Parameters.Add("@desc", SqlDbType.NVarChar, 200).Value = descNorm

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            Dim desc As String = Convert.ToString(r("descripcion"))
                            Dim imp As Decimal = ToDec(r("impacto"))
                            Dim prob As Decimal = ToDec(r("probabilidad"))
                            Dim nivel As Decimal = NivelDesdeCampos(r("impacto"), r("probabilidad"), r("nivel"), r("nivel_riesgo_pld"))

                            niveles.Add(nivel)

                            desglose.Add(New With {
                                .origen_tipo = origen,
                                .origen = origen,
                                .referencia_id = 0,
                                .descripcion = desc,
                                .impacto = imp,
                                .probabilidad = prob,
                                .nivel_riesgo_pld = ToDec(r("nivel_riesgo_pld")),
                                .nivel = nivel
                            })
                        End If
                    End Using
                End Using
            End Sub

        ' 1) Siempre
        loadFactor("producto_financiero", productoId)
        loadFactor("canal_pago", canalId)
        loadFactor("destino_recursos", destinoId)
        loadFactor("medio_contacto", medioId)
        loadFactor("ocupacion", ocupacionId)
        loadFactor("origen_recursos", origenId)
        loadFactor("nacionalidad", nacionalidadId)

        ' 2) Nacimiento
        loadFactor("pais", paisNacimientoId)
        If paisNacimientoId > 0 AndAlso EsMexicoPorPaisId(conn, paisNacimientoId) Then
            loadFactor("estado_nacimiento", estadoNacimientoId)
        End If

        ' 3) Domicilio (sumar TODOS los domicilios activos)
        If domiciliosList IsNot Nothing AndAlso domiciliosList.Count > 0 Then

            For Each d As Dictionary(Of String, Object) In domiciliosList

                Dim activoDom As Integer = 1
                If d.ContainsKey("activo") Then activoDom = ToInt(d("activo"))
                If activoDom <> 1 Then Continue For

                Dim pId As Integer = 0, eId As Integer = 0, mId As Integer = 0
                Dim pNom As String = "", eNom As String = "", mNom As String = ""

                If d.ContainsKey("pais_id") Then pId = ToInt(d("pais_id"))
                If d.ContainsKey("estado_id") Then eId = ToInt(d("estado_id"))
                If d.ContainsKey("municipio_id") Then mId = ToInt(d("municipio_id"))

                If d.ContainsKey("pais") Then pNom = Convert.ToString(d("pais"))
                If d.ContainsKey("estado") Then eNom = Convert.ToString(d("estado"))
                If d.ContainsKey("municipio") Then mNom = Convert.ToString(d("municipio"))

                Dim esMexico As Boolean = False
                If pId > 0 Then
                    esMexico = EsMexicoPorPaisId(conn, pId)
                Else
                    esMexico = (NormalizaTexto(pNom) = "MEXICO")
                End If

                ' País siempre
                If pId > 0 Then
                    loadFactor("pais_domicilio", pId)
                Else
                    loadFactorByDesc("pais_domicilio", pNom)
                End If

                ' Si México: sumar Estado + Municipio; si extranjero: solo País
                If esMexico Then
                    If eId > 0 Then
                        loadFactor("estado_domicilio", eId)
                    Else
                        loadFactorByDesc("estado_domicilio", eNom)
                    End If

                    If mId > 0 Then
                        loadFactor("municipio_domicilio", mId)
                    Else
                        loadFactorByDesc("municipio_domicilio", mNom)
                    End If
                End If

            Next

        Else
            ' Compatibilidad (un solo domicilio)
            loadFactor("pais_domicilio", paisDomId)
            If paisDomId > 0 AndAlso EsMexicoPorPaisId(conn, paisDomId) Then
                loadFactor("estado_domicilio", estadoDomId)
                loadFactor("municipio_domicilio", municipioDomId)
            End If
        End If

        ' 4) Extras del set completo
        loadFactor("moneda_divisa", monedaId)
        loadFactor("actividad_economica", actividadEcoId)
        loadFactor("credito_pld", creditoPldId)

        ' Suma y promedio
        Dim sumaNivel As Decimal = 0D
        For Each n In niveles
            sumaNivel += n
        Next

        Dim promedioNivel As Decimal = 0D
        If niveles.Count > 0 Then
            promedioNivel = Math.Round(sumaNivel / niveles.Count, 2)
        End If

        ' NUEVO: tipo de riesgo (PF) según puntaje total (= sumaNivel)
        Dim tipoRiesgo As String = GetTipoRiesgoPF(conn, Math.Round(sumaNivel, 2))

        Dim metodologia As String = "v3-full-15-factors"

        ' Guardar SOLO si solicitud_id existe (evita FK conflict)
        Dim solicitudId As Integer = GetInt(context, "solicitud_id")
        Dim guardado As Boolean = False

        If solicitudId > 0 AndAlso ExisteSolicitud(conn, solicitudId) Then
            ' Encabezado
            Using cmdUpd As New SqlCommand("
                    UPDATE dbo.solicitud_credito
                    SET
                        pld_puntaje_total       = @suma,
                        pld_nivel_riesgo_pld    = @nivel,
                        pld_metodologia_version = @metodo,
                        pld_fecha_calculo       = SYSUTCDATETIME(),
                        fecha_modificacion      = SYSUTCDATETIME()
                    WHERE id = @id;
                ", conn)

                cmdUpd.Parameters.Add("@suma", SqlDbType.Decimal).Value = puntodec(Math.Round(sumaNivel, 4))
                cmdUpd.Parameters.Add("@nivel", SqlDbType.Decimal).Value = puntodec(promedioNivel)
                cmdUpd.Parameters.Add("@metodo", SqlDbType.NVarChar, 50).Value = metodologia
                cmdUpd.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
                cmdUpd.ExecuteNonQuery()
            End Using

            ' Detalle: borrar e insertar
            Using cmdDel As New SqlCommand("DELETE FROM dbo.solicitud_pld_detalle WHERE solicitud_id = @id;", conn)
                cmdDel.Parameters.Add("@id", SqlDbType.Int).Value = solicitudId
                cmdDel.ExecuteNonQuery()
            End Using

            Using cmdIns As New SqlCommand("
                    INSERT INTO dbo.solicitud_pld_detalle
                        (solicitud_id, origen_tipo, referencia_id, factor_codigo,
                         impacto, probabilidad, nivel_riesgo_pld, peso, contribucion,
                         fecha_creacion, creado_por)
                    VALUES
                        (@sid, @origen, @ref, NULL,
                         @imp, @prob, @nivel, @peso, @cont,
                         SYSUTCDATETIME(), @usr);
                ", conn)

                Dim pesoBase As Decimal = If(desglose.Count > 0, Math.Round(1D / desglose.Count, 4), 0D)

                For Each f In desglose
                    Dim contrib As Decimal = 0D
                    Dim fNivel As Decimal = ToDec(f.nivel)
                    If promedioNivel > 0D AndAlso fNivel > 0D AndAlso pesoBase > 0D Then
                        contrib = Math.Round((fNivel * pesoBase) / promedioNivel, 4)
                    End If

                    cmdIns.Parameters.Clear()
                    cmdIns.Parameters.Add("@sid", SqlDbType.Int).Value = solicitudId
                    cmdIns.Parameters.Add("@origen", SqlDbType.NVarChar, 50).Value = Convert.ToString(f.origen)
                    cmdIns.Parameters.Add("@ref", SqlDbType.Int).Value = Convert.ToInt32(f.referencia_id)
                    cmdIns.Parameters.Add("@imp", SqlDbType.Int).Value = CInt(Math.Truncate(ToDec(f.impacto)))
                    cmdIns.Parameters.Add("@prob", SqlDbType.Int).Value = CInt(Math.Truncate(ToDec(f.probabilidad)))
                    cmdIns.Parameters.Add("@nivel", SqlDbType.Decimal).Value = puntodec(Math.Round(fNivel, 2))
                    cmdIns.Parameters.Add("@peso", SqlDbType.Decimal).Value = puntodec(pesoBase)
                    cmdIns.Parameters.Add("@cont", SqlDbType.Decimal).Value = puntodec(contrib)
                    cmdIns.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = "SYSTEM"
                    cmdIns.ExecuteNonQuery()
                Next
            End Using

            guardado = True
        End If

        WriteJson(context, New With {
            .ok = True,
            .suma_nivel = Math.Round(sumaNivel, 4),
            .promedio_nivel = promedioNivel,
            .nivel_final = promedioNivel,
            .puntaje_total = Math.Round(sumaNivel, 2),
            .tipo_riesgo = tipoRiesgo,
            .metodologia = metodologia,
            .factores = desglose,
            .persistido = guardado,
            .solicitud_id = If(solicitudId > 0, solicitudId, 0)
        })
    End Sub

    '==========================================================
    ' ACTION: guardar (crea solicitud_credito + snapshot detalle)
    '==========================================================
    Private Sub HandleGuardar(ctx As HttpContext)
        Dim productoId As Integer = GetInt(ctx, "producto_financiero_id")
        If productoId <= 0 Then
            WriteError(ctx, "producto_financiero_id es requerido.")
            Return
        End If

        Dim clienteId As Integer = GetInt(ctx, "cliente_id")
        Dim canalId As Integer = GetInt(ctx, "canal_pago_id")
        Dim destinoId As Integer = GetInt(ctx, "destino_recursos_id")
        Dim medioId As Integer = GetInt(ctx, "medio_contacto_id")

        Dim montoSolicitado As Decimal = 0D
        Decimal.TryParse(If(ctx.Request("monto_solicitado"), "0"), NumberStyles.Any, CultureInfo.InvariantCulture, montoSolicitado)

        Dim plazo As Integer = 0
        Integer.TryParse(If(ctx.Request("plazo"), "0"), NumberStyles.Integer, CultureInfo.InvariantCulture, plazo)

        Dim tasaEntrada As Decimal = 0D
        Decimal.TryParse(If(ctx.Request("tasa_entrada"), "0"), NumberStyles.Any, CultureInfo.InvariantCulture, tasaEntrada)

        Dim observ As String = If(ctx.Request("observaciones"), Nothing)

        Using con = GetConn()
            con.Open()

            Dim detalles As New List(Of Object)()
            Dim niveles As New List(Of Decimal)()

            Dim addFactor As Action(Of String, Integer) =
                Sub(origen As String, refId As Integer)
                    If refId <= 0 Then Exit Sub

                    Using cmd As New SqlCommand("
                            SELECT TOP 1 descripcion, impacto, probabilidad, nivel_riesgo_pld, nivel
                            FROM dbo.vw_pld_factores
                            WHERE origen_tipo = @origen AND referencia_id = @id AND activo = 1;
                        ", con)
                        cmd.Parameters.Add("@origen", SqlDbType.NVarChar, 50).Value = origen
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = refId

                        Using rd = cmd.ExecuteReader()
                            If rd.Read() Then
                                Dim imp As Decimal = ToDec(rd("impacto"))
                                Dim pr As Decimal = ToDec(rd("probabilidad"))
                                Dim nivel As Decimal = NivelDesdeCampos(rd("impacto"), rd("probabilidad"), rd("nivel"), rd("nivel_riesgo_pld"))

                                niveles.Add(nivel)
                                detalles.Add(New With {
                                    .origen = origen,
                                    .referencia_id = refId,
                                    .impacto = imp,
                                    .probabilidad = pr,
                                    .nivel = nivel
                                })
                            End If
                        End Using
                    End Using
                End Sub

            ' Mismos parámetros que calcular
            Dim ocupacionId As Integer = GetInt(ctx, "ocupacion_id")
            Dim origenRecId As Integer = GetInt(ctx, "origen_recursos_id")

            Dim nacionalidadId As Integer = GetInt(ctx, "nacionalidad_id")
            Dim paisNacimientoId As Integer = GetInt(ctx, "pais_id")
            Dim estadoNacimientoId As Integer = GetInt(ctx, "estado_nacimiento_id")

            Dim paisDomId As Integer = GetInt(ctx, "pais_domicilio_id")
            Dim estadoDomId As Integer = GetInt(ctx, "estado_domicilio_id")
            Dim municipioDomId As Integer = GetInt(ctx, "municipio_domicilio_id")

            ' NOTA: el front manda moneda_id
            Dim monedaId As Integer = GetInt(ctx, "moneda_id")
            Dim actividadEcoId As Integer = GetInt(ctx, "actividad_economica_id")
            Dim creditoPldId As Integer = GetInt(ctx, "credito_pld_id")

            addFactor("producto_financiero", productoId)
            addFactor("canal_pago", canalId)
            addFactor("destino_recursos", destinoId)
            addFactor("medio_contacto", medioId)
            addFactor("ocupacion", ocupacionId)
            addFactor("origen_recursos", origenRecId)
            addFactor("nacionalidad", nacionalidadId)

            addFactor("pais", paisNacimientoId)
            If paisNacimientoId > 0 AndAlso EsMexicoPorPaisId(con, paisNacimientoId) Then
                addFactor("estado_nacimiento", estadoNacimientoId)
            End If

            addFactor("pais_domicilio", paisDomId)
            If paisDomId > 0 AndAlso EsMexicoPorPaisId(con, paisDomId) Then
                addFactor("estado_domicilio", estadoDomId)
                addFactor("municipio_domicilio", municipioDomId)
            End If

            addFactor("moneda_divisa", monedaId)
            addFactor("actividad_economica", actividadEcoId)
            addFactor("credito_pld", creditoPldId)

            Dim suma As Decimal = 0D
            For Each n In niveles : suma += n : Next
            Dim promedio As Decimal = If(niveles.Count > 0, Math.Round(suma / niveles.Count, 2), 0D)

            ' NUEVO: tipo de riesgo (PF) según puntaje total (= suma)
            Dim tipoRiesgo As String = GetTipoRiesgoPF(con, Math.Round(suma, 2))

            Dim solicitudId As Integer = 0
            Dim metodologia As String = "v3-full-13-factors"

            Using tx = con.BeginTransaction()
                Using cmd As New SqlCommand("
                        INSERT INTO dbo.solicitud_credito
                            (cliente_id,
                             producto_financiero_id, canal_pago_id, destino_recursos_id, medio_contacto_id, moneda_id,
                             monto_solicitado, plazo, tasa_entrada, observaciones,
                             pld_puntaje_total, pld_nivel_riesgo_pld, pld_metodologia_version, pld_fecha_calculo,
                             estatus, activo, creado_por, fecha_creacion)
                        VALUES
                            (@cliente_id,
                             @pf_id, @canal_id, @dest_id, @medio_id, @moneda_id,
                             @monto, @plazo, @tasa, @obs,
                             @suma, @nivel, @version, SYSUTCDATETIME(),
                             @estatus, 1, @creado_por, SYSUTCDATETIME());
                        SELECT SCOPE_IDENTITY();
                    ", con, tx)

                    cmd.Parameters.Add("@cliente_id", SqlDbType.Int).Value = If(clienteId > 0, CType(clienteId, Object), DBNull.Value)
                    cmd.Parameters.Add("@pf_id", SqlDbType.Int).Value = productoId
                    cmd.Parameters.Add("@canal_id", SqlDbType.Int).Value = If(canalId > 0, CType(canalId, Object), DBNull.Value)
                    cmd.Parameters.Add("@dest_id", SqlDbType.Int).Value = If(destinoId > 0, CType(destinoId, Object), DBNull.Value)
                    cmd.Parameters.Add("@medio_id", SqlDbType.Int).Value = If(medioId > 0, CType(medioId, Object), DBNull.Value)
                    cmd.Parameters.Add("@moneda_id", SqlDbType.Int).Value = If(monedaId > 0, CType(monedaId, Object), DBNull.Value)

                    cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = puntodec(montoSolicitado)
                    cmd.Parameters.Add("@plazo", SqlDbType.Int).Value = If(plazo > 0, CType(plazo, Object), DBNull.Value)
                    cmd.Parameters.Add("@tasa", SqlDbType.Decimal).Value = puntodec(tasaEntrada)
                    cmd.Parameters.Add("@obs", SqlDbType.NVarChar, 1000).Value = If(String.IsNullOrEmpty(observ), CType(DBNull.Value, Object), observ)

                    cmd.Parameters.Add("@suma", SqlDbType.Decimal).Value = puntodec(Math.Round(suma, 4))
                    cmd.Parameters.Add("@nivel", SqlDbType.Decimal).Value = puntodec(promedio)
                    cmd.Parameters.Add("@version", SqlDbType.NVarChar, 50).Value = metodologia
                    cmd.Parameters.Add("@estatus", SqlDbType.NVarChar, 30).Value = "CALCULADO"
                    cmd.Parameters.Add("@creado_por", SqlDbType.NVarChar, 100).Value = "SYSTEM"

                    solicitudId = Convert.ToInt32(cmd.ExecuteScalar())
                End Using

                Using cmdDet As New SqlCommand("
                        INSERT INTO dbo.solicitud_pld_detalle
                            (solicitud_id, origen_tipo, referencia_id, factor_codigo,
                             impacto, probabilidad, nivel_riesgo_pld, peso, contribucion,
                             fecha_creacion, creado_por)
                        VALUES
                            (@sid, @origen, @ref, NULL,
                             @imp, @prob, @nivel, @peso, @cont,
                             SYSUTCDATETIME(), @usr);
                    ", con, tx)

                    Dim pesoBase As Decimal = If(detalles.Count > 0, Math.Round(1D / detalles.Count, 4), 0D)

                    For Each f In detalles
                        Dim fNivel As Decimal = ToDec(f.nivel)
                        Dim contrib As Decimal = 0D
                        If promedio > 0D AndAlso fNivel > 0D AndAlso pesoBase > 0D Then
                            contrib = Math.Round((fNivel * pesoBase) / promedio, 4)
                        End If

                        cmdDet.Parameters.Clear()
                        cmdDet.Parameters.Add("@sid", SqlDbType.Int).Value = solicitudId
                        cmdDet.Parameters.Add("@origen", SqlDbType.NVarChar, 50).Value = Convert.ToString(f.origen)
                        cmdDet.Parameters.Add("@ref", SqlDbType.Int).Value = Convert.ToInt32(f.referencia_id)
                        cmdDet.Parameters.Add("@imp", SqlDbType.Int).Value = CInt(Math.Truncate(ToDec(f.impacto)))
                        cmdDet.Parameters.Add("@prob", SqlDbType.Int).Value = CInt(Math.Truncate(ToDec(f.probabilidad)))
                        cmdDet.Parameters.Add("@nivel", SqlDbType.Decimal).Value = puntodec(Math.Round(fNivel, 2))
                        cmdDet.Parameters.Add("@peso", SqlDbType.Decimal).Value = puntodec(pesoBase)
                        cmdDet.Parameters.Add("@cont", SqlDbType.Decimal).Value = puntodec(contrib)
                        cmdDet.Parameters.Add("@usr", SqlDbType.NVarChar, 100).Value = "SYSTEM"
                        cmdDet.ExecuteNonQuery()
                    Next
                End Using

                tx.Commit()
            End Using

            WriteJson(ctx, New With {
                .ok = True,
                .message = "Solicitud guardada.",
                .solicitud_id = solicitudId,
                .metodologia = metodologia,
                .puntaje_total = Math.Round(suma, 2),
                .tipo_riesgo = tipoRiesgo,
                .promedio_nivel = promedio
            })
        End Using
    End Sub

End Class
