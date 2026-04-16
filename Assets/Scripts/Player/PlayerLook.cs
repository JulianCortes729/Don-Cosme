using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la rotación de la cámara del jugador a partir del movimiento del ratón.
///
/// Comportamiento:
/// - Rota verticalmente la cámara (pitch) y limita su ángulo para evitar giros completos.
/// - Rota horizontalmente el cuerpo del jugador (yaw) para sincronizar la dirección.
///
/// Requisitos:
/// - <see cref="playerBody"/> debe apuntar al transform que representa el cuerpo del jugador
///   (normalmente la raíz que contiene el CharacterController).
/// - Este componente asume que existe un sistema de estados (<see cref="GameManager.CurrentState"/>)
///   y no procesa entradas si el juego no está en estado <see cref="GameState.Playing"/>.
/// </summary>
public class PlayerLook : MonoBehaviour
{

    /// <summary>
    /// Sensibilidad del ratón aplicada a los ejes X e Y. Valores mayores hacen que
    /// la cámara responda más rápidamente al movimiento del ratón.
    /// </summary>
    /// <remarks>
    /// Valores recomendados: entre 1.0 y 10.0. Ajusta según la resolución y la preferencia del jugador.
    /// </remarks>
    [SerializeField] private float mouseSensitivity = 3f;

    /// <summary>
    /// Referencia al transform del cuerpo del jugador. Se utiliza para aplicar
    /// la rotación horizontal (yaw) cuando el ratón se mueve en X.
    /// </summary>
    /// <remarks>
    /// Debe asignarse desde el inspector. Si no se asigna, la rotación horizontal
    /// fallará silenciosamente y el jugador no girará correctamente.
    /// </remarks>
    [SerializeField] private Transform playerBody;

    /// <summary>
    /// Ángulo acumulado de rotación en el eje X (pitch). Se mantiene entre -90 y 90
    /// grados para evitar que la cámara gire completamente.
    /// </summary>
    private float xRotation = 0f;


    /// <summary>
    /// Inicialización del componente.
    /// </summary>
    /// <remarks>
    /// Bloquea el cursor en el centro de la pantalla para capturar el movimiento del ratón.
    /// Si tu juego permite liberar el cursor (por menús, pause, etc.), gestiona <see cref="Cursor.lockState"/>
    /// y <see cref="Cursor.visible"/> desde el sistema de UI correspondiente.
    /// </remarks>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Actualización por frame que procesa la entrada del ratón y aplica las rotaciones.
    /// </summary>
    /// <remarks>
    /// Este método ignora la entrada si <see cref="GameManager.CurrentState"/> no es
    /// <see cref="GameState.Playing"/>, evitando que la cámara se mueva durante cinemáticas
    /// o menús. Usa ejes Input estándar "Mouse X" y "Mouse Y" definidos en el InputManager.
    /// </remarks>
    void Update()
    {
        if (GameManager.CurrentState == GameState.Cinematic) return;

        float mouseX = Input.GetAxis("Mouse X") * (mouseSensitivity);
        float mouseY = Input.GetAxis("Mouse Y") * (mouseSensitivity);

        // Invertimos Y para que mover el ratón hacia arriba disminuya el pitch.
        xRotation -= mouseY;

        // Limitamos la rotación vertical para evitar giro completo de la cámara.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplicamos la rotación vertical local a la cámara.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotación horizontal aplicada al transform del cuerpo del jugador.
        playerBody.Rotate(Vector3.up * mouseX);

    }
}
