Imports System.Windows.Media
Imports System.Windows.Media.Media3D

Public Class clFuego
    Public ReadOnly Property Visual As ModelVisual3D

    Private Const NUM_PARTICULAS As Integer = 80 ' Cantidad de partículas de la llama
    Private _modelos As GeometryModel3D()
    Private _traslaciones As TranslateTransform3D()
    Private _escalas As ScaleTransform3D()

    Private _posX As Double()
    Private _posY As Double()
    Private _posZ As Double()
    Private _velY As Double()
    Private _velX As Double()
    Private _velZ As Double()
    Private _vida As Double()
    Private _vidaMaxima As Double()

    Private _rnd As New Random()
    Private _origenX As Double
    Private _origenY As Double
    Private _origenZ As Double

    ' --- Propiedades públicas para detectar colisiones (como quemar la cuerda) ---
    Public ReadOnly Property PosX As Double
        Get
            Return _origenX
        End Get
    End Property
    Public ReadOnly Property PosY As Double
        Get
            Return _origenY
        End Get
    End Property
    Public ReadOnly Property PosZ As Double
        Get
            Return _origenZ
        End Get
    End Property
    ' ----------------------------------------------------------------------------

    Public Sub New(origenX As Double, origenY As Double, origenZ As Double)
        _origenX = origenX
        _origenY = origenY
        _origenZ = origenZ

        Visual = New ModelVisual3D()
        Dim grupoModelos As New Model3DGroup()

        ' Inicializar arrays
        ReDim _modelos(NUM_PARTICULAS - 1)
        ReDim _traslaciones(NUM_PARTICULAS - 1)
        ReDim _escalas(NUM_PARTICULAS - 1)
        ReDim _posX(NUM_PARTICULAS - 1) : ReDim _posY(NUM_PARTICULAS - 1) : ReDim _posZ(NUM_PARTICULAS - 1)
        ReDim _velY(NUM_PARTICULAS - 1) : ReDim _velX(NUM_PARTICULAS - 1) : ReDim _velZ(NUM_PARTICULAS - 1)
        ReDim _vida(NUM_PARTICULAS - 1) : ReDim _vidaMaxima(NUM_PARTICULAS - 1)

        ' Usamos un cubo simple como partícula (bajo coste de rendimiento)
        Dim meshBase = CrearCubo(0.08)

        ' Crear materiales brillantes (EmissiveMaterial simula que emiten luz)
        Dim matAmarillo As New MaterialGroup()
        matAmarillo.Children.Add(New DiffuseMaterial(New SolidColorBrush(Colors.Yellow) With {.Opacity = 0.9}))
        matAmarillo.Children.Add(New EmissiveMaterial(New SolidColorBrush(Color.FromRgb(255, 255, 100))))

        Dim matNaranja As New MaterialGroup()
        matNaranja.Children.Add(New DiffuseMaterial(New SolidColorBrush(Colors.DarkOrange) With {.Opacity = 0.8}))
        matNaranja.Children.Add(New EmissiveMaterial(New SolidColorBrush(Colors.Orange)))

        Dim matRojo As New MaterialGroup()
        matRojo.Children.Add(New DiffuseMaterial(New SolidColorBrush(Colors.Red) With {.Opacity = 0.7}))
        matRojo.Children.Add(New EmissiveMaterial(New SolidColorBrush(Color.FromRgb(200, 0, 0))))

        For i As Integer = 0 To NUM_PARTICULAS - 1
            Dim mat As MaterialGroup
            ' Repartir colores: mucho naranja, algo de amarillo y rojo
            Dim randColor As Double = _rnd.NextDouble()
            If randColor < 0.3 Then
                mat = matAmarillo
            ElseIf randColor < 0.8 Then
                mat = matNaranja
            Else
                mat = matRojo
            End If

            _modelos(i) = New GeometryModel3D(meshBase, mat)
            _modelos(i).BackMaterial = mat

            _traslaciones(i) = New TranslateTransform3D(0, 0, 0)
            _escalas(i) = New ScaleTransform3D(1, 1, 1)

            Dim tg As New Transform3DGroup()
            tg.Children.Add(_escalas(i))
            tg.Children.Add(_traslaciones(i))
            _modelos(i).Transform = tg

            grupoModelos.Children.Add(_modelos(i))

            RenacerParticula(i)
            ' Desfasar la vida inicial para que no nazcan todas a la vez al arrancar
            _vida(i) = _rnd.NextDouble() * _vidaMaxima(i)
        Next

        Visual.Content = grupoModelos
    End Sub

    Private Sub RenacerParticula(i As Integer)
        ' Nace en una posición aleatoria alrededor del origen (base de la llama)
        _posX(i) = _origenX + (_rnd.NextDouble() - 0.5) * 0.3
        _posY(i) = _origenY + (_rnd.NextDouble() * 0.1)
        _posZ(i) = _origenZ + (_rnd.NextDouble() - 0.5) * 0.3

        ' Velocidad ascendente y ligera dispersión lateral
        _velY(i) = 0.02 + (_rnd.NextDouble() * 0.04)
        _velX(i) = (_rnd.NextDouble() - 0.5) * 0.01
        _velZ(i) = (_rnd.NextDouble() - 0.5) * 0.01

        _vida(i) = 0
        _vidaMaxima(i) = 20 + _rnd.Next(35) ' Entre 20 y 55 frames de vida
    End Sub

    Public Sub animar()
        For i As Integer = 0 To NUM_PARTICULAS - 1
            _vida(i) += 1

            If _vida(i) >= _vidaMaxima(i) Then
                RenacerParticula(i)
            Else
                ' 1. Movimiento natural del calor (hacia arriba)
                _posX(i) += _velX(i)
                _posY(i) += _velY(i)
                _posZ(i) += _velZ(i)

                ' 2. Física de Llama: Converger hacia el centro mientras sube (forma de lágrima/cono)
                _posX(i) += (_origenX - _posX(i)) * 0.06
                _posZ(i) += (_origenZ - _posZ(i)) * 0.06

                ' 3. Achicarse a medida que se enfría/consume
                Dim factorEscala As Double = 1.0 - (_vida(i) / _vidaMaxima(i))
                _escalas(i).ScaleX = factorEscala
                _escalas(i).ScaleY = factorEscala
                _escalas(i).ScaleZ = factorEscala
            End If

            _traslaciones(i).OffsetX = _posX(i)
            _traslaciones(i).OffsetY = _posY(i)
            _traslaciones(i).OffsetZ = _posZ(i)
        Next
    End Sub

    ' Utilidad para dibujar un cubo rápido y de pocos polígonos
    Private Function CrearCubo(tamano As Double) As MeshGeometry3D
        Dim mesh As New MeshGeometry3D()
        Dim w As Double = tamano / 2

        Dim p0 As New Point3D(-w, -w, w) : Dim p1 As New Point3D(w, -w, w)
        Dim p2 As New Point3D(w, w, w) : Dim p3 As New Point3D(-w, w, w)
        Dim p4 As New Point3D(-w, -w, -w) : Dim p5 As New Point3D(w, -w, -w)
        Dim p6 As New Point3D(w, w, -w) : Dim p7 As New Point3D(-w, w, -w)

        AddCara(mesh, p0, p1, p2, p3) ' Frente
        AddCara(mesh, p5, p4, p7, p6) ' Atrás
        AddCara(mesh, p3, p2, p6, p7) ' Arriba
        AddCara(mesh, p4, p5, p1, p0) ' Abajo
        AddCara(mesh, p4, p0, p3, p7) ' Izquierda
        AddCara(mesh, p1, p5, p6, p2) ' Derecha

        Return mesh
    End Function

    Private Sub AddCara(mesh As MeshGeometry3D, p1 As Point3D, p2 As Point3D, p3 As Point3D, p4 As Point3D)
        Dim idx As Integer = mesh.Positions.Count
        mesh.Positions.Add(p1) : mesh.Positions.Add(p2) : mesh.Positions.Add(p3) : mesh.Positions.Add(p4)
        mesh.TriangleIndices.Add(idx) : mesh.TriangleIndices.Add(idx + 1) : mesh.TriangleIndices.Add(idx + 2)
        mesh.TriangleIndices.Add(idx + 2) : mesh.TriangleIndices.Add(idx + 3) : mesh.TriangleIndices.Add(idx)
    End Sub
End Class