using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Lista cerrada de todos los productos disponibles en el juego.
/// Tipo por valor (Zero-Allocation).
/// </summary>
public enum ProductType
{
    Ninguno,
    Alfajor,
    Papitas,
    Gaseosa,
    Galletitas,
    Cigarrillos,
    Caramelos,
    Chicles,
    Bizcochos
}

// 1. LA CAJA PEQUEÑA (Un cuadro de tu storyboard)
[System.Serializable]
/// <summary>
/// Representa un único "nodo" o cuadro de diálogo dentro de una secuencia.
/// Contiene el texto a mostrar, un SFX opcional y una imagen de fondo opcional.
/// </summary>
public struct DialogueNode
{

    [Tooltip("Nombre del personaje que habla. Ej: 'Laura', 'Don Cosme', '?'")]
    public string speakerName;
    /// <summary>
    /// Texto que se mostrará en este nodo de diálogo.
    /// </summary>
    [TextArea(3, 5)]
    public string dialogueText; // Texto del diálogo

    /// <summary>
    /// Efecto de sonido opcional que se reproducirá al mostrar este nodo.
    /// </summary>
    public AudioClip sfx; // Audio del diálogo

    /// <summary>
    /// Imagen de fondo opcional que se mostrará mientras este nodo está activo.
    /// </summary>
    public Sprite backgroundImage;

    [Tooltip("Si está activo, el diálogo se pausa aquí hasta que el jugador entregue el pedido.")]
    public bool isWaitingForProductNode; // 🆕 Nodo marcador de entrega
}


// 2. LA CAJA GRANDE (El Día / La Conversación entera)
[CreateAssetMenu(fileName = "New Sequence", menuName = "Dialogue/Sequence")] // MAGIA: Esto crea un botón en el menú de Unity para fabricar este archivo
/// <summary>
/// ScriptableObject que agrupa una secuencia ordenada de <see cref="DialogueNode"/>.
/// Cada elemento del array representa un cuadro del diálogo que se reproducirá en orden.
/// </summary>
public class DialogueSequence : ScriptableObject
{
    /// <summary>
    /// Array ordenado de nodos que componen la secuencia de diálogo.
    /// El orden en el array determina el orden de reproducción.
    /// </summary>
    public DialogueNode[] nodes; // Una lista o arreglo de las cajas pequeñas

}
