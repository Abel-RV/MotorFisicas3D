Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Partial Public Class clMinero
    ' =========================================================
    ' LÓGICA DE LA MÁQUINA DE ESTADOS Y ANIMACIÓN
    ' =========================================================

    Public Sub Animar()
        tiempoAnimacion += 0.015

        Select Case estado
            Case EstadoMinero.Trabajando
                Dim cycle As Double = tiempoAnimacion Mod 1.0
                AnimarMineria(cycle)

                If cronometro.Elapsed.TotalSeconds >= TIEMPO_TRABAJO_SEGUNDOS AndAlso cycle > 0.0 AndAlso cycle < 0.05 Then
                    _tsHombroDer = rotHombroDer.Angle
                    _tsCodoDer = rotCodoDer.Angle
                    _tsHombroIzq = rotHombroIzq.Angle
                    _tsCodoIzq = rotCodoIzq.Angle
                    _tsTorso = rotTorso.Angle
                    _tsCintura = rotCintura.Angle
                    _tsCabeza = rotCabeza.Angle
                    _tsCabezaY = rotCabezaY.Angle
                    _tsMusloIzq = rotMusloIzq.Angle
                    _tsRodillaIzq = rotRodillaIzq.Angle
                    _tsMusloDer = rotMusloDer.Angle
                    _tsRodillaDer = rotRodillaDer.Angle
                    estado = EstadoMinero.Transicion
                    progresoTransicion = 0.0
                End If

            Case EstadoMinero.Transicion
                progresoTransicion += 0.005
                If progresoTransicion >= 1.0 Then
                    progresoTransicion = 1.0
                    estado = EstadoMinero.Descansando
                End If
                AnimarSecuenciaSentarse(progresoTransicion)

            Case EstadoMinero.Descansando
                AnimarRespiracionSentado()
        End Select

        UpdateEscombrosPhys()
    End Sub

    Private Sub AnimarMineria(cycle As Double)
        scalePicoMano.ScaleX = 1 : scalePicoMano.ScaleY = 1 : scalePicoMano.ScaleZ = 1
        scalePicoSuelo.ScaleX = 0 : scalePicoSuelo.ScaleY = 0 : scalePicoSuelo.ScaleZ = 0
        posPersonaje.OffsetX = posBaseX

        rotMusloIzqX.Angle = 0 : rotMusloIzqY.Angle = 0
        rotMusloDerX.Angle = 0 : rotMusloDerY.Angle = 0

        Dim tBody As Double = ObtenerEstadoAnimacion(cycle)
        Dim armCycle As Double = cycle - 0.02
        If armCycle < 0 Then armCycle += 1.0
        Dim tArm As Double = ObtenerEstadoAnimacion(armCycle)

        rotHombroDer.Angle = Lerp(160.0, ANGULO_IMPACTO_PICO, tArm)
        rotCodoDer.Angle = Lerp(80.0, 5.0, tArm)
        rotTorso.Angle = Lerp(15.0, -30.0, tBody)
        rotCintura.Angle = Lerp(20.0, -10.0, tBody)

        Dim squash As Double = Lerp(0.0, 0.02, tBody)
        Dim breathe As Double = 0.005 * Math.Sin(tiempoAnimacion * 40.0)
        Dim microBounce As Double = If(cycle > 0.6 And cycle < 0.7, 0.008 * Math.Sin((cycle - 0.6) * Math.PI * 40), 0)
        posPersonaje.OffsetY = posBaseY - squash + microBounce + breathe

        rotCabeza.Angle = -rotTorso.Angle * 0.8
        rotCabezaY.Angle = -rotCintura.Angle * 0.9

        rotHombroIzq.Angle = Lerp(10.0, -20.0, tBody)
        rotCodoIzq.Angle = Lerp(20.0, 35.0, tBody)

        Dim flexIzq As Double = Lerp(-5.0, -15.0, tBody)
        rotMusloIzq.Angle = flexIzq
        rotRodillaIzq.Angle = -flexIzq * 1.5
        Dim flexDer As Double = Lerp(5.0, 10.0, tBody)
        rotMusloDer.Angle = flexDer
        rotRodillaDer.Angle = flexDer * 1.2

        If cycle >= 0.59 And cycle <= 0.61 Then GenerarEscombros()

        rotTobilloIzq.Angle = -(rotMusloIzq.Angle + rotRodillaIzq.Angle)
        rotTobilloDer.Angle = -(rotMusloDer.Angle + rotRodillaDer.Angle)
    End Sub

    Private Sub AnimarSecuenciaSentarse(t As Double)
        ' FASE 1: Baja el brazo para soltar el pico y estabilizar ejes
        Dim p1 As Double = Math.Min(t / 0.15, 1.0)

        rotHombroDer.Angle = Lerp(_tsHombroDer, -10.0, p1)
        rotCodoDer.Angle = Lerp(_tsCodoDer, 10.0, p1)
        rotHombroIzq.Angle = Lerp(_tsHombroIzq, 0.0, p1)
        rotCodoIzq.Angle = Lerp(_tsCodoIzq, 5.0, p1)
        rotTorso.Angle = Lerp(_tsTorso, 0.0, p1)
        rotCintura.Angle = Lerp(_tsCintura, 0.0, p1)
        rotCabeza.Angle = Lerp(_tsCabeza, 0.0, p1)
        rotCabezaY.Angle = Lerp(_tsCabezaY, 0.0, p1)

        rotMusloIzq.Angle = Lerp(_tsMusloIzq, 0.0, p1)
        rotRodillaIzq.Angle = Lerp(_tsRodillaIzq, 0.0, p1)
        rotMusloDer.Angle = Lerp(_tsMusloDer, 0.0, p1)
        rotRodillaDer.Angle = Lerp(_tsRodillaDer, 0.0, p1)

        ' Limpiamos cualquier torsión previa para evitar el Gimbal Lock
        rotMusloIzqX.Angle = Lerp(rotMusloIzqX.Angle, 0.0, p1)
        rotMusloIzqY.Angle = Lerp(rotMusloIzqY.Angle, 0.0, p1)
        rotMusloDerX.Angle = Lerp(rotMusloDerX.Angle, 0.0, p1)
        rotMusloDerY.Angle = Lerp(rotMusloDerY.Angle, 0.0, p1)
        rotTobilloIzq.Angle = Lerp(rotTobilloIzq.Angle, 0.0, p1)
        rotTobilloDer.Angle = Lerp(rotTobilloDer.Angle, 0.0, p1)

        ' FASE 1.5: Intercambio de pico y caída
        If t >= 0.15 Then
            scalePicoMano.ScaleX = 0 : scalePicoMano.ScaleY = 0 : scalePicoMano.ScaleZ = 0
            scalePicoSuelo.ScaleX = 1 : scalePicoSuelo.ScaleY = 1 : scalePicoSuelo.ScaleZ = 1

            Dim pDrop As Double = Math.Min(Math.Max((t - 0.15) / 0.2, 0.0), 1.0)
            Dim easeDrop = Math.Pow(pDrop, 2)
            posPicoSuelo.OffsetY = Lerp(posBaseY - 0.05, posBaseY - 0.45, easeDrop)
            rotPicoSuelo.Angle = Lerp(25.0, 85.0, easeDrop)
        Else
            scalePicoMano.ScaleX = 1 : scalePicoMano.ScaleY = 1 : scalePicoMano.ScaleZ = 1
            scalePicoSuelo.ScaleX = 0 : scalePicoSuelo.ScaleY = 0 : scalePicoSuelo.ScaleZ = 0
        End If

        ' FASE 2: CAMINATA HACIA ATRÁS (Limpia y sin atravesar el suelo)
        If t > 0.25 And t <= 0.6 Then
            Dim p2 As Double = (t - 0.25) / 0.35
            posPersonaje.OffsetX = Lerp(posBaseX, posBaseX - 0.6, p2)

            Dim velPasos As Double = p2 * Math.PI * 4 ' 2 pasos completos y limpios
            Dim senPasos As Double = Math.Sin(velPasos)

            ' Balanceo de piernas tipo tijera
            rotMusloIzq.Angle = senPasos * 20
            rotMusloDer.Angle = -senPasos * 20

            ' La rodilla solo se dobla cuando la pierna va hacia atrás
            rotRodillaIzq.Angle = If(senPasos > 0, senPasos * 30, 0)
            rotRodillaDer.Angle = If(senPasos < 0, Math.Abs(senPasos) * 30, 0)

            ' Mantenemos los tobillos a 0 para que no hagan cosas raras
            rotTobilloIzq.Angle = 0
            rotTobilloDer.Angle = 0

            ' Balanceo de brazos
            rotHombroDer.Angle = senPasos * 15 - 10
            rotHombroIzq.Angle = -senPasos * 15

            ' Rebote vertical (Absoluto y positivo para JAMÁS hundirse en el suelo)
            posPersonaje.OffsetY = posBaseY + Math.Abs(Math.Cos(velPasos)) * 0.02

        ElseIf t > 0.6 Then
            posPersonaje.OffsetX = posBaseX - 0.6
        End If

        ' FASE 3: SENTARSE COMO HUMANO (Sentadilla + Asimetría + Anti-Pop)
        If t > 0.6 And t <= 0.8 Then
            ' FASE 3A: Sentadilla (Baja el centro de gravedad sin despegar los pies)
            Dim p3a As Double = (t - 0.6) / 0.2
            p3a = p3a * p3a * (3 - 2 * p3a)

            rotMusloIzq.Angle = Lerp(0, -45.0, p3a)
            rotMusloDer.Angle = Lerp(0, -45.0, p3a)
            rotRodillaIzq.Angle = Lerp(0, 90.0, p3a)
            rotRodillaDer.Angle = Lerp(0, 90.0, p3a)

            ' Tobillos magnéticos anclados al suelo
            rotTobilloIzq.Angle = -(rotMusloIzq.Angle + rotRodillaIzq.Angle)
            rotTobilloDer.Angle = -(rotMusloDer.Angle + rotRodillaDer.Angle)

            ' Arranca desde +0.02 (el rebote final de la caminata) para evitar el primer "pop"
            posPersonaje.OffsetY = Lerp(posBaseY + 0.02, posBaseY - 0.2, p3a)

            rotTorso.Angle = Lerp(0, 20.0, p3a)
            rotCabeza.Angle = Lerp(0, -15.0, p3a)

            rotHombroDer.Angle = Lerp(-10.0, -30.0, p3a)
            rotHombroIzq.Angle = Lerp(0.0, -30.0, p3a)
            rotCodoDer.Angle = Lerp(0, 20.0, p3a)
            rotCodoIzq.Angle = Lerp(0, 20.0, p3a)

        ElseIf t > 0.8 Then
            ' FASE 3B: Sentarse en el suelo REAL (Adiós a la silla invisible)
            Dim p3b As Double = (t - 0.8) / 0.2
            Dim easeNormal As Double = p3b * p3b * (3 - 2 * p3b)

            Dim breathe As Double = 0.015 * Math.Sin(tiempoAnimacion * 2.0)
            Dim breatheBlend As Double = breathe * easeNormal

            ' 1. Bajamos la cadera a tope (-0.40) para que se siente de verdad en la plataforma
            posPersonaje.OffsetY = Lerp(posBaseY - 0.2, posBaseY - 0.4, easeNormal) + (breatheBlend * 0.5)

            ' 2. Llevamos las rodillas un poco más hacia el pecho (-110 grados)
            rotMusloIzq.Angle = Lerp(-45.0, -110.0, easeNormal)
            rotMusloDer.Angle = Lerp(-45.0, -110.0, easeNormal)

            ' Ligera apertura en "V" para más naturalidad
            rotMusloIzqY.Angle = Lerp(0, 15.0, easeNormal)
            rotMusloDerY.Angle = Lerp(0, -15.0, easeNormal)
            rotMusloIzqX.Angle = 0
            rotMusloDerX.Angle = 0

            ' 3. Doblamos más las rodillas (130) para que las botas bajen al nivel del suelo
            rotRodillaIzq.Angle = Lerp(90.0, 130.0, easeNormal)
            rotRodillaDer.Angle = Lerp(90.0, 130.0, easeNormal)

            ' 4. Pies planos
            rotTobilloIzq.Angle = Lerp(-45.0, -20.0, easeNormal)
            rotTobilloDer.Angle = Lerp(-45.0, -20.0, easeNormal)

            ' Torso relajado hacia adelante y brazos en su sitio
            rotTorso.Angle = Lerp(20.0, 10.0, easeNormal) + (breatheBlend * 100)
            rotCabeza.Angle = Lerp(-15.0, -5.0, easeNormal) - (breatheBlend * 50)

            rotHombroDer.Angle = Lerp(-30.0, -15.0, easeNormal) + (breatheBlend * 50)
            rotHombroIzq.Angle = Lerp(-30.0, -15.0, easeNormal) + (breatheBlend * 50)
            rotCodoDer.Angle = Lerp(20.0, 60.0, easeNormal)
            rotCodoIzq.Angle = Lerp(20.0, 60.0, easeNormal)
        End If
    End Sub

    Private Sub AnimarRespiracionSentado()
        Dim breathe As Double = 0.015 * Math.Sin(tiempoAnimacion * 2.0)

        rotTorso.Angle = 10.0 + (breathe * 100)
        rotCabeza.Angle = -5.0 - (breathe * 50)

        rotHombroDer.Angle = -15.0 + (breathe * 50)
        rotHombroIzq.Angle = -15.0 + (breathe * 50)

        ' Valores de la postura de rodillas al pecho
        rotMusloIzq.Angle = -110.0
        rotMusloDer.Angle = -110.0
        rotMusloIzqX.Angle = 0
        rotMusloDerX.Angle = 0
        rotMusloIzqY.Angle = 15.0
        rotMusloDerY.Angle = -15.0
        rotRodillaIzq.Angle = 130.0
        rotRodillaDer.Angle = 130.0
        rotTobilloIzq.Angle = -20.0
        rotTobilloDer.Angle = -20.0
        rotCodoDer.Angle = 60.0
        rotCodoIzq.Angle = 60.0

        ' Altura anclada al suelo
        posPersonaje.OffsetY = posBaseY - 0.4 + (breathe * 0.5)
        posPersonaje.OffsetX = posBaseX - 0.6
    End Sub

    Private Sub UpdateEscombrosPhys()
        For i As Integer = listaEscombros.Count - 1 To 0 Step -1
            Dim escombro = listaEscombros(i)
            Dim debrisVisual = CType(escombrosGroup.Children(escombro.VisualIdx), GeometryModel3D)
            escombro.Position += escombro.Velocity
            escombro.Velocity.Y += GRAVEDAD
            escombro.Velocity *= 0.97
            debrisVisual.Transform = New TranslateTransform3D(escombro.Position.X, escombro.Position.Y, escombro.Position.Z)
            escombro.TTL -= 1
            If escombro.TTL <= 0 Or escombro.Position.Y < -0.1 Then
                debrisVisual.Transform = New TranslateTransform3D(-999, -999, -999)
                listaEscombros.RemoveAt(i)
            End If
        Next
    End Sub
End Class
