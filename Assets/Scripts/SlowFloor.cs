using UnityEngine;

public class SlowFloor : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0 = para tudo, 1 = sem efeito")]
    public float slowMultiplier = 0.4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerMain")) return;
        var controller = other.GetComponentInParent<PlayerLand>();
        if (controller != null)
            controller.ApplySlow(slowMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerMain")) return;
        var controller = other.GetComponentInParent<PlayerLand>();
        if (controller != null)
            controller.RemoveSlow();
    }
}
