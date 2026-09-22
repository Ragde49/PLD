Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions

Public Class handler_consulta_listas_pld
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = Param(context, "action").Trim().ToLowerInvariant()

            Select Case action
                Case "buscar"
                    respuesta = Buscar(context)
                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select
        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_consulta_listas_pld."
            respuesta("detalle") = ex.Message
        End Try

        context.Response.Write(serializer.Serialize(respuesta))
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Private Function CnStr() As String
        Return ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
    End Function

    Private Function Param(ByVal ctx As HttpContext, ByVal nombre As String) As String
        If ctx.Request(nombre) Is Nothing Then Return ""
        Return Convert.ToString(ctx.Request(nombre))
    End Function

    Private Function Usuario(ByVal ctx As HttpContext) As String
        If ctx.Session IsNot Nothing AndAlso ctx.Session("usuario") IsNot Nothing Then
            Dim u As String = Convert.ToString(ctx.Session("usuario")).Trim()
            If u <> "" Then Return u
        End If
        Return "SISTEMA"
    End Function

    Private Function ToInt(ByVal valor As Object, Optional ByVal def As Integer = 0) As Integer
        Dim n As Integer = def
        If valor IsNot Nothing Then Integer.TryParse(Convert.ToString(valor), n)
        Return n
    End Function

    Private Function NormalizarNombre(ByVal valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then Return ""
        Dim formD As String = valor.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder()
        For Each ch As Char In formD
            Dim cat As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch)
            If cat <> UnicodeCategory.NonSpacingMark Then
                If Char.IsLetterOrDigit(ch) OrElse Char.IsWhiteSpace(ch) Then
                    sb.Append(ch)
                Else
                    sb.Append(" "c)
                End If
            End If
        Next
        Return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), "\s+", " ").Trim()
    End Function

    Private Function NormalizarClave(ByVal valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then Return ""
        Return Regex.Replace(valor.Trim().ToUpperInvariant(), "[^A-Z0-9]", "")
    End Function

    Private Function Query(ByVal cn As SqlConnection, ByVal tr As SqlTransaction, ByVal sql As String, ByVal ps As List(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()
        Using cmd As New SqlCommand(sql, cn, tr)
            If ps IsNot Nothing Then cmd.Parameters.AddRange(ps.ToArray())
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function

    Private Function Buscar(ByVal ctx As HttpContext) As Dictionary(Of String, Object)
        Dim r As New Dictionary(Of String, Object)()
        Dim nombre As String = Param(ctx, "nombre").Trim()
        Dim rfc As String = Param(ctx, "rfc").Trim()
        Dim curp As String = Param(ctx, "curp").Trim()
        Dim clienteId As Integer = ToInt(Param(ctx, "cliente_id"), 0)

        Dim nn As String = NormalizarNombre(nombre)
        Dim rn As String = NormalizarClave(rfc)
        Dim cnorm As String = NormalizarClave(curp)

        If nn = "" AndAlso rn = "" AndAlso cnorm = "" Then
            r("ok") = False
            r("mensaje") = "Captura nombre, RFC o CURP para consultar."
            Return r
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim ps As New List(Of SqlParameter)()
                    Dim cond As New List(Of String)()

                    If nn <> "" Then
                        cond.Add("p.nombre_normalizado=@nombre")
                        ps.Add(New SqlParameter("@nombre", SqlDbType.NVarChar, 250) With {.Value = nn})
                    End If
                    If rn <> "" Then
                        cond.Add("p.rfc_normalizado=@rfc")
                        ps.Add(New SqlParameter("@rfc", SqlDbType.VarChar, 20) With {.Value = rn})
                    End If
                    If cnorm <> "" Then
                        cond.Add("p.curp_normalizado=@curp")
                        ps.Add(New SqlParameter("@curp", SqlDbType.VarChar, 30) With {.Value = cnorm})
                    End If

                    Dim dt As DataTable = Query(
                        cn, tr,
                        "SELECT p.id AS persona_id,p.nombre,p.nombre_normalizado,p.rfc,p.rfc_normalizado,p.curp,p.curp_normalizado," &
                        "l.id AS lista_id,l.clave AS lista_clave,l.nombre AS lista,c.id AS carga_id,c.nombre_archivo,c.referencia_fuente,c.fecha_recepcion,c.fecha_activacion " &
                        "FROM dbo.listas_pld_personas p " &
                        "INNER JOIN dbo.listas_pld_cargas c ON c.id=p.carga_id AND c.activo=1 AND c.vigente=1 " &
                        "INNER JOIN dbo.catalogo_listas_pld l ON l.id=c.lista_id AND l.activo=1 " &
                        "WHERE p.activo=1 AND (" & String.Join(" OR ", cond.ToArray()) & ") " &
                        "ORDER BY l.nombre,p.nombre;",
                        ps
                    )

                    Dim consultaId As Long
                    Using cmd As New SqlCommand(
                        "INSERT INTO dbo.listas_pld_consultas(cliente_id,nombre_consultado,rfc_consultado,curp_consultado,coincidencias,usuario) " &
                        "VALUES(@cliente,@nombre,@rfc,@curp,@coinc,@usuario); SELECT CAST(SCOPE_IDENTITY() AS bigint);", cn, tr)
                        cmd.Parameters.Add("@cliente", SqlDbType.Int).Value = If(clienteId > 0, CType(clienteId, Object), DBNull.Value)
                        cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 250).Value = If(nombre = "", CType(DBNull.Value, Object), nombre)
                        cmd.Parameters.Add("@rfc", SqlDbType.VarChar, 20).Value = If(rfc = "", CType(DBNull.Value, Object), rfc.ToUpperInvariant())
                        cmd.Parameters.Add("@curp", SqlDbType.VarChar, 30).Value = If(curp = "", CType(DBNull.Value, Object), curp.ToUpperInvariant())
                        cmd.Parameters.Add("@coinc", SqlDbType.Int).Value = dt.Rows.Count
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(ctx)
                        consultaId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    Dim lista As New List(Of Dictionary(Of String, Object))()

                    For Each row As DataRow In dt.Rows
                        Dim tipos As New List(Of String)()
                        If nn <> "" AndAlso Convert.ToString(row("nombre_normalizado")) = nn Then tipos.Add("NOMBRE")
                        If rn <> "" AndAlso Convert.ToString(row("rfc_normalizado")) = rn Then tipos.Add("RFC")
                        If cnorm <> "" AndAlso Convert.ToString(row("curp_normalizado")) = cnorm Then tipos.Add("CURP")
                        Dim tipo As String = String.Join("+", tipos.ToArray())

                        Using cmd As New SqlCommand(
                            "INSERT INTO dbo.listas_pld_consulta_resultados(consulta_id,persona_id,tipo_coincidencia) VALUES(@consulta,@persona,@tipo);", cn, tr)
                            cmd.Parameters.Add("@consulta", SqlDbType.BigInt).Value = consultaId
                            cmd.Parameters.Add("@persona", SqlDbType.BigInt).Value = Convert.ToInt64(row("persona_id"))
                            cmd.Parameters.Add("@tipo", SqlDbType.VarChar, 50).Value = tipo
                            cmd.ExecuteNonQuery()
                        End Using

                        Dim item As New Dictionary(Of String, Object)()
                        For Each col As DataColumn In dt.Columns
                            If col.ColumnName <> "nombre_normalizado" AndAlso
                               col.ColumnName <> "rfc_normalizado" AndAlso
                               col.ColumnName <> "curp_normalizado" Then
                                item(col.ColumnName) = If(row(col) Is DBNull.Value, Nothing, row(col))
                            End If
                        Next
                        item("tipo_coincidencia") = tipo
                        lista.Add(item)
                    Next

                    tr.Commit()

                    r("ok") = True
                    r("consulta_id") = consultaId
                    r("coincidencias") = lista.Count
                    r("data") = lista
                    r("mensaje") = If(
                        lista.Count = 0,
                        "Sin coincidencias exactas en las versiones vigentes.",
                        "Se encontraron coincidencias exactas. Requieren revisión; no implican identidad confirmada."
                    )
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return r
    End Function
End Class
