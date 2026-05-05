# MotorFisicas3D

Aplicación de escritorio desarrollada en **Visual Basic .NET** que simula en tiempo real múltiples fenómenos físicos tridimensionales: fluidos viscosos, cuerdas con quemado, cadenas metálicas rígidas, tela, fuego por partículas, viento y un personaje animado con rig esquelético, todo ello renderizado con WPF 3D.

---

## 📋 Descripción

MotorFisicas3D es un motor de físicas tridimensional orientado a la demostración y el aprendizaje. Todos los sistemas corren simultáneamente en una única escena interactiva: puedes arrastrar la cuerda y la cadena con el ratón, observar cómo el fuego quema y rompe la cuerda segmento a segmento, ver cómo el líquido viscoso cae en cascada dentro del tanque y cómo la tela se deforma sobre él, y contemplar al minero animado que pica roca, se cansa y se sienta a descansar.

---

## ✨ Características

- Simulación de fluido viscoso con gotas esféricas, colisiones y superficie dinámica
- Cuerda con física Verlet, interacción con viento y sistema de quemado/rotura realista
- Cadena metálica rígida con eslabones 3D matemáticamente exactos y auto-colisión
- Sistema de partículas de fuego con colores dinámicos y propagación de llama
- Tela/tejido que cae sobre el tanque y se moldea a su geometría
- Ventilador oscilante que genera una fuerza de viento direccional en la escena
- Personaje 3D (minero) con rig esquelético completo y máquina de estados (trabajar → transición → descansar)
- Cámara orbital con ratón (orbitar, panear, zoom con rueda)
- Escena iluminada con luz direccional principal, contraluz cálida y luz ambiental

---

## 🛠️ Tecnologías utilizadas

| Tecnología       | Detalle                                          |
|------------------|--------------------------------------------------|
| Lenguaje         | Visual Basic .NET                                |
| Plataforma       | .NET Framework con WPF (`System.Windows.Media.Media3D`) |
| Tipo de proyecto | Aplicación de escritorio Windows Forms + WPF     |
| Renderizado 3D   | `Viewport3D`, `MeshGeometry3D`, `GeometryModel3D`|

---

## ⚙️ Requisitos previos

- [Visual Studio](https://visualstudio.microsoft.com/) 2019 o superior
- .NET Framework 4.7.2 o compatible (el que requiera el `.vbproj`)
- SDK de WPF incluido en la instalación de Visual Studio (carga de trabajo *Desarrollo de escritorio .NET*)

---

## 🚀 Instalación y ejecución

1. **Clona el repositorio:**

   ```bash
   git clone https://github.com/Abel-RV/MotorFisicas3D.git
   ```

2. **Abre la solución:**

   Abre `Entorno3D.sln` con Visual Studio.

3. **Compila el proyecto:**

   `Compilar › Compilar solución` o `Ctrl+Shift+B`.

4. **Ejecuta la aplicación:**

   Pulsa `F5` o el botón **Iniciar**.

---

## 🎮 Controles

| Acción | Descripción |
|---|---|
| **Botón derecho + arrastrar** | Orbitar la cámara alrededor de la escena |
| **Shift + Botón derecho + arrastrar** | Panear la cámara lateralmente |
| **Rueda del ratón** | Zoom in/out (rango: 2 – 25 unidades) |
| **Botón izquierdo sobre la cuerda** | Agarrar y arrastrar un nodo de la cuerda |
| **Botón izquierdo sobre la cadena** | Agarrar y arrastrar un eslabón de la cadena |

---

## 🗂️ Estructura del proyecto

```
Entorno3D.sln
└── Entorno3D/
    ├── FormTresDe.vb              ← Formulario principal y bucle de simulación
    ├── Forms/
    │   └── FormTresDe.vb          ← Diseñador WPF del formulario
    ├── Clasess/
    │   ├── clCilindro.vb          ← Tanque cilíndrico hueco
    │   ├── clCuerda.vb            ← Cuerda física con quemado
    │   ├── clCadena.vb            ← Cadena metálica rígida
    │   ├── clFuego.vb             ← Sistema de partículas de fuego
    │   ├── clGota.vb              ← Gota de fluido viscoso
    │   ├── clSuperficieAgua.vb    ← Superficie dinámica del líquido
    │   ├── clTela.vb              ← Tela/tejido deformable
    │   └── clVentilador.vb        ← Ventilador oscilante con viento
    └── Minero/
        ├── clMinero.vb            ← Construcción del personaje y estado
        ├── clMinero.Animacion.vb  ← Máquina de estados y animaciones
        └── clMinero.Geometria.vb  ← Primitivas geométricas del personaje
```

---

## 📦 Documentación de clases

### `FormTresDe` — Formulario principal y bucle de simulación
**Archivo:** `Entorno3D/FormTresDe.vb`

Es el punto de entrada visual de la aplicación. Contiene el `Viewport3D` de WPF embebido en un `ElementHost` de Windows Forms. Gestiona:

- **Inicialización de la escena** (`FormTresDe_Load`): crea el suelo, instancia todas las clases de simulación, configura las luces (luz direccional principal blanca, contraluz cálida roja y luz ambiental azulada) y registra los eventos del ratón.
- **Bucle de animación** (`animTimer_Tick`, cada 10 ms): ejecuta en orden la emisión de gotas, física de fluidos, actualización de la superficie viscosa, deformación de la tela, animación del ventilador, simulación de cuerda y cadena, animación del fuego y del minero, y la detección de colisión fuego–cuerda.
- **Cámara orbital** (`ActualizarCamara`): convierte ángulos esféricos (yaw, pitch, distancia) a coordenadas cartesianas y actualiza la `PerspectiveCamera`.
- **Interacción con ratón** (`Grid_MouseDown/Move/Up/Wheel`): selecciona el nodo más cercano al punto de impacto para arrastrar la cuerda o la cadena; el botón derecho orbita u hace paneo según si se mantiene Shift.

---

### `clCilindro` — Tanque cilíndrico hueco
**Archivo:** `Clasess/clCilindro.vb`

Genera la malla 3D de un cilindro hueco (como un tanque) con radio exterior, radio interior y altura configurables. Internamente construye 40 segmentos radiales con caras laterales externas, internas y tapas superior e inferior.

**Propiedades principales:**
| Propiedad | Descripción |
|---|---|
| `Visual` | `ModelVisual3D` listo para añadir al viewport |
| `PosX/Y/Z` | Posición actual del cilindro en el espacio 3D |
| `EscalaActual` | Factor de escala uniforme aplicado |

**Métodos:**
- `posicionar(x, y, z)` — Desplaza el cilindro mediante `TranslateTransform3D`.
- `escalar(factor)` — Escala uniformemente el cilindro mediante `ScaleTransform3D`.

En la escena hay dos instancias: el **cilindro interno** (el tanque de color naranja que contiene el fluido) y el **cilindro externo** semitransparente (la pared exterior azul).

---

### `clCuerda` — Cuerda física con quemado realista
**Archivo:** `Clasess/clCuerda.vb`

Simula una cuerda de 25 nodos usando **integración de Verlet con posiciones**. Cada nodo almacena posición actual y posición anterior; la velocidad se deduce implícitamente en cada paso.

**Variables de estado:**
- `vidaNodo(i)` — Nivel de vida de cada segmento (1.0 = intacto, 0.0 = roto/quemado). Cuando llega a 0, ese segmento desaparece visualmente y sus vecinos se desconectan físicamente.
- `enLlamas(i)` — Indica si ese nodo está ardiendo. El fuego se propaga automáticamente a los vecinos cuando la vida baja de 0.6.

**Método `simularFisica`:** aplica en cada frame:
1. **Fuerzas:** gravedad (−0.005/frame en Y) y viento direccional del ventilador (solo si el nodo está dentro del cono de influencia a 8 unidades).
2. **Colisiones:** suelo plano a Y = −1.25, pared cilíndrica del tanque (interior y exterior) con resolución de penetración.
3. **20 iteraciones de restricciones:** rigidez blanda (distancia de dos nodos), longitud de segmento (rotura si `vidaNodo ≤ 0`) y auto-colisión suave entre nodos no adyacentes.
4. **Actualización de malla:** genera un tubo 3D con radio variable (más fino donde está quemado). El color del material se oscurece progresivamente según el nodo con menos vida.

La cuerda puede **agarrarse con el ratón**: el nodo más cercano al punto de clic queda anclado a la posición del cursor en el espacio 3D.

---

### `clCadena` — Cadena metálica rígida
**Archivo:** `Clasess/clCadena.vb`

Simula una cadena de 25 nodos de acero usando integración de Verlet, pero con **60 iteraciones de restricciones** por frame para lograr rigidez real de metal.

**Diferencias clave respecto a la cuerda:**
- No tiene sistema de quemado.
- La fricción es mayor (coeficiente 0.99) para simular el peso del metal.
- El límite de velocidad está acotado a 0.5 unidades/frame para evitar inestabilidad.
- Los eslabones se generan matemáticamente como **toroides ovalados** alternando 90° de rotación para que encajen entre sí (`GenerarEslabonOvalado`). La fórmula de `zOffset` asegura que el borde interior de cada eslabón descanse exactamente sobre el nodo contiguo.
- Incluye **auto-colisión** entre nodos no adyacentes para evitar que la cadena se atraviese a sí misma.

El nodo agarrado se puede mover con el ratón igual que en la cuerda.

---

### `clFuego` — Sistema de partículas de fuego
**Archivo:** `Clasess/clFuego.vb`

Genera 80 partículas cúbicas animadas que simulan una llama. Cada partícula tiene:

- **Ciclo de vida** aleatorio entre 20 y 55 frames.
- **Color** distribuido: 30% amarillo, 50% naranja, 20% rojo. Todos usan `EmissiveMaterial` para simular emisión de luz propia.
- **Física de llama:** en cada frame (`animar`) la partícula sube (velocidad Y positiva), se desplaza lateralmente con ruido pequeño y converge hacia el eje central con factor 0.06 — esto crea la característica forma cónica de la llama.
- **Escala decreciente:** la partícula se encoge a medida que se consume (`factorEscala = 1 − vida/vidaMaxima`).

Al renacer, cada partícula reaparece en la base de la llama con posición y velocidad aleatorias, desfasadas para que el efecto sea continuo desde el primer frame.

Las propiedades `PosX/Y/Z` exponen la posición del origen de la llama para que `FormTresDe` pueda calcular la distancia a los nodos de la cuerda y activar el quemado.

---

### `clGota` — Gota de fluido viscoso
**Archivo:** `Clasess/clGota.vb`

Representa una gota esférica (malla de 10×10 paralelos/meridianos, radio 0.08) del líquido del tanque. Expone posición y velocidad como campos públicos para que `FormTresDe` aplique directamente la física (gravedad, rebote en el fondo, colisión con la pared cilíndrica e interacciones de viscosidad entre gotas vecinas).

**Método `ActualizarGrafico`:** actualiza la translación y aplica una deformación de escala que aplana la gota lateralmente cuando cae rápido (simula la elongación aerodinámica de un fluido viscoso), y la oculta cuando su velocidad baja de 0.025 (la gota se "funde" con la superficie).

---

### `clSuperficieAgua` — Superficie dinámica del líquido
**Archivo:** `Clasess/clSuperficieAgua.vb`

Crea una malla plana de 35×35 vértices que cubre el interior del tanque. En cada frame (`GenerarSuperficie`) recalcula la altura de cada vértice sumando la influencia de todas las gotas cercanas que ya se han posado (velocidad Y > −0.05):

- **Radio de efecto:** 0.2 unidades por gota.
- **Perfil de influencia:** función `(1 − distancia/radio)^1.5` para lograr montículos suaves y abombados donde aterrizan las gotas.
- Los vértices fuera del radio del tanque se proyectan sobre el borde para que la superficie nunca sobresalga del cilindro.

El resultado es una superficie viscosa y espesa que acumula volumen con cada gota.

---

### `clTela` — Tela/tejido deformable
**Archivo:** `Clasess/clTela.vb`

Genera una cuadrícula de 30×30 puntos (lado 4 unidades) con doble cara (visible por arriba y por abajo). En cada frame (`DeformarYCaer`) cada vértice:

1. Recibe una aceleración constante de gravedad (−0.015/frame).
2. Se restringe según su posición relativa al tanque:
   - **Dentro del radio interno** del tanque: cae siguiendo una curva cuadrática desde el borde hasta el fondo interno.
   - **Sobre la pared del tanque** (entre radio interno y externo): se detiene en la altura del borde superior.
   - **Fuera del tanque:** cae siguiendo una curva cuadrática hasta el nivel del suelo externo.
3. La velocidad se pone a cero cuando el vértice toca su límite, para que la tela quede moldeada sobre la geometría del tanque.

---

### `clVentilador` — Ventilador oscilante
**Archivo:** `Clasess/clVentilador.vb`

Construye un ventilador articulado jerárquicamente:
- **Base:** caja gris oscura.
- **Poste:** caja plateada vertical.
- **Cabeza (motor):** caja oscura que oscila en Y con `Math.Sin(tiempo) × 50°`.
- **Aspas:** dos barras cruzadas de 0.9 unidades que giran 25°/frame alrededor del eje Z.
- **Líneas de viento:** 9 pequeñas cajas blancas que avanzan desde las aspas hacia adelante y se reciclan al superar 2.5 unidades, con posición aleatoria en cada ciclo para dar sensación de turbulencia.

**Propiedades:**
- `posicionCabeza` — Posición 3D del motor (origen del viento).
- `direccionViento` — Vector unitario calculado del ángulo de oscilación de la cabeza.

`FormTresDe` usa estas dos propiedades junto con una fuerza fija de 0.025 para aplicar el viento sobre cada nodo de la cuerda dentro del cono de influencia.

---

### `clMinero` — Personaje 3D animado con rig esquelético
**Archivos:** `Minero/clMinero.vb`, `clMinero.Animacion.vb`, `clMinero.Geometria.vb`

Personaje humanoide completo dividido en tres archivos (`Partial Class`):

#### `clMinero.vb` — Construcción del personaje
Instancia toda la jerarquía de `Model3DGroup` (piernas → cintura → torso → cabeza/brazos) usando las primitivas de geometría. Cada articulación tiene su propio `AxisAngleRotation3D` para poder ser animada independientemente. También crea:
- El pico en la mano derecha (visible durante la minería) y el pico en el suelo (visible durante la transición).
- El sistema de partículas de escombros (8 esferas pequeñas grises que salen disparadas al impactar el pico).

#### `clMinero.Animacion.vb` — Máquina de estados y animaciones
Gestiona tres estados con `cronometro` para temporizar:

| Estado | Descripción |
|---|---|
| `Trabajando` | Anima el ciclo de minería durante 10 segundos |
| `Transicion` | Secuencia de 4 fases para bajar el pico, caminar hacia atrás y sentarse |
| `Descansando` | Respiración sutil en posición sentada |

- **`AnimarMineria(cycle)`:** ciclo de 1.0 s con función de easing (`ObtenerEstadoAnimacion`) que calcula un perfil asimétrico (bajada lenta → impacto rápido → recuperación), aplicado a los ángulos de hombro, codo, torso, cintura, cabeza y piernas. Al frame de impacto (cycle ≈ 0.6) lanza los escombros.
- **`AnimarSecuenciaSentarse(t)`:** cuatro fases interpoladas con `Lerp`: (1) estabilizar postura, (1.5) intercambiar pico mano→suelo, (2) caminata hacia atrás con balanceo de tijera, (3a) sentadilla, (3b) sentarse en el suelo con piernas cruzadas y apertura en V.
- **`AnimarRespiracionSentado`:** oscilación sinusoidal muy suave (amplitud 0.015) en torso, cabeza y hombros.
- **`UpdateEscombrosPhys`:** cada escombro vuela con su velocidad inicial, recibe gravedad (−0.013/frame) y fricción (×0.97); desaparece al caer bajo Y = −0.1 o al expirar su TTL de 35 frames.

#### `clMinero.Geometria.vb` — Primitivas geométricas
Funciones auxiliares para construir los componentes del personaje:
- `CrearHueso` — Cilindro de 16 lados como segmento de miembro.
- `CrearEsfera` — Esfera UV parametrizable (usada para articulaciones, cabeza, manos, pies).
- `CrearCilindro` — Cilindro con tapas para el cuello y el mango del pico.
- `CrearCono` — Cono para las puntas del pico.
- `CrearRoca` — Esfera deformada aleatoriamente (ruido por vértice) para la roca que pica.
- `CrearModeloPicoGenerico` — Ensambla mango + centro + dos puntas en un `Model3DGroup`.
- `Lerp` — Interpolación lineal entre dos doubles.
- `ObtenerEstadoAnimacion` — Curva de easing asimétrica para el ciclo de golpe.

---

## 👤 Autor

**Abel Ramírez Villarejo**

- GitHub: [@Abel-RV](https://github.com/Abel-RV)

---

## 📄 Licencia

Este proyecto se distribuye con fines educativos y de demostración.

