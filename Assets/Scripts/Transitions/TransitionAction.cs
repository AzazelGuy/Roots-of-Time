using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionAction : MonoBehaviour
{
    public string targetScene;

    public enum TypeofMove
    {
        LefttoRight,
        RighttoLeft
    }
    public TypeofMove typeofMove;
    public void Go()
    {
        if (TransitionScene.Instance.active == false)
        {
            GameManager.Instance.nextSpawnID = null;

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
}
