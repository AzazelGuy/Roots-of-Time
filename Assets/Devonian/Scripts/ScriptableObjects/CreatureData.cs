using UnityEngine;

/// <summary>
/// ScriptableObject que define os atributos biológicos de uma espécie.
/// Crie um asset para cada espécie via: Assets > Create > Devonian > Creature Data
/// </summary>
[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Devonian/Creature Data")]
public class CreatureData : ScriptableObject
{
    [Header("Identificação")]
    public string speciesName;
    [TextArea(2, 4)]
    public string description;

    [Header("Locomoção")]
    public float maxSpeedWater = 5f;
    public float maxSpeedLand = 2f;
    public float accelerationWater = 10f;
    public float accelerationLand = 4f;
    public float rotationSpeed = 180f;   // graus/s
    public float linearDragWater = 1f;
    public float linearDragLand = 8f;
    public float gravityScaleWater = 0f;
    public float gravityScaleLand = 2f;

    [Header("Física de Inércia (apenas player)")]
    [Tooltip("Quão rápido o bicho vira no eixo (0 = instantâneo, 1 = muito lento)")]
    [Range(0f, 1f)]
    public float inertiaDamping = 0.85f;

    [Header("Atributos Vitais")]
    public int maxHealth = 10;
    public float metabolicRate = 1f;     // unidades de fome/segundo
    public float hungerMax = 100f;

    [Header("Combate")]
    public float biteForce = 5f;
    public float biteRange = 1.5f;
    public float attackCooldown = 1.5f;   // segundos
    public float detectionRange = 8f;

    [Header("Dessecação (player)")]
    public float moistureMax = 100f;
    public float moistureDrainRate = 5f;    // por segundo fora d'água
    public float desiccationDamageRate = 2f; // HP por segundo com moisture = 0

    [Header("Prefab")]
    public GameObject prefab;
}