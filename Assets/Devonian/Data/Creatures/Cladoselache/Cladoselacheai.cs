using UnityEngine;

public class Cladoselacheai : CreatureAI
{
    [Header("Cladoselache")]
    [SerializeField, Range(0f, 1f)] private float fleeHealthThreshold = 0.4f;
    [SerializeField] private float huntSpeedMultiplier = 1.6f;
    [SerializeField] private float escapeSpeedMultiplier = 2f;
    [SerializeField] private float largePredatorSenseRange = 12f;

    private int maxHealth;
    private bool wasAlerted;
    private float orbitAngle;

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "SmallPrey", "Prey" };
        predatorTags = new[] { "LargePredator" };
        maxHealth    = data != null ? data.maxHealth : 8;
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        WanderMovement();
        ScanForLargePredators();

        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnHunger()
    {
        ScanForLargePredators();
        Transform prey = FindNearest(preyTags, EffectiveDetectionRange);

        if (prey != null)
        {
            target = prey;
            orbitAngle = Random.Range(0f, 360f);
            ChangeState(CreatureState.Hunt);
            return;
        }
        WanderMovement();
    }

    protected override void OnHunt()
    {
        ScanForLargePredators();
        if (ShouldFlee()) { ChangeState(CreatureState.Flee, 7f); return; }
        if (target == null || !target.gameObject.activeInHierarchy)
        { ChangeState(CreatureState.Wander); return; }

        float dist = Vector2.Distance(rb.position, target.position);

        // Fase de Cerco: Se ainda estiver meio longe, circula a presa antes do ataque final
        if (dist > data.biteRange * 2.5f)
        {
            orbitAngle += Time.deltaTime * 45f; // velocidade de �rbita
            Vector2 offset = new Vector2(Mathf.Cos(orbitAngle * Mathf.Deg2Rad), Mathf.Sin(orbitAngle * Mathf.Deg2Rad)) * (data.biteRange * 2f);
            MoveToward((Vector2)target.position + offset, data.maxSpeedWater);
        }
        else
        {
            // Carga de ataque direto
            MoveToward(target.position, data.maxSpeedWater * huntSpeedMultiplier);
            if (dist <= data.biteRange && attackCooldownTimer <= 0f)
                TryAttack(target);
        }
    }

    protected override void OnFlee()
    {
        Transform fleeFrom = target ?? playerTransform;
        if (fleeFrom != null)
            MoveAway(fleeFrom.position, data.maxSpeedWater * escapeSpeedMultiplier);

        float dist = fleeFrom != null ? Vector2.Distance(rb.position, fleeFrom.position) : float.MaxValue;

        if (dist > largePredatorSenseRange * 1.5f || stateTimer <= 0f)
        { wasAlerted = false; target = null; ChangeState(CreatureState.Wander); }
    }

    protected override void OnThreatDetected(Transform threat)
    { wasAlerted = true; target = threat; ChangeState(CreatureState.Flee, 8f); }

    public override void OnDamageTaken(int amount)
    {
        if (ShouldFlee()) { target = playerTransform; ChangeState(CreatureState.Flee, 7f); }
    }

    private void ScanForLargePredators()
    {
        if (wasAlerted) return;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(rb.position, largePredatorSenseRange);
        foreach (var col in nearby)
            if (col.CompareTag("LargePredator"))
            { OnThreatDetected(col.transform); return; }
    }

    private bool ShouldFlee() => (float)health.CurrentHealth / maxHealth <= fleeHealthThreshold;
}