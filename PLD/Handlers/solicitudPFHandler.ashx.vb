Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Collections.Generic

Public Class solicitudPFHandler : Implements IHttpHandler
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim action As String = (If(context.Request("action"), "")).Trim().ToLowerInvariant()
        If action = "listar" Then
            ListarSolicitudes(context)
            Return
        End If
        context.Response.Write("{""ok"":false,""error"":""Acción no válida""}")
    End Sub

    Private Sub ListarSolicitudes(ByVal context As HttpContext)
        Dim lista As New List(Of Dictionary(Of String, Object))()
        Try
            Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                conn.Open()
                Dim sql As String = "
SELECT TOP 200
    sc.id, sc.monto_solicitado, sc.monto_autorizado, sc.fecha_vigencia_inicio, sc.fecha_vigencia_fin,
    sc.estatus, sc.fecha_creacion,
    c.id_cliente,
    CONCAT(c.primer_nombre, ' ', ISNULL(c.segundo_nombre,''), ' ', c.apellido_paterno, ' ', ISNULL(c.apellido_materno,'')) AS nombre_completo,
    pf.id AS producto_id, pf.descripcion_larga AS producto_nombre,
    cc.nombre_credito AS tipo_credito, ISNULL(cc.es_revolvente,0) AS es_revolvente,
    vr.saldo_utilizado, vr.disponible
FROM dbo.solicitud_credito sc
LEFT JOIN dbo.cliente_persona_fisica c ON c.id_cliente = sc.cliente_id
LEFT JOIN dbo.catalogo_producto_financiero pf ON pf.id = sc.producto_financiero_id
LEFT JOIN dbo.catalogo_creditos cc ON cc.id = pf.tipo_credito_id
LEFT JOIN dbo.vw_credito_revolvente_saldo vr ON vr.solicitud_credito_id = sc.id
ORDER BY sc.fecha_creacion DESC;"
                Using cmd As New SqlCommand(sql, conn)
                    Using rd As SqlDataReader = cmd.ExecuteReader()
                        While rd.Read()
                            lista.Add(New Dictionary(Of String, Object) From {
                                {"id", rd("id")},
                                {"cliente_id", If(IsDBNull(rd("id_cliente")), Nothing, rd("id_cliente"))},
                                {"cliente", If(IsDBNull(rd("nombre_completo")), Nothing, rd("nombre_completo"))},
                                {"producto", If(IsDBNull(rd("producto_nombre")), Nothing, rd("producto_nombre"))},
                                {"tipo_credito", If(IsDBNull(rd("tipo_credito")), Nothing, rd("tipo_credito"))},
                                {"es_revolvente", If(IsDBNull(rd("es_revolvente")), 0, rd("es_revolvente"))},
                                {"monto_solicitado", If(IsDBNull(rd("monto_solicitado")), Nothing, rd("monto_solicitado"))},
                                {"monto_autorizado", If(IsDBNull(rd("monto_autorizado")), Nothing, rd("monto_autorizado"))},
                                {"saldo_utilizado", If(IsDBNull(rd("saldo_utilizado")), Nothing, rd("saldo_utilizado"))},
                                {"disponible", If(IsDBNull(rd("disponible")), Nothing, rd("disponible"))},
                                {"fecha_vigencia_inicio", If(IsDBNull(rd("fecha_vigencia_inicio")), Nothing, rd("fecha_vigencia_inicio"))},
                                {"fecha_vigencia_fin", If(IsDBNull(rd("fecha_vigencia_fin")), Nothing, rd("fecha_vigencia_fin"))},
                                {"estatus", rd("estatus")},
                                {"fecha_creacion", rd("fecha_creacion")}
                            })
                        End While
                    End Using
                End Using
            End Using
            context.Response.Write(New JavaScriptSerializer().Serialize(New Dictionary(Of String, Object) From {{"ok", True}, {"data", lista}}))
        Catch ex As Exception
            context.Response.Write(New JavaScriptSerializer().Serialize(New Dictionary(Of String, Object) From {{"ok", False}, {"error", ex.Message}}))
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
End Class
