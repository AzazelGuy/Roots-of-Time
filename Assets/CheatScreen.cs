using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatScreen : MonoBehaviour
{
    public void Cambriano()
    {
        SceneManager.LoadScene("MapWater1");
    }

    public void Devoniano()
    {
        GameManager.Instance.PlayerHealthMax = 100;
        GameManager.Instance.PlayerHealth = 100;
        
        SceneManager.LoadScene("MapaDevoniano1");
    }
}
