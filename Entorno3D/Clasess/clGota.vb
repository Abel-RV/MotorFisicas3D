Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clGota
    Public ReadOnly Property Visual As ModelVisual3D

    Private _traslacion As TranslateTransform3D
    Private _escala As ScaleTransform3D

    Public PosX As Double
    Public PosY As Double
    Public PosZ As Double

    Public VelX As Double
    Public VelY As Double
    Public VelZ As Double
    Public Radio As Double

    Public Sub New(x As Double, y As Double, z As Double, col As Color)
        Dim mesh As New MeshGeometry3D()
        Radio = 0.08

        Dim paralelos As Integer = 10
        Dim meridianos As Integer = 10

        For i As Integer = 0 To paralelos
            Dim theta As Double = i * Math.PI / paralelos
            Dim sinTheta As Double = Math.Sin(theta)
            Dim cosTheta As Double = Math.Cos(theta)

            For j As Integer = 0 To meridianos
                Dim phi As Double = j * 2 * Math.PI / meridianos
                Dim px As Double = Radio * sinTheta * Math.Cos(phi)
                Dim py As Double = Radio * cosTheta
                Dim pz As Double = Radio * sinTheta * Math.Sin(phi)
                mesh.Positions.Add(New Point3D(px, py, pz))
            Next
        Next

        For i As Integer = 0 To paralelos - 1
            For j As Integer = 0 To meridianos - 1
                Dim a As Integer = i * (meridianos + 1) + j
                Dim b As Integer = a + meridianos + 1
                mesh.TriangleIndices.Add(a) : mesh.TriangleIndices.Add(b) : mesh.TriangleIndices.Add(a + 1)
                mesh.TriangleIndices.Add(b) : mesh.TriangleIndices.Add(b + 1) : mesh.TriangleIndices.Add(a + 1)
            Next
        Next

        Dim brochaBase As New SolidColorBrush(col)
        brochaBase.Opacity = 0.95 ' Un poco más opaco para el chocolate
        Dim grupoMateriales As New MaterialGroup()
        grupoMateriales.Children.Add(New DiffuseMaterial(brochaBase))
        grupoMateriales.Children.Add(New SpecularMaterial(New SolidColorBrush(Colors.White), 80.0)) ' Brillo más suave

        Dim modelo As New GeometryModel3D(mesh, grupoMateriales)

        _traslacion = New TranslateTransform3D(x, y, z)
        _escala = New ScaleTransform3D(1, 1, 1)

        Dim tg As New Transform3DGroup()
        tg.Children.Add(_escala)
        tg.Children.Add(_traslacion)

        modelo.Transform = tg
        Visual = New ModelVisual3D() With {.Content = modelo}

        PosX = x : PosY = y : PosZ = z
        VelX = 0 : VelY = -0.15 : VelZ = 0
    End Sub

    Public Sub ActualizarGrafico()
        _traslacion.OffsetX = PosX
        _traslacion.OffsetY = PosY
        _traslacion.OffsetZ = PosZ

        Dim v As Double = Math.Abs(VelY)

        If v < 0.025 Then
            ' Ocultar gota cuando se asienta en el chocolate
            _escala.ScaleX = 0 : _escala.ScaleY = 0 : _escala.ScaleZ = 0
        Else
            ' Chorro espeso controlado
            Dim f As Double = Math.Min(1.3, 1.0 + (v * 1.5))
            _escala.ScaleY = f
            _escala.ScaleX = 1.0 / Math.Sqrt(f)
            _escala.ScaleZ = _escala.ScaleX
        End If
    End Sub
End Class