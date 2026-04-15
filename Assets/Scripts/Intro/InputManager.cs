using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestión centralizada de entradas relacionadas con la interacción del jugador.
/// Expone un evento estático que otros sistemas pueden suscribir para recibir
/// la señal cuando el jugador pulsa una tecla o botón de interacción.
/// </summary>
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Evento disparado cuando el jugador realiza una acción de interacción.
    /// Se invoca al detectar: Espacio, Enter, clic izquierdo del ratón o la tecla E.
    /// Otros sistemas pueden suscribirse para reaccionar a esta entrada.
    /// </summary>
    public static event Action OnInteractPressed;

    /// <summary>
    /// Comprobación por frame de las teclas/botones de interacción.
    /// Cuando se detecta una pulsación válida, se invoca <see cref="OnInteractPressed"/>.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            OnInteractPressed?.Invoke();
        }
    }
} 
