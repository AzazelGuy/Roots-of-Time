using UnityEngine;

public class DunkleosteusAI : CreatureAI
{
    [Header("Dunkleosteus")]
    [SerializeField] private bool isInDeepWater = true;
    [SerializeField] private float shallowSpeedPenalty = 0.5f;
    [SerializeField] private float oxygenDebuffDelay = 2f;

    private float shallowTimer;
    private bool suffocating;
    private Vector2 retreatPoint;
    private bool isIntimidating;

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "Shark", "Cladoselache", "Stethacanthus", "Prey", "SmallPrey", "PlayerMain" };
        predatorTags = new string[0];
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        if (!isInDeepWater) HandleShallowWater();

        // Intimidação passiva se o jogador chegar perto e o Boss estiver de barriga cheia
        if (playerTransform != null && !isIntimidating && Vector2.Distance(rb.position, playerTransform.position) < EffectiveDetectionRange * 0.7f)
        {
            isIntimidating = true;
            anim?.SetTrigger(AnimAttack); // animação de rosnado/mandíbula aberta
        }

        if (isIntimidating)
        {
            MoveToward(playerTransform.position, data.maxSpeedWater * 0.4f); // aproximação lenta e intimidadora
            if (Vector2.Distance(rb.position, playerTransform.position) > EffectiveDetectionRange)
                isIntimidating = false;
        }
        else
        {
            WanderMovement();
        }

        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnHunger()
    {
        if (!isInDeepWater) { HandleShallowWater(); return; }
        isIntimidating = false;

        // Hierarquia rígida de predação (Tubarões > Presas > Player por último)
        Transform prey = FindNearest(new[] { "Shark", "Cladoselache", "Stethacanthus" }, EffectiveDetectionRange);
        if (prey == null) prey = FindNearest(new[] { "Prey", "SmallPrey" }, EffectiveDetectionRange);
        if (prey == null && playerTransform != null && Vector2.Distance(rb.position, playerTransform.position) <= EffectiveDetectionRange)
            prey = playerTransform;

        if (prey != null) { target = prey; ChangeState(CreatureState.Hunt); return; }

        WanderMovement();
    }

    protected override void OnHunt()
    {
        if (!isInDeepWater) { HandleShallowWater(); return; }
        base.OnHunt();
    }

    protected override void OnThreatDetected(Transform threat) { }

    protected override void OnFlee()
    {
        if (suffocating)
        {
            MoveToward(retreatPoint, data.maxSpeedWater * (1f - shallowSpeedPenalty));

            // Espasmos mecânicos por falta de ar aplicados no Rigidbody
            rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Force);

            if (Vector2.Distance(rb.position, retreatPoint) < 1.5f || isInDeepWater)
            {
                suffocating  = false;
                shallowTimer = 0f;
                ChangeState(CreatureState.Wander);
            }
            return;
        }
        ChangeState(CreatureState.Wander);
    }

    private void HandleShallowWater()
    {
        shallowTimer += Time.deltaTime;
        float penalty = Mathf.Clamp01(shallowTimer / oxygenDebuffDelay);
        rb.velocity *= (1f - shallowSpeedPenalty * penalty * Time.deltaTime);

        if (shallowTimer >= oxygenDebuffDelay && !suffocating)
        {
            suffocating = true;
            Vector2 away = playerTransform != null
                ? (rb.position - (Vector2)playerTransform.position).normalized
                : Vector2.up;
            retreatPoint = rb.position + away * 18f;
            ChangeState(CreatureState.Flee, 8f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeepWater")) { isInDeepWater = true; shallowTimer = 0f; suffocating = false; }
        if (other.CompareTag("Water")) { isInDeepWater = false; }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("DeepWater")) isInDeepWater = false;
    }
}