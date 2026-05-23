using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance;

    [Header("UI")]
    public RectTransform transitionImage;

    [Header("Config")]
    public float speed = 1500f;
    public float fadeSpeed = 2f;

    [Header("Cenas que NÃO salvam ao carregar")]
    public string[] noSaveScenes = { "MainMenu", "GameOver", "Credits" };

    public bool active = false;

    public enum TransitionDirection { LeftToRight, RightToLeft }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithTransition(string sceneName, TransitionDirection direction)
    {
        if (!active)
        {
            StartCoroutine(TransitionCoroutine(sceneName, direction));
            active = true;
        }
    }

    private bool ShouldSave(string sceneName)
    {
        foreach (var s in noSaveScenes)
            if (s == sceneName) return false;

        return true;
    }

    private IEnumerator TransitionCoroutine(string sceneName, TransitionDirection direction)
    {
        float imageWidth = transitionImage.rect.width;
        Vector3 cover = Vector3.zero;
        Vector3 start, exit;

        if (direction == TransitionDirection.LeftToRight)
        {
            start = new Vector3(-imageWidth, 0, 0);
            exit = new Vector3(imageWidth, 0, 0);
        }
        else
        {
            start = new Vector3(imageWidth, 0, 0);
            exit = new Vector3(-imageWidth, 0, 0);
        }

        transitionImage.anchoredPosition = start;

        float duration = 0.5f;
        float elapsed = 0f;

        // Entrada
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transitionImage.anchoredPosition = Vector3.Lerp(start, cover, EaseInOut(elapsed / duration));
            yield return null;
        }
        transitionImage.anchoredPosition = cover;

        // Carrega cena
        yield return SceneManager.LoadSceneAsync(sceneName);

        if (ShouldSave(sceneName))
        {
            // Espera PlayerSpawn terminar (tem 1 yield interno)
            yield return null;
            yield return null;

            GameManager.Instance?.LetsSave();
        }

        elapsed = 0f;

        // Saída
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transitionImage.anchoredPosition = Vector3.Lerp(cover, exit, EaseInOut(elapsed / duration));
            yield return null;
        }
        transitionImage.anchoredPosition = exit;

        active = false;
    }

    private float EaseInOut(float t) => t * t * (3f - 2f * t);
}