Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clSuperficieAgua
    Public ReadOnly Property Visual As ModelVisual3D

    Private _mesh As MeshGeometry3D
    Private _vertices As Point3D()
    Private Resolucion As Integer = 35
    Public RadioTanque As Double

    Public Sub New(radio As Double, col As Color)
        _mesh = New MeshGeometry3D()
        RadioTanque = radio

        Dim totalVertices As Integer = (Resolucion + 1) * (Resolucion + 1)
        _vertices = New Point3D(totalVertices - 1) {}

        Dim paso As Double = (RadioTanque * 2.2) / Resolucion
        Dim offset As Double = (RadioTanque * 2.2) / 2.0

        Dim indice As Integer = 0
        For z As Integer = 0 To Resolucion
            For x As Integer = 0 To Resolucion
                Dim px As Double = (x * paso) - offset
                Dim pz As Double = (z * paso) - offset
                _vertices(indice) = New Point3D(px, 0, pz)
                _mesh.Positions.Add(_vertices(indice))
                indice += 1
            Next
        Next

        For z As Integer = 0 To Resolucion - 1
            For x As Integer = 0 To Resolucion - 1
                Dim i1 As Integer = z * (Resolucion + 1) + x
                Dim i2 As Integer = i1 + 1
                Dim i3 As Integer = (z + 1) * (Resolucion + 1) + x
                Dim i4 As Integer = i3 + 1

                _mesh.TriangleIndices.Add(i1) : _mesh.TriangleIndices.Add(i3) : _mesh.TriangleIndices.Add(i2)
                _mesh.TriangleIndices.Add(i2) : _mesh.TriangleIndices.Add(i3) : _mesh.TriangleIndices.Add(i4)
            Next
        Next

        Dim brochaBase As New SolidColorBrush(col)
        brochaBase.Opacity = 0.95 ' Superficie opaca y pesada
        Dim grupoMateriales As New MaterialGroup()
        grupoMateriales.Children.Add(New DiffuseMaterial(brochaBase))
        grupoMateriales.Children.Add(New SpecularMaterial(New SolidColorBrush(Colors.White), 80.0))

        Dim modelo As New GeometryModel3D(_mesh, grupoMateriales)
        modelo.BackMaterial = grupoMateriales

        Visual = New ModelVisual3D() With {.Content = modelo}
    End Sub

    Public Sub GenerarSuperficie(centroX As Double, centroZ As Double, baseTanque As Double, gotas As List(Of clGota))
        Dim posicionesActualizadas As New Point3DCollection(_vertices.Length)
        Dim rEf As Double = 0.35 ' Radio amplio para suavidad extrema

        For i As Integer = 0 To _vertices.Length - 1
            Dim p As Point3D = _vertices(i)

            Dim pXLocal As Double = p.X
            Dim pZLocal As Double = p.Z
            Dim distCentroLocal As Double = Math.Sqrt(pXLocal * pXLocal + pZLocal * pZLocal)

            If distCentroLocal >= RadioTanque Then
                pXLocal = (p.X / distCentroLocal) * RadioTanque
                pZLocal = (p.Z / distCentroLocal) * RadioTanque
                distCentroLocal = RadioTanque
            End If

            Dim posXReal As Double = centroX + pXLocal
            Dim posZReal As Double = centroZ + pZLocal

            Dim alturaInfluencia As Double = 0
            Dim radioEfecto As Double = 0.2

            If distCentroLocal < RadioTanque - 0.02 Then
                For Each g In gotas
                    ' Ignorar las gotas en caída libre
                    If g.VelY < -0.05 Then Continue For

                    Dim dx As Double = g.PosX - posXReal
                    Dim dz As Double = g.PosZ - posZReal
                    Dim distCuadrada As Double = (dx * dx) + (dz * dz)

                    If distCuadrada < radioEfecto Then
                        Dim distReal As Double = Math.Sqrt(distCuadrada)
                        Dim radioReal As Double = Math.Sqrt(radioEfecto)

                        Dim influencia As Double = 1.0 - (distReal / radioReal)
                        influencia = Math.Pow(influencia, 1.5) ' Curva abombada para los montículos de chocolate

                        Dim alturaGota As Double = g.PosY - baseTanque + (g.Radio * 0.9)
                        Dim elevacion As Double = alturaGota * influencia

                        If elevacion > alturaInfluencia Then
                            alturaInfluencia = elevacion
                        Else
                            alturaInfluencia += elevacion * 0.4 ' Mayor acumulación de espesor
                        End If
                    End If
                Next
            End If

            Dim posYFinal As Double = baseTanque + alturaInfluencia
            posicionesActualizadas.Add(New Point3D(posXReal, posYFinal, posZReal))
        Next

        _mesh.Positions = posicionesActualizadas
    End Sub
End Class