using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia a umidade (moisture) do Tiktaalik e aplica dano por dessecação.
/// Unity 2020.
/// Depende de: CreatureDataEntity, WaterDetector, GameManager
/// </summary>
public class TiktaalikMoisture : MonoBehaviour
{
    // ─── Referências ──────────────────────────────────────────────────────────

    [Header("Dados Biológicos")]
    [SerializeField] private CreatureData data;

    [Header("UI")]
    [SerializeField] private Slider moistureBar;

    // ─── Estado interno ───────────────────────────────────────────────────────

    private WaterDetector waterDetector;

    private float moisture;
    private float desiccationMultiplier = 1f;

    public float Moisture        => moisture;
    public float MoisturePercent => data != null ? moisture / data.moistureMax : 0f;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        waterDetector = GetComponent<WaterDetector>();

        if (data != null)
            moisture = data.moistureMax;
    }

    private void Update()
    {
        TickDesiccation();
        UpdateUI();
    }

    // ─── Dessecação ───────────────────────────────────────────────────────────

    private void TickDesiccation()
    {
        if (data == null) return;

        bool inWater = waterDetector != null && waterDetector.IsInWater;

        if (inWater)
        {
            moisture = Mathf.Min(
                moisture + data.moistureDrainRate * Time.deltaTime,
                data.moistureMax);
            return;
        }

        moisture -= data.moistureDrainRate * desiccationMultiplier * Time.deltaTime;
        moisture  = Mathf.Max(moisture, 0f);

        if (moisture <= 0f && GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHealth -=
                Mathf.RoundToInt(data.desiccationDamageRate * Time.deltaTime);
        }
    }

    /// <summary>Multiplica a taxa de perda de umidade (ex.: terreno arenoso = 2x).</summary>
    public void SetDesiccationMultiplier(float multiplier)
        => desiccationMultiplier = Mathf.Max(0f, multiplier);

    // ─── UI ───────────────────────────────────────────────────────────────────

    private void UpdateUI()
    {
        if (moistureBar != null && data != null)
            moistureBar.value = MoisturePercent;
    }
}
