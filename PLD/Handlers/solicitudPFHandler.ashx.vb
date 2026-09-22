Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class solicitudPFHandler : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"

        Dim action As String = context.Request("action")

        If action = "listar" Then
            ListarSolicitudes(context)
            Return
        End If

        context.Response.Write("{""error"":""Acción no válida""}")
    End Sub


    Private Sub ListarSolicitudes(context As HttpContext)
        Dim lista As New List(Of Dictionary(Of String, Object))()

        Try
            Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
                conn.Open()

                Dim sql As String = "
                    SELECT TOP 200
                        sc.id,
                        sc.monto_solicitado,
                        sc.estatus,
                        sc.fecha_creacion,

                        c.id_cliente,
                        CONCAT(c.primer_nombre, ' ', ISNULL(c.segundo_nombre,''), ' ', 
                               c.apellido_paterno, ' ', ISNULL(c.apellido_materno,'')) AS nombre_completo,

                        pf.id AS producto_id,
                        pf.descripcion_larga AS producto_nombre
                    FROM solicitud_credito sc
                    LEFT JOIN cliente_persona_fisica c
                        ON c.id_cliente = sc.cliente_id
                    LEFT JOIN catalogo_producto_financiero pf
                        ON pf.id = sc.producto_financiero_id
                    ORDER BY sc.fecha_creacion DESC
                "

                Using cmd As New SqlCommand(sql, conn)
                    Dim rd = cmd.ExecuteReader()
                    While rd.Read()
                        Dim fila As New Dictionary(Of String, Object) From {
                            {"id", rd("id")},
                            {"cliente_id", rd("id_cliente")},
                            {"cliente", rd("nombre_completo")},
                            {"producto", rd("producto_nombre")},
                            {"monto_solicitado", rd("monto_solicitado")},
                            {"estatus", rd("estatus")},
                            {"fecha_creacion", rd("fecha_creacion")}
                        }
                        lista.Add(fila)
                    End While
                End Using
            End Using

            Dim json As String = New JavaScriptSerializer().Serialize(
                New Dictionary(Of String, Object) From {
                    {"ok", True},
                    {"data", lista}
                }
            )
            context.Response.Write(json)

        Catch ex As Exception
            context.Response.Write("{""ok"":false,""error"":""" &
                                   ex.Message.Replace("""", "'") & """}")
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
