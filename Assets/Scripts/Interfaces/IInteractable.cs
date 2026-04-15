using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contrato que deben implementar los objetos que puedan ser interactuados
/// por el jugador (por ejemplo, recogidos o soltados).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Llamado cuando el jugador interactúa con el objeto para agarrarlo.
    /// Se proporciona un <paramref name="grabPoint"/> que representa la
    /// posición/transform donde el objeto debe colocarse mientras esté agarrado.
    /// </summary>
    /// <param name="grabPoint">Transform objetivo al que el objeto debe seguir mientras está agarrado.</param>
    void Interact(GameObject grabPoint);

    
}
