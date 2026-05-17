using UnityEngine;

/// <summary>
/// Simula o nível do mar com subida e descida suave ao longo do tempo.
/// Anexe este script ao GameObject da sua água (sprite).
/// </summary>
public class SeaLevelController : MonoBehaviour
{
    [Header("Configuração do Nível")]
    [Tooltip("Posição Y mínima da água (nível mais baixo)")]
    public float minLevel = -2f;

    [Tooltip("Posição Y máxima da água (nível mais alto)")]
    public float maxLevel = 2f;

    [Tooltip("Posição Y inicial da água")]
    public float startLevel = 0f;

    [Header("Configuração do Movimento")]
    [Tooltip("Tempo (em segundos) para completar um ciclo completo de subida e descida")]
    public float cycleDuration = 10f;

    [Tooltip("Tipo de movimento da onda")]
    public WaveType waveType = WaveType.Sine;

    [Header("Variação Aleatória (opcional)")]
    [Tooltip("Adiciona variação aleatória à velocidade do ciclo")]
    public bool useRandomVariation = false;

    [Tooltip("Variação máxima aplicada ao cycleDuration")]
    [Range(0f, 5f)]
    public float randomVariationAmount = 1f;

    [Header("Eventos de Nível")]
    [Tooltip("Dispara evento quando a água está acima deste valor")]
    public bool useHighTideEvent = false;
    public float highTideThreshold = 1.5f;

    [Tooltip("Dispara evento quando a água está abaixo deste valor")]
    public bool useLowTideEvent = false;
    public float lowTideThreshold = -1.5f;

    // ─── Estado interno ───────────────────────────────────────────
    private float _timer = 0f;
    private float _currentCycleDuration;
    private bool _isHighTide = false;
    private bool _isLowTide = false;
    private Vector3 _initialPosition;

    // ─── Propriedades públicas (leitura) ──────────────────────────
    /// <summary>Nível Y atual da água (0 = mínimo, 1 = máximo).</summary>
    public float NormalizedLevel { get; private set; }

    /// <summary>Posição Y absoluta atual da água.</summary>
    public float CurrentLevel => transform.position.y;

    // ─── Delegates para eventos ───────────────────────────────────
    public delegate void TideEvent();
    public event TideEvent OnHighTide;
    public event TideEvent OnLowTide;

    public enum WaveType
    {
        Sine,           // Movimento suave senoidal
        Triangle,       // Subida/descida linear constante
        EaseInOut       // Mais lento nas extremidades, rápido no meio
    }

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _initialPosition = transform.position;
        _currentCycleDuration = cycleDuration;

        // Posiciona a água no nível inicial
        SetLevel(startLevel);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // Normaliza o timer dentro do ciclo (0 → 1)
        float t = (_timer % _currentCycleDuration) / _currentCycleDuration;

        // Calcula a posição normalizada conforme o tipo de onda
        NormalizedLevel = EvaluateWave(t);

        // Aplica o nível (interpolando entre min e max)
        float targetY = Mathf.Lerp(minLevel, maxLevel, NormalizedLevel);
        transform.position = new Vector3(
            _initialPosition.x,
            targetY,
            _initialPosition.z
        );

        // Verifica eventos de maré
        CheckTideEvents(targetY);

        // Reinicia o ciclo e aplica variação aleatória
        if (_timer >= _currentCycleDuration)
        {
            _timer = 0f;
            if (useRandomVariation)
            {
                _currentCycleDuration = Mathf.Max(
                    0.5f,
                    cycleDuration + Random.Range(-randomVariationAmount, randomVariationAmount)
                );
            }
        }
    }

    // ─── Cálculo da onda ──────────────────────────────────────────
    private float EvaluateWave(float t)
    {
        switch (waveType)
        {
            case WaveType.Sine:
                // Oscila suavemente entre 0 e 1
                return (Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f;

            case WaveType.Triangle:
                // Sobe linearmente até 0.5, desce linearmente até 1
                return t < 0.5f ? t * 2f : 1f - (t - 0.5f) * 2f;

            case WaveType.EaseInOut:
                // Cúbica suave (SmoothStep)
                float sine = (Mathf.Sin(t * Mathf.PI * 2f - Mathf.PI / 2f) + 1f) / 2f;
                return sine * sine * (3f - 2f * sine);

            default:
                return 0.5f;
        }
    }

    // ─── Eventos de maré ──────────────────────────────────────────
    private void CheckTideEvents(float currentY)
    {
        if (useHighTideEvent)
        {
            bool above = currentY >= highTideThreshold;
            if (above && !_isHighTide)
            {
                _isHighTide = true;
                OnHighTide?.Invoke();
                Debug.Log("[SeaLevel] Maré alta atingida!");
            }
            else if (!above)
            {
                _isHighTide = false;
            }
        }

        if (useLowTideEvent)
        {
            bool below = currentY <= lowTideThreshold;
            if (below && !_isLowTide)
            {
                _isLowTide = true;
                OnLowTide?.Invoke();
                Debug.Log("[SeaLevel] Maré baixa atingida!");
            }
            else if (!below)
            {
                _isLowTide = false;
            }
        }
    }

    // ─── Métodos públicos utilitários ─────────────────────────────

    /// <summary>Define o nível da água diretamente por posição Y.</summary>
    public void SetLevel(float yPosition)
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(yPosition, minLevel, maxLevel);
        transform.position = pos;
    }

    /// <summary>Pausa ou retoma a simulação.</summary>
    public void SetPaused(bool paused)
    {
        enabled = !paused;
    }

    /// <summary>Reinicia o ciclo do zero.</summary>
    public void ResetCycle()
    {
        _timer = 0f;
        _currentCycleDuration = cycleDuration;
    }

#if UNITY_EDITOR
    // Gizmos para visualizar os limites no Editor
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? _initialPosition : transform.position;

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);
        Gizmos.DrawCube(new Vector3(pos.x, maxLevel, pos.z), new Vector3(5f, 0.05f, 0.1f));

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);
        Gizmos.DrawCube(new Vector3(pos.x, minLevel, pos.z), new Vector3(5f, 0.05f, 0.1f));

        if (useHighTideEvent)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawCube(new Vector3(pos.x, highTideThreshold, pos.z), new Vector3(5f, 0.03f, 0.1f));
        }

        if (useLowTideEvent)
        {
            Gizmos.color = new Color(0f, 1f, 0.3f, 0.4f);
            Gizmos.DrawCube(new Vector3(pos.x, lowTideThreshold, pos.z), new Vector3(5f, 0.03f, 0.1f));
        }
    }
#endif
}