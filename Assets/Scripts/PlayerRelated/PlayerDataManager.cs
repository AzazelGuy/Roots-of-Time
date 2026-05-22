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

        // Espera pelo menos 1 frame antes de começar a buscar
        yield return null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("PlayerMain");
            if (player != null) break;

            elapsed += Time.deltaTime; // ✅ deltaTime normal é mais seguro aqui
            if (elapsed >= PLAYER_SEARCH_TIMEOUT)
            {
                Debug.LogError("PlayerMain não apareceu. Abortando load.");
                loadedData = null;
                yield break;
            }

            yield return null;
        }

        // ✅ Aguarda o final do frame COM o player já encontrado
        yield return new WaitForEndOfFrame();

        // ✅ DEPOIS
        Vector3 targetPos = new Vector3(
            loadedData.Position[0],
            loadedData.Position[1],
            loadedData.Position[2]
        );

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // Desativa RB
        if (rb != null) rb.simulated = false;

        // Move o root
        player.transform.position = new Vector3(
            loadedData.Position[0],
            loadedData.Position[1],
            loadedData.Position[2]
        );

        // ✅ FORÇA propagação para todos os filhos imediatamente
        player.transform.hasChanged = false;
        foreach (Transform child in player.GetComponentsInChildren<Transform>())
        {
            child.hasChanged = false;
        }

        // ✅ Isso força o Unity a recalcular a matriz de todos os filhos agora
        player.SetActive(false);
        player.SetActive(true);  // reativa — isso reseta a hierarquia inteira

        // Reativa RB após o SetActive
        rb = player.GetComponent<Rigidbody2D>(); // busca de novo pois SetActive resetou
        if (rb != null)
        {
            rb.velocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated       = true;
        }

        GameManager.Instance.PlayerHealth    = loadedData.health;
        GameManager.Instance.PlayerHealthMax = loadedData.healthmax;

        Debug.Log($"Load aplicado: HP {loadedData.health}/{loadedData.healthmax}");
        loadedData = null;
        IsLoadingData = false; // ✅ libera spawns normais de novo
    }
}

