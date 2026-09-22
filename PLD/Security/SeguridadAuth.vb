Imports System
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Principal
Imports System.Web
Imports System.Web.Security

Public NotInheritable Class SeguridadAuth
    Private Sub New()
    End Sub

    Public Shared Function CadenaConexion() As String
        Dim cs = ConfigurationManager.ConnectionStrings("PLDConnection")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New Exception("No existe connectionString 'PLDConnection' en web.config.")
        End If
        Return cs.ConnectionString
    End Function

    Public Shared Function ValidarCredenciales(ByVal usuario As String,
                                               ByVal password As String,
                                               ByRef sesion As SeguridadSesion,
                                               ByRef mensaje As String) As Boolean
        sesion = Nothing
        usuario = If(usuario, "").Trim()

        If usuario = "" OrElse String.IsNullOrEmpty(password) Then
            mensaje = "Captura usuario y contrasena."
            Return False
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT TOP 1
                    u.id,
                    u.usuario,
                    u.nombre,
                    u.password_hash,
                    u.rol_id,
                    r.rol,
                    ISNULL(u.activo, 0) AS usuario_activo,
                    ISNULL(r.activo, 0) AS rol_activo,
                    ISNULL(u.debe_cambiar_password, 0) AS debe_cambiar_password
                FROM dbo.seguridad_usuarios u
                INNER JOIN dbo.catalogo_roles_permisos r ON r.id = u.rol_id
                WHERE u.usuario = @usuario;", cn)
                cmd.Parameters.Add("@usuario", SqlDbType.VarChar, 80).Value = usuario

                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        mensaje = "Usuario o contrasena incorrectos."
                        Return False
                    End If

                    If Not Convert.ToBoolean(rd("usuario_activo")) Then
                        mensaje = "El usuario esta inactivo."
                        Return False
                    End If

                    If Not Convert.ToBoolean(rd("rol_activo")) Then
                        mensaje = "El rol del usuario esta inactivo."
                        Return False
                    End If

                    Dim hash = Convert.ToString(rd("password_hash"))
                    If Not SeguridadPassword.Verificar(password, hash) Then
                        mensaje = "Usuario o contrasena incorrectos."
                        Return False
                    End If

                    sesion = New SeguridadSesion With {
                        .Id = Convert.ToInt32(rd("id")),
                        .Usuario = Convert.ToString(rd("usuario")),
                        .Nombre = Convert.ToString(rd("nombre")),
                        .RolId = Convert.ToInt32(rd("rol_id")),
                        .Rol = Convert.ToString(rd("rol")),
                        .DebeCambiarPassword = Convert.ToBoolean(rd("debe_cambiar_password"))
                    }
                End Using
            End Using

            Using cmd As New SqlCommand("
                UPDATE dbo.seguridad_usuarios
                SET ultimo_acceso = GETDATE()
                WHERE id = @id;", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = sesion.Id
                cmd.ExecuteNonQuery()
            End Using
        End Using

        mensaje = "Acceso correcto."
        Return True
    End Function

    Public Shared Sub IniciarSesion(ByVal ctx As HttpContext,
                                    ByVal sesion As SeguridadSesion,
                                    Optional ByVal recordar As Boolean = False)
        If ctx Is Nothing OrElse sesion Is Nothing Then Return

        Dim expiracion = If(recordar, DateTime.Now.AddDays(7), DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes))
        Dim ticket As New FormsAuthenticationTicket(
            1,
            sesion.Usuario,
            DateTime.Now,
            expiracion,
            recordar,
            sesion.ToUserData(),
            FormsAuthentication.FormsCookiePath)

        Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
        cookie.HttpOnly = True
        cookie.Path = FormsAuthentication.FormsCookiePath
        If recordar Then cookie.Expires = expiracion
        If ctx.Request IsNot Nothing AndAlso ctx.Request.IsSecureConnection Then cookie.Secure = True

        ctx.Response.Cookies.Add(cookie)
        ctx.User = CrearPrincipal(sesion)

        If ctx.Session IsNot Nothing Then
            ctx.Session("usuario_id") = sesion.Id
            ctx.Session("usuario") = sesion.Usuario
            ctx.Session("nombre_usuario") = sesion.Nombre
            ctx.Session("rol_id") = sesion.RolId
            ctx.Session("rol") = sesion.Rol
        End If
    End Sub

    Public Shared Sub CerrarSesion(ByVal ctx As HttpContext)
        FormsAuthentication.SignOut()
        If ctx IsNot Nothing AndAlso ctx.Session IsNot Nothing Then
            ctx.Session.Clear()
        End If
    End Sub

    Public Shared Function ObtenerUsuarioActual(ByVal ctx As HttpContext) As SeguridadSesion
        If ctx Is Nothing OrElse ctx.Request Is Nothing Then Return Nothing

        Dim cookie = ctx.Request.Cookies(FormsAuthentication.FormsCookieName)
        If cookie Is Nothing OrElse String.IsNullOrEmpty(cookie.Value) Then Return Nothing

        Try
            Dim ticket = FormsAuthentication.Decrypt(cookie.Value)
            If ticket Is Nothing OrElse ticket.Expired Then Return Nothing
            Return SeguridadSesion.FromTicket(ticket)
        Catch
            Return Nothing
        End Try
    End Function

    Public Shared Sub RestaurarPrincipalDesdeCookie(ByVal ctx As HttpContext)
        Dim sesion = ObtenerUsuarioActual(ctx)
        If sesion Is Nothing Then Return
        ctx.User = CrearPrincipal(sesion)
        System.Threading.Thread.CurrentPrincipal = ctx.User
    End Sub

    Public Shared Function CambiarPassword(ByVal ctx As HttpContext,
                                           ByVal actual As String,
                                           ByVal nueva As String,
                                           ByRef mensaje As String) As Boolean
        Dim sesion = ObtenerUsuarioActual(ctx)
        If sesion Is Nothing Then
            mensaje = "La sesion expiro. Inicia sesion nuevamente."
            Return False
        End If

        If String.IsNullOrEmpty(nueva) OrElse nueva.Length < 8 Then
            mensaje = "La nueva contrasena debe tener al menos 8 caracteres."
            Return False
        End If

        Using cn As New SqlConnection(CadenaConexion())
            cn.Open()

            Dim hashActual As String = Nothing
            Using cmd As New SqlCommand("SELECT password_hash FROM dbo.seguridad_usuarios WHERE id = @id AND activo = 1;", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = sesion.Id
                Dim o = cmd.ExecuteScalar()
                If o IsNot Nothing AndAlso o IsNot DBNull.Value Then hashActual = Convert.ToString(o)
            End Using

            If String.IsNullOrEmpty(hashActual) OrElse Not SeguridadPassword.Verificar(actual, hashActual) Then
                mensaje = "La contrasena actual no es correcta."
                Return False
            End If

            Dim hashNuevo = SeguridadPassword.CrearHash(nueva)
            Using cmd As New SqlCommand("
                UPDATE dbo.seguridad_usuarios
                SET password_hash = @hash,
                    debe_cambiar_password = 0,
                    fecha_password = GETDATE(),
                    fecha_modificacion = GETDATE()
                WHERE id = @id;", cn)
                cmd.Parameters.Add("@hash", SqlDbType.VarChar, 300).Value = hashNuevo
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = sesion.Id
                cmd.ExecuteNonQuery()
            End Using
        End Using

        sesion.DebeCambiarPassword = False
        IniciarSesion(ctx, sesion, False)
        mensaje = "Contrasena actualizada correctamente."
        Return True
    End Function

    Private Shared Function CrearPrincipal(ByVal sesion As SeguridadSesion) As IPrincipal
        Dim identidad As New FormsIdentity(New FormsAuthenticationTicket(
            1,
            sesion.Usuario,
            DateTime.Now,
            DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
            False,
            sesion.ToUserData(),
            FormsAuthentication.FormsCookiePath))

        Return New GenericPrincipal(identidad, New String() {sesion.Rol})
    End Function
End Class
