using System;
using UnityEngine;

/// <summary>
/// Gestión centralizada de entradas. Expone eventos separados
/// por tecla para que cada sistema escuche solo lo que necesita.
/// </summary>
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Tecla E — Agarrar / soltar objetos y recoger plata.
    /// </summary>
    public static event Action OnInteractPressed;

    /// <summary>
    /// Click izquierdo / Space / Enter — Avanzar diálogos.
    /// </summary>
    public static event Action OnAdvanceDialogue;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractPressed?.Invoke();
        }

        if (Input.GetMouseButtonDown(0)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return))
        {
            OnAdvanceDialogue?.Invoke();
        }
    }
}