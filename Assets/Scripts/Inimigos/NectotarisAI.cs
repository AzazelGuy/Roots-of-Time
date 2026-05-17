using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NectotarisAI : MonoBehaviour
{
    [Header("Configurações de Ritmo")]
    public float pulseFrequency = 1.2f;
    public float burstForce = 7f;
    public float fleeForceMultiplier = 2f;

    [Header("Sensores e Território")]
    public Transform player;
    public float detectionRadius = 6f;
    public float roamRadius = 10f;
    public bool spriteFacesRight = true;

    [Header("Desvio de Paredes")]
    public LayerMask wallLayer;
    public float wallDetectionRange = 1.5f;  // Distância do raycast
    public float wallAvoidanceWeight = 2.5f; // Peso da força de desvio

    private Rigidbody2D rb;
    private Vector2 homePosition;
    private Vector2 currentMoveDir;
    private float pulseTimer;
    private bool isFleeing = false;

    private readonly Vector2[] directions8 = {
        new Vector2(1,0), new Vector2(1,1), new Vector2(0,1), new Vector2(-1,1),
        new Vector2(-1,0), new Vector2(-1,-1), new Vector2(0,-1), new Vector2(1,-1)
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.drag = 3.5f;
        rb.freezeRotation = true;

        homePosition = transform.position;
        PickNewDirection();
    }

    void Update()
    {
        if (player == null) { FindPlayer(); return; }

        CheckPlayerPresence();
        HandleVisuals();

        pulseTimer -= Time.deltaTime;
        if (pulseTimer <= 0)
        {
            ExecutePulse();
            pulseTimer = isFleeing ? pulseFrequency * 0.5f : pulseFrequency;
        }
    }

    private void CheckPlayerPresence()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= detectionRadius)
        {
            isFleeing = true;
            Vector2 escape = ((Vector2)transform.position - (Vector2)player.position).normalized;
            currentMoveDir = GetBestDirection(escape);
        }
        else
        {
            if (isFleeing) { isFleeing = false; PickNewDirection(); }

            if (Vector2.Distance(transform.position, homePosition) > roamRadius)
            {
                Vector2 home = (homePosition - (Vector2)transform.position).normalized;
                currentMoveDir = GetBestDirection(home);
            }
        }
    }

    private void ExecutePulse()
    {
        if (!isFleeing && Random.value < 0.3f) PickNewDirection();

        // Recalcula a direção considerando paredes antes de cada pulso
        currentMoveDir = GetBestDirection(currentMoveDir);

        float force = isFleeing ? burstForce * fleeForceMultiplier : burstForce;

        rb.velocity = Vector2.zero;
        rb.AddForce(currentMoveDir * force, ForceMode2D.Impulse);
    }

    private void PickNewDirection()
    {
        // Tenta escolher uma direção aleatória que não esteja bloqueada
        Vector2[] shuffled = ShuffleDirections();
        foreach (Vector2 dir in shuffled)
        {
            if (!IsWallInDirection(dir))
            {
                currentMoveDir = dir;
                return;
            }
        }
        // Se todas estiverem bloqueadas, usa a menos obstruída
        currentMoveDir = GetBestDirection(directions8[Random.Range(0, directions8.Length)]);
    }

    /// <summary>
    /// Escolhe a melhor direção das 8 disponíveis,
    /// penalizando direções com paredes próximas.
    /// </summary>
    private Vector2 GetBestDirection(Vector2 desiredDir)
    {
        float bestScore = float.NegativeInfinity;
        Vector2 bestDir = desiredDir.normalized;

        foreach (Vector2 dir in directions8)
        {
            Vector2 normalized = dir.normalized;

            // Alinhamento com a direção desejada
            float alignment = Vector2.Dot(desiredDir.normalized, normalized);

            // Penalidade de parede: quanto mais próxima, menor o score
            float wallPenalty = GetWallPenalty(normalized);

            float score = alignment - (wallPenalty * wallAvoidanceWeight);

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = normalized;
            }
        }

        return bestDir;
    }

    /// <summary>
    /// Retorna um valor entre 0 e 1 indicando o quão bloqueada está a direção.
    /// 0 = livre, 1 = parede imediata.
    /// </summary>
    private float GetWallPenalty(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            wallDetectionRange,
            wallLayer
        );

        if (hit.collider != null)
        {
            // Penalidade inversamente proporcional à distância
            return 1f - (hit.distance / wallDetectionRange);
        }

        return 0f;
    }

    private bool IsWallInDirection(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            wallDetectionRange * 0.5f, // Margem mais conservadora para PickNewDirection
            wallLayer
        );
        return hit.collider != null;
    }

    private Vector2[] ShuffleDirections()
    {
        Vector2[] copy = (Vector2[])directions8.Clone();
        for (int i = copy.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    private void HandleVisuals()
    {
        Vector2 lookDir = (rb.velocity.sqrMagnitude > 0.5f) ? rb.velocity.normalized : currentMoveDir;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            bool shouldFlip = spriteFacesRight ? (lookDir.x < 0) : (lookDir.x > 0);
            float finalAngle = shouldFlip ? angle + 180 : angle;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, finalAngle), Time.deltaTime * 10f);
            transform.localScale = new Vector3(1, shouldFlip ? -1 : 1, 1);
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("PlayerMain");
        if (p != null) player = p.transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(homePosition, roamRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualiza os raycasts de detecção de parede
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2 dir in directions8)
            {
                Gizmos.DrawRay(transform.position, dir.normalized * wallDetectionRange);
            }
        }
    }
}