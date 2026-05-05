Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clCadena
#Region "Variables"
    Public ReadOnly Property Visual As ModelVisual3D
    Private _mesh As MeshGeometry3D
    Private _nodos As Point3D()
    Private _posAnteriores As Point3D()
    Private _numNodos As Integer = 25
    Private _longitudSegmento As Double = 0.1 ' Distancia real entre los pivotes

    Public nodoAgarrado As Integer = -1
    Public posRaton As Point3D
    Private _rnd As New Random()
#End Region

#Region "Constructor"
    Public Sub New(origen As Point3D, col As Color)
        _mesh = New MeshGeometry3D()
        _nodos = New Point3D(_numNodos - 1) {}
        _posAnteriores = New Point3D(_numNodos - 1) {}

        Dim amplitudPerturbacion As Double = 0.01

        For i As Integer = 0 To _numNodos - 1
            Dim x As Double = origen.X
            Dim y As Double = origen.Y - (i * _longitudSegmento)
            Dim z As Double = origen.Z

            If i > 0 Then
                x += (_rnd.NextDouble() * 2 - 1) * amplitudPerturbacion
                z += (_rnd.NextDouble() * 2 - 1) * amplitudPerturbacion
            End If

            _nodos(i) = New Point3D(x, y, z)
            _posAnteriores(i) = _nodos(i)
        Next

        Dim grupo As New MaterialGroup()
        ' Color acero realista
        Dim colorAcero As Color = Color.FromRgb(160, 165, 170)
        grupo.Children.Add(New DiffuseMaterial(New SolidColorBrush(colorAcero)))
        grupo.Children.Add(New SpecularMaterial(New SolidColorBrush(Colors.White), 80.0))

        Dim modelo As New GeometryModel3D(_mesh, grupo)
        modelo.BackMaterial = grupo
        Visual = New ModelVisual3D() With {.Content = modelo}
    End Sub

    Public Function obtenerPosicionNodo(i As Integer) As Point3D
        Return _nodos(i)
    End Function
#End Region

#Region "Física y Restricciones"
    Public Sub simularFisica(tanqueX As Double, tanqueZ As Double,
                             radioTanqueExterno As Double, radioTanqueInterno As Double,
                             alturaBorde As Double, alturaFondo As Double)

        Dim diametroEslabon As Double = 0.1
        Dim radioC As Double = diametroEslabon / 2.0
        Dim nivelSuelo As Double = -1.25

        ' 1. Movimiento y Gravedad
        For i As Integer = 0 To _numNodos - 1
            If i = nodoAgarrado Then
                _nodos(i) = posRaton
                _posAnteriores(i) = posRaton
                Continue For
            End If

            Dim velX = _nodos(i).X - _posAnteriores(i).X
            Dim velY = _nodos(i).Y - _posAnteriores(i).Y
            Dim velZ = _nodos(i).Z - _posAnteriores(i).Z

            Dim velCuadrada = velX * velX + velY * velY + velZ * velZ
            If velCuadrada > 0.25 Then
                Dim factor = 0.5 / Math.Sqrt(velCuadrada)
                velX *= factor : velY *= factor : velZ *= factor
            End If

            ' Fricción del metal
            velX *= 0.99
            velY *= 0.99
            velZ *= 0.99

            _posAnteriores(i) = _nodos(i)

            ' Gravedad
            _nodos(i).X += velX
            _nodos(i).Y += velY - 0.008
            _nodos(i).Z += velZ
        Next

        ' 2. Restricciones (60 iteraciones para hacerla rígida de verdad)
        For iter As Integer = 0 To 60
            If nodoAgarrado >= 0 Then _nodos(nodoAgarrado) = posRaton

            ' --- A. COLISIONES PRIMERO --- 
            ' (Hacerlo antes evita que el tanque rompa las uniones de la cadena)
            For i As Integer = 0 To _numNodos - 1
                If i = nodoAgarrado Then Continue For

                If _nodos(i).Y < nivelSuelo + radioC Then
                    _nodos(i).Y = nivelSuelo + radioC
                End If

                Dim dxT = _nodos(i).X - tanqueX
                Dim dzT = _nodos(i).Z - tanqueZ
                Dim distCentro = Math.Sqrt(dxT * dxT + dzT * dzT)

                If distCentro > 0.0001 AndAlso distCentro < radioTanqueExterno + radioC AndAlso _nodos(i).Y < alturaBorde Then
                    If distCentro >= radioTanqueInterno - radioC Then
                        Dim distArriba = Math.Abs(_nodos(i).Y - alturaBorde)
                        Dim distAfuera = Math.Abs(distCentro - (radioTanqueExterno + radioC))
                        Dim distAdentro = Math.Abs(distCentro - (radioTanqueInterno - radioC))

                        If distArriba < 0.15 Then
                            _nodos(i).Y = alturaBorde
                        ElseIf distAfuera <= distAdentro Then
                            _nodos(i).X = tanqueX + (dxT / distCentro) * (radioTanqueExterno + radioC)
                            _nodos(i).Z = tanqueZ + (dzT / distCentro) * (radioTanqueExterno + radioC)
                        Else
                            _nodos(i).X = tanqueX + (dxT / distCentro) * (radioTanqueInterno - radioC)
                            _nodos(i).Z = tanqueZ + (dzT / distCentro) * (radioTanqueInterno - radioC)
                        End If
                    Else
                        If _nodos(i).Y < alturaFondo + radioC Then
                            _nodos(i).Y = alturaFondo + radioC
                        End If
                    End If
                End If
            Next

            ' --- B. AUTO-COLISIÓN SUAVE ---
            For i As Integer = 0 To _numNodos - 1
                For j As Integer = i + 2 To _numNodos - 1
                    Dim dX = _nodos(j).X - _nodos(i).X
                    Dim dY = _nodos(j).Y - _nodos(i).Y
                    Dim dZ = _nodos(j).Z - _nodos(i).Z
                    Dim distSq = dX * dX + dY * dY + dZ * dZ

                    Dim radioCol = _longitudSegmento * 0.7
                    If distSq < radioCol * radioCol AndAlso distSq > 0.0001 Then
                        Dim dist = Math.Sqrt(distSq)
                        Dim f = (radioCol - dist) / dist * 0.05

                        If i <> nodoAgarrado Then
                            _nodos(i).X -= dX * f : _nodos(i).Y -= dY * f : _nodos(i).Z -= dZ * f
                        End If
                        If j <> nodoAgarrado Then
                            _nodos(j).X += dX * f : _nodos(j).Y += dY * f : _nodos(j).Z += dZ * f
                        End If
                    End If
                Next
            Next

            ' --- C. DISTANCIA RÍGIDA AL FINAL ---
            ' (Garantiza que la cadena NUNCA se separe)
            For i As Integer = 0 To _numNodos - 2
                Dim dX = _nodos(i + 1).X - _nodos(i).X
                Dim dY = _nodos(i + 1).Y - _nodos(i).Y
                Dim dZ = _nodos(i + 1).Z - _nodos(i).Z
                Dim dist = Math.Sqrt(dX * dX + dY * dY + dZ * dZ)

                If dist > 0.0001 Then
                    Dim dif = _longitudSegmento - dist
                    Dim pushFactor = (dif / dist) * 0.5

                    Dim pushX = dX * pushFactor
                    Dim pushY = dY * pushFactor
                    Dim pushZ = dZ * pushFactor

                    If i = nodoAgarrado Then
                        _nodos(i + 1).X += pushX * 2 : _nodos(i + 1).Y += pushY * 2 : _nodos(i + 1).Z += pushZ * 2
                    ElseIf i + 1 = nodoAgarrado Then
                        _nodos(i).X -= pushX * 2 : _nodos(i).Y -= pushY * 2 : _nodos(i).Z -= pushZ * 2
                    Else
                        _nodos(i).X -= pushX : _nodos(i).Y -= pushY : _nodos(i).Z -= pushZ
                        _nodos(i + 1).X += pushX : _nodos(i + 1).Y += pushY : _nodos(i + 1).Z += pushZ
                    End If
                End If
            Next
        Next

        actualizarMalla()
    End Sub
#End Region

#Region "Generación de Malla 3D (Eslabones Matemáticamente Perfectos)"
    Private Sub actualizarMalla()
        Dim posiciones As New Point3DCollection()
        Dim indices As New Int32Collection()

        Dim radioAlambre As Double = 0.016 ' Grosor del metal
        Dim radioEslabon As Double = 0.026 ' Apertura del hueco interior

        Dim segRadial As Integer = 16
        Dim segCurva As Integer = 8

        For i As Integer = 0 To _numNodos - 2
            Dim pA = _nodos(i)
            Dim pB = _nodos(i + 1)
            Dim dir = pB - pA
            Dim dist = dir.Length
            If dist < 0.0001 Then Continue For
            dir.Normalize()

            ' FÓRMULA MATEMÁTICA EXACTA PARA ENGARCE PERFECTO:
            ' Asegura que el borde interior del hueco descanse JUSTO sobre el nodo.
            Dim zOffset As Double = (dist / 2.0) - radioEslabon + radioAlambre
            If zOffset < 0 Then zOffset = 0 ' Seguro por si la cadena se comprime

            Dim centro As New Point3D((pA.X + pB.X) / 2.0, (pA.Y + pB.Y) / 2.0, (pA.Z + pB.Z) / 2.0)

            Dim matrizAlineacion As New Matrix3D()
            Dim up = New Vector3D(0, 1, 0)
            If Math.Abs(Vector3D.DotProduct(dir, up)) > 0.99 Then up = New Vector3D(1, 0, 0)

            Dim right = Vector3D.CrossProduct(up, dir) : right.Normalize()
            Dim forward = Vector3D.CrossProduct(right, dir) : forward.Normalize()

            matrizAlineacion.M11 = right.X : matrizAlineacion.M12 = right.Y : matrizAlineacion.M13 = right.Z
            matrizAlineacion.M21 = forward.X : matrizAlineacion.M22 = forward.Y : matrizAlineacion.M23 = forward.Z
            matrizAlineacion.M31 = dir.X : matrizAlineacion.M32 = dir.Y : matrizAlineacion.M33 = dir.Z

            ' Alternar rotación 90 grados
            If i Mod 2 = 0 Then
                matrizAlineacion.Rotate(New Quaternion(New Vector3D(0, 0, 1), 90))
            End If

            GenerarEslabonOvalado(posiciones, indices, centro, matrizAlineacion, radioAlambre, radioEslabon, zOffset, segRadial, segCurva)
        Next

        _mesh.Positions = posiciones
        _mesh.TriangleIndices = indices
    End Sub

    Private Sub GenerarEslabonOvalado(pos As Point3DCollection, idx As Int32Collection, centro As Point3D, matriz As Matrix3D,
                                     rAlambre As Double, rEslabon As Double, zOffset As Double,
                                     segRadial As Integer, segCurva As Integer)

        Dim baseIdx As Integer = pos.Count
        Dim angulos As New List(Of Double)
        Dim offsets As New List(Of Double)

        For i As Integer = 0 To segCurva
            angulos.Add(i * Math.PI / segCurva)
            offsets.Add(zOffset)
        Next
        For i As Integer = 0 To segCurva
            angulos.Add(Math.PI + (i * Math.PI / segCurva))
            offsets.Add(-zOffset)
        Next

        Dim numAnillos As Integer = angulos.Count

        For j As Integer = 0 To numAnillos - 1
            Dim alpha As Double = angulos(j)
            Dim zShift As Double = offsets(j)

            For k As Integer = 0 To segRadial - 1
                Dim gamma As Double = k * 2 * Math.PI / segRadial

                Dim xR As Double = (rEslabon + rAlambre * Math.Cos(gamma)) * Math.Cos(alpha)
                Dim yR As Double = rAlambre * Math.Sin(gamma)
                Dim zR As Double = (rEslabon + rAlambre * Math.Cos(gamma)) * Math.Sin(alpha) + zShift

                Dim puntoLocal As New Point3D(xR, yR, zR)
                pos.Add(matriz.Transform(puntoLocal) + New Vector3D(centro.X, centro.Y, centro.Z))
            Next
        Next

        For j As Integer = 0 To numAnillos - 1
            For k As Integer = 0 To segRadial - 1
                Dim i0 = baseIdx + j * segRadial + k
                Dim i1 = baseIdx + j * segRadial + (k + 1) Mod segRadial
                Dim i2 = baseIdx + ((j + 1) Mod numAnillos) * segRadial + k
                Dim i3 = baseIdx + ((j + 1) Mod numAnillos) * segRadial + (k + 1) Mod segRadial

                idx.Add(i0) : idx.Add(i2) : idx.Add(i1)
                idx.Add(i1) : idx.Add(i2) : idx.Add(i3)
            Next
        Next
    End Sub
#End Region
End Class