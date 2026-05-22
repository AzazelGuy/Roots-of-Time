using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string nextSpawnID;

    public int PlayerHealth    = 10;
    public int PlayerHealthMax = 10;

    public DialogueControler dialogueController;
    public DialoguePage[]    dialoguePages;
    public ParticleSystem    DeathParticle;

    // CORREÇÃO: flag evita chamar LoadScene mais de uma vez por morte
    // No editor dificilmente aparece, mas em builds o Update pode rodar
    // mais de um frame com HP <= 0 antes da cena trocar.
    private bool isGameOver = false;
    private PlayerDataManager _dataManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _dataManager = GetComponent<PlayerDataManager>(); // ✅ cacheia uma vez
        }
        else Destroy(gameObject);
    }

    public void LetsSave() => _dataManager?.SaveGame();
    public void LetsLoad() => _dataManager?.LoadGame();
    public void LoadDefaults()
    {
        _dataManager?.ClearData();
        isGameOver      = false;
        PlayerHealth    = 4;
        PlayerHealthMax = 4;
    }

    private void Update()
    {
        // Clamp sempre antes de checar game over
        PlayerHealth = Mathf.Clamp(PlayerHealth, 0, PlayerHealthMax);

        if (PlayerHealth <= 0 && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(HandleGameOver());
        }
    }

    private IEnumerator HandleGameOver()
    {
        // Desativa o player imediatamente
        var player = GameObject.FindGameObjectWithTag("PlayerMain");
        if (player != null) player.SetActive(false);

        // Opcional: toca partícula de morte, espera animação, etc.
        // if (DeathParticle != null) DeathParticle.Play();
        // yield return new WaitForSeconds(1f);

        yield return null; // garante pelo menos 1 frame antes de trocar de cena

        SceneManager.LoadScene("GameOver");
    }

    // Chame isso ao reiniciar o jogo (ex: na tela de Game Over)
    public void ResetGameOverFlag()
    {
        isGameOver = false;
        PlayerHealth = 1; // ou LoadDefaults()
    }
    public void ShowdevMenu()
    {
        // implementar depois
    }
}
