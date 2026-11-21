using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el puntaje y el final de la partida (ganar o perder).
public class GameManager : MonoBehaviour
{
    // Acceso fácil desde otros scripts: GameManager.Instance.AddScore(10)
    public static GameManager Instance { get; private set; }

    // "static" = se conservan al cambiar de escena, para mostrarlos en GameOver
    public static int FinalScore;
    public static bool PlayerWon;

    [SerializeField] private Text scoreText;
    [SerializeField] private string gameOverScene = "GameOver";

    private int score;
    private bool gameEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void WinGame()  { EndGame(true); }
    public void LoseGame() { EndGame(false); }

    private void EndGame(bool won)
    {
        if (gameEnded) return; // evita terminar dos veces
        gameEnded = true;

        PlayerWon = won;
        FinalScore = score;
        SceneManager.LoadScene(gameOverScene);
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Puntos: " + score;
    }
}
