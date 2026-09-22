Imports System
Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text
Imports System.IO
Imports System.IO.Compression
Imports System.Xml
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions

Public Class handler_listas_pld
    Implements IHttpHandler

    Private ReadOnly serializer As New JavaScriptSerializer()

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        context.Response.ContentEncoding = Encoding.UTF8

        Dim respuesta As New Dictionary(Of String, Object)()

        Try
            Dim action As String = Param(context, "action").Trim().ToLowerInvariant()

            Select Case action
                Case "catalogos"
                    respuesta = Catalogos()
                Case "cargas"
                    respuesta = Cargas(context)
                Case "importar"
                    respuesta = Importar(context)
                Case "activar"
                    respuesta = Activar(context)
                Case "buscar"
                    respuesta = Buscar(context)
                Case Else
                    respuesta("ok") = False
                    respuesta("mensaje") = "Acción no válida."
            End Select
        Catch ex As Exception
            respuesta("ok") = False
            respuesta("mensaje") = "Error en handler_listas_pld."
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

    Private Function ToDateNullable(ByVal valor As String) As DateTime?
        If String.IsNullOrWhiteSpace(valor) Then Return Nothing
        Dim d As DateTime
        If DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d.Date
        If DateTime.TryParse(valor, CultureInfo.CurrentCulture, DateTimeStyles.None, d) Then Return d.Date
        Return Nothing
    End Function

    Private Function TablaALista(ByVal dt As DataTable) As List(Of Dictionary(Of String, Object))
        Dim lista As New List(Of Dictionary(Of String, Object))()
        For Each row As DataRow In dt.Rows
            Dim item As New Dictionary(Of String, Object)()
            For Each col As DataColumn In dt.Columns
                item(col.ColumnName) = If(row(col) Is DBNull.Value, Nothing, row(col))
            Next
            lista.Add(item)
        Next
        Return lista
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

    Private Function Sha256Hex(ByVal bytes As Byte()) As String
        Using sha As SHA256 = SHA256.Create()
            Dim hash As Byte() = sha.ComputeHash(bytes)
            Dim sb As New StringBuilder()
            For Each b As Byte In hash
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture))
            Next
            Return sb.ToString()
        End Using
    End Function

    Private Function Catalogos() As Dictionary(Of String, Object)
        Dim r As New Dictionary(Of String, Object)()
        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim dt = Query(cn, Nothing,
                "SELECT id,clave,nombre,descripcion FROM dbo.catalogo_listas_pld WHERE activo=1 ORDER BY nombre;", Nothing)
            r("ok") = True
            r("data") = TablaALista(dt)
        End Using
        Return r
    End Function

    Private Function Cargas(ByVal ctx As HttpContext) As Dictionary(Of String, Object)
        Dim r As New Dictionary(Of String, Object)()
        Dim listaId As Integer = ToInt(Param(ctx, "lista_id"), 0)
        Dim ps As New List(Of SqlParameter)()
        Dim whereSql As String = "WHERE c.activo=1 "
        If listaId > 0 Then
            whereSql &= "AND c.lista_id=@lista_id "
            ps.Add(New SqlParameter("@lista_id", SqlDbType.Int) With {.Value = listaId})
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Dim dt = Query(cn, Nothing,
                "SELECT TOP 200 c.id,c.lista_id,l.clave,l.nombre AS lista,c.nombre_archivo,c.extension,c.referencia_fuente," &
                "c.fecha_recepcion,c.total_registros,c.vigente,c.creado_por,c.fecha_creacion,c.activado_por,c.fecha_activacion " &
                "FROM dbo.listas_pld_cargas c INNER JOIN dbo.catalogo_listas_pld l ON l.id=c.lista_id " &
                whereSql & "ORDER BY c.fecha_creacion DESC,c.id DESC;", ps)
            r("ok") = True
            r("data") = TablaALista(dt)
        End Using
        Return r
    End Function

    Private Function Importar(ByVal ctx As HttpContext) As Dictionary(Of String, Object)
        Dim r As New Dictionary(Of String, Object)()
        Dim listaId As Integer = ToInt(Param(ctx, "lista_id"), 0)
        Dim referencia As String = Param(ctx, "referencia_fuente").Trim()
        Dim fechaRecepcion As DateTime? = ToDateNullable(Param(ctx, "fecha_recepcion"))

        If listaId <= 0 Then
            r("ok") = False : r("mensaje") = "Selecciona la lista." : Return r
        End If
        If ctx.Request.Files.Count = 0 Then
            r("ok") = False : r("mensaje") = "Selecciona un archivo XLSX o CSV." : Return r
        End If

        Dim archivo As HttpPostedFile = ctx.Request.Files(0)
        Dim ext As String = Path.GetExtension(archivo.FileName).ToLowerInvariant()
        If ext <> ".xlsx" AndAlso ext <> ".csv" Then
            r("ok") = False : r("mensaje") = "Formato no soportado. Usa .xlsx o .csv." : Return r
        End If

        Dim bytes As Byte()
        Using ms As New MemoryStream()
            archivo.InputStream.CopyTo(ms)
            bytes = ms.ToArray()
        End Using
        If bytes.Length = 0 Then
            r("ok") = False : r("mensaje") = "El archivo está vacío." : Return r
        End If
        If bytes.Length > 15 * 1024 * 1024 Then
            r("ok") = False : r("mensaje") = "El archivo excede 15 MB." : Return r
        End If

        Dim filas As List(Of Dictionary(Of String, String))
        If ext = ".xlsx" Then
            filas = LeerXlsx(bytes)
        Else
            filas = LeerCsv(bytes)
        End If

        If filas.Count = 0 Then
            r("ok") = False : r("mensaje") = "No se encontraron registros válidos. Debe existir una columna NAME o NOMBRE." : Return r
        End If

        Dim hash As String = Sha256Hex(bytes)

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr = cn.BeginTransaction()
                Try
                    Using dup As New SqlCommand("SELECT TOP 1 id FROM dbo.listas_pld_cargas WHERE lista_id=@lista AND hash_sha256=@hash AND activo=1;", cn, tr)
                        dup.Parameters.Add("@lista", SqlDbType.Int).Value = listaId
                        dup.Parameters.Add("@hash", SqlDbType.VarChar, 64).Value = hash
                        Dim existente As Object = dup.ExecuteScalar()
                        If existente IsNot Nothing AndAlso existente IsNot DBNull.Value Then
                            tr.Rollback()
                            r("ok") = False
                            r("mensaje") = "Este archivo ya fue cargado para la lista seleccionada."
                            r("carga_id") = Convert.ToInt64(existente)
                            Return r
                        End If
                    End Using

                    Dim cargaId As Long
                    Using cmd As New SqlCommand(
                        "INSERT INTO dbo.listas_pld_cargas(lista_id,nombre_archivo,extension,referencia_fuente,fecha_recepcion,hash_sha256,total_registros,vigente,activo,creado_por) " &
                        "VALUES(@lista,@archivo,@ext,@ref,@fecha,@hash,@total,0,1,@usuario); SELECT CAST(SCOPE_IDENTITY() AS bigint);", cn, tr)
                        cmd.Parameters.Add("@lista", SqlDbType.Int).Value = listaId
                        cmd.Parameters.Add("@archivo", SqlDbType.NVarChar, 260).Value = Path.GetFileName(archivo.FileName)
                        cmd.Parameters.Add("@ext", SqlDbType.VarChar, 10).Value = ext
                        cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 250).Value = If(referencia = "", CType(DBNull.Value, Object), referencia)
                        cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = If(fechaRecepcion.HasValue, CType(fechaRecepcion.Value.Date, Object), DBNull.Value)
                        cmd.Parameters.Add("@hash", SqlDbType.VarChar, 64).Value = hash
                        cmd.Parameters.Add("@total", SqlDbType.Int).Value = filas.Count
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(ctx)
                        cargaId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    Dim insertSql As String =
                        "INSERT INTO dbo.listas_pld_personas(carga_id,nombre,nombre_normalizado,rfc,rfc_normalizado,curp,curp_normalizado,fila_origen,activo) " &
                        "VALUES(@carga,@nombre,@nn,@rfc,@rn,@curp,@cn,@fila,1);"

                    For Each f In filas
                        Using cmd As New SqlCommand(insertSql, cn, tr)
                            Dim nombre As String = f("nombre").Trim()
                            Dim rfc As String = If(f.ContainsKey("rfc"), f("rfc").Trim(), "")
                            Dim curp As String = If(f.ContainsKey("curp"), f("curp").Trim(), "")
                            cmd.Parameters.Add("@carga", SqlDbType.BigInt).Value = cargaId
                            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 250).Value = nombre
                            cmd.Parameters.Add("@nn", SqlDbType.NVarChar, 250).Value = NormalizarNombre(nombre)
                            cmd.Parameters.Add("@rfc", SqlDbType.VarChar, 20).Value = If(rfc = "", CType(DBNull.Value, Object), rfc.ToUpperInvariant())
                            cmd.Parameters.Add("@rn", SqlDbType.VarChar, 20).Value = If(rfc = "", CType(DBNull.Value, Object), NormalizarClave(rfc))
                            cmd.Parameters.Add("@curp", SqlDbType.VarChar, 30).Value = If(curp = "", CType(DBNull.Value, Object), curp.ToUpperInvariant())
                            cmd.Parameters.Add("@cn", SqlDbType.VarChar, 30).Value = If(curp = "", CType(DBNull.Value, Object), NormalizarClave(curp))
                            cmd.Parameters.Add("@fila", SqlDbType.Int).Value = ToInt(f("fila"), 0)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    tr.Commit()
                    r("ok") = True
                    r("mensaje") = "Archivo cargado correctamente. La versión aún no está vigente."
                    r("carga_id") = cargaId
                    r("total_registros") = filas.Count
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using

        Return r
    End Function

    Private Function Activar(ByVal ctx As HttpContext) As Dictionary(Of String, Object)
        Dim r As New Dictionary(Of String, Object)()
        Dim cargaId As Long
        If Not Long.TryParse(Param(ctx, "carga_id"), cargaId) OrElse cargaId <= 0 Then
            r("ok") = False : r("mensaje") = "Carga inválida." : Return r
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Dim listaId As Integer = 0
                    Using cmd As New SqlCommand("SELECT lista_id FROM dbo.listas_pld_cargas WITH(UPDLOCK,HOLDLOCK) WHERE id=@id AND activo=1;", cn, tr)
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = cargaId
                        Dim o = cmd.ExecuteScalar()
                        If o Is Nothing OrElse o Is DBNull.Value Then
                            tr.Rollback()
                            r("ok") = False : r("mensaje") = "No se encontró la carga." : Return r
                        End If
                        listaId = Convert.ToInt32(o)
                    End Using

                    Using cmd As New SqlCommand(
                        "UPDATE dbo.listas_pld_cargas SET vigente=0 WHERE lista_id=@lista AND activo=1;" &
                        "UPDATE dbo.listas_pld_cargas SET vigente=1,activado_por=@usuario,fecha_activacion=SYSDATETIME() WHERE id=@id;", cn, tr)
                        cmd.Parameters.Add("@lista", SqlDbType.Int).Value = listaId
                        cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = cargaId
                        cmd.Parameters.Add("@usuario", SqlDbType.NVarChar, 100).Value = Usuario(ctx)
                        cmd.ExecuteNonQuery()
                    End Using

                    tr.Commit()
                    r("ok") = True
                    r("mensaje") = "Versión marcada como vigente."
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using
        Return r
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
            r("ok") = False : r("mensaje") = "Captura nombre, RFC o CURP para consultar." : Return r
        End If

        Using cn As New SqlConnection(CnStr())
            cn.Open()
            Using tr = cn.BeginTransaction()
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

                    Dim dt = Query(cn, tr,
                        "SELECT p.id AS persona_id,p.nombre,p.nombre_normalizado,p.rfc,p.rfc_normalizado,p.curp,p.curp_normalizado," &
                        "l.id AS lista_id,l.clave AS lista_clave,l.nombre AS lista,c.id AS carga_id,c.nombre_archivo,c.referencia_fuente,c.fecha_recepcion,c.fecha_activacion " &
                        "FROM dbo.listas_pld_personas p " &
                        "INNER JOIN dbo.listas_pld_cargas c ON c.id=p.carga_id AND c.activo=1 AND c.vigente=1 " &
                        "INNER JOIN dbo.catalogo_listas_pld l ON l.id=c.lista_id AND l.activo=1 " &
                        "WHERE p.activo=1 AND (" & String.Join(" OR ", cond.ToArray()) & ") " &
                        "ORDER BY l.nombre,p.nombre;", ps)

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
                            If col.ColumnName <> "nombre_normalizado" AndAlso col.ColumnName <> "rfc_normalizado" AndAlso col.ColumnName <> "curp_normalizado" Then
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
                    r("mensaje") = If(lista.Count = 0,
                        "Sin coincidencias exactas en las versiones vigentes.",
                        "Se encontraron coincidencias exactas. Requieren revisión; no implican identidad confirmada.")
                Catch
                    tr.Rollback()
                    Throw
                End Try
            End Using
        End Using
        Return r
    End Function

    Private Function LeerCsv(ByVal bytes As Byte()) As List(Of Dictionary(Of String, String))
        Dim filas As New List(Of Dictionary(Of String, String))()
        Dim texto As String
        Using sr As New StreamReader(New MemoryStream(bytes), Encoding.UTF8, True)
            texto = sr.ReadToEnd()
        End Using

        Dim lineas As String() = texto.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)
        If lineas.Length = 0 Then Return filas

        Dim sep As Char = DetectarSeparador(lineas(0))
        Dim encabezados = ParseCsvLine(lineas(0), sep)
        Dim mapa = MapaEncabezados(encabezados)
        If Not mapa.ContainsKey("nombre") Then Return filas

        For i As Integer = 1 To lineas.Length - 1
            If String.IsNullOrWhiteSpace(lineas(i)) Then Continue For
            Dim vals = ParseCsvLine(lineas(i), sep)
            Dim nombre As String = ValorCol(vals, mapa("nombre"))
            If String.IsNullOrWhiteSpace(nombre) Then Continue For

            Dim f As New Dictionary(Of String, String) From {
                {"nombre", nombre.Trim()},
                {"rfc", If(mapa.ContainsKey("rfc"), ValorCol(vals, mapa("rfc")), "")},
                {"curp", If(mapa.ContainsKey("curp"), ValorCol(vals, mapa("curp")), "")},
                {"fila", (i + 1).ToString(CultureInfo.InvariantCulture)}
            }
            filas.Add(f)
        Next
        Return filas
    End Function

    Private Function DetectarSeparador(ByVal header As String) As Char
        Dim candidatos As Char() = {","c, ";"c, ControlChars.Tab}
        Dim mejor As Char = ","c
        Dim max As Integer = -1
        For Each c As Char In candidatos
            Dim n As Integer = header.Count(Function(x) x = c)
            If n > max Then max = n : mejor = c
        Next
        Return mejor
    End Function

    Private Function ParseCsvLine(ByVal line As String, ByVal sep As Char) As List(Of String)
        Dim r As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim quoted As Boolean = False
        Dim i As Integer = 0
        While i < line.Length
            Dim ch As Char = line(i)
            If ch = """"c Then
                If quoted AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                    sb.Append(""""c) : i += 1
                Else
                    quoted = Not quoted
                End If
            ElseIf ch = sep AndAlso Not quoted Then
                r.Add(sb.ToString()) : sb.Clear()
            Else
                sb.Append(ch)
            End If
            i += 1
        End While
        r.Add(sb.ToString())
        Return r
    End Function

    Private Function LeerXlsx(ByVal bytes As Byte()) As List(Of Dictionary(Of String, String))
        Dim filas As New List(Of Dictionary(Of String, String))()
        Using ms As New MemoryStream(bytes)
            Using zip As New ZipArchive(ms, ZipArchiveMode.Read, True)
                Dim shared = LeerSharedStrings(zip)
                Dim sheetPath As String = PrimerWorksheetPath(zip)
                If sheetPath = "" Then Return filas
                Dim entry = zip.GetEntry(sheetPath)
                If entry Is Nothing Then Return filas

                Dim rows As New List(Of Dictionary(Of Integer, String))()
                Using st = entry.Open()
                    Dim doc As New XmlDocument()
                    doc.Load(st)
                    Dim ns As New XmlNamespaceManager(doc.NameTable)
                    ns.AddNamespace("m", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
                    For Each rowNode As XmlNode In doc.SelectNodes("//m:sheetData/m:row", ns)
                        Dim row As New Dictionary(Of Integer, String)()
                        For Each cell As XmlNode In rowNode.SelectNodes("m:c", ns)
                            Dim ref As String = If(cell.Attributes("r") Is Nothing, "", cell.Attributes("r").Value)
                            Dim col As Integer = ColumnaDesdeReferencia(ref)
                            Dim tipo As String = If(cell.Attributes("t") Is Nothing, "", cell.Attributes("t").Value)
                            Dim valor As String = ""
                            If tipo = "inlineStr" Then
                                Dim t = cell.SelectSingleNode("m:is/m:t", ns)
                                If t IsNot Nothing Then valor = t.InnerText
                            Else
                                Dim v = cell.SelectSingleNode("m:v", ns)
                                If v IsNot Nothing Then
                                    valor = v.InnerText
                                    If tipo = "s" Then
                                        Dim idx As Integer = ToInt(valor, -1)
                                        If idx >= 0 AndAlso idx < shared.Count Then valor = shared(idx)
                                    End If
                                End If
                            End If
                            If col >= 0 Then row(col) = valor
                        Next
                        rows.Add(row)
                    Next
                End Using

                If rows.Count = 0 Then Return filas
                Dim maxCol As Integer = If(rows(0).Count = 0, -1, rows(0).Keys.Max())
                If maxCol < 0 Then Return filas
                Dim headers As New List(Of String)()
                For i As Integer = 0 To maxCol
                    headers.Add(If(rows(0).ContainsKey(i), rows(0)(i), ""))
                Next
                Dim mapa = MapaEncabezados(headers)
                If Not mapa.ContainsKey("nombre") Then Return filas

                For i As Integer = 1 To rows.Count - 1
                    Dim row = rows(i)
                    Dim nombre As String = If(row.ContainsKey(mapa("nombre")), row(mapa("nombre")), "")
                    If String.IsNullOrWhiteSpace(nombre) Then Continue For
                    filas.Add(New Dictionary(Of String, String) From {
                        {"nombre", nombre.Trim()},
                        {"rfc", If(mapa.ContainsKey("rfc") AndAlso row.ContainsKey(mapa("rfc")), row(mapa("rfc")), "")},
                        {"curp", If(mapa.ContainsKey("curp") AndAlso row.ContainsKey(mapa("curp")), row(mapa("curp")), "")},
                        {"fila", (i + 1).ToString(CultureInfo.InvariantCulture)}
                    })
                Next
            End Using
        End Using
        Return filas
    End Function

    Private Function LeerSharedStrings(ByVal zip As ZipArchive) As List(Of String)
        Dim r As New List(Of String)()
        Dim e = zip.GetEntry("xl/sharedStrings.xml")
        If e Is Nothing Then Return r
        Using st = e.Open()
            Dim doc As New XmlDocument()
            doc.Load(st)
            Dim ns As New XmlNamespaceManager(doc.NameTable)
            ns.AddNamespace("m", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
            For Each si As XmlNode In doc.SelectNodes("//m:si", ns)
                Dim sb As New StringBuilder()
                For Each t As XmlNode In si.SelectNodes(".//m:t", ns)
                    sb.Append(t.InnerText)
                Next
                r.Add(sb.ToString())
            Next
        End Using
        Return r
    End Function

    Private Function PrimerWorksheetPath(ByVal zip As ZipArchive) As String
        Dim wb = zip.GetEntry("xl/workbook.xml")
        Dim rel = zip.GetEntry("xl/_rels/workbook.xml.rels")
        If wb Is Nothing OrElse rel Is Nothing Then
            If zip.GetEntry("xl/worksheets/sheet1.xml") IsNot Nothing Then Return "xl/worksheets/sheet1.xml"
            Return ""
        End If

        Dim rid As String = ""
        Using st = wb.Open()
            Dim doc As New XmlDocument()
            doc.Load(st)
            Dim ns As New XmlNamespaceManager(doc.NameTable)
            ns.AddNamespace("m", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
            ns.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
            Dim sh = doc.SelectSingleNode("//m:sheets/m:sheet[1]", ns)
            If sh IsNot Nothing AndAlso sh.Attributes("r:id") IsNot Nothing Then rid = sh.Attributes("r:id").Value
        End Using
        If rid = "" Then Return ""

        Using st = rel.Open()
            Dim doc As New XmlDocument()
            doc.Load(st)
            Dim ns As New XmlNamespaceManager(doc.NameTable)
            ns.AddNamespace("p", "http://schemas.openxmlformats.org/package/2006/relationships")
            Dim node = doc.SelectSingleNode("//p:Relationship[@Id='" & rid.Replace("'", "") & "']", ns)
            If node Is Nothing OrElse node.Attributes("Target") Is Nothing Then Return ""
            Dim target As String = node.Attributes("Target").Value.Replace("\", "/").TrimStart("/"c)
            If target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) Then Return target
            Return "xl/" & target
        End Using
    End Function

    Private Function ColumnaDesdeReferencia(ByVal referencia As String) As Integer
        If referencia = "" Then Return -1
        Dim n As Integer = 0
        For Each ch As Char In referencia
            If ch >= "A"c AndAlso ch <= "Z"c Then
                n = n * 26 + (AscW(ch) - AscW("A"c) + 1)
            Else
                Exit For
            End If
        Next
        Return n - 1
    End Function

    Private Function MapaEncabezados(ByVal headers As IList(Of String)) As Dictionary(Of String, Integer)
        Dim mapa As New Dictionary(Of String, Integer)()
        For i As Integer = 0 To headers.Count - 1
            Dim h As String = NormalizarNombre(headers(i)).Replace(" ", "")
            If (h = "NAME" OrElse h = "NOMBRE" OrElse h = "NOMBRECOMPLETO") AndAlso Not mapa.ContainsKey("nombre") Then mapa("nombre") = i
            If h = "RFC" AndAlso Not mapa.ContainsKey("rfc") Then mapa("rfc") = i
            If h = "CURP" AndAlso Not mapa.ContainsKey("curp") Then mapa("curp") = i
        Next
        Return mapa
    End Function

    Private Function ValorCol(ByVal vals As List(Of String), ByVal idx As Integer) As String
        If idx < 0 OrElse idx >= vals.Count Then Return ""
        Return vals(idx)
    End Function
End Class
