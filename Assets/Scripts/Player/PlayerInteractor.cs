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
    /// Punto (Transform) donde se colocará el objeto cuando el jugador lo agarre.
    /// </summary>
    [SerializeField] private Transform grabPoint;

    /// <summary>
    /// Distancia máxima (en unidades) a la que el jugador puede interactuar con objetos.
    /// </summary>
    [SerializeField] private float interactDistance = 3f;

    /// <summary>
    /// Máscara de capas usada por el raycast para filtrar únicamente objetos interactuables.
    /// </summary>
    [SerializeField] private LayerMask interactableLayer;

    /// <summary>
    /// Referencia al objeto interactuable actualmente agarrado (si lo hay).
    /// </summary>
    private IInteractable currentInteractable;

    /// <summary>
    /// Lógica que se ejecuta cada frame.
    /// - Detecta la pulsación de la tecla de interacción (E).
    /// - Si no hay un objeto agarrado, realiza un raycast desde la cámara para intentar
    ///   detectar un <see cref="IInteractable"/> y, en caso de encontrarlo, llama a
    ///   <see cref="IInteractable.Interact"/> pasando el <see cref="grabPoint"/>.
    /// - Si ya hay un objeto agarrado, llama a <see cref="IInteractable.Drop"/> y
    ///   libera la referencia.
    /// </summary>
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable == null)
            {
                // Intentar detectar un objeto interactuable con un raycast desde la cámara.
                RaycastHit hit;
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDistance, interactableLayer))
                {
                    // Usamos TryGetComponent para comprobar si el collider tiene IInteractable.
                    if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
                    {
                        currentInteractable = interactable;
                        interactable.Interact(grabPoint);
                    }
                }
            }
            else
            {
                // Si ya había un objeto agarrado, lo soltamos.
                currentInteractable.Drop();
                currentInteractable = null;
            }
        }
    }

}
