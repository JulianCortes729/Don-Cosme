using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la reproducción de secuencias de diálogo en pantalla.
///
/// Responsibilities:
/// - Mostrar/ocultar la UI de diálogo.
/// - Reproducir cada nodo de <see cref="DialogueSequence"/> (texto, imagen y SFX).
/// - Exponer un evento estático <see cref="OnDialogueEnded"/> para notificar
///   a otros sistemas cuando una secuencia finaliza.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Panel raíz de la UI de diálogo. Se habilita/deshabilita al iniciar/terminar una secuencia.
    /// </summary>
    [SerializeField] private GameObject dialoguePanel;

    /// <summary>
    /// Texto donde se mostrará el contenido del nodo actual.
    /// </summary>
    [SerializeField] private TMP_Text dialogueText;

    /// <summary>
    /// Imagen de fondo opcional que puede mostrar arte o un panel al reproducir un nodo.
    /// </summary>
    [SerializeField] private Image backgroundImage;

    /// <summary>
    /// Fuente de audio utilizada para reproducir efectos de sonido asociados a nodos.
    /// </summary>
    [SerializeField] private AudioSource audioSource; // Componente para reproducir el SFX

    [Header("State (No tocar en el Inspector)")]
    /// <summary>
    /// Secuencia actual que se está reproduciendo. Null cuando no hay ninguna activa.
    /// </summary>
    private DialogueSequence currentSequence; // El "disco" actual que estamos leyendo

    /// <summary>
    /// Índice del nodo actual dentro de la secuencia.
    /// </summary>
    private int currentNodeIndex = 0; // El "track" o cuadro actual

    /// <summary>
    /// Evento disparado cuando una secuencia de diálogo finaliza. Otros sistemas
    /// pueden suscribirse para reaccionar (por ejemplo, reanudar el control del jugador).
    /// </summary>
    public static event Action OnDialogueEnded; // <-- Declaración


    // --- MÉTODOS PÚBLICOS (Para que otros scripts inicien la charla) ---

    /// <summary>
    /// Inicia la reproducción de una nueva secuencia de diálogo.
    /// Activa la UI, resetea el índice y muestra el primer nodo.
    /// </summary>
    /// <param name="newSequence">Secuencia de diálogo a reproducir. No debe ser null.</param>
    public void StartSequence(DialogueSequence newSequence)
    {
        // 1. Asigna 'newSequence' a tu variable 'currentSequence'.
        currentSequence = newSequence;
        // 2. Resetea el 'currentNodeIndex' a 0.
        currentNodeIndex = 0;

        dialoguePanel.SetActive(true);
        // 3. Llama al método DisplayCurrentNode().
        DisplayCurrentNode();
    }

    // --- LÓGICA INTERNA ---

    /// <summary>
    /// Muestra en pantalla el nodo actual de la secuencia: actualiza texto, imagen
    /// y reproduce el SFX asociado si existe.
    /// </summary>
    private void DisplayCurrentNode()
    {
        if (currentSequence == null) return;

        // 1. Obtén el nodo actual accediendo al arreglo dentro de 'currentSequence':
        //    Ejemplo: DialogueNode node = currentSequence.nodes[currentNodeIndex];
        DialogueNode currentNode = currentSequence.nodes[currentNodeIndex];

        // 2. Asigna el texto del nodo a dialogueText.text
        dialogueText.text = currentNode.dialogueText;

        // 3. SI el nodo tiene una imagen de fondo, la asignamos y habilitamos la imagen.
        //    Si NO tiene, deshabilitamos el componente Image para dejar el fondo transparente.
        if (currentNode.backgroundImage != null)
        {
            backgroundImage.sprite = currentNode.backgroundImage;
            backgroundImage.enabled = true;  // Mantener el DialoguePanel activo
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.enabled = false; // El DialoguePanel permanece visible sin fondo
        }

        // 4. SI el nodo tiene un audio (node.sfx != null), reprodúcelo usando PlayOneShot.
        if (currentNode.sfx != null)
        {
            audioSource.PlayOneShot(currentNode.sfx);
        }
    }


    private void OnEnable()
    {
        // Suscribimos los manejadores a los eventos relevantes.
        InputManager.OnInteractPressed += AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated += StartSequence; 
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar memory leaks o llamadas inesperadas.
        InputManager.OnInteractPressed -= AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated -= StartSequence;
    }

    /// <summary>
    /// Avanza al siguiente nodo de la secuencia cuando el jugador pulsa la tecla
    /// de interacción. Si no hay más nodos, cierra la UI y dispara <see cref="OnDialogueEnded"/>.
    /// </summary>
    private void AdvanceDialogue()
    {

        if (currentSequence == null) return;

        // Avanzamos al siguiente nodo
        currentNodeIndex++;
        if (currentNodeIndex < currentSequence.nodes.Length)
        {
            // Si aún hay nodos, mostramos el siguiente
            DisplayCurrentNode();
        }
        else
        {
            // Si ya no hay más nodos, terminamos la charla:
            currentSequence = null; // Limpiamos la secuencia actual
            dialoguePanel.gameObject.SetActive(false);

            // Disparamos el evento para notificar que la charla terminó
            OnDialogueEnded?.Invoke();
        }
    }
}
