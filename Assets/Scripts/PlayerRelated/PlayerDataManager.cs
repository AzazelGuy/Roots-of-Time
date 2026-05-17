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
            SceneManager.LoadScene(loadedData.SceneName);
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
        // CORREÇÃO PRINCIPAL:
        // WaitForEndOfFrame não é confiável em builds — a Unity pode não ter
        // terminado de instanciar e inicializar todos os objetos da cena ainda.
        //
        // A solução correta é esperar ativamente até o player existir,
        // com um timeout para não travar o jogo se algo der errado.

        float elapsed = 0f;
        GameObject player = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("PlayerMain");

            if (player != null) break;

            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= PLAYER_SEARCH_TIMEOUT)
            {
                Debug.LogError($"ApplyLoadedData: PlayerMain não apareceu em {PLAYER_SEARCH_TIMEOUT}s. Abortando.");
                loadedData = null;
                yield break;
            }

            // Espera um frame e tenta de novo
            yield return null;
        }

        // Garante que o player já rodou pelo menos um frame completo
        // antes de mover (evita conflito com scripts de spawn/posicionamento)
        yield return new WaitForEndOfFrame();

        player.transform.position = new Vector3(
            loadedData.Position[0],
            loadedData.Position[1],
            loadedData.Position[2]
        );

        GameManager.Instance.PlayerHealth    = loadedData.health;
        GameManager.Instance.PlayerHealthMax = loadedData.healthmax;

        Debug.Log($"Dados aplicados: HP {loadedData.health}/{loadedData.healthmax}, pos {player.transform.position}");

        loadedData = null;
    }
}
