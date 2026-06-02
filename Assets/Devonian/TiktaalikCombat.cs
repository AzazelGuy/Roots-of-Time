using System.Collections;
using UnityEngine;

public class TiktaalikCombat : MonoBehaviour
{

    private Animator anim;
    private float attackCooldownTimer;
    [SerializeField ]private float attackCooldownTime;
    private bool isAttacking;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        attackCooldownTimer = attackCooldownTime;
    }

    private void Update()
    {
        
        attackCooldownTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && attackCooldownTimer <= 0f){
            anim.SetTrigger("AnimAttack");
            attackCooldownTimer = attackCooldownTime;
            isAttacking = true;
        }
    }

    public void ResetAttack()
    {
        isAttacking = false;
    }

}