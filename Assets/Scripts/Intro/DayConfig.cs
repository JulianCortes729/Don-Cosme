using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contenedor de datos que define todo lo que ocurre a nivel narrativo en un día específico.
/// </summary>
/// <remarks>
/// Este ScriptableObject agrupa las secuencias de diálogo que se reproducen durante
/// una jornada: la secuencia introductoria al despertar y la lista de diálogos de
/// los clientes que aparecerán en el transcurso del día.
/// </remarks>
[CreateAssetMenu(fileName = "New Day Config", menuName = "Dialogue/Day Config")]
public class DayConfig : ScriptableObject
{
    [Header("Secuencias del Día")]
    /// <summary>
    /// Secuencia de diálogo que se reproduce automáticamente al inicio del día
    /// (por ejemplo, un monólogo o introducción narrativa).
    /// </summary>
    /// <remarks>
    /// Se recomienda usar esta secuencia para presentaciones o eventos que deban
    /// ocurrir siempre al comenzar la jornada. Puede ser null si no se requiere.
    /// </remarks>
    [Tooltip("El monólogo o diálogo automático que se reproduce al despertar en este día.")]
    public DialogueSequence introSequence;


    [Header("Reparto (Fase de Preparación)")]
    [Tooltip("Los prefabs de los productos/cajas que aparecerán en el suelo hoy.")]
    public GameObject[] deliveryPrefabs; // 📌 GDD: Lo que llega en el camión

    /// <summary>
    /// Array ordenado de secuencias de diálogo correspondientes a los clientes
    /// que llegarán durante este día.
    /// </summary>
    /// <remarks>
    /// El orden del array define el orden de aparición de los clientes. Puede estar
    /// vacío si no se esperan clientes ese día.
    /// </remarks>
    [Tooltip("Lista ordenada de clientes que llegarán hoy.")]
    public ClientData[] clients; 
}