using UnityEngine;

/// <summary>
/// Datos de un cliente: modelo, diálogo, delay de spawn y recompensa.
/// Los productos pedidos ahora viven en los nodos isWaitingForProductNode
/// de la DialogueSequence asignada.
/// </summary>
[CreateAssetMenu(fileName = "ClientData_New", menuName = "Client/Client Data")]
public class ClientData : ScriptableObject
{
    [Header("Ritmo y Eventos")]
    [Tooltip("Segundos que tarda en llegar después de que se fue el anterior.")]
    [SerializeField] private float spawnDelay = 5f;

    [Tooltip("Si es verdadero, no hay NPC físico. En su lugar suena el teléfono.")]
    [SerializeField] private bool isPhoneCall = false;

    [Header("Presentación")]
    [Tooltip("Prefab del modelo 3D del cliente.")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Diálogo")]
    [Tooltip("Secuencia de diálogo. Los productos pedidos se configuran en los nodos isWaitingForProductNode.")]
    [SerializeField] private DialogueSequence dialogueSequence;

    [Header("Recompensa")]
    [Tooltip("Dinero que deja el cliente por cada producto entregado correctamente.")]
    [SerializeField] private int rewardPerProduct = 10;

    public float SpawnDelay => spawnDelay;
    public bool IsPhoneCall => isPhoneCall;
    public GameObject ClientPrefab => clientPrefab;
    public DialogueSequence DialogueSequence => dialogueSequence;
    public int RewardPerProduct => rewardPerProduct;
}