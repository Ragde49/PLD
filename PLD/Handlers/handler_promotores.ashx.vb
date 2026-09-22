Imports System.Web
Imports System.Web.Script.Serialization
Imports System.Data.SqlClient

Public Class handler_promotores : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "application/json"
        Dim json As New JavaScriptSerializer()
        Dim action As String = context.Request("action")

        Try
            Select Case action

                Case "consultar"
                    context.Response.Write(json.Serialize(Consultar()))

                Case "insertar"
                    Dim p As New Promotor With {
                        .primer_nombre = context.Request("primer_nombre"),
                        .segundo_nombre = context.Request("segundo_nombre"),
                        .apellido_paterno = context.Request("apellido_paterno"),
                        .apellido_materno = context.Request("apellido_materno"),
                        .sucursal = context.Request("sucursal"),
                        .tipo_credito = context.Request("tipo_credito"),
                        .correo_electronico = context.Request("correo_electronico"),
                        .impacto = context.Request("impacto"),
                        .probabilidad = context.Request("probabilidad"),
                        .nivel_riesgo_pld = context.Request("nivel_riesgo_pld")
                    }
                    Insertar(p)
                    context.Response.Write("{""estatus"":""ok""}")

                Case "editar"
                    Dim p As New Promotor With {
                        .id = context.Request("id"),
                        .primer_nombre = context.Request("primer_nombre"),
                        .segundo_nombre = context.Request("segundo_nombre"),
                        .apellido_paterno = context.Request("apellido_paterno"),
                        .apellido_materno = context.Request("apellido_materno"),
                        .sucursal = context.Request("sucursal"),
                        .tipo_credito = context.Request("tipo_credito"),
                        .correo_electronico = context.Request("correo_electronico"),
                        .impacto = context.Request("impacto"),
                        .probabilidad = context.Request("probabilidad"),
                        .nivel_riesgo_pld = context.Request("nivel_riesgo_pld")
                    }
                    Editar(p)
                    context.Response.Write("{""estatus"":""ok""}")

                Case "cambiar_estatus"
                    Dim id As Integer = context.Request("id")
                    Dim activo As Boolean = context.Request("activo")
                    CambiarEstatus(id, activo)
                    context.Response.Write("{""estatus"":""ok""}")

            End Select

        Catch ex As Exception
            context.Response.Write("{""error"":""" & ex.Message & """}")
        End Try
    End Sub

    Public Function Consultar() As List(Of Promotor)
        Dim lista As New List(Of Promotor)
        Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            conn.Open()
            Dim cmd As New SqlCommand("SELECT * FROM catalogo_promotores ORDER BY id DESC", conn)
            Dim rdr As SqlDataReader = cmd.ExecuteReader()
            While rdr.Read()
                lista.Add(New Promotor With {
                    .id = rdr("id"),
                    .primer_nombre = rdr("primer_nombre"),
                    .segundo_nombre = rdr("segundo_nombre"),
                    .apellido_paterno = rdr("apellido_paterno"),
                    .apellido_materno = rdr("apellido_materno"),
                    .sucursal = rdr("sucursal"),
                    .tipo_credito = rdr("tipo_credito"),
                    .correo_electronico = rdr("correo_electronico"),
                    .impacto = rdr("impacto"),
                    .probabilidad = rdr("probabilidad"),
                    .nivel_riesgo_pld = If(IsDBNull(rdr("nivel_riesgo_pld")), Nothing, rdr("nivel_riesgo_pld")),
                    .activo = rdr("activo")
                })
            End While
        End Using
        Return lista
    End Function

    Public Sub Insertar(p As Promotor)
        Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            conn.Open()
            Dim query As String = "INSERT INTO catalogo_promotores (primer_nombre, segundo_nombre, apellido_paterno, apellido_materno, sucursal, tipo_credito, correo_electronico, impacto, probabilidad, nivel_riesgo_pld, activo, fecha_creacion) VALUES (@primer_nombre, @segundo_nombre, @apellido_paterno, @apellido_materno, @sucursal, @tipo_credito, @correo_electronico, @impacto, @probabilidad, @nivel_riesgo_pld, 1, GETDATE())"
            Dim cmd As New SqlCommand(query, conn)
            With cmd.Parameters
                .AddWithValue("@primer_nombre", p.primer_nombre)
                .AddWithValue("@segundo_nombre", p.segundo_nombre)
                .AddWithValue("@apellido_paterno", p.apellido_paterno)
                .AddWithValue("@apellido_materno", p.apellido_materno)
                .AddWithValue("@sucursal", p.sucursal)
                .AddWithValue("@tipo_credito", p.tipo_credito)
                .AddWithValue("@correo_electronico", p.correo_electronico)
                .AddWithValue("@impacto", p.impacto)
                .AddWithValue("@probabilidad", p.probabilidad)
                .AddWithValue("@nivel_riesgo_pld", If(String.IsNullOrEmpty(p.nivel_riesgo_pld), DBNull.Value, p.nivel_riesgo_pld))
            End With
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub Editar(p As Promotor)
        Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            conn.Open()
            Dim query As String = "UPDATE catalogo_promotores SET primer_nombre=@primer_nombre, segundo_nombre=@segundo_nombre, apellido_paterno=@apellido_paterno, apellido_materno=@apellido_materno, sucursal=@sucursal, tipo_credito=@tipo_credito, correo_electronico=@correo_electronico, impacto=@impacto, probabilidad=@probabilidad, nivel_riesgo_pld=@nivel_riesgo_pld WHERE id=@id"
            Dim cmd As New SqlCommand(query, conn)
            With cmd.Parameters
                .AddWithValue("@id", p.id)
                .AddWithValue("@primer_nombre", p.primer_nombre)
                .AddWithValue("@segundo_nombre", p.segundo_nombre)
                .AddWithValue("@apellido_paterno", p.apellido_paterno)
                .AddWithValue("@apellido_materno", p.apellido_materno)
                .AddWithValue("@sucursal", p.sucursal)
                .AddWithValue("@tipo_credito", p.tipo_credito)
                .AddWithValue("@correo_electronico", p.correo_electronico)
                .AddWithValue("@impacto", p.impacto)
                .AddWithValue("@probabilidad", p.probabilidad)
                .AddWithValue("@nivel_riesgo_pld", If(String.IsNullOrEmpty(p.nivel_riesgo_pld), DBNull.Value, p.nivel_riesgo_pld))
            End With
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub CambiarEstatus(id As Integer, activo As Boolean)
        Using conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("PLDConnection").ConnectionString)
            conn.Open()
            Dim cmd As New SqlCommand("UPDATE catalogo_promotores SET activo = @activo WHERE id = @id", conn)
            cmd.Parameters.AddWithValue("@id", id)
            cmd.Parameters.AddWithValue("@activo", activo)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Class Promotor
        Public Property id As Integer
        Public Property primer_nombre As String
        Public Property segundo_nombre As String
        Public Property apellido_paterno As String
        Public Property apellido_materno As String
        Public Property sucursal As String
        Public Property tipo_credito As String
        Public Property correo_electronico As String
        Public Property impacto As String
        Public Property probabilidad As String
        Public Property nivel_riesgo_pld As String
        Public Property activo As Boolean
    End Class
End Class
