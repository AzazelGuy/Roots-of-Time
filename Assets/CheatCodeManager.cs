using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatCodeManager : MonoBehaviour
{
    private string currentInput = "";
    public AudioClip correntSFX;
    private const string LEVELMAN = "LEVELMAN";

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetterOrDigit(c))
            {
                currentInput += char.ToUpper(c);

                // Mantém apenas os últimos caracteres necessários
                if (currentInput.Length > LEVELMAN.Length)
                {
                    currentInput = currentInput.Substring(
                        currentInput.Length - LEVELMAN.Length
                    );
                }

                if (currentInput == LEVELMAN)
                {
                    AudioController.Instance.PlaySFX(correntSFX, 1f);
                    Debug.Log("Cheat ativado!");
                    SceneManager.LoadScene("LevelSelect");

                    currentInput = "";
                }
            }
        }
    }
}