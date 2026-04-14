using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la rotación de la cámara del jugador a partir del movimiento del ratón.
/// - Rota verticalmente la cámara (pitch) y limita su ángulo para evitar giros completos.
/// - Rota horizontalmente el cuerpo del jugador (yaw) para sincronizar la dirección.
/// </summary>
public class PlayerLook : MonoBehaviour
{

    /// <summary>
    /// Sensibilidad del ratón aplicada a los ejes X e Y. Valores mayores hacen que
    /// la cámara responda más rápidamente al movimiento del ratón.
    /// </summary>
    [SerializeField] private float mouseSensitivity = 3f;

    /// <summary>
    /// Referencia al transform del cuerpo del jugador. Se utiliza para aplicar
    /// la rotación horizontal (yaw) cuando el ratón se mueve en X.
    /// </summary>
    [SerializeField] private Transform playerBody;

    /// <summary>
    /// Ángulo acumulado de rotación en el eje X (pitch). Se mantiene entre -90 y 90
    /// grados para evitar que la cámara gire completamente.
    /// </summary>
    private float xRotation = 0f;


    /// <summary>
    /// Inicialización: bloquea el cursor al centro de la pantalla para captura de ratón.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Lógica por frame que realiza lo siguiente:
    /// 1) Lee el movimiento del ratón en los ejes X/Y multiplicado por la sensibilidad.
    /// 2) Actualiza el ángulo vertical acumulado y lo limita con <see cref="Mathf.Clamp"/>.
    /// 3) Aplica la rotación vertical a la cámara (este transform) y la rotación horizontal
    ///    al <see cref="playerBody"/> para que el personaje gire.
    /// </summary>
    void Update()
    {
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
