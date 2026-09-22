Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Public Class handler_alertas_pld
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = System.Text.Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = ObtenerParametro(context, "action").Trim().ToLowerInvariant()

            If String.IsNullOrWhiteSpace(action) Then
                action = ObtenerParametro(context, "accion").Trim().ToLowerInvariant()
            End If

            Select Case action
                Case "categorias"
                    respuesta = ConsultarCategorias()

                Case "motivos"
                    respuesta = ConsultarMotivos(context)

                Case "reglas"
                    respuesta = ConsultarReglas(context)

                Case "bandeja"
                    respuesta = ConsultarBandeja(context)

                Case "obtener"
                    respuesta = ObtenerAlerta(context)

                Case "bitacora"
                    respuesta = ConsultarBitacora(context)

                Case "generar_manual"
                    respuesta = GenerarAlertaManual(context)

                Case "generar_desde_solicitud"
                    respuesta = GenerarAlertasDesdeSolicitud(context)

                Case "generar_desde_pago"
                    respuesta = GenerarAlertasDesdePago(context)

                Case "generar_desde_pago_mensual"
                    respuesta = GenerarAlertasDesdePagoMensual(context)

                Case "generar_desde_pago_credito"
                    respuesta = GenerarAlertasDesdePagoCredito(context)

                Case "generar_desde_cliente_periodo"
                    respuesta = GenerarAlertasDesdeClientePeriodo(context)

                Case "generar_desde_pago_completo"
                    respuesta = GenerarAlertasDesdePagoCompleto(context)

                Case "generar_desde_perfil_transaccional"
                    respuesta = GenerarAlertasDesdePerfilTransaccional(context)

                Case "cambiar_estatus"
                    respuesta = CambiarEstatusAlerta(context)

                Case "asignar"
                    respuesta = AsignarAlerta(context)

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_alertas_pld."
            respuesta("detalle") = ex.Message
        End Try

        context.Response.Write(serializer.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Function CadenaConexion() As String
        Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
    End Function

    Private Function ObtenerParametro(ByVal context As HttpContext, ByVal nombre As String) As String
        Dim valor As String = ""

        If context.Request(nombre) IsNot Nothing Then
            valor = Convert.ToString(context.Request(nombre))
        End If

        Return valor
    End Function

    Private Function ObtenerUsuario(ByVal context As HttpContext) As String
        Dim usuario As String = ""

        If context.Session IsNot Nothing AndAlso context.Session("usuario") IsNot Nothing Then
            usuario = Convert.ToString(context.Session("usuario"))
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = ObtenerParametro(context, "usuario")
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = "SISTEMA"
        End If

        Return usuario.Trim()
    End Function

    Private Function ToInt(ByVal valor As Object, Optional ByVal defaultValue As Integer = 0) As Integer
        If valor Is Nothing Then Return defaultValue

        Dim resultado As Integer = defaultValue
        Integer.TryParse(Convert.ToString(valor), resultado)
        Return resultado
    End Function

    Private Function ToDecimalNullable(ByVal valor As Object) As Decimal?
        If valor Is Nothing Then Return Nothing

        Dim texto As String = Convert.ToString(valor).Trim()

        If String.IsNullOrWhiteSpace(texto) Then Return Nothing

        texto = texto.Replace(",", ".")

        Dim resultado As Decimal

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, resultado) Then
            Return resultado
        End If

        Return Nothing
    End Function

    Private Function ToBool(ByVal valor As Object, Optional ByVal defaultValue As Boolean = False) As Boolean
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return defaultValue

        Dim texto As String = Convert.ToString(valor).Trim().ToLowerInvariant()

        If texto = "1" OrElse texto = "true" OrElse texto = "si" OrElse texto = "sí" Then
            Return True
        End If

        If texto = "0" OrElse texto = "false" OrElse texto = "no" Then
            Return False
        End If

        Return defaultValue
    End Function

    Private Function TablaALista(ByVal dt As DataTable) As List(Of Dictionary(Of String, Object))
        Dim lista As New List(Of Dictionary(Of String, Object))()

        For Each fila As DataRow In dt.Rows
            Dim item As New Dictionary(Of String, Object)()

            For Each col As DataColumn In dt.Columns
                If fila(col) Is DBNull.Value Then
                    item(col.ColumnName) = Nothing
                Else
                    item(col.ColumnName) = fila(col)
                End If
            Next

            lista.Add(item)
        Next

        Return lista
    End Function

    Private Function EjecutarTabla(ByVal sql As String, ByVal parametros As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()

        Using cn As New SqlConnection(CadenaConexion())
            Using cmd As New SqlCommand(sql, cn)
                cmd.CommandType = CommandType.Text

                If parametros IsNot Nothing Then
                    cmd.Parameters.AddRange(parametros.ToArray())
                End If

                Using da As New SqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        Return dt
    End Function

    Private Function ConsultarCategorias() As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim sql As String =
            "SELECT id, clave, descripcion, orden, activo " &
            "FROM dbo.catalogo_alerta_categoria " &
            "WHERE activo = 1 " &
            "ORDER BY orden, descripcion;"

        Dim dt As DataTable = EjecutarTabla(sql, Nothing)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function ConsultarMotivos(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim categoriaId As Integer = ToInt(ObtenerParametro(context, "categoria_id"), 0)

        Dim parametros As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE m.activo = 1 "

        If categoriaId > 0 Then
            whereSql &= "AND m.categoria_id = @categoria_id "
            parametros.Add(New SqlParameter("@categoria_id", categoriaId))
        End If

        Dim sql As String =
            "SELECT " &
            "m.id, m.categoria_id, c.descripcion AS categoria, m.clave, m.motivo, m.descripcion, " &
            "m.impacto, m.probabilidad, m.nivel_riesgo_pld, m.nivel_severidad, " &
            "m.requiere_revision, m.activo " &
            "FROM dbo.catalogo_alerta_motivo m " &
            "INNER JOIN dbo.catalogo_alerta_categoria c ON c.id = m.categoria_id " &
            whereSql &
            "ORDER BY c.orden, m.motivo;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function ConsultarReglas(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim motivoId As Integer = ToInt(ObtenerParametro(context, "motivo_id"), 0)

        Dim parametros As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE r.activo = 1 "

        If motivoId > 0 Then
            whereSql &= "AND r.motivo_id = @motivo_id "
            parametros.Add(New SqlParameter("@motivo_id", motivoId))
        End If

        Dim sql As String =
            "SELECT " &
            "r.id, r.motivo_id, m.motivo, r.clave, r.nombre_regla, r.descripcion, " &
            "r.tipo_disparador, r.operador, r.valor_umbral, r.valor_texto, r.origen_tipo, " &
            "r.tipo_persona, r.evitar_duplicado_abierto, r.prioridad, r.activo " &
            "FROM dbo.catalogo_alerta_regla r " &
            "INNER JOIN dbo.catalogo_alerta_motivo m ON m.id = r.motivo_id " &
            whereSql &
            "ORDER BY r.prioridad DESC, r.nombre_regla;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function ConsultarBandeja(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim estatus As Integer = ToInt(ObtenerParametro(context, "estatus"), 0)
        Dim categoriaId As Integer = ToInt(ObtenerParametro(context, "categoria_id"), 0)
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_id"), 0)
        Dim texto As String = ObtenerParametro(context, "q").Trim()

        Dim parametros As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE activo = 1 "

        If estatus > 0 Then
            whereSql &= "AND estatus_alerta = @estatus "
            parametros.Add(New SqlParameter("@estatus", estatus))
        End If

        If categoriaId > 0 Then
            whereSql &= "AND categoria_id = @categoria_id "
            parametros.Add(New SqlParameter("@categoria_id", categoriaId))
        End If

        If clienteId > 0 Then
            whereSql &= "AND cliente_id = @cliente_id "
            parametros.Add(New SqlParameter("@cliente_id", clienteId))
        End If

        If solicitudId > 0 Then
            whereSql &= "AND solicitud_id = @solicitud_id "
            parametros.Add(New SqlParameter("@solicitud_id", solicitudId))
        End If

        If Not String.IsNullOrWhiteSpace(texto) Then
            whereSql &= "AND (folio LIKE @q OR cliente_nombre LIKE @q OR cliente_rfc LIKE @q OR titulo LIKE @q OR motivo LIKE @q) "
            parametros.Add(New SqlParameter("@q", "%" & texto & "%"))
        End If

        Dim sql As String =
            "SELECT TOP 500 " &
            "id, folio, fecha_generacion, categoria, motivo, nombre_regla, cliente_id, cliente_nombre, cliente_rfc, " &
            "solicitud_id, origen_evento, titulo, valor_detectado, valor_umbral, impacto, probabilidad, " &
            "nivel_riesgo_pld, prioridad, estatus_alerta, estatus_alerta_texto, asignado_a " &
            "FROM dbo.vw_alertas_pld_bandeja " &
            whereSql &
            "ORDER BY fecha_generacion DESC, prioridad DESC, id DESC;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function ObtenerAlerta(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim id As Integer = ToInt(ObtenerParametro(context, "id"), 0)

        If id <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de alerta inválido."
            Return respuesta
        End If

        Dim parametros As New List(Of SqlParameter)()
        parametros.Add(New SqlParameter("@id", id))

        Dim sql As String =
            "SELECT * " &
            "FROM dbo.vw_alertas_pld_bandeja " &
            "WHERE id = @id;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        If dt.Rows.Count = 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "No se encontró la alerta."
            Return respuesta
        End If

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)(0)

        Return respuesta
    End Function

    Private Function ConsultarBitacora(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim alertaId As Integer = ToInt(ObtenerParametro(context, "alerta_id"), 0)

        If alertaId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de alerta inválido."
            Return respuesta
        End If

        Dim parametros As New List(Of SqlParameter)()
        parametros.Add(New SqlParameter("@alerta_id", alertaId))

        Dim sql As String =
            "SELECT id, alerta_id, tipo_movimiento, estatus_anterior, estatus_nuevo, comentario, usuario, fecha_movimiento " &
            "FROM dbo.alertas_pld_bitacora " &
            "WHERE alerta_id = @alerta_id " &
            "ORDER BY fecha_movimiento DESC, id DESC;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function GenerarAlertaManual(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim usuario As String = ObtenerUsuario(context)
        Dim categoriaId As Integer = ToInt(ObtenerParametro(context, "categoria_id"), 0)
        Dim motivoId As Integer = ToInt(ObtenerParametro(context, "motivo_id"), 0)
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_id"), 0)
        Dim titulo As String = ObtenerParametro(context, "titulo").Trim()
        Dim descripcion As String = ObtenerParametro(context, "descripcion").Trim()
        Dim impacto As Integer = ToInt(ObtenerParametro(context, "impacto"), 1)
        Dim probabilidad As Integer = ToInt(ObtenerParametro(context, "probabilidad"), 1)
        Dim nivelRiesgo As Decimal? = ToDecimalNullable(ObtenerParametro(context, "nivel_riesgo_pld"))
        Dim prioridad As Integer = ToInt(ObtenerParametro(context, "prioridad"), 1)

        If categoriaId <= 0 OrElse motivoId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "Categoría y motivo son obligatorios."
            Return respuesta
        End If

        If String.IsNullOrWhiteSpace(titulo) Then
            respuesta("ok") = False
            respuesta("mensaje") = "El título de la alerta es obligatorio."
            Return respuesta
        End If

        If Not nivelRiesgo.HasValue Then
            nivelRiesgo = Convert.ToDecimal((impacto * probabilidad) / 100.0)
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim alertaId As Integer = InsertarAlerta(
                        cn,
                        tr,
                        categoriaId,
                        motivoId,
                        Nothing,
                        clienteId,
                        solicitudId,
                        "MANUAL",
                        Nothing,
                        Nothing,
                        titulo,
                        descripcion,
                        Nothing,
                        Nothing,
                        impacto,
                        probabilidad,
                        nivelRiesgo.Value,
                        prioridad,
                        usuario
                    )

                    InsertarBitacora(cn, tr, alertaId, "CREACION", Nothing, 1, "Alerta manual generada.", usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Alerta generada correctamente."
                    respuesta("alerta_id") = alertaId

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdeSolicitud(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_id"), 0)
        Dim usuario As String = ObtenerUsuario(context)

        If solicitudId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de solicitud inválido."
            Return respuesta
        End If

        Dim generadas As Integer = 0
        Dim omitidasDuplicado As Integer = 0
        Dim evaluadas As Integer = 0

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim solicitud As Dictionary(Of String, Object) = ObtenerSolicitudParaAlertas(cn, tr, solicitudId)

                    If solicitud Is Nothing Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró la solicitud."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim clienteId As Integer = ToInt(solicitud("cliente_id"), 0)
                    Dim montoSolicitado As Decimal = ObtenerDecimalSeguro(solicitud("monto_solicitado"))
                    Dim nivelSolicitud As Decimal = ObtenerDecimalSeguro(solicitud("pld_nivel_riesgo_pld"))
                    Dim pldFechaCalculoTieneValor As Boolean = TieneValorFecha(solicitud("pld_fecha_calculo"))

                    Dim reglas As DataTable = ObtenerReglasActivas(cn, tr)

                    For Each row As DataRow In reglas.Rows
                        Dim reglaId As Integer = Convert.ToInt32(row("id"))
                        Dim motivoId As Integer = Convert.ToInt32(row("motivo_id"))
                        Dim categoriaId As Integer = Convert.ToInt32(row("categoria_id"))
                        Dim tipoDisparador As String = Convert.ToString(row("tipo_disparador")).Trim().ToUpperInvariant()
                        Dim operador As String = Convert.ToString(row("operador")).Trim().ToUpperInvariant()

                        Dim valorUmbral As Decimal? = Nothing
                        If row("valor_umbral") IsNot DBNull.Value Then
                            valorUmbral = Convert.ToDecimal(row("valor_umbral"), CultureInfo.InvariantCulture)
                        End If

                        Dim prioridad As Integer = ToInt(row("prioridad"), 1)
                        Dim evitarDuplicado As Boolean = ToBool(row("evitar_duplicado_abierto"), True)
                        Dim valorDetectado As Decimal? = Nothing
                        Dim cumple As Boolean = False
                        Dim descripcionExtra As String = ""

                        evaluadas += 1

                        Select Case tipoDisparador

                            Case "MONTO_SOLICITADO"
                                valorDetectado = montoSolicitado
                                cumple = EvaluarDecimal(valorDetectado.Value, operador, valorUmbral)

                            Case "NIVEL_RIESGO_PLD"
                                valorDetectado = nivelSolicitud
                                cumple = EvaluarDecimal(valorDetectado.Value, operador, valorUmbral)

                            Case "SOLICITUD_SIN_CLIENTE"
                                valorDetectado = If(clienteId <= 0, 1D, 0D)
                                cumple = (clienteId <= 0)

                            Case "PLD_SIN_CALCULO"
                                valorDetectado = nivelSolicitud
                                cumple = (Not pldFechaCalculoTieneValor OrElse nivelSolicitud <= 0D)

                            Case "IDENTIFICACION_VENCIDA"
                                valorDetectado = If(ClienteTieneIdentificacionVencida(cn, tr, clienteId), 1D, 0D)
                                cumple = (valorDetectado.Value = 1D)

                            Case "SIN_DOMICILIO_ACTIVO"
                                valorDetectado = If(TieneDomicilioActivo(cn, tr, solicitudId), 0D, 1D)
                                cumple = (valorDetectado.Value = 1D)

                            Case "SIN_CONTACTO_ACTIVO"
                                valorDetectado = If(TieneContactoActivo(cn, tr, solicitudId), 0D, 1D)
                                cumple = (valorDetectado.Value = 1D)

                            Case "PAIS_RIESGO_ALTO"
                                Dim nivelPais As Decimal = ObtenerNivelPaisRiesgoAlto(cn, tr, clienteId, solicitudId, valorUmbral)
                                valorDetectado = nivelPais
                                cumple = EvaluarDecimal(nivelPais, operador, valorUmbral)

                            Case Else
                                cumple = False

                        End Select

                        If cumple Then
                            If evitarDuplicado AndAlso ExisteAlertaAbierta(cn, tr, clienteId, solicitudId, motivoId, reglaId) Then
                                omitidasDuplicado += 1
                            Else
                                Dim titulo As String = Convert.ToString(row("motivo"))
                                Dim descripcion As String = Convert.ToString(row("descripcion"))

                                If String.IsNullOrWhiteSpace(descripcion) Then
                                    descripcion = Convert.ToString(row("nombre_regla"))
                                End If

                                If Not String.IsNullOrWhiteSpace(descripcionExtra) Then
                                    descripcion = descripcion & " " & descripcionExtra
                                End If

                                Dim impacto As Integer = ToInt(row("impacto"), 1)
                                Dim probabilidad As Integer = ToInt(row("probabilidad"), 1)
                                Dim nivelRiesgo As Decimal = Convert.ToDecimal(If(row("nivel_riesgo_pld") Is DBNull.Value, 0D, row("nivel_riesgo_pld")), CultureInfo.InvariantCulture)

                                Dim alertaId As Integer = InsertarAlerta(
                                    cn,
                                    tr,
                                    categoriaId,
                                    motivoId,
                                    reglaId,
                                    clienteId,
                                    solicitudId,
                                    "SOLICITUD",
                                    "solicitud_credito",
                                    solicitudId,
                                    titulo,
                                    descripcion,
                                    valorDetectado,
                                    valorUmbral,
                                    impacto,
                                    probabilidad,
                                    nivelRiesgo,
                                    prioridad,
                                    usuario
                                )

                                InsertarBitacora(cn, tr, alertaId, "CREACION", Nothing, 1, "Alerta generada automáticamente desde solicitud.", usuario)

                                generadas += 1
                            End If
                        End If
                    Next

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación de alertas finalizada."
                    respuesta("evaluadas") = evaluadas
                    respuesta("generadas") = generadas
                    respuesta("omitidas_duplicado") = omitidasDuplicado

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdePago(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)

        If pagoId <= 0 Then
            pagoId = ToInt(ObtenerParametro(context, "pago_id"), 0)
        End If

        Return GenerarAlertasDesdePagoInterno(pagoId, ObtenerUsuario(context))
    End Function

    Private Function GenerarAlertasDesdePagoMensual(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim anio As Integer = ToInt(ObtenerParametro(context, "anio"), 0)
        Dim mes As Integer = ToInt(ObtenerParametro(context, "mes"), 0)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        Dim usuario As String = ObtenerUsuario(context)

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    If pagoId > 0 AndAlso (clienteId <= 0 OrElse anio <= 0 OrElse mes <= 0) Then
                        Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)

                        If pago Is Nothing Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "No se encontró el pago."
                            tr.Rollback()
                            Return respuesta
                        End If

                        clienteId = ObtenerEnteroColumna(pago, "cliente_id")
                        anio = ObtenerEnteroColumna(pago, "anio_pago")
                        mes = ObtenerEnteroColumna(pago, "mes_pago")
                    End If

                    If clienteId <= 0 OrElse anio <= 0 OrElse mes <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Debe indicar cliente_id, anio y mes, o un pago_credito_id válido."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim resultado As Dictionary(Of String, Integer) = EvaluarPagoMensual(cn, tr, clienteId, anio, mes, usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación mensual de pagos finalizada."
                    respuesta("evaluadas") = resultado("evaluadas")
                    respuesta("generadas") = resultado("generadas")
                    respuesta("omitidas_duplicado") = resultado("omitidas_duplicado")

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdePerfilTransaccional(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim anio As Integer = ToInt(ObtenerParametro(context, "anio"), 0)
        Dim mes As Integer = ToInt(ObtenerParametro(context, "mes"), 0)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        Dim usuario As String = ObtenerUsuario(context)

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    If pagoId > 0 AndAlso (clienteId <= 0 OrElse anio <= 0 OrElse mes <= 0) Then
                        Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)
                        If pago Is Nothing Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "No se encontró el pago."
                            tr.Rollback()
                            Return respuesta
                        End If
                        clienteId = ObtenerEnteroColumna(pago, "cliente_id")
                        anio = ObtenerEnteroColumna(pago, "anio_pago")
                        mes = ObtenerEnteroColumna(pago, "mes_pago")
                    End If

                    If clienteId <= 0 OrElse anio <= 0 OrElse mes <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Debe indicar cliente_id, anio y mes, o un pago_credito_id válido."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim resultado As Dictionary(Of String, Integer) = EvaluarPerfilTransaccional(cn, tr, clienteId, anio, mes, usuario)

                    tr.Commit()
                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación de perfil transaccional finalizada."
                    respuesta("evaluadas") = resultado("evaluadas")
                    respuesta("generadas") = resultado("generadas")
                    respuesta("omitidas_duplicado") = resultado("omitidas_duplicado")
                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdePagoCredito(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim solicitudId As Integer = ToInt(ObtenerParametro(context, "solicitud_id"), 0)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        Dim usuario As String = ObtenerUsuario(context)

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    If pagoId > 0 AndAlso solicitudId <= 0 Then
                        Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)

                        If pago Is Nothing Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "No se encontró el pago."
                            tr.Rollback()
                            Return respuesta
                        End If

                        solicitudId = ObtenerEnteroColumna(pago, "solicitud_credito_id")
                    End If

                    If solicitudId <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Debe indicar solicitud_id o pago_credito_id válido."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim resultado As Dictionary(Of String, Integer) = EvaluarPagoCredito(cn, tr, solicitudId, usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación por crédito finalizada."
                    respuesta("evaluadas") = resultado("evaluadas")
                    respuesta("generadas") = resultado("generadas")
                    respuesta("omitidas_duplicado") = resultado("omitidas_duplicado")

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdeClientePeriodo(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim clienteId As Integer = ToInt(ObtenerParametro(context, "cliente_id"), 0)
        Dim solicitudIdBase As Integer = ToInt(ObtenerParametro(context, "solicitud_id"), 0)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)
        Dim usuario As String = ObtenerUsuario(context)

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    If pagoId > 0 AndAlso (clienteId <= 0 OrElse solicitudIdBase <= 0) Then
                        Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)

                        If pago Is Nothing Then
                            respuesta("ok") = False
                            respuesta("mensaje") = "No se encontró el pago."
                            tr.Rollback()
                            Return respuesta
                        End If

                        clienteId = ObtenerEnteroColumna(pago, "cliente_id")
                        solicitudIdBase = ObtenerEnteroColumna(pago, "solicitud_credito_id")
                    End If

                    If clienteId <= 0 OrElse solicitudIdBase <= 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Debe indicar cliente_id y solicitud_id, o un pago_credito_id válido."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim resultado As Dictionary(Of String, Integer) = EvaluarClientePeriodo(cn, tr, clienteId, solicitudIdBase, usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación por cliente-periodo finalizada."
                    respuesta("evaluadas") = resultado("evaluadas")
                    respuesta("generadas") = resultado("generadas")
                    respuesta("omitidas_duplicado") = resultado("omitidas_duplicado")

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdePagoCompleto(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim pagoId As Integer = ToInt(ObtenerParametro(context, "pago_credito_id"), 0)

        If pagoId <= 0 Then
            pagoId = ToInt(ObtenerParametro(context, "pago_id"), 0)
        End If

        If pagoId <= 0 Then
            Dim respuestaInvalida As New Dictionary(Of String, Object)()
            respuestaInvalida("ok") = False
            respuestaInvalida("mensaje") = "ID de pago inválido."
            Return respuestaInvalida
        End If

        Dim respuesta As New Dictionary(Of String, Object)()
        Dim usuario As String = ObtenerUsuario(context)

        Dim totalEvaluadas As Integer = 0
        Dim totalGeneradas As Integer = 0
        Dim totalOmitidas As Integer = 0

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)

                    If pago Is Nothing Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró el pago."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim clienteId As Integer = ObtenerEnteroColumna(pago, "cliente_id")
                    Dim solicitudId As Integer = ObtenerEnteroColumna(pago, "solicitud_credito_id")
                    Dim anio As Integer = ObtenerEnteroColumna(pago, "anio_pago")
                    Dim mes As Integer = ObtenerEnteroColumna(pago, "mes_pago")

                    Dim rPago As Dictionary(Of String, Integer) = EvaluarPago(cn, tr, pago, usuario)
                    totalEvaluadas += rPago("evaluadas")
                    totalGeneradas += rPago("generadas")
                    totalOmitidas += rPago("omitidas_duplicado")

                    If clienteId > 0 AndAlso anio > 0 AndAlso mes > 0 Then
                        Dim rMensual As Dictionary(Of String, Integer) = EvaluarPagoMensual(cn, tr, clienteId, anio, mes, usuario)
                        totalEvaluadas += rMensual("evaluadas")
                        totalGeneradas += rMensual("generadas")
                        totalOmitidas += rMensual("omitidas_duplicado")

                        Dim rPerfil As Dictionary(Of String, Integer) = EvaluarPerfilTransaccional(cn, tr, clienteId, anio, mes, usuario)
                        totalEvaluadas += rPerfil("evaluadas")
                        totalGeneradas += rPerfil("generadas")
                        totalOmitidas += rPerfil("omitidas_duplicado")
                    End If

                    If solicitudId > 0 Then
                        Dim rCredito As Dictionary(Of String, Integer) = EvaluarPagoCredito(cn, tr, solicitudId, usuario)
                        totalEvaluadas += rCredito("evaluadas")
                        totalGeneradas += rCredito("generadas")
                        totalOmitidas += rCredito("omitidas_duplicado")
                    End If

                    If clienteId > 0 AndAlso solicitudId > 0 Then
                        Dim rPeriodo As Dictionary(Of String, Integer) = EvaluarClientePeriodo(cn, tr, clienteId, solicitudId, usuario)
                        totalEvaluadas += rPeriodo("evaluadas")
                        totalGeneradas += rPeriodo("generadas")
                        totalOmitidas += rPeriodo("omitidas_duplicado")
                    End If

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación completa de alertas desde pago finalizada."
                    respuesta("evaluadas") = totalEvaluadas
                    respuesta("generadas") = totalGeneradas
                    respuesta("omitidas_duplicado") = totalOmitidas

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function GenerarAlertasDesdePagoInterno(ByVal pagoId As Integer, ByVal usuario As String) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        If pagoId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de pago inválido."
            Return respuesta
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim pago As DataRow = ObtenerFilaPorPago(cn, tr, pagoId)

                    If pago Is Nothing Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "No se encontró el pago."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim resultado As Dictionary(Of String, Integer) = EvaluarPago(cn, tr, pago, usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Evaluación de alertas desde pago finalizada."
                    respuesta("evaluadas") = resultado("evaluadas")
                    respuesta("generadas") = resultado("generadas")
                    respuesta("omitidas_duplicado") = resultado("omitidas_duplicado")

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function EvaluarPago(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal pago As DataRow, ByVal usuario As String) As Dictionary(Of String, Integer)
        Dim reglas As DataTable = ObtenerReglasActivasPorTipo(cn, tr, "PAGO")
        Dim pagoId As Integer = ObtenerEnteroColumna(pago, "pago_credito_id")

        Return EvaluarReglasSobreFila(
            cn,
            tr,
            reglas,
            pago,
            "PAGO",
            "pagos_credito",
            pagoId,
            usuario,
            "Alerta generada automáticamente desde pago."
        )
    End Function

    Private Function EvaluarPagoMensual(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal anio As Integer, ByVal mes As Integer, ByVal usuario As String) As Dictionary(Of String, Integer)
        Dim fila As DataRow = ObtenerFilaPagoMensual(cn, tr, clienteId, anio, mes)

        If fila Is Nothing Then
            Return CrearResultadoEvaluacion()
        End If

        Dim reglas As DataTable = ObtenerReglasActivasPorTipo(cn, tr, "PAGO_MENSUAL")
        Dim referenciaTabla As String = "vw_pagos_credito_pld_mensual_" & anio.ToString() & "_" & mes.ToString("00") & "_cliente_" & clienteId.ToString()

        Return EvaluarReglasSobreFila(
            cn,
            tr,
            reglas,
            fila,
            "PAGO_MENSUAL",
            referenciaTabla,
            clienteId,
            usuario,
            "Alerta generada automáticamente desde acumulado mensual de pagos."
        )
    End Function

    Private Function EvaluarPagoCredito(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer, ByVal usuario As String) As Dictionary(Of String, Integer)
        Dim fila As DataRow = ObtenerFilaPagoCredito(cn, tr, solicitudId)

        If fila Is Nothing Then
            Return CrearResultadoEvaluacion()
        End If

        Dim reglas As DataTable = ObtenerReglasActivasPorTipo(cn, tr, "PAGO_CREDITO")

        Return EvaluarReglasSobreFila(
            cn,
            tr,
            reglas,
            fila,
            "PAGO_CREDITO",
            "vw_pagos_credito_pld_credito",
            solicitudId,
            usuario,
            "Alerta generada automáticamente desde resumen de pagos por crédito."
        )
    End Function

    Private Function EvaluarClientePeriodo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal solicitudIdBase As Integer, ByVal usuario As String) As Dictionary(Of String, Integer)
        Dim fila As DataRow = ObtenerFilaClientePeriodo(cn, tr, clienteId, solicitudIdBase)

        If fila Is Nothing Then
            Return CrearResultadoEvaluacion()
        End If

        Dim reglas As DataTable = ObtenerReglasActivasPorTipo(cn, tr, "CLIENTE_PERIODO")

        Return EvaluarReglasSobreFila(
            cn,
            tr,
            reglas,
            fila,
            "CLIENTE_PERIODO",
            "vw_pagos_credito_pld_cliente_periodo",
            solicitudIdBase,
            usuario,
            "Alerta generada automáticamente desde patrón de cliente por periodo."
        )
    End Function

    Private Function EvaluarPerfilTransaccional(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal anio As Integer, ByVal mes As Integer, ByVal usuario As String) As Dictionary(Of String, Integer)
        Dim fila As DataRow = ObtenerFilaPerfilTransaccional(cn, tr, clienteId, anio, mes)
        If fila Is Nothing Then Return CrearResultadoEvaluacion()

        Dim reglas As DataTable = ObtenerReglasActivasPorTipo(cn, tr, "PERFIL_TRANSACCIONAL")

        Return EvaluarReglasSobreFila(
            cn,
            tr,
            reglas,
            fila,
            "PERFIL_TRANSACCIONAL",
            "vw_cliente_perfil_transaccional_mensual",
            clienteId,
            usuario,
            "Alerta generada automáticamente desde comparación de perfil transaccional esperado contra pagos reales."
        )
    End Function

    Private Function EvaluarReglasSobreFila(
        ByVal cn As SqlConnection,
        ByVal tr As SqlTransaction,
        ByVal reglas As DataTable,
        ByVal fila As DataRow,
        ByVal origenEvento As String,
        ByVal referenciaTabla As String,
        ByVal referenciaId As Integer,
        ByVal usuario As String,
        ByVal comentarioBitacora As String
    ) As Dictionary(Of String, Integer)

        Dim resultado As Dictionary(Of String, Integer) = CrearResultadoEvaluacion()

        Dim clienteId As Integer = ObtenerEnteroColumna(fila, "cliente_id")
        Dim solicitudId As Integer = ObtenerEnteroColumna(fila, "solicitud_credito_id")

        If solicitudId <= 0 Then
            solicitudId = ObtenerEnteroColumna(fila, "solicitud_credito_id_base")
        End If

        For Each row As DataRow In reglas.Rows
            resultado("evaluadas") += 1

            Dim reglaId As Integer = Convert.ToInt32(row("id"))
            Dim motivoId As Integer = Convert.ToInt32(row("motivo_id"))
            Dim categoriaId As Integer = Convert.ToInt32(row("categoria_id"))
            Dim operador As String = Convert.ToString(row("operador")).Trim().ToUpperInvariant()
            Dim campoValor As String = Convert.ToString(row("valor_texto")).Trim()
            Dim prioridad As Integer = ToInt(row("prioridad"), 1)
            Dim evitarDuplicado As Boolean = ToBool(row("evitar_duplicado_abierto"), True)

            Dim valorUmbral As Decimal? = Nothing
            If row("valor_umbral") IsNot DBNull.Value Then
                valorUmbral = Convert.ToDecimal(row("valor_umbral"), CultureInfo.InvariantCulture)
            End If

            If String.IsNullOrWhiteSpace(campoValor) Then
                Continue For
            End If

            Dim valorDetectado As Decimal = ObtenerDecimalColumna(fila, campoValor)
            Dim cumple As Boolean = EvaluarDecimal(valorDetectado, operador, valorUmbral)

            If cumple Then
                If evitarDuplicado AndAlso ExisteAlertaAbiertaPorReferencia(cn, tr, reglaId, origenEvento, referenciaTabla, referenciaId) Then
                    resultado("omitidas_duplicado") += 1
                Else
                    Dim titulo As String = Convert.ToString(row("motivo"))
                    Dim descripcion As String = Convert.ToString(row("descripcion"))

                    If String.IsNullOrWhiteSpace(descripcion) Then
                        descripcion = Convert.ToString(row("nombre_regla"))
                    End If

                    descripcion &= " Valor detectado: " & valorDetectado.ToString("0.####", CultureInfo.InvariantCulture) & ". Campo evaluado: " & campoValor & "."

                    Dim impacto As Integer = ToInt(row("impacto"), 1)
                    Dim probabilidad As Integer = ToInt(row("probabilidad"), 1)
                    Dim nivelRiesgo As Decimal = Convert.ToDecimal(If(row("nivel_riesgo_pld") Is DBNull.Value, 0D, row("nivel_riesgo_pld")), CultureInfo.InvariantCulture)

                    Try
                        Dim alertaId As Integer = InsertarAlerta(
                            cn,
                            tr,
                            categoriaId,
                            motivoId,
                            reglaId,
                            clienteId,
                            solicitudId,
                            origenEvento,
                            referenciaTabla,
                            referenciaId,
                            titulo,
                            descripcion,
                            valorDetectado,
                            valorUmbral,
                            impacto,
                            probabilidad,
                            nivelRiesgo,
                            prioridad,
                            usuario
                        )

                        InsertarBitacora(cn, tr, alertaId, "CREACION", Nothing, 1, comentarioBitacora, usuario)

                        resultado("generadas") += 1

                    Catch exSql As SqlException
                        If EsErrorDuplicado(exSql) Then
                            resultado("omitidas_duplicado") += 1
                        Else
                            Throw
                        End If
                    End Try
                End If
            End If
        Next

        Return resultado
    End Function

    Private Function CrearResultadoEvaluacion() As Dictionary(Of String, Integer)
        Dim resultado As New Dictionary(Of String, Integer)()
        resultado("evaluadas") = 0
        resultado("generadas") = 0
        resultado("omitidas_duplicado") = 0
        Return resultado
    End Function

    Private Function ObtenerFilaPorPago(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal pagoId As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_pagos_credito_pld " &
            "WHERE pago_credito_id = @pago_credito_id;"

        Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr, sql, New List(Of SqlParameter) From {
            New SqlParameter("@pago_credito_id", pagoId)
        })

        If dt.Rows.Count = 0 Then Return Nothing

        Return dt.Rows(0)
    End Function

    Private Function ObtenerFilaPagoMensual(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal anio As Integer, ByVal mes As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_pagos_credito_pld_mensual " &
            "WHERE cliente_id = @cliente_id " &
            "AND anio_pago = @anio " &
            "AND mes_pago = @mes;"

        Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr, sql, New List(Of SqlParameter) From {
            New SqlParameter("@cliente_id", clienteId),
            New SqlParameter("@anio", anio),
            New SqlParameter("@mes", mes)
        })

        If dt.Rows.Count = 0 Then Return Nothing

        Return dt.Rows(0)
    End Function

    Private Function ObtenerFilaPagoCredito(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_pagos_credito_pld_credito " &
            "WHERE solicitud_credito_id = @solicitud_id;"

        Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr, sql, New List(Of SqlParameter) From {
            New SqlParameter("@solicitud_id", solicitudId)
        })

        If dt.Rows.Count = 0 Then Return Nothing

        Return dt.Rows(0)
    End Function

    Private Function ObtenerFilaClientePeriodo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal solicitudIdBase As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_pagos_credito_pld_cliente_periodo " &
            "WHERE cliente_id = @cliente_id " &
            "AND solicitud_credito_id_base = @solicitud_id_base " &
            "ORDER BY fecha_base_periodo DESC;"

        Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr, sql, New List(Of SqlParameter) From {
            New SqlParameter("@cliente_id", clienteId),
            New SqlParameter("@solicitud_id_base", solicitudIdBase)
        })

        If dt.Rows.Count = 0 Then Return Nothing

        Return dt.Rows(0)
    End Function

    Private Function ObtenerFilaPerfilTransaccional(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal anio As Integer, ByVal mes As Integer) As DataRow
        Dim sql As String =
            "SELECT TOP 1 * " &
            "FROM dbo.vw_cliente_perfil_transaccional_mensual " &
            "WHERE cliente_id = @cliente_id AND anio = @anio AND mes = @mes;"

        Dim dt As DataTable = EjecutarTablaTransaccion(cn, tr, sql, New List(Of SqlParameter) From {
            New SqlParameter("@cliente_id", clienteId),
            New SqlParameter("@anio", anio),
            New SqlParameter("@mes", mes)
        })

        If dt.Rows.Count = 0 Then Return Nothing
        Return dt.Rows(0)
    End Function

    Private Function EjecutarTablaTransaccion(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal sql As String, ByVal parametros As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.CommandType = CommandType.Text

            If parametros IsNot Nothing Then
                cmd.Parameters.AddRange(parametros.ToArray())
            End If

            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using

        Return dt
    End Function

    Private Function ObtenerEnteroColumna(ByVal fila As DataRow, ByVal columna As String, Optional ByVal defaultValue As Integer = 0) As Integer
        If fila Is Nothing Then Return defaultValue
        If Not fila.Table.Columns.Contains(columna) Then Return defaultValue
        If fila(columna) Is DBNull.Value Then Return defaultValue

        Return ToInt(fila(columna), defaultValue)
    End Function

    Private Function ObtenerDecimalColumna(ByVal fila As DataRow, ByVal columna As String, Optional ByVal defaultValue As Decimal = 0D) As Decimal
        If fila Is Nothing Then Return defaultValue
        If Not fila.Table.Columns.Contains(columna) Then Return defaultValue
        If fila(columna) Is DBNull.Value Then Return defaultValue

        Return ObtenerDecimalSeguro(fila(columna))
    End Function

    Private Function EsErrorDuplicado(ByVal ex As SqlException) As Boolean
        For Each err As SqlError In ex.Errors
            If err.Number = 2601 OrElse err.Number = 2627 Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function CambiarEstatusAlerta(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim alertaId As Integer = ToInt(ObtenerParametro(context, "alerta_id"), 0)
        Dim nuevoEstatus As Integer = ToInt(ObtenerParametro(context, "estatus_alerta"), 0)
        Dim comentario As String = ObtenerParametro(context, "comentario").Trim()
        Dim resolucion As String = ObtenerParametro(context, "resolucion").Trim()
        Dim usuario As String = ObtenerUsuario(context)

        If alertaId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de alerta inválido."
            Return respuesta
        End If

        If nuevoEstatus < 1 OrElse nuevoEstatus > 5 Then
            respuesta("ok") = False
            respuesta("mensaje") = "Estatus inválido."
            Return respuesta
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim estatusAnterior As Integer = ObtenerEstatusActual(cn, tr, alertaId)

                    Dim sql As String =
                        "UPDATE dbo.alertas_pld SET " &
                        "estatus_alerta = @estatus_alerta, " &
                        "observaciones = CASE WHEN @comentario = '' THEN observaciones ELSE @comentario END, " &
                        "resolucion = CASE WHEN @resolucion = '' THEN resolucion ELSE @resolucion END, " &
                        "fecha_revision = CASE WHEN @estatus_alerta IN (2,3,4,5) AND fecha_revision IS NULL THEN SYSDATETIME() ELSE fecha_revision END, " &
                        "fecha_cierre = CASE WHEN @estatus_alerta IN (4,5) THEN SYSDATETIME() ELSE fecha_cierre END, " &
                        "modificado_por = @usuario, " &
                        "fecha_modificacion = SYSDATETIME() " &
                        "WHERE id = @id;"

                    Using cmd As New SqlCommand(sql, cn, tr)
                        cmd.Parameters.AddWithValue("@estatus_alerta", nuevoEstatus)
                        cmd.Parameters.AddWithValue("@comentario", comentario)
                        cmd.Parameters.AddWithValue("@resolucion", resolucion)
                        cmd.Parameters.AddWithValue("@usuario", usuario)
                        cmd.Parameters.AddWithValue("@id", alertaId)
                        cmd.ExecuteNonQuery()
                    End Using

                    InsertarBitacora(cn, tr, alertaId, "CAMBIO_ESTATUS", estatusAnterior, nuevoEstatus, comentario, usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Estatus actualizado correctamente."

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function AsignarAlerta(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim alertaId As Integer = ToInt(ObtenerParametro(context, "alerta_id"), 0)
        Dim asignadoA As String = ObtenerParametro(context, "asignado_a").Trim()
        Dim usuario As String = ObtenerUsuario(context)

        If alertaId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID de alerta inválido."
            Return respuesta
        End If

        If String.IsNullOrWhiteSpace(asignadoA) Then
            respuesta("ok") = False
            respuesta("mensaje") = "Debe indicar a quién se asigna la alerta."
            Return respuesta
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim sql As String =
                        "UPDATE dbo.alertas_pld SET " &
                        "asignado_a = @asignado_a, " &
                        "fecha_asignacion = SYSDATETIME(), " &
                        "modificado_por = @usuario, " &
                        "fecha_modificacion = SYSDATETIME() " &
                        "WHERE id = @id;"

                    Using cmd As New SqlCommand(sql, cn, tr)
                        cmd.Parameters.AddWithValue("@asignado_a", asignadoA)
                        cmd.Parameters.AddWithValue("@usuario", usuario)
                        cmd.Parameters.AddWithValue("@id", alertaId)
                        cmd.ExecuteNonQuery()
                    End Using

                    InsertarBitacora(cn, tr, alertaId, "ASIGNACION", Nothing, Nothing, "Alerta asignada a " & asignadoA & ".", usuario)

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Alerta asignada correctamente."

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function ObtenerSolicitudParaAlertas(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As Dictionary(Of String, Object)
        Dim sql As String =
            "SELECT id, cliente_id, monto_solicitado, pld_nivel_riesgo_pld, pld_fecha_calculo " &
            "FROM dbo.solicitud_credito " &
            "WHERE id = @id AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@id", solicitudId)

            Using rd As SqlDataReader = cmd.ExecuteReader()
                If rd.Read() Then
                    Dim item As New Dictionary(Of String, Object)()
                    item("id") = rd("id")
                    item("cliente_id") = rd("cliente_id")
                    item("monto_solicitado") = rd("monto_solicitado")
                    item("pld_nivel_riesgo_pld") = rd("pld_nivel_riesgo_pld")
                    item("pld_fecha_calculo") = rd("pld_fecha_calculo")
                    Return item
                End If
            End Using
        End Using

        Return Nothing
    End Function

    Private Function ObtenerDecimalSeguro(ByVal valor As Object) As Decimal
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return 0D

        Dim resultado As Decimal = 0D
        Decimal.TryParse(Convert.ToString(valor, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, resultado)

        Return resultado
    End Function

    Private Function TieneValorFecha(ByVal valor As Object) As Boolean
        If valor Is Nothing OrElse valor Is DBNull.Value Then Return False

        Dim fecha As DateTime
        If DateTime.TryParse(Convert.ToString(valor, CultureInfo.InvariantCulture), fecha) Then
            Return True
        End If

        Return False
    End Function

    Private Function ClienteTieneIdentificacionVencida(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer) As Boolean
        If clienteId <= 0 Then Return False

        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.cliente_persona_fisica " &
            "WHERE id_cliente = @cliente_id " &
            "AND vigencia_identificacion IS NOT NULL " &
            "AND CAST(vigencia_identificacion AS DATE) < CAST(GETDATE() AS DATE);"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@cliente_id", clienteId)
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function TieneDomicilioActivo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.contacto_solicitud cs " &
            "INNER JOIN dbo.contacto_solicitud_domicilio d ON d.contacto_id = cs.id " &
            "WHERE cs.solicitud_id = @solicitud_id " &
            "AND cs.activo = 1 " &
            "AND d.activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@solicitud_id", solicitudId)
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function TieneContactoActivo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal solicitudId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.contacto_solicitud cs " &
            "WHERE cs.solicitud_id = @solicitud_id " &
            "AND cs.activo = 1 " &
            "AND ( " &
            "    EXISTS (SELECT 1 FROM dbo.contacto_solicitud_telefono t WHERE t.contacto_id = cs.id AND t.activo = 1) " &
            "    OR EXISTS (SELECT 1 FROM dbo.contacto_solicitud_email e WHERE e.contacto_id = cs.id AND e.activo = 1) " &
            ");"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@solicitud_id", solicitudId)
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function ObtenerNivelPaisRiesgoAlto(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal solicitudId As Integer, ByVal valorUmbral As Decimal?) As Decimal
        If clienteId <= 0 Then Return 0D

        Dim umbral As Decimal = 0.09D

        If valorUmbral.HasValue Then
            umbral = valorUmbral.Value
        End If

        Dim sql As String =
            "DECLARE @nivel DECIMAL(18,4) = 0; " &
            "SELECT @nivel = MAX(x.nivel) " &
            "FROM ( " &
            "    SELECT ISNULL(f.nivel, 0) AS nivel " &
            "    FROM dbo.cliente_persona_fisica c " &
            "    INNER JOIN dbo.vw_pld_factores f " &
            "        ON f.origen_tipo = N'pais' " &
            "       AND f.referencia_id = c.pais_nacimiento_id " &
            "       AND f.activo = 1 " &
            "    WHERE c.id_cliente = @cliente_id " &
            " " &
            "    UNION ALL " &
            " " &
            "    SELECT ISNULL(f.nivel, 0) AS nivel " &
            "    FROM dbo.contacto_solicitud cs " &
            "    INNER JOIN dbo.contacto_solicitud_domicilio d " &
            "        ON d.contacto_id = cs.id " &
            "       AND d.activo = 1 " &
            "    INNER JOIN dbo.catalogo_paises p " &
            "        ON UPPER(LTRIM(RTRIM(p.pais))) = UPPER(LTRIM(RTRIM(d.pais))) " &
            "    INNER JOIN dbo.vw_pld_factores f " &
            "        ON f.origen_tipo = N'pais_domicilio' " &
            "       AND f.referencia_id = p.id " &
            "       AND f.activo = 1 " &
            "    WHERE cs.solicitud_id = @solicitud_id " &
            "      AND cs.activo = 1 " &
            ") x " &
            "WHERE x.nivel >= @umbral; " &
            "SELECT ISNULL(@nivel, 0);"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@cliente_id", clienteId)
            cmd.Parameters.AddWithValue("@solicitud_id", solicitudId)
            cmd.Parameters.AddWithValue("@umbral", umbral)

            Return Convert.ToDecimal(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

    Private Function ObtenerReglasActivas(ByVal cn As SqlConnection, ByVal tr As SqlTransaction) As DataTable
        Dim dt As New DataTable()

        Dim sql As String =
            "SELECT " &
            "r.id, r.motivo_id, m.categoria_id, r.clave, r.nombre_regla, r.descripcion, " &
            "r.tipo_disparador, r.operador, r.valor_umbral, r.valor_texto, r.origen_tipo, " &
            "r.evitar_duplicado_abierto, r.prioridad, " &
            "m.motivo, m.impacto, m.probabilidad, m.nivel_riesgo_pld " &
            "FROM dbo.catalogo_alerta_regla r " &
            "INNER JOIN dbo.catalogo_alerta_motivo m ON m.id = r.motivo_id " &
            "WHERE r.activo = 1 AND m.activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using

        Return dt
    End Function

    Private Function ObtenerReglasActivasPorTipo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal tipoDisparador As String) As DataTable
        Dim dt As New DataTable()

        Dim sql As String =
            "SELECT " &
            "r.id, r.motivo_id, m.categoria_id, r.clave, r.nombre_regla, r.descripcion, " &
            "r.tipo_disparador, r.operador, r.valor_umbral, r.valor_texto, r.origen_tipo, " &
            "r.evitar_duplicado_abierto, r.prioridad, " &
            "m.motivo, m.impacto, m.probabilidad, m.nivel_riesgo_pld " &
            "FROM dbo.catalogo_alerta_regla r " &
            "INNER JOIN dbo.catalogo_alerta_motivo m ON m.id = r.motivo_id " &
            "WHERE r.activo = 1 " &
            "AND m.activo = 1 " &
            "AND UPPER(LTRIM(RTRIM(r.tipo_disparador))) = @tipo_disparador " &
            "ORDER BY r.prioridad DESC, r.id ASC;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@tipo_disparador", tipoDisparador.Trim().ToUpperInvariant())

            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using

        Return dt
    End Function

    Private Function EvaluarDecimal(ByVal valorDetectado As Decimal, ByVal operador As String, ByVal valorUmbral As Decimal?) As Boolean
        If Not valorUmbral.HasValue Then Return False

        Select Case operador
            Case ">="
                Return valorDetectado >= valorUmbral.Value
            Case ">"
                Return valorDetectado > valorUmbral.Value
            Case "<="
                Return valorDetectado <= valorUmbral.Value
            Case "<"
                Return valorDetectado < valorUmbral.Value
            Case "="
                Return valorDetectado = valorUmbral.Value
            Case "<>"
                Return valorDetectado <> valorUmbral.Value
            Case Else
                Return False
        End Select
    End Function

    Private Function ExisteAlertaAbierta(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal clienteId As Integer, ByVal solicitudId As Integer, ByVal motivoId As Integer, ByVal reglaId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.alertas_pld " &
            "WHERE activo = 1 " &
            "AND estatus_alerta IN (1,2,3) " &
            "AND ISNULL(cliente_id,0) = @cliente_id " &
            "AND ISNULL(solicitud_id,0) = @solicitud_id " &
            "AND motivo_id = @motivo_id " &
            "AND ISNULL(regla_id,0) = @regla_id;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@cliente_id", clienteId)
            cmd.Parameters.AddWithValue("@solicitud_id", solicitudId)
            cmd.Parameters.AddWithValue("@motivo_id", motivoId)
            cmd.Parameters.AddWithValue("@regla_id", reglaId)

            Dim total As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            Return total > 0
        End Using
    End Function

    Private Function ExisteAlertaAbiertaPorReferencia(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal reglaId As Integer, ByVal origenEvento As String, ByVal referenciaTabla As String, ByVal referenciaId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.alertas_pld " &
            "WHERE activo = 1 " &
            "AND estatus_alerta IN (1,2,3) " &
            "AND ISNULL(regla_id,0) = @regla_id " &
            "AND origen_evento = @origen_evento " &
            "AND ISNULL(referencia_tabla,'') = @referencia_tabla " &
            "AND ISNULL(referencia_id,0) = @referencia_id;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@regla_id", reglaId)
            cmd.Parameters.AddWithValue("@origen_evento", origenEvento)
            cmd.Parameters.AddWithValue("@referencia_tabla", referenciaTabla)
            cmd.Parameters.AddWithValue("@referencia_id", referenciaId)

            Dim total As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            Return total > 0
        End Using
    End Function

    Private Function InsertarAlerta(
    ByVal cn As SqlConnection,
    ByVal tr As SqlTransaction,
    ByVal categoriaId As Integer,
    ByVal motivoId As Integer,
    ByVal reglaId As Integer?,
    ByVal clienteId As Integer,
    ByVal solicitudId As Integer,
    ByVal origenEvento As String,
    ByVal referenciaTabla As String,
    ByVal referenciaId As Integer?,
    ByVal titulo As String,
    ByVal descripcion As String,
    ByVal valorDetectado As Decimal?,
    ByVal valorUmbral As Decimal?,
    ByVal impacto As Integer,
    ByVal probabilidad As Integer,
    ByVal nivelRiesgoPld As Decimal,
    ByVal prioridad As Integer,
    ByVal usuario As String
) As Integer

        Dim folio As String = GenerarFolioAlerta(cn, tr)

        Dim sql As String =
        "INSERT INTO dbo.alertas_pld " &
        "(folio, categoria_id, motivo_id, regla_id, cliente_id, solicitud_id, origen_evento, referencia_tabla, referencia_id, " &
        "titulo, descripcion, valor_detectado, valor_umbral, impacto, probabilidad, nivel_riesgo_pld, estatus_alerta, prioridad, creado_por) " &
        "VALUES " &
        "(@folio, @categoria_id, @motivo_id, @regla_id, @cliente_id, @solicitud_id, @origen_evento, @referencia_tabla, @referencia_id, " &
        "@titulo, @descripcion, @valor_detectado, @valor_umbral, @impacto, @probabilidad, @nivel_riesgo_pld, 1, @prioridad, @creado_por); " &
        "SELECT CAST(SCOPE_IDENTITY() AS INT);"

        Dim alertaId As Integer = 0

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@folio", folio)
            cmd.Parameters.AddWithValue("@categoria_id", categoriaId)
            cmd.Parameters.AddWithValue("@motivo_id", motivoId)

            If reglaId.HasValue Then
                cmd.Parameters.AddWithValue("@regla_id", reglaId.Value)
            Else
                cmd.Parameters.AddWithValue("@regla_id", DBNull.Value)
            End If

            If clienteId > 0 Then
                cmd.Parameters.AddWithValue("@cliente_id", clienteId)
            Else
                cmd.Parameters.AddWithValue("@cliente_id", DBNull.Value)
            End If

            If solicitudId > 0 Then
                cmd.Parameters.AddWithValue("@solicitud_id", solicitudId)
            Else
                cmd.Parameters.AddWithValue("@solicitud_id", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@origen_evento", origenEvento)

            If String.IsNullOrWhiteSpace(referenciaTabla) Then
                cmd.Parameters.AddWithValue("@referencia_tabla", DBNull.Value)
            Else
                cmd.Parameters.AddWithValue("@referencia_tabla", referenciaTabla)
            End If

            If referenciaId.HasValue Then
                cmd.Parameters.AddWithValue("@referencia_id", referenciaId.Value)
            Else
                cmd.Parameters.AddWithValue("@referencia_id", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@titulo", titulo)

            If String.IsNullOrWhiteSpace(descripcion) Then
                cmd.Parameters.AddWithValue("@descripcion", DBNull.Value)
            Else
                cmd.Parameters.AddWithValue("@descripcion", descripcion)
            End If

            If valorDetectado.HasValue Then
                cmd.Parameters.AddWithValue("@valor_detectado", valorDetectado.Value)
            Else
                cmd.Parameters.AddWithValue("@valor_detectado", DBNull.Value)
            End If

            If valorUmbral.HasValue Then
                cmd.Parameters.AddWithValue("@valor_umbral", valorUmbral.Value)
            Else
                cmd.Parameters.AddWithValue("@valor_umbral", DBNull.Value)
            End If

            cmd.Parameters.AddWithValue("@impacto", impacto)
            cmd.Parameters.AddWithValue("@probabilidad", probabilidad)
            cmd.Parameters.AddWithValue("@nivel_riesgo_pld", nivelRiesgoPld)
            cmd.Parameters.AddWithValue("@prioridad", prioridad)
            cmd.Parameters.AddWithValue("@creado_por", usuario)

            alertaId = Convert.ToInt32(cmd.ExecuteScalar())
        End Using

        Try
            NotificarAlertaPorCorreo(cn, tr, alertaId, usuario)
        Catch exCorreo As Exception
            Try
                InsertarBitacora(
                cn,
                tr,
                alertaId,
                "CORREO_ERROR",
                Nothing,
                1,
                "No se pudo procesar notificación por correo: " & exCorreo.Message,
                usuario
            )
            Catch
            End Try
        End Try

        Return alertaId
    End Function

    Private Sub NotificarAlertaPorCorreo(
    ByVal cn As SqlConnection,
    ByVal tr As SqlTransaction,
    ByVal alertaId As Integer,
    ByVal usuario As String
)

        Dim alerta As Dictionary(Of String, Object) = ObtenerDatosAlertaCorreo(cn, tr, alertaId)

        If alerta Is Nothing Then
            InsertarBitacora(
            cn,
            tr,
            alertaId,
            "CORREO_SIN_DATOS",
            Nothing,
            1,
            "No se encontraron datos de la alerta para notificación por correo.",
            usuario
        )
            Return
        End If

        Dim destinatarios As List(Of String) = ObtenerDestinatariosCorreoAlerta(cn, tr, alerta)

        If destinatarios.Count = 0 Then
            InsertarBitacora(
            cn,
            tr,
            alertaId,
            "CORREO_SIN_DESTINATARIOS",
            Nothing,
            1,
            "No existen usuarios activos con correo para los puestos configurados en esta alerta.",
            usuario
        )
            Return
        End If

        Dim asunto As String =
        "[PLD] " &
        Convert.ToString(alerta("categoria")) &
        " - " &
        Convert.ToString(alerta("folio"))

        Dim cuerpoHtml As String = ConstruirCuerpoCorreoAlerta(alerta)

        Dim resultado As PLD.PldEmailService.EmailResult =
        PLD.PldEmailService.EnviarCorreo(destinatarios, asunto, cuerpoHtml)

        Dim tipoMovimiento As String = "CORREO_ENVIADO"

        If Not resultado.Enabled Then
            tipoMovimiento = "CORREO_DESHABILITADO"
        ElseIf Not resultado.Ok Then
            tipoMovimiento = "CORREO_ERROR"
        End If

        Dim comentario As String =
        resultado.Mensaje &
        " " &
        If(String.IsNullOrWhiteSpace(resultado.Detalle), "", "Detalle: " & resultado.Detalle)

        InsertarBitacora(
        cn,
        tr,
        alertaId,
        tipoMovimiento,
        Nothing,
        1,
        comentario,
        usuario
    )
    End Sub

    Private Function ObtenerDatosAlertaCorreo(
    ByVal cn As SqlConnection,
    ByVal tr As SqlTransaction,
    ByVal alertaId As Integer
) As Dictionary(Of String, Object)

        Dim sql As String =
        "SELECT TOP 1 " &
        "a.id, a.folio, a.categoria_id, c.descripcion AS categoria, " &
        "a.motivo_id, m.motivo, m.clave AS motivo_clave, " &
        "a.regla_id, r.nombre_regla, r.clave AS regla_clave, " &
        "a.cliente_id, a.solicitud_id, a.origen_evento, a.referencia_tabla, a.referencia_id, " &
        "a.titulo, a.descripcion, a.valor_detectado, a.valor_umbral, " &
        "a.impacto, a.probabilidad, a.nivel_riesgo_pld, a.prioridad, " &
        "a.estatus_alerta, a.fecha_generacion " &
        "FROM dbo.alertas_pld a " &
        "INNER JOIN dbo.catalogo_alerta_categoria c ON c.id = a.categoria_id " &
        "INNER JOIN dbo.catalogo_alerta_motivo m ON m.id = a.motivo_id " &
        "LEFT JOIN dbo.catalogo_alerta_regla r ON r.id = a.regla_id " &
        "WHERE a.id = @alerta_id;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@alerta_id", alertaId)

            Using rd As SqlDataReader = cmd.ExecuteReader()
                If Not rd.Read() Then
                    Return Nothing
                End If

                Dim item As New Dictionary(Of String, Object)()

                For i As Integer = 0 To rd.FieldCount - 1
                    If rd.IsDBNull(i) Then
                        item(rd.GetName(i)) = Nothing
                    Else
                        item(rd.GetName(i)) = rd.GetValue(i)
                    End If
                Next

                Return item
            End Using
        End Using
    End Function

    Private Function ObtenerDestinatariosCorreoAlerta(
    ByVal cn As SqlConnection,
    ByVal tr As SqlTransaction,
    ByVal alerta As Dictionary(Of String, Object)
) As List(Of String)

        Dim destinatarios As New List(Of String)()

        Dim categoriaId As Integer = ToInt(alerta("categoria_id"), 0)
        Dim motivoId As Integer = ToInt(alerta("motivo_id"), 0)
        Dim reglaId As Integer = ToInt(alerta("regla_id"), 0)

        Dim sql As String =
        "SELECT DISTINCT LTRIM(RTRIM(su.email)) AS email " &
        "FROM dbo.config_alertas_pld_destinatarios_puesto d " &
        "INNER JOIN dbo.seguridad_usuarios su ON su.puesto_id = d.puesto_id " &
        "WHERE d.activo = 1 " &
        "AND d.enviar_correo = 1 " &
        "AND su.activo = 1 " &
        "AND ISNULL(LTRIM(RTRIM(su.email)), '') <> '' " &
        "AND ( " &
        "       (d.regla_id IS NOT NULL AND d.regla_id = @regla_id) " &
        "    OR (d.regla_id IS NULL AND d.motivo_id IS NOT NULL AND d.motivo_id = @motivo_id) " &
        "    OR (d.regla_id IS NULL AND d.motivo_id IS NULL AND d.categoria_id IS NOT NULL AND d.categoria_id = @categoria_id) " &
        ") " &
        "ORDER BY email;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@categoria_id", categoriaId)
            cmd.Parameters.AddWithValue("@motivo_id", motivoId)
            cmd.Parameters.AddWithValue("@regla_id", reglaId)

            Using rd As SqlDataReader = cmd.ExecuteReader()
                While rd.Read()
                    Dim email As String = Convert.ToString(rd("email")).Trim()

                    If Not String.IsNullOrWhiteSpace(email) AndAlso Not destinatarios.Contains(email) Then
                        destinatarios.Add(email)
                    End If
                End While
            End Using
        End Using

        Return destinatarios
    End Function

    Private Function ConstruirCuerpoCorreoAlerta(ByVal alerta As Dictionary(Of String, Object)) As String
        Dim sb As New StringBuilder()

        sb.Append("<html>")
        sb.Append("<body style='font-family: Arial, sans-serif; font-size: 14px; color: #222;'>")

        sb.Append("<h2 style='color:#b42318;'>Alerta P.L.D. generada</h2>")

        sb.Append("<p>Se generó una nueva alerta en el Sistema P.L.D.</p>")

        sb.Append("<table cellpadding='6' cellspacing='0' style='border-collapse:collapse; width:100%; max-width:760px;'>")

        AgregarFilaCorreo(sb, "Folio", alerta("folio"))
        AgregarFilaCorreo(sb, "Categoría", alerta("categoria"))
        AgregarFilaCorreo(sb, "Motivo", alerta("motivo"))
        AgregarFilaCorreo(sb, "Regla", alerta("nombre_regla"))
        AgregarFilaCorreo(sb, "Origen", alerta("origen_evento"))
        AgregarFilaCorreo(sb, "Solicitud ID", alerta("solicitud_id"))
        AgregarFilaCorreo(sb, "Cliente ID", alerta("cliente_id"))
        AgregarFilaCorreo(sb, "Valor detectado", alerta("valor_detectado"))
        AgregarFilaCorreo(sb, "Valor umbral", alerta("valor_umbral"))
        AgregarFilaCorreo(sb, "Nivel riesgo PLD", alerta("nivel_riesgo_pld"))
        AgregarFilaCorreo(sb, "Prioridad", alerta("prioridad"))
        AgregarFilaCorreo(sb, "Fecha generación", alerta("fecha_generacion"))

        sb.Append("</table>")

        sb.Append("<h4>Descripción</h4>")
        sb.Append("<p>")
        sb.Append(HttpUtility.HtmlEncode(Convert.ToString(alerta("descripcion"))))
        sb.Append("</p>")

        sb.Append("<hr />")
        sb.Append("<p style='font-size:12px;color:#666;'>")
        sb.Append("Este correo fue generado automáticamente por el Sistema PLD.")
        sb.Append("</p>")

        sb.Append("</body>")
        sb.Append("</html>")

        Return sb.ToString()
    End Function

    Private Sub AgregarFilaCorreo(ByVal sb As StringBuilder, ByVal etiqueta As String, ByVal valor As Object)
        Dim texto As String = ""

        If valor IsNot Nothing AndAlso valor IsNot DBNull.Value Then
            texto = Convert.ToString(valor)
        End If

        sb.Append("<tr>")
        sb.Append("<td style='border:1px solid #ddd; background:#f6f6f6; font-weight:bold; width:190px;'>")
        sb.Append(HttpUtility.HtmlEncode(etiqueta))
        sb.Append("</td>")
        sb.Append("<td style='border:1px solid #ddd;'>")
        sb.Append(HttpUtility.HtmlEncode(texto))
        sb.Append("</td>")
        sb.Append("</tr>")
    End Sub

    Private Function GenerarFolioAlerta(ByVal cn As SqlConnection, ByVal tr As SqlTransaction) As String
        Dim consecutivo As Integer = 1
        Dim prefijo As String = "ALR-" & DateTime.Now.ToString("yyyyMMdd") & "-"

        Dim sql As String =
            "SELECT ISNULL(MAX(CAST(RIGHT(folio, 5) AS INT)), 0) + 1 " &
            "FROM dbo.alertas_pld WITH (UPDLOCK, HOLDLOCK) " &
            "WHERE folio LIKE @prefijo + '%';"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@prefijo", prefijo)
            consecutivo = Convert.ToInt32(cmd.ExecuteScalar())
        End Using

        Return prefijo & consecutivo.ToString("00000")
    End Function

    Private Sub InsertarBitacora(
        ByVal cn As SqlConnection,
        ByVal tr As SqlTransaction,
        ByVal alertaId As Integer,
        ByVal tipoMovimiento As String,
        ByVal estatusAnterior As Integer?,
        ByVal estatusNuevo As Integer?,
        ByVal comentario As String,
        ByVal usuario As String
    )

        Dim sql As String =
            "INSERT INTO dbo.alertas_pld_bitacora " &
            "(alerta_id, tipo_movimiento, estatus_anterior, estatus_nuevo, comentario, usuario) " &
            "VALUES " &
            "(@alerta_id, @tipo_movimiento, @estatus_anterior, @estatus_nuevo, @comentario, @usuario);"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@alerta_id", alertaId)
            cmd.Parameters.AddWithValue("@tipo_movimiento", tipoMovimiento)

            If estatusAnterior.HasValue Then
                cmd.Parameters.AddWithValue("@estatus_anterior", estatusAnterior.Value)
            Else
                cmd.Parameters.AddWithValue("@estatus_anterior", DBNull.Value)
            End If

            If estatusNuevo.HasValue Then
                cmd.Parameters.AddWithValue("@estatus_nuevo", estatusNuevo.Value)
            Else
                cmd.Parameters.AddWithValue("@estatus_nuevo", DBNull.Value)
            End If

            If String.IsNullOrWhiteSpace(comentario) Then
                cmd.Parameters.AddWithValue("@comentario", DBNull.Value)
            Else
                cmd.Parameters.AddWithValue("@comentario", comentario)
            End If

            cmd.Parameters.AddWithValue("@usuario", usuario)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function ObtenerEstatusActual(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal alertaId As Integer) As Integer
        Dim sql As String = "SELECT estatus_alerta FROM dbo.alertas_pld WHERE id = @id;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.AddWithValue("@id", alertaId)

            Dim valor As Object = cmd.ExecuteScalar()

            If valor Is Nothing OrElse valor Is DBNull.Value Then
                Throw New Exception("No se encontró la alerta para cambiar estatus.")
            End If

            Return Convert.ToInt32(valor)
        End Using
    End Function

End Class