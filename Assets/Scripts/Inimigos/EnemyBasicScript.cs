using UnityEngine;

public class EnemyBasicScript : MonoBehaviour
{
    public float spd = 5f; //Velocidade do Inimigo

    private Rigidbody2D rb; //Rigidbody
    private SpriteRenderer spr; //Sprite Renderer

    public LayerMask GroundMask; //Camada do Chão

    private int dir = 1; //Direção
    private Vector2 direction = Vector2.right; //Direção 2: O inimigo é outro feat Vector2

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //preciso repetir isso?
        spr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        
        rb.velocity = new Vector2(dir * spd, rb.velocity.y); //Aplicar velocidade
        var hit = Physics2D.Raycast(transform.position, direction, 1f, GroundMask); //Conferir obstaculos
        
        if (hit.collider != null)
        {
            Virar();
        }

    }

    private void Virar() //Virar o Inimigo ao bater em obstaculo
    {
        dir *= -1;

        if (dir == 1)
        {
            spr.flipX = false;
            direction = Vector2.right;
        }
        else
        {
            spr.flipX = true;
            direction = Vector2.left;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy")) //Virar se ver outro inimigo
        {
            Virar();
        }
    }

}
