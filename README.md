# Aventura Pixel — Prueba 02: Arquitectura y Lógica de un Videojuego

## Objetivo
Prototipo de un nivel de un juego de plataformas 2D en Unity. El jugador recorre el nivel, recoge monedas, esquiva o pisa enemigos y evita pinchos y caídas hasta llegar a la bandera de meta.

## Plataforma: PC Windows (64 bits)
- Es un juego de plataformas de precisión que se juega mejor con teclado.
- El build para Windows se prueba fácilmente en los computadores del laboratorio, sin instalar nada (solo hay que abrir el `.exe`).
- Ventana de 1280×720 redimensionable, con VSync activado para evitar *tearing*.

## Configuración clave
| Elemento | Decisión |
|---|---|
| Versión de Unity | 6000.2.3f1 |
| Pipeline | URP con **2D Renderer** (`Assets/Settings/Renderer2D.asset`) y una **Global Light 2D** en cada escena. Los sprites usan el material *Sprite-Lit-Default*. |
| Gráficos | Pixel art de 16×16 px, 16 Pixels Per Unit, filtro *Point* y sin compresión (bordes nítidos). |
| Nivel | `Tilemap` "Ground" (con `TilemapCollider2D`, capa **Ground**) y `Tilemap` "Decoration" (nubes y arbustos, sin colisión). |
| Input | Input System nuevo (`Keyboard.current`). |
| Quality | Configuración por defecto de URP para PC y VSync = 1. |

### Organización de carpetas
```
Assets/_Game/
  Scripts/   lógica del juego (C#)
  Prefabs/   Player, Coin, Enemy, Spike, Goal
  Scenes/    MainMenu, Level1, GameOver
  Sprites/   Tiles/   Audio/   Physics/
  Editor/    GameBuilder.cs (herramienta que generó los assets y escenas)
```

## Game loop
`MainMenu` → **Jugar** → `Level1` → (tocar la meta = victoria / pinchos, enemigo o caída = derrota) → `GameOver` (muestra el resultado y el puntaje) → **Reintentar** o **Menú**.

## Física
- `Rigidbody2D` dinámico en el jugador, con **gravedad ajustada**: `gravityScale = 3`, multiplicada ×1.8 al caer para que el salto se sienta firme.
- **Salto** por velocidad vertical y detección de suelo con `Physics2D.OverlapCircle` sobre la capa Ground.
- **Physics Material 2D** con fricción 0 en el jugador, para que no se quede pegado a las paredes.
- Colisiones: triggers para monedas, pinchos, meta y zona de caída. Colisión sólida con los enemigos (`Rigidbody2D` cinemático): si el jugador cae encima, rebota y el enemigo desaparece.

## Audio
- **BGM**: música en bucle en el menú y en el nivel.
- **SFX**: salto, moneda, pisar a un enemigo y clic de botón.

## Ejecución
- **Jugar:** abrir `Build/AventuraPixel.exe`.
- **Editar:** abrir la carpeta `Project/` desde Unity Hub con Unity 6000.2.3f1 y cargar la escena `Assets/_Game/Scenes/MainMenu.unity`.

## Controles
| Acción | Tecla |
|---|---|
| Mover | A / D o ← / → |
| Saltar | Espacio |
| Menús | Mouse |

## Dependencias
Solo paquetes oficiales de Unity: Universal RP, 2D Tilemap, Input System y uGUI. No usa assets externos: los sprites y los sonidos se generaron por código.

## Riesgos y mitigaciones
| Riesgo | Mitigación |
|---|---|
| El jugador se queda pegado a las paredes | Physics Material 2D con fricción 0 |
| Saltos infinitos en el aire | Detección de suelo antes de permitir el salto |
| Perder o ganar dos veces en el mismo frame | Bandera `gameEnded` en `GameManager` |
| Caer fuera del nivel sin que pase nada | `KillZone` (trigger) bajo el nivel |
| Subir archivos pesados a GitHub | `.gitignore` excluye `Library/`, `Temp/` y `Logs/` |
| Parámetros fijos en el código | Valores expuestos con `[SerializeField]` y editables en el Inspector |

## Roles
- *(Tu nombre)*: diseño del nivel, programación, física, audio, build y documentación.
