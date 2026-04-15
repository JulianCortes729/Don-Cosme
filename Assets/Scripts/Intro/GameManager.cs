using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa los posibles estados del juego gestionados por <see cref="GameManager"/>.
/// </summary>
public enum GameState { Cinematic, Playing }

/// <summary>
/// Coordinador central del flujo de la partida: control de estados (cinemático/jugando),
/// gestión de las secuencias del día y orquestación del NPC de la ventana.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Estado actual del juego. Lectura pública, asignación privada.
    /// </summary>
    public static GameState CurrentState { get; private set; }

    [Header("Referencias de Sistemas")]
    /// <summary>
    /// Referencia al <see cref="DialogueManager"/> encargado de mostrar diálogos en pantalla.
    /// </summary>
    [SerializeField] private DialogueManager dialogueManager;

    /// <summary>
    /// Referencia al componente <see cref="NPCDialogue"/> del NPC situado en la ventana.
    /// Se usa para inyectar secuencias de diálogo a medida que llegan clientes.
    /// </summary>
    [Tooltip("El script NPCDialogue del personaje estacionado en la ventana.")]
    [SerializeField] private NPCDialogue windowNPC;

    /// <summary>
    /// Panel de interfaz que se muestra durante el modo de juego (HUD, controles, etc.).
    /// </summary>
    [SerializeField] private GameObject panelGamePlay;

    [Header("Gestión de Días")]
    /// <summary>
    /// Configuración por días (ScriptableObjects) que definen las secuencias narrativas.
    /// </summary>
    [SerializeField] private DayConfig[] days;

    /// <summary>
    /// Índice del día actual dentro del array <see cref="days"/>.
    /// </summary>
    [SerializeField] private int currentDayIndex = 0;

    // Variables de Estado Interno
    /// <summary>
    /// Configuración del día actualmente activo o null si no está definida.
    /// </summary>
    private DayConfig currentDayConfig;

    /// <summary>
    /// Índice del cliente actual dentro de la configuración del día.
    /// </summary>
    private int currentClientIndex = 0;

    /// <summary>
    /// Flag que indica si la intro del día se está reproduciendo actualmente.
    /// </summary>
    private bool isIntroPlaying = false;

    /// <summary>
    /// Inicialización: desactiva la UI de gameplay, establece el estado inicial y
    /// lanza la secuencia de introducción del día si existe, o carga el primer cliente.
    /// </summary>
    private void Start()
    {
        panelGamePlay.SetActive(false);
        CurrentState = GameState.Cinematic;

        if (days != null && currentDayIndex < days.Length)
        {
            currentDayConfig = days[currentDayIndex];

            // Lanzamos la intro si existe
            if (currentDayConfig.introSequence != null)
            {
                isIntroPlaying = true;
                dialogueManager.StartSequence(currentDayConfig.introSequence);
            }
            else
            {
                // Si no hay intro, pasamos directamente a cargar el primer cliente
                UnlockPlayerAndLoadNextClient();
            }
        }
        else
        {
            Debug.LogWarning("No hay días configurados.");
            UnlockPlayerAndLoadNextClient();
        }
    }

    private void OnEnable()
    {
        // Nos suscribimos para recibir notificaciones del DialogueManager y NPCs
        DialogueManager.OnDialogueEnded += HandleDialogueEnded;

        // Nos suscribimos para saber cuándo el jugador inicia una charla con el NPC
        NPCDialogue.OnDialogueInitiated += HandleDialogueStarted;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
        NPCDialogue.OnDialogueInitiated -= HandleDialogueStarted;
    }

    /// <summary>
    /// Manejador llamado cuando se inicia una charla (por ejemplo, el jugador interactúa con el NPC).
    /// Bloquea los controles del jugador y oculta la UI de gameplay para la cinemática.
    /// </summary>
    /// <param name="sequence">Secuencia que se está iniciando (no usada directamente aquí).</param>
    private void HandleDialogueStarted(DialogueSequence sequence)
    {
        CurrentState = GameState.Cinematic;
        panelGamePlay.SetActive(false);
    }

    /// <summary>
    /// Manejador llamado cuando finaliza una secuencia de diálogo.
    /// Dependiendo de si era la intro o una charla con cliente, avanza el flujo del día.
    /// </summary>
    private void HandleDialogueEnded()
    {
        if (isIntroPlaying)
        {
            // Terminó la intro de la mañana: liberamos al jugador y cargamos al primer cliente
            isIntroPlaying = false;
            UnlockPlayerAndLoadNextClient();
        }
        else
        {
            // Terminó de hablar con un cliente: liberamos al jugador y preparamos al siguiente
            UnlockPlayerAndLoadNextClient();
        }
    }

    /// <summary>
    /// Pone el juego en estado <see cref="GameState.Playing"/>, muestra la UI de gameplay
    /// y, si quedan clientes para el día actual, configura el NPC de ventana con la
    /// siguiente secuencia de diálogo. Si no quedan clientes, oculta el NPC.
    /// </summary>
    private void UnlockPlayerAndLoadNextClient()
    {
        CurrentState = GameState.Playing;
        panelGamePlay.SetActive(true);

        if (currentDayConfig == null || currentDayConfig.clientSequences == null) return;

        // Verificamos si aún quedan clientes en la fila para este día
        if (currentClientIndex < currentDayConfig.clientSequences.Length)
        {
            DialogueSequence nextClientSequence = currentDayConfig.clientSequences[currentClientIndex];

            // ZERO ALLOC: Inyectamos los datos en el NPC existente sin instanciar nada nuevo
            windowNPC.SetSequence(nextClientSequence);
            windowNPC.gameObject.SetActive(true); // Aseguramos que el NPC esté visible

            currentClientIndex++; // Avanzamos el índice para el próximo cliente
        }
        else
        {
            // Ya no quedan clientes. El turno terminó.
            windowNPC.SetSequence(null);
            windowNPC.gameObject.SetActive(false); // Ocultamos al NPC de la ventana
            Debug.Log("¡El turno ha terminado por hoy!");
        }
    }
}
