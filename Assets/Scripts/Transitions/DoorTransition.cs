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
        if (playerInside && TransitionScene.Instance.active == false)
        {
            GameManager.Instance.nextSpawnID = targetSpawnID;

            // Chama a transição
            if (TransitionScene.Instance != null) { 
            if (typeofMove == TypeofMove.LefttoRight)
                TransitionScene.Instance.LoadSceneWithTransition(targetScene, TransitionScene.TransitionDirection.LeftToRight);
            if (typeofMove == TypeofMove.RighttoLeft)
                TransitionScene.Instance.LoadSceneWithTransition(targetScene, TransitionScene.TransitionDirection.RightToLeft);
        }
        else
            SceneManager.LoadScene(targetScene); // fallback
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain")) 
        {
            playerInside = true;
           WaterModeDefense invencible = other.GetComponentInParent<WaterModeDefense>();
            if (invencible != null) invencible.invenciTimer = 9999999;

        }
            

    }
}
