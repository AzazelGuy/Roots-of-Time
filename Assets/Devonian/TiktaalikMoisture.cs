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
    [SerializeField] private SpriteRenderer fill;
    [SerializeField] private float maxWidth = 2f;
    [SerializeField] private GameObject groupBar;

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
        SetMoisture();
    }

    // ─── Dessecação ───────────────────────────────────────────────────────────

    private void TickDesiccation()
    {
        if (data == null) return;

        bool inWater = waterDetector != null && waterDetector.IsInWater;

        if (inWater)
        {
            moisture = Mathf.Min(
                moisture + (data.moistureDrainRate * 3) * Time.deltaTime,
                data.moistureMax);

            groupBar.SetActive(false);
            return;
        }

        groupBar.SetActive(true);
        moisture -= data.moistureDrainRate * desiccationMultiplier * Time.deltaTime;
        moisture  = Mathf.Max(moisture, 0f);

        if (moisture <= 0f && GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHealth -= 5;
            moisture = data.moistureMax;
        }
    }

    /// <summary>Multiplica a taxa de perda de umidade (ex.: terreno arenoso = 2x).</summary>
    public void SetDesiccationMultiplier(float multiplier)
        => desiccationMultiplier = Mathf.Max(0f, multiplier);

    

    public void SetMoisture()
    {
        Vector2 size = fill.size;
        size.x = maxWidth * MoisturePercent;
        fill.size = size;
    }
}
