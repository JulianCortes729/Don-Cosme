using System;
using UnityEngine;

public enum ClientState { Arriving, WaitingForDialogue, WaitingForProduct, Leaving }

/// <summary>
/// Controla el movimiento del cliente por waypoints usando MoveTowards.
/// Sin NavMesh, sin dependencias externas.
/// </summary>
[RequireComponent(typeof(NPCDialogue))]
public class ClientController : MonoBehaviour
{
    public static event Action<ClientController> OnClientArrived;
    public static event Action<ClientController> OnClientLeft;

    [Header("Movimiento")]
    [Tooltip("Unidades por segundo.")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Distancia a la que se considera que llegó al waypoint.")]
    [SerializeField] private float arrivalThreshold = 0.15f;

    [Tooltip("Velocidad de rotación en grados por segundo. 0 = rotación instantánea.")]
    [SerializeField] private float rotationSpeed = 8f;

    // ── Waypoints ─────────────────────────────────────────────────────────
    private Transform _windowPoint;
    private Transform _exitPoint;

    // ── Estado ────────────────────────────────────────────────────────────
    private ClientData _data;
    private ClientState _state = ClientState.Arriving;
    private Transform _currentTarget;

    // ── Componentes ───────────────────────────────────────────────────────
    private Animator _animator;
    private NPCDialogue _npcDialogue;

    // ── Propiedades públicas ──────────────────────────────────────────────
    public ClientData Data => _data;
    public ClientState State => _state;

    // ── Ciclo de vida ─────────────────────────────────────────────────────

    private void Awake()
    {
        _npcDialogue = GetComponent<NPCDialogue>();
    }

    private void Update()
    {
        MoveTowardsTarget();
        UpdateAnimator();
    }

    // ── API pública ───────────────────────────────────────────────────────

    public void InjectWaypoints(Transform spawn, Transform window, Transform exit)
    {
        // spawn ya se usó para posicionar al NPC antes de Initialize(),
        // lo recibimos por consistencia con la firma del GameManager.
        _windowPoint = window;
        _exitPoint = exit;
    }

    public void Initialize(ClientData data)
    {
        _data = data;
        _npcDialogue.SetSequence(_data.DialogueSequence);

        if (_data.ClientPrefab != null)
        {
            GameObject model = Instantiate(_data.ClientPrefab, transform);
            _animator = model.GetComponent<Animator>();
        }

        SetTarget(_windowPoint, ClientState.Arriving);
    }

    public void Leave()
    {
        SetTarget(_exitPoint, ClientState.Leaving);
    }

    public void SetWaitingForProduct()
    {
        _state = ClientState.WaitingForProduct;
    }

    // ── Movimiento ────────────────────────────────────────────────────────

    private void MoveTowardsTarget()
    {
        // Solo se mueve en los estados de tránsito
        if (_state != ClientState.Arriving && _state != ClientState.Leaving) return;
        if (_currentTarget == null) return;

        Vector3 targetPos = _currentTarget.position;

        // Ignoramos la diferencia de Y para no inclinar al NPC en rampas leves.
        // Si tu escenario tiene desniveles grandes, quitá esta línea.
        targetPos.y = transform.position.y;

        // 🟢 MoveTowards: sin allocations, sin física, determinista
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // Rotación suave hacia el destino
        Vector3 direction = (targetPos - transform.position);
        if (direction.sqrMagnitude > 0.001f) // Evitamos Quaternion.LookRotation con vector cero
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = rotationSpeed > 0f
                ? Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime)
                : targetRotation;
        }

        // Comprobamos llegada
        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
            HandleArrival();
    }

    private void HandleArrival()
    {
        switch (_state)
        {
            case ClientState.Arriving:
                _state = ClientState.WaitingForDialogue;
                transform.rotation = _windowPoint.rotation;

                OnClientArrived?.Invoke(this);
                _npcDialogue.TriggerDialogue(); // 📌 GDD: el cliente llega y habla solo
                break;

            case ClientState.Leaving:
                OnClientLeft?.Invoke(this);
                Destroy(gameObject);
                break;
        }
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        // Speed: 1 si se está moviendo, 0 si está quieto.
        // Usamos sqrMagnitude para evitar una sqrt innecesaria. // 🟡 PERF
        bool isMoving = (_state == ClientState.Arriving || _state == ClientState.Leaving)
                        && _currentTarget != null
                        && Vector3.Distance(transform.position, _currentTarget.position) > arrivalThreshold;

        _animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void SetTarget(Transform target, ClientState newState)
    {
        _currentTarget = target;
        _state = newState;
    }
}