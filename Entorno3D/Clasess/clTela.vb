Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clTela
    Public ReadOnly Property Visual As ModelVisual3D

    Private _mesh As MeshGeometry3D
    Private _vertices As Point3D()
    Private _velocidadesY As Double()

    Public Lado As Double
    Private Resolucion As Integer = 30 ' Una cuadrícula de 30x30 puntos

    Public Sub New(yInicial As Double, col As Color)
        _mesh = New MeshGeometry3D()
        Lado = 4.0 ' 4.0 es mucho más grande que el tanque (que mide 2.0 de diámetro)

        Dim totalVertices As Integer = (Resolucion + 1) * (Resolucion + 1)
        _vertices = New Point3D(totalVertices - 1) {}
        _velocidadesY = New Double(totalVertices - 1) {}

        Dim paso As Double = Lado / Resolucion
        Dim offset As Double = Lado / 2.0

        ' 1. Crear los vértices de la cuadrícula
        Dim indice As Integer = 0
        For z As Integer = 0 To Resolucion
            For x As Integer = 0 To Resolucion
                Dim px As Double = (x * paso) - offset
                Dim pz As Double = (z * paso) - offset
                _vertices(indice) = New Point3D(px, yInicial, pz)
                _velocidadesY(indice) = 0
                _mesh.Positions.Add(_vertices(indice))
                indice += 1
            Next
        Next

        ' 2. Crear los triángulos para unir la cuadrícula
        For z As Integer = 0 To Resolucion - 1
            For x As Integer = 0 To Resolucion - 1
                Dim i1 As Integer = z * (Resolucion + 1) + x
                Dim i2 As Integer = i1 + 1
                Dim i3 As Integer = (z + 1) * (Resolucion + 1) + x
                Dim i4 As Integer = i3 + 1

                ' Triángulos cara superior
                _mesh.TriangleIndices.Add(i1) : _mesh.TriangleIndices.Add(i3) : _mesh.TriangleIndices.Add(i2)
                _mesh.TriangleIndices.Add(i2) : _mesh.TriangleIndices.Add(i3) : _mesh.TriangleIndices.Add(i4)

                ' Triángulos cara inferior (para que se vea por debajo)
                _mesh.TriangleIndices.Add(i1) : _mesh.TriangleIndices.Add(i2) : _mesh.TriangleIndices.Add(i3)
                _mesh.TriangleIndices.Add(i2) : _mesh.TriangleIndices.Add(i4) : _mesh.TriangleIndices.Add(i3)
            Next
        Next

        Dim material As New DiffuseMaterial(New SolidColorBrush(col))
        Dim modelo As New GeometryModel3D(_mesh, material)
        modelo.BackMaterial = material

        Visual = New ModelVisual3D() With {.Content = modelo}
    End Sub

    Public Sub DeformarYCaer(centroX As Double, centroZ As Double, radioInterno As Double, radioExterno As Double, alturaBorde As Double, alturaSueloInterno As Double, alturaSueloExterno As Double)
        Dim posicionesActualizadas As New Point3DCollection(_vertices.Length)

        For i As Integer = 0 To _vertices.Length - 1
            Dim p As Point3D = _vertices(i)

            _velocidadesY(i) -= 0.015
            p.Y += _velocidadesY(i)

            Dim dx As Double = p.X - centroX
            Dim dz As Double = p.Z - centroZ
            Dim distAlCentro As Double = Math.Sqrt(dx * dx + dz * dz)

            Dim limiteY As Double

            If distAlCentro < radioInterno Then
                Dim distParedInterna As Double = radioInterno - distAlCentro
                Dim curvaInterna As Double = Math.Pow(distParedInterna * 4.0, 2)
                limiteY = alturaBorde - curvaInterna

                If limiteY < alturaSueloInterno Then limiteY = alturaSueloInterno

            ElseIf distAlCentro >= radioInterno AndAlso distAlCentro <= radioExterno Then
                limiteY = alturaBorde

            Else
                Dim distanciaFuera As Double = distAlCentro - radioExterno
                Dim caidaCurva As Double = Math.Pow(distanciaFuera * 2.0, 2)
                limiteY = alturaBorde - caidaCurva

                If limiteY < alturaSueloExterno Then limiteY = alturaSueloExterno
            End If

            If p.Y <= limiteY Then
                p.Y = limiteY
                _velocidadesY(i) = 0
            End If

            _vertices(i) = p
            posicionesActualizadas.Add(p)
        Next

        _mesh.Positions = posicionesActualizadas
    End Sub
End Class