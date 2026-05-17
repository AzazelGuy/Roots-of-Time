using UnityEngine;

/// <summary>
/// Move um objeto em uma direção até ultrapassar a posição de um objeto alvo,
/// depois retorna à posição inicial. Repete por um tempo configurável.
/// </summary>
public class OscillatingMover : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O objeto que serve como ponto de virada (o script verifica quando o mover passa por ele)")]
    public Transform targetObject;

    [Header("Movimento")]
    [Tooltip("Direção do movimento (será normalizada automaticamente)")]
    public Vector3 moveDirection = Vector3.right;

    [Tooltip("Velocidade do movimento em unidades/segundo")]
    public float moveSpeed = 3f;

    [Header("Tempo")]
    [Tooltip("Duração total da oscilação em segundos (padrão: 180 = 3 minutos)")]
    public float duration = 180f;

    // --- Estado interno ---
    private Vector3 _startPosition;
    private Vector3 _normalizedDir;
    private float _elapsedTime;
    private bool _isRunning;
    private bool _goingForward = true;

    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning($"[OscillatingMover] '{name}': nenhum targetObject atribuído. Script desativado.");
            enabled = false;
            return;
        }

        _startPosition    = transform.position;
        _normalizedDir    = moveDirection.normalized;
        _elapsedTime      = 0f;
        _isRunning        = true;
        _goingForward     = true;
    }

    private void Update()
    {
        if (!_isRunning) return;

        // Contagem de tempo
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= duration)
        {
            StopAndReset();
            return;
        }

        if (_goingForward)
            MoveForward();
        else
            MoveBack();
    }

    // ---------------------------------------------------------------

    private void MoveForward()
    {
        transform.position += _normalizedDir * (moveSpeed * Time.deltaTime);

        // Verifica se "passou" o targetObject projetando ambos na direção do movimento
        float selfProj   = Vector3.Dot(transform.position,  _normalizedDir);
        float targetProj = Vector3.Dot(targetObject.position, _normalizedDir);

        if (selfProj >= targetProj)
        {
            _goingForward = false;          // inverte: vai voltar
        }
    }

    private void MoveBack()
    {
        transform.position -= _normalizedDir * (moveSpeed * Time.deltaTime);

        // Verifica se chegou de volta à posição inicial
        float selfProj  = Vector3.Dot(transform.position, _normalizedDir);
        float startProj = Vector3.Dot(_startPosition,      _normalizedDir);

        if (selfProj <= startProj)
        {
            transform.position = _startPosition;   // snap exato
            _goingForward = true;                   // inverte: vai pra frente de novo
        }
    }

    private void StopAndReset()
    {
        _isRunning         = false;
        transform.position = _startPosition;
        Debug.Log($"[OscillatingMover] '{name}': tempo encerrado após {duration}s. Objeto retornado à posição inicial.");
    }

    // ---------------------------------------------------------------
    // Helpers públicos (úteis para chamar via código ou UnityEvents)

    /// <summary>Reinicia o ciclo do zero.</summary>
    public void Restart()
    {
        _startPosition = transform.position;
        _elapsedTime   = 0f;
        _goingForward  = true;
        _isRunning     = true;
    }

    /// <summary>Pausa o movimento sem resetar o tempo.</summary>
    public void Pause()  => _isRunning = false;

    /// <summary>Retoma o movimento de onde parou.</summary>
    public void Resume() => _isRunning = true;

    // ---------------------------------------------------------------
    // Gizmos para facilitar a visualização no Editor
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? _startPosition : transform.position;
        Vector3 dir    = moveDirection.normalized;

        // Linha indicando a direção
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, dir * 5f);

        // Posição inicial
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, 0.15f);

        // Posição do alvo
        if (targetObject != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetObject.position, 0.2f);
            Gizmos.DrawLine(origin, targetObject.position);
        }
    }
#endif
}
