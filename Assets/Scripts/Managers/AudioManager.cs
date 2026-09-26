using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música")]
    [SerializeField] private AudioSource musicSource;

    [Header("Ducking")]
    [SerializeField][Range(0f, 1f)] private float normalVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float duckVolumeDialogue = 0f;
    [SerializeField][Range(0f, 1f)] private float duckVolumePhone = 0.3f;
    [SerializeField] private float duckSpeed = 0.4f;

    // ── Estado interno ─────────────────────────────────────────────────────
    private int _duckRequestCount = 0;
    private float _targetDuckVolume = 1f; // Arranca en normalVolume

    // 🔧 FIX: dos coroutines completamente separadas.
    // _musicCoroutine   → solo para crossfade de clip entre días
    // _duckCoroutine    → solo para subir/bajar volumen por ducking
    private Coroutine _musicCoroutine;
    private Coroutine _duckCoroutine;

    // ── Ciclo de vida ─────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (musicSource == null) return;
        musicSource.loop = true;
        musicSource.volume = normalVolume;
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueStarted += HandleDialogueStarted;
        DialogueManager.OnDialogueEnded += HandleDialogueEnded;
        TelephoneInteractable.OnPhoneStartRinging += HandlePhoneStartRinging;
        TelephoneInteractable.OnPhoneStopRinging += HandlePhoneStopRinging;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueStarted -= HandleDialogueStarted;
        DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
        TelephoneInteractable.OnPhoneStartRinging -= HandlePhoneStartRinging;
        TelephoneInteractable.OnPhoneStopRinging -= HandlePhoneStopRinging;
    }

    // ── Handlers ──────────────────────────────────────────────────────────

    private void HandleDialogueStarted() => RequestDuck(duckVolumeDialogue);
    private void HandleDialogueEnded() => ReleaseDuck();
    private void HandlePhoneStartRinging() => RequestDuck(duckVolumePhone);
    private void HandlePhoneStopRinging() => ReleaseDuck();

    // ── API pública ───────────────────────────────────────────────────────

    /// <summary>
    /// Cambia el clip de música con crossfade. No toca el ducking.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (_musicCoroutine != null) StopCoroutine(_musicCoroutine);
        _musicCoroutine = StartCoroutine(CrossfadeMusic(clip));
    }

    // ── Crossfade de clip ─────────────────────────────────────────────────

    /// <summary>
    /// Hace fade out → swap de clip → fade in.
    /// El fade respeta el volumen de ducking activo en cada momento.
    /// </summary>
    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade out hasta 0, partiendo del volumen actual
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duckSpeed)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duckSpeed);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in hasta _targetDuckVolume (no hasta normalVolume)
        // Así si hay ducking activo mientras cambia el día, respetamos ese estado.
        float targetVolume = _targetDuckVolume;
        elapsed = 0f;

        while (elapsed < duckSpeed)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duckSpeed);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _musicCoroutine = null;
    }

    // ── Ducking ───────────────────────────────────────────────────────────

    private void RequestDuck(float targetVolume)
    {
        _duckRequestCount++;
        _targetDuckVolume = _duckRequestCount == 1
            ? targetVolume
            : Mathf.Min(_targetDuckVolume, targetVolume);

        ApplyDuck(_targetDuckVolume);
    }

    private void ReleaseDuck()
    {
        _duckRequestCount = Mathf.Max(0, _duckRequestCount - 1);

        if (_duckRequestCount == 0)
        {
            _targetDuckVolume = normalVolume;
            ApplyDuck(normalVolume);
        }
    }

    /// <summary>
    /// Aplica el volumen de ducking con fade. No interfiere con _musicCoroutine.
    /// Si el crossfade está corriendo, lo dejamos terminar —
    /// él mismo leerá _targetDuckVolume al hacer fade in.
    /// </summary>
    private void ApplyDuck(float target)
    {
        // Si hay un crossfade de clip en curso, no lanzamos el duck lerp
        // porque el crossfade ya va a terminar en _targetDuckVolume.
        if (_musicCoroutine != null) return;

        if (_duckCoroutine != null) StopCoroutine(_duckCoroutine);
        _duckCoroutine = StartCoroutine(LerpVolume(target));
    }

    private IEnumerator LerpVolume(float target)
    {
        float start = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duckSpeed)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, elapsed / duckSpeed);
            yield return null;
        }

        musicSource.volume = target;
        _duckCoroutine = null;
    }
}