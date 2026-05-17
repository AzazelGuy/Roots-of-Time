using System.Collections.Generic;
using UnityEngine;
using TMPro; // Necessário para o Dropdown
using UnityEngine.UI; // Necessário para o Toggle

public class SettingResolution : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // 1. Detecta as resoluções suportadas pelo monitor
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Verifica qual é a resolução atual para marcar no menu ao iniciar
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // 2. Preenche o Dropdown e define o valor atual
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // 3. Sincroniza o estado inicial do Toggle de Tela Cheia
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    // Função para mudar a resolução (ligar ao OnValueChanged do Dropdown)
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    // Função para alternar Tela Cheia (ligar ao OnValueChanged do Toggle)
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}