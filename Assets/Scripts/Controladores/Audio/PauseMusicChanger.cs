using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMusicChanger : MonoBehaviour
{
    [Header("Música do Menu de Pause")]
    [SerializeField] private AudioClip pauseMenuClip;

    [Header("Fade")]
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;

    public void OnPauseOpen() //Salva a musica e toca a do pause
    {
        if (pauseMenuClip == null)
        {
            Debug.LogWarning("[PauseMusicChanger] Nenhum AudioClip de pause atribuído.");
            return;
        }

        //Aqui ele confere se o objeto singleton do AUDIOCONTROLLER existe, se sim, tocar musica
        AudioController.Instance?.SaveAndPlayMusic(pauseMenuClip, fadeDuration, useFade);
    }

    public void OnPauseClose() //Retorna a musica normal
    {
        //Mesma ideia
        AudioController.Instance?.RestoreSavedMusic(fadeDuration, useFade);
    }
}
