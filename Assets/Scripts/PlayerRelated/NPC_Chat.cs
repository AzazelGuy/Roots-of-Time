using UnityEngine;

/// <summary>
/// Script geral para conversas entre NPC's
/// </summary>
public class NPC_Chat : MonoBehaviour
{
    public DialogueControler dialogueController;
    public DialoguePage[] dialoguePages;          // configura tudo no Inspector (paginas, dialogo, se é o ultimo texto, se tem opções e etc....

    private bool playerNearby = false;

    private void Start()
    {
        dialogueController = DialogueControler.instance; // sem GetComponent, instance já é o componente
    }
    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !dialogueController.dialoguePanel.activeSelf) //Inicar dialogo com jogador tocando no colisor dele
        {
            dialogueController.StartDialogue(dialoguePages);
            playerNearby = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain")) playerNearby = true; //PLayer tá no colisor
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain")) playerNearby = false; //Player não tá no colisor :(
    }
}
