using UnityEngine;
using UnityEngine.InputSystem;

// Mueve al jugador con el teclado y lo hace saltar usando la física de Unity.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallGravityMultiplier = 1.8f; // al caer, la gravedad aumenta (salto menos "flotante")
    [SerializeField] private float bounceForce = 10f;            // rebote al pisar a un enemigo

    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck;              // punto bajo los pies
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;              // qué capas cuentan como suelo

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float baseGravity;
    private float moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        baseGravity = rb.gravityScale;
    }

    // Update: se ejecuta cada frame. Aquí leemos el teclado.
    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        moveInput = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += 1f;

        if (kb.spaceKey.wasPressedThisFrame && IsGrounded())
            Jump();

        // Voltear el sprite según la dirección
        if (moveInput != 0f) spriteRenderer.flipX = moveInput < 0f;
    }

    // FixedUpdate: se ejecuta a ritmo fijo. Aquí aplicamos la física.
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Gravedad ajustada: más fuerte cuando el jugador va cayendo
        rb.gravityScale = rb.linearVelocity.y < 0f ? baseGravity * fallGravityMultiplier : baseGravity;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        audioSource.PlayOneShot(jumpSound);
    }

    // Lo llama el enemigo cuando el jugador lo pisa
    public void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }
}
