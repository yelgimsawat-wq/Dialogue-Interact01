using UnityEngine;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueContainer Dialogue; 

    public void Interact()
    {
        DialogueCanvas canvas = FindObjectOfType<DialogueCanvas>();
        
        if (canvas != null)
        {
            canvas.ShowDialogue(Dialogue);
        }
    }

    public string InteractionText => "Press [E] to talk";
}