Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Web.SessionState

Public Class seguridad_handler
    Implements IHttpHandler
    Implements IRequiresSessionState

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        serializer.MaxJsonLength = Integer.MaxValue

        Dim data = LeerJson(context)
        Dim action = ObtenerTexto(context, data, "action")
        If action = "" Then action = ObtenerTexto(context, data, "accion")
        action = action.Trim().ToLowerInvariant()

        Try
            Select Case action
                Case "login"
                    Login(context, data)
                    Return

                Case "cambiar_password"
                    CambiarPassword(context, data)
                    Return
            End Select

            If Not SeguridadAutorizacion.ExigirPermisoSeguridadAjax(context) Then Return

            Select Case action
                Case "usuarios_listar"
                    UsuariosListar(context)
                Case "usuario_obtener"
                    UsuarioObtener(context, data)
                Case "usuario_guardar"
                    UsuarioGuardar(context, data)
                Case "usuario_toggle"
                    UsuarioToggle(context, data)
                Case "roles_listar"
                    RolesListar(context)
                Case "rol_guardar"
                    RolGuardar(context, data)
                Case "rol_toggle"
                    RolToggle(context, data)
                Case "puestos_listar"
                    PuestosListar(context)
                Case "puesto_guardar"
                    PuestoGuardar(context, data)
                Case "puesto_toggle"
                    PuestoToggle(context, data)
                Case "paginas_listar"
                    PaginasListar(context)
                Case "menu_listar"
                    WriteOk(context, SeguridadMenu.ListarPlano())
                Case "permisos_por_rol"
                    PermisosPorRol(context, data)
                Case "permisos_guardar"
                    PermisosGuardar(context, data)
                Case Else
                    WriteError(context, "Accion no reconocida.", 400)
            End Select
        Catch ex As Exception
            WriteError(context, "Error en seguridad_handler: " & ex.Message, 500)
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Sub Login(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim usuario = ObtenerTexto(ctx, data, "usuario")
        Dim password = ObtenerTexto(ctx, data, "password")
        Dim recordar = ObtenerBooleano(ctx, data, "recordar", False)
        Dim returnUrl = ObtenerTexto(ctx, data, "returnUrl")

        Dim sesion As SeguridadSesion = Nothing
        Dim mensaje As String = ""
        If Not SeguridadAuth.ValidarCredenciales(usuario, password, sesion, mensaje) Then
            WriteJson(ctx, New With {.ok = False, .mensaje = mensaje})
            Return
        End If

        SeguridadAuth.IniciarSesion(ctx, sesion, recordar)

        Dim redirectUrl As String = If(sesion.DebeCambiarPassword, "/CambiarPassword.aspx", SanitizarReturnUrl(returnUrl))
        WriteJson(ctx, New With {.ok = True, .mensaje = mensaje, .debe_cambiar_password = sesion.DebeCambiarPassword, .redirect = redirectUrl})
    End Sub

    Private Sub CambiarPassword(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim actual = ObtenerTexto(ctx, data, "actual")
        Dim nueva = ObtenerTexto(ctx, data, "nueva")
        Dim confirmar = ObtenerTexto(ctx, data, "confirmar")

        If nueva <> confirmar Then
            WriteJson(ctx, New With {.ok = False, .mensaje = "La confirmacion no coincide."})
            Return
        End If

        Dim mensaje As String = ""
        If SeguridadAuth.CambiarPassword(ctx, actual, nueva, mensaje) Then
            WriteJson(ctx, New With {.ok = True, .mensaje = mensaje, .redirect = "/Default.aspx"})
        Else
            WriteJson(ctx, New With {.ok = False, .mensaje = mensaje})
        End If
    End Sub

    Private Sub UsuariosListar(ByVal ctx As HttpContext)
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    u.id,
                    u.usuario,
                    u.nombre,
                    u.email,
                    u.rol_id,
                    r.rol,
                    u.puesto_id,
                    p.puesto,
                    ISNULL(u.activo, 0) AS activo,
                    ISNULL(u.debe_cambiar_password, 0) AS debe_cambiar_password,
                    u.ultimo_acceso,
                    u.fecha_creacion
                FROM dbo.seguridad_usuarios u
                INNER JOIN dbo.catalogo_roles_permisos r ON r.id = u.rol_id
                LEFT JOIN dbo.catalogo_puestos p ON p.id = u.puesto_id
                ORDER BY u.usuario;", cn)
                WriteOk(ctx, LeerFilas(cmd))
            End Using
        End Using
    End Sub

    Private Sub UsuarioObtener(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT id, usuario, nombre, email, rol_id, puesto_id,
                       ISNULL(activo, 0) AS activo,
                       ISNULL(debe_cambiar_password, 0) AS debe_cambiar_password
                FROM dbo.seguridad_usuarios
                WHERE id = @id;", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                Dim rows = LeerFilas(cmd)
                If rows.Count = 0 Then
                    WriteError(ctx, "Usuario no encontrado.", 404)
                Else
                    WriteOk(ctx, rows(0))
                End If
            End Using
        End Using
    End Sub

    Private Sub UsuarioGuardar(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim usuario = ObtenerTexto(ctx, data, "usuario").Trim()
        Dim nombre = ObtenerTexto(ctx, data, "nombre").Trim()
        Dim email = ObtenerTexto(ctx, data, "email").Trim()
        Dim password = ObtenerTexto(ctx, data, "password")
        Dim rolId = ObtenerEntero(ctx, data, "rol_id", 0)
        Dim puestoId = ObtenerEnteroNullable(ctx, data, "puesto_id")
        Dim activo = ObtenerBooleano(ctx, data, "activo", True)
        Dim debeCambiar = ObtenerBooleano(ctx, data, "debe_cambiar_password", True)

        If usuario = "" OrElse nombre = "" OrElse rolId <= 0 Then
            WriteError(ctx, "Usuario, nombre y rol son obligatorios.", 400)
            Return
        End If

        If id = 0 AndAlso String.IsNullOrEmpty(password) Then
            WriteError(ctx, "La contrasena inicial es obligatoria.", 400)
            Return
        End If

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()

            If ExisteOtro(cn, "dbo.seguridad_usuarios", "usuario", usuario, id) Then
                WriteError(ctx, "Ya existe un usuario con ese nombre de acceso.", 400)
                Return
            End If

            If id = 0 Then
                Using cmd As New SqlCommand("
                    INSERT INTO dbo.seguridad_usuarios
                        (usuario, nombre, email, password_hash, rol_id, puesto_id, activo, debe_cambiar_password, fecha_password, fecha_creacion)
                    VALUES
                        (@usuario, @nombre, @email, @hash, @rol_id, @puesto_id, @activo, @debe, GETDATE(), GETDATE());", cn)
                    AgregarUsuarioParams(cmd, usuario, nombre, email, rolId, puestoId, activo, debeCambiar)
                    cmd.Parameters.Add("@hash", SqlDbType.VarChar, 300).Value = SeguridadPassword.CrearHash(password)
                    cmd.ExecuteNonQuery()
                End Using
            Else
                Dim sql As String = "
                    UPDATE dbo.seguridad_usuarios
                    SET usuario = @usuario,
                        nombre = @nombre,
                        email = @email,
                        rol_id = @rol_id,
                        puesto_id = @puesto_id,
                        activo = @activo,
                        debe_cambiar_password = @debe,
                        fecha_modificacion = GETDATE()"

                If Not String.IsNullOrEmpty(password) Then
                    sql &= ", password_hash = @hash, fecha_password = GETDATE()"
                End If

                sql &= " WHERE id = @id;"

                Using cmd As New SqlCommand(sql, cn)
                    AgregarUsuarioParams(cmd, usuario, nombre, email, rolId, puestoId, activo, debeCambiar)
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                    If Not String.IsNullOrEmpty(password) Then
                        cmd.Parameters.Add("@hash", SqlDbType.VarChar, 300).Value = SeguridadPassword.CrearHash(password)
                    End If
                    cmd.ExecuteNonQuery()
                End Using
            End If
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Usuario guardado correctamente."})
    End Sub

    Private Sub UsuarioToggle(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim activo = ObtenerBooleano(ctx, data, "activo", False)

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("UPDATE dbo.seguridad_usuarios SET activo = @activo, fecha_modificacion = GETDATE() WHERE id = @id;", cn)
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cmd.ExecuteNonQuery()
            End Using
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Estatus actualizado."})
    End Sub

    Private Sub RolesListar(ByVal ctx As HttpContext)
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT id, rol, descripcion, ISNULL(activo, 0) AS activo, fecha_creacion
                FROM dbo.catalogo_roles_permisos
                ORDER BY rol;", cn)
                WriteOk(ctx, LeerFilas(cmd))
            End Using
        End Using
    End Sub

    Private Sub RolGuardar(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim rol = ObtenerTexto(ctx, data, "rol").Trim()
        Dim descripcion = ObtenerTexto(ctx, data, "descripcion").Trim()
        Dim activo = ObtenerBooleano(ctx, data, "activo", True)

        If rol = "" Then
            WriteError(ctx, "El rol es obligatorio.", 400)
            Return
        End If

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()

            If id > 0 AndAlso EsRolAdministrador(cn, id) AndAlso Not rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) Then
                WriteError(ctx, "El rol Administrador no se puede renombrar.", 400)
                Return
            End If

            If ExisteOtro(cn, "dbo.catalogo_roles_permisos", "rol", rol, id) Then
                WriteError(ctx, "Ya existe un rol con ese nombre.", 400)
                Return
            End If

            If id = 0 Then
                Using cmd As New SqlCommand("
                    INSERT INTO dbo.catalogo_roles_permisos (rol, descripcion, activo, fecha_creacion)
                    VALUES (@rol, @descripcion, @activo, GETDATE());", cn)
                    cmd.Parameters.Add("@rol", SqlDbType.VarChar, 100).Value = rol
                    cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 250).Value = DbNullIfEmpty(descripcion)
                    cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                    cmd.ExecuteNonQuery()
                End Using
            Else
                If EsRolAdministrador(cn, id) Then activo = True
                Using cmd As New SqlCommand("
                    UPDATE dbo.catalogo_roles_permisos
                    SET rol = @rol, descripcion = @descripcion, activo = @activo
                    WHERE id = @id;", cn)
                    cmd.Parameters.Add("@rol", SqlDbType.VarChar, 100).Value = rol
                    cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 250).Value = DbNullIfEmpty(descripcion)
                    cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                    cmd.ExecuteNonQuery()
                End Using
            End If
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Rol guardado correctamente."})
    End Sub

    Private Sub RolToggle(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim activo = ObtenerBooleano(ctx, data, "activo", False)

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            If EsRolAdministrador(cn, id) AndAlso Not activo Then
                WriteError(ctx, "El rol Administrador no se puede desactivar.", 400)
                Return
            End If

            Using cmd As New SqlCommand("UPDATE dbo.catalogo_roles_permisos SET activo = @activo WHERE id = @id;", cn)
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cmd.ExecuteNonQuery()
            End Using
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Estatus actualizado."})
    End Sub

    Private Sub PuestosListar(ByVal ctx As HttpContext)
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT id, puesto, descripcion, ISNULL(activo, 0) AS activo, fecha_creacion
                FROM dbo.catalogo_puestos
                ORDER BY puesto;", cn)
                WriteOk(ctx, LeerFilas(cmd))
            End Using
        End Using
    End Sub

    Private Sub PuestoGuardar(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim puesto = ObtenerTexto(ctx, data, "puesto").Trim()
        Dim descripcion = ObtenerTexto(ctx, data, "descripcion").Trim()
        Dim activo = ObtenerBooleano(ctx, data, "activo", True)

        If puesto = "" Then
            WriteError(ctx, "El puesto es obligatorio.", 400)
            Return
        End If

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()

            If ExisteOtro(cn, "dbo.catalogo_puestos", "puesto", puesto, id) Then
                WriteError(ctx, "Ya existe un puesto con ese nombre.", 400)
                Return
            End If

            If id = 0 Then
                Using cmd As New SqlCommand("
                    INSERT INTO dbo.catalogo_puestos (puesto, descripcion, activo, fecha_creacion)
                    VALUES (@puesto, @descripcion, @activo, GETDATE());", cn)
                    cmd.Parameters.Add("@puesto", SqlDbType.VarChar, 150).Value = puesto
                    cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 250).Value = DbNullIfEmpty(descripcion)
                    cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                    cmd.ExecuteNonQuery()
                End Using
            Else
                Using cmd As New SqlCommand("
                    UPDATE dbo.catalogo_puestos
                    SET puesto = @puesto,
                        descripcion = @descripcion,
                        activo = @activo,
                        fecha_modificacion = GETDATE()
                    WHERE id = @id;", cn)
                    cmd.Parameters.Add("@puesto", SqlDbType.VarChar, 150).Value = puesto
                    cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, 250).Value = DbNullIfEmpty(descripcion)
                    cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                    cmd.ExecuteNonQuery()
                End Using
            End If
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Puesto guardado correctamente."})
    End Sub

    Private Sub PuestoToggle(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim id = ObtenerEntero(ctx, data, "id", 0)
        Dim activo = ObtenerBooleano(ctx, data, "activo", False)

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("UPDATE dbo.catalogo_puestos SET activo = @activo, fecha_modificacion = GETDATE() WHERE id = @id;", cn)
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cmd.ExecuteNonQuery()
            End Using
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Estatus actualizado."})
    End Sub

    Private Sub PaginasListar(ByVal ctx As HttpContext)
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT id, titulo, ruta, modulo, es_menu, es_handler, ISNULL(activo, 0) AS activo
                FROM dbo.seguridad_paginas
                WHERE ISNULL(es_handler, 0) = 0
                ORDER BY modulo, titulo;", cn)
                WriteOk(ctx, LeerFilas(cmd))
            End Using
        End Using
    End Sub

    Private Sub PermisosPorRol(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim rolId = ObtenerEntero(ctx, data, "rol_id", 0)

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    m.id AS menu_id,
                    m.parent_id,
                    p.id AS page_id,
                    m.titulo,
                    ISNULL(m.icono, '') AS icono,
                    ISNULL(m.url, '') AS url,
                    ISNULL(m.orden, 0) AS orden,
                    p.id AS pagina_id,
                    p.ruta,
                    p.modulo,
                    CAST(CASE WHEN p.id IS NULL THEN 1 ELSE 0 END AS bit) AS es_categoria,
                    CAST(CASE WHEN p.id IS NULL THEN 0 ELSE ISNULL(rp.puede_ver, 0) END AS bit) AS permitido
                FROM dbo.seguridad_menu m
                LEFT JOIN dbo.seguridad_paginas p
                    ON p.id = m.page_id
                   AND ISNULL(p.activo, 0) = 1
                   AND ISNULL(p.es_handler, 0) = 0
                LEFT JOIN dbo.seguridad_rol_pagina rp
                    ON rp.pagina_id = p.id
                   AND rp.rol_id = @rol_id
                WHERE ISNULL(m.activo, 0) = 1
                  AND (m.page_id IS NULL OR p.id IS NOT NULL)
                ORDER BY ISNULL(m.parent_id, 0), m.orden, m.titulo;", cn)
                cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = rolId
                WriteOk(ctx, LeerFilas(cmd))
            End Using
        End Using
    End Sub

    Private Sub PermisosGuardar(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object))
        Dim rolId = ObtenerEntero(ctx, data, "rol_id", 0)
        If rolId <= 0 Then
            WriteError(ctx, "Selecciona un rol.", 400)
            Return
        End If

        Dim paginaIds = ObtenerListaEnteros(data, "pagina_ids").Distinct().ToList()

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()

            If EsRolAdministrador(cn, rolId) Then
                OtorgarTodoAdministrador(cn, rolId)
                WriteJson(ctx, New With {.ok = True, .mensaje = "El rol Administrador conserva acceso completo."})
                Return
            End If

            Using tr = cn.BeginTransaction()
                Try
                    Using cmd As New SqlCommand("
                        DELETE rp
                        FROM dbo.seguridad_rol_pagina rp
                        INNER JOIN dbo.seguridad_paginas p ON p.id = rp.pagina_id
                        INNER JOIN dbo.seguridad_menu m ON m.page_id = p.id AND ISNULL(m.activo, 0) = 1
                        WHERE rp.rol_id = @rol_id
                          AND ISNULL(p.es_handler, 0) = 0;", cn, tr)
                        cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = rolId
                        cmd.ExecuteNonQuery()
                    End Using

                    For Each paginaId In paginaIds
                        Using cmd As New SqlCommand("
                            INSERT INTO dbo.seguridad_rol_pagina (rol_id, pagina_id, puede_ver, puede_crear, puede_editar, puede_eliminar)
                            SELECT @rol_id, id, 1, 0, 0, 0
                            FROM dbo.seguridad_paginas
                            WHERE id = @pagina_id
                              AND activo = 1
                              AND ISNULL(es_handler, 0) = 0
                              AND EXISTS (
                                  SELECT 1
                                  FROM dbo.seguridad_menu
                                  WHERE page_id = @pagina_id
                                    AND ISNULL(activo, 0) = 1
                              );", cn, tr)
                            cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = rolId
                            cmd.Parameters.Add("@pagina_id", SqlDbType.Int).Value = paginaId
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    tr.Commit()
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        WriteJson(ctx, New With {.ok = True, .mensaje = "Permisos guardados correctamente."})
    End Sub

    Private Function LeerJson(ByVal ctx As HttpContext) As Dictionary(Of String, Object)
        Dim data As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
        If ctx.Request.HttpMethod <> "POST" Then Return data

        Dim body As String = ""
        Try
            If ctx.Request.InputStream.CanSeek Then ctx.Request.InputStream.Position = 0
            Using sr As New StreamReader(ctx.Request.InputStream)
                body = sr.ReadToEnd()
            End Using
        Catch
            body = ""
        End Try

        If String.IsNullOrWhiteSpace(body) Then Return data

        Try
            Dim parsed = serializer.Deserialize(Of Dictionary(Of String, Object))(body)
            If parsed IsNot Nothing Then Return New Dictionary(Of String, Object)(parsed, StringComparer.OrdinalIgnoreCase)
        Catch
        End Try

        Return data
    End Function

    Private Function ObtenerTexto(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object), ByVal key As String) As String
        If data IsNot Nothing AndAlso data.ContainsKey(key) AndAlso data(key) IsNot Nothing Then Return Convert.ToString(data(key))
        Dim v = ctx.Request(key)
        If v Is Nothing Then Return ""
        Return v
    End Function

    Private Function ObtenerEntero(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object), ByVal key As String, ByVal defaultValue As Integer) As Integer
        Dim s = ObtenerTexto(ctx, data, key)
        Dim v As Integer
        If Integer.TryParse(s, v) Then Return v
        Return defaultValue
    End Function

    Private Function ObtenerEnteroNullable(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object), ByVal key As String) As Integer?
        Dim s = ObtenerTexto(ctx, data, key).Trim()
        If s = "" Then Return Nothing
        Dim v As Integer
        If Integer.TryParse(s, v) AndAlso v > 0 Then Return v
        Return Nothing
    End Function

    Private Function ObtenerBooleano(ByVal ctx As HttpContext, ByVal data As Dictionary(Of String, Object), ByVal key As String, ByVal defaultValue As Boolean) As Boolean
        If data IsNot Nothing AndAlso data.ContainsKey(key) AndAlso data(key) IsNot Nothing Then
            Dim o = data(key)
            If TypeOf o Is Boolean Then Return Convert.ToBoolean(o)
            Dim raw = Convert.ToString(o).Trim().ToLowerInvariant()
            If raw = "1" OrElse raw = "true" OrElse raw = "si" Then Return True
            If raw = "0" OrElse raw = "false" OrElse raw = "no" Then Return False
        End If

        Dim s = ctx.Request(key)
        If String.IsNullOrWhiteSpace(s) Then Return defaultValue
        s = s.Trim().ToLowerInvariant()
        If s = "1" OrElse s = "true" OrElse s = "si" Then Return True
        If s = "0" OrElse s = "false" OrElse s = "no" Then Return False
        Return defaultValue
    End Function

    Private Function ObtenerListaEnteros(ByVal data As Dictionary(Of String, Object), ByVal key As String) As List(Of Integer)
        Dim ids As New List(Of Integer)()
        If data Is Nothing OrElse Not data.ContainsKey(key) OrElse data(key) Is Nothing Then Return ids

        Dim lista = TryCast(data(key), ArrayList)
        If lista Is Nothing Then
            Dim singleValue As Integer
            If Integer.TryParse(Convert.ToString(data(key)), singleValue) Then ids.Add(singleValue)
            Return ids
        End If

        For Each item In lista
            Dim v As Integer
            If item IsNot Nothing AndAlso Integer.TryParse(Convert.ToString(item), v) Then ids.Add(v)
        Next

        Return ids
    End Function

    Private Function LeerFilas(ByVal cmd As SqlCommand) As List(Of Dictionary(Of String, Object))
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Using rd = cmd.ExecuteReader()
            While rd.Read()
                Dim row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                For i As Integer = 0 To rd.FieldCount - 1
                    row(rd.GetName(i)) = If(rd.IsDBNull(i), Nothing, rd.GetValue(i))
                Next
                rows.Add(row)
            End While
        End Using
        Return rows
    End Function

    Private Sub WriteOk(ByVal ctx As HttpContext, ByVal data As Object)
        WriteJson(ctx, New With {.ok = True, .data = data})
    End Sub

    Private Sub WriteError(ByVal ctx As HttpContext, ByVal mensaje As String, Optional ByVal statusCode As Integer = 200)
        ctx.Response.StatusCode = statusCode
        WriteJson(ctx, New With {.ok = False, .mensaje = mensaje})
    End Sub

    Private Sub WriteJson(ByVal ctx As HttpContext, ByVal obj As Object)
        ctx.Response.ContentType = "application/json; charset=utf-8"
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Function DbNullIfEmpty(ByVal value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Return value.Trim()
    End Function

    Private Sub AgregarUsuarioParams(ByVal cmd As SqlCommand,
                                     ByVal usuario As String,
                                     ByVal nombre As String,
                                     ByVal email As String,
                                     ByVal rolId As Integer,
                                     ByVal puestoId As Integer?,
                                     ByVal activo As Boolean,
                                     ByVal debeCambiar As Boolean)
        cmd.Parameters.Add("@usuario", SqlDbType.VarChar, 80).Value = usuario
        cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 150).Value = nombre
        cmd.Parameters.Add("@email", SqlDbType.VarChar, 150).Value = DbNullIfEmpty(email)
        cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = rolId
        cmd.Parameters.Add("@puesto_id", SqlDbType.Int).Value = If(puestoId.HasValue, CType(puestoId.Value, Object), DBNull.Value)
        cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo
        cmd.Parameters.Add("@debe", SqlDbType.Bit).Value = debeCambiar
    End Sub

    Private Function ExisteOtro(ByVal cn As SqlConnection,
                                ByVal tabla As String,
                                ByVal columna As String,
                                ByVal valor As String,
                                ByVal id As Integer) As Boolean
        Dim sql = "SELECT 1 FROM " & tabla & " WHERE " & columna & " = @valor AND id <> @id;"
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@valor", SqlDbType.VarChar, 150).Value = valor
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
            Return cmd.ExecuteScalar() IsNot Nothing
        End Using
    End Function

    Private Function EsRolAdministrador(ByVal cn As SqlConnection, ByVal rolId As Integer) As Boolean
        Using cmd As New SqlCommand("SELECT 1 FROM dbo.catalogo_roles_permisos WHERE id = @id AND rol = 'Administrador';", cn)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = rolId
            Return cmd.ExecuteScalar() IsNot Nothing
        End Using
    End Function

    Private Sub OtorgarTodoAdministrador(ByVal cn As SqlConnection, ByVal rolId As Integer)
        Using cmd As New SqlCommand("
            INSERT INTO dbo.seguridad_rol_pagina (rol_id, pagina_id, puede_ver, puede_crear, puede_editar, puede_eliminar)
            SELECT @rol_id, p.id, 1, 0, 0, 0
            FROM dbo.seguridad_paginas p
            INNER JOIN dbo.seguridad_menu m ON m.page_id = p.id AND ISNULL(m.activo, 0) = 1
            WHERE p.activo = 1
              AND ISNULL(p.es_handler, 0) = 0
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.seguridad_rol_pagina rp
                  WHERE rp.rol_id = @rol_id
                    AND rp.pagina_id = p.id
              );

            UPDATE dbo.seguridad_rol_pagina
            SET puede_ver = 1,
                puede_crear = 0,
                puede_editar = 0,
                puede_eliminar = 0
            WHERE rol_id = @rol_id
              AND pagina_id IN (
                  SELECT p.id
                  FROM dbo.seguridad_paginas p
                  INNER JOIN dbo.seguridad_menu m ON m.page_id = p.id AND ISNULL(m.activo, 0) = 1
                  WHERE ISNULL(p.es_handler, 0) = 0
              );", cn)
            cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = rolId
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function SanitizarReturnUrl(ByVal returnUrl As String) As String
        If String.IsNullOrWhiteSpace(returnUrl) Then Return "/Default.aspx"
        returnUrl = returnUrl.Trim()
        If returnUrl.StartsWith("/") AndAlso Not returnUrl.StartsWith("//") AndAlso Not returnUrl.StartsWith("/\") Then Return returnUrl
        Return "/Default.aspx"
    End Function
End Class
