using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que permite que un objeto sea recogible por el jugador.
/// Implementa <see cref="IInteractable"/> para integrarse con el sistema de interacción
/// y utiliza un <see cref="Rigidbody"/> para desplazar el objeto hacia el punto de agarre.
/// </summary>
/// <summary>
/// Componente que permite que un objeto sea recogible por el jugador.
/// Implementa <see cref="IInteractable"/> para integrarse con el sistema de interacción
/// y <see cref="IDroppable"/> para exponer un contrato de soltado.
/// </summary>
/// <remarks>
/// - Cuando se llama a <see cref="Interact(Transform)"/>, este componente desactiva
///   la gravedad y hace que el rigidbody siga suavemente el <paramref name="grabPoint"/>.
/// - Cuando se llama a <see cref="Drop"/>, reactiva la gravedad y deja de seguir el punto.
/// - <see cref="followSpeed"/> controla la rapidez con la que el objeto se mueve hacia
///   el punto de agarre. Se recomienda ajustar este valor desde el Inspector.
/// </remarks>
public class ObjectGrabbable : MonoBehaviour, IInteractable, IDroppable
{
    /// <summary>
    /// Rigidbody del objeto. Se usa para controlar la posición y la gravedad.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Punto objetivo al que el objeto debe seguir cuando es agarrado. Si es null,
    /// el objeto no está siendo sostenido.
    /// </summary>
    private Transform currentGrabPoint; //Aquí guardaremos el punto al que debemos seguir

    /// <summary>
    /// Velocidad de seguimiento hacia el punto de agarre. Controla la rapidez con
    /// la que el objeto se mueve para alcanzar la posición objetivo.
    /// </summary>
    [SerializeField]
    [Tooltip("Velocidad a la que el objeto se alinea con el punto de agarre (mayor = más rápido).")]
    private float followSpeed = 10f;



    /// <summary>
    /// Se ejecuta al inicializar el componente. Obtiene referencias necesarias
    /// (por ejemplo, el <see cref="Rigidbody"/>) para evitar llamadas repetidas.
    /// </summary>
    private void Awake()
    {
        // Obtenemos el Rigidbody al arrancar
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Método llamado por el sistema de interacción cuando el jugador agarra el objeto.
    /// <para>
    /// Se espera que se le pase el <see cref="Transform"/> del punto de agarre del jugador
    /// (por ejemplo, `PlayerInteractor.GrabPoint`). El componente desactiva la gravedad
    /// y comienza a seguir el punto objetivo.
    /// </para>
    /// </summary>
    /// <param name="grabPoint">GameObject que representa la posición objetivo a seguir.</param>
    public void Interact(GameObject grabPoint)
    {
        if (grabPoint == null) return;

        // Guardamos el punto de agarre y reiniciamos velocidad para evitar saltos bruscos.
        currentGrabPoint = grabPoint.transform; // Guardamos el punto de agarre del jugador
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    public void Drop()
    {
        // Dejamos de seguir el punto y restauramos la física normal.
        currentGrabPoint = null;
        rb.useGravity = true;
    }


    /// <summary>
    /// Actualización de física que mueve el objeto hacia el punto de agarre cuando
    /// este está definido. Se recomienda usar FixedUpdate para operaciones que
    /// modifican Rigidbody/velocidad.
    /// </summary>
    private void FixedUpdate()
    {
        if (currentGrabPoint != null)
        {
            // Calcula la velocidad necesaria para desplazarse hacia el grab point.
            Vector3 directionToGrabPoint = (currentGrabPoint.position - transform.position) * followSpeed;
            rb.velocity = directionToGrabPoint;
        }
    }
}
