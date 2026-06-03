using UnityEngine;

public class WaterDetector : MonoBehaviour
{
    [Header("Componentes")]
    public PlayerLand landMovement;
    public WaterMovement waterMovement;

    private int waterCount = 0;
    public bool IsInWater { get; private set; } = false;

    void Awake()
    {
        // Garante estado inicial correto
        landMovement.enabled = true;
        waterMovement.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Water")) return;

        waterCount++;
        if (waterCount == 1) EnterWater();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Water")) return;

        waterCount--;
        if (waterCount == 0) ExitWater();
    }

    void EnterWater()
    {
        IsInWater = true;
        landMovement.enabled = false;
        waterMovement.enabled = true;
        Debug.Log("Entrou na água");
    }

    void ExitWater()
    {
        IsInWater = false;
        landMovement.enabled = true;
        waterMovement.enabled = false;
        Debug.Log("Saiu da água");
    }
}
