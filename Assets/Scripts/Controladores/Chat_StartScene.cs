using UnityEngine;

public class Chat_StartScene : MonoBehaviour
{
    public DialogueControler dialogueController; // arrastar prefab do Canvas
    public DialoguePage[] dialoguePages;          // configura tudo no Inspector


    private void Start()
    {
        dialogueController = DialogueControler.instance; // sem GetComponent, instance já é o componente
        dialogueController.StartDialogue(dialoguePages);
    }
}
