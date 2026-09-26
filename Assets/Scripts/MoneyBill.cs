using UnityEngine;

/// <summary>
/// Representa un billete físico que el jugador puede recolectar con la mirada (Raycast).
/// Incluye configuración defensiva de físicas para evitar el "Physics Pop".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MoneyBill : MonoBehaviour, IInteractable
{
    [Header("Feedback")]
    [Tooltip("Sonido opcional al recoger el billete")]
    [SerializeField] private AudioClip collectSound;
    

    public void Interact(GameObject interactor)
    {
        // Feedback Sonoro: PlayClipAtPoint permite que el sonido suene
        // en el mundo 3D independientemente de que este objeto sea destruido al instante.
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Debug.Log("💸 ¡Billete recogido!");

        // Sumar dinero al GameManager aquí (ej: GameManager.Instance.AddMoney(10);)

        // Limpieza del objeto
        Destroy(gameObject);
    }
}