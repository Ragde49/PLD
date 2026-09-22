Imports System
Imports System.Security.Cryptography

Public NotInheritable Class SeguridadPassword
    Private Sub New()
    End Sub

    Private Const IteracionesDefault As Integer = 100000
    Private Const TamanoSalt As Integer = 16
    Private Const TamanoHash As Integer = 32

    Public Shared Function CrearHash(ByVal password As String) As String
        If String.IsNullOrEmpty(password) Then
            Throw New ArgumentException("La contrasena no puede estar vacia.")
        End If

        Dim salt(TamanoSalt - 1) As Byte
        Using rng = RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Using

        Dim hash As Byte()
        Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, IteracionesDefault)
            hash = pbkdf2.GetBytes(TamanoHash)
        End Using

        Return "PBKDF2$" & IteracionesDefault.ToString() & "$" &
            Convert.ToBase64String(salt) & "$" &
            Convert.ToBase64String(hash)
    End Function

    Public Shared Function Verificar(ByVal password As String, ByVal hashGuardado As String) As Boolean
        If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(hashGuardado) Then Return False

        Dim partes = hashGuardado.Split("$"c)
        If partes.Length <> 4 OrElse Not partes(0).Equals("PBKDF2", StringComparison.OrdinalIgnoreCase) Then Return False

        Dim iteraciones As Integer
        If Not Integer.TryParse(partes(1), iteraciones) OrElse iteraciones <= 0 Then Return False

        Try
            Dim salt = Convert.FromBase64String(partes(2))
            Dim esperado = Convert.FromBase64String(partes(3))
            Dim calculado As Byte()

            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iteraciones)
                calculado = pbkdf2.GetBytes(esperado.Length)
            End Using

            Return ComparacionConstante(esperado, calculado)
        Catch
            Return False
        End Try
    End Function

    Private Shared Function ComparacionConstante(ByVal a As Byte(), ByVal b As Byte()) As Boolean
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return False

        Dim diferencia As Integer = 0
        For i As Integer = 0 To a.Length - 1
            diferencia = diferencia Or (a(i) Xor b(i))
        Next

        Return diferencia = 0
    End Function
End Class
