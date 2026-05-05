Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Forms.Integration
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class FormTresDe

#Region "Variables"

    Dim A As Integer = 2147483647
    Private cilindroInterno As clCilindro
    Private cilindroExterno As clCilindro
    Private cuerda As clCuerda
    Private ventilador As clVentilador
    Private cadenaMetalica As clCadena
    Private fuego As clFuego
    Private minero As clMinero

    Private camTarget As New Point3D(0, -1.0, 0)
    Private camDistance As Double = 8.0
    Private camYaw As Double = 45.0
    Private camPitch As Double = 25.0

    Private lastMousePosition As Point
    Private isOrbiting As Boolean = False
    Private isPanning As Boolean = False
    Private myPCamera As PerspectiveCamera

    Private WithEvents animTimer As New Timer With {.Interval = 10}
    Private alturaActual As Double = 0
    Private alturaObjetivo As Double = 0
    Private myViewport3D As New Viewport3D()
    Private canva As Canvas
    Private lblEtiquetaa As TextBlock

    Private Const ALTURA_MAXIMA_3D As Double = 2.5

    Private gotasApiladas As New List(Of clGota)
    Private rnd As New Random()
    Private colorActualLiquido As Color = Colors.LightBlue
    Private cuadriculaFondo(9, 9) As Double
    Private contenedorGotas As New ModelVisual3D()
    Private tanqueAnteriorX As Double = 0
    Private tanqueAnteriorZ As Double = 0

    Private cuadrado As clTela
    Private superficieAgua As clSuperficieAgua

    Private Const TAMANIO_CELDA = 0.2

    Private indexNodoAgarrado As Integer = -1
    Private posAgarre3D As Point3D

#End Region

    Private Sub ActualizarCamara()
        If camPitch > 89.0 Then camPitch = 89.0
        If camPitch < -89.0 Then camPitch = -89.0

        ' 2. Convertir a radianes
        Dim yawRad As Double = camYaw * Math.PI / 180.0
        Dim pitchRad As Double = camPitch * Math.PI / 180.0

        ' 3. Coordenadas esféricas a cartesianas (Magia orbital)
        Dim x As Double = camTarget.X + camDistance * Math.Cos(pitchRad) * Math.Sin(yawRad)
        Dim y As Double = camTarget.Y + camDistance * Math.Sin(pitchRad)
        Dim z As Double = camTarget.Z + camDistance * Math.Cos(pitchRad) * Math.Cos(yawRad)

        myPCamera.Position = New Point3D(x, y, z)

        ' 4. Forzar a mirar siempre al objetivo
        myPCamera.LookDirection = camTarget - myPCamera.Position

        actualizarEtiqueta()
    End Sub
    Private Sub FormTresDe_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim host As New ElementHost With {.Dock = DockStyle.Fill}

        myPCamera = New PerspectiveCamera()
        myPCamera.FieldOfView = 60
        ' Limpiamos transformaciones viejas (ahora la posición es absoluta)
        myPCamera.Transform = Transform3D.Identity
        myViewport3D.Camera = myPCamera

        ActualizarCamara()

        Dim sueloMesh As New MeshGeometry3D()
        Dim tamanoSuelo As Double = 20.0
        Dim ySuelo As Double = -1.26 ' Justo un poco por debajo del tanque

        sueloMesh.Positions.Add(New Point3D(-tamanoSuelo, ySuelo, -tamanoSuelo))
        sueloMesh.Positions.Add(New Point3D(tamanoSuelo, ySuelo, -tamanoSuelo))
        sueloMesh.Positions.Add(New Point3D(-tamanoSuelo, ySuelo, tamanoSuelo))
        sueloMesh.Positions.Add(New Point3D(tamanoSuelo, ySuelo, tamanoSuelo))

        ' Triángulos del suelo
        sueloMesh.TriangleIndices.Add(0) : sueloMesh.TriangleIndices.Add(2) : sueloMesh.TriangleIndices.Add(1)
        sueloMesh.TriangleIndices.Add(1) : sueloMesh.TriangleIndices.Add(2) : sueloMesh.TriangleIndices.Add(3)

        Dim materialSuelo As New DiffuseMaterial(New SolidColorBrush(Color.FromRgb(45, 50, 60))) ' Gris azulado oscuro
        Dim modeloSuelo As New GeometryModel3D(sueloMesh, materialSuelo)
        myViewport3D.Children.Add(New ModelVisual3D() With {.Content = modeloSuelo})
        ' -----------------------

        cilindroInterno = New clCilindro(0.9, 0.0, 0.05, Colors.DarkOrange, 1.0)
        cilindroInterno.posicionar(0, -1.25, 0)
        myViewport3D.Children.Add(cilindroInterno.Visual)

        myViewport3D.Children.Add(contenedorGotas)

        superficieAgua = New clSuperficieAgua(0.95, colorActualLiquido)
        myViewport3D.Children.Add(superficieAgua.Visual)

        cuerda = New clCuerda(New Point3D(2.5, 2.0, 0), Colors.SaddleBrown)
        myViewport3D.Children.Add(cuerda.Visual)

        cadenaMetalica = New clCadena(New Point3D(-2.5, 2.0, 0), Colors.LightGray)
        myViewport3D.Children.Add(cadenaMetalica.Visual)

        fuego = New clFuego(2.0, -1.0, 0.0)
        myViewport3D.Children.Add(fuego.Visual)

        minero = New clMinero(2.0, -1.25, -2.0)
        myViewport3D.Children.Add(minero.Visual)
        ' --- LUCES MEJORADAS ---
        ' Luz principal
        myViewport3D.Children.Add(New ModelVisual3D() With {.Content = New DirectionalLight(Colors.White, New Vector3D(-1, -1, -3))})
        ' Nueva Luz secundaria (Contra luz cálida suave para dar volumen)
        myViewport3D.Children.Add(New ModelVisual3D() With {.Content = New DirectionalLight(Color.FromRgb(100, 80, 80), New Vector3D(1, 0, 3))})
        ' Luz ambiental
        myViewport3D.Children.Add(New ModelVisual3D() With {.Content = New AmbientLight(Color.FromRgb(90, 90, 100))}) ' Ligeramente azulada

        ventilador = New clVentilador(-2.5, -1.25, -2.5)
        myViewport3D.Children.Add(ventilador.visual)
        Dim fondoGradiente As New LinearGradientBrush()
        fondoGradiente.StartPoint = New Point(0, 0)
        fondoGradiente.EndPoint = New Point(0, 1)
        fondoGradiente.GradientStops.Add(New GradientStop(Color.FromRgb(20, 30, 45), 0.0))
        fondoGradiente.GradientStops.Add(New GradientStop(Color.FromRgb(5, 5, 10), 1.0))

        Dim gridContenedor As New Grid() With {.Background = fondoGradiente}
        ' -------------------------------------

        gridContenedor.Children.Add(myViewport3D)

        canva = New Canvas() With {.IsHitTestVisible = False}
        gridContenedor.Children.Add(canva)
        lblEtiquetaa = New TextBlock() With {.Foreground = Brushes.White, .Background = New SolidColorBrush(Colors.Gray), .Padding = New Thickness(6, 2, 6, 2), .Visibility = Visibility.Collapsed}
        canva.Children.Add(lblEtiquetaa)

        cilindroExterno = New clCilindro(1.0, 0.95, 2.5, Colors.LightBlue, 0.3)
        cilindroExterno.posicionar(0, -1.25, 0)
        myViewport3D.Children.Add(cilindroExterno.Visual)

        Me.KeyPreview = True

        ' ---> AÑADE ESTAS 4 LÍNEAS AQUÍ <---
        AddHandler gridContenedor.MouseDown, AddressOf Grid_MouseDown
        AddHandler gridContenedor.MouseUp, AddressOf Grid_MouseUp
        AddHandler gridContenedor.MouseMove, AddressOf Grid_MouseMove
        AddHandler gridContenedor.MouseWheel, AddressOf Grid_MouseWheel
        ' -----------------------------------

        host.Child = gridContenedor
        Me.Controls.Add(host)

        ReiniciarCuadricula()
        animTimer.Start()
    End Sub

    Private Sub ReiniciarCuadricula()
        For x As Integer = 0 To 9
            For z As Integer = 0 To 9
                cuadriculaFondo(x, z) = cilindroInterno.PosY + 0.1
            Next
        Next
    End Sub

    Private Sub animTimer_Tick(sender As Object, e As EventArgs) Handles animTimer.Tick
        Dim diferencia As Double = alturaObjetivo - alturaActual

        Dim velTanqueX As Double = cilindroInterno.PosX - tanqueAnteriorX
        Dim velTanqueZ As Double = cilindroInterno.PosZ - tanqueAnteriorZ

        tanqueAnteriorX = cilindroInterno.PosX
        tanqueAnteriorZ = cilindroInterno.PosZ

        ' --- 1. EMISIÓN DE GOTAS ---
        If diferencia > 0.05 AndAlso gotasApiladas.Count < 99999 Then
            Dim radioEmision As Double = 0.3 * cilindroInterno.EscalaActual
            Dim px As Double = cilindroInterno.PosX + (rnd.NextDouble() - 0.5) * radioEmision
            Dim pz As Double = cilindroInterno.PosZ + (rnd.NextDouble() - 0.5) * radioEmision
            Dim alturaEmisor As Double = cilindroInterno.PosY + (3.5 * cilindroInterno.EscalaActual)

            Dim nuevaGota As New clGota(px, alturaEmisor, pz, colorActualLiquido)
            nuevaGota.VelX = (rnd.NextDouble() - 0.5) * 0.02
            nuevaGota.VelZ = (rnd.NextDouble() - 0.5) * 0.02

            gotasApiladas.Add(nuevaGota)
            contenedorGotas.Children.Add(nuevaGota.Visual)
            alturaActual += 0.01
        End If

        Dim radioTanque As Double = 0.9 * cilindroInterno.EscalaActual
        Dim fondoTanque As Double = cilindroInterno.PosY + 0.1

        ' --- 2. MOVIMIENTO, GRAVEDAD Y FRICCIÓN DE GOTAS ---
        For i As Integer = 0 To gotasApiladas.Count - 1
            Dim g As clGota = gotasApiladas(i)

            g.VelX *= 0.97
            g.VelY = (g.VelY - 0.005) * 0.97
            g.VelZ *= 0.97

            g.PosX += g.VelX
            g.PosY += g.VelY
            g.PosZ += g.VelZ

            If g.PosY - g.Radio < fondoTanque Then
                g.PosY = fondoTanque + g.Radio
                g.VelY *= -0.01
                g.VelX *= 0.7
                g.VelZ *= 0.7

                g.VelX += velTanqueX * 0.2
                g.VelZ += velTanqueZ * 0.2
            End If

            Dim dist As Double = Math.Sqrt((g.PosX - cilindroInterno.PosX) ^ 2 + (g.PosZ - cilindroInterno.PosZ) ^ 2)
            If dist + g.Radio > radioTanque Then
                Dim nx As Double = (g.PosX - cilindroInterno.PosX) / dist
                Dim nz As Double = (g.PosZ - cilindroInterno.PosZ) / dist
                g.PosX = cilindroInterno.PosX + nx * (radioTanque - g.Radio)
                g.PosZ = cilindroInterno.PosZ + nz * (radioTanque - g.Radio)
                g.VelX *= -0.2
                g.VelZ *= -0.2
            End If
        Next

        ' --- 3. FLUIDOS (ALTA VISCOSIDAD) ---
        Dim radioInfluencia As Double = 0.15
        Dim repulsion As Double = 0.005
        Dim viscosidad As Double = 0.12

        For i As Integer = 0 To gotasApiladas.Count - 1
            Dim g1 As clGota = gotasApiladas(i)
            For j As Integer = i + 1 To gotasApiladas.Count - 1
                Dim g2 As clGota = gotasApiladas(j)
                Dim dx As Double = g2.PosX - g1.PosX
                Dim dy As Double = g2.PosY - g1.PosY
                Dim dz As Double = g2.PosZ - g1.PosZ
                Dim d As Double = Math.Sqrt(dx * dx + dy * dy + dz * dz)

                If d < radioInfluencia And d > 0.001 Then
                    Dim q As Double = 1.0 - (d / radioInfluencia)
                    Dim fuerza As Double = q * q * repulsion

                    Dim viscY As Double = (g2.VelY - g1.VelY) * viscosidad * q
                    g1.VelY += viscY : g2.VelY -= viscY

                    g1.VelX -= (dx / d) * fuerza : g2.VelX += (dx / d) * fuerza
                    g1.VelZ -= (dz / d) * fuerza : g2.VelZ += (dz / d) * fuerza
                End If
            Next
            g1.ActualizarGrafico()
        Next

        ' --- 4. ACTUALIZAR SUPERFICIE ESPESA ---
        If superficieAgua IsNot Nothing Then
            superficieAgua.GenerarSuperficie(cilindroInterno.PosX, cilindroInterno.PosZ, fondoTanque, gotasApiladas)
        End If

        ' --- 5. TELA Y UI ---
        Dim radioInternoTanque As Double = 0.95 * cilindroExterno.EscalaActual
        Dim radioExternoTanque As Double = 1.0 * cilindroExterno.EscalaActual
        Dim alturaBordeTanque As Double = cilindroExterno.PosY + (2.5 * cilindroExterno.EscalaActual)
        Dim sueloInterno As Double = cilindroInterno.PosY + 0.1
        Dim sueloExterno As Double = -1.25

        If cuadrado IsNot Nothing Then
            cuadrado.DeformarYCaer(cilindroExterno.PosX, cilindroExterno.PosZ, radioInternoTanque, radioExternoTanque, alturaBordeTanque, sueloInterno, sueloExterno)
        End If

        ' --- 6. VENTILADOR ---
        Dim vPos As New Point3D(0, 0, 0)
        Dim vDir As New Vector3D(0, 0, 0)
        Dim vFuerza As Double = 0.0

        If ventilador IsNot Nothing Then
            ventilador.animar()
            vPos = ventilador.posicionCabeza
            vDir = ventilador.direccionViento
            vFuerza = 0.025
        End If

        ' --- 7. FÍSICAS DE CUERDA Y CADENA ---
        If cuerda IsNot Nothing Then
            cuerda.simularFisica(posAgarre3D, indexNodoAgarrado,
                                 cilindroInterno.PosX, cilindroInterno.PosZ,
                                 radioExternoTanque, radioInternoTanque,
                                 alturaBordeTanque, sueloInterno,
                                 vPos, vDir, vFuerza)
        End If

        If cadenaMetalica IsNot Nothing Then
            cadenaMetalica.simularFisica(cilindroInterno.PosX, cilindroInterno.PosZ, radioExternoTanque, radioInternoTanque, alturaBordeTanque, sueloInterno)
        End If

        ' --- 8. ANIMAR EL FUEGO (¡Esto era lo que faltaba!) ---
        If fuego IsNot Nothing Then
            fuego.animar()
        End If

        If minero IsNot Nothing Then
            minero.animar()
        End If

        ' --- 9. INTERACCIÓN: QUEMAR LA CUERDA ---
        If fuego IsNot Nothing AndAlso cuerda IsNot Nothing Then
            ' ¡AQUÍ ESTÁ LA CLAVE! Reducimos el radio mortal de 0.5 a 0.2
            Dim radioFuego As Double = 0.2

            ' Comprobar QUÉ eslabón exacto está dentro del fuego
            For i As Integer = 0 To 24
                ' Si ya está totalmente quemado, lo ignoramos
                If cuerda.vidaNodo(i) <= 0 Then Continue For

                Dim nodo = cuerda.obtenerPosicionNodo(i)
                Dim dxF = nodo.X - fuego.PosX
                Dim dyF = nodo.Y - (fuego.PosY + 0.2) ' Ajustamos un poco más abajo el centro térmico
                Dim dzF = nodo.Z - fuego.PosZ

                Dim distFuegoSq = (dxF * dxF) + (dyF * dyF) + (dzF * dzF)

                ' Si entra en la hitbox reducida, arde
                If distFuegoSq < (radioFuego * radioFuego) Then
                    cuerda.enLlamas(i) = True
                End If
            Next
        End If

        lblEtiquetaa.Text = $"Volumen de Fluido: {gotasApiladas.Count}"
        actualizarEtiqueta()
    End Sub

    Private Sub Grid_MouseWheel(sender As Object, e As MouseWheelEventArgs)

        camDistance -= e.Delta * 0.005

        If camDistance < 2.0 Then camDistance = 2.0
        If camDistance > 25.0 Then camDistance = 25.0

        ActualizarCamara()
    End Sub
    Private Sub Grid_MouseDown(sender As Object, e As MouseButtonEventArgs)
        lastMousePosition = e.GetPosition(DirectCast(sender, IInputElement))

        If e.LeftButton = MouseButtonState.Pressed Then
            Dim hitResult As HitTestResult = VisualTreeHelper.HitTest(myViewport3D, lastMousePosition)

            If hitResult IsNot Nothing AndAlso TypeOf hitResult Is RayMeshGeometry3DHitTestResult Then
                Dim meshResult = DirectCast(hitResult, RayMeshGeometry3DHitTestResult)

                ' --- 1. COMPROBAR LA CUERDA ORIGINAL ---
                If cuerda IsNot Nothing AndAlso meshResult.ModelHit Is cuerda.modeloCuerda Then
                    Dim puntoImpacto = meshResult.PointHit
                    Dim minDist = Double.MaxValue
                    For i As Integer = 0 To 24
                        Dim d = (cuerda.obtenerPosicionNodo(i) - puntoImpacto).LengthSquared
                        If d < minDist Then
                            minDist = d : indexNodoAgarrado = i
                        End If
                    Next
                    If indexNodoAgarrado >= 0 Then
                        posAgarre3D = cuerda.obtenerPosicionNodo(indexNodoAgarrado)
                        DirectCast(sender, UIElement).CaptureMouse()
                        Return
                    End If
                End If

                ' --- 2. COMPROBAR LA NUEVA CADENA METÁLICA ---
                If cadenaMetalica IsNot Nothing Then
                    ' Obtenemos el modelo interior del visual de la cadena
                    Dim modeloCadena = DirectCast(cadenaMetalica.Visual.Content, GeometryModel3D)

                    If meshResult.ModelHit Is modeloCadena Then
                        Dim puntoImpacto = meshResult.PointHit
                        Dim minDist = Double.MaxValue
                        For i As Integer = 0 To 19 ' 20 nodos en clCadena
                            Dim d = (cadenaMetalica.obtenerPosicionNodo(i) - puntoImpacto).LengthSquared
                            If d < minDist Then
                                minDist = d : cadenaMetalica.nodoAgarrado = i
                            End If
                        Next
                        If cadenaMetalica.nodoAgarrado >= 0 Then
                            cadenaMetalica.posRaton = cadenaMetalica.obtenerPosicionNodo(cadenaMetalica.nodoAgarrado)
                            DirectCast(sender, UIElement).CaptureMouse()
                            Return
                        End If
                    End If
                End If
            End If
        End If

        ' Clic derecho: Paneo / Orbita
        If e.RightButton = MouseButtonState.Pressed Then
            If Keyboard.IsKeyDown(Key.LeftShift) OrElse Keyboard.IsKeyDown(Key.RightShift) Then
                isPanning = True
            Else
                isOrbiting = True
            End If
            DirectCast(sender, UIElement).CaptureMouse()
        End If
    End Sub

    Private Sub Grid_MouseMove(sender As Object, e As MouseEventArgs)
        Dim currentPosition = e.GetPosition(DirectCast(sender, IInputElement))
        Dim deltaX As Double = currentPosition.X - lastMousePosition.X
        Dim deltaY As Double = currentPosition.Y - lastMousePosition.Y

        Dim radianes As Double = camYaw * Math.PI / 180.0

        ' Mover la cuerda original
        If indexNodoAgarrado >= 0 Then
            posAgarre3D.X += (deltaX * Math.Cos(radianes)) * 0.015
            posAgarre3D.Z += (-deltaX * Math.Sin(radianes)) * 0.015
            posAgarre3D.Y -= deltaY * 0.015
            lastMousePosition = currentPosition
            Return
        End If

        ' Mover la NUEVA CADENA METÁLICA
        If cadenaMetalica IsNot Nothing AndAlso cadenaMetalica.nodoAgarrado >= 0 Then
            cadenaMetalica.posRaton.X += (deltaX * Math.Cos(radianes)) * 0.015
            cadenaMetalica.posRaton.Z += (-deltaX * Math.Sin(radianes)) * 0.015
            cadenaMetalica.posRaton.Y -= deltaY * 0.015
            lastMousePosition = currentPosition
            Return
        End If

        ' Mover la cámara (orbita o paneo) ... (mantén tu código actual aquí)
        If isOrbiting Then
            camYaw -= deltaX * 0.5
            camPitch += deltaY * 0.5
            ActualizarCamara()
        ElseIf isPanning Then
            Dim lookDir As Vector3D = myPCamera.LookDirection
            lookDir.Normalize()
            Dim globalUp As New Vector3D(0, 1, 0)
            Dim rightVector As Vector3D = Vector3D.CrossProduct(lookDir, globalUp)
            rightVector.Normalize()
            Dim upVector As Vector3D = Vector3D.CrossProduct(rightVector, lookDir)
            upVector.Normalize()
            Dim velPaneo As Double = 0.01 * camDistance
            camTarget -= rightVector * (deltaX * velPaneo)
            camTarget += upVector * (deltaY * velPaneo)
            ActualizarCamara()
        End If

        lastMousePosition = currentPosition
    End Sub

    Private Sub Grid_MouseUp(sender As Object, e As MouseButtonEventArgs)
        isOrbiting = False
        isPanning = False

        ' Soltar ambas
        indexNodoAgarrado = -1
        If cadenaMetalica IsNot Nothing Then cadenaMetalica.nodoAgarrado = -1

        DirectCast(sender, UIElement).ReleaseMouseCapture()
    End Sub

    Private Sub numRelleno_ValueChanged(sender As Object, e As EventArgs) Handles numRelleno.ValueChanged
        calcularRelleno()
    End Sub
    Private Sub calcularRelleno()
        Dim porcentaje As Double = CDbl(numRelleno.Value)
        alturaObjetivo = (porcentaje / 100.0) * ALTURA_MAXIMA_3D
    End Sub
    Private Sub btnChocoBlanco_Click(sender As Object, e As EventArgs)
        colorActualLiquido = Colors.LightYellow
    End Sub
    Private Sub btnChocoLeche_Click(sender As Object, e As EventArgs)
        colorActualLiquido = Colors.DarkOrange
    End Sub
    Private Sub btnChocoNegro_Click(sender As Object, e As EventArgs)
        colorActualLiquido = Colors.Red
    End Sub
    Private Sub actualizarEtiqueta()
        If myViewport3D Is Nothing OrElse lblEtiquetaa Is Nothing Then Return
        Dim puntoLocal As New Point3D(0, 1.5, 0)
        Try
            Dim transform As GeneralTransform3DTo2D = TryCast(cilindroInterno.Visual.TransformToAncestor(myViewport3D), GeneralTransform3DTo2D)
            If transform Is Nothing Then Return
            Dim punto2D As Point = Nothing
            If transform.TryTransform(puntoLocal, punto2D) Then
                Canvas.SetLeft(lblEtiquetaa, punto2D.X - lblEtiquetaa.ActualWidth / 2)
                Canvas.SetTop(lblEtiquetaa, punto2D.Y - lblEtiquetaa.ActualHeight - 4)
                lblEtiquetaa.Visibility = Visibility.Visible
            Else
                lblEtiquetaa.Visibility = Visibility.Collapsed
            End If
        Catch ex As Exception
            lblEtiquetaa.Visibility = Visibility.Collapsed

        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If cuadrado IsNot Nothing Then myViewport3D.Children.Remove(cuadrado.Visual)
        cuadrado = New clTela(4.0, Colors.MediumPurple)
        myViewport3D.Children.Remove(cilindroExterno.Visual)
        myViewport3D.Children.Add(cuadrado.Visual)
        myViewport3D.Children.Add(cilindroExterno.Visual)
    End Sub
End Class