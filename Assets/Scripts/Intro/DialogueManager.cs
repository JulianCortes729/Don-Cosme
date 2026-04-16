using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameLabel; // 🆕 Label del nombre
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AudioSource audioSource;

    // ── Eventos ───────────────────────────────────────────────────────────
    public static event Action OnDialogueEnded;
    public static event Action OnWaitingForProductReached; // 🆕 Pausa para entrega

    // ── Estado ────────────────────────────────────────────────────────────
    private DialogueSequence _currentSequence;
    private int _currentNodeIndex = 0;
    private bool _isPaused = false; // 🆕 Pausado esperando entrega
    private bool _justStarted = false;

    // ── API pública ───────────────────────────────────────────────────────

    public void StartSequence(DialogueSequence newSequence)
    {
        _currentSequence = newSequence;
        _currentNodeIndex = 0;
        _isPaused = false;
        _justStarted = true;
        dialoguePanel.SetActive(true);
        DisplayCurrentNode();
    }

    /// <summary>
    /// Llamado por GameManager cuando el jugador entregó el producto correcto.
    /// Reanuda el diálogo desde el nodo siguiente al marcador.
    /// </summary>
    public void ResumeAfterDelivery()
    {
        if (!_isPaused) return;
        _isPaused = false;
        _currentNodeIndex++;                     // Saltamos el nodo marcador
        dialoguePanel.SetActive(true);

        if (_currentNodeIndex < _currentSequence.nodes.Length)
            DisplayCurrentNode();
        else
            EndSequence();
    }

    // ── Internos ──────────────────────────────────────────────────────────

    private void DisplayCurrentNode()
    {
        DialogueNode node = _currentSequence.nodes[_currentNodeIndex];

        // 🆕 Nombre del hablante
        if (speakerNameLabel != null)
        {
            speakerNameLabel.text = node.speakerName;
            speakerNameLabel.enabled = !string.IsNullOrEmpty(node.speakerName);
        }

        dialogueText.text = node.dialogueText;

        if (node.backgroundImage != null)
        {
            backgroundImage.sprite = node.backgroundImage;
            backgroundImage.enabled = true;
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.enabled = false;
        }

        if (node.sfx != null)
            audioSource.PlayOneShot(node.sfx);
    }

    private void AdvanceDialogue()
    {
        if (_justStarted || _isPaused || _currentSequence == null) return;

        _currentNodeIndex++;

        if (_currentNodeIndex >= _currentSequence.nodes.Length)
        {
            EndSequence();
            return;
        }

        DialogueNode nextNode = _currentSequence.nodes[_currentNodeIndex];

        // 🆕 Si el siguiente nodo es el marcador de espera, pausamos
        if (nextNode.isWaitingForProductNode)
        {
            _isPaused = true;
            dialoguePanel.SetActive(false);          // Ocultamos UI para que el jugador pueda moverse
            OnWaitingForProductReached?.Invoke();    // Le avisamos al GameManager
            return;
        }

        DisplayCurrentNode();
    }

    private void EndSequence()
    {
        _currentSequence = null;
        dialoguePanel.SetActive(false);
        OnDialogueEnded?.Invoke();
    }

    private void LateUpdate()
    {
        if (_justStarted) _justStarted = false;
    }

    private void OnEnable()
    {
        InputManager.OnInteractPressed += AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated += StartSequence;
    }

    private void OnDisable()
    {
        InputManager.OnInteractPressed -= AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated -= StartSequence;
    }
}