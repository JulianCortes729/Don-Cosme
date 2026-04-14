using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que permite que un objeto sea recogible por el jugador.
/// Implementa <see cref="IInteractable"/> para integrarse con el sistema de interacción
/// y utiliza un <see cref="Rigidbody"/> para desplazar el objeto hacia el punto de agarre.
/// </summary>
public class ObjectGrabbable : MonoBehaviour, IInteractable
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
    /// Guarda el <paramref name="grabPoint"/> y desactiva la gravedad para que el
    /// objeto pueda ser controlado por la lógica de seguimiento.
    /// </summary>
    /// <param name="grabPoint">Transform que representa la posición objetivo a seguir.</param>
    public void Interact(Transform grabPoint)
    {
        // 1. Guardar el grabPoint que nos pasó el jugador en 'currentGrabPoint'.
        currentGrabPoint = grabPoint;


        // 2. Apagar la gravedad del Rigidbody (rb.useGravity = false) para evitar
        // que la física normal haga que el objeto caiga mientras está agarrado.
        rb.useGravity = false;
    }

    /// <summary>
    /// Suelta el objeto: borra el punto de agarre y vuelve a activar la gravedad.
    /// </summary>
    public void Drop()
    {
        // 4. Para soltar el objeto, simplemente ponemos 'currentGrabPoint' a null y volvemos a activar la gravedad.
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
