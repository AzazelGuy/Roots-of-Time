using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instanceg;
    public bool onPause = false;

    public RectTransform pauseMenui;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onPause = false;
        pauseMenui.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var cenaAtual = SceneManager.GetActiveScene();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cenaAtual.name == "MainMenu") return;

            onPause = !onPause;
            pauseMenui.gameObject.SetActive(onPause);
            //AudioManager.instance.SetPaused(onPause);
            Time.timeScale = onPause ? 0f : 1f;

        }
    }

    public void Unpause()
    {
        if (!onPause) return;


        onPause = false;
        //AudioManager.instance.SetPaused(onPause);
        pauseMenui.gameObject.SetActive(false);
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
    }

    IEnumerator RelockCursorNextFrame()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MenuReturn()
    {
        if (!onPause) return;
        onPause = false;
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        if (!onPause) return;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        Application.Quit();
    }

    public void SaveGame()
    {
        GameManager.Instance.LetsSave();
    }

    public void LoadGame()
    {
        GameManager.Instance.LetsLoad();
    }
}
