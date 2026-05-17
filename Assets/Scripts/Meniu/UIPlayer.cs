using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour
{
    public static UIPlayer Instance;
    public Image LifeBar;
    public Image Vignnet;
    
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

        Vignnet.color = new Color(1f,1f,1f,0f);
    }
    // Update is called once per frame
    void Update()
    {
        float max = GameManager.Instance.PlayerHealthMax;
        LifeBar.fillAmount = max > 0 ? (float)GameManager.Instance.PlayerHealth / max : 0f;
        if (SceneManager.GetActiveScene().name == "Menu" || SceneManager.GetActiveScene().name == "GameOver")
        {
            Destroy(gameObject);
        }
    }

}
