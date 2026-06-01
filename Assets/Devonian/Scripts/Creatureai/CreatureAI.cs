using UnityEngine;

/// <summary>
/// CreatureAI — Sincronizada e Estabilizada.
/// Movimentação precisa baseada em velocidade linear, rotação estrita (sem andar de ré),
/// e pronta para Object Pooling e monitoramento do Ecossistema.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CreatureHealth))]
public abstract class CreatureAI : MonoBehaviour
{
    public enum CreatureState { Wander, Hunger, Hunt, Flee, Eat, Idle, Ambush }

    [Header("Dados Biológicos")]
    [SerializeField] protected CreatureData data;

    [Header("Tiers de Predação")]
    [SerializeField] protected string[] preyTags;
    [SerializeField] protected string[] predatorTags;

    [Header("Comportamento")]
    [SerializeField] private float aiStartDelay = 2f;
    [SerializeField] protected float hungerThreshold = 0.6f;

    protected CreatureState currentState = CreatureState.Wander;
    protected Rigidbody2D rb;
    protected CreatureHealth health;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;
    protected Transform target;

    public float hungerLevel { get; protected set; } // Liberado para leitura externa se necessário
    protected float stateTimer;
    protected float attackCooldownTimer;

    private bool aiActive = false;
    private float startDelayTimer;

    private Vector2 wanderTarget;
    private float wanderTimer;

    protected static Transform playerTransform;

    protected static readonly int AnimSpeed = Animator.StringToHash("Speed");
    protected static readonly int AnimAttack = Animator.StringToHash("Attack");
    protected static readonly int AnimFlee = Animator.StringToHash("Flee");

    protected float EffectiveDetectionRange => data != null ? data.detectionRange : 8f;

    protected virtual void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        health         = GetComponent<CreatureHealth>();
        anim           = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("PlayerMain");
            if (p != null) playerTransform = p.transform;
        }

        health.OnDamaged.AddListener(OnDamageTaken);
        SetupPhysics();
    }

    /// <summary>
    /// Chamado pelo DynamicSpawner sempre que a criatura sai do Pool.
    /// Reseta todos os parâmetros para a IA voltar a funcionar do zero.
    /// </summary>
    public virtual void ResetAI()
    {
        if (data != null)
        {
            health.InitFromData(data);
            hungerLevel = data.hungerMax;
        }

        currentState = CreatureState.Wander;
        target = null;
        aiActive = false;
        startDelayTimer = aiStartDelay;
        rb.velocity = Vector2.zero;
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        if (data == null) return;
        rb.drag = data.linearDragWater;
        rb.gravityScale = data.gravityScaleWater;
    }

    protected virtual void Update()
    {
        if (health.IsDead) return;

        if (!aiActive)
        {
            startDelayTimer -= Time.deltaTime;
            if (startDelayTimer <= 0f) aiActive = true;
            WanderMovement();
            return;
        }

        attackCooldownTimer -= Time.deltaTime;
        stateTimer          -= Time.deltaTime;
        TickHunger();
        RunFSM();
        UpdateAnimator();
    }

    private void RunFSM()
    {
        switch (currentState)
        {
            case CreatureState.Wander: OnWander(); break;
            case CreatureState.Hunger: OnHunger(); break;
            case CreatureState.Hunt: OnHunt(); break;
            case CreatureState.Flee: OnFlee(); break;
            case CreatureState.Eat: OnEat(); break;
            case CreatureState.Idle: OnIdle(); break;
            case CreatureState.Ambush: OnAmbush(); break;
        }
    }

    protected virtual void OnWander()
    {
        WanderMovement();
        ScanForThreats();

        if (data != null && hungerLevel < data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Hunger);
    }

    protected virtual void OnHunger()
    {
        Transform prey = FindNearest(preyTags, EffectiveDetectionRange);
        if (prey != null) { target = prey; ChangeState(CreatureState.Hunt); return; }

        WanderMovement();
        ScanForThreats();

        if (hungerLevel >= data.hungerMax * hungerThreshold)
            ChangeState(CreatureState.Wander);
    }

    protected virtual void OnHunt()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        { ChangeState(CreatureState.Wander); return; }

        ScanForThreats();
        MoveToward(target.position, data.maxSpeedWater);

        float dist = Vector2.Distance(rb.position, target.position);
        if (dist <= data.biteRange && attackCooldownTimer <= 0f)
            TryAttack(target);
    }

    protected virtual void OnFlee()
    {
        if (target == null) { ChangeState(CreatureState.Wander); return; }
        MoveAway(target.position, data.maxSpeedWater * 1.3f);
        float dist = Vector2.Distance(rb.position, target.position);
        if (dist > EffectiveDetectionRange * 1.5f || stateTimer <= 0f)
        { target = null; ChangeState(CreatureState.Wander); }
    }

    protected virtual void OnEat()
    {
        if (stateTimer <= 0f)
        {
            hungerLevel = Mathf.Min(hungerLevel + 40f, data != null ? data.hungerMax : 100f);
            ChangeState(CreatureState.Wander);
        }
    }

    protected virtual void OnIdle() => rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 2f);
    protected virtual void OnAmbush() => rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 5f);

    // ─── Movimentação Absoluta (Sem Drift) ────────────────────────────────────

    protected void MoveToward(Vector2 destination, float speed)
    {
        Vector2 dir = (destination - rb.position).normalized;
        Vector2 desiredVelocity = dir * speed;

        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, Time.deltaTime * data.accelerationWater);
        ApplyRotation(rb.velocity);
    }

    protected void MoveAway(Vector2 threat, float speed)
    {
        Vector2 dir = (rb.position - threat).normalized;
        Vector2 desiredVelocity = dir * speed;

        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, Time.deltaTime * data.accelerationWater * 1.2f);
        ApplyRotation(rb.velocity);
    }

    protected void WanderMovement()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f || Vector2.Distance(rb.position, wanderTarget) < 1f)
        {
            wanderTarget = rb.position + (Vector2)Random.insideUnitCircle * 5f;
            wanderTimer  = Random.Range(3f, 6f);
        }
        MoveToward(wanderTarget, data.maxSpeedWater * 0.5f);
    }

    /// <summary>
    /// Trava a rotação no vetor real de movimento para garantir zero movimentação de ré.
    /// </summary>
    private void ApplyRotation(Vector2 currentVelocity)
    {
        if (currentVelocity.sqrMagnitude < 0.05f) return;

        float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        bool goingLeft = Mathf.Abs(angle) > 90f;

        if (spriteRenderer != null)
            spriteRenderer.flipX = goingLeft;

        float visualAngle = goingLeft
            ? (angle > 0f ? 180f - angle : -180f - angle)
            : angle;

        float rotSpeed = data != null ? data.rotationSpeed : 180f;
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, visualAngle, Time.deltaTime * (rotSpeed / 180f) * 5f));
    }

    // ─── Utilitários ──────────────────────────────────────────────────────────

    protected Transform FindNearest(string[] tags, float range = -1f)
    {
        if (tags == null || tags.Length == 0) return null;
        if (range < 0f) range = data != null ? data.detectionRange : 8f;
        Transform nearest = null;
        float minDist = range;

        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (var go in targets)
            {
                if (!go.activeInHierarchy || go.transform == transform) continue;
                float d = Vector2.Distance(rb.position, go.transform.position);
                if (d < minDist) { minDist = d; nearest = go.transform; }
            }
        }
        return nearest;
    }

    protected void ScanForThreats()
    {
        Transform threat = FindNearest(predatorTags, EffectiveDetectionRange);
        if (threat != null) { target = threat; OnThreatDetected(threat); }
    }

    protected virtual void OnThreatDetected(Transform threat) => ChangeState(CreatureState.Flee);

    protected virtual void TryAttack(Transform victim)
    {
        if (victim.TryGetComponent<CreatureHealth>(out var h))
            h.TakeDamage((int)data.biteForce);
        if (victim.CompareTag("PlayerMain") && Devoniancamera.Instance != null)
            Devoniancamera.Instance.Shake(data.biteForce / 20f, 0.2f);

        attackCooldownTimer = data.attackCooldown;
        anim?.SetTrigger(AnimAttack);
    }

    private void TickHunger()
    {
        if (data == null) return;
        hungerLevel -= data.metabolicRate * Time.deltaTime;
        hungerLevel  = Mathf.Max(hungerLevel, 0f);
    }

    public virtual void OnDamageTaken(int amount) { }

    protected void ChangeState(CreatureState newState, float duration = 5f)
    {
        currentState = newState;
        stateTimer   = duration;
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;
        anim.SetFloat(AnimSpeed, rb.velocity.magnitude);
        anim.SetBool(AnimFlee, currentState == CreatureState.Flee);
    }
}