using UnityEngine;

/// <summary>
/// Datos de un cliente: qué modelo usa, qué dice, qué pide y cuánto paga.
/// Creá uno por tipo de cliente desde Create > Client > Client Data.
/// </summary>
[CreateAssetMenu(fileName = "ClientData_New", menuName = "Client/Client Data")]
public class ClientData : ScriptableObject
{
    [Header("Presentación")]
    [Tooltip("Prefab del modelo 3D del cliente.")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Diálogo")]
    [Tooltip("Secuencia de diálogo que incluye el pedido.")]
    [SerializeField] private DialogueSequence dialogueSequence;

    [Header("Pedido Múltiple")]
    [Tooltip("Lista de productos que este cliente va a pedir.")]
    [SerializeField] private ProductType[] requestedProducts;

    [Header("Recompensa")]
    [Tooltip("Dinero que deja el cliente al ser atendido correctamente.")]
    [SerializeField] private int rewardAmount = 10;

    // ── Propiedades ───────────────────────────────────────────────────────
    public GameObject ClientPrefab => clientPrefab;
    public DialogueSequence DialogueSequence => dialogueSequence;
    public ProductType[] RequestedProducts => requestedProducts;
    public int RewardAmount => rewardAmount;
}