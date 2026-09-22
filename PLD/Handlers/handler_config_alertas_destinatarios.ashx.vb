Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Text

Public Class handler_config_alertas_destinatarios
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = ObtenerParametro(context, "action").Trim().ToLowerInvariant()

            If String.IsNullOrWhiteSpace(action) Then
                action = ObtenerParametro(context, "accion").Trim().ToLowerInvariant()
            End If

            Select Case action
                Case "catalogos"
                    respuesta = Catalogos()

                Case "consultar"
                    respuesta = Consultar(context)

                Case "guardar"
                    respuesta = Guardar(context)

                Case "eliminar"
                    respuesta = Eliminar(context)

                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select

        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_config_alertas_destinatarios."
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
        If context.Request(nombre) IsNot Nothing Then
            Return Convert.ToString(context.Request(nombre))
        End If

        Return ""
    End Function

    Private Function ObtenerUsuario(ByVal context As HttpContext) As String
        Dim usuario As String = ""

        If context.Session IsNot Nothing AndAlso context.Session("usuario") IsNot Nothing Then
            usuario = Convert.ToString(context.Session("usuario"))
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            If context.User IsNot Nothing AndAlso context.User.Identity IsNot Nothing AndAlso context.User.Identity.IsAuthenticated Then
                usuario = context.User.Identity.Name
            End If
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = "SISTEMA"
        End If

        Return usuario.Trim()
    End Function

    Private Function ToInt(ByVal valor As Object, Optional ByVal defaultValue As Integer = 0) As Integer
        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return defaultValue
        End If

        Dim resultado As Integer = defaultValue
        Integer.TryParse(Convert.ToString(valor), resultado)

        Return resultado
    End Function

    Private Function ToBool(ByVal valor As Object, Optional ByVal defaultValue As Boolean = True) As Boolean
        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return defaultValue
        End If

        Dim texto As String = Convert.ToString(valor).Trim().ToLowerInvariant()

        If texto = "1" OrElse texto = "true" OrElse texto = "si" OrElse texto = "sí" OrElse texto = "on" Then
            Return True
        End If

        If texto = "0" OrElse texto = "false" OrElse texto = "no" OrElse texto = "off" Then
            Return False
        End If

        Return defaultValue
    End Function

    Private Function DbIntNullable(ByVal valor As Integer) As Object
        If valor > 0 Then
            Return valor
        End If

        Return DBNull.Value
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

    Private Function Catalogos() As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim data As New Dictionary(Of String, Object)()

        Dim sqlCategorias As String =
            "SELECT id, clave, descripcion, orden, activo " &
            "FROM dbo.catalogo_alerta_categoria " &
            "WHERE activo = 1 " &
            "ORDER BY orden, descripcion;"

        Dim sqlMotivos As String =
            "SELECT m.id, m.categoria_id, c.descripcion AS categoria, m.clave, m.motivo, m.activo " &
            "FROM dbo.catalogo_alerta_motivo m " &
            "INNER JOIN dbo.catalogo_alerta_categoria c ON c.id = m.categoria_id " &
            "WHERE m.activo = 1 AND c.activo = 1 " &
            "ORDER BY c.orden, m.id;"

        Dim sqlReglas As String =
            "SELECT r.id, r.motivo_id, m.categoria_id, c.descripcion AS categoria, m.motivo, " &
            "       r.clave, r.nombre_regla, r.tipo_disparador, r.activo " &
            "FROM dbo.catalogo_alerta_regla r " &
            "INNER JOIN dbo.catalogo_alerta_motivo m ON m.id = r.motivo_id " &
            "INNER JOIN dbo.catalogo_alerta_categoria c ON c.id = m.categoria_id " &
            "WHERE r.activo = 1 AND m.activo = 1 AND c.activo = 1 " &
            "ORDER BY c.orden, m.id, r.id;"

        Dim sqlPuestos As String =
            "SELECT id, puesto, descripcion, activo " &
            "FROM dbo.catalogo_puestos " &
            "WHERE activo = 1 " &
            "ORDER BY puesto;"

        data("categorias") = TablaALista(EjecutarTabla(sqlCategorias, Nothing))
        data("motivos") = TablaALista(EjecutarTabla(sqlMotivos, Nothing))
        data("reglas") = TablaALista(EjecutarTabla(sqlReglas, Nothing))
        data("puestos") = TablaALista(EjecutarTabla(sqlPuestos, Nothing))

        respuesta("ok") = True
        respuesta("data") = data

        Return respuesta
    End Function

    Private Function Consultar(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()

        Dim categoriaId As Integer = ToInt(ObtenerParametro(context, "categoria_id"), 0)
        Dim motivoId As Integer = ToInt(ObtenerParametro(context, "motivo_id"), 0)
        Dim reglaId As Integer = ToInt(ObtenerParametro(context, "regla_id"), 0)
        Dim puestoId As Integer = ToInt(ObtenerParametro(context, "puesto_id"), 0)

        Dim parametros As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE d.activo = 1 "

        If categoriaId > 0 Then
            whereSql &= "AND ISNULL(d.categoria_id, 0) = @categoria_id "
            parametros.Add(New SqlParameter("@categoria_id", categoriaId))
        End If

        If motivoId > 0 Then
            whereSql &= "AND ISNULL(d.motivo_id, 0) = @motivo_id "
            parametros.Add(New SqlParameter("@motivo_id", motivoId))
        End If

        If reglaId > 0 Then
            whereSql &= "AND ISNULL(d.regla_id, 0) = @regla_id "
            parametros.Add(New SqlParameter("@regla_id", reglaId))
        End If

        If puestoId > 0 Then
            whereSql &= "AND d.puesto_id = @puesto_id "
            parametros.Add(New SqlParameter("@puesto_id", puestoId))
        End If

        Dim sql As String =
            "SELECT " &
            "    d.id, " &
            "    d.categoria_id, " &
            "    c.descripcion AS categoria, " &
            "    d.motivo_id, " &
            "    m.motivo, " &
            "    d.regla_id, " &
            "    r.nombre_regla, " &
            "    r.clave AS regla_clave, " &
            "    d.puesto_id, " &
            "    p.puesto, " &
            "    d.enviar_correo, " &
            "    d.activo, " &
            "    d.creado_por, " &
            "    d.fecha_creacion, " &
            "    d.modificado_por, " &
            "    d.fecha_modificacion, " &
            "    ISNULL(u.total_usuarios, 0) AS total_usuarios, " &
            "    ISNULL(u.total_con_email, 0) AS total_con_email " &
            "FROM dbo.config_alertas_pld_destinatarios_puesto d " &
            "LEFT JOIN dbo.catalogo_alerta_categoria c ON c.id = d.categoria_id " &
            "LEFT JOIN dbo.catalogo_alerta_motivo m ON m.id = d.motivo_id " &
            "LEFT JOIN dbo.catalogo_alerta_regla r ON r.id = d.regla_id " &
            "INNER JOIN dbo.catalogo_puestos p ON p.id = d.puesto_id " &
            "OUTER APPLY ( " &
            "    SELECT " &
            "        COUNT(1) AS total_usuarios, " &
            "        SUM(CASE WHEN ISNULL(LTRIM(RTRIM(su.email)), '') <> '' THEN 1 ELSE 0 END) AS total_con_email " &
            "    FROM dbo.seguridad_usuarios su " &
            "    WHERE su.puesto_id = d.puesto_id " &
            "      AND su.activo = 1 " &
            ") u " &
            whereSql &
            "ORDER BY c.orden, m.id, r.id, p.puesto;"

        Dim dt As DataTable = EjecutarTabla(sql, parametros)

        respuesta("ok") = True
        respuesta("data") = TablaALista(dt)

        Return respuesta
    End Function

    Private Function Guardar(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim usuario As String = ObtenerUsuario(context)

        Dim id As Integer = ToInt(ObtenerParametro(context, "id"), 0)
        Dim categoriaId As Integer = ToInt(ObtenerParametro(context, "categoria_id"), 0)
        Dim motivoId As Integer = ToInt(ObtenerParametro(context, "motivo_id"), 0)
        Dim reglaId As Integer = ToInt(ObtenerParametro(context, "regla_id"), 0)
        Dim puestoId As Integer = ToInt(ObtenerParametro(context, "puesto_id"), 0)
        Dim enviarCorreo As Boolean = ToBool(ObtenerParametro(context, "enviar_correo"), True)

        If puestoId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "Debes seleccionar un puesto."
            Return respuesta
        End If

        If categoriaId <= 0 AndAlso motivoId <= 0 AndAlso reglaId <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "Debes seleccionar al menos una categoría, motivo o regla."
            Return respuesta
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    If Not ExistePuesto(cn, tr, puestoId) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El puesto seleccionado no existe o está inactivo."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If categoriaId > 0 AndAlso Not ExisteCategoria(cn, tr, categoriaId) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "La categoría seleccionada no existe o está inactiva."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If motivoId > 0 AndAlso Not ExisteMotivo(cn, tr, motivoId) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "El motivo seleccionado no existe o está inactivo."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If reglaId > 0 AndAlso Not ExisteRegla(cn, tr, reglaId) Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "La regla seleccionada no existe o está inactiva."
                        tr.Rollback()
                        Return respuesta
                    End If

                    Dim idDuplicado As Integer = BuscarConfiguracionExistente(cn, tr, categoriaId, motivoId, reglaId, puestoId, id)

                    If idDuplicado > 0 Then
                        respuesta("ok") = False
                        respuesta("mensaje") = "Ya existe una configuración activa para esa combinación."
                        tr.Rollback()
                        Return respuesta
                    End If

                    If id > 0 Then
                        Dim sqlUpdate As String =
                            "UPDATE dbo.config_alertas_pld_destinatarios_puesto SET " &
                            "categoria_id = @categoria_id, " &
                            "motivo_id = @motivo_id, " &
                            "regla_id = @regla_id, " &
                            "puesto_id = @puesto_id, " &
                            "enviar_correo = @enviar_correo, " &
                            "activo = 1, " &
                            "modificado_por = @modificado_por, " &
                            "fecha_modificacion = SYSDATETIME() " &
                            "WHERE id = @id;"

                        Using cmd As New SqlCommand(sqlUpdate, cn, tr)
                            cmd.Parameters.Add("@categoria_id", SqlDbType.Int).Value = DbIntNullable(categoriaId)
                            cmd.Parameters.Add("@motivo_id", SqlDbType.Int).Value = DbIntNullable(motivoId)
                            cmd.Parameters.Add("@regla_id", SqlDbType.Int).Value = DbIntNullable(reglaId)
                            cmd.Parameters.Add("@puesto_id", SqlDbType.Int).Value = puestoId
                            cmd.Parameters.Add("@enviar_correo", SqlDbType.Bit).Value = enviarCorreo
                            cmd.Parameters.Add("@modificado_por", SqlDbType.NVarChar, 100).Value = usuario
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id

                            Dim n As Integer = cmd.ExecuteNonQuery()

                            If n = 0 Then
                                respuesta("ok") = False
                                respuesta("mensaje") = "No se encontró la configuración para actualizar."
                                tr.Rollback()
                                Return respuesta
                            End If
                        End Using

                        tr.Commit()

                        respuesta("ok") = True
                        respuesta("mensaje") = "Configuración actualizada correctamente."
                        respuesta("id") = id
                        Return respuesta
                    End If

                    Dim sqlInsert As String =
                        "INSERT INTO dbo.config_alertas_pld_destinatarios_puesto " &
                        "(categoria_id, motivo_id, regla_id, puesto_id, enviar_correo, activo, creado_por, fecha_creacion) " &
                        "VALUES " &
                        "(@categoria_id, @motivo_id, @regla_id, @puesto_id, @enviar_correo, 1, @creado_por, SYSDATETIME()); " &
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);"

                    Dim nuevoId As Integer = 0

                    Using cmd As New SqlCommand(sqlInsert, cn, tr)
                        cmd.Parameters.Add("@categoria_id", SqlDbType.Int).Value = DbIntNullable(categoriaId)
                        cmd.Parameters.Add("@motivo_id", SqlDbType.Int).Value = DbIntNullable(motivoId)
                        cmd.Parameters.Add("@regla_id", SqlDbType.Int).Value = DbIntNullable(reglaId)
                        cmd.Parameters.Add("@puesto_id", SqlDbType.Int).Value = puestoId
                        cmd.Parameters.Add("@enviar_correo", SqlDbType.Bit).Value = enviarCorreo
                        cmd.Parameters.Add("@creado_por", SqlDbType.NVarChar, 100).Value = usuario

                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using

                    tr.Commit()

                    respuesta("ok") = True
                    respuesta("mensaje") = "Configuración guardada correctamente."
                    respuesta("id") = nuevoId

                Catch ex As Exception
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return respuesta
    End Function

    Private Function Eliminar(ByVal context As HttpContext) As Dictionary(Of String, Object)
        Dim respuesta As New Dictionary(Of String, Object)()
        Dim usuario As String = ObtenerUsuario(context)
        Dim id As Integer = ToInt(ObtenerParametro(context, "id"), 0)

        If id <= 0 Then
            respuesta("ok") = False
            respuesta("mensaje") = "ID inválido."
            Return respuesta
        End If

        Dim sql As String =
            "UPDATE dbo.config_alertas_pld_destinatarios_puesto SET " &
            "activo = 0, " &
            "modificado_por = @modificado_por, " &
            "fecha_modificacion = SYSDATETIME() " &
            "WHERE id = @id;"

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cmd.Parameters.Add("@modificado_por", SqlDbType.NVarChar, 100).Value = usuario

                Dim n As Integer = cmd.ExecuteNonQuery()

                If n = 0 Then
                    respuesta("ok") = False
                    respuesta("mensaje") = "No se encontró la configuración."
                    Return respuesta
                End If
            End Using
        End Using

        respuesta("ok") = True
        respuesta("mensaje") = "Configuración eliminada correctamente."

        Return respuesta
    End Function

    Private Function ExistePuesto(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal puestoId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.catalogo_puestos " &
            "WHERE id = @id AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = puestoId
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function ExisteCategoria(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal categoriaId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.catalogo_alerta_categoria " &
            "WHERE id = @id AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = categoriaId
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function ExisteMotivo(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal motivoId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.catalogo_alerta_motivo " &
            "WHERE id = @id AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = motivoId
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function ExisteRegla(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal reglaId As Integer) As Boolean
        Dim sql As String =
            "SELECT COUNT(1) " &
            "FROM dbo.catalogo_alerta_regla " &
            "WHERE id = @id AND activo = 1;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = reglaId
            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function BuscarConfiguracionExistente(
        ByVal cn As SqlConnection,
        ByVal tr As SqlTransaction,
        ByVal categoriaId As Integer,
        ByVal motivoId As Integer,
        ByVal reglaId As Integer,
        ByVal puestoId As Integer,
        ByVal idActual As Integer
    ) As Integer

        Dim sql As String =
            "SELECT TOP 1 id " &
            "FROM dbo.config_alertas_pld_destinatarios_puesto " &
            "WHERE activo = 1 " &
            "AND puesto_id = @puesto_id " &
            "AND ISNULL(categoria_id, 0) = @categoria_id " &
            "AND ISNULL(motivo_id, 0) = @motivo_id " &
            "AND ISNULL(regla_id, 0) = @regla_id " &
            "AND id <> @id_actual;"

        Using cmd As New SqlCommand(sql, cn, tr)
            cmd.Parameters.Add("@puesto_id", SqlDbType.Int).Value = puestoId
            cmd.Parameters.Add("@categoria_id", SqlDbType.Int).Value = categoriaId
            cmd.Parameters.Add("@motivo_id", SqlDbType.Int).Value = motivoId
            cmd.Parameters.Add("@regla_id", SqlDbType.Int).Value = reglaId
            cmd.Parameters.Add("@id_actual", SqlDbType.Int).Value = idActual

            Dim obj As Object = cmd.ExecuteScalar()

            If obj Is Nothing OrElse obj Is DBNull.Value Then
                Return 0
            End If

            Return Convert.ToInt32(obj)
        End Using
    End Function

End Class

