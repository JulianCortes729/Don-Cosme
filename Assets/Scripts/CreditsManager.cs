using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [Header("Pantallas de Final")]
    [SerializeField] private GameObject finScreen;
    [SerializeField] private GameObject graciasScreen;
    [SerializeField] private GameObject creditosScreen;

    [Header("Tiempos")]
    [SerializeField] private float timePerScreen = 4f; // Segundos que dura cada imagen

    private void Start()
    {
        // Aseguramos que empezamos con la pantalla limpia y el cursor libre
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        finScreen.SetActive(false);
        graciasScreen.SetActive(false);
        creditosScreen.SetActive(false);

        // Disparamos la secuencia
        StartCoroutine(ShowCreditsSequence());
    }

    private IEnumerator ShowCreditsSequence()
    {
        // 1. Mostramos "Fin"
        finScreen.SetActive(true);
        yield return new WaitForSeconds(timePerScreen);
        finScreen.SetActive(false);

        // 2. Mostramos "Gracias por jugar"
        graciasScreen.SetActive(true);
        yield return new WaitForSeconds(timePerScreen);
        graciasScreen.SetActive(false);

        // 3. Mostramos "Créditos"
        creditosScreen.SetActive(true);
        yield return new WaitForSeconds(timePerScreen);

        // OPCIONAL: Devolvemos al jugador al Menú Principal al terminar
        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }
}
