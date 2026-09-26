using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameLabel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image backgroundImage;

    [Header("Audio — SFX de nodo (Crossfade)")]
    [Tooltip("Dos AudioSources para crossfade. Asigná dos componentes AudioSource en este mismo GameObject.")]
    [SerializeField] private AudioSource sfxSourceA;
    [SerializeField] private AudioSource sfxSourceB;
    [Tooltip("Segundos que tarda el crossfade entre SFX de nodos distintos.")]
    [SerializeField][Range(0.05f, 3f)] private float sfxFadeDuration = 0.5f;

    [Header("Audio — Typewriter")]
    [SerializeField] private AudioSource typewriterAudioSource;
    [SerializeField] private AudioClip[] typewriterSounds;
    [SerializeField][Range(1, 6)] private int charsPerSound = 2;

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.04f;

    // ── Eventos ───────────────────────────────────────────────────────────
    public static event Action OnDialogueEnded;
    public static event Action OnDialogueStarted;
    public static event Action<ProductType[]> OnWaitingForProductReached;

    // ── Estado interno ────────────────────────────────────────────────────
    private DialogueSequence _currentSequence;
    private int _currentNodeIndex = 0;
    private bool _isPaused = false;
    private bool _isTyping = false;
    private bool _isSfxBlocking = false;

    private Coroutine _typewriterRoutine;
    private Coroutine _sfxCrossfadeRoutine;  // Una sola coroutine maneja todo el crossfade
    private Coroutine _sfxBlockRoutine;

    // 🆕 CROSSFADE: A y B se alternan. _activeSfxSource apunta al que está sonando ahora.
    private AudioSource _activeSfxSource;
    private AudioSource _inactiveSfxSource;

    private int _charSoundCounter = 0;
    private readonly StringBuilder _typewriterBuffer = new StringBuilder(256);

    // ── Ciclo de vida ─────────────────────────────────────────────────────

    private void Awake()
    {
        // Inicializamos el crossfade: A es el activo al arrancar.
        _activeSfxSource = sfxSourceA;
        _inactiveSfxSource = sfxSourceB;

        sfxSourceA.volume = 1f;
        sfxSourceB.volume = 0f;
    }

    // ── API pública ───────────────────────────────────────────────────────

    public void StartSequence(DialogueSequence newSequence)
    {
        if (newSequence == null) return;

        _currentSequence = newSequence;
        _currentNodeIndex = 0;
        _isPaused = false;
        dialoguePanel.SetActive(true);

        OnDialogueStarted?.Invoke();
        DisplayCurrentNode();
    }

    public void ResumeAfterDelivery()
    {
        if (!_isPaused) return;

        _isPaused = false;
        _currentNodeIndex++;
        dialoguePanel.SetActive(true);

        if (_currentNodeIndex < _currentSequence.nodes.Length)
            DisplayCurrentNode();
        else
            EndSequence();
    }

    // ── Display ───────────────────────────────────────────────────────────

    private void DisplayCurrentNode()
    {
        DialogueNode node = _currentSequence.nodes[_currentNodeIndex];

        // Nombre
        if (speakerNameLabel != null)
        {
            speakerNameLabel.text = node.speakerName;
            speakerNameLabel.enabled = !string.IsNullOrEmpty(node.speakerName);
        }

        // Imagen de fondo
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

        // 🔧 SFX: solo actuamos si el nodo tiene un clip asignado.
        // Si es null, el audio anterior sigue sonando sin interrupciones.
        if (node.sfx != null)
            PlaySfxWithCrossfade(node.sfx, node.blockAdvanceUntilSfxEnd);

        // Typewriter
        if (_typewriterRoutine != null) StopCoroutine(_typewriterRoutine);
        _typewriterRoutine = StartCoroutine(TypewriterRoutine(node.dialogueText));
    }

    // ── Crossfade ─────────────────────────────────────────────────────────

    /// <summary>
    /// Arranca el crossfade entre el SFX activo y el nuevo clip.
    /// Si el nuevo clip es null, solo hace fade out del activo.
    /// </summary>
    private void PlaySfxWithCrossfade(AudioClip newClip, bool blocking)
    {
        // Cancelamos cualquier crossfade o bloqueo en curso
        if (_sfxCrossfadeRoutine != null) StopCoroutine(_sfxCrossfadeRoutine);
        if (_sfxBlockRoutine != null) StopCoroutine(_sfxBlockRoutine);
        _isSfxBlocking = false;

        _sfxCrossfadeRoutine = StartCoroutine(CrossfadeRoutine(newClip));

        if (newClip != null && blocking)
        {
            _isSfxBlocking = true;
            _sfxBlockRoutine = StartCoroutine(WaitForSfxRoutine(newClip.length));
        }
    }

    /// <summary>
    /// Hace fade out del AudioSource activo y fade in del inactivo con el nuevo clip.
    /// Al terminar, intercambia los roles para el próximo crossfade.
    /// </summary>
    private IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        // Preparamos el inactivo con el nuevo clip, volumen 0, y lo arrancamos
        _inactiveSfxSource.clip = newClip;
        _inactiveSfxSource.volume = 0f;

        if (newClip != null)
            _inactiveSfxSource.Play();

        float elapsed = 0f;

        while (elapsed < sfxFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / sfxFadeDuration;

            _activeSfxSource.volume = Mathf.Lerp(1f, 0f, t); // Fade OUT
            _inactiveSfxSource.volume = Mathf.Lerp(0f, 1f, t); // Fade IN

            yield return null;
        }

        // Valores finales exactos — Lerp no garantiza llegar a 0/1 perfectamente
        _activeSfxSource.volume = 0f;
        _inactiveSfxSource.volume = 1f;

        // Paramos el que quedó en silencio para liberar recursos
        _activeSfxSource.Stop();

        // 🔄 Swap: el que acaba de sonar es el nuevo activo
        (_activeSfxSource, _inactiveSfxSource) = (_inactiveSfxSource, _activeSfxSource);

        _sfxCrossfadeRoutine = null;
    }

    /// <summary>
    /// Espera a que el SFX bloqueante termine y libera el avance.
    /// </summary>
    private IEnumerator WaitForSfxRoutine(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        _isSfxBlocking = false;
        _sfxBlockRoutine = null;
    }

    // ── Typewriter ────────────────────────────────────────────────────────

    private IEnumerator TypewriterRoutine(string fullText)
    {
        _isTyping = true;
        _charSoundCounter = 0;
        _typewriterBuffer.Clear();
        dialogueText.text = string.Empty;

        for (int i = 0; i < fullText.Length; i++)
        {
            _typewriterBuffer.Append(fullText[i]);
            dialogueText.text = _typewriterBuffer.ToString(); // 🔴 GC ALLOC — aceptable

            if (fullText[i] != ' ' && fullText[i] != '\n')
            {
                _charSoundCounter++;
                if (_charSoundCounter >= charsPerSound)
                {
                    _charSoundCounter = 0;
                    PlayTypewriterSound();
                }
            }

            yield return new WaitForSeconds(charDelay);
        }

        _isTyping = false;
        _typewriterRoutine = null;
        if (typewriterAudioSource != null) typewriterAudioSource.Stop();
    }

    private void PlayTypewriterSound()
    {
        if (typewriterAudioSource == null) return;
        if (typewriterSounds == null || typewriterSounds.Length == 0) return;
        if (typewriterAudioSource.isPlaying) return; // 🛡️ Anti-overlap

        typewriterAudioSource.clip = typewriterSounds[UnityEngine.Random.Range(0, typewriterSounds.Length)];
        typewriterAudioSource.Play();
    }

    // ── Input ─────────────────────────────────────────────────────────────

    private void AdvanceDialogue()
    {
        if (_currentSequence == null) return;

        // 1. SFX bloqueante: el jugador tiene que esperar
        if (_isSfxBlocking) return;

        // 2. Typewriter en curso: skip
        if (_isTyping)
        {
            SkipTypewriter();
            return;
        }

        // 3. Pausado esperando entrega de producto
        if (_isPaused) return;

        // 4. Avance normal — el crossfade se dispara en DisplayCurrentNode()
        //    a través de PlaySfxWithCrossfade(), que reemplaza el SFX activo
        //    con fade en lugar de cortarlo.
        _currentNodeIndex++;

        if (_currentNodeIndex >= _currentSequence.nodes.Length)
        {
            EndSequence();
            return;
        }

        DialogueNode nextNode = _currentSequence.nodes[_currentNodeIndex];

        if (nextNode.isWaitingForProductNode)
        {
            _isPaused = true;
            dialoguePanel.SetActive(false);
            StopAllTypewriter();

            // Fade out del SFX al entrar en gameplay
            PlaySfxWithCrossfade(null, false);

            OnWaitingForProductReached?.Invoke(nextNode.productsToDeliver);
            return;
        }

        DisplayCurrentNode();
    }

    private void SkipTypewriter()
    {
        if (_typewriterRoutine != null)
        {
            StopCoroutine(_typewriterRoutine);
            _typewriterRoutine = null;
        }
        dialogueText.text = _currentSequence.nodes[_currentNodeIndex].dialogueText;
        _isTyping = false;
        if (typewriterAudioSource != null) typewriterAudioSource.Stop();
    }

    private void EndSequence()
    {
        StopAllTypewriter();

        // Fade out del SFX activo al terminar la secuencia
        PlaySfxWithCrossfade(null, false);

        _currentSequence = null;
        dialoguePanel.SetActive(false);
        OnDialogueEnded?.Invoke();
    }

    private void StopAllTypewriter()
    {
        if (_typewriterRoutine != null)
        {
            StopCoroutine(_typewriterRoutine);
            _typewriterRoutine = null;
        }
        _isTyping = false;
        if (typewriterAudioSource != null) typewriterAudioSource.Stop();
    }

    private void OnEnable()
    {
        InputManager.OnAdvanceDialogue += AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated += StartSequence;
    }

    private void OnDisable()
    {
        InputManager.OnAdvanceDialogue -= AdvanceDialogue;
        NPCDialogue.OnDialogueInitiated -= StartSequence;
    }
}