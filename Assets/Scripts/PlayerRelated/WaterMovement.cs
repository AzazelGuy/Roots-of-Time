using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaterMovement : MonoBehaviour
{
    [Header("Configurações")]
    public float speed = 6f;
    [SerializeField] private Transform spriteTransform;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // Remove gravidade e zera velocidade ao entrar na água
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    void OnDisable()
    {
        // Zera velocidade ao sair da água
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x < 0)
        {
            spriteTransform.localScale = new Vector3(1f, -1f, 1f);
        }else
        {
            spriteTransform.localScale = new Vector3(1f, 1f, 1f);
        }
            // 8 direções: aceita qualquer combinação de X e Y
            moveInput = new Vector2(x, y).normalized;
    }

    void Move()
    {
        rb.velocity = moveInput * speed;

        if (moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            rb.rotation = Mathf.Lerp(rb.rotation,angle, 0.5f);
        }

    }
}
