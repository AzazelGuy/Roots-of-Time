using UnityEngine;
using UnityEngine.UI; // Importante para reconhecer o Slider

public class AudioSlider : MonoBehaviour
{
    public Slider sliderMusica;
    public Slider sliderSFX;

    void Start()
    {
        // Ao iniciar, a barra assume o valor que já está no AudioController
        if (AudioController.Instance != null)
        {
            sliderMusica.value = AudioController.Instance.MusicVolume;
            sliderSFX.value = AudioController.Instance.SFXVolume;
        }

        // Adiciona um "ouvinte" que detecta quando você move a barra
        sliderMusica.onValueChanged.AddListener(SetVolumeMusica);
        sliderSFX.onValueChanged.AddListener(SetVolumeSFX);
    }

    public void SetVolumeMusica(float valor)
    {
        AudioController.Instance.MusicVolume = valor;
    }

    public void SetVolumeSFX(float valor)
    {
        AudioController.Instance.SFXVolume = valor;
    }
}