Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Globalization

' CRUD Catálogo de Créditos (PLD) - JSON
' Ops:
'   op=consultar
'   op=obtener&id=##
'   op=guardar            (POST JSON)
'   op=actualizar         (POST JSON)
'   op=desactivar         (POST JSON {id})
'   op=activar            (POST JSON {id})
'   op=toggle             (POST JSON {id})

Public Class catalogo_creditos_pld
    Implements IHttpHandler

    Private ReadOnly ser As New JavaScriptSerializer() With {.MaxJsonLength = Integer.MaxValue}
    Private ReadOnly inv As CultureInfo = CultureInfo.InvariantCulture

    Public Sub ProcessRequest(ctx As HttpContext) Implements IHttpHandler.ProcessRequest
        ctx.Response.ContentType = "application/json"
        ctx.Response.ContentEncoding = Text.Encoding.UTF8

        Dim op As String = (ctx.Request("op") & "").Trim().ToLower()
        Try
            Select Case op
                Case "consultar" : Consultar(ctx)
                Case "obtener" : Obtener(ctx)
                Case "guardar" : Guardar(ctx)
                Case "actualizar" : Actualizar(ctx)
                Case "desactivar" : CambiarEstatus(ctx, 0)
                Case "activar" : CambiarEstatus(ctx, 1)
                Case "toggle" : ToggleEstatus(ctx)
                Case Else
                    Write(ctx, New With {.ok = False, .mensaje = "Operación no válida.", .op = op})
            End Select
        Catch ex As Exception
            Write(ctx, New With {.ok = False, .mensaje = ex.Message})
        End Try
    End Sub

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    '============= Helpers =============
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

    Private Function ReadBody(ctx As HttpContext) As String
        ctx.Request.InputStream.Position = 0
        Using sr As New IO.StreamReader(ctx.Request.InputStream, Text.Encoding.UTF8)
            Return sr.ReadToEnd()
        End Using
    End Function

    Private Function ToInt(o As Object, Optional def As Integer = 0) As Integer
        If o Is Nothing Then Return def
        Dim s = Convert.ToString(o).Trim()
        Dim n As Integer
        If Integer.TryParse(s, NumberStyles.Integer, inv, n) Then Return n
        Return def
    End Function

    Private Function ToDec(o As Object, Optional def As Decimal = 0D) As Decimal
        If o Is Nothing Then Return def
        Dim s = Convert.ToString(o).Trim().Replace(",", ".")
        Dim n As Decimal
        If Decimal.TryParse(s, NumberStyles.Number Or NumberStyles.AllowDecimalPoint, inv, n) Then Return n
        Return def
    End Function

    Private Function ToBit(o As Object, Optional def As Integer = 0) As Integer
        If o Is Nothing Then Return def
        Dim s = Convert.ToString(o).Trim().ToLower()
        If s = "true" OrElse s = "1" OrElse s = "activo" Then Return 1
        If s = "false" OrElse s = "0" OrElse s = "inactivo" Then Return 0
        Dim n As Integer
        If Integer.TryParse(s, n) Then Return If(n <> 0, 1, 0)
        Return def
    End Function

    '============= Ops =============
    Private Sub Consultar(ctx As HttpContext)
        Dim lista As New List(Of Dictionary(Of String, Object))()

        Dim sql As String = "
SELECT
    c.id                                   AS id,
    c.nombre_credito                       AS descripcion,
    p.impacto                              AS impacto,
    p.probabilidad                         AS probabilidad,
    p.nivel_riesgo_pld                     AS nivel_riesgo_pld,
    ISNULL(p.estatus,0)                    AS estatus,
    CASE WHEN ISNULL(p.estatus,0)=1 THEN 'Activo' ELSE 'Inactivo' END AS estatus_texto,
    ISNULL(p.mitigantes,0)                 AS mitigantes,
    tec.descripcion                        AS tipo_estado_cuenta,
    p.tipo_estado_cuenta_id                AS tipo_estado_cuenta_id
FROM dbo.catalogo_creditos c
LEFT JOIN dbo.catalogo_creditos_pld p ON p.credito_id = c.id
LEFT JOIN dbo.catalogo_tipo_estado_cuenta tec ON tec.id = p.tipo_estado_cuenta_id
ORDER BY c.id;"

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using cmd As New SqlCommand(sql, cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        Dim r As New Dictionary(Of String, Object) From {
                            {"id", rd("id")},
                            {"descripcion", rd("descripcion")},
                            {"impacto", If(IsDBNull(rd("impacto")), Nothing, rd("impacto"))},
                            {"probabilidad", If(IsDBNull(rd("probabilidad")), Nothing, rd("probabilidad"))},
                            {"nivel_riesgo_pld", If(IsDBNull(rd("nivel_riesgo_pld")), Nothing, rd("nivel_riesgo_pld"))},
                            {"estatus", If(IsDBNull(rd("estatus")), 0, If(Convert.ToInt32(rd("estatus")) = 1, 1, 0))},
                            {"estatus_texto", rd("estatus_texto")},
                            {"mitigantes", If(IsDBNull(rd("mitigantes")), 0, rd("mitigantes"))},
                            {"tipo_estado_cuenta", If(IsDBNull(rd("tipo_estado_cuenta")), Nothing, rd("tipo_estado_cuenta"))},
                            {"tipo_estado_cuenta_id", If(IsDBNull(rd("tipo_estado_cuenta_id")), Nothing, rd("tipo_estado_cuenta_id"))}
                        }
                        lista.Add(r)
                    End While
                End Using
            End Using
        End Using

        Write(ctx, lista)
    End Sub

    Private Sub Obtener(ctx As HttpContext)
        Dim id As Integer = ToInt(ctx.Request("id"))
        If id <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "ID inválido."})
            Return
        End If

        Dim sql As String = "
SELECT TOP 1
    c.id                                   AS id,
    c.nombre_credito                       AS descripcion,
    ISNULL(p.impacto,0)                    AS impacto,
    ISNULL(p.probabilidad,0)               AS probabilidad,
    ISNULL(p.nivel_riesgo_pld,0)           AS nivel_riesgo_pld,
    ISNULL(p.mitigantes,0)                 AS mitigantes,
    ISNULL(p.estatus,0)                    AS estatus,
    p.tipo_estado_cuenta_id
FROM dbo.catalogo_creditos c
LEFT JOIN dbo.catalogo_creditos_pld p ON p.credito_id = c.id
WHERE c.id = @id;"

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        Dim r = New With {
                            .id = rd("id"),
                            .descripcion = rd("descripcion"),
                            .impacto = rd("impacto"),
                            .probabilidad = rd("probabilidad"),
                            .nivel_riesgo_pld = rd("nivel_riesgo_pld"),
                            .mitigantes = rd("mitigantes"),
                            .estatus = rd("estatus"),
                            .tipo_estado_cuenta_id = If(IsDBNull(rd("tipo_estado_cuenta_id")), Nothing, rd("tipo_estado_cuenta_id"))
                        }
                        Write(ctx, r)
                    Else
                        Write(ctx, New With {.ok = False, .mensaje = "No encontrado."})
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(ctx As HttpContext)
        Dim data = ser.Deserialize(Of Dictionary(Of String, Object))(ReadBody(ctx))

        Dim descripcion As String = (If(data.ContainsKey("descripcion"), Convert.ToString(data("descripcion")), "")).Trim()
        Dim impacto As Integer = ToInt(If(data.ContainsKey("impacto"), data("impacto"), 0))
        Dim probabilidad As Integer = ToInt(If(data.ContainsKey("probabilidad"), data("probabilidad"), 0))
        Dim riesgo As Decimal = ToDec(If(data.ContainsKey("nivel_riesgo_pld"), data("nivel_riesgo_pld"), impacto * probabilidad / 100D))
        Dim tipoId As Integer = ToInt(If(data.ContainsKey("tipo_estado_cuenta_id"), data("tipo_estado_cuenta_id"), 0))
        Dim mitigantes As Integer = ToInt(If(data.ContainsKey("mitigantes"), data("mitigantes"), 0))
        Dim estatus As Integer = ToBit(If(data.ContainsKey("estatus"), data("estatus"), 1))

        If String.IsNullOrWhiteSpace(descripcion) Then
            Write(ctx, New With {.ok = False, .mensaje = "Falta descripción."})
            Return
        End If
        If tipoId <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "Selecciona Tipo de estado de cuenta."})
            Return
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim tx = cn.BeginTransaction()
            Try
                Dim idNew As Integer
                Using cmd As New SqlCommand("
INSERT INTO dbo.catalogo_creditos(nombre_credito, activo, fecha_creacion)
VALUES (@desc, 1, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx)
                    cmd.Parameters.AddWithValue("@desc", descripcion)
                    idNew = Convert.ToInt32(cmd.ExecuteScalar())
                End Using

                Using cmd As New SqlCommand("
INSERT INTO dbo.catalogo_creditos_pld(credito_id, impacto, probabilidad, nivel_riesgo_pld, tipo_estado_cuenta_id, mitigantes, estatus, fecha_creacion)
VALUES (@id, @imp, @prob, @riesgo, @tipo, @mit, @est, GETDATE());", cn, tx)
                    cmd.Parameters.AddWithValue("@id", idNew)
                    cmd.Parameters.AddWithValue("@imp", impacto)
                    cmd.Parameters.AddWithValue("@prob", probabilidad)
                    cmd.Parameters.AddWithValue("@riesgo", riesgo)
                    cmd.Parameters.AddWithValue("@tipo", tipoId)
                    cmd.Parameters.AddWithValue("@mit", mitigantes)
                    cmd.Parameters.AddWithValue("@est", estatus)
                    cmd.ExecuteNonQuery()
                End Using

                tx.Commit()
                Write(ctx, New With {.ok = True, .id = idNew, .mensaje = "Guardado."})
            Catch ex As Exception
                Try : tx.Rollback() : Catch : End Try
                Write(ctx, New With {.ok = False, .mensaje = ex.Message})
            End Try
        End Using
    End Sub

    Private Sub Actualizar(ctx As HttpContext)
        Dim data = ser.Deserialize(Of Dictionary(Of String, Object))(ReadBody(ctx))

        Dim id As Integer = ToInt(If(data.ContainsKey("id"), data("id"), 0))
        Dim descripcion As String = (If(data.ContainsKey("descripcion"), Convert.ToString(data("descripcion")), "")).Trim()
        Dim impacto As Integer = ToInt(If(data.ContainsKey("impacto"), data("impacto"), 0))
        Dim probabilidad As Integer = ToInt(If(data.ContainsKey("probabilidad"), data("probabilidad"), 0))
        Dim riesgo As Decimal = ToDec(If(data.ContainsKey("nivel_riesgo_pld"), data("nivel_riesgo_pld"), impacto * probabilidad / 100D))
        Dim tipoId As Integer = ToInt(If(data.ContainsKey("tipo_estado_cuenta_id"), data("tipo_estado_cuenta_id"), 0))
        Dim mitigantes As Integer = ToInt(If(data.ContainsKey("mitigantes"), data("mitigantes"), 0))
        Dim estatus As Integer = ToBit(If(data.ContainsKey("estatus"), data("estatus"), 1))

        If id <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "ID inválido."})
            Return
        End If
        If String.IsNullOrWhiteSpace(descripcion) Then
            Write(ctx, New With {.ok = False, .mensaje = "Falta descripción."})
            Return
        End If
        If tipoId <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "Selecciona Tipo de estado de cuenta."})
            Return
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim tx = cn.BeginTransaction()
            Try
                Using cmd As New SqlCommand("UPDATE dbo.catalogo_creditos SET nombre_credito=@desc WHERE id=@id;", cn, tx)
                    cmd.Parameters.AddWithValue("@desc", descripcion)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New SqlCommand("
MERGE dbo.catalogo_creditos_pld AS tgt
USING (SELECT @id AS credito_id) AS s
   ON s.credito_id = tgt.credito_id
WHEN MATCHED THEN
    UPDATE SET impacto=@imp, probabilidad=@prob, nivel_riesgo_pld=@riesgo,
               tipo_estado_cuenta_id=@tipo, mitigantes=@mit, estatus=@est, fecha_actualizacion=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (credito_id, impacto, probabilidad, nivel_riesgo_pld, tipo_estado_cuenta_id, mitigantes, estatus, fecha_creacion)
    VALUES (@id, @imp, @prob, @riesgo, @tipo, @mit, @est, GETDATE());", cn, tx)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.Parameters.AddWithValue("@imp", impacto)
                    cmd.Parameters.AddWithValue("@prob", probabilidad)
                    cmd.Parameters.AddWithValue("@riesgo", riesgo)
                    cmd.Parameters.AddWithValue("@tipo", tipoId)
                    cmd.Parameters.AddWithValue("@mit", mitigantes)
                    cmd.Parameters.AddWithValue("@est", estatus)
                    cmd.ExecuteNonQuery()
                End Using

                tx.Commit()
                Write(ctx, New With {.ok = True, .id = id, .mensaje = "Actualizado."})
            Catch ex As Exception
                Try : tx.Rollback() : Catch : End Try
                Write(ctx, New With {.ok = False, .mensaje = ex.Message})
            End Try
        End Using
    End Sub

    Private Sub CambiarEstatus(ctx As HttpContext, nuevo As Integer)
        Dim data = ser.Deserialize(Of Dictionary(Of String, Object))(ReadBody(ctx))
        Dim id As Integer = ToInt(If(data.ContainsKey("id"), data("id"), 0))
        If id <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "ID inválido."})
            Return
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using cmd As New SqlCommand("
UPDATE dbo.catalogo_creditos_pld
SET estatus = @nuevo, fecha_actualizacion = GETDATE()
WHERE credito_id = @id;", cn)
                cmd.Parameters.AddWithValue("@nuevo", If(nuevo <> 0, 1, 0))
                cmd.Parameters.AddWithValue("@id", id)
                Dim rows = cmd.ExecuteNonQuery()
                If rows = 0 Then
                    Write(ctx, New With {.ok = False, .mensaje = "No existe configuración PLD para el crédito indicado."})
                Else
                    Write(ctx, New With {.ok = True, .mensaje = If(nuevo = 1, "Activado.", "Desactivado.")})
                End If
            End Using
        End Using
    End Sub

    Private Sub ToggleEstatus(ctx As HttpContext)
        Dim data = ser.Deserialize(Of Dictionary(Of String, Object))(ReadBody(ctx))
        Dim id As Integer = ToInt(If(data.ContainsKey("id"), data("id"), 0))
        If id <= 0 Then
            Write(ctx, New With {.ok = False, .mensaje = "ID inválido."})
            Return
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using cmd As New SqlCommand("
UPDATE p SET estatus = CASE WHEN ISNULL(estatus,0)=1 THEN 0 ELSE 1 END,
           fecha_actualizacion = GETDATE()
FROM dbo.catalogo_creditos_pld p
WHERE p.credito_id = @id;

SELECT ISNULL(estatus,0) AS estatus
FROM dbo.catalogo_creditos_pld
WHERE credito_id = @id;", cn)
                cmd.Parameters.AddWithValue("@id", id)
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        Dim nuevo = Convert.ToInt32(rd("estatus"))
                        Write(ctx, New With {.ok = True, .estatus = nuevo, .mensaje = If(nuevo = 1, "Activado.", "Desactivado.")})
                    Else
                        Write(ctx, New With {.ok = False, .mensaje = "No existe configuración PLD para el crédito indicado."})
                    End If
                End Using
            End Using
        End Using
    End Sub
End Class