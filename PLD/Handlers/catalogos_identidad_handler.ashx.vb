Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration

Public Class catalogos_identidad_handler : Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json; charset=utf-8"
        Dim action As String = (context.Request("action") & "").Trim().ToLowerInvariant()

        Try
            Select Case action
                Case "ping"
                    WriteOk(context, New With {.pong = True})

                Case "nacionalidades"
                    HandleNacionalidades(context)

                Case "paises"
                    HandlePaises(context)

                Case "actividad_economica"
                    HandleActividadEconomica(context)

                Case "entidades_por_pais"
                    HandleEntidadesPorPais(context)

                Case Else
                    WriteError(context, "Acción no soportada.", 400)
            End Select

        Catch ex As Exception
            WriteError(context, "Error: " & ex.Message, 500)
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '==================== Helpers ====================

    Private Function Cnn() As SqlConnection
        Dim cs As String = ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString
        Return New SqlConnection(cs)
    End Function

    Private Sub WriteOk(ctx As HttpContext, payload As Object)
        Dim obj = New With {.ok = True, .data = payload}
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteOkList(ctx As HttpContext, rows As List(Of Dictionary(Of String, Object)))
        Dim obj = New With {.ok = True, .data = rows}
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Sub WriteError(ctx As HttpContext, msg As String, Optional statusCode As Integer = 500)
        ctx.Response.StatusCode = statusCode
        Dim obj = New With {.ok = False, .message = msg}
        ctx.Response.Write(serializer.Serialize(obj))
    End Sub

    Private Function ReadList(cmd As SqlCommand) As List(Of Dictionary(Of String, Object))
        Dim list As New List(Of Dictionary(Of String, Object))()
        Using da As New SqlDataAdapter(cmd)
            Dim dt As New DataTable()
            da.Fill(dt)
            For Each r As DataRow In dt.Rows
                Dim d As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                For Each c As DataColumn In dt.Columns
                    d(c.ColumnName) = If(IsDBNull(r(c)), Nothing, r(c))
                Next
                list.Add(d)
            Next
        End Using
        Return list
    End Function

    Private Function ExistsTable(conn As SqlConnection, schema As String, tableName As String) As Boolean
        Using cmd As New SqlCommand("
            SELECT 1
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t;", conn)
            cmd.Parameters.AddWithValue("@s", schema)
            cmd.Parameters.AddWithValue("@t", tableName)
            Dim o = cmd.ExecuteScalar()
            Return o IsNot Nothing
        End Using
    End Function

    '==================== Actions ====================

    ' 1) catalogo_nacionalidades
    '    id, pais, nacionalidad, clave, impacto, probabilidad, nivel_riesgo_pld, activo
    Private Sub HandleNacionalidades(ctx As HttpContext)
        Using cn = Cnn() : cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    id,
                    nacionalidad AS descripcion,
                    pais,
                    clave,
                    impacto,
                    probabilidad,
                    nivel_riesgo_pld
                FROM dbo.catalogo_nacionalidades WITH (NOLOCK)
                WHERE activo = 1
                ORDER BY nacionalidad;", cn)
                Dim rows = ReadList(cmd)
                WriteOkList(ctx, rows)
            End Using
        End Using
    End Sub

    ' 2) Países de nacimiento desde catalogo_nacionalidades (evitamos asumir otra tabla)
    '    Devuelve id=textoPais, descripcion=textoPais
    Private Sub HandlePaises(ctx As HttpContext)
        Using cn = Cnn() : cn.Open()
            Using cmd As New SqlCommand("
                SELECT DISTINCT
                    CAST(pais AS varchar(100)) AS id,
                    CAST(pais AS varchar(100)) AS descripcion
                FROM dbo.catalogo_nacionalidades WITH (NOLOCK)
                WHERE activo = 1
                AND pais IS NOT NULL AND LTRIM(RTRIM(pais)) <> ''
                ORDER BY pais;", cn)
                Dim rows = ReadList(cmd)
                WriteOkList(ctx, rows)
            End Using
        End Using
    End Sub

    ' 3) catalogo_actividad_economica
    '    id, nombre, activo
    Private Sub HandleActividadEconomica(ctx As HttpContext)
        Using cn = Cnn() : cn.Open()
            Using cmd As New SqlCommand("
                SELECT
                    id,
                    nombre AS descripcion
                FROM dbo.catalogo_actividad_economica WITH (NOLOCK)
                WHERE ISNULL(activo,1) = 1
                ORDER BY nombre;", cn)
                Dim rows = ReadList(cmd)
                WriteOkList(ctx, rows)
            End Using
        End Using
    End Sub

    ' 4) Entidades por país (estados/provincias)
    '    Intentamos consultar dbo.catalogo_estados si existe (id, nombre, pais_id ó pais)
    '    Si no existe, regresamos lista vacía (no tronamos).
    Private Sub HandleEntidadesPorPais(ctx As HttpContext)
        Dim paisIdOTexto As String = (ctx.Request("pais_id") & "").Trim()
        If String.IsNullOrEmpty(paisIdOTexto) Then
            WriteOkList(ctx, New List(Of Dictionary(Of String, Object))())
            Return
        End If

        Using cn = Cnn() : cn.Open()
            If Not ExistsTable(cn, "dbo", "catalogo_estados") Then
                ' No asumimos estructura si no existe la tabla
                WriteOkList(ctx, New List(Of Dictionary(Of String, Object))())
                Return
            End If

            ' Intento 1: estructura común (id, nombre, pais_id)
            Dim sql1 As String = "
                SELECT id, nombre AS descripcion
                FROM dbo.catalogo_estados WITH (NOLOCK)
                WHERE (CAST(pais_id AS varchar(100)) = @pid OR CAST(pais_id AS nvarchar(100)) = @pid)
                   OR (CAST(@pid AS varchar(100)) = CAST(@pid AS varchar(100)) AND 1=0) -- fuerza a ir al intento 2 si no hay match
                ORDER BY nombre;"

            ' Intento 2: si el país se guarda como texto (columna 'pais' varchar)
            Dim sql2 As String = "
                SELECT id, nombre AS descripcion
                FROM dbo.catalogo_estados WITH (NOLOCK)
                WHERE LTRIM(RTRIM(UPPER(pais))) = LTRIM(RTRIM(UPPER(@pid)))
                ORDER BY nombre;"

            ' Probamos sql1 primero; si no trae filas, probamos sql2
            Dim rows As List(Of Dictionary(Of String, Object))
            Using cmd1 As New SqlCommand(sql1, cn)
                cmd1.Parameters.AddWithValue("@pid", paisIdOTexto)
                rows = ReadList(cmd1)
            End Using

            If rows Is Nothing OrElse rows.Count = 0 Then
                Using cmd2 As New SqlCommand(sql2, cn)
                    cmd2.Parameters.AddWithValue("@pid", paisIdOTexto)
                    rows = ReadList(cmd2)
                End Using
            End If

            WriteOkList(ctx, rows)
        End Using
    End Sub

End Class
