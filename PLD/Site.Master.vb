Public Class SiteMaster

    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        litAppVersion.Text = v.ToString()
        litMenu.Text = SeguridadMenu.Renderizar(Context)
        CargarUsuarioActual()

        If Not Page.IsPostBack Then
            CargarBreadcrumbAutomatico()
        End If
    End Sub

    Private Sub CargarUsuarioActual()
        Dim sesion = SeguridadAuth.ObtenerUsuarioActual(Context)
        If sesion Is Nothing Then
            litCurrentUser.Text = ""
            Return
        End If

        litCurrentUser.Text =
            "<span class='small text-muted d-none d-md-inline'>" & HttpUtility.HtmlEncode(sesion.Nombre) & "</span>" &
            "<span class='badge bg-light text-dark d-none d-md-inline'>" & HttpUtility.HtmlEncode(sesion.Rol) & "</span>" &
            "<a class='btn btn-sm btn-outline-secondary' href='/Logout.aspx' title='Salir'>" &
            "<i class='fa fa-sign-out-alt'></i>" &
            "</a>"
    End Sub

    Private Sub CargarBreadcrumbAutomatico()
        Dim rutas As New Dictionary(Of String, String()) From {
        {"seguridad.aspx", New String() {"Configuración", "Seguridad"}},
        {"alertas_pld.aspx", New String() {"P.L.D.", "Alertas P.L.D."}},
        {"pagos_credito.aspx", New String() {"P.L.D.", "Pagos de Crédito P.L.D."}},
        {"config_alertas_destinatarios.aspx", New String() {"P.L.D.", "Destinatarios Alertas P.L.D."}},
        {"catalogos.aspx", New String() {"Configuración", "Catálogos"}},
        {"catalogo_fondeador.aspx", New String() {"Configuración", "Catálogos", "Fuente de Fondeo"}},
        {"catalogo_tipo_credito.aspx", New String() {"Configuración", "Catálogos", "Tipo de Crédito"}},
        {"solicitud_pf.aspx", New String() {"Captura de Clientes", "Solicitud PF"}},
        {"presolicitud.aspx", New String() {"Captura de Clientes", "Presolicitud"}},
        {"anexos.aspx", New String() {"Captura de Clientes", "Anexos"}},
        {"documentacion.aspx", New String() {"Captura de Clientes", "Documentación"}},
        {"buro.aspx", New String() {"Captura de Clientes", "Buró de Crédito"}},
        {"modulo_pld.aspx", New String() {"Evaluación PLD"}},
        {"reporte_general.aspx", New String() {"Reportes", "Reporte General"}},
        {"reporte_cobranza.aspx", New String() {"Reportes", "Cobranza"}}
        }

        Dim archivo As String = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower()
        Dim html As New Text.StringBuilder()

        html.Append("<nav aria-label='breadcrumb'>")
        html.Append("<ol class='breadcrumb mb-1'>")
        html.Append("<li class='breadcrumb-item'><a href='../default.aspx'>Inicio</a></li>")

        If rutas.ContainsKey(archivo) Then
            Dim niveles = rutas(archivo)
            For i As Integer = 0 To niveles.Length - 1
                If i < niveles.Length - 1 Then
                    html.Append("<li class='breadcrumb-item'><a href='#'>" & niveles(i) & "</a></li>")
                Else
                    html.Append("<li class='breadcrumb-item active' aria-current='page'>" & niveles(i) & "</li>")
                End If
            Next
        Else
            html.Append("<li class='breadcrumb-item active' aria-current='page'>" & Page.Title & "</li>")
        End If

        html.Append("</ol></nav>")

        LiteralBreadcrumb.Text = html.ToString()
    End Sub

End Class
