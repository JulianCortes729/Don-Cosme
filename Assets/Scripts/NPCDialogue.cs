using System;
using UnityEngine;

/// <summary>
/// Componente responsable de exponer las secuencias de diálogo de un NPC.
/// <para>
/// - La secuencia se inyecta desde un sistema externo mediante <see cref="SetSequence"/>.
/// - El diálogo puede iniciarse de dos formas:
///   1. Automáticamente, cuando el sistema lo solicite via <see cref="TriggerDialogue"/>.
///   2. Manualmente, cuando el jugador interactúa con el NPC via <see cref="Interact"/>.
/// - Un flag interno garantiza que la secuencia solo se dispara UNA vez.
/// </para>
/// </summary>
public class NPCDialogue : MonoBehaviour, IInteractable
{
    private DialogueSequence _currentSequence;

    // 🛡️ Guardia de disparo único: evita que Interact() o TriggerDialogue()
    //    reinicien la secuencia si el diálogo ya está en curso.
    private bool _hasTriggered = false;

    public static event Action<DialogueSequence> OnDialogueInitiated;

    /// <summary>
    /// Inyecta una nueva secuencia y resetea el flag de disparo.
    /// Debe llamarse antes de que el cliente llegue a la ventanilla.
    /// </summary>
    public void SetSequence(DialogueSequence newSequence)
    {
        _currentSequence = newSequence;
        _hasTriggered = false; // 📌 GDD: cada cliente nuevo tiene su propio diálogo fresco
    }

    /// <summary>
    /// Dispara el diálogo programáticamente (ej: cuando el cliente llega a la ventanilla).
    /// Es idempotente: llamarlo varias veces no reinicia el diálogo.
    /// </summary>
    public void TriggerDialogue()
    {
        if (_hasTriggered || _currentSequence == null) return;
        _hasTriggered = true;
        OnDialogueInitiated?.Invoke(_currentSequence);
    }

    /// <summary>
    /// Implementación de IInteractable. Reutiliza TriggerDialogue para mantener
    /// la guardia de disparo único tanto en triggers manuales como automáticos.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (_currentSequence == null)
        {
            Debug.Log("[NPCDialogue] Este NPC no tiene nada más que decir por hoy.");
            return;
        }
        TriggerDialogue();
    }
}