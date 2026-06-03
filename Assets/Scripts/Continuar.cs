using UnityEngine;
using UnityEngine.SceneManagement;

public class Continuar : MonoBehaviour
{

    public string targetScene;   
    public void VAI() {SceneManager.LoadScene(targetScene); }

}