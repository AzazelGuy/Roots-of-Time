using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioClip Music;
    private void Awake()
    {
        GameManager.Instance.LoadDefaults();
        Random.InitState(System.DateTime.Now.GetHashCode());
    }

    public void Iniciar()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadGame()
    {
        GameManager.Instance.LetsLoad();
    }
    public void Sair()
    {
        Application.Quit();
    }

    public void Creditos()
    {
        SceneManager.LoadScene(1);
    }
    public void VoltarCreditos()
    {
        SceneManager.LoadScene(0);
    }
}
