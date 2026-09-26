using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Cinematic, Preparing, Talking, Playing }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameState CurrentState { get; private set; }

    [Header("Referencias de Sistemas")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject panelGamePlay;
    [SerializeField] private GameObject panelGameFinal;
    [SerializeField] private ShopSign shopSign; // Referencia al cartel físico
    [SerializeField] private TelephoneInteractable telephone; // 🆕 Referencia al Teléfono Físico

    [Header("UI de Transición")]
    [SerializeField] private GameObject dayEndPanel; // 🆕 Panel de "Día Terminado"

    [Header("Puntos de Spawn")]
    [SerializeField] private Transform deliverySpawnPoint; // 📦 Donde caen las cajas


    [Header("Spawn de Clientes")]
    [SerializeField] private ClientController clientControllerPrefab;

    [Header("Waypoints compartidos")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform windowPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Gestión de Días")]
    [SerializeField] private DayConfig[] days;
    [SerializeField] private int currentDayIndex = 0;

    [Header("Economía")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private int billsToSpawn = 3;
    [SerializeField] private Transform moneySpawnPoint; // 🆕 Ponelo sobre el mostrador en la escena

    private DayConfig _currentDayConfig;
    private int _currentClientIndex = 0;
    private bool _isIntroPlaying = false;
    private ClientController _activeClient = null;

    // 🟢 POOL / ZERO ALLOC: Reutilizamos esta lista para todos los clientes en lugar de hacer 'new List'
    private List<ProductType> _pendingProducts = new List<ProductType>(10);

    // 🛡️ Estado anti-spam para la consola
    private ObjectGrabbable _lastRejectedProduct = null;


    private bool _isOutroPlaying = false;

    private bool _isSpawning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 📌 GDD: Iniciamos el primer día
        InitDay(currentDayIndex);
    }

    /// <summary>
    /// Configura y arranca un día específico desde cero.
    /// </summary>
    private void InitDay(int dayIndex)
    {
        if (days == null || dayIndex >= days.Length)
        {
            if(panelGameFinal != null) panelGameFinal.SetActive(true);
            SceneManager.LoadScene("CreditsScene");
            return;
        }

        _currentClientIndex = 0;
        _currentDayConfig = days[dayIndex];
        if (dayEndPanel) dayEndPanel.SetActive(false);
        panelGamePlay.SetActive(false);

        // 🆕 Cambiamos la música al arrancar el día.
        // Si el DayConfig no tiene clip asignado, AudioManager ignora la llamada
        // y la música del día anterior sigue sonando.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(_currentDayConfig.gameplayMusic);

        if (_currentDayConfig.introSequence != null)
        {
            _isIntroPlaying = true;
            CurrentState = GameState.Cinematic;
            dialogueManager.StartSequence(_currentDayConfig.introSequence);
        }
        else
        {
            _isIntroPlaying = false;
            StartPreparationPhase();
        }
    }

    /// <summary>
    /// Llamado cuando el último cliente del día se retira.
    /// </summary>
    private void EndCurrentDay()
    {
        Debug.Log($"✅ Día {currentDayIndex + 1} completado.");
        panelGamePlay.SetActive(false);

        // 📌 GDD: Si el día tiene cinemática de cierre (ej: cumpleaños Día 3), la reproducimos.
        // HandleDialogueEnded se encarga de avanzar al día siguiente cuando termine.
        if (_currentDayConfig.outroSequence != null)
        {
            CurrentState = GameState.Cinematic;
            _isIntroPlaying = false;          // Reutilizamos el flag pero para el outro
            _isOutroPlaying = true;           // 🆕 Flag específico para no confundir flujos
            dialogueManager.StartSequence(_currentDayConfig.outroSequence);
        }
        else
        {
            CurrentState = GameState.Cinematic;
            if (dayEndPanel != null) dayEndPanel.SetActive(true);
            Invoke(nameof(AdvanceToNextDay), 3f);
        }
    }

    private void AdvanceToNextDay()
    {
        currentDayIndex++;
        InitDay(currentDayIndex);
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueEnded += HandleDialogueEnded;
        DialogueManager.OnWaitingForProductReached += HandleWaitingForProduct; // 🔧 firma nueva
        NPCDialogue.OnDialogueInitiated += HandleDialogueStarted;
        DeliveryZone.OnProductDropped += HandleDelivery;
        ClientController.OnClientArrived += HandleClientArrived;
        ClientController.OnClientLeft += HandleClientLeft;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
        DialogueManager.OnWaitingForProductReached -= HandleWaitingForProduct;
        NPCDialogue.OnDialogueInitiated -= HandleDialogueStarted;
        DeliveryZone.OnProductDropped -= HandleDelivery;
        ClientController.OnClientArrived -= HandleClientArrived;
        ClientController.OnClientLeft -= HandleClientLeft;
    }

    private void HandleDialogueStarted(DialogueSequence sequence)
    {
        CurrentState = GameState.Talking;
    }

    private void HandleDialogueEnded()
    {
        // Caso 1: Terminó el outro → arrancamos el día siguiente inmediatamente.
        // El dialoguePanel ya se cerró en EndSequence(), así que no hay
        // ningún frame de "mundo descubierto" si pasamos directo a InitDay().
        if (_isOutroPlaying)
        {
            _isOutroPlaying = false;
            AdvanceToNextDay(); // 🔧 directo, sin Invoke ni delay
            return;
        }

        CurrentState = GameState.Playing;
        panelGamePlay.SetActive(true);

        if (_isIntroPlaying)
        {
            _isIntroPlaying = false;
            StartPreparationPhase();
            return;
        }

        if (_activeClient != null)
            _activeClient.Leave();
        else
            SpawnNextClient();
    }

    private void HandleClientArrived(ClientController client)
    {
        Debug.Log("[GameManager] El cliente llegó a la ventana.");
    }

    private void HandleClientLeft(ClientController client)
    {
        _activeClient = null;
        SpawnNextClient();
    }

    // 🔧 FIX DEL LOOP: recibimos los productos directamente del nodo,
    // no los leemos de ClientData. Cada pausa en una conversación
    // tiene su propia lista independiente.
    private void HandleWaitingForProduct(ProductType[] products)
    {
        CurrentState = GameState.Playing;
        panelGamePlay.SetActive(true);

        if (_activeClient != null)
            _activeClient.SetWaitingForProduct();

        _pendingProducts.Clear();
        if (products != null)
            _pendingProducts.AddRange(products);

        Debug.Log($"[GameManager] Esperando {_pendingProducts.Count} producto(s)...");
    }

    private void HandleDelivery(ObjectGrabbable deliveredProduct)
    {
        if (CurrentState != GameState.Playing || _activeClient == null || _activeClient.State != ClientState.WaitingForProduct) return;

        if (_pendingProducts.Contains(deliveredProduct.Type))
        {
            _pendingProducts.Remove(deliveredProduct.Type);
            SpawnMoney();
            _lastRejectedProduct = null;
            Destroy(deliveredProduct.gameObject);

            if (_pendingProducts.Count == 0)
            {
                dialogueManager.ResumeAfterDelivery();
            }
        }
        else
        {
            if (_lastRejectedProduct != deliveredProduct)
            {
                Debug.LogWarning("Rechazado");
                _lastRejectedProduct = deliveredProduct;
            }
        }
    }

    private void SpawnNextClient()
    {
        // 🛡️ Si ya hay una coroutine de spawn corriendo, no arrancamos otra.
        if (_isSpawning) return;
        // 📌 GDD: Usamos una corrutina para poder tener pausas en el tiempo (Modo Simulación)
        StartCoroutine(SpawnClientRoutine());
    }

    private IEnumerator SpawnClientRoutine()
    {
        _isSpawning = true;

        if (_currentDayConfig == null || _currentDayConfig.clients == null)
        {
            _isSpawning = false;
            yield break;
        }

        if (_currentClientIndex >= _currentDayConfig.clients.Length)
        {
            _isSpawning = false;
            EndCurrentDay();
            yield break;
        }

        ClientData nextData = _currentDayConfig.clients[_currentClientIndex];
        _currentClientIndex++;

        if (nextData.SpawnDelay > 0)
            yield return new WaitForSeconds(nextData.SpawnDelay);

        if (nextData.IsPhoneCall)
        {
            _activeClient = null;
            if (telephone != null)
                telephone.StartRinging(nextData.DialogueSequence);
            else
            {
                Debug.LogWarning("⚠️ Telephone no asignado en GameManager.");
                _isSpawning = false;
                SpawnNextClient();
                yield break;
            }
        }
        else
        {
            _activeClient = Instantiate(clientControllerPrefab, spawnPoint.position, spawnPoint.rotation);
            _activeClient.InjectWaypoints(spawnPoint, windowPoint, exitPoint);
            _activeClient.Initialize(nextData);
        }

        _isSpawning = false;
    }

    private void SpawnMoney()  // 🆕 Ya no necesita originPosition
    {
        if (moneyPrefab == null) return;
        if (moneySpawnPoint == null)
        {
            Debug.LogWarning("[GameManager] Asigná el Money Spawn Point en el Inspector.");
            return;
        }

        for (int i = 0; i < billsToSpawn; i++)
        {
            Vector3 spawnPos = moneySpawnPoint.position + new Vector3(
                Random.Range(-0.15f, 0.15f),
                0f,
                Random.Range(-0.15f, 0.15f)
            );

            Instantiate(moneyPrefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            // No reseteamos velocidad acá — lo hace MoneyBill.Awake con WakeUp()
        }
    }


    private void StartPreparationPhase()
    {
        CurrentState = GameState.Preparing;
        panelGamePlay.SetActive(true);
        if (shopSign) shopSign.ResetSign();

        // 🆕 Delegamos TODA la responsabilidad del spawn a la corrutina. 
        // Solo la llamamos UNA vez.
        StartCoroutine(SpawnDeliveryRoutine());
    }
    public void StartSellingPhase()
    {
        CurrentState = GameState.Playing;
        SpawnNextClient();
    }

    private void SpawnDailyDelivery()
    {
        if (_currentDayConfig.deliveryPrefabs == null || deliverySpawnPoint == null) return;

        Debug.Log($"📦 Spawning {_currentDayConfig.deliveryPrefabs.Length} cajas de mercadería...");

        foreach (GameObject prefab in _currentDayConfig.deliveryPrefabs)
        {
            // 🟡 PERF: Spawning físico. Usamos un pequeño random offset para evitar el "Physics Pop"
            // (cuando dos objetos spawnean en el mismo sitio y salen disparados)
            Vector3 randomOffset = deliverySpawnPoint.position;

            // 🔴 GC ALLOC: Instanciación de mercadería
            Instantiate(prefab, deliverySpawnPoint.position + randomOffset, Quaternion.identity);
        }
    }


   
        private IEnumerator SpawnDeliveryRoutine()
    {
        if (_currentDayConfig.deliveryPrefabs == null || deliverySpawnPoint == null) yield break;

        foreach (GameObject prefab in _currentDayConfig.deliveryPrefabs)
        {
            // 1. Spawneamos en el PUNTO EXACTO, sin variaciones
            Instantiate(prefab, deliverySpawnPoint.position, Quaternion.identity);

            // 2. ⏳ EL TRUCO: Esperamos lo suficiente para que la caja anterior 
            // empiece a caer y deje el espacio libre para la nueva.
            // Si notas que siguen chocando, sube este número a 0.5f.
            yield return new WaitForSeconds(0.3f);
        }
    }

}