using UnityEngine;

public class Pterygotusai : CreatureAI
{
    [Header("Pterygotus")]
    [SerializeField, Range(0f, 1f)] private float fleeHealthThreshold = 0.35f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxHeightAboveGround = 2.0f;
    [SerializeField] private float clawDamage = 6f;

    private bool isFleeing;
    private int maxHealth;

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "Prey", "SmallPrey" };
        predatorTags = new[] { "LargePredator" };
        maxHealth    = data != null ? data.maxHealth : 10;
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        StayNearGround();
        WanderMovement();
        ScanForThreats();

        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnHunger()
    {
        StayNearGround();
        ScanForThreats();

        if (ShouldFlee()) { ChangeState(CreatureState.Flee, 6f); return; }

        Transform prey = FindNearest(preyTags, EffectiveDetectionRange);
        if (prey != null) { target = prey; ChangeState(CreatureState.Hunt); return; }

        // Ataque oportunista de curto alcance ao jogador
        if (playerTransform != null && attackCooldownTimer <= 0f)
        {
            if (Vector2.Distance(rb.position, playerTransform.position) <= data.biteRange)
                ClawAttack(playerTransform);
        }

        WanderMovement();
    }

    protected override void OnHunt()
    {
        StayNearGround();
        if (ShouldFlee()) { ChangeState(CreatureState.Flee, 6f); return; }

        if (target == null || !target.gameObject.activeInHierarchy)
        { ChangeState(CreatureState.Wander); return; }

        MoveToward(target.position, data.maxSpeedWater);
        if (Vector2.Distance(rb.position, target.position) <= data.biteRange && attackCooldownTimer <= 0f)
            ClawAttack(target);
    }

    protected override void OnFlee()
    {
        Transform fleeFrom = target ?? playerTransform;

        if (fleeFrom != null)
        {
            // Recuo t�tico defensivo: afasta fisicamente, mas a rota��o org�nica base herda a velocidade do recuo
            MoveAway(fleeFrom.position, data.maxSpeedWater * 1.5f);
        }

        float dist = fleeFrom != null ? Vector2.Distance(rb.position, fleeFrom.position) : float.MaxValue;

        if (dist > data.detectionRange * 2.2f || stateTimer <= 0f)
        { isFleeing = false; target = null; ChangeState(CreatureState.Wander); }
    }

    protected override void OnThreatDetected(Transform threat)
    { target = threat; ChangeState(CreatureState.Flee, 6f); }

    public override void OnDamageTaken(int amount)
    {
        if (ShouldFlee() && !isFleeing)
        { isFleeing = true; target = playerTransform; ChangeState(CreatureState.Flee, 8f); }
    }

    private void ClawAttack(Transform victim)
    {
        if (victim.TryGetComponent<CreatureHealth>(out var h))
            h.TakeDamage((int)clawDamage);
        if (victim.CompareTag("PlayerMain") && Devoniancamera.Instance != null)
            Devoniancamera.Instance.Shake(clawDamage / 14f, 0.25f);

        attackCooldownTimer = data.attackCooldown;
        anim?.SetTrigger(AnimAttack);
    }

    private bool ShouldFlee() => (float)health.CurrentHealth / maxHealth <= fleeHealthThreshold;

    private void StayNearGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 6f, groundLayer);
        if (!hit) return;
        float targetY = hit.point.y + maxHeightAboveGround;
        if (rb.position.y > targetY + 0.1f)
        {
            Vector2 v = rb.velocity;
            v.y = Mathf.Lerp(v.y, -2f, Time.deltaTime * 3f);
            rb.velocity = v;
        }
    }
}