using UnityEngine;

// Bandera de meta: si el jugador la toca, gana.
public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.WinGame();
    }
}
