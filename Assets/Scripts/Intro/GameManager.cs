using System.Collections.Generic;
using UnityEngine;

public enum GameState { Cinematic, Preparing, Talking, Playing }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameState CurrentState { get; private set; }

    [Header("Referencias de Sistemas")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject panelGamePlay;
    [SerializeField] private ShopSign shopSign; // Referencia al cartel físico

    [Header("UI de Transición")]
    [SerializeField] private GameObject dayEndPanel; // 🆕 Panel de "Día Terminado"

    [Header("Puntos de Spawn")]
    [SerializeField] private Transform deliverySpawnPoint; // 📦 Donde caen las cajas
    [SerializeField] private float spawnScatter = 0.5f;   // Dispersión para que no choquen

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

    private DayConfig _currentDayConfig;
    private int _currentClientIndex = 0;
    private bool _isIntroPlaying = false;
    private ClientController _activeClient = null;

    // 🟢 POOL / ZERO ALLOC: Reutilizamos esta lista para todos los clientes en lugar de hacer 'new List'
    private List<ProductType> _pendingProducts = new List<ProductType>(10);

    // 🛡️ Estado anti-spam para la consola
    private ObjectGrabbable _lastRejectedProduct = null;

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
        // 1. Validar si quedan más días
        if (days == null || dayIndex >= days.Length)
        {
            Debug.Log("🏁 ¡Has completado todos los días del juego!");
            // Aquí podrías cargar una escena de créditos o menú principal
            return;
        }

        // 2. Limpieza de estado para el nuevo día
        _currentClientIndex = 0; // ⚠️ CRÍTICO: Resetear puntero de clientes
        _currentDayConfig = days[dayIndex];
        dayEndPanel.SetActive(false);
        panelGamePlay.SetActive(false);

        // 3. Arrancar flujo narrativo
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

        CurrentState = GameState.Cinematic; // Bloqueamos movimiento
        panelGamePlay.SetActive(false);

        // Mostramos feedback al jugador
        if (dayEndPanel != null)
        {
            dayEndPanel.SetActive(true);
            // 💡 HINT: Aquí podrías sumar el dinero ganado hoy
        }

        // En un juego profesional, aquí esperaríamos a que el jugador 
        // presione un botón de "Siguiente Día" en la UI.
        // Para esta implementación, lo haremos automático tras 3 segundos.
        Invoke(nameof(AdvanceToNextDay), 3f);
    }

    private void AdvanceToNextDay()
    {
        currentDayIndex++;
        InitDay(currentDayIndex);
    }

    private void OnEnable()
    {
        DialogueManager.OnDialogueEnded += HandleDialogueEnded;
        NPCDialogue.OnDialogueInitiated += HandleDialogueStarted;
        DeliveryZone.OnProductDropped += HandleDelivery;
        ClientController.OnClientArrived += HandleClientArrived;
        ClientController.OnClientLeft += HandleClientLeft;
        DialogueManager.OnWaitingForProductReached += HandleWaitingForProduct;
    }

    private void OnDisable()
    {
        DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
        NPCDialogue.OnDialogueInitiated -= HandleDialogueStarted;
        DeliveryZone.OnProductDropped -= HandleDelivery;
        ClientController.OnClientArrived -= HandleClientArrived;
        ClientController.OnClientLeft -= HandleClientLeft;
        DialogueManager.OnWaitingForProductReached -= HandleWaitingForProduct;
    }

    private void HandleDialogueStarted(DialogueSequence sequence)
    {
        CurrentState = GameState.Talking;
    }

    private void HandleDialogueEnded()
    {
        if (_isIntroPlaying)
        {
            _isIntroPlaying = false;
            StartPreparationPhase(); // 🆕 Después de la intro, a acomodar cajas!
        }
        else
        {
            if (_activeClient != null) _activeClient.Leave();
        }
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

    private void HandleWaitingForProduct()
    {
        CurrentState = GameState.Playing;
        panelGamePlay.SetActive(true);

        if (_activeClient != null)
        {
            _activeClient.SetWaitingForProduct();

            // 📌 GDD: Vaciamos la lista vieja y copiamos el pedido del cliente actual
            _pendingProducts.Clear();
            if (_activeClient.Data.RequestedProducts != null)
            {
                _pendingProducts.AddRange(_activeClient.Data.RequestedProducts);
            }
        }

        Debug.Log($"[GameManager] Esperando entrega de {_pendingProducts.Count} productos...");
    }

    private void HandleDelivery(ObjectGrabbable deliveredProduct)
    {
        if (CurrentState != GameState.Playing || _activeClient == null || _activeClient.State != ClientState.WaitingForProduct) return;

        // Validamos si el producto entregado existe en la lista de pendientes
        if (_pendingProducts.Contains(deliveredProduct.Type))
        {
            // Tachamos un producto de la lista
            _pendingProducts.Remove(deliveredProduct.Type);
            Debug.Log($"[GameManager] ¡Producto correcto! Faltan entregar: {_pendingProducts.Count}");

            SpawnMoney(deliveredProduct.transform.position);
            _lastRejectedProduct = null;

            // 🔴 GC ALLOC: En el futuro aplicaremos Object Pooling aquí
            Destroy(deliveredProduct.gameObject);

            // Si la lista de pendientes llegó a cero, completamos la orden
            if (_pendingProducts.Count == 0)
            {
                Debug.Log("[GameManager] ¡Orden completada! Despidiendo al cliente...");
                dialogueManager.ResumeAfterDelivery();
            }
        }
        else
        {
            if (_lastRejectedProduct != deliveredProduct)
            {
                Debug.LogWarning($"[GameManager] RECHAZADO: El cliente no pidió {deliveredProduct.Type} o ya se lo entregaste.");
                _lastRejectedProduct = deliveredProduct;
            }
        }
    }

    private void SpawnNextClient()
    {
        if (_currentDayConfig == null || _currentDayConfig.clients == null) return;

        // Comprobar si terminamos la lista de clientes de hoy
        if (_currentClientIndex >= _currentDayConfig.clients.Length)
        {
            EndCurrentDay(); // 🆕 Disparar el cambio de día
            return;
        }

        ClientData nextData = _currentDayConfig.clients[_currentClientIndex];
        _currentClientIndex++;

        _activeClient = Instantiate(clientControllerPrefab, spawnPoint.position, spawnPoint.rotation);
        _activeClient.InjectWaypoints(spawnPoint, windowPoint, exitPoint);
        _activeClient.Initialize(nextData);
    }

    private void SpawnMoney(Vector3 originPosition)
    {
        if (moneyPrefab == null) return;

        for (int i = 0; i < billsToSpawn; i++)
        {
            Vector3 spawnPos = originPosition + new Vector3(Random.Range(-0.1f, 0.1f), 0.3f, Random.Range(-0.1f, 0.1f));
            GameObject bill = Instantiate(moneyPrefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));

            if (bill.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(Vector3.up * Random.Range(0f, 0.3f), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * Random.Range(0f, 0.3f), ForceMode.Impulse);
            }
        }
    }


    private void StartPreparationPhase()
    {
        CurrentState = GameState.Preparing;
        panelGamePlay.SetActive(true); // El jugador ya puede moverse
        if (shopSign) shopSign.ResetSign();

        SpawnDailyDelivery();
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
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnScatter, spawnScatter),
                0,
                Random.Range(-spawnScatter, spawnScatter)
            );

            // 🔴 GC ALLOC: Instanciación de mercadería
            Instantiate(prefab, deliverySpawnPoint.position + randomOffset, Quaternion.identity);
        }
    }
}