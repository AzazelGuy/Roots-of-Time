using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DynamicSpawner — Unity 2020
/// Pool manual com Stack<GameObject> (sem UnityEngine.Pool) PELA MOR DE DEUS THALES.
/// Instancia criaturas baseado na densidade do ecossistema (via Ecosystemmanager).
/// Respeita limites por espécie e distribui spawns em pontos configuráveis.
/// </summary>
public class Dynamicspawner : MonoBehaviour
{
    // ─── Configuração de spawn por espécie ────────────────────────────────────

    [System.Serializable]
    public class SpawnEntry
    {
        public string speciesName;
        public GameObject prefab;
        [Tooltip("Tag aplicada ao spawnar (para o Ecosystemmanager monitorar)")]
        public string tag;
        [Tooltip("É predador ou presa?")]
        public bool isPredator;
        [Tooltip("Quantidade mínima sempre presente na cena")]
        public int minCount = 1;
        [Tooltip("Quantidade máxima permitida na cena")]
        public int maxCount = 5;
        [Tooltip("Tamanho inicial do pool")]
        public int poolSize = 6;

        // Pool manual — substitui IObjectPool do Unity 6
        [HideInInspector] public Stack<GameObject> pool = new Stack<GameObject>();
        [HideInInspector] public int currentCount;
    }

    [Header("Espécies Gerenciadas")]
    [SerializeField] private List<SpawnEntry> spawnEntries = new List<SpawnEntry>();

    [Header("Pontos de Spawn")]
    [Tooltip("Transforms vazios espalhados pelo mapa onde criaturas podem aparecer")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Distância mínima do player para spawnar")]
    [SerializeField] private float minSpawnDistFromPlayer = 8f;

    [Header("Timing")]
    [Tooltip("Intervalo entre verificações de spawn (segundos)")]
    [SerializeField] private float spawnCheckInterval = 6f;

    [Tooltip("Delay de fade-in ao spawnar (segundos)")]
    [SerializeField] private float spawnFadeInDuration = 0.5f;

    // ─── Internos ─────────────────────────────────────────────────────────────

    private static Transform playerTransform;
    private float spawnTimer;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("PlayerMain");
        if (p != null) playerTransform = p.transform;

        InitializePools();
    }

    private void Start()
    {
        foreach (var entry in spawnEntries)
            SpawnUpToMinimum(entry);
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnCheckInterval;
            EvaluateAndSpawn();
        }
    }

    // ─── Pool Manual ──────────────────────────────────────────────────────────

    private void InitializePools()
    {
        foreach (var entry in spawnEntries)
        {
            if (entry.prefab == null) continue;

            // Pré-instancia o pool com poolSize objetos inativos
            for (int i = 0; i < entry.poolSize; i++)
            {
                var go = CreateInstance(entry);
                entry.pool.Push(go);
            }
        }
    }

    private GameObject CreateInstance(SpawnEntry entry)
    {
        var go = Instantiate(entry.prefab);
        go.SetActive(false);

        // Registra retorno ao pool quando a criatura morre
        if (go.TryGetComponent<CreatureHealth>(out var h))
            h.OnDied.AddListener(() => ReleaseToPool(go, entry));

        return go;
    }

    private GameObject GetFromPool(SpawnEntry entry)
    {
        GameObject go;

        if (entry.pool.Count > 0)
        {
            go = entry.pool.Pop();
            // Objeto pode ter sido destruído externamente
            if (go == null) go = CreateInstance(entry);
        }
        else
        {
            // Pool vazio: instancia um novo
            go = CreateInstance(entry);
        }

        go.SetActive(true);
        return go;
    }

    private void ReleaseToPool(GameObject go, SpawnEntry entry)
    {
        if (go == null) return;
        entry.currentCount = Mathf.Max(0, entry.currentCount - 1);
        go.SetActive(false);
        entry.pool.Push(go);
    }

    // ─── Lógica de Spawn ──────────────────────────────────────────────────────

    private void EvaluateAndSpawn()
    {
        if (Ecosystemmanager.Instance == null) return;

        var recommendation = Ecosystemmanager.Instance.GetSpawnRecommendation();

        foreach (var entry in spawnEntries)
        {
            UpdateCount(entry);

            bool shouldSpawnMore = false;

            if (entry.currentCount < entry.minCount)
                shouldSpawnMore = true;

            if (recommendation == Ecosystemmanager.SpawnRecommendation.SpawnPredator
                && entry.isPredator && entry.currentCount < entry.maxCount)
                shouldSpawnMore = true;

            if (recommendation == Ecosystemmanager.SpawnRecommendation.SpawnPrey
                && !entry.isPredator && entry.currentCount < entry.maxCount)
                shouldSpawnMore = true;

            if (shouldSpawnMore)
                SpawnOne(entry);
        }
    }

    private void SpawnUpToMinimum(SpawnEntry entry)
    {
        UpdateCount(entry);
        int deficit = entry.minCount - entry.currentCount;
        for (int i = 0; i < deficit; i++)
            SpawnOne(entry);
    }

    private void SpawnOne(SpawnEntry entry)
    {
        if (entry.prefab == null) return;

        Vector2? point = GetValidSpawnPoint();
        if (point == null) return;

        var go = GetFromPool(entry);
        go.transform.position = point.Value;
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        entry.currentCount++;
        StartCoroutine(FadeIn(go, spawnFadeInDuration));
    }

    // ─── Utilitários ──────────────────────────────────────────────────────────

    private void UpdateCount(SpawnEntry entry)
    {
        if (string.IsNullOrEmpty(entry.tag)) return;
        entry.currentCount = GameObject.FindGameObjectsWithTag(entry.tag).Length;
    }

    private Vector2? GetValidSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            var pt = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (pt == null) continue;

            if (playerTransform != null)
            {
                float dist = Vector2.Distance(pt.position, playerTransform.position);
                if (dist < minSpawnDistFromPlayer) continue;
            }

            return pt.position;
        }

        return null;
    }

    private IEnumerator FadeIn(GameObject go, float duration)
    {
        if (!go.TryGetComponent<SpriteRenderer>(out var sr)) yield break;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a      = Mathf.Clamp01(elapsed / duration);
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        foreach (var pt in spawnPoints)
            if (pt != null) Gizmos.DrawWireSphere(pt.position, 0.5f);
    }
}