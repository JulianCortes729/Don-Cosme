using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interfaz que define el contrato mínimo para objetos que pueden ser soltados
/// o dejados por alguna entidad (por ejemplo, el jugador).
/// </summary>
public interface IDroppable 
{
    /// <summary>
    /// Método llamado para soltar el objeto. La implementación debe restaurar el
    /// comportamiento normal del objeto tras ser soltado (por ejemplo, reactivar
    /// la gravedad, limpiar referencias de agarre, aplicar una fuerza si es necesario, etc.).
    /// </summary>
    void Drop();
}
