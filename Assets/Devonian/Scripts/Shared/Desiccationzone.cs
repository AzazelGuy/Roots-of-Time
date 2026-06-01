using UnityEngine;

/// <summary>
/// DesiccationZone � P4
/// Zonas de terra/lama que modificam a taxa de desseca��o do TiktaalikController.
/// Coloque em triggers sobre tilemaps de terra seca, rochas ou deserto.
///
/// Funciona enviando um multiplicador ao TiktaalikController.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Desiccationzone : MonoBehaviour
{
    [Header("Configura��o")]
    [Tooltip("Multiplicador da taxa de desseca��o nesta zona (1=normal, 2=dobro, 0.5=metade)")]
    [SerializeField] private float drainMultiplier = 2f;

    [Tooltip("Efeito visual de calor/poeira (opcional)")]
    [SerializeField] private ParticleSystem heatParticles;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (heatParticles != null) heatParticles.Stop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerMain")) return;

        if (other.TryGetComponent<TiktaalikMoisture>(out var player))
            player.SetDesiccationMultiplier(drainMultiplier);

        if (heatParticles != null) heatParticles.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerMain")) return;

        if (other.TryGetComponent<TiktaalikMoisture>(out var player))
            player.SetDesiccationMultiplier(1f);

        if (heatParticles != null) heatParticles.Stop();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.2f);
        var col = GetComponent<Collider2D>();
        if (col != null) Gizmos.DrawCube(transform.position, col.bounds.size);
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.7f);
        if (col != null) Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
}