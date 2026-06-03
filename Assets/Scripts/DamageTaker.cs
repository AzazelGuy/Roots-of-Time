using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTaker : MonoBehaviour
{
    [Header("Invencibilidade")]
    [SerializeField] float invenciTime = 1.5f;

    public Transform spriteTransform;
    [Header("Regenera��o")]
    public float RegenTime = 0f;

    [Header("�udio")]
    [SerializeField] AudioClip Hurt;

    [HideInInspector] public float invenciTimer = 0f;

    private bool isInvincible = false;
    private float RegenTimer = 0f;

    private void Start()
    {
        RegenTimer = RegenTime;
    }

    private void Update()
    {
        UpdateInvencibility();
        HandleRegen();
    }

    private void UpdateInvencibility()
    {
        if (invenciTimer <= 0f)
        {
            invenciTimer = 0f;
            isInvincible = false;
            spriteTransform.gameObject.SetActive(true);
            return;
        }
        else
        {
            spriteTransform.gameObject.SetActive(
            Mathf.FloorToInt(Time.time * 20) % 2 == 0);
        }

        invenciTimer -= Time.deltaTime;
    }

    private void HandleRegen()
    {
        if (RegenTimer <= 0 &&
            GameManager.Instance.PlayerHealth < GameManager.Instance.PlayerHealthMax)
        {
            GameManager.Instance.PlayerHealth += 2;
            RegenTimer = RegenTime;
        }

        if (RegenTimer > 0)
            RegenTimer -= Time.deltaTime;
    }

    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyAttack"))
            return;

        if (isInvincible || invenciTimer > 0)
            return;

        EnemySwimAIDevo enemy = collision.GetComponentInParent<EnemySwimAIDevo>();
        if (enemy != null) GameManager.Instance.PlayerHealth -= enemy.Damage;

        DunkeosteusBoss boss = collision.GetComponentInParent<DunkeosteusBoss>();
        if (boss != null) GameManager.Instance.PlayerHealth -= 25;

        RegenTimer = invenciTime;

        invenciTimer = invenciTime;
        isInvincible = true;

        AudioController.Instance.PlaySFXRandomPitch(Hurt, -.8f, 1f);

        Debug.Log("Tomou dano!");
    }
}