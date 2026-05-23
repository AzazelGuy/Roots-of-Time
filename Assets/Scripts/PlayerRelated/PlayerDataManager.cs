using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de variáveis de um sistema de save.
/// Corrigido: race condition de ordem de execução em builds.
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public Transform PlayerTransform;
    public int health;
    public int healthmax;
    public string SceneName;
    public bool IsLoadingData { get; private set; } = false;
    // Timeout em segundos — evita loop infinito se o player nunca aparecer
    private const float PLAYER_SEARCH_TIMEOUT = 5f;

    private PlayerData loadedData;

    public void SaveGame()
    {
        // Busca o player na hora de salvar, não guarda referência antiga
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerMain");
        if (playerObj == null)
        {
            Debug.LogError("SaveGame: PlayerMain não encontrado!");
            return;
        }

        PlayerTransform = playerObj.transform;

        PlayerData playerData = new PlayerData();
        playerData.Position = new float[]
        {
            PlayerTransform.position.x,
            PlayerTransform.position.y,
            PlayerTransform.position.z
        };
        playerData.health    = GameManager.Instance.PlayerHealth;
        playerData.healthmax = GameManager.Instance.PlayerHealthMax;
        playerData.SceneName = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(playerData);
        string path = Application.persistentDataPath + "/playerData.json";
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"Jogo salvo em: {path}");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/playerData.json";
        if (!File.Exists(path))
        {
            Debug.Log("Arquivo de save não encontrado.");
            return;
        }

        string json = File.ReadAllText(path);
        loadedData = JsonUtility.FromJson<PlayerData>(json);

        if (!string.IsNullOrEmpty(loadedData.SceneName))
        {
            IsLoadingData = true; // ✅ avisa antes de trocar de cena
            SceneManager.LoadScene(loadedData.SceneName);
        }
        else
            Debug.LogError("SaveData sem SceneName!");
    }

    public void ClearData()
    {
        loadedData = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadedData == null) return;
        StartCoroutine(ApplyLoadedData());
    }

    private IEnumerator ApplyLoadedData()
    {
        float elapsed = 0f;
        GameObject player = null;

        yield return null; // espera 1 frame pós-sceneLoaded

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("PlayerMain");

            elapsed += Time.deltaTime;
            if (elapsed >= PLAYER_SEARCH_TIMEOUT)
            {
                Debug.LogError("PlayerMain não apareceu. Abortando load.");
                loadedData = null;
                IsLoadingData = false;
                yield break;
            }

            if (player == null) yield return null;
        }

        // Garante que o player já rodou Start() e Awake() completos
        yield return new WaitForEndOfFrame();

        Vector3 targetPos = new Vector3(
            loadedData.Position[0],
            loadedData.Position[1],
            loadedData.Position[2]
        );

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        Debug.Log($"ANTES de aplicar | player pos: {player.transform.position}");
        rb.position = targetPos;
        player.transform.position = targetPos;
        yield return null;
        Debug.Log($"DEPOIS de 1 frame | player pos: {player.transform.position}");
        yield return null;
        Debug.Log($"DEPOIS de 2 frames | player pos: {player.transform.position}");

        // NÃO use SetActive(false/true) — causa o colisor fantasma

        GameManager.Instance.PlayerHealth = loadedData.health;
        GameManager.Instance.PlayerHealthMax = loadedData.healthmax;

        Debug.Log($"Load aplicado: HP {loadedData.health}/{loadedData.healthmax} | Pos: {targetPos}");

        loadedData = null;
        IsLoadingData = false;
    }
}

