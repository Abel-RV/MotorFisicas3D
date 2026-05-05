Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Partial Public Class clMinero
    Private _visual As ModelVisual3D
    Public ReadOnly Property Visual As ModelVisual3D
        Get
            Return _visual
        End Get
    End Property

    ' =========================================================
    ' CONFIGURACIÓN DE TIEMPO Y ESTADOS
    ' =========================================================
    Private Const TIEMPO_TRABAJO_SEGUNDOS As Double = 10.0

    Private Enum EstadoMinero
        Trabajando
        Transicion
        Descansando
    End Enum

    Private estado As EstadoMinero = EstadoMinero.Trabajando
    Private progresoTransicion As Double = 0.0
    Private cronometro As New System.Diagnostics.Stopwatch()

    ' =========================================================
    ' CONTROLADORES DEL RIG ESQUELÉTICO
    ' =========================================================
    Private posPersonaje As TranslateTransform3D
    Private posBaseX As Double
    Private posBaseY As Double
    Private posBaseZ As Double

    Private rotCintura As AxisAngleRotation3D
    Private rotTorso As AxisAngleRotation3D

    Private rotCabeza As AxisAngleRotation3D
    Private rotCabezaY As AxisAngleRotation3D

    Private rotHombroDer As AxisAngleRotation3D
    Private rotCodoDer As AxisAngleRotation3D
    Private rotHombroIzq As AxisAngleRotation3D
    Private rotCodoIzq As AxisAngleRotation3D

    ' --- PIERNAS ---
    Private rotMusloDer As AxisAngleRotation3D
    Private rotMusloDerX As AxisAngleRotation3D
    Private rotMusloDerY As AxisAngleRotation3D
    Private rotRodillaDer As AxisAngleRotation3D
    Private rotTobilloDer As AxisAngleRotation3D

    Private rotMusloIzq As AxisAngleRotation3D
    Private rotMusloIzqX As AxisAngleRotation3D
    Private rotMusloIzqY As AxisAngleRotation3D
    Private rotRodillaIzq As AxisAngleRotation3D
    Private rotTobilloIzq As AxisAngleRotation3D

    ' =========================================================
    ' SISTEMA DE OBJETOS Y EFECTOS
    ' =========================================================
    Private escombrosGroup As Model3DGroup
    Private listaEscombros As New List(Of Escombro)
    Private Const NUM_ESCOMBROS As Integer = 8
    Private Const GRAVEDAD As Double = -0.013
    Private Const VID_ESCOMBRO As Integer = 35
    Private Const ANGULO_IMPACTO_PICO As Double = 45.0

    ' =========================================================
    ' VALORES INICIALES GUARDADOS AL COMENZAR LA TRANSICIÓN
    ' =========================================================
    Private _tsHombroDer As Double = 100.0
    Private _tsCodoDer As Double = 40.0
    Private _tsHombroIzq As Double = 10.0
    Private _tsCodoIzq As Double = 20.0
    Private _tsTorso As Double = 15.0
    Private _tsCintura As Double = 20.0
    Private _tsCabeza As Double = -12.0
    Private _tsCabezaY As Double = -18.0
    Private _tsMusloIzq As Double = -5.0
    Private _tsRodillaIzq As Double = 7.5
    Private _tsMusloDer As Double = 5.0
    Private _tsRodillaDer As Double = 6.0

    Private scalePicoMano As ScaleTransform3D
    Private scalePicoSuelo As ScaleTransform3D
    Private posPicoSuelo As TranslateTransform3D
    Private rotPicoSuelo As AxisAngleRotation3D

    Private tiempoAnimacion As Double = 0
    Private rnd As New Random()
    Private posImpacto As Point3D

    Private Class Escombro
        Public VisualIdx As Integer
        Public Position As Point3D
        Public Velocity As Vector3D
        Public TTL As Integer
    End Class

    Public Sub New(x As Double, y As Double, z As Double)
        Dim grupoPrincipal As New Model3DGroup()

        Dim roca As GeometryModel3D = CrearRoca(0.35, Colors.DimGray)
        roca.Transform = New TranslateTransform3D(x + 0.8, y + 0.2, z)
        grupoPrincipal.Children.Add(roca)
        posImpacto = New Point3D(x + 0.75, y + 0.35, z)

        Dim colorPiel As Color = Color.FromRgb(255, 200, 160)
        Dim colorRopa As Color = Colors.RoyalBlue
        Dim colorPantalon As Color = Colors.DarkBlue
        Dim colorZapatos As Color = Color.FromRgb(40, 40, 40)

        Dim l_torso As Double = 0.35
        Dim l_muslo As Double = 0.2
        Dim l_pantorrilla As Double = 0.2
        Dim l_brazo As Double = 0.18
        Dim l_antebrazo As Double = 0.18

        posBaseX = x
        posBaseY = y + 0.42
        posBaseZ = z

        Dim personajeRootGroup As New Model3DGroup()
        posPersonaje = New TranslateTransform3D(posBaseX, posBaseY, posBaseZ)
        personajeRootGroup.Transform = posPersonaje

        ' --- PIERNA IZQUIERDA ---
        Dim musloIzqGroup As New Model3DGroup()
        rotMusloIzq = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        rotMusloIzqX = New AxisAngleRotation3D(New Vector3D(1, 0, 0), 0)
        rotMusloIzqY = New AxisAngleRotation3D(New Vector3D(0, 1, 0), 0)

        Dim tgMusloIzqGrp As New Transform3DGroup()
        tgMusloIzqGrp.Children.Add(New RotateTransform3D(rotMusloIzq))
        tgMusloIzqGrp.Children.Add(New RotateTransform3D(rotMusloIzqX))
        tgMusloIzqGrp.Children.Add(New RotateTransform3D(rotMusloIzqY))
        tgMusloIzqGrp.Children.Add(New TranslateTransform3D(0, -0.05, -0.08))
        musloIzqGroup.Transform = tgMusloIzqGrp
        musloIzqGroup.Children.Add(CrearHueso(0.045, l_muslo, colorPantalon))

        Dim pantorrillaIzqGroup As New Model3DGroup()
        rotRodillaIzq = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgRodillaIzqGrp As New Transform3DGroup()
        tgRodillaIzqGrp.Children.Add(New RotateTransform3D(rotRodillaIzq))
        tgRodillaIzqGrp.Children.Add(New TranslateTransform3D(0, -l_muslo, 0))
        pantorrillaIzqGroup.Transform = tgRodillaIzqGrp
        pantorrillaIzqGroup.Children.Add(CrearEsfera(0.045, colorPantalon, 16, 16))
        pantorrillaIzqGroup.Children.Add(CrearHueso(0.04, l_pantorrilla, colorPantalon))

        Dim pieIzq As GeometryModel3D = CrearEsfera(0.05, colorZapatos, 16, 16)
        rotTobilloIzq = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgPieIzq As New Transform3DGroup()
        tgPieIzq.Children.Add(New ScaleTransform3D(1.2, 0.8, 0.8))
        tgPieIzq.Children.Add(New RotateTransform3D(rotTobilloIzq))
        tgPieIzq.Children.Add(New TranslateTransform3D(0.05, -l_pantorrilla, 0))
        pieIzq.Transform = tgPieIzq
        pantorrillaIzqGroup.Children.Add(pieIzq)
        musloIzqGroup.Children.Add(pantorrillaIzqGroup)
        personajeRootGroup.Children.Add(musloIzqGroup)

        ' --- PIERNA DERECHA ---
        Dim musloDerGroup As New Model3DGroup()
        rotMusloDer = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        rotMusloDerX = New AxisAngleRotation3D(New Vector3D(1, 0, 0), 0)
        rotMusloDerY = New AxisAngleRotation3D(New Vector3D(0, 1, 0), 0)

        Dim tgMusloDerGrp As New Transform3DGroup()
        tgMusloDerGrp.Children.Add(New RotateTransform3D(rotMusloDer))
        tgMusloDerGrp.Children.Add(New RotateTransform3D(rotMusloDerX))
        tgMusloDerGrp.Children.Add(New RotateTransform3D(rotMusloDerY))
        tgMusloDerGrp.Children.Add(New TranslateTransform3D(0, -0.05, 0.08))
        musloDerGroup.Transform = tgMusloDerGrp
        musloDerGroup.Children.Add(CrearHueso(0.045, l_muslo, colorPantalon))

        Dim pantorrillaDerGroup As New Model3DGroup()
        rotRodillaDer = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgRodillaDerGrp As New Transform3DGroup()
        tgRodillaDerGrp.Children.Add(New RotateTransform3D(rotRodillaDer))
        tgRodillaDerGrp.Children.Add(New TranslateTransform3D(0, -l_muslo, 0))
        pantorrillaDerGroup.Transform = tgRodillaDerGrp
        pantorrillaDerGroup.Children.Add(CrearEsfera(0.045, colorPantalon, 16, 16))
        pantorrillaDerGroup.Children.Add(CrearHueso(0.04, l_pantorrilla, colorPantalon))

        Dim pieDer As GeometryModel3D = CrearEsfera(0.05, colorZapatos, 16, 16)
        rotTobilloDer = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgPieDer As New Transform3DGroup()
        tgPieDer.Children.Add(New ScaleTransform3D(1.2, 0.8, 0.8))
        tgPieDer.Children.Add(New RotateTransform3D(rotTobilloDer))
        tgPieDer.Children.Add(New TranslateTransform3D(0.05, -l_pantorrilla, 0))
        pieDer.Transform = tgPieDer
        pantorrillaDerGroup.Children.Add(pieDer)
        musloDerGroup.Children.Add(pantorrillaDerGroup)
        personajeRootGroup.Children.Add(musloDerGroup)

        ' --- CINTURA Y TORSO ---
        Dim cinturaGroup As New Model3DGroup()
        rotCintura = New AxisAngleRotation3D(New Vector3D(0, 1, 0), 0)
        cinturaGroup.Transform = New RotateTransform3D(rotCintura)
        cinturaGroup.Children.Add(CrearEsfera(0.09, colorPantalon, 16, 16))

        Dim torsoGroup As New Model3DGroup()
        rotTorso = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        torsoGroup.Transform = New RotateTransform3D(rotTorso)
        Dim pecho As GeometryModel3D = CrearHueso(0.12, l_torso, colorRopa)
        Dim tgPecho As New Transform3DGroup()
        tgPecho.Children.Add(New ScaleTransform3D(0.7, 1.0, 1.3))
        tgPecho.Children.Add(New RotateTransform3D(New AxisAngleRotation3D(New Vector3D(0, 0, 1), 180)))
        pecho.Transform = tgPecho
        torsoGroup.Children.Add(pecho)

        ' --- CABEZA ---
        Dim cabezaGroup As New Model3DGroup()
        rotCabeza = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        rotCabezaY = New AxisAngleRotation3D(New Vector3D(0, 1, 0), 0)
        Dim tgCabezaGrp As New Transform3DGroup()
        tgCabezaGrp.Children.Add(New RotateTransform3D(rotCabezaY))
        tgCabezaGrp.Children.Add(New RotateTransform3D(rotCabeza))
        tgCabezaGrp.Children.Add(New TranslateTransform3D(0, l_torso, 0))
        cabezaGroup.Transform = tgCabezaGrp
        Dim cuello As GeometryModel3D = CrearCilindro(0.03, 0.08, colorPiel, 16)
        cuello.Transform = New TranslateTransform3D(0, 0.04, 0)
        cabezaGroup.Children.Add(cuello)
        Dim cabeza As GeometryModel3D = CrearEsfera(0.11, colorPiel, 24, 24)
        cabeza.Transform = New TranslateTransform3D(0, 0.12, 0)
        cabezaGroup.Children.Add(cabeza)
        Dim ojoIzq As GeometryModel3D = CrearEsfera(0.012, Colors.Black, 16, 16)
        ojoIzq.Transform = New TranslateTransform3D(0.09, 0.14, -0.04)
        cabezaGroup.Children.Add(ojoIzq)
        Dim ojoDer As GeometryModel3D = CrearEsfera(0.012, Colors.Black, 16, 16)
        ojoDer.Transform = New TranslateTransform3D(0.09, 0.14, 0.04)
        cabezaGroup.Children.Add(ojoDer)
        Dim casco As GeometryModel3D = CrearEsfera(0.12, Colors.Gold, 24, 24)
        Dim tgCasco As New Transform3DGroup()
        tgCasco.Children.Add(New ScaleTransform3D(1.0, 0.7, 1.0))
        tgCasco.Children.Add(New TranslateTransform3D(0, 0.19, 0))
        casco.Transform = tgCasco
        cabezaGroup.Children.Add(casco)
        torsoGroup.Children.Add(cabezaGroup)

        ' --- BRAZO IZQUIERDO ---
        Dim brazoIzqGroup As New Model3DGroup()
        rotHombroIzq = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgHombroIzqGrp As New Transform3DGroup()
        tgHombroIzqGrp.Children.Add(New RotateTransform3D(rotHombroIzq))
        tgHombroIzqGrp.Children.Add(New TranslateTransform3D(0, l_torso - 0.05, -0.18))
        brazoIzqGroup.Transform = tgHombroIzqGrp
        brazoIzqGroup.Children.Add(CrearEsfera(0.05, colorRopa, 16, 16))
        brazoIzqGroup.Children.Add(CrearHueso(0.035, l_brazo, colorRopa))

        Dim antebrazoIzqGroup As New Model3DGroup()
        rotCodoIzq = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgCodoIzqGrp As New Transform3DGroup()
        tgCodoIzqGrp.Children.Add(New RotateTransform3D(rotCodoIzq))
        tgCodoIzqGrp.Children.Add(New TranslateTransform3D(0, -l_brazo, 0))
        antebrazoIzqGroup.Transform = tgCodoIzqGrp
        antebrazoIzqGroup.Children.Add(CrearEsfera(0.04, colorRopa, 16, 16))
        antebrazoIzqGroup.Children.Add(CrearHueso(0.03, l_antebrazo, colorPiel))
        Dim manoIzq As GeometryModel3D = CrearEsfera(0.04, colorPiel, 16, 16)
        manoIzq.Transform = New TranslateTransform3D(0, -l_antebrazo, 0)
        antebrazoIzqGroup.Children.Add(manoIzq)
        brazoIzqGroup.Children.Add(antebrazoIzqGroup)
        torsoGroup.Children.Add(brazoIzqGroup)

        ' --- BRAZO DERECHO ---
        Dim brazoDerGroup As New Model3DGroup()
        rotHombroDer = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgHombroDerGrp As New Transform3DGroup()
        tgHombroDerGrp.Children.Add(New RotateTransform3D(rotHombroDer))
        tgHombroDerGrp.Children.Add(New TranslateTransform3D(0, l_torso - 0.05, 0.18))
        brazoDerGroup.Transform = tgHombroDerGrp
        brazoDerGroup.Children.Add(CrearEsfera(0.05, colorRopa, 16, 16))
        brazoDerGroup.Children.Add(CrearHueso(0.035, l_brazo, colorRopa))

        Dim antebrazoDerGroup As New Model3DGroup()
        rotCodoDer = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgCodoDerGrp As New Transform3DGroup()
        tgCodoDerGrp.Children.Add(New RotateTransform3D(rotCodoDer))
        tgCodoDerGrp.Children.Add(New TranslateTransform3D(0, -l_brazo, 0))
        antebrazoDerGroup.Transform = tgCodoDerGrp
        antebrazoDerGroup.Children.Add(CrearEsfera(0.04, colorRopa, 16, 16))
        antebrazoDerGroup.Children.Add(CrearHueso(0.03, l_antebrazo, colorPiel))
        Dim manoDer As GeometryModel3D = CrearEsfera(0.04, colorPiel, 16, 16)
        manoDer.Transform = New TranslateTransform3D(0, -l_antebrazo, 0)
        antebrazoDerGroup.Children.Add(manoDer)

        ' --- PICO EN LA MANO ---
        Dim picoManoGroup As Model3DGroup = CrearModeloPicoGenerico()
        Dim tgPicoMano As New Transform3DGroup()
        tgPicoMano.Children.Add(New RotateTransform3D(New AxisAngleRotation3D(New Vector3D(0, 0, 1), 25)))
        tgPicoMano.Children.Add(New TranslateTransform3D(0, -l_antebrazo, 0))
        scalePicoMano = New ScaleTransform3D(1, 1, 1)
        tgPicoMano.Children.Add(scalePicoMano)
        picoManoGroup.Transform = tgPicoMano
        antebrazoDerGroup.Children.Add(picoManoGroup)

        brazoDerGroup.Children.Add(antebrazoDerGroup)
        torsoGroup.Children.Add(brazoDerGroup)

        cinturaGroup.Children.Add(torsoGroup)
        personajeRootGroup.Children.Add(cinturaGroup)
        grupoPrincipal.Children.Add(personajeRootGroup)

        ' --- PICO EN EL SUELO ---
        Dim picoSueloGroup As Model3DGroup = CrearModeloPicoGenerico()
        Dim tgPicoSuelo As New Transform3DGroup()
        rotPicoSuelo = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        tgPicoSuelo.Children.Add(New RotateTransform3D(rotPicoSuelo))
        tgPicoSuelo.Children.Add(New RotateTransform3D(New AxisAngleRotation3D(New Vector3D(1, 0, 0), -15)))
        posPicoSuelo = New TranslateTransform3D(posBaseX + 0.4, posBaseY, posBaseZ + 0.2)
        tgPicoSuelo.Children.Add(posPicoSuelo)
        scalePicoSuelo = New ScaleTransform3D(0, 0, 0)
        tgPicoSuelo.Children.Add(scalePicoSuelo)
        picoSueloGroup.Transform = tgPicoSuelo
        grupoPrincipal.Children.Add(picoSueloGroup)

        ' --- SISTEMA DE PARTÍCULAS ---
        escombrosGroup = New Model3DGroup()
        For i As Integer = 0 To NUM_ESCOMBROS
            Dim shard As GeometryModel3D = CrearEsfera(0.02, Colors.DimGray, 8, 8)
            shard.Transform = New TranslateTransform3D(-999, -999, -999)
            escombrosGroup.Children.Add(shard)
        Next
        grupoPrincipal.Children.Add(escombrosGroup)

        _visual = New ModelVisual3D() With {.Content = grupoPrincipal}
        cronometro.Start()
    End Sub
End Class