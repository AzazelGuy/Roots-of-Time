using UnityEngine;

/// <summary>
/// Script geral para conversas entre NPC's
/// </summary>
public class Chat_StartOnce : MonoBehaviour
{
    public DialogueControler dialogueController;
    public DialoguePage[] dialoguePages;          // configura tudo no Inspector (paginas, dialogo, se é o ultimo texto, se tem opções e etc....

    public ActivateBoss ActivateBoss;
    private void Start()
    {
        dialogueController = DialogueControler.instance; // sem GetComponent, instance já é o componente
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerInteract")) {
            if (ActivateBoss != null)
            {
                ActivateBoss.ativar();
            }
            dialogueController.StartDialogue(dialoguePages);
            Destroy(gameObject);
        }
        ; //PLayer tá no colisor
    }
}
