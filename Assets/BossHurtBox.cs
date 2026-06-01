using UnityEngine;

public class BossHurtBox : MonoBehaviour
{
    public DunkeosteusBoss boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack"))
        {
            boss.TakeDamage(5);
        }
    }
}