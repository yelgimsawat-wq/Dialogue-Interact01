using UnityEngine;

public partial class InteractableObject
{
    [SerializeField]
    private DialogueCanvas dialogueCanvas;
    [SerializeField]
    private DialogueContainer dialogueContainer;

    partial void DialogueUpdate()
    {
        if (dialogueCanvas == null || dialogueContainer == null)
        {
            Debug.LogWarning("DialogueCanvas or DialogueContainer is not assigned.");
            return;
        }

        if (!dialogueCanvas.gameObject.activeSelf)
        {
            dialogueCanvas.ShowDialogue(dialogueContainer);
        }
        else
        {
            dialogueCanvas.HideDialogue();
        }
    }
}