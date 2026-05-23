using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    public string targetScene;
    public string targetSpawnID;

    private bool playerInside;

    public enum TypeofMove
    {
        LefttoRight,
        RighttoLeft
    }
    public TypeofMove typeofMove;
    void Update()
    {
        if (!playerInside) return;

        //  Checa null ANTES de acessar .active
        if (TransitionScene.Instance != null)
        {
            if (TransitionScene.Instance.active) return; // já tem transição rolando

            GameManager.Instance.nextSpawnID = targetSpawnID;

            if (typeofMove == TypeofMove.LefttoRight)
                TransitionScene.Instance.LoadSceneWithTransition(targetScene, TransitionScene.TransitionDirection.LeftToRight);
            else
                TransitionScene.Instance.LoadSceneWithTransition(targetScene, TransitionScene.TransitionDirection.RightToLeft);
        }
        else
        {
            // Fallback se o TransitionScene não existir
            GameManager.Instance.nextSpawnID = targetSpawnID;
            SceneManager.LoadScene(targetScene);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract")) 
        {
            playerInside = true;
           WaterModeDefense invencible = other.GetComponentInParent<WaterModeDefense>();
            if (invencible != null) invencible.invenciTimer = 9999999;

        }
            

    }
}
