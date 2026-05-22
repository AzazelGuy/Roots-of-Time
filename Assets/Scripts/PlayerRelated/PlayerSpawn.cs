using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null) return;

        // ✅ Se tem um load em andamento, o ApplyLoadedData cuida da posição
        var dataManager = GameManager.Instance.GetComponent<PlayerDataManager>();
        if (dataManager != null && dataManager.IsLoadingData) return;

        string id = GameManager.Instance.nextSpawnID;
        if (string.IsNullOrEmpty(id)) return;

        SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in spawns)
        {
            if (sp.spawnID == id)
            {
                transform.position = sp.transform.position;
                break;
            }
        }
    }
}