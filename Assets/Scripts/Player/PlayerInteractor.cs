using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja la lógica de interacción del jugador con objetos interactuables en escena.
/// Este componente realiza un raycast desde la cámara para buscar objetos que
/// implementen <see cref="IInteractable"/> y permite agarrarlos/soltarlos con la tecla E.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    /// <summary>
    /// Transform de la cámara (origen del raycast). Normalmente es la cámara del jugador.
    /// </summary>
    [SerializeField] private Transform cameraTransform;

    /// <summary>
    /// gameObject donde se colocará el objeto cuando el jugador lo agarre.
    /// </summary>
    [SerializeField] private GameObject grabPointObject;
    public Transform GrabPoint => grabPointObject.transform;

    /// <summary>
    /// Distancia máxima (en unidades) a la que el jugador puede interactuar con objetos.
    /// </summary>
    [SerializeField] private float interactDistance = 3f;

    /// <summary>
    /// Máscara de capas usada por el raycast para filtrar únicamente objetos interactuables.
    /// </summary>
    [SerializeField] private LayerMask interactableLayer;

    /// <summary>
    /// Referencia al GameObject actualmente agarrado por el jugador. Null si no hay ninguno.
    /// Se guarda solo si el objeto implementa <see cref="IDroppable"/>, lo que permite soltarlo.
    /// </summary>
    private GameObject currentObjectGrabbable;

    private void OnEnable()
    {
        // Nos suscribimos al evento centralizado de entrada para manejar interacciones.
        InputManager.OnInteractPressed += GrabObject;
    }
    private void OnDisable()
    {
        // Nos desuscribimos para evitar llamadas fuera de vida del objeto.
        InputManager.OnInteractPressed -= GrabObject;
    }

    private void GrabObject()
    {
        // Si no estamos en estado Playing, no procesamos interacciones.
        if (GameManager.CurrentState == GameState.Cinematic) return;

        if (currentObjectGrabbable == null)
        {
            // Intentar detectar un objeto interactuable con un raycast desde la cámara.
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDistance, interactableLayer))
            {
                // Usamos TryGetComponent para comprobar si el collider tiene IInteractable.
                if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
                {
                    // Pasamos el GameObject del grab point para que el objeto sepa
                    // dónde debe posicionarse mientras esté agarrado.
                    interactable.Interact(grabPointObject);

                    // Si además implementa IDroppable, guardamos la referencia para poder soltarlo.
                    if (hit.collider.TryGetComponent<IDroppable>(out _))
                    {
                        currentObjectGrabbable = hit.collider.gameObject;
                    }
                }
            }
        }
        else
        {
            // Si ya tenemos un objeto agarrado, intentamos soltarlo mediante IDroppable.
            if (currentObjectGrabbable.TryGetComponent<IDroppable>(out IDroppable droppable))
            {
                droppable.Drop();
                currentObjectGrabbable = null;
            }
        }
    }
        
   
}
