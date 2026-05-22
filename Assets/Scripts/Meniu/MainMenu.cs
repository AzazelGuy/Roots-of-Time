using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [Header("Hard Reset")]
    [SerializeField] private KeyCode resetKey = KeyCode.Return; // Enter
    [SerializeField] private float holdDuration = 5f; // segundos segurando
    [SerializeField] AudioClip DestroySave;

    private float holdTimer = 0f;
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
        string path = Application.persistentDataPath + "/playerData.json";
        if (File.Exists(path))
            GameManager.Instance.LetsLoad();
        else Debug.Log("Save não encontrado!");
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

    private void Update()
    {
        if (Input.GetKey(resetKey))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                holdTimer = 0f;
                HardReset();
            }
        }
        else
        {
            holdTimer = 0f; // soltou antes do tempo, reseta
        }
    }

    private void HardReset()
    {
        //Apaga o arquivo de save
        string path = Application.persistentDataPath + "/playerData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save deletado.");
            AudioController.Instance.PlaySFX(DestroySave, 2f);
        }

        // Reseta o GameManager se já existir (DontDestroyOnLoad)
        if (GameManager.Instance != null)
            GameManager.Instance.LoadDefaults();

        Debug.Log("Hard reset concluído.");
        // Fica no menu — não faz nada a mais
    }
}
