Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Public Class clVentilador

#Region "Variables"
    Public ReadOnly Property visual As ModelVisual3D

    Public ReadOnly Property posicionCabeza As Point3D
        Get
            Dim baseTransfor = DirectCast(visual.Transform, TranslateTransform3D)
            Return New Point3D(baseTransfor.OffsetX, baseTransfor.OffsetY, baseTransfor.OffsetZ)
        End Get
    End Property

    Public ReadOnly Property direccionViento As Point3D
        Get
            Dim radianes As Double = rotacionCabeza.Angle * Math.PI / 180.0
            Return New Vector3D(Math.Sin(radianes), 0, Math.Cos(radianes))
        End Get
    End Property

    Private cabeza As ModelVisual3D
    Private aspasVisual As ModelVisual3D

    Private rotacionCabeza As AxisAngleRotation3D
    Private rotacionAspas As AxisAngleRotation3D

    Private tiempoOscilacion As Double = 0

    Private vientoTransForms As New List(Of TranslateTransform3D)
    Private rndViento As New Random()

#End Region

#Region "constructor"
    Public Sub New(x As Double, y As Double, z As Double)
        visual = New ModelVisual3D
        Dim tranformacionbase As New TranslateTransform3D(x, y, z)
        visual.Transform = tranformacionbase

        Dim modeloBase = CrearCaja(0.6, 0.1, 0.6, Colors.DarkSlateGray)
        Dim modeloPoste = CrearCaja(0.1, 1.5, 0.1, Colors.Silver)
        Dim tgPoste As New TranslateTransform3D(0, 0.75, 0)
        modeloPoste.Transform = tgPoste

        visual.Children.Add(New ModelVisual3D With {.Content = modeloBase})
        visual.Children.Add(New ModelVisual3D With {.Content = modeloPoste})

        cabeza = New ModelVisual3D()
        Dim modeloMotor = CrearCaja(0.3, 0.3, 0.4, Colors.DarkSlateGray)
        cabeza.Content = modeloMotor

        rotacionCabeza = New AxisAngleRotation3D(New Vector3D(0, 1, 0), 0)
        Dim tgCabeza As New Transform3DGroup()
        tgCabeza.Children.Add(New RotateTransform3D(rotacionCabeza))
        tgCabeza.Children.Add(New TranslateTransform3D(0, 1.6, 0))
        cabeza.Transform = tgCabeza
        visual.Children.Add(cabeza)

        aspasVisual = New ModelVisual3D()

        Dim grupoAspas As New Model3DGroup()

        grupoAspas.Children.Add(CrearCaja(0.9, 0.1, 0.05, Colors.LightGray))
        grupoAspas.Children.Add(CrearCaja(0.1, 0.9, 0.05, Colors.LightGray))
        grupoAspas.Children.Add(CrearCaja(0.15, 0.15, 0.1, Colors.Black))

        aspasVisual.Content = grupoAspas

        rotacionAspas = New AxisAngleRotation3D(New Vector3D(0, 0, 1), 0)
        Dim tgAspas As New Transform3DGroup()
        tgAspas.Children.Add(New RotateTransform3D(rotacionAspas))
        tgAspas.Children.Add(New TranslateTransform3D(0, 0, 0.25))

        aspasVisual.Transform = tgAspas

        Dim vientoVisual As New ModelVisual3D

        Dim meshViento = CrearCaja(0.015, 0.015, 0.4, Colors.White)

        For i As Integer = 0 To 8
            Dim linea As New ModelVisual3D() With {.Content = meshViento}

            ' Posicionarlas en un radio alrededor del centro de las aspas
            Dim radio As Double = rndViento.NextDouble() * 0.4 + 0.1
            Dim angulo As Double = rndViento.NextDouble() * Math.PI * 2
            Dim px As Double = Math.Cos(angulo) * radio
            Dim py As Double = Math.Sin(angulo) * radio
            Dim pz As Double = rndViento.NextDouble() * 2.0 ' Repartidas hacia adelante

            Dim tgLinea As New TranslateTransform3D(px, py, pz)
            linea.Transform = tgLinea

            vientoTransForms.Add(tgLinea)
            vientoVisual.Children.Add(linea)
        Next

        aspasVisual.Children.Add(vientoVisual)
        cabeza.Children.Add(aspasVisual)

    End Sub
#End Region

#Region "PUBLIC SUB animar"
    ''' <summary>
    ''' Metodo que se encarga de animar las aspas del ventilador girando
    ''' </summary>
    Public Sub animar()
        rotacionAspas.Angle += 25

        If rotacionAspas.Angle >= 360 Then
            rotacionAspas.Angle -= 360
        End If

        tiempoOscilacion += 0.02
        rotacionCabeza.Angle = Math.Sin(tiempoOscilacion) * 50

        ' --- NUEVO: MOVER LAS LÍNEAS DE VIENTO ---
        For Each tg In vientoTransForms
            tg.OffsetZ += 0.02 ' Velocidad a la que sale el viento hacia adelante

            ' Si la línea se aleja mucho, la regresamos a la base de las aspas
            If tg.OffsetZ > 2.5 Then
                tg.OffsetZ = 0.1

                ' Opcional: Recalcular su posición X e Y para que no salga siempre del mismo sitio
                Dim radio As Double = rndViento.NextDouble() * 0.4 + 0.1
                Dim angulo As Double = rndViento.NextDouble() * Math.PI * 2
                tg.OffsetX = Math.Cos(angulo) * radio
                tg.OffsetY = Math.Sin(angulo) * radio
            End If
        Next
    End Sub
#End Region

#Region "PRIVATE SUB crearCaja(ancho , alto, profundidad, color)"
    ''' <summary>
    ''' Funcion que crea el cuerpo del ventidador
    ''' </summary>
    ''' <param name="ancho">El ancho de la base</param>
    ''' <param name="alto">El alto de la base</param>
    ''' <param name="prof">La profundidad de la base</param>
    ''' <param name="col">Color de la base</param>
    ''' <returns>Retorna el modelo creado</returns>
    ''' 
    Private Function CrearCaja(ancho As Double, alto As Double, prof As Double, col As Color) As GeometryModel3D
        Dim mesh As New MeshGeometry3D()
        Dim w As Double = ancho / 2
        Dim h As Double = alto / 2
        Dim d As Double = prof / 2

        ' Helper para añadir caras asegurando normales correctas para la luz
        AddCara(mesh, New Point3D(-w, h, d), New Point3D(-w, -h, d), New Point3D(w, -h, d), New Point3D(w, h, d)) ' Frente
        AddCara(mesh, New Point3D(w, h, -d), New Point3D(w, -h, -d), New Point3D(-w, -h, -d), New Point3D(-w, h, -d)) ' Atrás
        AddCara(mesh, New Point3D(-w, h, -d), New Point3D(-w, h, d), New Point3D(w, h, d), New Point3D(w, h, -d)) ' Arriba
        AddCara(mesh, New Point3D(-w, -h, d), New Point3D(-w, -h, -d), New Point3D(w, -h, -d), New Point3D(w, -h, d)) ' Abajo
        AddCara(mesh, New Point3D(-w, h, -d), New Point3D(-w, -h, -d), New Point3D(-w, -h, d), New Point3D(-w, h, d)) ' Izquierda
        AddCara(mesh, New Point3D(w, h, d), New Point3D(w, -h, d), New Point3D(w, -h, -d), New Point3D(w, h, -d)) ' Derecha

        Dim material As New DiffuseMaterial(New SolidColorBrush(col))
        Dim grupoMateriales As New MaterialGroup()
        grupoMateriales.Children.Add(material)
        grupoMateriales.Children.Add(New SpecularMaterial(New SolidColorBrush(Colors.White), 30.0))

        Return New GeometryModel3D(mesh, grupoMateriales)
    End Function
#End Region

#Region "PRIVATE SUB AddCara()"
    ''' <summary>
    ''' Función que se encarga de crear las caras de la base
    ''' </summary>
    ''' <param name="mesh">La malla de la base</param>
    ''' <param name="p1">Punto 1 de la base</param>
    ''' <param name="p2">Punto 2 de la base</param>
    ''' <param name="p3">Punto 3 de la base</param>
    ''' <param name="p4">Punto 4 de la base</param>
    Private Sub AddCara(mesh As MeshGeometry3D, p1 As Point3D, p2 As Point3D, p3 As Point3D, p4 As Point3D)
        Dim idx As Integer = mesh.Positions.Count
        mesh.Positions.Add(p1) : mesh.Positions.Add(p2) : mesh.Positions.Add(p3) : mesh.Positions.Add(p4)
        mesh.TriangleIndices.Add(idx) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(idx + 2)
        mesh.TriangleIndices.Add(idx + 2) : mesh.TriangleIndices.Add(idx + 3) : mesh.TriangleIndices.Add(idx)
    End Sub

#End Region

End Class
