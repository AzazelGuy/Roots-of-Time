using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrilobitaAnimatioController : MonoBehaviour
{
    [Header ("Controlador de Animação")]
    public Animator animator;

    //Escondido no inspetor, variaveis para controlar animacao
    [HideInInspector] public float xSpeed;
    [HideInInspector] public float ySpeed;
    [HideInInspector] private bool isDefendind;
    [HideInInspector] private bool isJumping;
    [HideInInspector] public bool Grounded;
    public void Defend() //Defesa
    {
        if (!isDefendind) { isDefendind = true; return; }

        return;

    }

    public void Jump() // Pulo
    {
        if (!isJumping) { isJumping = true; return; }

        return;
    }

    private void Update()
    {
        if (isDefendind) { animator.SetTrigger("Defend"); isDefendind = false; }
        if (isJumping) { animator.SetTrigger("Jump"); isJumping = false; }
        animator.SetBool("Grounded", Grounded);
        animator.SetFloat("xSpeed", Mathf.Abs(xSpeed));

        if (Mathf.Abs(ySpeed) < 0.01f) ySpeed = 0;
        animator.SetFloat("ySpeed", ySpeed);
    }
}
