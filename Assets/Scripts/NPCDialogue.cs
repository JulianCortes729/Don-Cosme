using System;
using UnityEngine;

/// <summary>
/// Componente responsable de exponer las secuencias de diálogo de un NPC.
/// <para>
/// - La secuencia se inyecta desde un sistema externo (por ejemplo, <c>GameManager</c>)
///   mediante <see cref="SetSequence"/>.
/// - Cuando el jugador interactúa con el NPC, este componente dispara
///   el evento <see cref="OnDialogueInitiated"/> con la secuencia actualmente asignada.
/// </para>
/// </summary>
public class NPCDialogue : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Secuencia de diálogo actualmente asociada a este NPC. Puede ser null si no hay diálogo.
    /// </summary>
    // SOLID: Ahora es privada; un sistema externo (GameManager) se encarga de inyectar los datos.
    private DialogueSequence currentSequence;

    /// <summary>
    /// Evento lanzado cuando el NPC inicia una conversación. Proporciona la
    /// <see cref="DialogueSequence"/> que debe reproducirse.
    /// </summary>
    public static event Action<DialogueSequence> OnDialogueInitiated;

    /// <summary>
    /// Método público para inyectar una nueva secuencia de diálogo al NPC.
    /// </summary>
    /// <param name="newSequence">Secuencia que el NPC debe exponer cuando sea interactuado.</param>
    public void SetSequence(DialogueSequence newSequence)
    {
        currentSequence = newSequence;
    }

    /// <summary>
    /// Implementación del contrato <see cref="IInteractable"/>.
    /// Cuando el jugador interactúa con este NPC se invoca este método. El
    /// parámetro <paramref name="interactor"/> se proporciona por compatibilidad
    /// con el sistema de interacción, pero en este componente no es necesario
    /// para iniciar la conversación.
    /// </summary>
    /// <param name="interactor">GameObject del punto de interacción (no usado aquí).</param>
    public void Interact(GameObject interactor)
    {
        // Solo iniciamos el diálogo si el NPC tiene una secuencia asignada.
        if (currentSequence != null)
        {
            OnDialogueInitiated?.Invoke(currentSequence);
        }
        else
        {
            Debug.Log("Este NPC no tiene nada más que decir por hoy.");
        }
    }
}
