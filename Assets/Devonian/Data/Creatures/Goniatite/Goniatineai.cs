using UnityEngine;

public class Goniatineai : CreatureAI
{
    [Header("Configuração de Fuga")]
    [SerializeField] private float forcaDoJato = 30f;
    [Tooltip("Quanto menor, mais agressivamente ele foge. Ajuste aqui para balancear.")]
    [SerializeField] private float tempoRecargaJato = 2.5f;

    private float timerJato;

    protected override void Awake()
    {
        base.Awake();
        preyTags = new[] { "SmallPrey" };
        predatorTags = new[] { "PlayerMain", "Predator", "LargePredator" };
        ChangeState(CreatureState.Wander);
    }

    protected override void Update()
    {
        base.Update();
        if (timerJato > 0) timerJato -= Time.deltaTime;
    }

    protected override void OnFlee()
    {
        if (target == null) { ChangeState(CreatureState.Wander); return; }

        // Tenta usar o jato se estiver disponível
        if (timerJato <= 0f)
        {
            DispararJato();
            timerJato = tempoRecargaJato;
        }
        else
        {
            // Movimento normal de fuga
            MoveAway(target.position, data.maxSpeedWater);
            // Rotaciona normalmente para fugir
            LookAtDirection(target.position, true);
        }

        // Condição de encerramento
        if (Vector2.Distance(rb.position, target.position) > data.detectionRange * 2f || stateTimer <= 0f)
        {
            target = null;
            ChangeState(CreatureState.Wander);
        }
    }

    private void DispararJato()
    {
        Vector2 direcaoFuga = (rb.position - (Vector2)target.position).normalized;

        // --- A LÓGICA DO GIRO ---
        // Se a sprite está virada para a direita, a frente dele é o vetor (1,0).
        // Para fugir de costas (tentáculos para frente), giramos a criatura na direção da fuga.
        float angle = Mathf.Atan2(direcaoFuga.y, direcaoFuga.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Aplica o impulso na direção oposta ao predador
        rb.velocity = Vector2.zero;
        rb.AddForce(direcaoFuga * forcaDoJato, ForceMode2D.Impulse);

        // Opcional: Adicionar um efeito de partículas de bolhas aqui
    }

    private void LookAtDirection(Vector2 targetPos, bool away)
    {
        Vector2 dir = away ? (Vector2)transform.position - targetPos : targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 5f);
    }
}