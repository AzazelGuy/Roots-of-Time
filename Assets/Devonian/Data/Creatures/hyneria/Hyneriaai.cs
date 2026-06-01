using System.Collections;
using UnityEngine;

public class Hyneriaai : CreatureAI
{
    [Header("Hyneria")]
    [SerializeField] private float ambushRadius = 3.5f;
    [SerializeField] private float lungeForce = 24f;
    [SerializeField] private float lungeCooldown = 4.5f;

    private bool isLunging;
    private float lungeCooldownTimer;
    private static readonly int AnimLunge = Animator.StringToHash("Lunge");

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "Prey", "SmallPrey", "PlayerMain" };
        predatorTags = new string[0];
        ChangeState(CreatureState.Ambush, float.MaxValue);
    }

    protected override void Update()
    {
        lungeCooldownTimer -= Time.deltaTime;
        base.Update();
    }

    protected override void OnAmbush()
    {
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 6f);

        bool isHungry = hungerLevel < data.hungerMax * hungerThreshold;
        if (!isHungry || isLunging || lungeCooldownTimer > 0f) return;

        // Foca no player
        if (playerTransform != null && Vector2.Distance(rb.position, playerTransform.position) <= ambushRadius)
        {
            StartCoroutine(ExecuteLunge(playerTransform));
            return;
        }

        // Foca em presas gerais
        Transform prey = FindNearest(new[] { "Prey", "SmallPrey" }, ambushRadius);
        if (prey != null) StartCoroutine(ExecuteLunge(prey));
    }

    protected override void OnThreatDetected(Transform threat) { }

    private IEnumerator ExecuteLunge(Transform victim)
    {
        isLunging = true;

        // Ajuste org�nico pr�-bote: Vira instantaneamente pro alvo antes do impacto f�sico
        Vector2 targetDir = ((Vector2)victim.position - rb.position).normalized;
        if (spriteRenderer != null) spriteRenderer.flipX = targetDir.x < 0f;

        anim?.SetTrigger(AnimLunge);
        yield return new WaitForSeconds(0.1f); // Janela visual de antecipa��o

        if (victim != null)
        {
            Vector2 finalDir = ((Vector2)victim.position - rb.position).normalized;
            rb.AddForce(finalDir * lungeForce, ForceMode2D.Impulse);
        }

        float timeout = 1.3f;
        while (timeout > 0f)
        {
            timeout -= Time.deltaTime;
            if (victim == null || !victim.gameObject.activeInHierarchy) break;

            if (Vector2.Distance(rb.position, victim.position) <= data.biteRange)
            {
                TryAttack(victim);
                break;
            }
            yield return null;
        }

        lungeCooldownTimer = lungeCooldown;
        isLunging          = false;
        ChangeState(CreatureState.Ambush, float.MaxValue);
    }
}