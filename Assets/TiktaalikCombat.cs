using System.Collections;
using UnityEngine;

/// <summary>
/// Combate do Tiktaalik: neck snap com hitbox.
/// Unity 2020 — Input Manager clássico.
/// Depende de: CreatureData, CreatureHealth
/// </summary>
public class TiktaalikCombat : MonoBehaviour
{
    [Header("Dados Biológicos")]
    [SerializeField] private CreatureData data;

    [Header("Pescoço / Hitbox")]
    [SerializeField] private PolygonCollider2D neckHitbox;
    [SerializeField] private Transform         headTransform;
    [SerializeField] private float             neckSnapDistance = 1.2f;
    [SerializeField] private float             neckSnapSpeed    = 0.1f;

    private Animator anim;
    private bool     isAttacking;
    private float    attackCooldownTimer;

    private static readonly int AnimAttack = Animator.StringToHash("NeckSnap");

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (neckHitbox != null) neckHitbox.enabled = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && attackCooldownTimer <= 0f && !isAttacking)
            StartCoroutine(NeckSnap());
    }

    private IEnumerator NeckSnap()
    {
        if (data == null) yield break;

        isAttacking = true;
        anim?.SetTrigger(AnimAttack);

        Vector3 originPos   = headTransform != null ? headTransform.localPosition : Vector3.zero;
        Vector3 mouseWorld  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetLocal = transform.InverseTransformPoint(mouseWorld);
        targetLocal         = Vector3.ClampMagnitude(targetLocal, neckSnapDistance);

        // Avança a cabeça até o alvo
        float elapsed = 0f;
        while (elapsed < neckSnapSpeed)
        {
            elapsed += Time.deltaTime;
            if (headTransform != null)
                headTransform.localPosition = Vector3.Lerp(
                    originPos, targetLocal, elapsed / neckSnapSpeed);
            yield return null;
        }

        // Ativa hitbox brevemente
        if (neckHitbox != null) neckHitbox.enabled = true;
        yield return new WaitForSeconds(0.08f);
        if (neckHitbox != null) neckHitbox.enabled = false;

        // Retorna a cabeça à posição original
        elapsed = 0f;
        while (elapsed < neckSnapSpeed * 1.5f)
        {
            elapsed += Time.deltaTime;
            if (headTransform != null)
                headTransform.localPosition = Vector3.Lerp(
                    targetLocal, originPos, elapsed / (neckSnapSpeed * 1.5f));
            yield return null;
        }

        if (headTransform != null)
            headTransform.localPosition = originPos;

        attackCooldownTimer = data.attackCooldown;
        isAttacking         = false;

        StartCoroutine(TickCooldown());
    }

    private IEnumerator TickCooldown()
    {
        while (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking || data == null) return;
        if (other.TryGetComponent<CreatureHealth>(out var health))
            health.TakeDamage((int)data.biteForce);
    }
}