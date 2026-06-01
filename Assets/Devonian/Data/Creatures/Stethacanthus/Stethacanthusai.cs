using UnityEngine;

/// <summary>
/// Stethacanthus � tubar�o primitivo (~60-90cm).
/// Rota��o: herdada do CreatureAI (flipX fix).
/// Comportamento: patrulha em meia-�gua, foge do player,
///                contra-ataca UMA vez se encurralado.
/// </summary>
public class Stethacanthusai : CreatureAI
{
    [Header("Stethacanthus � Patrulha")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaypointRadius = 0.4f;

    [Header("Encurralamento")]
    [SerializeField] private float corneredDistance = 1.2f;
    [SerializeField] private float corneredBiteDamage = 3f;

    private int patrolIndex;
    private bool hasCounterAttacked;

    protected override void Awake()
    {
        base.Awake();
        // Ca�a presas menores, foge do player
        preyTags     = new[] { "SmallPrey" };
        predatorTags = new[] { "PlayerMain", "Predator", "LargePredator" };
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        Patrol();
        ScanForThreats();
        hasCounterAttacked = false;

        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnFlee()
    {
        if (target == null) { ChangeState(CreatureState.Wander); return; }

        float dist = Vector2.Distance(rb.position, target.position);

        // Encurralado: contra-ataque �nico de desespero antes de fugir
        if (dist < corneredDistance && !hasCounterAttacked && attackCooldownTimer <= 0f)
        {
            CounterAttack();
            return;
        }

        MoveAway(target.position, data.maxSpeedWater * 1.4f);

        if (dist > data.detectionRange * 1.8f || stateTimer <= 0f)
        { target = null; ChangeState(CreatureState.Wander); }
    }

    protected override void OnThreatDetected(Transform threat)
    { target = threat; hasCounterAttacked = false; ChangeState(CreatureState.Flee, 7f); }

    public override void OnDamageTaken(int amount)
    {
        if (playerTransform != null)
        { target = playerTransform; ChangeState(CreatureState.Flee, 7f); }
    }

    private void CounterAttack()
    {
        if (target == null) return;
        if (target.TryGetComponent<CreatureHealth>(out var h))
            h.TakeDamage((int)corneredBiteDamage);
        if (target.CompareTag("PlayerMain") && Devoniancamera.Instance != null)
            Devoniancamera.Instance.Shake(0.3f, 0.2f);
        hasCounterAttacked  = true;
        attackCooldownTimer = data.attackCooldown;
        anim?.SetTrigger(AnimAttack);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        { WanderMovement(); return; }

        Transform wp = patrolPoints[patrolIndex];
        if (wp == null) { patrolIndex = 0; return; }

        MoveToward(wp.position, data.maxSpeedWater * 0.6f);

        if (Vector2.Distance(rb.position, wp.position) < patrolWaypointRadius)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }
}