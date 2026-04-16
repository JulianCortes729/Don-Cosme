using System;
using UnityEngine;

/// <summary>
/// Representa el mostrador físico. Detecta cuando un objeto cae en él
/// y avisa al sistema central.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DeliveryZone : MonoBehaviour
{
    /// <summary>
    /// Evento disparado cuando se deposita un producto válido en el mostrador.
    /// </summary>
    public static event Action<ObjectGrabbable> OnProductDropped;

    private void OnTriggerStay(Collider other)
    {
        // ⚠️ SOLID (Liskov): Usamos TryGetComponent para no asumir qué chocó.
        if (other.TryGetComponent<ObjectGrabbable>(out ObjectGrabbable product))
        {
            // Solo lo aceptamos si el jugador NO lo tiene agarrado (lo soltó)
            if (!product.IsGrabbed)
            {
                OnProductDropped?.Invoke(product);
            }
        }
    }
}