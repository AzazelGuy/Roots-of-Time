using UnityEngine;

public class EnemySwimAIDevo : MonoBehaviour
{
    #region Variaveis
    [Header("Referências")]
    public Transform target;
    public Transform spriteTransform;
    private Collider2D myCollider;

    [Header("Invulnerabilidade")]
    public float invulnerabilityTime = 0.5f;
    private float invulnerabilityTimer = 0f;

    [Header("Movimento")]
    public float swimSpeed = 5f;
    public float rotateSpeed = 200f;
    public float knockbackTimer = 0;
    public float knockbackTime = 0.5f;

    [Header("Detecção")]
    public float obstacleCheckDistance = 1.5f;
    public LayerMask obstacleLayer;

    [Header("Fuga de Zonas")]
    public LayerMask hidingZoneLayer;
    public float zoneRepulsionDistance = 3f;   // distância que começa a repelir
    public float zoneRepulsionStrength = 2.5f; // força da repulsão

    [Header("Randomização")]

    public SpriteRenderer spriteMask;

    public Sprite[] possibleMasks;

    public Vector2 healthRange = new Vector2(2, 8);
    public Vector2 damageRange = new Vector2(1, 4);
    public Vector2 speedRange = new Vector2(2f, 7f);
    public Vector2 rangeDetectionRange = new Vector2(5f, 15f);

    [HideInInspector] public int Damage;

    [Header("Área de Nado")]
    public Collider2D swimArea;
    public float borderCheckDistance = 2f;
    public float borderRepulsionStrength = 4f;

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
    #endregion
    
    void RandomizeCreature()
{
    // Vida
    Health = Random.Range(
        (int)healthRange.x,
        (int)healthRange.y + 1
    );

    // Dano
    Damage = Random.Range(
        (int)damageRange.x,
        (int)damageRange.y + 1
    );

    // Velocidade
    swimSpeed = Random.Range(
        speedRange.x,
        speedRange.y
    );

    // Alcance
    myRange = Random.Range(
        rangeDetectionRange.x,
        rangeDetectionRange.y
    );

    // Escala
    float scale = Random.Range(0.8f, 1.4f);

    spriteTransform.localScale =
        new Vector3(scale, scale, scale);

    // SpriteMask aleatória
    if (spriteMask != null &&
        possibleMasks != null &&
        possibleMasks.Length > 0)
    {
        spriteMask.sprite =
            possibleMasks[
                Random.Range(0, possibleMasks.Length)
            ];
    }
}
    void Awake()
    {
        RandomizeCreature();

        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponentInChildren<Collider2D>();
        myRange = Random.Range(minRange, maxRange);
        myRange = Mathf.Min(myRange, Vector2.Distance(transform.position, target.position) - 1f);

        if (myRange < maxRange / 2.5f)
            swimSpeed *= 0.3f;
        else
            swimSpeed *= 0.8f;

    }

    void Update()
    {
        if (Health <= 0) { Destroy(gameObject); return; }
        if (StunTimer > 0) { StunTimer -= Time.deltaTime; return; }
        if (invulnerabilityTimer > 0) {
            invulnerabilityTimer -= Time.deltaTime;
            spriteTransform.gameObject.SetActive(
            Mathf.FloorToInt(Time.time * 20) % 2 == 0);
        }
        else
        {
            spriteTransform.gameObject.SetActive(true);
        }
        if (target == null) return;

        float distToPlayer = Vector2.Distance(transform.position, target.position);
        isActive = distToPlayer <= myRange;

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

        desiredDirection =
        (
            desiredDirection +
            GetRepulsionFromZones() +
            GetSwimAreaForce()
        ).normalized;

        currentDirection = Vector2.Lerp(currentDirection, desiredDirection, Time.deltaTime * 5f);
        RotateSprite(currentDirection);
    }

    void FixedUpdate()
    {
        if (!isActive || StunTimer > 0) return;
        if (knockbackTimer > 0f) { knockbackTimer -= Time.fixedDeltaTime; return; }

        Vector2 nextPos =
        rb.position + currentDirection * swimSpeed * Time.fixedDeltaTime;

    if (swimArea != null && !swimArea.OverlapPoint(nextPos))
    {
        currentDirection = -currentDirection;
        rb.velocity = Vector2.zero;
        return;
    }

    rb.velocity = currentDirection * swimSpeed;

        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerAttack")) return;
        if (invulnerabilityTimer > 0f)
            return;

        Health -= 5;

        other.GetComponentInParent<DamageTaker>().invenciTimer = 1f;
        invulnerabilityTimer = invulnerabilityTime;

        if (Health <= 0)
        {
            Destroy(gameObject);
        }
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

    Vector2 GetSwimAreaForce()
{
    if (swimArea == null)
        return Vector2.zero;

    Vector2 correction = Vector2.zero;

    int checks = 8;

    for (int i = 0; i < checks; i++)
    {
        float angle = i * (360f / checks);

        Vector2 dir = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        Vector2 checkPoint =
            (Vector2)transform.position + dir * borderCheckDistance;

        if (!swimArea.OverlapPoint(checkPoint))
        {
            correction += -dir;
        }
    }

    return correction.normalized * borderRepulsionStrength;
}

    void IdleSwim()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            idleTimer = Random.Range(1f, idleChangeDirectionTime);
            idleDirection = Random.insideUnitCircle.normalized;
        }
        Vector2 desired =
        (
            idleDirection +
            GetRepulsionFromZones() +
            GetSwimAreaForce()
        ).normalized;
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