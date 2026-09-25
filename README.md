# Kioskito de mi Barrio 🍬

> Juego de simulación con identidad barrial de Buenos Aires, ambientado en 2006.

**[▶ Ver en itch.io](https://juliancortes729.itch.io/kioskito-de-mi-barrio)**

Motor: **Unity** · Lenguaje: **C#** · Plataforma: **Windows**

---

## Sobre el juego

Atendés el kiosco. Los clientes llegan, piden, pagan y se van, y entre medio hay que desempacar
mercadería y mantener el orden del inventario. La atención al público es también la vía de la narrativa:
cada cliente trae su diálogo.

## Dónde mirar el código

Todo el código propio está en `Assets/Scripts/`.

| Archivo / Carpeta | Qué hace |
| :--- | :--- |
| `ClienteController.cs` | Ciclo de vida del cliente: llegada, espera de diálogo, espera de producto, salida. Navegación con `NavMeshAgent` |
| `ClienteData.cs` | Datos del cliente (qué pide, quién es) |
| `DeliveryZone.cs` | Zona de entrega: valida que se le dé al cliente lo que pidió |
| `ObjectGrabbable.cs` | Agarrar y soltar objetos en primera persona |
| `MoneyBill.cs` | Manejo del dinero |
| `Interfaces/` | `IInteractable`, `IDroppable` — contratos de interacción |
| `Intro/` | Diálogos, configuración del día y flujo de la partida |
| `Player/` | Movimiento, cámara e interacción del jugador |

**Por dónde empezar:** `ClienteController.cs` (máquina de estados del cliente) y `PlayerInteractor.cs` (cómo el jugador toca el mundo).

## Notas técnicas

- Los estados del cliente son un `enum` explícito (`Arriving`, `WaitingForDialogue`, `WaitingForProduct`, `Leaving`) en vez de banderas sueltas.
- El controlador de cliente y el diálogo son **componentes separados** que se combinan en el mismo objeto, no una sola clase que hace todo.
- Los sistemas se avisan por `static event Action` (`OnClientArrived`, `OnClientLeft`).
- Interacción por interfaces (`IInteractable`), así que agregar un objeto interactuable nuevo no obliga a tocar el jugador.

## Créditos

- **Julián Cortés** — Programación: sistema de desempaque y orden de inventario, simulación de atención al público, lógica de diálogos y eventos narrativos.
- Arte, sonido y diseño: resto del equipo (ver página de itch.io).
