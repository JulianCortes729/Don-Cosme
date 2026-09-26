using UnityEngine;

public enum ProductType
{
    Ninguno,
    Alfajor, Caramelos, Chicles, Galletitas, Obleas, Chocolate,
    Agua, Gaseosa, Jugo, Cerveza, Vino,
    Papitas, Bizcochos, Harina, Aceite, DulceDeLeche,
    Atun, Picadillo, PureDeTomate, Duraznos, Yerba,
    Cigarrillos, Velitas, Nachos
}

[System.Serializable]
public struct DialogueNode
{
    [Tooltip("Nombre del personaje que habla.")]
    public string speakerName;

    [TextArea(3, 5)]
    public string dialogueText;

    [Tooltip("SFX de ambiente que suena al mostrar este nodo.")]
    public AudioClip sfx;

    // 🆕 OPCIÓN C: Si está activo, el jugador NO puede avanzar hasta que
    // el SFX termine. Ideal para: timbre del camión, llamada de Micaela,
    // canción de cumpleaños. Dejalo en false para efectos prescindibles.
    [Tooltip("Si está activo, el jugador no puede pasar al siguiente nodo hasta que el SFX termine.")]
    public bool blockAdvanceUntilSfxEnd;

    [Tooltip("Imagen de fondo opcional para este nodo.")]
    public Sprite backgroundImage;

    [Tooltip("Si está activo, el diálogo se pausa aquí hasta que el jugador entregue el pedido.")]
    public bool isWaitingForProductNode;

    [Tooltip("Productos que el jugador debe entregar en ESTA pausa.")]
    public ProductType[] productsToDeliver;
}

[CreateAssetMenu(fileName = "New Sequence", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueNode[] nodes;
}
