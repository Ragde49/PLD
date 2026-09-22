Imports System
Imports System.Web
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration

Public Class handler_tipo_estado_cuenta
    Implements IHttpHandler

    Private ReadOnly ser As New JavaScriptSerializer() With {.MaxJsonLength = Integer.MaxValue}

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim op As String = (context.Request("op") & "").ToLower().Trim()

        Try
            Select Case op
                Case "consultar" : Consultar(context, True)   ' usado por el modal
                Case "consultar_todos" : Consultar(context, False)
                Case Else
                    Write(context, New With {.ok = False, .mensaje = "Operación no válida."})
            End Select
        Catch ex As Exception
            Write(context, New With {.ok = False, .mensaje = ex.Message})
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    ' ===== Helpers =====
    Private Function CnStr() As String
        Dim cs = ConfigurationManager.ConnectionStrings("PLDConnection")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New Exception("Falta connectionStrings('PLDConnection') en web.config.")
        End If
        Return cs.ConnectionString
    End Function

    Private Sub Write(ctx As HttpContext, obj As Object)
        ctx.Response.Write(ser.Serialize(obj))
    End Sub

    ' ===== Ops =====
    Private Sub Consultar(ctx As HttpContext, soloActivos As Boolean)
        Dim sql As String = "SELECT id, descripcion, activo FROM dbo.catalogo_tipo_estado_cuenta"
        If soloActivos Then sql &= " WHERE activo = 1"
        sql &= " ORDER BY descripcion;"

        Dim list As New List(Of Dictionary(Of String, Object))()
        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using cmd As New SqlCommand(sql, cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        list.Add(New Dictionary(Of String, Object) From {
                            {"id", rd("id")},
                            {"descripcion", rd("descripcion")},
                            {"activo", rd("activo")}
                        })
                    End While
                End Using
            End Using
        End Using
        Write(ctx, list)
    End Sub

End Class