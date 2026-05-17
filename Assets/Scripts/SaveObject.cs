using UnityEngine;

public class SaveObject : MonoBehaviour
{
    private bool playerNearby = false;

    public void Save()
    {
        Debug.Log("Jogo Savo!");
        GameManager.Instance.LetsSave();
    }
}
