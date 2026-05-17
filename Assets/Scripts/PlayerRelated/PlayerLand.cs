using UnityEngine;

public class PlayerLand : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float wallJumpForce = 12f;
    [SerializeField] float wallSlideSpeed = 2f;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] float coyoteTimeDuration = 0.1f;
    [SerializeField] float jumpBufferDuration = 0.1f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] PhysicsMaterial2D fullFriction;
    [SerializeField] bool inWater;

    [Header("Wall Check")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float wallDistance = 0.2f;
    [SerializeField] float wallJumpBlockDuration = 0.2f;

    // Timers
    float coyoteTimer;
    float jumpBufferTimer;
    float jumpBlockTimer;
    float jumpBlockDuration = 0.1f;
    float moveBlockTimer;
    float moveBlockDuration = 0.1f;
    float groundDisableTimer;
    float groundDisableDuration = 0.1f;
    float snapBlockTimer;
    float snapBlockDuration = 0.1f;
    float wallJumpBlockTimer;
    float startDelayTimer;
    [SerializeField] float startDelayDuration = 0.5f;

    Rigidbody2D rb;
    CapsuleCollider2D col;
    SpriteRenderer spr;

    float xInput;
    int facingDir = 1; // 1 = direita, -1 = esquerda

    bool isGrounded;
    bool onWall;
    bool snapToGround;
    public bool moving = false;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponentInChildren<CapsuleCollider2D>();
        spr = GetComponentInChildren<SpriteRenderer>();
        startDelayTimer = startDelayDuration;
    }

    void Update()
    {
        rb.rotation = 0;
        CheckGround();
        CheckWall();

        xInput = Mathf.Round(Input.GetAxisRaw("Horizontal"));

        TickTimers();

        if (startDelayTimer <= 0) moving = true;

        col.sharedMaterial = (xInput == 0 && isGrounded) ? fullFriction : noFriction;

        HandleGravity();
        UpdateFacing();

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferDuration;
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Move();

        bool canJump = jumpBufferTimer > 0 && (coyoteTimer > 0 || onWall) && jumpBlockTimer <= 0;
        if (canJump)
        {
            jumpBufferTimer = 0;
            jumpBlockTimer = jumpBlockDuration;
            coyoteTimer = 0;
            Jump();
        }

        if (snapToGround && jumpBufferTimer <= 0 && snapBlockTimer <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Lerp(rb.velocity.y, -rb.gravityScale, 0.5f));
        }
    }

    void TickTimers()
    {
        startDelayTimer    -= Time.deltaTime;
        snapBlockTimer     -= Time.deltaTime;
        moveBlockTimer     -= Time.deltaTime;
        jumpBlockTimer     -= Time.deltaTime;
        jumpBufferTimer    -= Time.deltaTime;
        wallJumpBlockTimer -= Time.deltaTime;
        groundDisableTimer -= Time.deltaTime;

        if (!isGrounded)
            coyoteTimer -= Time.deltaTime;
    }

    void HandleGravity()
    {
        if (isGrounded)
        {
            coyoteTimer  = coyoteTimeDuration;
            snapToGround = true;
            if (!inWater) rb.gravityScale = 3f;
            return;
        }

        bool wallSliding = xInput != 0 && onWall && wallJumpBlockTimer <= 0;
        if (wallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else if (!inWater)
        {
            rb.gravityScale = 10f;
        }

        if (coyoteTimer <= 0)
            snapToGround = false;
    }

    void UpdateFacing()
    {
        if (moveBlockTimer > 0 || xInput == 0) return;

        facingDir = xInput > 0 ? 1 : -1;
        spr.transform.localScale = new Vector3(facingDir, spr.transform.localScale.y, 1f);
    }

    void CheckGround()
    {
        if (groundDisableTimer > 0)
        {
            isGrounded = false;
            return;
        }

        var hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance, groundLayer);
        isGrounded = hit;
    }

    void CheckWall()
    {
        var hit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallDistance, groundLayer);
        onWall = hit;
    }

    void Move()
    {
        if (moveBlockTimer > 0) return;
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        groundDisableTimer = groundDisableDuration;
        coyoteTimer        = -10f;
        snapToGround       = false;
        snapBlockTimer     = snapBlockDuration;

        if (onWall && !isGrounded && !inWater)
        {
            WallJump();
            return;
        }

        // Pulo normal
        if (!inWater) rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        moveBlockTimer     = moveBlockDuration;
        wallJumpBlockTimer = wallJumpBlockDuration;

        rb.AddForce(Vector2.up          * jumpForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right       * (-facingDir * wallJumpForce), ForceMode2D.Impulse);

        facingDir *= -1;
        spr.transform.localScale = new Vector3(facingDir, spr.transform.localScale.y, 1f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundDistance);

        if (wallCheck != null)
        {
            Gizmos.DrawRay(wallCheck.position, Vector2.right * wallDistance);
            Gizmos.DrawRay(wallCheck.position, Vector2.left  * wallDistance);
        }
    }
#endif
}