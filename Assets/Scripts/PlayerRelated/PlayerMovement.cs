using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f; // velocidade horizontal do jogador

    [Header("Jump")]
    [SerializeField] float jumpForce = 12f; // força do pulo normal
    [SerializeField] float wallJumpForce = 12; // força horizontal do pulo na parede

    float CayoteTime = 0f; // tempo restante do "Coyote Time" (tempo extra para pular após sair do chão)
    [SerializeField] float CayoteTimeMax = 0f; // valor máximo do Coyote Time

    float jumpBlockTimer = 0f; // timer que bloqueia pulo temporariamente
    [SerializeField] float jumpBlockTime = 0.1f; // duração do bloqueio de pulo
    float jumpBufferTimer = 0f; // timer que faz o buffer do pulo temporariamente
    [SerializeField] float jumpBufferTime = 0.1f; // duração do buffer de pulo

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck; // ponto de onde o raycast para checar chão sai
    [SerializeField] float groundDistance = 0.2f; // distância do raycast para detectar o chão
    [SerializeField] LayerMask groundLayer; // layer considerada como chão
    float groundDisableTimer = 0f; // timer que impede o personagem de ser considerado no chão logo após pular
    [SerializeField] float groundDisableTime = 0.1f; // duração desse bloqueio de detecção de chão
    [SerializeField] PhysicsMaterial2D noFriction; // material sem fricção (para deslizar)
    [SerializeField] PhysicsMaterial2D FullFriction; // material com fricção total (para parar rápido)
    [SerializeField] bool inWater;
    [Header("WallCheck")]
    [SerializeField] Transform wallCheck; // ponto de raycast para checar parede
    [SerializeField] float wallDistance = 0.2f; // distância para detectar parede
    [SerializeField] float wallJumpBlockTime = 0.2f;
    float wallJumpBlockTimer = 0f;
    [SerializeField] float wallSlideSpeed = 2;

    float moveBlockTimer = 0f; // timer que bloqueia movimento (usado no wall jump)
    [SerializeField] float moveBlockTime = 0.1f; // duração do bloqueio de movimento

    [HideInInspector]public bool isHidden = false;
    Rigidbody2D rb; // referência ao Rigidbody do jogador
    CapsuleCollider2D col; // collider usado para física
    SpriteRenderer spr; // sprite do jogador

    float xInput; // input horizontal do jogador
    int dir = 1; // direção atual que o jogador está olhando (1 = direita, -1 = esquerda)

    bool isGrounded; // indica se o jogador está no chão
    bool Snap = false; // sistema de snap para "colar" no chão ao descer rampas

    public bool moving = true;
    bool onWall; // indica se o jogador está encostando numa parede

    float snapBlockTimer = 0f; // bloqueia temporariamente o snap após pular
    [SerializeField] float snapBlockTime = 0.1f; // duração do bloqueio de snap

    float WaitStartTimer = 0f;
    [SerializeField] float WaitStartTime = .5f;
    public TrilobitaAnimatioController TrilobitaAnimatioController;
    void Awake()
    {
        // pega referências dos componentes necessários
        rb = GetComponent<Rigidbody2D>();
        col = GetComponentInChildren<CapsuleCollider2D>();
        spr = GetComponentInChildren<SpriteRenderer>();
        WaitStartTimer = WaitStartTime;
        moving = false;
    }

    void Update()
    {
        // checagens físicas
        CheckGround();
        CheckWall();

        // pega input horizontal e arredonda para -1, 0 ou 1
        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));

        // diminui os timers a cada frame
        WaitStartTime -= Time.deltaTime;
        snapBlockTimer -= Time.deltaTime;
        moveBlockTimer -= Time.deltaTime;
        jumpBlockTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        wallJumpBlockTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;

        if (WaitStartTime <= 0) moving=true;
        // troca o material do collider dependendo se está parado no chão
        if (xInput == 0 && isGrounded)
        {
            // quando parado no chão, usa fricção total
            col.sharedMaterial = FullFriction;
        }
        else
        {
            // quando se movendo ou no ar, remove fricção
            col.sharedMaterial = noFriction;
        }

        // lógica quando está no chão
        if (isGrounded)
        {
            // reseta o Coyote Time
            CayoteTime = CayoteTimeMax;

            // se não está apertando pulo
            if (!Input.GetButtonDown("Jump") && !inWater)
            {
                // gravidade menor para manter movimento mais suave
                rb.gravityScale = 3f;

                // ativa snap para manter colado no chão
                Snap = true;
            }
        }
        else
        {
            // quando está no ar

            if (xInput != 0 && onWall && !Input.GetButtonDown("Jump") && wallJumpBlockTimer <= 0)
            {
                rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Max(rb.velocity.y, -wallSlideSpeed)
                );
            }
            else if(!Input.GetButtonDown("Jump") && !inWater)
            {
                // aumenta a gravidade para cair mais rápido
                rb.gravityScale = 10f;
            }

                // diminui o tempo restante do Coyote Time
                CayoteTime -= Time.deltaTime;

            // desativa snap quando não pode mais pular
            if (CayoteTime <= 0 || jumpBufferTimer <= 0)
            {
                Snap = false;
            }
        }

        // controle de direção do sprite
        if (xInput > 0 && moveBlockTimer <= 0)
        {
            dir = 1;
            spr.transform.localScale = new Vector3(1, spr.transform.localScale.y);
        }
        else if (xInput < 0  && moveBlockTimer <= 0)
        {
            dir = -1;
            spr.transform.localScale = new Vector3(-1, spr.transform.localScale.y);
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }

        
    }

    void FixedUpdate()
    {
        if (!moving) return;
        // aplica movimento horizontal
        Move();

        // lógica de pulo normal com Coyote Time, buffer e wall jump
        if (jumpBufferTimer > 0 && (CayoteTime > 0 || onWall) && jumpBlockTimer < 0)
        {
            jumpBufferTimer = 0;
            jumpBlockTimer = jumpBlockTime; // inicia bloqueio de pulo
            CayoteTime = 0; // cancela o Coyote Time
            Jump();
        }

        // lógica de snap para "colar" no chão ao descer rampas
        // só aplica o snap se:
        // - não houver input de pulo armazenado (buffer)
        // - o snap não estiver bloqueado temporariamente
        if (Snap && jumpBufferTimer <= 0 && snapBlockTimer <= 0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Lerp(rb.velocity.y, -(rb.gravityScale), 0.5f)
            );
        }
    }

    void CheckGround()
    {
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            -Vector2.up,
            groundDistance,
            groundLayer
        );

        isGrounded = hit;
        TrilobitaAnimatioController.Grounded = isGrounded;
    }

    void CheckWall()
    {
        // raycast na direção que o jogador está olhando
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * dir,
            wallDistance,
            groundLayer
        );

        if (hit)
        {
            // debug para mostrar qual objeto foi atingido
            Debug.Log("Hit: " + hit.collider.name);
            onWall = true;
        }
        else
        {
            onWall = false;
        }
    }

    void Move()
    {
        // se o movimento estiver bloqueado (ex: wall jump), não move
        if (moveBlockTimer > 0) return;

        // aplica velocidade horizontal
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
        TrilobitaAnimatioController.xSpeed = rb.velocity.x;
        TrilobitaAnimatioController.ySpeed = rb.velocity.y;
    }

    void Jump()
    {
        groundDisableTimer = groundDisableTime;
        jumpBufferTimer = 0;

        TrilobitaAnimatioController.Jump();
        // desativa Coyote Time
        CayoteTime = -10;

        // desativa snap ao pular
        Snap = false;

        // bloqueia snap temporariamente
        snapBlockTimer = snapBlockTime;

        if (onWall && !isGrounded && !inWater) //lógica wall jump
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            // bloqueia movimento horizontal temporariamente
            moveBlockTimer = moveBlockTime;

            // força extra vertical
            rb.AddForce(Vector2.up * (jumpForce), ForceMode2D.Impulse);

            // força horizontal oposta à parede
            rb.AddForce(Vector2.right * (-dir * wallJumpForce), ForceMode2D.Impulse);
            dir *=-1;
            spr.transform.localScale = new Vector3(dir, spr.transform.localScale.y);

            wallJumpBlockTimer = wallJumpBlockTime; //BLoqueia wall jump para deixar o jogador subir
            return;
        }
        else //pulo normal
        {
            // remove gravidade momentaneamente
            if (!inWater) rb.gravityScale = 0f;

            // reseta velocidade vertical antes do pulo
            rb.velocity = new Vector2(rb.velocity.x, 0);

            // aplica força vertical do pulo
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
    }

    void OnDrawGizmos()
    {
        // desenha raycast do chão no editor
        if (groundCheck != null)
        {
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);
        }

        // desenha raycast de parede no editor
        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left * wallDistance);
        }
    }
}