using UnityEngine;

public class EnemySwimAI : MonoBehaviour
{
    [Header("Referências")]
    public Transform target;
    public Transform spriteTransform;
    private Collider2D myCollider;

    [Header("Movimento")]
    public float swimSpeed = 5f;
    public float rotateSpeed = 200f;
    public float knockbackTimer = 0;
    public float knockbackTime = 0.5f;

    [Header("Detecção")]
    public float obstacleCheckDistance = 1.5f;
    public LayerMask obstacleLayer;

    // ✅ NOVO: camada das zonas de esconderijo
    [Header("Fuga de Zonas")]
    public LayerMask hidingZoneLayer;
    public float zoneRepulsionDistance = 3f;   // distância que começa a repelir
    public float zoneRepulsionStrength = 2.5f; // força da repulsão

    [Header("Range Aleatório")]
    public float minRange = 5f;
    public float maxRange = 15f;

    [Header("Stun Lock")]
    public float StunTimer = 0f;
    public float StunTime = 1.5f;

    [Header("Idle Swim")]
    public float idleChangeDirectionTime = 2f;
    private float idleTimer;
    private Vector2 idleDirection;

    [Header("Fuga")]
    public int fleeHealthThreshold = 2;
    public int Health = 4;

    float myRange;

    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector2 currentDirection;
    [HideInInspector] PlayerMovement playerScript;
    bool isActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponentInChildren<Collider2D>();
        myRange = Random.Range(minRange, maxRange);
        myRange = Mathf.Min(myRange, Vector2.Distance(transform.position, target.position) - 1f);

        if (myRange < maxRange / 2.5f)
            swimSpeed *= 0.3f;
        else
            swimSpeed *= 0.8f;

        if (target != null)
            playerScript = target.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Health <= 0) { Destroy(gameObject); return; }
        if (StunTimer > 0) { StunTimer -= Time.deltaTime; return; }
        if (target == null) return;

        float distToPlayer = Vector2.Distance(transform.position, target.position);
        isActive = distToPlayer <= myRange && !playerScript.isHidden;

        if (!isActive)
        {
            IdleSwim();
            return;
        }

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

        // ✅ NOVO: soma repulsão das zonas antes de aplicar a direção
        desiredDirection = (desiredDirection + GetRepulsionFromZones()).normalized;

        currentDirection = Vector2.Lerp(currentDirection, desiredDirection, Time.deltaTime * 5f);
        RotateSprite(currentDirection);
    }

    void FixedUpdate()
    {
        if (!isActive || StunTimer > 0) return;
        if (knockbackTimer > 0f) { knockbackTimer -= Time.fixedDeltaTime; return; }

        rb.velocity = currentDirection * swimSpeed;

        
    }
    Vector2 GetRepulsionFromZones()
    {
        Vector2 repulsion = Vector2.zero;
        int rayCount = 8; // raycasts em círculo

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount);
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, dir, zoneRepulsionDistance, hidingZoneLayer);

            if (hit.collider != null)
            {
                // força inversamente proporcional à distância
                float strength = 1f - (hit.distance / zoneRepulsionDistance);
                repulsion += -dir * strength;
            }
        }

        return repulsion * zoneRepulsionStrength;
    }

    void IdleSwim()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            idleTimer = Random.Range(1f, idleChangeDirectionTime);
            idleDirection = Random.insideUnitCircle.normalized;
        }

        // ✅ repulsão também no idle
        Vector2 desired = (idleDirection + GetRepulsionFromZones()).normalized;
        currentDirection = Vector2.Lerp(currentDirection, desired, Time.deltaTime * 2f);
        RotateSprite(currentDirection);
        rb.velocity = currentDirection * (swimSpeed * 0.5f);
    }

    void RotateSprite(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.MoveTowardsAngle(
            spriteTransform.eulerAngles.z, angle, rotateSpeed * Time.deltaTime);
        spriteTransform.rotation = Quaternion.Euler(0, 0, smoothAngle);

        // Flip Y quando estiver apontando para a esquerda
        Vector3 scale = spriteTransform.localScale;
        scale.y = (direction.x < 0) ? -1f : 1f;
        spriteTransform.localScale = scale;
    }
}