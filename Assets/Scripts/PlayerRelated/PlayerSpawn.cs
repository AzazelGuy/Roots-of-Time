using UnityEngine;
using System.Collections;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {

        Debug.Log($"PlayerSpawn.Start | IsLoadingData={GameManager.Instance?.GetComponent<PlayerDataManager>()?.IsLoadingData}");

        if (GameManager.Instance == null) return;
        var dataManager = GameManager.Instance.GetComponent<PlayerDataManager>();
        if (dataManager != null && dataManager.IsLoadingData)
        {
            Debug.Log("PlayerSpawn: abortou por IsLoadingData");
            return;
        }

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