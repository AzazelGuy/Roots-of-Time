using UnityEngine;

public class AnomalocarisChase : MonoBehaviour
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

    [Header("Stun Lock")]
    public float StunTimer = 0f;
    public float StunTime = 1.5f;

    [Header("Knockback")]
    public float knockbackTimer = 0f;
    public float knockbackTime = 0.5f;

    [Header("Timer de Vida")]
    public float lifeDuration = 30f;   // tempo total antes de despawnar
    public float exitSpeed = 8f;        // velocidade ao sair do cenário
    public Transform exitPoint;         // ponto fora do cenário para onde vai ao morrer

    // --- Privados ---
    float lifeTimer;
    float dashTimer;
    float dashCooldownTimer;
    bool isDashing;
    bool isExiting;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector2 currentDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lifeTimer = lifeDuration;
    }

    void Update()
    {
        if (target == null) return;
        if (StunTimer > 0) { StunTimer -= Time.deltaTime; return; }

        // --- Countdown de vida ---
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f && !isExiting)
            StartExit();

        if (isExiting)
        {
            MoveToExit();
            return;
        }

        // --- Dash ---
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
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

        if (isExiting)
        {
            rb.velocity = currentDirection * exitSpeed;
            return;
        }

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

    void StartExit()
    {
        isExiting = true;
        transitionAction.Go();
        if (exitPoint != null)
            currentDirection = (exitPoint.position - transform.position).normalized;
        else
            currentDirection = -currentDirection; // vai na direção oposta se não tiver ponto
    }

    void MoveToExit()
    {
        if (exitPoint == null) return;

        float dist = Vector2.Distance(transform.position, exitPoint.position);
        if (dist < 0.5f)
            transitionAction.Go();
            Destroy(gameObject);

        RotateSprite(currentDirection);
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
        scale.y = (direction.x < 0) ? -4.429254f : 4.429254f;
        spriteTransform.localScale = scale;
    }
}