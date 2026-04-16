using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador usando un <see cref="CharacterController"/>.
/// Gestiona el movimiento horizontal relativo a la orientación del jugador y aplica
/// una gravedad simple para el movimiento vertical. 
/// </summary>
/// <remarks>
/// Implementación basada en un modelo simplificado de física:
/// - El movimiento horizontal se calcula en el espacio local del jugador y se aplica
///   mediante <see cref="CharacterController.Move"/> (por tanto, no usa fuerzas físicas). 
/// - La gravedad se integra explícitamente en el valor <see cref="velocityVertical"/>
///   y se aplica como desplazamiento por frame; esto es suficiente para la mayoría de
///   los controles en primera persona pero no sustituye una simulación física completa.
/// - Todas las velocidades están en unidades de Unity (generalmente metros) por segundo.
/// </remarks>
[RequireComponent(typeof(CharacterController))] // Fuerza a Unity a añadir el componente si no existe 
public class PlayerMovement : MonoBehaviour
{

    [Header("Referencias")]
    /// <summary>
    /// Referencia al <see cref="CharacterController"/> que se usa para desplazar al jugador.
    /// Se asigna desde el inspector de Unity o mediante el atributo <see cref="RequireComponent"/>.
    /// </summary>
    [SerializeField] private CharacterController controller;

    [Header("Parámetros")]
    /// <summary>
    /// Velocidad de desplazamiento en unidades por segundo para el movimiento horizontal.
    /// </summary>
    [SerializeField] private float speed = 12f;

    /// <summary>
    /// Valor constante de la gravedad aplicado a la velocidad vertical (m/s^2).
    /// Negativo para indicar dirección hacia abajo.
    /// </summary>
    private const float gravity = -9.81f; // Gravedad constante para simular la caída del jugador

    /// <summary>
    /// Componente de velocidad vertical del jugador (m/s). Se integra cada frame con la gravedad
    /// y se convierte a desplazamiento antes de llamar a <see cref="CharacterController.Move"/>.
    /// </summary>
    private float velocityVertical = 0f; // Velocidad vertical del jugador, que se verá afectada por la gravedad


    /// <summary>
    /// Inicialización temprana del componente.
    /// Verifica que la referencia al <see cref="CharacterController"/> esté asignada;
    /// si no lo está, intenta obtenerla del mismo GameObject para evitar errores en tiempo de ejecución.
    /// </summary>
    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    /// <summary>
    /// Lógica por frame que realiza lo siguiente:
    /// 1) Lee el input horizontal y vertical (ejes "Horizontal" y "Vertical").
    /// 2) Construye un vector de movimiento en el espacio local del jugador
    ///    combinando su derecha y su adelante para que "adelante" sea relativo
    ///    a la rotación del jugador.
    /// 3) Aplica velocidad, clampa la magnitud para evitar desplazamientos
    ///    diagonales excesivos y convierte velocidad a desplazamiento por frame.
    /// 4) Gestiona la gravedad integrando la velocidad vertical y aplica un
    ///    pequeño empuje hacia abajo cuando el jugador está en el suelo para
    ///    mantenerlo pegado.
    /// 5) Llama a <see cref="CharacterController.Move"/> con la suma del
    ///    desplazamiento horizontal y vertical.
    /// </summary>
    void Update()
    {
        if (GameManager.CurrentState == GameState.Cinematic) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Movimiento local basado en la orientación del jugador.
        Vector3 movement = (transform.right * x + transform.forward * z); // combinación de ejes X/Z en espacio local
        Vector3 clampedInput = Vector3.ClampMagnitude(movement, 1f);
        clampedInput *= speed * Time.deltaTime; // velocidad -> desplazamiento por frame

        // Si estamos en el suelo y la velocidad vertical es negativa, mantenemos
        // un pequeño valor negativo para que el CharacterController detecte el suelo.
        if (controller.isGrounded && velocityVertical < 0)
        {
            velocityVertical = -2f; // pequeño empuje hacia abajo para 'pegar' al suelo
        }

        // Integramos la gravedad en la velocidad vertical (m/s).
        velocityVertical += gravity * Time.deltaTime;

        // Convertimos la velocidad vertical actual a desplazamiento para este frame.
        Vector3 movementVertical = new Vector3(0, velocityVertical, 0) * Time.deltaTime;

        // Aplicamos el movimiento combinado al CharacterController.
        controller.Move(clampedInput + movementVertical);

    }
}
