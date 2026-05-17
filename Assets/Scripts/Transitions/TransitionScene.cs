using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance;

    [Header("UI")]
    public RectTransform transitionImage;

    [Header("Config")]
    public float speed = 1500f;
    public float fadeSpeed = 2f;

    public bool active = false;

    public enum TransitionDirection
    {
        LeftToRight,
        RightToLeft
    }

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

    private IEnumerator TransitionCoroutine(string sceneName, TransitionDirection direction)
    {
        float imageWidth = transitionImage.rect.width;

        Vector3 start;
        Vector3 cover = Vector3.zero;
        Vector3 exit;

        // 🎯 Define direção
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

        float duration = 0.5f; // tempo da animação
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration; // agora vai de 0 → 1 garantido
            float eased = EaseInOut(t);

            transitionImage.anchoredPosition = Vector3.Lerp(start, cover, eased);

            yield return null;
        }

        // garante posição final exata
        transitionImage.anchoredPosition = cover;

        // 2️ CARREGA CENA
        yield return SceneManager.LoadSceneAsync(sceneName);
        if (!(SceneManager.GetActiveScene().name == "Menu" || 
            SceneManager.GetActiveScene().name == "Creditos" ||
            SceneManager.GetActiveScene().name == "GameOver" ||
            SceneManager.GetActiveScene().name == "FimPorEnquanto"))
        {
            if (GameManager.Instance != null) GameManager.Instance.LetsSave();
        }
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            float eased = EaseInOut(t);

            transitionImage.anchoredPosition = Vector3.Lerp(cover, exit, eased);

            yield return null;
        }

        transitionImage.anchoredPosition = exit;

        active = false;
    }

    // 🧠 easing bonitinho (smooth)
    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t); // SmoothStep
    }
}