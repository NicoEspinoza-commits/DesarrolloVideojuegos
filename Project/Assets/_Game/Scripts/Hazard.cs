using UnityEngine;

// Peligro (pinchos o el vacío bajo el nivel): si el jugador lo toca, pierde.
public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.LoseGame();
    }
}
