using UnityEngine;

public class DunkeosteusBoss : MonoBehaviour
{
    [Header("Referências")]
    public Transform target;
    public Transform spriteTransform;
    public AudioClip DashSound;
    public TransitionAction transitionAction;
    [Header("Movimento")]
    public float swimSpeed = 6f;
    public float rotateSpeed = 200f;

    [Header("Detecção de Obstáculos")]
    public float obstacleCheckDistance = 1.5f;
    public LayerMask obstacleLayer;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 3f;

    public Collider2D Hurtbox;
    public Collider2D Hitbox;

    [Header("Stun Lock")]
    public float StunTimer = 0f;
    public float StunTime = 1.5f;

    [Header("Knockback")]
    public float knockbackTimer = 0f;
    public float knockbackTime = 0.5f;

    [Header("Invulnerabilidade")]
    public float invulnerabilityTime = 0.5f;

    private float invulnerabilityTimer = 0f;
    public bool Active = false;
    public int health = 50;
    float dashTimer;
    float dashCooldownTimer;
    bool isDashing;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector2 currentDirection;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f){
            invulnerabilityTimer -= Time.deltaTime;
            spriteTransform.gameObject.SetActive(
            Mathf.FloorToInt(Time.time * 20) % 2 == 0
        );
        }
        else
        {
            spriteTransform.gameObject.SetActive(true);
        }

        Debug.Log(invulnerabilityTimer);

        if (!Active) return;
        if (target == null) return;
        if (StunTimer > 0) { StunTimer -= Time.deltaTime; return; }

        // --- Dash ---
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f){
                isDashing = false;
            }
            return; // não recalcula direção durante o dash
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
        else
            TryDash();

        // --- Perseguição direta ---
        Vector2 desiredDirection = (target.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, desiredDirection, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            Vector2 left = Quaternion.Euler(0, 0, 45) * desiredDirection;
            Vector2 right = Quaternion.Euler(0, 0, -45) * desiredDirection;

            bool leftBlocked = Physics2D.Raycast(transform.position, left, obstacleCheckDistance, obstacleLayer);
            bool rightBlocked = Physics2D.Raycast(transform.position, right, obstacleCheckDistance, obstacleLayer);

            if (!leftBlocked) desiredDirection = left;
            else if (!rightBlocked) desiredDirection = right;
            else desiredDirection = -desiredDirection;
        }

        currentDirection = Vector2.Lerp(currentDirection, desiredDirection, Time.deltaTime * 5f);
        RotateSprite(currentDirection);
        
    }

    void FixedUpdate()
    {
        if (StunTimer > 0) return;
        if (knockbackTimer > 0f) { knockbackTimer -= Time.fixedDeltaTime; return; }

        if (isDashing)
        {
            rb.velocity = currentDirection * dashSpeed;
            
            return;
        }

        rb.velocity = currentDirection * swimSpeed;
        
    }

    // -----------------------------------------------------------

    void TryDash()
    {
        // Só dasha se o jogador estiver na linha de visão (sem obstáculo direto)
        Vector2 toPlayer = (target.position - transform.position).normalized;
        bool blocked = Physics2D.Raycast(transform.position, toPlayer, obstacleCheckDistance * 2f, obstacleLayer);

        if (!blocked)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            currentDirection = toPlayer; // trava a direção do dash
            AudioController.Instance.PlaySFX(DashSound);
        }
    }

    void RotateSprite(Vector2 direction)
    {
        if (direction == Vector2.zero || isDashing) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.MoveTowardsAngle(
            spriteTransform.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
        spriteTransform.rotation = Quaternion.Euler(0, 0, smoothAngle);

        // Flip Y quando estiver apontando para a esquerda
        Vector3 scale = spriteTransform.localScale;
        scale.y = (direction.x < 0) ? -2.5f : 2.5f;
        spriteTransform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        if (invulnerabilityTimer > 0f)
            return;

        health -= damage;

        invulnerabilityTimer = invulnerabilityTime;

        Debug.Log($"Boss tomou dano! Vida: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Active = false;
        rb.velocity = Vector2.zero;

        // Sua lógica de morte aqui
        transitionAction.Go();
        
        Destroy(gameObject);
    }
}