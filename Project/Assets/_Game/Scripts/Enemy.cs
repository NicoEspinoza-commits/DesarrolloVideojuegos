using UnityEngine;

// Enemigo que camina de lado a lado.
// Si el jugador le cae encima, el enemigo muere; si lo toca de lado, el jugador pierde.
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float patrolDistance = 2f; // cuánto se aleja de su punto inicial
    [SerializeField] private float stompHeight = 0.4f;  // qué tan arriba debe estar el jugador para "pisarlo"
    [SerializeField] private AudioClip stompSound;

    private Rigidbody2D rb;
    private float startX;
    private int direction = 1; // 1 = derecha, -1 = izquierda

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;
    }

    private void FixedUpdate()
    {
        // Dar la vuelta al llegar al límite de patrulla
        if (transform.position.x > startX + patrolDistance) direction = -1;
        if (transform.position.x < startX - patrolDistance) direction = 1;

        rb.MovePosition(rb.position + Vector2.right * direction * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        bool playerIsAbove = collision.transform.position.y > transform.position.y + stompHeight;
        if (playerIsAbove)
        {
            collision.gameObject.GetComponent<PlayerController>().Bounce();
            AudioSource.PlayClipAtPoint(stompSound, Camera.main.transform.position);
            Destroy(gameObject);
        }
        else
        {
            GameManager.Instance.LoseGame();
        }
    }
}
