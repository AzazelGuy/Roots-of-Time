using UnityEngine;

public class Gorgonichthysai : CreatureAI
{
    [Header("Gorgonichthys")]
    [SerializeField] private float shearingBiteDamage = 12f;
    [SerializeField] private float cruiseSpeedMultiplier = 0.65f;
    [SerializeField] private float attackSpeedMultiplier = 1.3f;
    [SerializeField] private float cameraShakeMagnitude = 0.7f;

    private readonly string[] sharkTags = { "Shark", "Cladoselache", "Stethacanthus" };
    private readonly string[] generalTags = { "Prey", "SmallPrey" };

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "Shark", "Cladoselache", "Stethacanthus", "Prey", "SmallPrey", "PlayerMain" };
        predatorTags = new string[0];
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        WanderMovement();
        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnHunger()
    {
        // Alvos preferenciais (Fidelidade científica)
        Transform prey = FindNearest(sharkTags, EffectiveDetectionRange);

        if (prey == null)
            prey = FindNearest(generalTags, EffectiveDetectionRange);


        if (prey == null && playerTransform != null && Vector2.Distance(rb.position, playerTransform.position) <= EffectiveDetectionRange)
        {
            prey = playerTransform;
        }

        if (prey != null) { target = prey; ChangeState(CreatureState.Hunt); return; }
        WanderMovement();
    }

    protected override void OnHunt()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        { ChangeState(CreatureState.Wander); return; }

        float dist = Vector2.Distance(rb.position, target.position);

        // Sistema de Carga Bifásica
        float currentTargetSpeed = dist > data.biteRange * 2.5f
            ? data.maxSpeedWater * cruiseSpeedMultiplier  // Espreita silenciosa
            : data.maxSpeedWater * attackSpeedMultiplier; // Disparo explosivo

        MoveToward(target.position, currentTargetSpeed);

        if (dist <= data.biteRange && attackCooldownTimer <= 0f)
            ShearAttack(target);
    }

    protected override void OnThreatDetected(Transform threat) { }
    protected override void OnFlee() => ChangeState(CreatureState.Wander);

    private void ShearAttack(Transform victim)
    {
        if (victim.TryGetComponent<CreatureHealth>(out var h))
            h.TakeDamage((int)shearingBiteDamage);
        if (victim.CompareTag("PlayerMain") && Devoniancamera.Instance != null)
            Devoniancamera.Instance.Shake(cameraShakeMagnitude, 0.4f);

        attackCooldownTimer = data.attackCooldown;
        anim?.SetTrigger(AnimAttack);
    }
}