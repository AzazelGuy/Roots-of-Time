using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTaker : MonoBehaviour
{
    [Header("Invencibilidade")]
    [SerializeField] float invenciTime = 1.5f;

    [Header("Regeneração")]
    public float RegenTime = 0f;

    [Header("Áudio")]
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
            return;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("EnemyAttack"))
            return;

        if (isInvincible || invenciTimer > 0)
            return;

        GameManager.Instance.PlayerHealth -= 25;

        RegenTimer = invenciTime;

        invenciTimer = 5f;
        isInvincible = true;

        AudioController.Instance.PlaySFXRandomPitch(Hurt, -.8f, 1f);

        Debug.Log("Tomou dano!");
    }
}