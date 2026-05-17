using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterModeDefense : MonoBehaviour
{
    [Header("DefenseTiming")]
    [SerializeField] float DefenseTime = 0.5f;
    [SerializeField] float ParryTime = 0.15f;
    [SerializeField] float ParryStrenght = 25f;
    [SerializeField] float defenseCooldown = 0.2f;
    SpriteRenderer spr;
    float invenciTime = 1.5f;
    [HideInInspector] public float invenciTimer = 0f;
    float lastInputTime = -999f;
    float inputTime;
    float perfectWindowStart = 0.05f;
    public GameObject ParryParticles;
    public float RegenTime = 0;
    float RegenTimer = 0;
    public Sprite[] spriteteste;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color defenseColor = Color.cyan;
    [SerializeField] AudioClip Parry;
    private bool isInvincible = false;
    private void Start()
    {
        spr = GetComponentInChildren<SpriteRenderer>();
        RegenTimer = RegenTime;
    }
    private void Update()
    {
        UpdateInvencibility();

        if (Input.GetMouseButtonDown(0) && Time.time >= lastInputTime + defenseCooldown)
        {
            inputTime = Time.time;
            lastInputTime = Time.time;
        }

        float timeSinceInput = Time.time - inputTime;

        bool isDefending = timeSinceInput <= DefenseTime;

        spr.color = isDefending ? defenseColor : normalColor;

        HandleRegen();
    }

    private void UpdateInvencibility()
    {
        if (invenciTimer <= 0)
        {
            invenciTimer = 0;
            isInvincible = false; // Libera quando o timer acabar
            return;
        }

        invenciTimer -= Time.deltaTime;
    }

    private void HandleRegen()
    {
        if (RegenTimer <= 0 && GameManager.Instance.PlayerHealth < GameManager.Instance.PlayerHealthMax)
        {
            GameManager.Instance.PlayerHealth += 1;
            RegenTimer = RegenTime;
        }

        if (RegenTimer > 0)
            RegenTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyAttack")) return;

        // Bloqueia múltiplos hits no mesmo frame
        if (isInvincible || invenciTimer > 0) return;
        EnemySwimAI enemy = null;
        AnomalocarisChase Boss = null;
        if (collision.GetComponentInParent<EnemySwimAI>() != null)
        {
            enemy = collision.GetComponentInParent<EnemySwimAI>();
        }else if (collision.GetComponentInParent<AnomalocarisChase>() != null)
        {
            Boss = collision.GetComponentInParent<AnomalocarisChase>();
        }

        if (enemy != null){
            ApplyEnemyKnockback(enemy);
        }else if (Boss != null)
        {
            ApplyEnemyKnockbackBoss(Boss);
        }

            // Ativa imediatamente antes de qualquer dano
            isInvincible = true;
        invenciTimer = invenciTime;

        float timeSinceInput = Time.time - inputTime;
        bool isParry = timeSinceInput >= perfectWindowStart && timeSinceInput <= ParryTime;
        bool isDefense = timeSinceInput <= DefenseTime;

        if (enemy != null)
        {
            if (isParry)
                HandleParry(enemy);
            else if (isDefense)
                HandleHalfDamage(enemy);
            else
                HandleFullDamage(enemy);
        }else if (Boss != null)
        {
            if (isParry)
                HandleParryBoss(Boss);
            else if (isDefense)
                HandleHalfDamageBoss(Boss);
            else
                HandleFullDamageBoss(Boss);
        }
    }

    private void ApplyEnemyKnockback(EnemySwimAI enemy)
    {
        enemy.knockbackTimer = enemy.knockbackTime;
        enemy.rb.velocity = Vector2.zero;

        Vector2 dir = (enemy.spriteTransform.position - transform.position).normalized;
        enemy.rb.AddForce(dir * ParryStrenght, ForceMode2D.Impulse);
    }

    private void ApplyEnemyKnockbackBoss(AnomalocarisChase enemy)
    {
        enemy.knockbackTimer = enemy.knockbackTime;
        enemy.rb.velocity = Vector2.zero;

        Vector2 dir = (enemy.spriteTransform.position - transform.position).normalized;
        enemy.rb.AddForce(dir * ParryStrenght, ForceMode2D.Impulse);
    }

    private void HandleParry(EnemySwimAI enemy)
    {
        AudioController.Instance.PlaySFX(Parry);
        Debug.Log("Parry e Machucar Inimigo!");

        enemy.Health--;

        ApplyEnemyKnockback(enemy); // reforça impacto no parry
        enemy.StunTimer = enemy.StunTime;
        Instantiate(ParryParticles, transform.position, Quaternion.identity);
        var flash = Instantiate(ParryParticles, transform.position, Quaternion.identity);
        flash.transform.parent = transform;

        if (HitStopEffect.Instance != null)
        HitStopEffect.Instance.StopTime(0.5f);
    }

    private void HandleParryBoss(AnomalocarisChase enemy)
    {
        AudioController.Instance.PlaySFX(Parry);
        Debug.Log("Parry e Machucar Inimigo!");

        ApplyEnemyKnockbackBoss(enemy); // reforça impacto no parry
        enemy.StunTimer = enemy.StunTime;
        Instantiate(ParryParticles, transform.position, Quaternion.identity);
        var flash = Instantiate(ParryParticles, transform.position, Quaternion.identity);
        flash.transform.parent = transform;

        if (HitStopEffect.Instance != null)
            HitStopEffect.Instance.StopTime(0.5f);
    }
    private void HandleHalfDamage(EnemySwimAI enemy)
    {
        Debug.Log("Metade do Dano!");
        GameManager.Instance.PlayerHealth -= 1;

        ApplyEnemyKnockback(enemy);
        RegenTimer = RegenTime;
    }
    private void HandleHalfDamageBoss(AnomalocarisChase enemy)
    {
        Debug.Log("Metade do Dano!");
        GameManager.Instance.PlayerHealth -= 1;

        ApplyEnemyKnockbackBoss(enemy);
        RegenTimer = RegenTime;
    }

    private void HandleFullDamage(EnemySwimAI enemy)
    {
        ApplyEnemyKnockback(enemy);
        Debug.Log("Dano Total!");
        GameManager.Instance.PlayerHealth -= 2;
        RegenTimer = RegenTime;
    }
    private void HandleFullDamageBoss(AnomalocarisChase enemy)
    {
        ApplyEnemyKnockbackBoss(enemy);
        Debug.Log("Dano Total!");
        GameManager.Instance.PlayerHealth -= 2;
        RegenTimer = RegenTime;
    }
}
