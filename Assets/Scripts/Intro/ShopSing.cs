using UnityEngine;

/// <summary>
/// Representa el cartel de Abierto/Cerrado. 
/// Permite al jugador decidir cuándo termina la fase de preparación.
/// </summary>
public class ShopSign : MonoBehaviour, IInteractable
{
    [Header("Feedback Visual")]
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    private bool _isOpened = false;

    public void Interact(GameObject interactor)
    {
        // Si ya abrimos o no estamos preparando, no hacemos nada
        if (_isOpened || GameManager.CurrentState != GameState.Preparing) return;

        _isOpened = true;

        // Feedback visual
        if (closedVisual) closedVisual.SetActive(false);
        if (openVisual) openVisual.SetActive(true);

        Debug.Log("📢 ¡Tienda abierta! Llamando al primer cliente...");

        // 🛰️ Notificamos al GameManager
        GameManager.Instance.StartSellingPhase();
    }

    public void ResetSign()
    {
        _isOpened = false;
        if (closedVisual) closedVisual.SetActive(true);
        if (openVisual) openVisual.SetActive(false);
    }
}