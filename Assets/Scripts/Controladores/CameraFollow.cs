using UnityEngine;

/// <summary>
/// CAmera Man
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // ────────Variaveis────────────────────────────────────────────
    public Transform target; // Alvo
    public float smoothSpeed = 0.125f; // Velocidade da camera
    public Vector3 offset;

    // ──────────Define limites─────────────────────────────────────
    public Vector2 minBounds; // Limite inferior esquerdo
    public Vector2 maxBounds; // Limite superior direito

    private float camHeight;
    private float camWidth;

    void Start()
    {
        Camera cam = Camera.main;

        // Altura da camera (ortográfica)
        camHeight = cam.orthographicSize;

        // Largura baseada no aspect ratio
        camWidth = camHeight * cam.aspect;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Posição desejada
        Vector3 PDesejada = target.position + offset;

        // Clamp considerando o tamanho da câmera
        float clampedX = Mathf.Clamp(PDesejada.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(PDesejada.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        Vector3 posFinal = new Vector3(clampedX, clampedY, transform.position.z);

        // Suavização
        Vector3 Psuave = Vector3.Lerp(transform.position, posFinal, smoothSpeed);

        transform.position = Psuave;
    }
}