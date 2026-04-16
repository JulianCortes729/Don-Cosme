using UnityEngine;

/// <summary>
/// Representa un billete físico que el jugador puede recolectar con la mirada (Raycast).
/// </summary>
[RequireComponent(typeof(Rigidbody))] // Necesario para que el billete rebote en el mostrador
public class MoneyBill : MonoBehaviour, IInteractable
{
    [Header("Feedback")]
    [Tooltip("Sonido opcional al recoger el billete")]
    [SerializeField] private AudioClip collectSound;

    public void Interact(GameObject interactor)
    {
        // 1. Feedback Sonoro: Usamos PlayClipAtPoint para que el sonido se reproduzca
        // en el mundo 3D aunque este objeto se destruya en la siguiente línea.
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Debug.Log("💸 ¡Billete recogido!");

        // 2. Limpieza: Destruimos el objeto (🔴 GC ALLOC — futuro candidato a Object Pool)
        Destroy(gameObject);
    }
}
