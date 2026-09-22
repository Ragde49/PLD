Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Web.Script.Serialization

Public Class catalogos_handler : Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        Try
            Dim action As String = (If(context.Request("action"), "")).Trim().ToLowerInvariant()

            Select Case action
                Case "medios_contacto" : HandleMediosContacto(context)
                Case "producto_financiero" : HandleProductoFinanciero(context)
                Case "canales_pago" : HandleCanalesPago(context)
                Case "destinos_recursos" : HandleDestinosRecursos(context)
                Case "monedas" : HandleMonedas(context)
                Case "nacionalidades" : HandleNacionalidades(context)
                Case "actividades_economicas" : HandleActividadesEconomicas(context)
                Case "paises" : HandlePaises(context)
                Case "estados" : HandleEstados(context)
                Case "municipios" : HandleMunicipios(context)
                Case "origen_recursos"
                    Using con = GetConn()
                        con.Open()

                        Dim sql As String = "
                                SELECT 
                                    id,
                                    origen AS descripcion,
                                    impacto,
                                    probabilidad,
                                    nivel_riesgo_pld,
                                    activo
                                FROM catalogo_origen_recursos
                                ORDER BY descripcion;
                            "
                        Using cmd As New SqlCommand(sql, con)
                            Using rd = cmd.ExecuteReader()
                                Dim rows As New List(Of Object)
                                While rd.Read()
                                    rows.Add(New With {
                        .id = rd("id"),
                        .descripcion = rd("descripcion").ToString(),
                        .impacto = Convert.ToInt32(rd("impacto")),
                        .probabilidad = Convert.ToInt32(rd("probabilidad")),
                        .nivel_riesgo_pld = Convert.ToDecimal(rd("nivel_riesgo_pld")),
                        .activo = Convert.ToBoolean(rd("activo"))
                    })
                                End While

                                WriteJson(context, New With {
                    .ok = True,
                    .data = rows
                })
                            End Using
                        End Using
                    End Using
                    Return

                Case "ping" : WriteJson(context, New With {.ok = True, .message = "pong"})
                Case Else
                    WriteError(context, "Acción no soportada. Usa: medios_contacto | producto_financiero | canales_pago | destinos_recursos | monedas | nacionalidades | actividades_economicas | paises | estados | municipios | ping")
            End Select

        Catch ex As Exception
            WriteError(context, "Excepción no controlada: " & ex.Message)
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

    Private Sub ReadPaging(ctx As HttpContext, ByRef page As Integer, ByRef pageSize As Integer, Optional defaultPage As Integer = 1, Optional defaultSize As Integer = 50)
        page = defaultPage : pageSize = defaultSize
        Integer.TryParse(If(ctx.Request("page"), defaultPage.ToString()), page)
        Integer.TryParse(If(ctx.Request("pageSize"), defaultSize.ToString()), pageSize)
        If page < 1 Then page = 1
        If pageSize < 1 Then pageSize = defaultSize
    End Sub

    Private Function GetBool(value As String, defaultVal As Boolean) As Boolean
        If String.IsNullOrWhiteSpace(value) Then Return defaultVal
        Dim v = value.Trim()
        Return (v = "1" OrElse v.Equals("true", StringComparison.OrdinalIgnoreCase))
    End Function

    '=================== Acciones ===================
    ' MEDIOS DE CONTACTO
    Private Sub HandleMediosContacto(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim whereSql As String = " WHERE 1=1 "
        If Not String.IsNullOrEmpty(q) Then whereSql &= " AND forma_contacto LIKE @q "
        Dim offset As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_medios_contacto " & whereSql, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            Dim sql As String = "
                SELECT id, forma_contacto, impacto, probabilidad, nivel_riesgo_pld
                FROM dbo.catalogo_medios_contacto " & whereSql & "
                ORDER BY id
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("forma_contacto")),
                            .impacto = If(IsDBNull(rd("impacto")), CType(Nothing, Integer?), Convert.ToInt32(rd("impacto"))),
                            .probabilidad = If(IsDBNull(rd("probabilidad")), CType(Nothing, Integer?), Convert.ToInt32(rd("probabilidad"))),
                            .nivel = If(IsDBNull(rd("nivel_riesgo_pld")), CType(Nothing, Decimal?), Convert.ToDecimal(rd("nivel_riesgo_pld"), CultureInfo.InvariantCulture))
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' PRODUCTO FINANCIERO — etiqueta = descripcion_larga
    Private Sub HandleProductoFinanciero(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim soloActivos As Boolean = GetBool(If(ctx.Request("solo_activos"), "1"), True)
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim offset As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()

            Dim whereParts As New List(Of String) From {"1=1"}
            If soloActivos Then whereParts.Add("pf.activo = 1")
            If Not String.IsNullOrEmpty(q) Then
                whereParts.Add("(" &
                               "CAST(pf.id AS NVARCHAR(50)) LIKE @q OR " &
                               "ISNULL(pf.descripcion_larga,'') LIKE @q" &
                               ")")
            End If
            Dim whereSql As String = " WHERE " & String.Join(" AND ", whereParts)

            ' Total
            Dim sqlCount As String =
                "SELECT COUNT(1) FROM dbo.catalogo_producto_financiero pf " & whereSql & ";"
            Dim total As Integer
            Using cCount As New SqlCommand(sqlCount, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 300).Value = "%" & q & "%"
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            ' Página (nivel usa columna computada nivel_riesgo_final)
            Dim sql As String =
                "SELECT pf.id, pf.descripcion_larga, pf.impacto, pf.probabilidad, pf.nivel_riesgo_final AS nivel, " &
                "       pf.tipo_credito_id, cc.nombre_credito AS tipo_credito, ISNULL(cc.es_revolvente,0) AS es_revolvente " &
                "FROM dbo.catalogo_producto_financiero pf " &
                "LEFT JOIN dbo.catalogo_creditos cc ON cc.id = pf.tipo_credito_id " & whereSql & "
                 ORDER BY pf.id
                 OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"

            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 300).Value = "%" & q & "%"
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        Dim id As Integer = Convert.ToInt32(rd("id"))
                        Dim desc As String = Convert.ToString(rd("descripcion_larga"))
                        If String.IsNullOrWhiteSpace(desc) Then desc = "PF #" & id.ToString()
                        rows.Add(New With {
                            .id = id,
                            .descripcion = desc,
                            .tipo_credito_id = If(rd.IsDBNull(rd.GetOrdinal("tipo_credito_id")), CType(Nothing, Object), rd("tipo_credito_id")),
                            .tipo_credito = If(rd.IsDBNull(rd.GetOrdinal("tipo_credito")), "", Convert.ToString(rd("tipo_credito"))),
                            .es_revolvente = Convert.ToBoolean(rd("es_revolvente")),
                            .impacto = Convert.ToInt32(rd("impacto")),
                            .probabilidad = Convert.ToInt32(rd("probabilidad")),
                            .nivel = Convert.ToDecimal(rd("nivel"), Globalization.CultureInfo.InvariantCulture)
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' CANALES DE PAGO
    Private Sub HandleCanalesPago(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim soloActivos As Boolean = GetBool(If(ctx.Request("solo_activos"), "1"), True)
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim whereSql As String = " WHERE 1=1 "
        If soloActivos Then whereSql &= " AND activo = 1 "
        If Not String.IsNullOrEmpty(q) Then whereSql &= " AND canal LIKE @q "
        Dim offset As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_canal_pago " & whereSql, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            Dim sql As String = "
                SELECT id, canal, impacto, probabilidad, nivel_riesgo_pld
                FROM dbo.catalogo_canal_pago " & whereSql & "
                ORDER BY id
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        Dim imp As Integer = If(IsDBNull(rd("impacto")), 0, Convert.ToInt32(rd("impacto")))
                        Dim pr As Integer = If(IsDBNull(rd("probabilidad")), 0, Convert.ToInt32(rd("probabilidad")))
                        Dim nivDir As Decimal = If(IsDBNull(rd("nivel_riesgo_pld")), 0D, Convert.ToDecimal(rd("nivel_riesgo_pld"), CultureInfo.InvariantCulture))
                        Dim nivel As Decimal = If(nivDir > 0D, nivDir, Math.Round((imp * pr) / 100D, 2))
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("canal")),
                            .impacto = imp,
                            .probabilidad = pr,
                            .nivel = nivel
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' DESTINOS DE RECURSOS
    Private Sub HandleDestinosRecursos(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim soloActivos As Boolean = GetBool(If(ctx.Request("solo_activos"), "1"), True)
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim whereSql As String = " WHERE 1=1 "
        If soloActivos Then whereSql &= " AND activo = 1 "
        If Not String.IsNullOrEmpty(q) Then whereSql &= " AND descripcion LIKE @q "
        Dim offset As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_destino_recursos " & whereSql, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            Dim sql As String = "
                SELECT id, descripcion, impacto, ocurrencia, nivel_riesgo_pld
                FROM dbo.catalogo_destino_recursos " & whereSql & "
                ORDER BY id
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 210).Value = "%" & q & "%"
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        Dim imp As Integer = If(IsDBNull(rd("impacto")), 0, Convert.ToInt32(rd("impacto")))
                        Dim pr As Integer = If(IsDBNull(rd("ocurrencia")), 0, Convert.ToInt32(rd("ocurrencia")))
                        Dim nivDir As Decimal = If(IsDBNull(rd("nivel_riesgo_pld")), 0D, Convert.ToDecimal(rd("nivel_riesgo_pld"), CultureInfo.InvariantCulture))
                        Dim nivel As Decimal = If(nivDir > 0D, nivDir, Math.Round((imp * pr) / 100D, 2))
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("descripcion")),
                            .impacto = imp,
                            .probabilidad = pr,
                            .nivel = nivel
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' MONEDAS (según estructura real: id, clave, moneda, activo, impacto, probabilidad, nivel_riesgo_pld)
    Private Sub HandleMonedas(ctx As HttpContext)
        Dim q As String = (If(ctx.Request("q"), "")).Trim()
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim offset As Integer = (page - 1) * pageSize

        Using con = GetConn()
            con.Open()

            ' Filtro: solo activas por defecto
            Dim whereParts As New List(Of String) From {"1=1", "ISNULL(activo,1)=1"}
            If Not String.IsNullOrEmpty(q) Then
                whereParts.Add("(CAST(id AS NVARCHAR(50)) LIKE @q OR ISNULL(clave,'') LIKE @q OR ISNULL(moneda,'') LIKE @q)")
            End If
            Dim whereSql As String = " WHERE " & String.Join(" AND ", whereParts)

            ' Total
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_moneda_divisa " & whereSql, con)
                If Not String.IsNullOrEmpty(q) Then cCount.Parameters.Add("@q", SqlDbType.NVarChar, 80).Value = "%" & q & "%"
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using

            ' Página
            Dim sql As String = "
            SELECT id,
                   LTRIM(RTRIM(COALESCE(NULLIF(clave,''),'') + CASE WHEN ISNULL(moneda,'')<>'' AND NULLIF(clave,'') IS NOT NULL THEN ' - ' ELSE '' END + COALESCE(moneda,''))) AS etiqueta,
                   ISNULL(impacto,0) AS impacto,
                   ISNULL(probabilidad,0) AS probabilidad,
                   ISNULL(nivel_riesgo_pld,0) AS nivel_dir
            FROM dbo.catalogo_moneda_divisa " & whereSql & "
            ORDER BY id
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                If Not String.IsNullOrEmpty(q) Then cmd.Parameters.Add("@q", SqlDbType.NVarChar, 80).Value = "%" & q & "%"
                cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        Dim imp As Integer = Convert.ToInt32(rd("impacto"))
                        Dim pr As Integer = Convert.ToInt32(rd("probabilidad"))
                        Dim nivDir As Decimal = Convert.ToDecimal(rd("nivel_dir"), Globalization.CultureInfo.InvariantCulture)
                        Dim nivel As Decimal = If(nivDir > 0D, nivDir, Math.Round((imp * pr) / 100D, 2))
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = If(String.IsNullOrWhiteSpace(Convert.ToString(rd("etiqueta"))),
                                              "Moneda #" & Convert.ToInt32(rd("id")).ToString(),
                                              Convert.ToString(rd("etiqueta"))),
                            .impacto = imp,
                            .probabilidad = pr,
                            .nivel = nivel
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' NACIONALIDADES (usa dbo.catalogo_nacionalidades)
    Private Sub HandleNacionalidades(ctx As HttpContext)
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim offset As Integer = (page - 1) * pageSize
        Using con = GetConn()
            con.Open()
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_nacionalidades WHERE activo=1", con)
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using
            Dim sql As String = "
            SELECT id, nacionalidad AS descripcion
            FROM dbo.catalogo_nacionalidades
            WHERE activo=1
            ORDER BY nacionalidad
            OFFSET @o ROWS FETCH NEXT @s ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@o", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@s", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("descripcion"))
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' ACTIVIDADES ECONÓMICAS (usa dbo.catalogo_actividad_economica)
    Private Sub HandleActividadesEconomicas(ctx As HttpContext)
        Dim page, pageSize As Integer : ReadPaging(ctx, page, pageSize)
        Dim offset As Integer = (page - 1) * pageSize
        Using con = GetConn()
            con.Open()
            Dim total As Integer
            Using cCount As New SqlCommand("SELECT COUNT(1) FROM dbo.catalogo_actividad_economica WHERE ISNULL(activo,1)=1", con)
                total = Convert.ToInt32(cCount.ExecuteScalar())
            End Using
            Dim sql As String = "
            SELECT id, nombre AS descripcion
            FROM dbo.catalogo_actividad_economica
            WHERE ISNULL(activo,1)=1
            ORDER BY nombre
            OFFSET @o ROWS FETCH NEXT @s ROWS ONLY;"
            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@o", SqlDbType.Int).Value = offset
                cmd.Parameters.Add("@s", SqlDbType.Int).Value = pageSize
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("descripcion"))
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows, .total = total, .page = page, .pageSize = pageSize})
                End Using
            End Using
        End Using
    End Sub

    ' PAISES (catálogo común)  *** CORREGIDO: usa columna [pais] ***
    Private Sub HandlePaises(ctx As HttpContext)
        Using con = GetConn()
            con.Open()
            Dim sql As String = "SELECT id, pais AS descripcion FROM dbo.catalogo_paises WHERE ISNULL(activo,1)=1 ORDER BY pais;"
            Using cmd As New SqlCommand(sql, con)
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("descripcion"))
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows})
                End Using
            End Using
        End Using
    End Sub

    ' ESTADOS (para combos de entidad de nacimiento / domicilio, etc.)
    Private Sub HandleEstados(ctx As HttpContext)
        ' Nota:
        ' El parámetro pais_id se sigue aceptando para no romper el contrato,
        ' pero actualmente NO se usa porque la tabla catalogo_estados
        ' no tiene columna de relación a países.
        Dim paisIdRaw As String = If(ctx.Request("pais_id"), Nothing)
        ' Se podría loggear o validar en el futuro si se agrega esa relación.

        Using con = GetConn()
            con.Open()

            ' La tabla real es:
            ' catalogo_estados(id, descripcion, clave, clave_estado,
            '                  impacto, probabilidad, nivel_riesgo_pld,
            '                  estatus, fecha_creacion)
            '
            ' Usamos estatus como bandera de activo (1 = activo / NULL = activo por default).
            Dim sql As String = "
            SELECT id,
                   descripcion
            FROM dbo.catalogo_estados
            WHERE ISNULL(estatus, 1) = 1
            ORDER BY descripcion;"

            Using cmd As New SqlCommand(sql, con)
                Using rd As SqlDataReader = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                        .id = Convert.ToInt32(rd("id")),
                        .descripcion = Convert.ToString(rd("descripcion"))
                    })
                    End While

                    WriteJson(ctx, New With {
                    .ok = True,
                    .data = rows
                })
                End Using
            End Using
        End Using
    End Sub


    ' MUNICIPIOS por estado (usa catalogo_estados.clave_estado -> catalogo_municipios.clave_estado)
    Private Sub HandleMunicipios(ctx As HttpContext)
        Dim estadoId As Integer
        If Not Integer.TryParse(ctx.Request("estado_id"), estadoId) OrElse estadoId <= 0 Then
            WriteError(ctx, "estado_id requerido")
            Return
        End If

        Using con = GetConn()
            con.Open()

            ' 1) Obtener la clave_estado del catálogo de estados a partir del ID
            Dim claveEstado As String = Nothing
            Using cmdE As New SqlCommand("SELECT clave_estado FROM dbo.catalogo_estados WHERE id=@id AND estatus=1;", con)
                cmdE.Parameters.Add("@id", SqlDbType.Int).Value = estadoId
                Dim obj = cmdE.ExecuteScalar()
                If obj Is Nothing OrElse obj Is DBNull.Value Then
                    ' No se encontró el estado, regresamos lista vacía sin error
                    WriteJson(ctx, New With {.ok = True, .data = New List(Of Object)()})
                    Return
                End If
                claveEstado = Convert.ToString(obj)
            End Using

            ' 2) Listar municipios que correspondan a esa clave_estado
            Dim sql As String = "
            SELECT id, descripcion
            FROM dbo.catalogo_municipios
            WHERE estatus = 1
              AND clave_estado = @clave_estado
            ORDER BY descripcion;"

            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.Add("@clave_estado", SqlDbType.VarChar, 10).Value = claveEstado
                Using rd = cmd.ExecuteReader()
                    Dim rows As New List(Of Object)
                    While rd.Read()
                        rows.Add(New With {
                            .id = Convert.ToInt32(rd("id")),
                            .descripcion = Convert.ToString(rd("descripcion"))
                        })
                    End While
                    WriteJson(ctx, New With {.ok = True, .data = rows})
                End Using
            End Using
        End Using
    End Sub



End Class
