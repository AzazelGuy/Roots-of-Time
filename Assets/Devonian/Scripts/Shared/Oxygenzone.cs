using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OxygenZone — P3
/// Zonas que afetam criaturas com debuff de velocidade baseado no nível de oxigênio.
/// Já preparado para o Dunkleosteus (raso = pouco oxigênio).
///
/// Como funciona:
/// - Coloque um BoxCollider2D (isTrigger) cobrindo a zona rasa do mapa.
/// - Configure o OxygenLevel (0 = sufocante, 1 = saturado).
/// - Criaturas na lista affectedTags recebem debuff de velocidade.
/// - Criaturas com "CanSuffocate = true" no script entram em Flee após threshold.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Oxygenzone : MonoBehaviour
{
    [Header("Configuração da Zona")]
    [Tooltip("Nível de oxigênio da zona (0 = sufocante, 1 = normal)")]
    [SerializeField, Range(0f, 1f)] private float oxygenLevel = 0.3f;

    [Tooltip("Tags de criaturas afetadas por esta zona")]
    [SerializeField] private string[] affectedTags = { "LargePredator", "Predator" };

    [Tooltip("Tags imunes (ex: criaturas que respiram ar)")]
    [SerializeField] private string[] immuneTags = { "PlayerMain" };

    [Header("Debuff")]
    [Tooltip("Redução máxima de velocidade quando oxigênio = 0 (0–1)")]
    [SerializeField, Range(0f, 1f)] private float maxSpeedPenalty = 0.6f;

    [Tooltip("Segundos até começar a aplicar debuff de sufocamento")]
    [SerializeField] private float suffocationDelay = 3f;

    // ─── Internos ─────────────────────────────────────────────────────────────
    private readonly Dictionary<GameObject, float> timeInZone = new Dictionary<GameObject, float>();

    // ─── Colisão ──────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsImmune(other.gameObject)) return;
        if (!IsAffected(other.gameObject)) return;

        if (!timeInZone.ContainsKey(other.gameObject))
            timeInZone[other.gameObject] = 0f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsImmune(other.gameObject)) return;
        if (!IsAffected(other.gameObject)) return;
        if (!timeInZone.ContainsKey(other.gameObject)) return;

        timeInZone[other.gameObject] += Time.deltaTime;
        float time = timeInZone[other.gameObject];

        if (time < suffocationDelay) return;

        float debuffProgress = Mathf.Clamp01((time - suffocationDelay) / suffocationDelay);
        float penalty = maxSpeedPenalty * (1f - oxygenLevel) * debuffProgress;

        ApplyDebuff(other.gameObject, penalty);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (timeInZone.ContainsKey(other.gameObject))
        {
            timeInZone.Remove(other.gameObject);
            RemoveDebuff(other.gameObject);
        }
    }

    // ─── Debuff ───────────────────────────────────────────────────────────────

    private void ApplyDebuff(GameObject creature, float penalty)
    {
        if (!creature.TryGetComponent<Rigidbody2D>(out var rb)) return;

        // Reduz velocidade diretamente
        rb.velocity *= (1f - penalty * Time.deltaTime);

        // Avisa a IA para entrar em Flee se o sufocamento estiver grave
        if (penalty > 0.4f && creature.TryGetComponent<DunkleosteusAI>(out _))
        {
            // O DunkleosteusAI já lida com isso via isInDeepWater = false
            // Aqui apenas reforçamos o sinal caso outros predadores grandes estejam na zona
        }
    }

    private void RemoveDebuff(GameObject creature)
    {
        // Debuff é calculado on-the-fly, não há estado para limpar
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private bool IsAffected(GameObject go)
    {
        foreach (var tag in affectedTags)
            if (go.CompareTag(tag)) return true;
        return false;
    }

    private bool IsImmune(GameObject go)
    {
        foreach (var tag in immuneTags)
            if (go.CompareTag(tag)) return true;
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.15f);
        var col = GetComponent<Collider2D>();
        if (col != null) Gizmos.DrawCube(transform.position, col.bounds.size);

        // Exibe nível de oxigênio visualmente
        Gizmos.color = Color.Lerp(Color.red, Color.cyan, oxygenLevel);
        Gizmos.DrawWireCube(transform.position, col != null ? col.bounds.size : Vector3.one);
    }
}