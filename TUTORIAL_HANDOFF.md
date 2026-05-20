# CODE 7 — Handoff Tutorial Scene
**Fecha:** 2026-05-19
**Escena:** Tutorial
**Proyecto:** Unity 6 | Namespace: `CODEX.*` | Scripts en `Assets/Scripts/`

---

## Scripts existentes (no tocar)

| Script | Ubicacion | Estado |
|--------|-----------|--------|
| `PlayerController.cs` | `Scripts/Player/` | Completo. Ground detection, salto, dash |
| `ShootingSystem.cs` | `Scripts/Player/` | Completo. Disparo con mouse |
| `Projectile.cs` | `Scripts/Systems/` | Completo. IDamageable interface |
| `InfectedFile.cs` | `Scripts/Enemies/` | Completo. 5 tipos de enemigo |
| `DataCollectible.cs` + `DataManager` | `Scripts/Systems/` | Completo. Recoleccion + contador |
| `RepairTerminal.cs` | `Scripts/Interactables/` | Completo. Tecla E para activar |
| `CameraFollow.cs` | `Scripts/Systems/` | Completo. Sigue jugador + SnapToTarget |
| `ZoneTransition.cs` | `Scripts/Tutorial/` | Completo. Teletransporta + cambia fondos |
| `TutorialDialogueTrigger.cs` | `Scripts/Tutorial/` | Completo. Muestra hint al entrar zona |
| `ShootingTarget.cs` | `Scripts/Tutorial/` | Completo. Blanco disparable con IDamageable |
| `TutorialShootingBlock.cs` | `Scripts/Tutorial/` | Completo. Gestiona blancos + progreso UI |

---

## Scripts que FALTAN crear

### 1. `PlayerHealth.cs` - `Scripts/Player/`
CRITICO - sin esto el Bloque 4 no funciona.

Debe incluir:
- HP configurable, recibir danio desde `InfectedFile` via interfaz `IDamageable`
- Frames de invencibilidad al recibir danio (usar `PlayerController.SetInvincible()` que ya existe)
- Muerte + respawn en el ultimo Checkpoint visitado
- Eventos: `OnDamaged(int hp)`, `OnDied()`
- Referencia a `PlayerController` para aplicar knockback al recibir danio

### 2. `FallingPlatform.cs` - `Scripts/Tutorial/`
Necesario para Bloque 6.

Debe incluir:
- Al pisarse: esperar 0.5s, luego caer (desactivar `isKinematic` en Rigidbody2D)
- Resetear a posicion original tras N segundos o al respawn del jugador

---

## Configuracion GLOBAL (hacer antes de trabajar cualquier bloque)

Estos pasos son prerequisito - sin ellos ningun trigger ni sistema funciona:

1. **Player tag** - seleccionar el GameObject `Player` en Hierarchy -> Inspector -> cambiar `Tag: Untagged` a `Player`
2. **Layer "ground"** - seleccionar cada plataforma -> Layer -> asignar `ground`. Luego en PlayerController Inspector -> `Ground Layer` -> seleccionar `ground`
3. **Canvas UI** - crear un `Canvas` (Screen Space Overlay) con un panel hijo `DialoguePanel` + `TextMeshProUGUI`. Todos los bloques lo comparten para mostrar hints
4. **[BACKGROUNDS]** - verificar que existan fondos separados por bloque como GameObjects hijos de `[BACKGROUNDS]`. Cada `ZoneTransition` los habilita/deshabilita

---

## Estado y tareas por bloque

---

### [BLOQUE_1] Materializacion y Movimiento
Hijos actuales: `SpawnPoint_Player`, `Plataforma_Inicio`, `Plataforma_Salto_1`, `Plataforma_Salto_2`, `Checkpoint_1`, `TriggerDialogo_LUMA`, `Player`

- [ ] `TriggerDialogo_LUMA` -> agregar script `TutorialDialogueTrigger`
  Mensaje: "Sistema en linea. Soy LUMA. Usa A/D para moverte, Espacio para saltar, Shift para hacer dash."
- [ ] Crear GameObject vacio `TransicionBloque1_a_2` en el borde derecho del bloque -> `BoxCollider2D` alto y delgado (trigger) + script `ZoneTransition`:
  - Player Spawn Point -> arrastrar `Checkpoint_2`
  - Objects To Disable -> Background Bloque 1
  - Objects To Enable -> Background Bloque 2

---

### [BLOQUE_2] Disparo
Hijos actuales: `Enemy_EstaticoBloqueaPuerta`, `Puerta_Bloqueada`, `TriggerDialogo_Disparo`, `Checkpoint_2`

- [ ] `TriggerDialogo_Disparo` -> agregar script `TutorialDialogueTrigger`
  Mensaje: "Archivos infectados detectados. Apunta con el raton. Clic para disparar."
- [ ] `Enemy_EstaticoBloqueaPuerta` -> componente InfectedFile -> seccion Eventos -> `On Enemy Death` -> click en + -> arrastrar `Puerta_Bloqueada` -> funcion `GameObject > SetActive (bool)` -> DESMARCAR el checkbox (= false)
- [ ] Colocar 3-5 GameObjects de practica con script `ShootingTarget` antes del enemigo principal
- [ ] Agregar un GameObject vacio con script `TutorialShootingBlock`: referenciar los ShootingTarget y conectar Canvas UI
- [ ] Crear `TransicionBloque2_a_3` en el borde derecho -> `ZoneTransition` con spawn en `Checkpoint_3`

---

### [BLOQUE_3] Recoleccion Datos
Hijos actuales: (vacio - todo por crear)

- [ ] Agregar `Checkpoint_3` como hijo
- [ ] Colocar 5-8 GameObjects con script `DataCollectible` (flotan, el jugador los toca para recolectar). Necesitan `SpriteRenderer` y `Collider2D` trigger con layer `Player`
- [ ] Crear gestor que cuente los recolectados y al completarlos abra la puerta. Usar `DataManager.OnDataGoalReached`
- [ ] Crear puerta de salida bloqueada que se abre al recolectar todo
- [ ] TriggerDialogo de entrada: "Fragmentos de datos corruptos detectados. Recupetalos todos."
- [ ] ZoneTransition -> Bloque 4

---

### [BLOQUE_4] Danio y Esquive
Hijos actuales: (vacio - todo por crear)
REQUIERE `PlayerHealth.cs` creado primero.

- [ ] Agregar `Checkpoint_4`
- [ ] Colocar 1-2 `InfectedFile` tipo `TypeD_Melee` que persigan al jugador
- [ ] Zona con trampa/spike que aplique danio fijo (trigger simple que llame `PlayerHealth.TakeDamage`)
- [ ] TriggerDialogo de entrada: "Zona de alto riesgo. Usa Shift para hacer dash e ignorar el danio."
- [ ] Conectar colision enemigo con jugador -> `PlayerHealth.TakeDamage`
- [ ] Puerta bloqueada que se abre al eliminar todos los enemigos
- [ ] ZoneTransition -> Bloque 5

---

### [BLOQUE_5] Terminal 1
Hijos actuales: (vacio - todo por crear)

- [ ] Agregar `Checkpoint_5`
- [ ] Colocar un GameObject con script `RepairTerminal` + `Collider2D` trigger
- [ ] TriggerDialogo de entrada: "Terminal de sistema detectada. Presiona E para interactuar."
- [ ] `RepairTerminal.OnActivated` (UnityEvent en Inspector) -> conectar: abrir puerta o activar siguiente zona
- [ ] ZoneTransition -> Bloque 6

---

### [BLOQUE_6] Plataformas que Caen
Hijos actuales: (vacio - todo por crear)
REQUIERE `FallingPlatform.cs` creado primero.

- [ ] Agregar `Checkpoint_6`
- [ ] Colocar 4-6 plataformas con script `FallingPlatform` sobre un vacio (si el jugador se demora, cae)
- [ ] Cada plataforma necesita `Rigidbody2D` (kinematic por default) + `BoxCollider2D` con layer `ground`
- [ ] TriggerDialogo de entrada: "Las plataformas son inestables. Muevete rapido."
- [ ] ZoneTransition -> Bloque 7

---

### [BLOQUE_7] Enemigos Combinados
Hijos actuales: (vacio - todo por crear)

- [ ] Agregar `Checkpoint_7`
- [ ] Colocar 1 `InfectedFile` tipo `TypeB_Patrol` + 2 GameObjects vacios como `PatrolPoint_0` y `PatrolPoint_1` (arrastrarlos al array Patrol Points del componente)
- [ ] Colocar 1 `InfectedFile` tipo `TypeC_Projectile` + hijo vacio `FirePoint` asignado al campo Fire Point del componente. Necesita un prefab de proyectil asignado
- [ ] TriggerDialogo de entrada: "Objetivo final: elimina todos los archivos infectados usando todo lo aprendido."
- [ ] Crear gestor que cuente muertes con `OnEnemyDeath` de cada InfectedFile -> al morir todos, abre puerta hacia Bloque 8
- [ ] ZoneTransition -> Bloque 8

---

### [BLOQUE_8] Puerto de Salida
Hijos actuales: (vacio - todo por crear)

- [ ] Agregar `Checkpoint_8`
- [ ] Puerta/portal final: puede ser un `RepairTerminal` o un trigger automatico al entrar
- [ ] TriggerDialogo de entrada: "Tutorial completado. El sistema te espera, agente."
- [ ] Al activar -> usar `SceneTransition.cs` (ya existe en Scripts/UI/) para cargar la siguiente escena del juego

---

## Orden de prioridades recomendado

```
PASO 1  Configuracion global (Player tag, ground layer, Canvas UI)
        Sin esto, ningun trigger ni dialogo funciona

PASO 2  Crear PlayerHealth.cs
        Los bloques 4, 7 y 8 dependen de esto

PASO 3  Bloque 1: configurar TriggerDialogo_LUMA + ZoneTransition
PASO 4  Bloque 2: wiring enemigo-puerta + colocar ShootingTargets
PASO 5  Bloque 3: colocar DataCollectibles + gestor de recoleccion
PASO 6  Bloque 5: colocar RepairTerminal + configurar evento
PASO 7  Crear FallingPlatform.cs
PASO 8  Bloque 6: colocar plataformas con FallingPlatform
PASO 9  Bloque 4: colocar enemigos + conectar PlayerHealth
PASO 10 Bloque 7: colocar enemigos combinados + gestor de muertes
PASO 11 Bloque 8: puerta de salida + SceneTransition
```

---

## Notas tecnicas importantes

- **Namespace:** todo el codigo usa `CODEX.Player`, `CODEX.Systems`, `CODEX.Enemies`, `CODEX.Tutorial`
- **Input System:** el juego usa el nuevo Input System de Unity. No usar `Input.GetKey()` en scripts nuevos - usar `InputAction` como en `PlayerController.cs`
- **IDamageable:** la interfaz vive en `Projectile.cs` como `CODEX.Systems.Projectile.IDamageable`. Cualquier cosa que reciba danio de proyectiles debe implementarla
- **ZoneTransition:** el trigger del borde debe ser lo suficientemente alto para que el jugador no pueda pasarlo sin activarlo. Recomendado: 1 unidad de ancho, altura que cubra toda la pantalla
- **PlayerController.TeleportTo():** usar siempre este metodo para mover al jugador por codigo - resetea la velocidad del Rigidbody automaticamente
- **CameraFollow.SnapToTarget():** llamar siempre despues de teleportar al jugador para evitar que la camara haga slide entre zonas
