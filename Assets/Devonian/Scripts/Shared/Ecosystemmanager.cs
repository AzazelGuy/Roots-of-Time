using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EcosystemManager — P3
/// Monitora a densidade de predadores e presas no ecossistema.
/// Comunica os dados ao DynamicSpawner para tomar decisões de spawn.
/// Também faz predadores morrerem de fome se não houver presas suficientes.
/// </summary>
public class Ecosystemmanager : MonoBehaviour
{
    public static Ecosystemmanager Instance { get; private set; }

    [Header("Configuração do Ecossistema")]
    [Tooltip("Intervalo de análise do ecossistema em segundos")]
    [SerializeField] private float analysisInterval = 5f;

    [Tooltip("Proporção ideal de presas por predador")]
    [SerializeField] private float idealPreyPerPredator = 4f;

    [Tooltip("Quanto tempo (segundos) um predador sobrevive com fome = 0 antes de morrer")]
    [SerializeField] private float starvationDeathTime = 30f;

    [Header("Tags monitoradas")]
    [SerializeField] private string[] preyTags = { "Prey", "SmallPrey" };
    [SerializeField] private string[] predatorTags = { "Predator", "LargePredator" };

    // ─── Dados do ecossistema ─────────────────────────────────────────────────

    public int PreyCount { get; private set; }
    public int PredatorCount { get; private set; }
    public float PreyRatio => PredatorCount > 0 ? (float)PreyCount / PredatorCount : float.MaxValue;
    public bool PreyIsScarce => PreyRatio < idealPreyPerPredator * 0.5f;
    public bool PreyIsAbundant => PreyRatio > idealPreyPerPredator * 2f;

    // Mapa de starvation timers por instância
    private readonly Dictionary<CreatureAI, float> starvationTimers = new Dictionary<CreatureAI, float>();

    private float analysisTimer;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        analysisTimer -= Time.deltaTime;
        if (analysisTimer <= 0f)
        {
            analysisTimer = analysisInterval;
            AnalyzeEcosystem();
            TickStarvation();
        }
    }

    // ─── Análise ──────────────────────────────────────────────────────────────

    private void AnalyzeEcosystem()
    {
        int prey = 0;
        foreach (var tag in preyTags)
            prey += GameObject.FindGameObjectsWithTag(tag).Length;

        int pred = 0;
        foreach (var tag in predatorTags)
            pred += GameObject.FindGameObjectsWithTag(tag).Length;

        PreyCount     = prey;
        PredatorCount = pred;

        if (PreyIsScarce)
            Debug.Log($"[Ecosystem] ALERTA: Presas escassas! Ratio: {PreyRatio:F1}");
    }

    // ─── Starvation ───────────────────────────────────────────────────────────

    private void TickStarvation()
    {
        if (!PreyIsScarce) { starvationTimers.Clear(); return; }

        // Coleta todos os predadores vivos
        var predators = new List<CreatureAI>();
        foreach (var tag in predatorTags)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag(tag))
            {
                if (go.TryGetComponent<CreatureAI>(out var ai))
                    predators.Add(ai);
            }
        }

        // Incrementa timers e mata por inanição
        foreach (var ai in predators)
        {
            if (!starvationTimers.ContainsKey(ai))
                starvationTimers[ai] = 0f;

            starvationTimers[ai] += analysisInterval;

            if (starvationTimers[ai] >= starvationDeathTime)
            {
                Debug.Log($"[Ecosystem] {ai.gameObject.name} morreu de fome.");
                if (ai.TryGetComponent<CreatureHealth>(out var h))
                    h.TakeDamage(9999); // dano fatal
                starvationTimers.Remove(ai);
            }
        }

        // Remove timers de criaturas que morreram
        var toRemove = new List<CreatureAI>();
        foreach (var kv in starvationTimers)
            if (kv.Key == null || !kv.Key.gameObject.activeInHierarchy)
                toRemove.Add(kv.Key);
        foreach (var ai in toRemove)
            starvationTimers.Remove(ai);
    }

    // ─── API para outros sistemas ─────────────────────────────────────────────

    /// <summary>Retorna recomendação de spawn para o DynamicSpawner.</summary>
    public SpawnRecommendation GetSpawnRecommendation()
    {
        if (PreyIsAbundant && PredatorCount < 3)
            return SpawnRecommendation.SpawnPredator;
        if (PreyIsScarce && PreyCount < 5)
            return SpawnRecommendation.SpawnPrey;
        return SpawnRecommendation.Balanced;
    }

    public enum SpawnRecommendation { Balanced, SpawnPredator, SpawnPrey }
}