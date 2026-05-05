Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Partial Public Class clMinero
    ' =========================================================================
    ' FUNCIONES AUXILIARES (Matemáticas y Geometría)
    ' =========================================================================

    Private Function ObtenerEstadoAnimacion(c As Double) As Double
        If c < 0.3 Then Return 0.5 - 0.5 * Math.Sin((c / 0.3) * Math.PI / 2)
        If c < 0.45 Then Return 0.0
        If c < 0.6 Then Return Math.Pow((c - 0.45) / 0.15, 2.5)
        If c < 0.7 Then Return 1.0
        Return 1.0 - 0.5 * Math.Sin(((c - 0.7) / 0.3) * Math.PI / 2)
    End Function

    Private Function Lerp(a As Double, b As Double, t As Double) As Double
        Return a + (b - a) * t
    End Function

    Private Sub GenerarEscombros()
        If listaEscombros.Count = 0 Then
            For i As Integer = 0 To NUM_ESCOMBROS - 1
                Dim debris As GeometryModel3D = CType(escombrosGroup.Children(i), GeometryModel3D)
                listaEscombros.Add(New Escombro With {
                    .VisualIdx = i,
                    .Position = posImpacto,
                    .Velocity = New Vector3D((rnd.NextDouble() - 0.5) * 0.08, 0.06 + rnd.NextDouble() * 0.1, (rnd.NextDouble() - 0.5) * 0.08),
                    .TTL = VID_ESCOMBRO
                })
            Next
        End If
    End Sub

    Private Function CrearModeloPicoGenerico() As Model3DGroup
        Dim group As New Model3DGroup()
        Dim mango As GeometryModel3D = CrearCilindro(0.015, 0.8, Colors.SaddleBrown, 16)
        Dim tgMango As New Transform3DGroup()
        tgMango.Children.Add(New RotateTransform3D(New AxisAngleRotation3D(New Vector3D(0, 0, 1), -90)))
        tgMango.Children.Add(New TranslateTransform3D(0.2, 0, 0))
        mango.Transform = tgMango
        group.Children.Add(mango)
        Dim centroPico As GeometryModel3D = CrearEsfera(0.03, Colors.LightGray, 16, 16)
        centroPico.Transform = New TranslateTransform3D(0.5, 0, 0)
        group.Children.Add(centroPico)
        Dim punta1 As GeometryModel3D = CrearCono(0.025, 0.3, Colors.LightGray)
        Dim tgPunta1 As New Transform3DGroup()
        tgPunta1.Children.Add(New RotateTransform3D(New AxisAngleRotation3D(New Vector3D(0, 0, 1), 180)))
        tgPunta1.Children.Add(New TranslateTransform3D(0.5, -0.15, 0))
        punta1.Transform = tgPunta1
        group.Children.Add(punta1)
        Dim punta2 As GeometryModel3D = CrearCono(0.025, 0.3, Colors.LightGray)
        punta2.Transform = New TranslateTransform3D(0.5, 0.15, 0)
        group.Children.Add(punta2)
        Return group
    End Function

    Private Function CrearHueso(radio As Double, longitud As Double, col As Color) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        mesh.Positions.Add(New Point3D(0, 0, 0))
        mesh.Positions.Add(New Point3D(0, -longitud, 0))
        Dim baseIdx = 2
        For i = 0 To 15
            Dim angulo = i * 2 * Math.PI / 16
            Dim cx = radio * Math.Cos(angulo)
            Dim cz = radio * Math.Sin(angulo)
            mesh.Positions.Add(New Point3D(cx, 0, cz))
            mesh.Positions.Add(New Point3D(cx, -longitud, cz))
        Next
        For i = 0 To 15
            Dim idx = baseIdx + i * 2
            Dim nextIdx = baseIdx + ((i + 1) Mod 16) * 2
            mesh.TriangleIndices.Add(idx) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx)
            mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx + 1) : mesh.TriangleIndices.Add(nextIdx)
            mesh.TriangleIndices.Add(0) : mesh.TriangleIndices.Add(nextIdx) : mesh.TriangleIndices.Add(idx)
            mesh.TriangleIndices.Add(1) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx + 1)
        Next
        Dim mat As New DiffuseMaterial(New SolidColorBrush(col))
        Return New GeometryModel3D(mesh, mat) With {.BackMaterial = mat}
    End Function

    Private Function CrearRoca(radioBase As Double, col As Color) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        Dim rndNoise As New Random()
        Dim paralelos = 8
        Dim meridianos = 12
        Dim radios(paralelos, meridianos) As Double
        For i = 0 To paralelos
            For j = 0 To meridianos
                radios(i, j) = radioBase * (0.6 + rndNoise.NextDouble() * 0.4)
            Next
        Next
        For i = 0 To paralelos
            Dim theta = i * Math.PI / paralelos
            Dim sinTheta = Math.Sin(theta)
            Dim cosTheta = Math.Cos(theta)
            For j = 0 To meridianos
                Dim phi = j * 2 * Math.PI / meridianos
                Dim r = radios(i, j Mod meridianos)
                mesh.Positions.Add(New Point3D(r * sinTheta * Math.Cos(phi), r * cosTheta, r * sinTheta * Math.Sin(phi)))
            Next
        Next
        For i = 0 To paralelos - 1
            For j = 0 To meridianos - 1
                Dim a = i * (meridianos + 1) + j
                Dim b = a + meridianos + 1
                mesh.TriangleIndices.Add(a) : mesh.TriangleIndices.Add(b) : mesh.TriangleIndices.Add(a + 1)
                mesh.TriangleIndices.Add(b) : mesh.TriangleIndices.Add(b + 1) : mesh.TriangleIndices.Add(a + 1)
            Next
        Next
        Dim mat As New DiffuseMaterial(New SolidColorBrush(col))
        Return New GeometryModel3D(mesh, mat) With {.BackMaterial = mat}
    End Function

    Private Function CrearEsfera(radio As Double, col As Color, latLines As Integer, longLines As Integer) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        For i = 0 To latLines
            Dim theta = i * Math.PI / latLines
            Dim sinTheta = Math.Sin(theta)
            Dim cosTheta = Math.Cos(theta)
            For j = 0 To longLines
                Dim phi = j * 2 * Math.PI / longLines
                mesh.Positions.Add(New Point3D(radio * sinTheta * Math.Cos(phi), radio * cosTheta, radio * sinTheta * Math.Sin(phi)))
            Next
        Next
        For i = 0 To latLines - 1
            For j = 0 To longLines - 1
                Dim p1 = i * (longLines + 1) + j
                Dim p2 = p1 + longLines + 1
                mesh.TriangleIndices.Add(p1) : mesh.TriangleIndices.Add(p2) : mesh.TriangleIndices.Add(p1 + 1)
                mesh.TriangleIndices.Add(p1 + 1) : mesh.TriangleIndices.Add(p2) : mesh.TriangleIndices.Add(p2 + 1)
            Next
        Next
        Dim mat As New DiffuseMaterial(New SolidColorBrush(col))
        Return New GeometryModel3D(mesh, mat) With {.BackMaterial = mat}
    End Function

    Private Function CrearCilindro(radio As Double, altura As Double, col As Color, segmentos As Integer) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        mesh.Positions.Add(New Point3D(0, altura / 2, 0))
        mesh.Positions.Add(New Point3D(0, -altura / 2, 0))
        Dim baseIdx = 2
        For i = 0 To segmentos - 1
            Dim angulo = i * 2 * Math.PI / segmentos
            Dim cx = radio * Math.Cos(angulo)
            Dim cz = radio * Math.Sin(angulo)
            mesh.Positions.Add(New Point3D(cx, altura / 2, cz))
            mesh.Positions.Add(New Point3D(cx, -altura / 2, cz))
        Next
        For i = 0 To segmentos - 1
            Dim idx = baseIdx + i * 2
            Dim nextIdx = baseIdx + ((i + 1) Mod segmentos) * 2
            mesh.TriangleIndices.Add(idx) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx)
            mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx + 1) : mesh.TriangleIndices.Add(nextIdx)
            mesh.TriangleIndices.Add(0) : mesh.TriangleIndices.Add(nextIdx) : mesh.TriangleIndices.Add(idx)
            mesh.TriangleIndices.Add(1) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(nextIdx + 1)
        Next
        Dim mat As New DiffuseMaterial(New SolidColorBrush(col))
        Return New GeometryModel3D(mesh, mat) With {.BackMaterial = mat}
    End Function

    Private Function CrearCono(radio As Double, altura As Double, col As Color) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        Dim segmentos = 16
        mesh.Positions.Add(New Point3D(0, altura / 2, 0))
        mesh.Positions.Add(New Point3D(0, -altura / 2, 0))
        Dim baseIdx = 2
        For i = 0 To segmentos - 1
            Dim angulo = i * 2 * Math.PI / segmentos
            mesh.Positions.Add(New Point3D(radio * Math.Cos(angulo), -altura / 2, radio * Math.Sin(angulo)))
        Next
        For i = 0 To segmentos - 1
            Dim idx = baseIdx + i
            Dim nextIdx = baseIdx + ((i + 1) Mod segmentos)
            mesh.TriangleIndices.Add(0) : mesh.TriangleIndices.Add(nextIdx) : mesh.TriangleIndices.Add(idx)
            mesh.TriangleIndices.Add(1) : mesh.TriangleIndices.Add(idx) : mesh.TriangleIndices.Add(nextIdx)
        Next
        Dim mat As New DiffuseMaterial(New SolidColorBrush(col))
        Return New GeometryModel3D(mesh, mat) With {.BackMaterial = mat}
    End Function
End Class