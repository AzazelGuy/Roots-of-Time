using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    public Image LifeBar;
    public DunkeosteusBoss dunkeosteusBoss;
    // Update is called once per frame
    void Update()
    {
        float max = 50;
        LifeBar.fillAmount = max > 0 ? (float)dunkeosteusBoss.health / max : 0f;

    }

}
