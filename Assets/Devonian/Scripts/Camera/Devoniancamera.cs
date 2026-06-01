using System.Collections;
using UnityEngine;

/// <summary>
/// Câmera 2D simples: segue o player com smoothing e suporta Camera Shake.
/// Compatível com a câmera existente do Cambriano — apenas arraste este script
/// para o GameObject da câmera e configure os campos no Inspector.
/// </summary>
public class Devoniancamera : MonoBehaviour
{
    // ─── Referências ──────────────────────────────────────────────────────────

    [Header("Alvo")]
    [SerializeField] private Transform target;
    [Tooltip("Se verdadeiro, busca automaticamente o objeto com tag 'PlayerMain'")]
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Seguimento")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [Tooltip("Lookahead: a câmera 'olha um pouco à frente' do jogador")]
    [SerializeField] private float lookaheadDist = 0.8f;

    [Header("Shake")]
    [Tooltip("Duração padrão do shake em segundos")]
    [SerializeField] private float defaultShakeDuration = 0.3f;
    [Tooltip("Magnitude máxima do shake em unidades de mundo")]
    [SerializeField] private float maxShakeMagnitude = 0.4f;

    // ─── Estado interno ───────────────────────────────────────────────────────

    private Vector3 velocity = Vector3.zero;
    private Vector3 shakeOffset = Vector3.zero;
    private bool isShaking = false;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (autoFindPlayer)
        {
            var p = GameObject.FindGameObjectWithTag("PlayerMain");
            if (p != null) target = p.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Lookahead suave baseado na velocidade do target
        Vector3 lookahead = Vector3.zero;
        if (target.TryGetComponent<Rigidbody2D>(out var rb))
            lookahead = (Vector3)(rb.velocity.normalized * lookaheadDist);

        Vector3 desiredPos = target.position + offset + lookahead;
        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position, desiredPos,
            ref velocity, smoothTime);

        transform.position = smoothPos + shakeOffset;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Aciona o camera shake.
    /// magnitude: 0–1 (escalado pelo maxShakeMagnitude).
    /// Chame com magnitude proporcional ao tamanho da criatura que atacou.
    /// </summary>
    public void Shake(float magnitude = 0.5f, float duration = -1f)
    {
        if (isShaking) StopAllCoroutines();
        float dur = duration < 0f ? defaultShakeDuration : duration;
        StartCoroutine(ShakeRoutine(magnitude, dur));
    }

    // ─── Coroutine interna ────────────────────────────────────────────────────

    private IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float fade = 1f - (elapsed / duration);          // atenua ao final
            float mag = maxShakeMagnitude * magnitude * fade;
            shakeOffset = Random.insideUnitSphere * mag;
            shakeOffset.z = 0f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking   = false;
    }

    // ─── Singleton leve ───────────────────────────────────────────────────────

    public static Devoniancamera Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
    }
}