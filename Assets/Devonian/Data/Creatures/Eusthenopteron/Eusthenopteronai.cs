using System.Collections.Generic;
using UnityEngine;

public class Eusthenopteronai : CreatureAI
{
    [Header("Eusthenopteron")]
    [SerializeField] private float schoolAlertRadius = 6f;
    [SerializeField] private float desperateBiteDamage = 2f;
    [SerializeField] private float isolatedThreshold = 3f;

    private bool hasCounterAttacked;
    private static readonly List<Eusthenopteronai> allSchool = new List<Eusthenopteronai>();

    protected override void Awake()
    {
        base.Awake();
        preyTags     = new[] { "SmallPrey" };
        predatorTags = new[] { "PlayerMain", "Predator", "LargePredator" };
        allSchool.Add(this);
        ChangeState(CreatureState.Wander);
    }

    private void OnDestroy() => allSchool.Remove(this);
    private void OnDisable() => allSchool.Remove(this);

    protected override void OnWander()
    {
        // Força sutil de coesão para manter o cardume agrupado organicamente
        ApplySchoolCohesion();
        WanderMovement();
        ScanForThreats();
        hasCounterAttacked = false;

        if (data != null && hungerLevel < data.hungerMax * 0.75f)
            ChangeState(CreatureState.Hunger);
    }

    protected override void OnFlee()
    {
        if (target == null) { ChangeState(CreatureState.Wander); return; }

        if (!hasCounterAttacked && IsIsolated() && attackCooldownTimer <= 0f)
        {
            float dist = Vector2.Distance(rb.position, target.position);
            if (dist <= data.biteRange) { DesperateBite(); return; }
        }

        // Fuga em dispersão rápida
        MoveAway(target.position, data.maxSpeedWater * 1.6f);

        float fleeRange = data.detectionRange * 2f;
        if (Vector2.Distance(rb.position, target.position) > fleeRange || stateTimer <= 0f)
        { target = null; ChangeState(CreatureState.Wander); }
    }

    protected override void OnThreatDetected(Transform threat)
    {
        target = threat;
        AlertSchool(threat);
        ChangeState(CreatureState.Flee, 7f);
    }

    public override void OnDamageTaken(int amount)
    {
        if (playerTransform != null)
        {
            target = playerTransform;
            AlertSchool(playerTransform);
            ChangeState(CreatureState.Flee, 8f);
        }
    }

    private void ApplySchoolCohesion()
    {
        Vector2 centerOfMass = Vector2.zero;
        int neighborsCount = 0;

        foreach (var member in allSchool)
        {
            if (member == this || member == null) continue;
            float dist = Vector2.Distance(rb.position, member.rb.position);
            if (dist < schoolAlertRadius)
            {
                centerOfMass += (Vector2)member.transform.position;
                neighborsCount++;
            }
        }

        if (neighborsCount > 0)
        {
            centerOfMass /= neighborsCount;
            // Puxa suavemente em direção ao centro do bando
            Vector2 cohesionDir = (centerOfMass - rb.position).normalized;
            rb.AddForce(cohesionDir * (data.accelerationWater * 0.25f) * Time.deltaTime, ForceMode2D.Force);
        }
    }

    private void AlertSchool(Transform threat)
    {
        foreach (var member in allSchool)
        {
            if (member == this || member == null) continue;
            float dist = Vector2.Distance(rb.position, member.rb.position);
            if (dist <= schoolAlertRadius) member.ReceiveAlarm(threat);
        }
    }

    public void ReceiveAlarm(Transform threat)
    {
        if (currentState == CreatureState.Flee) return;
        target = threat;
        ChangeState(CreatureState.Flee, 7f);
    }

    private bool IsIsolated()
    {
        foreach (var member in allSchool)
        {
            if (member == this || member == null) continue;
            if (Vector2.Distance(rb.position, member.rb.position) < isolatedThreshold)
                return false;
        }
        return true;
    }

    private void DesperateBite()
    {
        if (target == null) return;
        if (target.TryGetComponent<CreatureHealth>(out var h))
            h.TakeDamage((int)desperateBiteDamage);
        if (target.CompareTag("PlayerMain") && Devoniancamera.Instance != null)
            Devoniancamera.Instance.Shake(0.15f, 0.15f);
        hasCounterAttacked  = true;
        attackCooldownTimer = data.attackCooldown;
        anim?.SetTrigger(AnimAttack);
    }

    // Correção aplicada aqui embaixo:
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, schoolAlertRadius);
    }
}