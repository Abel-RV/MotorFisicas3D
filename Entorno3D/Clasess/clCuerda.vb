Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clCuerda

#Region "Variables"
    Public ReadOnly Property Visual As ModelVisual3D
    Public Property modeloCuerda As GeometryModel3D

    Private _mesh As MeshGeometry3D
    Private _nodos As Point3D()
    Private _velocidades As Vector3D()
    Private _posAnteriores As Point3D()
    Private _numNodos As Integer = 25
    Private _longitudSegmento As Double = 0.15
    Private rnd As New Random()

    ' --- Variables para el quemado REALISTA ---
    Public vidaNodo() As Double  ' Nivel de vida de CADA segmento (1.0 = Intacto, 0.0 = Roto)
    Public enLlamas() As Boolean ' Determina si ese punto de la cuerda está ardiendo
    Private _materialDiffuse As DiffuseMaterial
    Private _colorOriginal As Color
    ' ---------------------------------

    Public Function obtenerPosicionNodo(indice As Integer) As Point3D
        Return _nodos(indice)
    End Function

#End Region

#Region "Constructor"
    Public Sub New(origen As Point3D, col As Color)
        _mesh = New MeshGeometry3D()
        _nodos = New Point3D(_numNodos - 1) {}
        _velocidades = New Vector3D(_numNodos - 1) {}
        _posAnteriores = New Point3D(_numNodos - 1) {}

        ReDim vidaNodo(_numNodos - 1)
        ReDim enLlamas(_numNodos - 1)

        For i As Integer = 0 To _numNodos - 1
            Dim ruidoX = (rnd.NextDouble() - 0.5) * 0.05
            Dim ruidoZ = (rnd.NextDouble() - 0.5) * 0.05
            _nodos(i) = New Point3D(origen.X + ruidoX, origen.Y - (i * _longitudSegmento), origen.Z + ruidoZ)
            _posAnteriores(i) = _nodos(i)
            _velocidades(i) = New Vector3D(0, 0, 0)

            vidaNodo(i) = 1.0
            enLlamas(i) = False
        Next

        actualizarMalla()

        _colorOriginal = col
        _materialDiffuse = New DiffuseMaterial(New SolidColorBrush(col))

        Dim modelo As New GeometryModel3D(_mesh, _materialDiffuse)
        modelo.BackMaterial = _materialDiffuse

        modeloCuerda = modelo
        Visual = New ModelVisual3D() With {.Content = modelo}
    End Sub
#End Region

#Region "Sistema de Quemado Realista"
    Private Sub procesarFuego()
        Dim ardiendo = False
        Dim vidaMinimaGlobal As Double = 1.0

        For i As Integer = 0 To _numNodos - 1
            If enLlamas(i) AndAlso vidaNodo(i) > 0 Then
                ardiendo = True
                vidaNodo(i) -= 0.01 ' Se consume poco a poco

                ' PROPAGACIÓN: Si está medio quemado, el fuego pasa a los vecinos
                If vidaNodo(i) < 0.6 Then
                    If i > 0 AndAlso vidaNodo(i - 1) > 0 Then enLlamas(i - 1) = True
                    If i < _numNodos - 1 AndAlso vidaNodo(i + 1) > 0 Then enLlamas(i + 1) = True
                End If
            End If

            If vidaNodo(i) < vidaMinimaGlobal AndAlso vidaNodo(i) > 0 Then
                vidaMinimaGlobal = vidaNodo(i)
            End If
            If vidaNodo(i) < 0 Then vidaNodo(i) = 0
        Next

        ' Si hay fuego, la cuerda entera se empieza a tiznar (oscurecer)
        If ardiendo Then
            Dim r As Byte = CByte(_colorOriginal.R * vidaMinimaGlobal)
            Dim g As Byte = CByte(_colorOriginal.G * Math.Pow(vidaMinimaGlobal, 2))
            Dim b As Byte = CByte(_colorOriginal.B * Math.Pow(vidaMinimaGlobal, 2))
            _materialDiffuse.Brush = New SolidColorBrush(Color.FromRgb(r, g, b))
        End If
    End Sub
#End Region

#Region "Private Sub actualizarMalla()"
    Private Sub actualizarMalla()
        Dim posiciones As New Point3DCollection()
        Dim indices As New Int32Collection()

        Dim numCaras As Integer = 8
        Dim prevU As Vector3D
        Dim prevDir As Vector3D

        For i As Integer = 0 To _numNodos - 1
            Dim p = _nodos(i)
            Dim dir As Vector3D

            If i = 0 Then
                dir = _nodos(1) - _nodos(0)
            ElseIf i = _numNodos - 1 Then
                dir = _nodos(_numNodos - 1) - _nodos(_numNodos - 2)
            Else
                dir = _nodos(i + 1) - _nodos(i - 1)
            End If

            If dir.LengthSquared > 0.0001 Then dir.Normalize() Else dir = New Vector3D(0, 1, 0)

            Dim u As Vector3D
            Dim v As Vector3D

            If i = 0 Then
                Dim arb As New Vector3D(0, 1, 0)
                If Math.Abs(Vector3D.DotProduct(dir, arb)) > 0.9 Then arb = New Vector3D(1, 0, 0)
                u = Vector3D.CrossProduct(dir, arb)
                u.Normalize()
            Else
                Dim cross = Vector3D.CrossProduct(prevDir, dir)
                If cross.LengthSquared > 0.0001 Then
                    Dim angle = Vector3D.AngleBetween(prevDir, dir)
                    Dim rot = New RotateTransform3D(New QuaternionRotation3D(New Quaternion(cross, angle)))
                    u = rot.Transform(prevU)
                ElseIf Vector3D.DotProduct(prevDir, dir) < 0 Then
                    u = -prevU
                Else
                    u = prevU
                End If
                u.Normalize()
            End If

            v = Vector3D.CrossProduct(dir, u)
            v.Normalize()

            prevU = u
            prevDir = dir

            ' El GROSOR de cada parte depende de si se ha quemado o no
            Dim radioLocal As Double = 0.04 * Math.Max(0.1, Math.Sqrt(vidaNodo(i)))

            For j As Integer = 0 To numCaras - 1
                Dim angulo As Double = (j / numCaras) * Math.PI * 2
                Dim px = p.X + (Math.Cos(angulo) * u.X + Math.Sin(angulo) * v.X) * radioLocal
                Dim py = p.Y + (Math.Cos(angulo) * u.Y + Math.Sin(angulo) * v.Y) * radioLocal
                Dim pz = p.Z + (Math.Cos(angulo) * u.Z + Math.Sin(angulo) * v.Z) * radioLocal
                posiciones.Add(New Point3D(px, py, pz))
            Next
        Next

        For i As Integer = 0 To _numNodos - 2
            ' ¡MAGIA! Si cualquiera de los dos extremos del segmento está quemado (0), no dibujamos la malla.
            If vidaNodo(i) <= 0 OrElse vidaNodo(i + 1) <= 0 Then Continue For

            Dim anilloActual = i * numCaras
            Dim anilloSiguiente = (i + 1) * numCaras

            For j As Integer = 0 To numCaras - 1
                Dim nextJ = (j + 1) Mod numCaras
                Dim a = anilloActual + j
                Dim b = anilloActual + nextJ
                Dim c = anilloSiguiente + j
                Dim d = anilloSiguiente + nextJ

                indices.Add(a) : indices.Add(c) : indices.Add(b)
                indices.Add(b) : indices.Add(c) : indices.Add(d)
            Next
        Next

        _mesh.Positions = posiciones
        _mesh.TriangleIndices = indices
    End Sub
#End Region

#Region "Public Sub simularFisica"
    Public Sub simularFisica(posRaton As Point3D, nodoAgarrado As Integer,
                         tanqueX As Double, tanqueZ As Double,
                         radioTanqueExterno As Double, radioTanqueInterno As Double,
                         alturaBorde As Double, alturaFondo As Double,
                         ventPos As Point3D, ventDir As Vector3D, fuerzaViento As Double)

        Dim nivelSuelo As Double = -1.25
        Dim diametroCuerda As Double = 0.08

        ' Calcular quemado y propagación
        procesarFuego()

        ' 1. APLICAR FUERZAS
        For i As Integer = 0 To _numNodos - 1
            _posAnteriores(i) = _nodos(i)
            _velocidades(i).Y -= 0.005 ' Gravedad

            If fuerzaViento > 0 AndAlso vidaNodo(i) > 0 Then
                Dim vectorAlNodo As Vector3D = _nodos(i) - ventPos
                Dim distancia As Double = vectorAlNodo.Length
                If distancia > 0 Then vectorAlNodo.Normalize()

                Dim alineacion As Double = Vector3D.DotProduct(vectorAlNodo, ventDir)
                If alineacion > 0.6 AndAlso distancia < 8.0 Then
                    Dim factorDistancia As Double = 1.0 - (distancia / 8.0)
                    Dim intensidadReal As Double = fuerzaViento * factorDistancia * alineacion

                    _velocidades(i).X += (ventDir.X * intensidadReal) + (rnd.NextDouble() - 0.5) * intensidadReal * 0.8
                    _velocidades(i).Y += (ventDir.Y * intensidadReal) + (rnd.NextDouble() - 0.5) * intensidadReal * 0.8
                    _velocidades(i).Z += (ventDir.Z * intensidadReal) + (rnd.NextDouble() - 0.5) * intensidadReal * 0.8
                End If
            End If

            _nodos(i) += _velocidades(i)

            ' --- COLISIONES ---
            If _nodos(i).Y < nivelSuelo + (diametroCuerda / 2) Then
                _nodos(i).Y = nivelSuelo + (diametroCuerda / 2)
            End If

            Dim dx As Double = _nodos(i).X - tanqueX
            Dim dz As Double = _nodos(i).Z - tanqueZ
            Dim distCentro As Double = Math.Sqrt(dx * dx + dz * dz)
            Dim radioCuerdaC As Double = diametroCuerda / 2.0

            If distCentro < radioTanqueExterno + radioCuerdaC AndAlso _nodos(i).Y < alturaBorde Then
                If distCentro >= radioTanqueInterno - radioCuerdaC Then
                    Dim distArriba = Math.Abs(_nodos(i).Y - alturaBorde)
                    Dim distAfuera = Math.Abs(distCentro - (radioTanqueExterno + radioCuerdaC))

                    If distArriba < 0.15 Then
                        _nodos(i).Y = alturaBorde
                    ElseIf distCentro < (radioTanqueInterno + radioTanqueExterno) / 2 Then
                        _nodos(i).X = tanqueX + (dx / distCentro) * (radioTanqueInterno - radioCuerdaC)
                        _nodos(i).Z = tanqueZ + (dz / distCentro) * (radioTanqueInterno - radioCuerdaC)
                    Else
                        _nodos(i).X = tanqueX + (dx / distCentro) * (radioTanqueExterno + radioCuerdaC)
                        _nodos(i).Z = tanqueZ + (dz / distCentro) * (radioTanqueExterno + radioCuerdaC)
                    End If
                Else
                    If _nodos(i).Y < alturaFondo + radioCuerdaC Then
                        _nodos(i).Y = alturaFondo + radioCuerdaC
                    End If
                End If
            End If
        Next

        ' 2. RESTRICCIONES FÍSICAS (Ignorar si está roto)
        If nodoAgarrado >= 0 Then _nodos(nodoAgarrado) = posRaton

        For iter As Integer = 0 To 19
            ' A. Rigidez
            For i As Integer = 0 To _numNodos - 3
                ' Si alguno de estos nodos está quemado (roto), la restricción desaparece
                If vidaNodo(i) <= 0 OrElse vidaNodo(i + 1) <= 0 OrElse vidaNodo(i + 2) <= 0 Then Continue For

                Dim dX = _nodos(i + 2).X - _nodos(i).X, dY = _nodos(i + 2).Y - _nodos(i).Y, dZ = _nodos(i + 2).Z - _nodos(i).Z
                Dim dist = Math.Sqrt(dX * dX + dY * dY + dZ * dZ)
                Dim dDeseada = _longitudSegmento * 1.5
                If dist < dDeseada And dist > 0.001 Then
                    Dim push = (dDeseada - dist) / dist * 0.5
                    If nodoAgarrado <> i And nodoAgarrado <> i + 2 Then
                        _nodos(i).X -= dX * push : _nodos(i).Y -= dY * push : _nodos(i).Z -= dZ * push
                        _nodos(i + 2).X += dX * push : _nodos(i + 2).Y += dY * push : _nodos(i + 2).Z += dZ * push
                    End If
                End If
            Next

            ' B. Longitud de los segmentos (LA ROTURA)
            For i As Integer = 0 To _numNodos - 2
                ' Si alguno de los dos nodos está quemado, se desconectan físicamente
                If vidaNodo(i) <= 0 OrElse vidaNodo(i + 1) <= 0 Then Continue For

                Dim dX = _nodos(i + 1).X - _nodos(i).X, dY = _nodos(i + 1).Y - _nodos(i).Y, dZ = _nodos(i + 1).Z - _nodos(i).Z
                Dim dist = Math.Sqrt(dX * dX + dY * dY + dZ * dZ)
                If dist > 0.0001 Then
                    Dim dif = (_longitudSegmento - dist) / dist * 0.5
                    If i <> nodoAgarrado Then _nodos(i).X -= dX * dif : _nodos(i).Y -= dY * dif : _nodos(i).Z -= dZ * dif
                    If (i + 1) <> nodoAgarrado Then _nodos(i + 1).X += dX * dif : _nodos(i + 1).Y += dY * dif : _nodos(i + 1).Z += dZ * dif
                End If
            Next

            ' C. Auto-colisión
            For i As Integer = 0 To _numNodos - 1
                For j As Integer = i + 2 To _numNodos - 1
                    Dim dX = _nodos(j).X - _nodos(i).X, dY = _nodos(j).Y - _nodos(i).Y, dZ = _nodos(j).Z - _nodos(i).Z
                    Dim distSq = dX * dX + dY * dY + dZ * dZ
                    If distSq < 0.0256 Then
                        Dim dist = Math.Sqrt(distSq)
                        Dim f = (0.16 - dist) / dist * 0.3
                        If i <> nodoAgarrado Then _nodos(i).X -= dX * f : _nodos(i).Y -= dY * f : _nodos(i).Z -= dZ * f
                        If j <> nodoAgarrado Then _nodos(j).X += dX * f : _nodos(j).Y += dY * f : _nodos(j).Z += dZ * f
                    End If
                Next
            Next

            If nodoAgarrado >= 0 Then _nodos(nodoAgarrado) = posRaton
        Next

        ' 3. ACTUALIZAR VELOCIDADES Y MALLA
        For i As Integer = 0 To _numNodos - 1
            If i <> nodoAgarrado Then
                Dim v = _nodos(i) - _posAnteriores(i)
                v *= 0.98 ' Fricción aire
                If _nodos(i).Y <= nivelSuelo + 0.05 Then v *= 0.1 ' Fricción suelo 
                _velocidades(i) = v
            Else
                _velocidades(i) = New Vector3D(0, 0, 0)
            End If
        Next
        actualizarMalla()
    End Sub
#End Region

End Class