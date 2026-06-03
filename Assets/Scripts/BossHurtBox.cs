using UnityEngine;

public class BossHurtBox : MonoBehaviour
{
    public DunkeosteusBoss boss;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttack"))
        {
            collision.GetComponentInParent<DamageTaker>().invenciTimer = 1f;
            boss.TakeDamage(5);
        }
    }
}