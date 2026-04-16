using System;
using UnityEngine;
using UnityEngine.AI;

public enum ClientState { Arriving, WaitingForDialogue, WaitingForProduct, Leaving }

/// <summary>
/// Controla el movimiento del cliente y ahora transporta su propio componente de diálogo.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCDialogue))] // 🟢 SOLID (Composición): Reutilizamos tu script de interacción
public class ClientController : MonoBehaviour
{
    public static event Action<ClientController> OnClientArrived;
    public static event Action<ClientController> OnClientLeft;

    [Header("Configuración")]
    [SerializeField] private float arrivalThreshold = 0.2f;

    private Transform _spawnPoint;
    private Transform _windowPoint;
    private Transform _exitPoint;

    private ClientData _data;
    private ClientState _state = ClientState.Arriving;

    private NavMeshAgent _agent;
    private Animator _animator;
    private NPCDialogue _npcDialogue; // Referencia al diálogo interno

    public ClientData Data => _data;
    public ClientState State => _state;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _npcDialogue = GetComponent<NPCDialogue>(); // Obtenemos el script de diálogo
        _agent.stoppingDistance = arrivalThreshold;
    }

    private void Update()
    {
        if (_animator != null)
        {
            _animator.SetFloat("Speed", _agent.velocity.magnitude);
        }
        CheckArrival();
    }

    public void InjectWaypoints(Transform spawn, Transform window, Transform exit)
    {
        _spawnPoint = spawn;
        _windowPoint = window;
        _exitPoint = exit;
    }

    public void Initialize(ClientData data)
    {
        _data = data;

        // 📌 GDD: Le inyectamos la secuencia de diálogo directamente al cuerpo de ESTE cliente
        _npcDialogue.SetSequence(_data.DialogueSequence);

        if (_data.ClientPrefab != null)
        {
            GameObject model = Instantiate(_data.ClientPrefab, transform);
            _animator = model.GetComponent<Animator>();
        }

        _agent.Warp(_spawnPoint.position);
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

    private void SetTarget(Transform target, ClientState newState)
    {
        _state = newState;
        _agent.SetDestination(target.position);
    }

    private void CheckArrival()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.1f)
            {
                if (_state == ClientState.Arriving)
                {
                    _state = ClientState.WaitingForDialogue;
                    OnClientArrived?.Invoke(this);
                    _npcDialogue.TriggerDialogue(); // 📌 GDD: el cliente llega y habla solo
                }
                else if (_state == ClientState.Leaving)
                {
                    OnClientLeft?.Invoke(this);
                    Destroy(gameObject);
                }
            }
        }
    }
}