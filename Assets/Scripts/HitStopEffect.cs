using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopEffect : MonoBehaviour
{
    public static HitStopEffect Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopTime(float duration)
    {
        StartCoroutine(DoStop(duration));
    }

    IEnumerator DoStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
