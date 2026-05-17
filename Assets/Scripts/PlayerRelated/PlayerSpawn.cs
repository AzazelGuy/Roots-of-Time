using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null) return;

        string id = GameManager.Instance.nextSpawnID;
        if (string.IsNullOrEmpty(id)) return;

        SpawnPoint[] spawns =
            FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

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
