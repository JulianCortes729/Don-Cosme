using UnityEngine;
using UnityEngine.SceneManagement; // Requerido para cambiar de escenas

/// <summary>
/// Gestiona la lógica de la escena del Menú Principal.
/// Controla las transiciones entre paneles y la carga del nivel principal.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("Nombre exacto de la escena del juego principal a cargar.")]
    [SerializeField] private string gameSceneName = "GameLevel";

    [Header("Paneles de UI")]
    [Tooltip("El panel que contiene los botones de Jugar, Opciones y Salir.")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("El panel que contiene la configuración (volumen, gráficos, etc.).")]
    [SerializeField] private GameObject optionsPanel;

    private void Start()
    {
        // 🛡️ Inicialización segura: Nos aseguramos de empezar en el panel correcto
        ShowMainPanel();

        // 📌 IMPORTANTE: Nos aseguramos de que el cursor esté libre y visible
        // por si venimos de la escena del juego donde lo habíamos bloqueado.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ── MÉTODOS PÚBLICOS (Para enlazar en el OnClick de los botones) ───────

    public void PlayGame()
    {
        Debug.Log("[MainMenu] Iniciando partida...");

        // 🟡 PERF: Usamos LoadScene (sincrónico) porque nuestra escena es liviana. 
        // Si en el futuro el juego pesa 2GB, cambiaremos esto a LoadSceneAsync 
        // para poder poner una barra de carga.
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Saliendo del Kioskito...");

        // Directivas de preprocesador: Ejecuta distinto código si estás en Unity o en el juego final
#if UNITY_EDITOR
        // Detiene el modo Play dentro del editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Cierra la aplicación (.exe o .apk)
            Application.Quit();
#endif
    }

    // ── INTERNOS ────────────────────────────────────────────────────────────

    private void ShowMainPanel()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }
}
