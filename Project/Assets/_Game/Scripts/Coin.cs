using UnityEngine;

// Moneda: al tocarla suma puntos, suena y desaparece.
public class Coin : MonoBehaviour
{
    [SerializeField] private int points = 10;
    [SerializeField] private AudioClip pickupSound;

    // Se llama cuando algo entra en el collider marcado como "Is Trigger"
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.AddScore(points);
        AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
        Destroy(gameObject);
    }
}
