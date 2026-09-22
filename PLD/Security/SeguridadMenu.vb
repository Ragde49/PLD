Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text
Imports System.Web

Public NotInheritable Class SeguridadMenu
    Private Sub New()
    End Sub

    Private Class ItemMenu
        Public Property Id As Integer
        Public Property ParentId As Integer?
        Public Property PageId As Integer?
        Public Property Titulo As String
        Public Property Icono As String
        Public Property Url As String
        Public Property Ruta As String
        Public Property Orden As Integer
        Public Property Permitido As Boolean
    End Class

    Public Shared Function Renderizar(ByVal ctx As HttpContext) As String
        Try
            Dim sesion = SeguridadAuth.ObtenerUsuarioActual(ctx)
            If sesion Is Nothing Then Return RenderizarFallback()

            Dim items = CargarItems(ctx, sesion)
            Dim sb As New StringBuilder()
            sb.AppendLine("<ul class=""menu"">")
            sb.AppendLine("<li class=""menu-title"">Navegacion</li>")

            For Each item In items.FindAll(Function(x) Not x.ParentId.HasValue)
                RenderizarItem(sb, items, item, 0)
            Next

            sb.AppendLine("</ul>")
            Return sb.ToString()
        Catch
            Return RenderizarFallback()
        End Try
    End Function

    Public Shared Function ListarPlano() As List(Of Dictionary(Of String, Object))
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    m.id,
                    m.parent_id,
                    m.page_id,
                    m.titulo,
                    m.icono,
                    m.url,
                    m.orden,
                    ISNULL(m.activo, 0) AS activo,
                    p.ruta
                FROM dbo.seguridad_menu m
                LEFT JOIN dbo.seguridad_paginas p ON p.id = m.page_id
                ORDER BY ISNULL(m.parent_id, 0), m.orden, m.titulo;", cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        Dim row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                        row("id") = Convert.ToInt32(rd("id"))
                        row("parent_id") = If(IsDBNull(rd("parent_id")), Nothing, rd("parent_id"))
                        row("page_id") = If(IsDBNull(rd("page_id")), Nothing, rd("page_id"))
                        row("titulo") = Convert.ToString(rd("titulo"))
                        row("icono") = Convert.ToString(rd("icono"))
                        row("url") = Convert.ToString(rd("url"))
                        row("orden") = Convert.ToInt32(rd("orden"))
                        row("activo") = Convert.ToBoolean(rd("activo"))
                        row("ruta") = If(IsDBNull(rd("ruta")), Nothing, rd("ruta"))
                        rows.Add(row)
                    End While
                End Using
            End Using
        End Using
        Return rows
    End Function

    Private Shared Function CargarItems(ByVal ctx As HttpContext, ByVal sesion As SeguridadSesion) As List(Of ItemMenu)
        Dim items As New List(Of ItemMenu)()

        Using cn As New SqlConnection(SeguridadAuth.CadenaConexion())
            cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    m.id,
                    m.parent_id,
                    m.page_id,
                    m.titulo,
                    ISNULL(m.icono, '') AS icono,
                    ISNULL(m.url, '') AS url,
                    ISNULL(m.orden, 0) AS orden,
                    p.ruta,
                    CASE
                        WHEN @es_admin = 1 THEN 1
                        WHEN m.page_id IS NULL THEN 1
                        WHEN rp.rol_id IS NOT NULL AND ISNULL(rp.puede_ver, 0) = 1 THEN 1
                        ELSE 0
                    END AS permitido
                FROM dbo.seguridad_menu m
                LEFT JOIN dbo.seguridad_paginas p ON p.id = m.page_id AND ISNULL(p.activo, 0) = 1
                LEFT JOIN dbo.seguridad_rol_pagina rp ON rp.pagina_id = p.id AND rp.rol_id = @rol_id
                WHERE ISNULL(m.activo, 0) = 1
                ORDER BY ISNULL(m.parent_id, 0), m.orden, m.titulo;", cn)
                cmd.Parameters.Add("@rol_id", SqlDbType.Int).Value = sesion.RolId
                cmd.Parameters.Add("@es_admin", SqlDbType.Bit).Value = sesion.Rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase)

                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        items.Add(New ItemMenu With {
                            .Id = Convert.ToInt32(rd("id")),
                            .ParentId = If(IsDBNull(rd("parent_id")), CType(Nothing, Integer?), Convert.ToInt32(rd("parent_id"))),
                            .PageId = If(IsDBNull(rd("page_id")), CType(Nothing, Integer?), Convert.ToInt32(rd("page_id"))),
                            .Titulo = Convert.ToString(rd("titulo")),
                            .Icono = Convert.ToString(rd("icono")),
                            .Url = Convert.ToString(rd("url")),
                            .Ruta = If(IsDBNull(rd("ruta")), "", Convert.ToString(rd("ruta"))),
                            .Orden = Convert.ToInt32(rd("orden")),
                            .Permitido = Convert.ToBoolean(rd("permitido"))
                        })
                    End While
                End Using
            End Using
        End Using

        Return items
    End Function

    Private Shared Function TieneHijosPermitidos(ByVal items As List(Of ItemMenu), ByVal id As Integer) As Boolean
        For Each child In items.FindAll(Function(x) x.ParentId.HasValue AndAlso x.ParentId.Value = id)
            If child.Permitido AndAlso (child.PageId.HasValue OrElse TieneHijosPermitidos(items, child.Id)) Then Return True
        Next
        Return False
    End Function

    Private Shared Sub RenderizarItem(ByVal sb As StringBuilder,
                                      ByVal items As List(Of ItemMenu),
                                      ByVal item As ItemMenu,
                                      ByVal nivel As Integer)
        Dim hijos = items.FindAll(Function(x) x.ParentId.HasValue AndAlso x.ParentId.Value = item.Id)
        Dim tieneHijos = hijos.Exists(Function(x) x.Permitido AndAlso (x.PageId.HasValue OrElse TieneHijosPermitidos(items, x.Id)))

        If Not item.Permitido AndAlso Not tieneHijos Then Return
        If Not item.PageId.HasValue AndAlso Not tieneHijos AndAlso String.IsNullOrWhiteSpace(item.Url) Then Return

        sb.AppendLine("<li class=""menu-item"">")

        If tieneHijos Then
            Dim collapseId = "menuSeg" & item.Id.ToString()
            sb.Append("<a href=""#").Append(collapseId).Append(""" data-bs-toggle=""collapse"" class=""menu-link"">")
            If nivel = 0 Then AppendIcono(sb, item.Icono)
            sb.Append("<span class=""menu-text"">").Append(HttpUtility.HtmlEncode(item.Titulo)).Append("</span>")
            sb.Append("<span class=""menu-arrow""></span></a>")
            sb.AppendLine("<div class=""collapse"" id=""" & collapseId & """><ul class=""sub-menu"">")
            For Each child In hijos
                RenderizarItem(sb, items, child, nivel + 1)
            Next
            sb.AppendLine("</ul></div>")
        Else
            Dim url = If(String.IsNullOrWhiteSpace(item.Url), item.Ruta, item.Url)
            If String.IsNullOrWhiteSpace(url) Then url = "#"
            sb.Append("<a href=""").Append(HttpUtility.HtmlAttributeEncode(url)).Append(""" class=""menu-link"">")
            If nivel = 0 Then AppendIcono(sb, item.Icono)
            sb.Append("<span class=""menu-text"">").Append(HttpUtility.HtmlEncode(item.Titulo)).Append("</span></a>")
        End If

        sb.AppendLine("</li>")
    End Sub

    Private Shared Sub AppendIcono(ByVal sb As StringBuilder, ByVal icono As String)
        If String.IsNullOrWhiteSpace(icono) Then icono = "circle"
        sb.Append("<span class=""menu-icon""><i data-feather=""")
        sb.Append(HttpUtility.HtmlAttributeEncode(icono))
        sb.Append("""></i></span>")
    End Sub

    Private Shared Function RenderizarFallback() As String
        Return "<ul class=""menu"">" &
            "<li class=""menu-title"">Navegacion</li>" &
            "<li class=""menu-item""><a href=""/default.aspx"" class=""menu-link""><span class=""menu-icon""><i data-feather=""home""></i></span><span class=""menu-text"">Inicio</span></a></li>" &
            "<li class=""menu-item""><a href=""/secure/seguridad.aspx"" class=""menu-link""><span class=""menu-icon""><i data-feather=""lock""></i></span><span class=""menu-text"">Seguridad</span></a></li>" &
            "</ul>"
    End Function
End Class
