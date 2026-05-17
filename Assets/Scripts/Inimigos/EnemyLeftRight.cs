using UnityEngine;

public class PatrulhaInimigo : MonoBehaviour
{
    public float velocidade = 5f;
    private bool indoParaDireita = true;
    private Rigidbody2D rb;

    void Start()
    {
        // Obtém a referência do Rigidbody2D anexado ao inimigo
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Define a velocidade constante no eixo X, mantendo a velocidade atual do eixo Y
        if (indoParaDireita)
        {
            rb.velocity = new Vector2(velocidade, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-velocidade, rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        // Quando colidir com qualquer coisa, inverte a direção
        InverterDirecao();
    }

    void InverterDirecao()
    {
        indoParaDireita = !indoParaDireita;

        // Inverte a escala no eixo X para o sprite "olhar" para o outro lado
        Vector3 escalaLocal = transform.localScale;
        escalaLocal.x *= -1;
        transform.localScale = escalaLocal;
    }
}