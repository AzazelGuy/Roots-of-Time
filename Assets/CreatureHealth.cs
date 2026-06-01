using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente de saúde genérico.
/// Usado pelo Player e por todas as criaturas da IA.
/// Suporta inicialização tanto por CreatureData (bestiário / IA)
/// quanto por CreatureDataEntity (entidades controláveis).
/// </summary>
public class CreatureHealth : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private int  maxHealth = 10;
    [SerializeField] private bool isPlayer  = false;

    [Header("Eventos")]
    public UnityEvent<int> OnDamaged; // passa o dano recebido
    public UnityEvent      OnDied;

    // ─── Estado ───────────────────────────────────────────────────────────────

    public int  CurrentHealth { get; private set; }
    public bool IsDead        { get; private set; }

    // ─── Inicialização ────────────────────────────────────────────────────────

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Inicializa a partir do ScriptableObject de bestiário.
    /// </summary>
    public void InitFromData(CreatureData data)
    {
        if (data == null) return;
        maxHealth     = data.maxHealth;
        CurrentHealth = maxHealth;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnDamaged?.Invoke(amount);

        if (isPlayer && GameManager.Instance != null)
            GameManager.Instance.PlayerHealth = CurrentHealth;

        if (CurrentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    // ─── Morte ────────────────────────────────────────────────────────────────

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDied?.Invoke();

        // Criaturas de IA apenas se desativam; o Spawner cuida do pool.
        if (!isPlayer)
            gameObject.SetActive(false);
    }
}
