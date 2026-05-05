Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clCilindro
    Public ReadOnly Property Visual As ModelVisual3D

    Private _mesh As MeshGeometry3D
    Private _transformacion As TranslateTransform3D
    Private _modelo As GeometryModel3D
    Private _escala As ScaleTransform3D

    Public Sub New(rExt As Double, rInt As Double, h As Double, col As Color, opacidad As Double)
        _mesh = New MeshGeometry3D()
        construirMalla(rExt, rInt, h)

        Dim grupo As New MaterialGroup()
        Dim brushBase As New SolidColorBrush(col)
        brushBase.Opacity = opacidad
        grupo.Children.Add(New DiffuseMaterial(brushBase))
        grupo.Children.Add(New SpecularMaterial(New SolidColorBrush(Colors.White), 50.0))

        _modelo = New GeometryModel3D(_mesh, grupo)
        _modelo.BackMaterial = grupo

        _transformacion = New TranslateTransform3D(0, 0, 0)
        _escala = New ScaleTransform3D(1, 1, 1)

        Dim tg As New Transform3DGroup()
        tg.Children.Add(_escala)
        tg.Children.Add(_transformacion)
        _modelo.Transform = tg
        Visual = New ModelVisual3D() With {.Content = _modelo}
    End Sub

    Private Sub construirMalla(rExt As Double, rInt As Double, h As Double)
        Dim segmentos As Integer = 40
        For i As Integer = 0 To segmentos - 1
            Dim angulo As Double = (i / segmentos) * 2 * Math.PI
            Dim xExt As Double = Math.Cos(angulo) * rExt
            Dim zExt As Double = Math.Sin(angulo) * rExt
            Dim xInt As Double = Math.Cos(angulo) * rInt
            Dim zInt As Double = Math.Sin(angulo) * rInt

            _mesh.Positions.Add(New Point3D(xExt, h, zExt))
            _mesh.Positions.Add(New Point3D(xExt, 0, zExt))
            _mesh.Positions.Add(New Point3D(xInt, h, zInt))
            _mesh.Positions.Add(New Point3D(xInt, 0, zInt))
            _mesh.Positions.Add(New Point3D(xExt, h, zExt))
            _mesh.Positions.Add(New Point3D(xExt, 0, zExt))
            _mesh.Positions.Add(New Point3D(xInt, h, zInt))
            _mesh.Positions.Add(New Point3D(xInt, 0, zInt))
        Next

        For i As Integer = 0 To segmentos - 1
            Dim a As Integer = i * 8
            Dim s As Integer = ((i + 1) Mod segmentos) * 8
            _mesh.TriangleIndices.Add(a) : _mesh.TriangleIndices.Add(a + 1) : _mesh.TriangleIndices.Add(s + 1)
            _mesh.TriangleIndices.Add(a) : _mesh.TriangleIndices.Add(s + 1) : _mesh.TriangleIndices.Add(s)
            _mesh.TriangleIndices.Add(a + 2) : _mesh.TriangleIndices.Add(s + 3) : _mesh.TriangleIndices.Add(a + 3)
            _mesh.TriangleIndices.Add(a + 2) : _mesh.TriangleIndices.Add(s + 2) : _mesh.TriangleIndices.Add(s + 3)
            _mesh.TriangleIndices.Add(a + 4) : _mesh.TriangleIndices.Add(s + 4) : _mesh.TriangleIndices.Add(a + 6)
            _mesh.TriangleIndices.Add(a + 6) : _mesh.TriangleIndices.Add(s + 4) : _mesh.TriangleIndices.Add(s + 6)
            _mesh.TriangleIndices.Add(a + 5) : _mesh.TriangleIndices.Add(a + 7) : _mesh.TriangleIndices.Add(s + 5)
            _mesh.TriangleIndices.Add(a + 7) : _mesh.TriangleIndices.Add(s + 7) : _mesh.TriangleIndices.Add(s + 5)
        Next
    End Sub

    Public Sub posicionar(x As Double, y As Double, z As Double)
        _transformacion.OffsetX = x
        _transformacion.OffsetY = y
        _transformacion.OffsetZ = z
    End Sub

    Public Sub escalar(factor As Double)
        _escala.ScaleX = factor
        _escala.ScaleY = factor
        _escala.ScaleZ = factor
    End Sub

    Public ReadOnly Property PosX As Double
        Get
            Return _transformacion.OffsetX
        End Get
    End Property
    Public ReadOnly Property PosY As Double
        Get
            Return _transformacion.OffsetY
        End Get
    End Property
    Public ReadOnly Property PosZ As Double
        Get
            Return _transformacion.OffsetZ
        End Get
    End Property
    Public ReadOnly Property EscalaActual As Double
        Get
            Return _escala.ScaleY
        End Get
    End Property
End Class