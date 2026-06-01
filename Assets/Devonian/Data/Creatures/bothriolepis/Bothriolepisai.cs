using UnityEngine;

public class Bothriolepisai : CreatureAI
{
    [Header("Bothriolepis")]
    [SerializeField] private float maxHeightAboveGround = 1.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float panicSpeedMultiplier = 1.7f;

    private float forageTimer;
    private bool isForaging;

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new string[0];
        predatorTags = new[] { "PlayerMain", "Predator", "LargePredator" };
        forageTimer = Random.Range(3f, 7f);
        ChangeState(CreatureState.Wander);
    }

    protected override void OnWander()
    {
        StayNearGround();
        ScanForThreats();

        if (isForaging)
        {
            // Diminui velocidade para vasculhar a areia
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 3f);
            forageTimer -= Time.deltaTime;

            if (forageTimer <= 0f)
            {
                isForaging = false;
                forageTimer = Random.Range(4f, 8f);
            }
            return;
        }

        WanderMovement();

        forageTimer -= Time.deltaTime;
        if (forageTimer <= 0f)
        {
            isForaging = true;
            forageTimer = Random.Range(1.5f, 3f); // tempo comendo
        }
    }

    protected override void OnFlee()
    {
        StayNearGround();
        isForaging = false;

        if (target == null) { ChangeState(CreatureState.Wander); return; }

        MoveAway(target.position, data.maxSpeedWater * panicSpeedMultiplier);

        float dist = Vector2.Distance(rb.position, target.position);
        if (dist > data.detectionRange * 2f || stateTimer <= 0f)
        { target = null; ChangeState(CreatureState.Wander); }
    }

    protected override void OnThreatDetected(Transform threat)
    { target = threat; ChangeState(CreatureState.Flee, 6f); }

    protected override void OnHunger() => ChangeState(CreatureState.Wander);

    public override void OnDamageTaken(int amount)
    {
        if (playerTransform != null)
        { target = playerTransform; ChangeState(CreatureState.Flee, 8f); }
    }

    private void StayNearGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, 6f, groundLayer);
        if (!hit) return;

        float targetY = hit.point.y + maxHeightAboveGround;
        if (rb.position.y > targetY + 0.1f)
        {
            Vector2 v = rb.velocity;
            v.y = Mathf.Lerp(v.y, -2.5f, Time.deltaTime * 2.5f);
            rb.velocity = v;
        }

        // Inclina��o sutil ao farejar o fundo
        if (isForaging && spriteRenderer != null)
        {
            float rotCorrection = spriteRenderer.flipX ? -15f : 15f;
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, rotCorrection, Time.deltaTime * 3f));
        }
    }
}