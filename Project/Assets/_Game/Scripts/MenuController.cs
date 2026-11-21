using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Botones de los menús (MainMenu y GameOver).
public class MenuController : MonoBehaviour
{
    [SerializeField] private Text resultText;     // solo se usa en GameOver (puede quedar vacío)
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickDelay = 0.2f; // espera para que se escuche el clic

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (resultText != null)
        {
            string title = GameManager.PlayerWon ? "¡Ganaste!" : "Game Over";
            resultText.text = title + "\nPuntos: " + GameManager.FinalScore;
        }
    }

    // Lo llaman los botones, con el nombre de la escena a cargar
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAfterClick(sceneName));
    }

    public void QuitGame()
    {
        audioSource.PlayOneShot(clickSound);
        Application.Quit();
    }

    private IEnumerator LoadAfterClick(string sceneName)
    {
        audioSource.PlayOneShot(clickSound);
        yield return new WaitForSeconds(clickDelay);
        SceneManager.LoadScene(sceneName);
    }
}
