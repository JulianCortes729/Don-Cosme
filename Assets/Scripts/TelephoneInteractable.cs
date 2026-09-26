using System;
using UnityEngine;

[RequireComponent(typeof(NPCDialogue))]
public class TelephoneInteractable : MonoBehaviour, IInteractable
{
    // 🆕 Eventos para que AudioManager pueda duckear la música con el ring
    public static event Action OnPhoneStartRinging;
    public static event Action OnPhoneStopRinging;

    [Header("Feedback")]
    [Tooltip("AudioSource DEDICADO al teléfono. No debe ser el mismo que la música.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ringClip;

    private NPCDialogue _npcDialogue;
    private bool _isRinging = false;

    private void Awake()
    {
        _npcDialogue = GetComponent<NPCDialogue>();
    }

    public void StartRinging(DialogueSequence sequence)
    {
        _npcDialogue.SetSequence(sequence);
        _isRinging = true;

        if (audioSource != null && ringClip != null)
        {
            audioSource.clip = ringClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("[Telephone] ⚠️ Asigná AudioSource y RingClip en el Inspector.");
        }

        // 🆕 Avisamos al AudioManager para que duckee la música suavemente
        OnPhoneStartRinging?.Invoke();

        Debug.Log("☎️ Riiiing! Riiiing!");
    }

    public void Interact(GameObject interactor)
    {
        if (!_isRinging || GameManager.CurrentState != GameState.Playing) return;

        _isRinging = false;

        if (audioSource != null)
            audioSource.Stop();

        // Avisamos al AudioManager para que restaure la música
        OnPhoneStopRinging?.Invoke();

        _npcDialogue.TriggerDialogue();
    }
}